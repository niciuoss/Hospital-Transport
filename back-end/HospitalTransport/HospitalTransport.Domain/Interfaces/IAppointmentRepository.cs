using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalTransport.Domain.Entities;

namespace HospitalTransport.Domain.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(int count);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date);
        Task<IEnumerable<int>> GetOccupiedSeatsAsync(DateTime date);
        Task<IEnumerable<Appointment>> SearchAppointmentsAsync(string searchTerm);
    }
}