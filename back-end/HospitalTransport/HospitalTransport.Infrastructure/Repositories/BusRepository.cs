using HospitalTransport.Domain.Entities;
using HospitalTransport.Domain.Interfaces;
using HospitalTransport.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalTransport.Infrastructure.Repositories
{
    public class BusRepository : Repository<Bus>, IBusRepository
    {
        public BusRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Bus>> GetActiveBusesAsync()
        {
            return await _dbSet
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<Bus?> GetByIdWithAppointmentsAsync(Guid id)
        {
            return await _dbSet
                .Include(b => b.Appointments)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}