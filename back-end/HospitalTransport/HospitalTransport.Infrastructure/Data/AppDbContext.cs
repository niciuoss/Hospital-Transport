using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HospitalTransport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalTransport.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            // Configurar Npgsql para usar timestamp sem timezone
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<SystemControl> SystemControl { get; set; } // ADICIONE ESTA LINHA

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da entidade User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Configuração da entidade Patient
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patients");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.RG).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CPF).IsRequired().HasMaxLength(14);
                entity.Property(e => e.SusCardNumber).IsRequired().HasMaxLength(15);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.MotherName).IsRequired().HasMaxLength(200);

                // Converter DateOnly para Date no banco
                entity.Property(e => e.BirthDate)
                    .HasColumnType("date");

                entity.HasIndex(e => e.CPF).IsUnique();
                entity.HasIndex(e => e.SusCardNumber).IsUnique();
            });

            // Configuração da entidade Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("Appointments");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.MedicalRecordNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DestinationHospital).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TreatmentTypeOther).HasMaxLength(200);

                // Configurar AppointmentDate como timestamp sem timezone
                entity.Property(e => e.AppointmentDate)
                    .HasColumnType("timestamp without time zone");

                entity.Property(e => e.PrintedAt)
                    .HasColumnType("timestamp without time zone");

                // Relacionamento com Patient
                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento com Companion (Patient)
                entity.HasOne(e => e.Companion)
                    .WithMany(p => p.AppointmentsAsCompanion)
                    .HasForeignKey(e => e.CompanionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Relacionamento com User
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.CreatedAppointments)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.AppointmentDate);
                entity.HasIndex(e => new { e.AppointmentDate, e.SeatNumber }).IsUnique();
            });

            // Configuração da entidade SystemControl
            modelBuilder.Entity<SystemControl>(entity =>
            {
                entity.ToTable("SystemControl");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.LastChanged)
                    .HasColumnType("timestamp without time zone");
            });

            // Seed inicial de usuário
            var defaultUserId = Guid.NewGuid();
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = defaultUserId,
                FullName = "Administrador do Sistema",
                Username = "admin",
                PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("admin123")),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

            // Seed SystemControl - Sistema habilitado por padrão
            modelBuilder.Entity<SystemControl>().HasData(new SystemControl
            {
                Id = 1,
                IsEnabled = true,
                Message = "Sistema operando normalmente",
                LastChanged = DateTime.UtcNow
            });
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Configurar conversão automática de DateOnly
            configurationBuilder.Properties<DateOnly>()
                .HaveConversion<DateOnlyConverter>()
                .HaveColumnType("date");
        }
    }

    // Converter DateOnly para o banco de dados
    public class DateOnlyConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateOnly, DateTime>
    {
        public DateOnlyConverter() : base(
            dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            dateTime => DateOnly.FromDateTime(dateTime))
        {
        }
    }
}
