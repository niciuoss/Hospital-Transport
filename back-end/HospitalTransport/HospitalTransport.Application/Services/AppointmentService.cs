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
                var dateOnly = DateOnly.FromDateTime(request.AppointmentDate.Date);
                var occupiedSeatsInBus = (await _unitOfWork.Appointments.FindAsync(a =>
                    a.IsActive &&
                    a.BusId == request.BusId &&
                    DateOnly.FromDateTime(a.AppointmentDate) == dateOnly))
                    .SelectMany(a => new[] { a.SeatNumber }.Concat(
                        a.CompanionSeatNumber.HasValue ? new[] { a.CompanionSeatNumber.Value } : Array.Empty<int>()
                    ))
                    .Where(s => s > 0)
                    .ToList();

                if (occupiedSeatsInBus.Contains(request.SeatNumber))
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse("Poltrona já está ocupada");
                }

                var bus = await _unitOfWork.Buses.GetByIdAsync(request.BusId);
                if (bus == null)
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse("Ônibus não encontrado");
                }

                if (bus.SeatLayout == "Standard")
                {
                    var prioritySeats = new List<int> { 19, 20 };

                    if (!request.IsPriority && prioritySeats.Contains(request.SeatNumber))
                    {
                        return BaseResponse<AppointmentResponse>.FailureResponse(
                            "Poltronas 19 e 20 são exclusivas para pacientes prioritários"
                        );
                    }
                }

                bool isInfant = patient.Age <= 7;

                // CRIANÇA DE COLO: Obrigatório ter acompanhante
                if (isInfant && !request.CompanionId.HasValue)
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse(
                        "Crianças de 0 a 7 ano devem ter um acompanhante"
                    );
                }

                // CRIANÇA DE COLO sem poltrona própria (no colo)
                if (isInfant && request.SeatNumber == 0)
                {
                    // Criança no colo - não precisa verificar disponibilidade de poltrona do paciente
                    request.IsInfant = true;
                }
                // CRIANÇA DE COLO com cadeirinha (precisa de poltrona)
                else if (isInfant && request.SeatNumber > 0)
                {
                    // Criança com cadeirinha - precisa verificar disponibilidade
                    request.IsInfant = false; // Não é "de colo" nesse caso

                    var isSeatAvailable = await _unitOfWork.Appointments
                        .IsSeatAvailableAsync(request.AppointmentDate, request.SeatNumber);

                    if (!isSeatAvailable)
                    {
                        return BaseResponse<AppointmentResponse>.FailureResponse(
                            $"Poltrona {request.SeatNumber} já está ocupada para esta data"
                        );
                    }
                }

                // Verificar se está tentando usar a poltrona 4 (não existe)
                if (request.SeatNumber == 4)
                {
                    return BaseResponse<AppointmentResponse>.FailureResponse(
                        "A poltrona 4 não existe no ônibus"
                    );
                }

                if (!isInfant || request.SeatNumber > 0)
                {
                    if (request.SeatNumber > 0)
                    {
                        var isSeatAvailable = await _unitOfWork.Appointments
                            .IsSeatAvailableAsync(request.AppointmentDate, request.SeatNumber);

                        if (!isSeatAvailable)
                        {
                            return BaseResponse<AppointmentResponse>.FailureResponse(
                                $"Poltrona {request.SeatNumber} já está ocupada para esta data"
                            );
                        }
                    }
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

                    if (request.CompanionId == request.PatientId)
                    {
                        return BaseResponse<AppointmentResponse>.FailureResponse(
                            "O acompanhante não pode ser o mesmo que o paciente"
                        );
                    }

                    if (!request.CompanionSeatNumber.HasValue)
                    {
                        return BaseResponse<AppointmentResponse>.FailureResponse(
                            "Número da poltrona do acompanhante é obrigatório"
                        );
                    }

                    if (request.CompanionSeatNumber.HasValue &&
                        occupiedSeatsInBus.Contains(request.CompanionSeatNumber.Value))
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
                    BusId = request.BusId,
                    CompanionSeatNumber = request.CompanionSeatNumber,
                    CreatedByUserId = request.CreatedByUserId,
                    IsTicketPrinted = false,
                    IsInfant = request.IsInfant,
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

        public async Task<BaseResponse<IEnumerable<AppointmentResponse>>> GetAllAppointmentsAsync()
        {
            try
            {
                var appointments = await _unitOfWork.Appointments.FindAsync(a => a.IsActive);
                var responses = new List<AppointmentResponse>();

                foreach (var appointment in appointments.OrderByDescending(a => a.AppointmentDate))
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
            Guid busId,
            bool isPriority)
        {
            try
            {
                // ✅ Buscar ônibus
                var bus = await _unitOfWork.Buses.GetByIdAsync(busId);
                if (bus == null)
                {
                    return BaseResponse<IEnumerable<SeatAvailabilityResponse>>.FailureResponse("Ônibus não encontrado");
                }

                var dateOnly = DateOnly.FromDateTime(date.Date);

                // ✅ Buscar agendamentos DESTE ÔNIBUS nesta data
                var appointments = (await _unitOfWork.Appointments.FindAsync(a =>
                    a.IsActive &&
                    a.BusId == busId &&
                    DateOnly.FromDateTime(a.AppointmentDate) == dateOnly))
                    .ToList();

                var occupiedSeats = new List<int>();

                foreach (var appointment in appointments)
                {
                    if (appointment.SeatNumber > 0)
                    {
                        occupiedSeats.Add(appointment.SeatNumber);
                    }

                    if (appointment.CompanionSeatNumber.HasValue)
                    {
                        occupiedSeats.Add(appointment.CompanionSeatNumber.Value);
                    }
                }

                var seatAvailability = new List<SeatAvailabilityResponse>();

                // ✅ Lista de poltronas prioritárias (apenas para ônibus Fortaleza)
                var prioritySeats = new List<int> { 19, 20 };

                // ✅ Gerar poltronas baseado no total do ônibus
                int maxSeat = bus.SeatLayout == "Standard" ? 48 : bus.TotalSeats;
                for (int i = 1; i <= maxSeat; i++)
                {
                    // ✅ Poltrona 4 não existe APENAS no ônibus Fortaleza
                    if (bus.SeatLayout == "Standard" && i == 4)
                    {
                        continue;
                    }

                    bool isPriorityOnly = prioritySeats.Contains(i) && bus.SeatLayout == "Standard";
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

        public async Task<BaseResponse<bool>> DeleteAppointmentAsync(Guid id)
        {
            try
            {
                var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
                if (appointment == null)
                {
                    return BaseResponse<bool>.FailureResponse("Agendamento não encontrado");
                }

                // Soft delete
                appointment.IsActive = false;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Appointments.UpdateAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                return BaseResponse<bool>.SuccessResponse(true, "Agendamento cancelado com sucesso");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.FailureResponse($"Erro ao cancelar agendamento: {ex.Message}");
            }
        }

        public async Task<BaseResponse<byte[]>> GenerateMonthlyReportPdfAsync(int year, int month)
        {
            try
            {
                // Calcular período do mês
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                Console.WriteLine($"📅 Buscando agendamentos de {startDate:dd/MM/yyyy} até {endDate:dd/MM/yyyy}");

                // Buscar todos os agendamentos do mês
                var appointments = (await _unitOfWork.Appointments.FindAsync(a =>
                    a.IsActive &&
                    a.AppointmentDate >= startDate &&
                    a.AppointmentDate <= endDate))
                    .OrderBy(a => a.AppointmentDate)
                    .ToList();

                Console.WriteLine($"✅ Encontrados {appointments.Count} agendamentos");

                if (!appointments.Any())
                {
                    return BaseResponse<byte[]>.FailureResponse(
                        "Nenhum agendamento encontrado para o período selecionado"
                    );
                }

                // Carregar dados relacionados (Patient e Companion)
                foreach (var appointment in appointments)
                {
                    // Carregar paciente
                    if (appointment.Patient == null)
                    {
                        appointment.Patient = await _unitOfWork.Patients.GetByIdAsync(appointment.PatientId);
                        Console.WriteLine($"✅ Carregado paciente: {appointment.Patient?.FullName}");
                    }

                    // Carregar acompanhante (se existir)
                    if (appointment.CompanionId.HasValue && appointment.Companion == null)
                    {
                        appointment.Companion = await _unitOfWork.Patients.GetByIdAsync(appointment.CompanionId.Value);
                        Console.WriteLine($"✅ Carregado acompanhante: {appointment.Companion?.FullName}");
                    }
                }

                Console.WriteLine("🔄 Gerando PDF...");

                // Gerar PDF
                var pdfBytes = _pdfService.GenerateMonthlyReportPdf(appointments, year, month);

                Console.WriteLine($"✅ PDF gerado com sucesso! Tamanho: {pdfBytes.Length} bytes");

                return BaseResponse<byte[]>.SuccessResponse(pdfBytes, "Relatório gerado com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO COMPLETO: {ex.ToString()}");
                return BaseResponse<byte[]>.FailureResponse($"Erro ao gerar relatório: {ex.Message}");
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