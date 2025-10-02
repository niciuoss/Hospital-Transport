using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalTransport.Domain.Entities;
namespace HospitalTransport.Domain.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm);
        Task<Patient?> GetByCPFAsync(string cpf);
        Task<Patient?> GetBySusCardAsync(string susCard);
    }
}