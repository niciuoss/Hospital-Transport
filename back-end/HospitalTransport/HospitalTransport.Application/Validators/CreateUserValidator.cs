using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HospitalTransport.Application.DTOs.User;

namespace HospitalTransport.Application.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Nome completo é obrigatório")
                .MaximumLength(200).WithMessage("Nome não pode ter mais de 200 caracteres");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Nome de usuário é obrigatório")
                .MinimumLength(3).WithMessage("Nome de usuário deve ter no mínimo 3 caracteres")
                .MaximumLength(100).WithMessage("Nome de usuário não pode ter mais de 100 caracteres")
                .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Nome de usuário deve conter apenas letras, números, ponto, hífen ou underscore");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Senha é obrigatória")
                .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres")
                .MaximumLength(100).WithMessage("Senha não pode ter mais de 100 caracteres");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Função é obrigatória")
                .Must(role => role == "Admin" || role == "Employee")
                .WithMessage("Função deve ser 'Admin' ou 'Employee'");
        }
    }
}
