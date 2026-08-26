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
        byte[] GenerateAnnualReportPdf(List<Appointment> appointments, int year);
        byte[] GenerateMonthlyReportPdf(List<Appointment> appointments, int year, int month);
        byte[] GeneratePatientsInPeriodPdf(List<Appointment> appointments, DateTime from, DateTime to);
        byte[] GenerateCompanionsInPeriodPdf(List<Appointment> appointments, DateTime from, DateTime to);
        byte[] GenerateRegistrationsPdf(List<Patient> patients, DateTime from, DateTime to);
        byte[] GenerateDestinationsReportPdf(List<Appointment> appointments, DateTime from, DateTime to);
    }
}
