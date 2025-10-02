using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IPatientRepository Patients { get; }
        IAppointmentRepository Appointments { get; }
        IUserRepository Users { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
