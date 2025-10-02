using FluentValidation;
using HospitalTransport.Application.DTOs.Common;
using HospitalTransport.Application.DTOs.Patient;
using HospitalTransport.Application.Interfaces;
using HospitalTransport.Domain.Entities;
using HospitalTransport.Domain.Interfaces;

namespace HospitalTransport.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreatePatientRequest> _createValidator;

        public PatientService(
            IUnitOfWork unitOfWork,
            IValidator<CreatePatientRequest> createValidator)
        {
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
        }

        public async Task<BaseResponse<PatientResponse>> CreatePatientAsync(CreatePatientRequest request)
        {
            try
            {
                var validationResult = await _createValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BaseResponse<PatientResponse>.FailureResponse(
                        "Dados inválidos",
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                // Verificar se CPF já existe
                var existingPatient = await _unitOfWork.Patients.GetByCPFAsync(request.CPF);
                if (existingPatient != null)
                {
                    return BaseResponse<PatientResponse>.FailureResponse("CPF já cadastrado no sistema");
                }

                // Verificar se cartão SUS já existe
                existingPatient = await _unitOfWork.Patients.GetBySusCardAsync(request.SusCardNumber);
                if (existingPatient != null)
                {
                    return BaseResponse<PatientResponse>.FailureResponse("Cartão SUS já cadastrado no sistema");
                }

                var patient = new Patient
                {
                    FullName = request.FullName,
                    RG = request.RG,
                    CPF = request.CPF,
                    Age = request.Age,
                    BirthDate = request.BirthDate,
                    SusCardNumber = request.SusCardNumber,
                    PhoneNumber = request.PhoneNumber,
                    MotherName = request.MotherName
                };

                await _unitOfWork.Patients.AddAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToPatientResponse(patient);

                return BaseResponse<PatientResponse>.SuccessResponse(
                    response,
                    "Paciente cadastrado com sucesso"
                );
            }
            catch (Exception ex)
            {
                return BaseResponse<PatientResponse>.FailureResponse($"Erro ao cadastrar paciente: {ex.Message}");
            }
        }

        public async Task<BaseResponse<PatientResponse>> UpdatePatientAsync(UpdatePatientRequest request)
        {
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(request.Id);
                if (patient == null)
                {
                    return BaseResponse<PatientResponse>.FailureResponse("Paciente não encontrado");
                }
                // Verificar se CPF já existe em outro paciente
                var existingPatient = await _unitOfWork.Patients.GetByCPFAsync(request.CPF);
                if (existingPatient != null && existingPatient.Id != request.Id)
                {
                    return BaseResponse<PatientResponse>.FailureResponse("CPF já cadastrado para outro paciente");
                }

                // Verificar se cartão SUS já existe em outro paciente
                existingPatient = await _unitOfWork.Patients.GetBySusCardAsync(request.SusCardNumber);
                if (existingPatient != null && existingPatient.Id != request.Id)
                {
                    return BaseResponse<PatientResponse>.FailureResponse("Cartão SUS já cadastrado para outro paciente");
                }

                patient.FullName = request.FullName;
                patient.RG = request.RG;
                patient.CPF = request.CPF;
                patient.Age = request.Age;
                patient.BirthDate = request.BirthDate;
                patient.SusCardNumber = request.SusCardNumber;
                patient.PhoneNumber = request.PhoneNumber;
                patient.MotherName = request.MotherName;
                patient.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Patients.UpdateAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToPatientResponse(patient);

                return BaseResponse<PatientResponse>.SuccessResponse(
                    response,
                    "Paciente atualizado com sucesso"
                );
            }
            catch (Exception ex)
            {
                return BaseResponse<PatientResponse>.FailureResponse($"Erro ao atualizar paciente: {ex.Message}");
            }
        }

        public async Task<BaseResponse<PatientResponse>> GetPatientByIdAsync(Guid id)
        {
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(id);
                if (patient == null)
                {
                    return BaseResponse<PatientResponse>.FailureResponse("Paciente não encontrado");
                }

                var response = MapToPatientResponse(patient);
                return BaseResponse<PatientResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<PatientResponse>.FailureResponse($"Erro ao buscar paciente: {ex.Message}");
            }
        }

        public async Task<BaseResponse<IEnumerable<PatientSearchResult>>> SearchPatientsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BaseResponse<IEnumerable<PatientSearchResult>>.SuccessResponse(
                        new List<PatientSearchResult>()
                    );
                }

                var patients = await _unitOfWork.Patients.SearchPatientsAsync(searchTerm);

                var results = patients.Select(p => new PatientSearchResult
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    SusCardNumber = p.SusCardNumber,
                    CPF = p.CPF
                }).ToList();

                return BaseResponse<IEnumerable<PatientSearchResult>>.SuccessResponse(results);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<PatientSearchResult>>.FailureResponse(
                    $"Erro ao buscar pacientes: {ex.Message}"
                );
            }
        }

        public async Task<BaseResponse<bool>> DeletePatientAsync(Guid id)
        {
            try
            {
                var patient = await _unitOfWork.Patients.GetByIdAsync(id);
                if (patient == null)
                {
                    return BaseResponse<bool>.FailureResponse("Paciente não encontrado");
                }

                patient.IsActive = false;
                patient.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Patients.UpdateAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                return BaseResponse<bool>.SuccessResponse(true, "Paciente removido com sucesso");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.FailureResponse($"Erro ao remover paciente: {ex.Message}");
            }
        }

        private PatientResponse MapToPatientResponse(Patient patient)
        {
            return new PatientResponse
            {
                Id = patient.Id,
                FullName = patient.FullName,
                RG = patient.RG,
                CPF = patient.CPF,
                Age = patient.Age,
                BirthDate = patient.BirthDate,
                SusCardNumber = patient.SusCardNumber,
                PhoneNumber = patient.PhoneNumber,
                MotherName = patient.MotherName,
                CreatedAt = patient.CreatedAt
            };
        }
    }
}