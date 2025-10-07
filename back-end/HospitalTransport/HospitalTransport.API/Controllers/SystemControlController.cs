using HospitalTransport.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalTransport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemControlController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemControlController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            var control = await _context.SystemControl.FirstOrDefaultAsync(sc => sc.Id == 1);
            return Ok(new
            {
                success = true,
                data = control
            });
        }

        [HttpPut("toggle")]
        public async Task<IActionResult> ToggleSystem([FromBody] ToggleSystemRequest request)
        {
            var control = await _context.SystemControl.FirstOrDefaultAsync(sc => sc.Id == 1);

            if (control == null)
            {
                control = new Domain.Entities.SystemControl
                {
                    Id = 1,
                    IsEnabled = request.IsEnabled,
                    Message = request.Message,
                    LastChanged = DateTime.UtcNow
                };
                _context.SystemControl.Add(control);
            }
            else
            {
                control.IsEnabled = request.IsEnabled;
                control.Message = request.Message;
                control.LastChanged = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = control.IsEnabled ? "Sistema ativado" : "Sistema desativado",
                data = control
            });
        }
    }

    public class ToggleSystemRequest
    {
        public bool IsEnabled { get; set; }
        public string? Message { get; set; }
    }
}
