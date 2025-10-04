using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Application.DTOs.User
{
    public class UpdateUserRequest
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; } 
        public string Role { get; set; } = "Employee";
    }
}