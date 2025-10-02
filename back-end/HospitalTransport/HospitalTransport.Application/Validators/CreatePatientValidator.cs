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
                .NotEmpty().WithMessage("RG é obrigatório")
                .MaximumLength(20).WithMessage("RG inválido");

            RuleFor(x => x.CPF)
                .NotEmpty().WithMessage("CPF é obrigatório")
                .Must(BeValidCPF).WithMessage("CPF inválido");

            RuleFor(x => x.Age)
                .GreaterThan(0).WithMessage("Idade deve ser maior que zero")
                .LessThan(150).WithMessage("Idade inválida");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Data de nascimento é obrigatória")
                .LessThan(DateTime.Now).WithMessage("Data de nascimento não pode ser futura");

            RuleFor(x => x.SusCardNumber)
                .NotEmpty().WithMessage("Número do cartão SUS é obrigatório")
                .Must(numero =>
                    (numero.Length == 15) || (numero.Length == 11)
                ).WithMessage("Cartão SUS deve ter 15 dígitos (antigo) ou 11 dígitos (novo CPF)");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Telefone é obrigatório");

            RuleFor(x => x.MotherName)
                .NotEmpty().WithMessage("Nome da mãe é obrigatório")
                .MaximumLength(200).WithMessage("Nome da mãe não pode ter mais de 200 caracteres");
        }

        private bool BeValidCPF(string cpf)
        {
            cpf = cpf.Replace(".", "").Replace("-", "").Trim();
            if (cpf.Length != 11) return false;
            if (cpf.Distinct().Count() == 1) return false;
            return true; // Validação simplificada
        }
    }
}