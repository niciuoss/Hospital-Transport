using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Application.DTOs.Appointment
{
    public class AnnualReportResponse
    {
        public int Year { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalPassengers { get; set; }
        public int TotalPriorityPassengers { get; set; }
        public int TotalCompanions { get; set; }
        public Dictionary<string, int> ByMonth { get; set; } = new();
        public Dictionary<string, int> ByTreatmentType { get; set; } = new();
        public Dictionary<string, int> ByDestination { get; set; } = new();
    }
}
