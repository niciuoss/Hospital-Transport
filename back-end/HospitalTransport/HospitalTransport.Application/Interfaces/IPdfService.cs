using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalTransport.Domain.Entities;

namespace HospitalTransport.Application.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateAppointmentTicket(Appointment appointment);
        byte[] GeneratePassengerListPdf(List<Appointment> appointments, DateTime date);
    }
}
