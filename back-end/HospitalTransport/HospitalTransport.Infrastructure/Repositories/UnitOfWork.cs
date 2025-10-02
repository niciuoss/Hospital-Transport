using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalTransport.Domain.Interfaces;
using HospitalTransport.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace HospitalTransport.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public IPatientRepository Patients { get; }
        public IAppointmentRepository Appointments { get; }
        public IUserRepository Users { get; }

        public UnitOfWork(
            AppDbContext context,
            IPatientRepository patientRepository,
            IAppointmentRepository appointmentRepository,
            IUserRepository userRepository)
        {
            _context = context;
            Patients = patientRepository;
            Appointments = appointmentRepository;
            Users = userRepository;
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
