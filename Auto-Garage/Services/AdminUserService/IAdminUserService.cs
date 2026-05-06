using Auto_Garage.Models.DtoModels.AdminUserDtos;

namespace Auto_Garage.Services.AdminUserService
{
    public interface IAdminUserService
    {
        // Customers
        Task<List<AdminCustomerDto>> GetAllCustomersAsync();
        Task<AdminCustomerDetailDto> GetCustomerByIdAsync(string customerId);
        Task<AdminCustomerDto> ToggleCustomerActiveAsync(string customerId);

        // Staff
        Task<List<AdminStaffDto>> GetAllStaffAsync();
        Task<AdminStaffDto> ToggleStaffActiveAsync(string staffId);
    }
}