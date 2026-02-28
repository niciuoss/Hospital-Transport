using HospitalTransport.Domain.Entities;
using HospitalTransport.Domain.Interfaces;
using HospitalTransport.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalTransport.Infrastructure.Repositories
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
        {
            searchTerm = searchTerm.ToLower().Trim();

            return await _dbSet
                .Where(p => p.IsActive &&
                    (p.FullName.ToLower().Contains(searchTerm) ||
                     p.CPF.Contains(searchTerm) ||
                     p.SusCardNumber.Contains(searchTerm) ||
                     p.RG.Contains(searchTerm)))
                .OrderBy(p => p.FullName)
                .Take(20)
                .ToListAsync();
        }

        public async Task<Patient?> GetByCPFAsync(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return null;

            return await _dbSet
                .FirstOrDefaultAsync(p => p.IsActive && p.CPF == cpf);
        }

        public async Task<Patient?> GetBySusCardAsync(string susCardNumber)
        {
            if (string.IsNullOrWhiteSpace(susCardNumber))
                return null;

            return await _dbSet
                .FirstOrDefaultAsync(p => p.IsActive && p.SusCardNumber == susCardNumber);
        }
    }
}