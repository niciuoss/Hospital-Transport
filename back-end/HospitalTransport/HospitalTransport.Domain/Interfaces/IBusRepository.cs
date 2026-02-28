using HospitalTransport.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Domain.Interfaces
{
    public interface IBusRepository : IRepository<Bus>
    {
        Task<IEnumerable<Bus>> GetActiveBusesAsync();
        Task<Bus?> GetByIdWithAppointmentsAsync(Guid id);
    }
}
