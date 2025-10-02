using HospitalTransport.Application.DTOs.Patient;
using HospitalTransport.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalTransport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
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
    }
}