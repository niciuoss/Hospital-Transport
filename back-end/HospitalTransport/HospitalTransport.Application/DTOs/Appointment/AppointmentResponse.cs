using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalTransport.Application.DTOs.Patient;

namespace HospitalTransport.Application.DTOs.Appointment
{
    public class AppointmentResponse
    {
        public Guid Id { get; set; }
        public PatientResponse Patient { get; set; } = null!;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string DestinationHospital { get; set; } = string.Empty;
        public string TreatmentType { get; set; } = string.Empty;
        public string? TreatmentTypeOther { get; set; }
        public bool IsPriority { get; set; }
        public int SeatNumber { get; set; }
        public DateTime AppointmentDate { get; set; }
        public PatientResponse? Companion { get; set; }
        public int? CompanionSeatNumber { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsTicketPrinted { get; set; }
    }
}
