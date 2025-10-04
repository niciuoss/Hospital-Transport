using HospitalTransport.Application.DTOs.Appointment;
using HospitalTransport.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTransport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
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
    }
}