using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalTransport.Application.DTOs.Common;
using HospitalTransport.Application.DTOs.User;

namespace HospitalTransport.Application.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<UserResponse>> CreateUserAsync(CreateUserRequest request);
        Task<BaseResponse<UserResponse>> UpdateUserAsync(UpdateUserRequest request);
        Task<BaseResponse<UserResponse>> GetUserByIdAsync(Guid id);
        Task<BaseResponse<IEnumerable<UserResponse>>> GetAllUsersAsync();
        Task<BaseResponse<bool>> DeleteUserAsync(Guid id);
        Task<BaseResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request);
    }
}