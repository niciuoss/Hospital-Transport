using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalTransport.Domain.Enums;

namespace HospitalTransport.Domain.Entities
{
    public class Appointment : BaseEntity
    {
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string DestinationHospital { get; set; } = string.Empty;
        public TreatmentType TreatmentType { get; set; }
        public string? TreatmentTypeOther { get; set; }
        public bool IsPriority { get; set; }

        public int SeatNumber { get; set; }
        public DateTime AppointmentDate { get; set; }

        // Acompanhante (opcional)
        public Guid? CompanionId { get; set; }
        public Patient? Companion { get; set; }
        public int? CompanionSeatNumber { get; set; }

        // Usuário que criou
        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        // Status
        public bool IsTicketPrinted { get; set; }
        public DateTime? PrintedAt { get; set; }
    }
}