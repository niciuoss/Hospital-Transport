using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalTransport.Application.DTOs.Appointment;
using HospitalTransport.Application.DTOs.Common;

namespace HospitalTransport.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<BaseResponse<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request);
        Task<BaseResponse<AppointmentResponse>> GetAppointmentByIdAsync(Guid id);
        Task<BaseResponse<IEnumerable<AppointmentResponse>>> GetAllAppointmentsAsync();
        Task<BaseResponse<IEnumerable<AppointmentResponse>>> GetRecentAppointmentsAsync(int count);
        Task<BaseResponse<IEnumerable<SeatAvailabilityResponse>>> GetSeatAvailabilityAsync(DateTime date, bool isPriority);
        Task<BaseResponse<IEnumerable<AppointmentResponse>>> SearchAppointmentsAsync(string searchTerm);
        Task<BaseResponse<byte[]>> GenerateTicketPdfAsync(Guid appointmentId);
        Task<BaseResponse<bool>> DeleteAppointmentAsync(Guid id);
    }
}