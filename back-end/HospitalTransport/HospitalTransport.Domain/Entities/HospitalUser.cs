using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Domain.Entities;

public class HospitalUser : BaseEntity
{
    public string FullName { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Department { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<Appointment> Appointments { get; private set; } = new List<Appointment>();

    protected HospitalUser() { } // EF Core

    public HospitalUser(string fullName, string username, string email,
                       string passwordHash, string department)
    {
        FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Department = department ?? throw new ArgumentNullException(nameof(department));
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}