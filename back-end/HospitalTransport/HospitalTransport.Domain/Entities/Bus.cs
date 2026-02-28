using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Domain.Entities
{
    public class Bus : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public string SeatLayout { get; set; } = "Standard";

        // Relacionamentos
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}