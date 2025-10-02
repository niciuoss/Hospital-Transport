using FluentValidation;
using HospitalTransport.Application.DTOs.Appointment;
using HospitalTransport.Application.DTOs.Common;
using HospitalTransport.Application.DTOs.Patient;
using HospitalTransport.Application.Interfaces;
using HospitalTransport.Domain.Entities;
using HospitalTransport.Domain.Enums;
using HospitalTransport.Domain.Interfaces;

namespace HospitalTransport.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateAppointmentRequest> _createValidator;
        private readonly IPdfService _pdfService;

        public AppointmentService(
            IUnitOfWork unitOfWork,
            IValidator<CreateAppointmentRequest> createValidator,
            IPdfService pdfService)
        {
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _pdfService = pdfService;
        }

        public async Task<BaseResponse<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request)
        {
            try
            {
                var validationResult = await _createValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse(
                        "Dados inválidos",
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }

                // Verificar se o paciente existe
                var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId);
                if (patient == null)
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse("Paciente não encontrado");
                }

                // Verificar se a poltrona está disponível
                var occupiedSeats = await _unitOfWork.Appointments.GetOccupiedSeatsAsync(request.AppointmentDate);
                if (occupiedSeats.Contains(request.SeatNumber))
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse("Poltrona já está ocupada");
                }

                // Verificar restrição de prioridade (poltronas 1, 2, 3)
                if (!request.IsPriority && request.SeatNumber <= 3)
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse(
                        "Poltronas 1, 2 e 3 são exclusivas para pacientes prioritários"
                    );
                }

                // Verificar acompanhante se informado
                Patient? companion = null;
                if (request.CompanionId.HasValue)
                {
                    companion = await _unitOfWork.Patients.GetByIdAsync(request.CompanionId.Value);
                    if (companion == null)
                    {
                        return BaseResponse<AppointmentResponse>.FailureResponse("Acompanhante não encontrado");
                    }

                    if (request.CompanionSeatNumber.HasValue &&
                        occupiedSeats.Contains(request.CompanionSeatNumber.Value))
                    {
                        return BaseResponse<AppointmentResponse>.FailureResponse(
                            "Poltrona do acompanhante já está ocupada"
                        );
                    }
                }

                // Verificar se o usuário existe
                var user = await _unitOfWork.Users.GetByIdAsync(request.CreatedByUserId);
                if (user == null)
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse("Usuário não encontrado");
                }

                var appointment = new Appointment
                {
                    PatientId = request.PatientId,
                    MedicalRecordNumber = request.MedicalRecordNumber,
                    DestinationHospital = request.DestinationHospital,
                    TreatmentType = (TreatmentType)request.TreatmentType,
                    TreatmentTypeOther = request.TreatmentTypeOther,
                    IsPriority = request.IsPriority,
                    SeatNumber = request.SeatNumber,
                    AppointmentDate = request.AppointmentDate,
                    CompanionId = request.CompanionId,
                    CompanionSeatNumber = request.CompanionSeatNumber,
                    CreatedByUserId = request.CreatedByUserId,
                    IsTicketPrinted = false
                };

                await _unitOfWork.Appointments.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                // Recarregar com includes
                var createdAppointment = await _unitOfWork.Appointments.GetByIdAsync(appointment.Id);

                var response = MapToAppointmentResponse(createdAppointment!, patient, companion, user);

                return BaseResponse<AppointmentResponse>.SuccessResponse(
                    response,
                    "Agendamento criado com sucesso"
                );
            }
            catch (Exception ex)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    $"Erro ao criar agendamento: {ex.Message}"
                );
            }
        }

        public async Task<BaseResponse<AppointmentResponse>> GetAppointmentByIdAsync(Guid id)
        {
            try
            {
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
                if (appointment == null)
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse("Agendamento não encontrado");
                }

                var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
                var user = await _unitOfWork.Users.GetByIdAsync(appointment.CreatedByUserId);
                Patient? companion = null;

                if (appointment.CompanionId.HasValue)
                {
                    companion = await _unitOfWork.Patients.GetByIdAsync(appointment.CompanionId.Value);
                }

                var response = MapToAppointmentResponse(appointment, patient!, companion, user!);

                return BaseResponse<AppointmentResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<AppointmentResponse>.FailureResponse(
                    $"Erro ao buscar agendamento: {ex.Message}"
                );
            }
        }

        public async Task<BaseResponse<IEnumerable<AppointmentResponse>>> GetRecentAppointmentsAsync(int count)
        {
            try
            {
                var appointments = await _unitOfWork.Appointments.GetRecentAppointmentsAsync(count);
                var responses = new List<AppointmentResponse>();

                foreach (var appointment in appointments)
                {
                    var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
                    var user = await _unitOfWork.Users.GetByIdAsync(appointment.CreatedByUserId);
                    Patient? companion = null;

                    if (appointment.CompanionId.HasValue)
                    {
                        companion = await _unitOfWork.Patients.GetByIdAsync(appointment.CompanionId.Value);
                    }

                    responses.Add(MapToAppointmentResponse(appointment, patient!, companion, user!));
                }

                return BaseResponse<IEnumerable<AppointmentResponse>>.SuccessResponse(responses);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<AppointmentResponse>>.FailureResponse(
                    $"Erro ao buscar agendamentos: {ex.Message}"
                );
            }
        }

        public async Task<BaseResponse<IEnumerable<SeatAvailabilityResponse>>> GetSeatAvailabilityAsync(
            DateTime date,
            bool isPriority)
        {
            try
            {
                var occupiedSeats = await _unitOfWork.Appointments.GetOccupiedSeatsAsync(date);
                var seatAvailability = new List<SeatAvailabilityResponse>();

                for (int i = 1; i <= 46; i++)
                {
                    bool isPriorityOnly = i <= 3;
                    bool isOccupied = occupiedSeats.Contains(i);
                    bool isAvailable = !isOccupied && (isPriority || !isPriorityOnly);

                    seatAvailability.Add(new SeatAvailabilityResponse
                    {
                        SeatNumber = i,
                        IsAvailable = isAvailable,
                        IsPriorityOnly = isPriorityOnly
                    });
                }

                return BaseResponse<IEnumerable<SeatAvailabilityResponse>>.SuccessResponse(seatAvailability);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<SeatAvailabilityResponse>>.FailureResponse(
                    $"Erro ao buscar disponibilidade: {ex.Message}"
                );
            }
        }

        public async Task<BaseResponse<IEnumerable<AppointmentResponse>>> SearchAppointmentsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BaseResponse<IEnumerable<AppointmentResponse>>.SuccessResponse(
                        new List<AppointmentResponse>()
                    );
                }

                var appointments = await _unitOfWork.Appointments.SearchAppointmentsAsync(searchTerm);
                var responses = new List<AppointmentResponse>();

                foreach (var appointment in appointments)
                {
                    var patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
                    var user = await _unitOfWork.Users.GetByIdAsync(appointment.CreatedByUserId);
                    Patient? companion = null;

                    if (appointment.CompanionId.HasValue)
                    {
                        companion = await _unitOfWork.Patients.GetByIdAsync(appointment.CompanionId.Value);
                    }

                    responses.Add(MapToAppointmentResponse(appointment, patient!, companion, user!));
                }

                return BaseResponse<IEnumerable<AppointmentResponse>>.SuccessResponse(responses);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<AppointmentResponse>>.FailureResponse(
                    $"Erro ao buscar agendamentos: {ex.Message}"
                );
            }
        }

        public async Task<BaseResponse<byte[]>> GenerateTicketPdfAsync(Guid appointmentId)
        {
            try
            {
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return BaseResponse<byte[]>.FailureResponse("Agendamento não encontrado");
                }

                var pdfBytes = _pdfService.GenerateAppointmentTicket(appointment);

                // Atualizar status de impressão
                appointment.IsTicketPrinted = true;
                appointment.PrintedAt = DateTime.UtcNow;
                await _unitOfWork.Appointments.UpdateAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                return BaseResponse<byte[]>.SuccessResponse(pdfBytes, "PDF gerado com sucesso");
            }
            catch (Exception ex)
            {
                return BaseResponse<byte[]>.FailureResponse($"Erro ao gerar PDF: {ex.Message}");
            }
        }

        private AppointmentResponse MapToAppointmentResponse(
            Appointment appointment,
            Patient patient,
            Patient? companion,
            User user)
        {
            return new AppointmentResponse
            {
                Id = appointment.Id,
                Patient = new PatientResponse
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
                },
                MedicalRecordNumber = appointment.MedicalRecordNumber,
                DestinationHospital = appointment.DestinationHospital,
                TreatmentType = appointment.TreatmentType.ToString(),
                TreatmentTypeOther = appointment.TreatmentTypeOther,
                IsPriority = appointment.IsPriority,
                SeatNumber = appointment.SeatNumber,
                AppointmentDate = appointment.AppointmentDate,
                Companion = companion != null ? new PatientResponse
                {
                    Id = companion.Id,
                    FullName = companion.FullName,
                    RG = companion.RG,
                    CPF = companion.CPF,
                    Age = companion.Age,
                    BirthDate = companion.BirthDate,
                    SusCardNumber = companion.SusCardNumber,
                    PhoneNumber = companion.PhoneNumber,
                    MotherName = companion.MotherName,
                    CreatedAt = companion.CreatedAt
                } : null,
                CompanionSeatNumber = appointment.CompanionSeatNumber,
                CreatedByUserName = user.FullName,
                CreatedAt = appointment.CreatedAt,
                IsTicketPrinted = appointment.IsTicketPrinted
            };
        }
    }
}