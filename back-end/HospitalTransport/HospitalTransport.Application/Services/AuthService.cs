using HospitalTransport.Application.DTOs.Auth;
using HospitalTransport.Application.DTOs.Common;
using HospitalTransport.Application.Interfaces;
using HospitalTransport.Domain.Interfaces;

namespace HospitalTransport.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);

                if (user == null)
                {
                    return BaseResponse<LoginResponse>.FailureResponse("Usuário ou senha incorretos");
                }

                // Validação simples de senha (em produção, use hash adequado)
                if (user.PasswordHash != HashPassword(request.Password))
                {
                    return BaseResponse<LoginResponse>.FailureResponse("Usuário ou senha incorretos");
                }

                var response = new LoginResponse
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Username = user.Username,
                    Token = GenerateSimpleToken(user.Id)
                };

                return BaseResponse<LoginResponse>.SuccessResponse(response, "Login realizado com sucesso");
            }
            catch (Exception ex)
            {
                return BaseResponse<LoginResponse>.FailureResponse($"Erro ao realizar login: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ValidateTokenAsync(string token)
        {
            try
            {
                // Validação simples de token
                if (string.IsNullOrEmpty(token))
                {
                    return BaseResponse<bool>.FailureResponse("Token inválido");
                }

                return BaseResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.FailureResponse($"Erro ao validar token: {ex.Message}");
            }
        }

        private string HashPassword(string password)
        {
            // Implementação simplificada - em produção use BCrypt ou similar
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private string GenerateSimpleToken(Guid userId)
        {
            // Token simplificado - em produção use JWT
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userId}:{DateTime.UtcNow}"));
        }
    }
}