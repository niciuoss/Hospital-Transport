using HospitalTransport.Domain.Entities;
using HospitalTransport.Domain.Interfaces;
using HospitalTransport.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalTransport.Infrastructure.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<Appointment?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(a => a.Patient)
                .Include(a => a.Companion)
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(int count)
        {
            return await _dbSet
                .Include(a => a.Patient)
                .Include(a => a.Companion)
                .Include(a => a.CreatedByUser)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _dbSet
                .Include(a => a.Patient)
                .Include(a => a.Companion)
                .Include(a => a.CreatedByUser)
                .Where(a => a.IsActive &&
                    a.AppointmentDate >= startDate &&
                    a.AppointmentDate < endDate)
                .OrderBy(a => a.SeatNumber)
                .ToListAsync();
        }


        public async Task<IEnumerable<Appointment>> SearchAppointmentsAsync(string searchTerm)
        {
            searchTerm = searchTerm.ToLower().Trim();

            return await _dbSet
                .Include(a => a.Patient)
                .Include(a => a.Companion)
                .Include(a => a.CreatedByUser)
                .Where(a => a.IsActive &&
                    (a.Patient.FullName.ToLower().Contains(searchTerm) ||
                     a.Patient.CPF.Contains(searchTerm) ||
                     a.Patient.SusCardNumber.Contains(searchTerm) ||
                     a.MedicalRecordNumber.Contains(searchTerm) ||
                     a.DestinationHospital.ToLower().Contains(searchTerm)))
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByYearAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59);

            return await _dbSet
                .Include(a => a.Patient)
                .Include(a => a.Companion)
                .Include(a => a.CreatedByUser)
                .Where(a => a.IsActive &&
                    a.AppointmentDate >= startDate &&
                    a.AppointmentDate <= endDate)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<bool> IsSeatAvailableAsync(DateTime date, int seatNumber, Guid busId)
        {
            var dateOnly = DateOnly.FromDateTime(date.Date);

            return !await _dbSet.AnyAsync(a =>
                a.IsActive &&
                a.BusId == busId &&
                DateOnly.FromDateTime(a.AppointmentDate) == dateOnly &&
                (a.SeatNumber == seatNumber || a.CompanionSeatNumber == seatNumber));
        }

        public async Task<List<int>> GetOccupiedSeatsAsync(DateTime date)
        {
            var dateOnly = DateOnly.FromDateTime(date.Date);

            var appointments = await _dbSet
                .Where(a => a.IsActive && DateOnly.FromDateTime(a.AppointmentDate) == dateOnly) 
                .ToListAsync();

            var occupiedSeats = new List<int>();

            foreach (var appointment in appointments)
            {
                if (appointment.SeatNumber > 0) // Ignorar crianças de colo (SeatNumber = 0)
                {
                    occupiedSeats.Add(appointment.SeatNumber);
                }

                if (appointment.CompanionSeatNumber.HasValue)
                {
                    occupiedSeats.Add(appointment.CompanionSeatNumber.Value);
                }
            }

            return occupiedSeats.Distinct().ToList();
        }
    }
}