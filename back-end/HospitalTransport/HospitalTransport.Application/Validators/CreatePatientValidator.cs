using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HospitalTransport.Application.DTOs.Patient;

namespace HospitalTransport.Application.Validators
{
    public class CreatePatientValidator : AbstractValidator<CreatePatientRequest>
    {
        public CreatePatientValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Nome completo é obrigatório")
                .MaximumLength(200).WithMessage("Nome não pode ter mais de 200 caracteres");

            RuleFor(x => x.RG)
                .Must(numero =>
                    (numero.Length >= 5 && numero.Length <= 20) || 
                    (numero.Length == 11) 
                ).WithMessage("RG deve ter formato válido (antigo) ou 11 dígitos (novo CPF)").When(x => !string.IsNullOrEmpty(x.RG));

            RuleFor(x => x.CPF)
                .Must(BeValidCPF).WithMessage("CPF inválido").When(x => !string.IsNullOrEmpty(x.CPF));

            RuleFor(x => x.Age)
                .GreaterThan(0).WithMessage("Idade deve ser maior que zero")
                .LessThan(150).WithMessage("Idade inválida");

            RuleFor(x => x.BirthDate)
                 .NotEmpty().WithMessage("Data de nascimento é obrigatória")
                 .LessThan(DateOnly.FromDateTime(DateTime.Now)).WithMessage("Data de nascimento não pode ser futura");

            RuleFor(x => x.SusCardNumber)
                .Must(numero =>
                    (numero.Length == 15) || (numero.Length == 11)
                ).WithMessage("Cartão SUS deve ter 15 dígitos (antigo) ou 11 dígitos (novo CPF)").When(x => !string.IsNullOrEmpty(x.SusCardNumber));

            RuleFor(x => x.PhoneNumber)
                .Must(numero =>
                    (numero.Length == 11)
                ).WithMessage("Telefone é obrigatório").When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Endereço é obrigatório")
                .MaximumLength(500).WithMessage("Endereço não pode ter mais de 500 caracteres");

            RuleFor(x => x.MotherName)
                .MaximumLength(200).WithMessage("Nome da mãe não pode ter mais de 200 caracteres").When(x => !string.IsNullOrEmpty(x.MotherName));
        }

        private bool BeValidCPF(string cpf)
        {
            return true;
        }
    }
}