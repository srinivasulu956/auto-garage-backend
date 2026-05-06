using Auto_Garage.Models.DomainModels.ServiceTypeModel;

namespace Auto_Garage.Repositories.ServiceTypeRepository
{
    public interface IServiceTypeRepository
    {
        Task<List<ServiceType>> GetAllActiveAsync();
        Task<ServiceType?> GetByIdAsync(Guid id);
        Task<bool> NameExistsAsync(string name);
        Task AddAsync(ServiceType serviceType);
        Task SaveChangesAsync();
        Task<List<ServiceType>> GetInactiveAsync();
        Task ReactivateAsync(ServiceType entity);
    }
}