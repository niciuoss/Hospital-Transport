using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalTransport.Application.DTOs.Patient
{
    public class CreatePatientDto
    {
        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [StringLength(200, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "RG é obrigatório")]
        [StringLength(20)]
        public string RG { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF é obrigatório")]
        [StringLength(14, MinimumLength = 11)]
        public string CPF { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "Idade deve estar entre 1 e 120 anos")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Data de nascimento é obrigatória")]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Cartão do SUS é obrigatório")]
        [StringLength(20)]
        public string SusCardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [Phone]
        [StringLength(20, MinimumLength = 10)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome da mãe é obrigatório")]
        [StringLength(200, MinimumLength = 2)]
        public string MotherName { get; set; } = string.Empty;
    }
}