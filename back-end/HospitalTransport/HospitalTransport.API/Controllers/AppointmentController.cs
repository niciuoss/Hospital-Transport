using HospitalTransport.Application.DTOs.Appointment;
using HospitalTransport.Application.Interfaces;
using HospitalTransport.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using HospitalTransport.Domain.Interfaces;
using HospitalTransport.Infrastructure.Repositories;

namespace HospitalTransport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPdfService _pdfService;
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentsController(IAppointmentService appointmentService, IPdfService pdfService, IUnitOfWork unitOfWork)
        {
            _appointmentService = appointmentService;
            _pdfService = pdfService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            var result = await _appointmentService.CreateAppointmentAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetAppointmentById), new { id = result.Data!.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAppointments()
        {
            var result = await _appointmentService.GetAllAppointmentsAsync();

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById(Guid id)
        {
            var result = await _appointmentService.GetAppointmentByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentAppointments([FromQuery] int count = 10)
        {
            var result = await _appointmentService.GetRecentAppointmentsAsync(count);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("seat-availability")]
        public async Task<IActionResult> GetSeatAvailability(
            [FromQuery] DateTime date,
            [FromQuery] bool isPriority = false)
        {
            var result = await _appointmentService.GetSeatAvailabilityAsync(date, isPriority);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAppointments([FromQuery] string searchTerm)
        {
            var result = await _appointmentService.SearchAppointmentsAsync(searchTerm);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/ticket")]
        public async Task<IActionResult> GenerateTicket(Guid id)
        {
            var result = await _appointmentService.GenerateTicketPdfAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return File(result.Data!, "application/pdf", $"passagem_{id}.pdf");
        }

        [HttpGet("passenger-list-pdf")]
        public async Task<IActionResult> GeneratePassengerListPdf([FromQuery] DateTime date)
        {
            try
            {
                var appointments = await _unitOfWork.Appointments.GetAppointmentsByDateAsync(date);

                if (!appointments.Any())
                {
                    return NotFound(new { success = false, message = "Nenhum agendamento encontrado para esta data" });
                }

                var pdfBytes = _pdfService.GeneratePassengerListPdf(appointments.ToList(), date);

                return File(pdfBytes, "application/pdf", $"lista_passageiros_{date:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Erro ao gerar PDF: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(Guid id)
        {
            var result = await _appointmentService.DeleteAppointmentAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}