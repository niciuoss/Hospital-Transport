using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalTransport.Domain.Entities;

namespace HospitalTransport.Domain.Interfaces
{
    public interface IHospitalUserRepository
    {
        Task<HospitalUser?> GetByIdAsync(Guid id);
        Task<HospitalUser?> GetByUsernameAsync(string username);
        Task<HospitalUser?> GetByEmailAsync(string email);
        Task<IEnumerable<HospitalUser>> GetAllActiveAsync();
        Task<HospitalUser> AddAsync(HospitalUser user);
        Task UpdateAsync(HospitalUser user);
        Task DeleteAsync(Guid id);
    }
}