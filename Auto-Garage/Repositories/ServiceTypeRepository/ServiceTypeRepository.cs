using Auto_Garage.Data.AutoGarageDb;
using Auto_Garage.Models.DomainModels.ServiceTypeModel;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Repositories.ServiceTypeRepository
{
    public class ServiceTypeRepository(AutoGarageDbContext db) : IServiceTypeRepository
    {
        private readonly AutoGarageDbContext _db = db;

        public async Task<List<ServiceType>> GetAllActiveAsync() =>
            await _db.ServiceTypes
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

        public async Task<ServiceType?> GetByIdAsync(Guid id) =>
            await _db.ServiceTypes.FindAsync(id);

        public async Task<bool> NameExistsAsync(string name) =>
            await _db.ServiceTypes.AnyAsync(s => s.Name.ToLower() == name.ToLower());

        public async Task AddAsync(ServiceType serviceType) =>
            await _db.ServiceTypes.AddAsync(serviceType);

        public async Task SaveChangesAsync() =>
            await _db.SaveChangesAsync();

        public async Task<List<ServiceType>> GetInactiveAsync()
        {
            return await _db.ServiceTypes
                .Where(x => !x.IsActive)
                .ToListAsync();
        }

        public async Task ReactivateAsync(ServiceType entity)
        {
            entity.IsActive = true;
            _db.ServiceTypes.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}