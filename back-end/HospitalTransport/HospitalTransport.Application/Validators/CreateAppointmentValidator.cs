using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HospitalTransport.Application.DTOs.Appointment;

namespace HospitalTransport.Application.Validators
{
    public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentRequest>
    {
        public CreateAppointmentValidator()
        {
            RuleFor(x => x.PatientId)
                .NotEmpty().WithMessage("Paciente é obrigatório");

            RuleFor(x => x.MedicalRecordNumber)
                .NotEmpty().WithMessage("Número do prontuário é obrigatório");

            RuleFor(x => x.DestinationHospital)
                .NotEmpty().WithMessage("Hospital de destino é obrigatório")
                .MaximumLength(200).WithMessage("Nome do hospital muito longo");

            RuleFor(x => x.TreatmentType)
                .IsInEnum().WithMessage("Tipo de tratamento inválido");

            RuleFor(x => x.TreatmentTypeOther)
                .NotEmpty().When(x => x.TreatmentType == 4)
                .WithMessage("Especifique o tipo de tratamento");

            RuleFor(x => x.SeatNumber)
                .GreaterThan(0).WithMessage("Número da poltrona inválido")
                .LessThanOrEqualTo(46).WithMessage("Número da poltrona não existe");

            RuleFor(x => x.AppointmentDate)
                .NotEmpty().WithMessage("Data do agendamento é obrigatória")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Data não pode ser no passado");

            RuleFor(x => x.CompanionSeatNumber)
                .GreaterThan(0).When(x => x.CompanionId.HasValue)
                .WithMessage("Poltrona do acompanhante é obrigatória")
                .LessThanOrEqualTo(46).When(x => x.CompanionId.HasValue)
                .WithMessage("Número da poltrona do acompanhante não existe");

            RuleFor(x => x.CreatedByUserId)
                .NotEmpty().WithMessage("Usuário criador é obrigatório");
        }
    }
}
