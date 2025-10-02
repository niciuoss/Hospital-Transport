using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Application.DTOs.Patient
{
    public class PatientSearchResult
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SusCardNumber { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string DisplayText => $"{FullName} - SUS: {SusCardNumber}";
    }
}