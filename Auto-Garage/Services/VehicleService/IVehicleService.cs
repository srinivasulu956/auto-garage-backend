using Auto_Garage.Models.DtoModels.VehicleDtos;

namespace Auto_Garage.Services.VehicleService
{
    public interface IVehicleService
    {
        Task<List<VehicleResponseDto>> GetAllActiveAsync(string customerId);
        Task<List<VehicleResponseDto>> GetAllInactiveAsync(string customerId);
        Task<VehicleResponseDto> GetByIdAsync(Guid id, string customerId);
        Task<VehicleResponseDto> CreateAsync(string customerId, AddVehicleRequestDto dto);
        Task<VehicleResponseDto> UpdateAsync(Guid id, string customerId, UpdateVehicleRequestDto dto);
        Task<VehicleResponseDto> ReactivateAsync(Guid id, string customerId);
        Task DeleteAsync(Guid id, string customerId);
        Task<IEnumerable<VehicleResponseDto>> GetAllByCustomerIdAsync(string customerId);
    }
}