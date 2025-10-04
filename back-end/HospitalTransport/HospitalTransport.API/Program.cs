using FluentValidation;
using HospitalTransport.Application.DTOs.Appointment;
using HospitalTransport.Application.DTOs.Patient;
using HospitalTransport.Application.DTOs.User;
using HospitalTransport.Application.Interfaces;
using HospitalTransport.Application.Services;
using HospitalTransport.Application.Validators;
using HospitalTransport.Domain.Interfaces;
using HospitalTransport.Infrastructure.Data;
using HospitalTransport.Infrastructure.Repositories;
using HospitalTransport.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar DbContext com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar Repositories
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Registrar Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// Registrar Validators
builder.Services.AddScoped<IValidator<CreatePatientRequest>, CreatePatientValidator>();
builder.Services.AddScoped<IValidator<CreateAppointmentRequest>, CreateAppointmentValidator>();
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserValidator>(); 
builder.Services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordValidator>(); 
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseMiddleware<HospitalTransport.API.Middlewares.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();