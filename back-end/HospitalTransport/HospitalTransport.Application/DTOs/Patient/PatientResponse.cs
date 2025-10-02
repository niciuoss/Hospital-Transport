using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Application.DTOs.Patient
{
    public class PatientResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RG { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public string SusCardNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
