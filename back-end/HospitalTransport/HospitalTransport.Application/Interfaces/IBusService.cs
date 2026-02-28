using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalTransport.Application.DTOs.Bus;
using HospitalTransport.Application.DTOs.Common;

namespace HospitalTransport.Application.Interfaces
{
    public interface IBusService
    {
        Task<BaseResponse<IEnumerable<BusResponse>>> GetActiveBusesAsync();
        Task<BaseResponse<BusResponse>> GetBusByIdAsync(Guid id);
    }
}

