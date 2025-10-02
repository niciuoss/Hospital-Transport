using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace HospitalTransport.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Nome de usuário é obrigatório")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Password { get; set; } = string.Empty;
    }
}
