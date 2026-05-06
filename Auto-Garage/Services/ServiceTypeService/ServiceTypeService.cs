using Auto_Garage.Models.DomainModels.ServiceTypeModel;
using Auto_Garage.Models.DtoModels.ServiceTypeDtos;
using Auto_Garage.Repositories.ServiceTypeRepository;

namespace Auto_Garage.Services.ServiceTypeService
{
    public class ServiceTypeService(
        IServiceTypeRepository serviceTypeRepository,
        ILogger<ServiceTypeService> logger) : IServiceTypeService
    {
        private readonly IServiceTypeRepository _serviceTypeRepository = serviceTypeRepository;
        private readonly ILogger<ServiceTypeService> _logger = logger;

        public async Task<List<ServiceTypeResponseDto>> GetAllAsync()
        {
            var types = await _serviceTypeRepository.GetAllActiveAsync();
            return types.Select(ToDto).ToList();
        }

        public async Task<ServiceTypeResponseDto> CreateAsync(AddServiceTypeRequestDto dto)
        {
            if (await _serviceTypeRepository.NameExistsAsync(dto.Name))
                throw new InvalidOperationException("A service type with this name already exists.");

            var serviceType = new ServiceType
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                EstimatedHours = dto.EstimatedHours,
            };

            await _serviceTypeRepository.AddAsync(serviceType);
            await _serviceTypeRepository.SaveChangesAsync();

            _logger.LogInformation("ServiceType '{Name}' created", dto.Name);

            return ToDto(serviceType);
        }

        public async Task<ServiceTypeResponseDto> UpdateAsync(Guid id, AddServiceTypeRequestDto dto)
        {
            var serviceType = await _serviceTypeRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Service type not found.");

            serviceType.Name = dto.Name;
            serviceType.Description = dto.Description;
            serviceType.BasePrice = dto.BasePrice;
            serviceType.EstimatedHours = dto.EstimatedHours;

            await _serviceTypeRepository.SaveChangesAsync();

            _logger.LogInformation("ServiceType {Id} updated", id);

            return ToDto(serviceType);
        }

        public async Task DeleteAsync(Guid id)
        {
            var serviceType = await _serviceTypeRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Service type not found.");

            // Soft delete — existing bookings still reference this service type
            serviceType.IsActive = false;
            await _serviceTypeRepository.SaveChangesAsync();

            _logger.LogInformation("ServiceType {Id} deactivated", id);
        }

        private static ServiceTypeResponseDto ToDto(ServiceType s) => new()
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            BasePrice = s.BasePrice,
            EstimatedHours = s.EstimatedHours
        };

        public async Task<List<ServiceTypeResponseDto>> GetInactiveAsync()
        {
            var data = await _serviceTypeRepository.GetInactiveAsync();

            return data.Select(x => new ServiceTypeResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                BasePrice = x.BasePrice,
                EstimatedHours = x.EstimatedHours
            }).ToList();
        }

        public async Task ReactivateAsync(Guid id)
        {
            var entity = await _serviceTypeRepository.GetByIdAsync(id);

            if (entity == null)
                throw new KeyNotFoundException("Service not found");

            await _serviceTypeRepository.ReactivateAsync(entity);
        }
    }
}