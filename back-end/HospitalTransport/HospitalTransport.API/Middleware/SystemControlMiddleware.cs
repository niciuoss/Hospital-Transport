using HospitalTransport.Infrastructure.Data;
using HospitalTransport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalTransport.API.Middlewares
{
    public class SystemControlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SystemControlMiddleware> _logger;

        public SystemControlMiddleware(RequestDelegate next, ILogger<SystemControlMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // Permite acesso a endpoints de saúde e swagger
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.Contains("/health") || path.Contains("/swagger"))
            {
                await _next(context);
                return;
            }

            try
            {
                var systemControl = await dbContext.SystemControl.FirstOrDefaultAsync(sc => sc.Id == 1);

                if (systemControl != null && !systemControl.IsEnabled)
                {
                    _logger.LogWarning("Sistema desativado. Acesso negado.");

                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        success = false,
                        message = "Sistema temporariamente desativado",
                        info = systemControl.Message ?? "Entre em contato com o suporte",
                        timestamp = DateTime.UtcNow
                    };

                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status do sistema");
                // Em caso de erro, permite continuar (fail-open)
            }

            await _next(context);
        }
    }
}
