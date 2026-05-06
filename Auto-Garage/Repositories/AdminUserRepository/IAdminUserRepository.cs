using Auto_Garage.Models.DomainModels;

namespace Auto_Garage.Repositories.AdminUserRepository
{
    public interface IAdminUserRepository
    {
        Task<List<AutoGarageUser>> GetAllCustomersAsync();
        Task<AutoGarageUser?> GetCustomerByIdAsync(string customerId);
        Task<List<(AutoGarageUser User, string Role)>> GetAllStaffAsync();
        Task<AutoGarageUser?> GetStaffByIdAsync(string staffId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<List<string>> GetRolesAsync(string userId);
        Task SaveChangesAsync();
    }
}