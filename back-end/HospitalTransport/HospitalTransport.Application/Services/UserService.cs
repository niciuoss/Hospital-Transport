using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HospitalTransport.Application.DTOs.Common;
using HospitalTransport.Application.DTOs.User;
using HospitalTransport.Application.Interfaces;
using HospitalTransport.Domain.Entities;
using HospitalTransport.Domain.Interfaces;

namespace HospitalTransport.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateUserRequest> _createValidator;
        private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

        public UserService(
            IUnitOfWork unitOfWork,
            IValidator<CreateUserRequest> createValidator,
            IValidator<ChangePasswordRequest> changePasswordValidator)
        {
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _changePasswordValidator = changePasswordValidator;
        }

        public async Task<BaseResponse<UserResponse>> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                var validationResult = await _createValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BaseResponse<UserResponse>.FailureResponse(
                        "Dados inválidos",
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                // Verificar se o username já existe
                var existingUser = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
                if (existingUser != null)
                {
                    return BaseResponse<UserResponse>.FailureResponse("Nome de usuário já está em uso");
                }

                var user = new User
                {
                    FullName = request.FullName,
                    Username = request.Username.ToLower(),
                    PasswordHash = HashPassword(request.Password),
                    Role = request.Role
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToUserResponse(user);

                return BaseResponse<UserResponse>.SuccessResponse(
                    response,
                    "Usuário cadastrado com sucesso"
                );
            }
            catch (Exception ex)
            {
                return BaseResponse<UserResponse>.FailureResponse($"Erro ao cadastrar usuário: {ex.Message}");
            }
        }

        public async Task<BaseResponse<UserResponse>> UpdateUserAsync(UpdateUserRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
                if (user == null)
                {
                    return BaseResponse<UserResponse>.FailureResponse("Usuário não encontrado");
                }

                // Verificar se o novo username já existe em outro usuário
                var existingUser = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
                if (existingUser != null && existingUser.Id != request.Id)
                {
                    return BaseResponse<UserResponse>.FailureResponse("Nome de usuário já está em uso");
                }

                user.FullName = request.FullName;
                user.Username = request.Username.ToLower();
                user.Role = request.Role;
                user.UpdatedAt = DateTime.UtcNow;

                // Atualizar senha apenas se foi fornecida
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordHash = HashPassword(request.Password);
                }

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToUserResponse(user);

                return BaseResponse<UserResponse>.SuccessResponse(
                    response,
                    "Usuário atualizado com sucesso"
                );
            }
            catch (Exception ex)
            {
                return BaseResponse<UserResponse>.FailureResponse($"Erro ao atualizar usuário: {ex.Message}");
            }
        }

        public async Task<BaseResponse<UserResponse>> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return BaseResponse<UserResponse>.FailureResponse("Usuário não encontrado");
                }

                var response = MapToUserResponse(user);
                return BaseResponse<UserResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<UserResponse>.FailureResponse($"Erro ao buscar usuário: {ex.Message}");
            }
        }

        public async Task<BaseResponse<IEnumerable<UserResponse>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.FindAsync(u => u.IsActive);

                var responses = users
                    .OrderBy(u => u.FullName)
                    .Select(u => MapToUserResponse(u))
                    .ToList();

                return BaseResponse<IEnumerable<UserResponse>>.SuccessResponse(responses);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<UserResponse>>.FailureResponse(
                    $"Erro ao buscar usuários: {ex.Message}"
                );
            }
        }

        public async Task<BaseResponse<bool>> DeleteUserAsync(Guid id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return BaseResponse<bool>.FailureResponse("Usuário não encontrado");
                }

                // Verificar se o usuário tem agendamentos
                var hasAppointments = await _unitOfWork.Appointments
                    .ExistsAsync(a => a.CreatedByUserId == id);

                if (hasAppointments)
                {
                    return BaseResponse<bool>.FailureResponse(
                        "Não é possível excluir usuário que possui agendamentos cadastrados. " +
                        "Desative o usuário ao invés de excluí-lo."
                    );
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return BaseResponse<bool>.SuccessResponse(true, "Usuário desativado com sucesso");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.FailureResponse($"Erro ao desativar usuário: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                var validationResult = await _changePasswordValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BaseResponse<bool>.FailureResponse(
                        "Dados inválidos",
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    return BaseResponse<bool>.FailureResponse("Usuário não encontrado");
                }

                // Verificar senha atual
                if (user.PasswordHash != HashPassword(request.CurrentPassword))
                {
                    return BaseResponse<bool>.FailureResponse("Senha atual incorreta");
                }

                // Atualizar senha
                user.PasswordHash = HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return BaseResponse<bool>.SuccessResponse(true, "Senha alterada com sucesso");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.FailureResponse($"Erro ao alterar senha: {ex.Message}");
            }
        }

        private UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
