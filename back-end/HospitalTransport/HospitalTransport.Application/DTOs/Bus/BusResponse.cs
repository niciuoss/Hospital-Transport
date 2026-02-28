using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Application.DTOs.Bus
{
    public class BusResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public string SeatLayout { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
