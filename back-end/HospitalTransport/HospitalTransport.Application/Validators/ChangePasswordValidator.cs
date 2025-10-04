using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HospitalTransport.Application.DTOs.User;

namespace HospitalTransport.Application.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("ID do usuário é obrigatório");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Senha atual é obrigatória");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Nova senha é obrigatória")
                .MinimumLength(6).WithMessage("Nova senha deve ter no mínimo 6 caracteres")
                .NotEqual(x => x.CurrentPassword).WithMessage("Nova senha deve ser diferente da senha atual");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Confirmação de senha é obrigatória")
                .Equal(x => x.NewPassword).WithMessage("Confirmação de senha não confere");
        }
    }
}
