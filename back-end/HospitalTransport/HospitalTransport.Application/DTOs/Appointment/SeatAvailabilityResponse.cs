using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Application.DTOs.Appointment
{
    public class SeatAvailabilityResponse
    {
        public int SeatNumber { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsPriorityOnly { get; set; }
    }
}