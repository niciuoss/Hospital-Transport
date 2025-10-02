using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalTransport.Application.Common;
using HospitalTransport.Application.DTOs.Auth;
using HospitalTransport.Application.Services;
using HospitalTransport.Application.DTOs.Common;

namespace HospitalTransport.Application.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<BaseResponse<bool>> ValidateTokenAsync(string token);
    }
}
