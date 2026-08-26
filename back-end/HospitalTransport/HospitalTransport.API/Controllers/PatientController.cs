using HospitalTransport.Application.DTOs.Patient;
using HospitalTransport.Application.Interfaces;
using HospitalTransport.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTransport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IPdfService _pdfService;
        private readonly IUnitOfWork _unitOfWork;

        public PatientsController(IPatientService patientService, IPdfService pdfService, IUnitOfWork unitOfWork)
        {
            _patientService = patientService;
            _pdfService = pdfService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request)
        {
            var result = await _patientService.CreatePatientAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetPatientById), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] UpdatePatientRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID do paciente não corresponde");
            }

            var result = await _patientService.UpdatePatientAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var result = await _patientService.GetAllPatientsAsync();

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(Guid id)
        {
            var result = await _patientService.GetPatientByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPatients([FromQuery] string searchTerm)
        {
            var result = await _patientService.SearchPatientsAsync(searchTerm);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            var result = await _patientService.DeletePatientAsync(id);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("registrations-report-pdf")]
        public async Task<IActionResult> GenerateRegistrationsReportPdf([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
        {
            try
            {
                var endDate = dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                var patients = (await _unitOfWork.Patients
                    .FindAsync(p => p.IsActive && p.CreatedAt >= dateFrom.Date && p.CreatedAt <= endDate))
                    .ToList();

                if (!patients.Any())
                    return NotFound(new { success = false, message = "Nenhum cadastro encontrado para o período" });

                var pdfBytes = _pdfService.GenerateRegistrationsPdf(patients, dateFrom, dateTo);
                return File(pdfBytes, "application/pdf", $"relatorio_cadastros_{dateFrom:yyyy-MM-dd}_{dateTo:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Erro ao gerar relatório: {ex.Message}" });
            }
        }
    }
}