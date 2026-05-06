using Auto_Garage.Models.DtoModels.ServiceTypeDtos;

namespace Auto_Garage.Services.ServiceTypeService
{
    public interface IServiceTypeService
    {
        Task<List<ServiceTypeResponseDto>> GetAllAsync();
        Task<ServiceTypeResponseDto> CreateAsync(AddServiceTypeRequestDto dto);
        Task<ServiceTypeResponseDto> UpdateAsync(Guid id, AddServiceTypeRequestDto dto);
        Task DeleteAsync(Guid id);
        Task<List<ServiceTypeResponseDto>> GetInactiveAsync();
        Task ReactivateAsync(Guid id);
    }
}