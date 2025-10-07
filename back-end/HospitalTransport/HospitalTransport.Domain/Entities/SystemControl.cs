using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalTransport.Domain.Entities
{
    public class SystemControl
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
        public string? Message { get; set; }
        public DateTime LastChanged { get; set; }
    }
}
