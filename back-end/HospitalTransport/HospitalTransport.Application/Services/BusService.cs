using HospitalTransport.Application.DTOs.Bus;
using HospitalTransport.Application.DTOs.Common;
using HospitalTransport.Application.Interfaces;
using HospitalTransport.Domain.Interfaces;

namespace HospitalTransport.Application.Services
{
    public class BusService : IBusService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<IEnumerable<BusResponse>>> GetActiveBusesAsync()
        {
            try
            {
                var buses = await _unitOfWork.Buses.GetActiveBusesAsync();

                var response = buses.Select(b => new BusResponse
                {
                    Id = b.Id,
                    Name = b.Name,
                    Destination = b.Destination,
                    TotalSeats = b.TotalSeats,
                    SeatLayout = b.SeatLayout,
                    IsActive = b.IsActive
                });

                return BaseResponse<IEnumerable<BusResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<IEnumerable<BusResponse>>.FailureResponse($"Erro ao buscar ônibus: {ex.Message}");
            }
        }

        public async Task<BaseResponse<BusResponse>> GetBusByIdAsync(Guid id)
        {
            try
            {
                var bus = await _unitOfWork.Buses.GetByIdAsync(id);

                if (bus == null)
                {
                    return BaseResponse<BusResponse>.FailureResponse("Ônibus não encontrado");
                }

                var response = new BusResponse
                {
                    Id = bus.Id,
                    Name = bus.Name,
                    Destination = bus.Destination,
                    TotalSeats = bus.TotalSeats,
                    SeatLayout = bus.SeatLayout,
                    IsActive = bus.IsActive
                };

                return BaseResponse<BusResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return BaseResponse<BusResponse>.FailureResponse($"Erro ao buscar ônibus: {ex.Message}");
            }
        }
    }
}
