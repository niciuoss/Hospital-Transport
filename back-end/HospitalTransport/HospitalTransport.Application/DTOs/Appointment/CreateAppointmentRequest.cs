using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Application.DTOs.Appointment
{
    public class CreateAppointmentRequest
    {
        public Guid PatientId { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string DestinationHospital { get; set; } = string.Empty;
        public int TreatmentType { get; set; }
        public string? TreatmentTypeOther { get; set; }
        public bool IsPriority { get; set; }
        public int SeatNumber { get; set; }
        public DateTime AppointmentDate { get; set; }
        public Guid? CompanionId { get; set; }
        public int? CompanionSeatNumber { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}
