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

        [HttpGet("monthly-report-pdf")]
        public async Task<IActionResult> GenerateMonthlyReportPdf([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new { success = false, message = "Mês inválido" });
                }

                if (year < 2000 || year > DateTime.Now.Year)
                {
                    return BadRequest(new { success = false, message = "Ano inválido" });
                }

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

                Console.WriteLine($"📅 Buscando agendamentos de {startDate:dd/MM/yyyy} até {endDate:dd/MM/yyyy}");

                var appointments = (await _unitOfWork.Appointments
                    .FindAsync(a => a.IsActive &&
                               a.AppointmentDate >= startDate &&
                               a.AppointmentDate <= endDate))
                    .OrderBy(a => a.AppointmentDate)
                    .ToList();

                Console.WriteLine($"✅ Encontrados {appointments.Count} agendamentos");

                if (!appointments.Any())
                {
                    return NotFound(new { success = false, message = "Nenhum agendamento encontrado para este mês" });
                }

                // ✅ CARREGAR DADOS RELACIONADOS (Patient e Companion)
                foreach (var appointment in appointments)
                {
                    // Carregar paciente
                    if (appointment.Patient == null)
                    {
                        appointment.Patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
                        Console.WriteLine($"  ✅ Paciente carregado: {appointment.Patient?.FullName ?? "NULL"}");
                    }

                    // Carregar acompanhante (se existir)
                    if (appointment.CompanionId.HasValue && appointment.Companion == null)
                    {
                        appointment.Companion = await _unitOfWork.Patients.GetByIdAsync(appointment.CompanionId.Value);
                        Console.WriteLine($"  ✅ Acompanhante carregado: {appointment.Companion?.FullName ?? "NULL"}");
                    }
                }

                Console.WriteLine("🔄 Gerando PDF...");

                var pdfBytes = _pdfService.GenerateMonthlyReportPdf(appointments, year, month);

                Console.WriteLine($"✅ PDF gerado! Tamanho: {pdfBytes.Length} bytes");

                return File(pdfBytes, "application/pdf", $"relatorio_mensal_{year}_{month:D2}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO COMPLETO: {ex}");
                return BadRequest(new { success = false, message = $"Erro ao gerar relatório: {ex.Message}" });
            }
        }

        [HttpGet("seat-availability")]
        public async Task<IActionResult> GetSeatAvailability(
            [FromQuery] DateTime date,
            [FromQuery] Guid busId,
            [FromQuery] bool isPriority = false)
        {
            var result = await _appointmentService.GetSeatAvailabilityAsync(date, busId, isPriority);

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


        [HttpGet("annual-report-pdf")]
        public async Task<IActionResult> GenerateAnnualReportPdf([FromQuery] int year)
        {
            try
            {
                if (year < 2000 || year > DateTime.Now.Year)
                {
                    return BadRequest(new { success = false, message = "Ano inválido" });
                }

                var appointments = await _unitOfWork.Appointments.GetAppointmentsByYearAsync(year);

                if (!appointments.Any())
                {
                    return NotFound(new { success = false, message = "Nenhum agendamento encontrado para este ano" });
                }

                var pdfBytes = _pdfService.GenerateAnnualReportPdf(appointments.ToList(), year);

                return File(pdfBytes, "application/pdf", $"relatorio_anual_{year}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Erro ao gerar relatório: {ex.Message}" });
            }
        }

        [HttpGet("patients-report-pdf")]
        public async Task<IActionResult> GeneratePatientsReportPdf([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
        {
            try
            {
                var endDate = dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                var appointments = (await _unitOfWork.Appointments
                    .FindAsync(a => a.IsActive && a.AppointmentDate >= dateFrom.Date && a.AppointmentDate <= endDate))
                    .ToList();

                if (!appointments.Any())
                    return NotFound(new { success = false, message = "Nenhum agendamento encontrado para o período" });

                var pdfBytes = _pdfService.GeneratePatientsInPeriodPdf(appointments, dateFrom, dateTo);
                return File(pdfBytes, "application/pdf", $"relatorio_pacientes_{dateFrom:yyyy-MM-dd}_{dateTo:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Erro ao gerar relatório: {ex.Message}" });
            }
        }

        [HttpGet("companions-report-pdf")]
        public async Task<IActionResult> GenerateCompanionsReportPdf([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
        {
            try
            {
                var endDate = dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                var appointments = (await _unitOfWork.Appointments
                    .FindAsync(a => a.IsActive && a.AppointmentDate >= dateFrom.Date && a.AppointmentDate <= endDate))
                    .ToList();

                if (!appointments.Any())
                    return NotFound(new { success = false, message = "Nenhum agendamento encontrado para o período" });

                var pdfBytes = _pdfService.GenerateCompanionsInPeriodPdf(appointments, dateFrom, dateTo);
                return File(pdfBytes, "application/pdf", $"relatorio_acompanhantes_{dateFrom:yyyy-MM-dd}_{dateTo:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Erro ao gerar relatório: {ex.Message}" });
            }
        }

        [HttpGet("destinations-report-pdf")]
        public async Task<IActionResult> GenerateDestinationsReportPdf([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
        {
            try
            {
                var endDate = dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                var appointments = (await _unitOfWork.Appointments
                    .FindAsync(a => a.IsActive && a.AppointmentDate >= dateFrom.Date && a.AppointmentDate <= endDate))
                    .ToList();

                if (!appointments.Any())
                    return NotFound(new { success = false, message = "Nenhum agendamento encontrado para o período" });

                var pdfBytes = _pdfService.GenerateDestinationsReportPdf(appointments, dateFrom, dateTo);
                return File(pdfBytes, "application/pdf", $"relatorio_destinos_{dateFrom:yyyy-MM-dd}_{dateTo:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Erro ao gerar relatório: {ex.Message}" });
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