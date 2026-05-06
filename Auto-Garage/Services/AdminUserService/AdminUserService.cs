using Auto_Garage.Data.AutoGarageDb;
using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using Auto_Garage.Models.DtoModels.AdminUserDtos;
using Auto_Garage.Repositories.AdminUserRepository;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Services.AdminUserService
{
    public class AdminUserService(
        IAdminUserRepository adminUserRepository,
        AutoGarageDbContext db,
        ILogger<AdminUserService> logger) : IAdminUserService
    {
        private readonly IAdminUserRepository _repo = adminUserRepository;
        private readonly AutoGarageDbContext _db = db;
        private readonly ILogger<AdminUserService> _logger = logger;

        // CUSTOMERS
        public async Task<List<AdminCustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _repo.GetAllCustomersAsync();
            var result = new List<AdminCustomerDto>();

            foreach (var c in customers)
            {
                var vehicleCount = await _db.Vehicles.CountAsync(v => v.CustomerId == c.Id);
                var bookingCount = await _db.ServiceBookings.CountAsync(b => b.CustomerId == c.Id);
                var invoiceCount = await _db.Invoices.CountAsync(i => i.CustomerId == c.Id);

                result.Add(new AdminCustomerDto
                {
                    Id = c.Id,
                    FirstName = c.FirstName ?? string.Empty,
                    LastName = c.LastName ?? string.Empty,
                    Email = c.Email ?? string.Empty,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    VehicleCount = vehicleCount,
                    BookingCount = bookingCount,
                    InvoiceCount = invoiceCount,
                });
            }

            return result;
        }

        public async Task<AdminCustomerDetailDto> GetCustomerByIdAsync(string customerId)
        {
            var customer = await _repo.GetCustomerByIdAsync(customerId)
                ?? throw new KeyNotFoundException("Customer not found.");

            var vehicles = await _db.Vehicles
                .Where(v => v.CustomerId == customerId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var bookings = await _db.ServiceBookings
                .Include(b => b.ServiceType)
                .Include(b => b.Vehicle)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var invoices = await _db.Invoices
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.IssuedAt)
                .ToListAsync();

            return new AdminCustomerDetailDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName ?? string.Empty,
                LastName = customer.LastName ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                VehicleCount = vehicles.Count,
                BookingCount = bookings.Count,
                InvoiceCount = invoices.Count,

                Vehicles = vehicles.Select(v => new AdminCustomerVehicleDto
                {
                    Id = v.Id,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    LicensePlate = v.LicensePlate,
                    FuelType = v.FuelType,
                    IsActive = v.IsActive,
                    CreatedAt = v.CreatedAt,
                }).ToList(),

                Bookings = bookings.Select(b => new AdminCustomerBookingDto
                {
                    Id = b.Id,
                    ServiceName = b.ServiceType?.Name ?? string.Empty,
                    VehiclePlate = b.Vehicle?.LicensePlate ?? string.Empty,
                    StatusLabel = GetStatusLabel(b.Status),
                    ScheduledDate = b.ScheduledDate,
                    CreatedAt = b.CreatedAt,
                }).ToList(),

                Invoices = invoices.Select(i => new AdminCustomerInvoiceDto
                {
                    Id = i.Id,
                    TotalAmount = i.TotalAmount,
                    StatusLabel = i.Status == Models.DomainModels.InvoiceModel.InvoiceStatus.Paid ? "Paid" : "Unpaid",
                    IssuedAt = i.IssuedAt,
                    PaidAt = i.PaidAt,
                }).ToList(),
            };
        }

        public async Task<AdminCustomerDto> ToggleCustomerActiveAsync(string customerId)
        {
            var customer = await _repo.GetCustomerByIdAsync(customerId)
                ?? throw new KeyNotFoundException("Customer not found.");

            customer.IsActive = !customer.IsActive;
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Customer {Id} IsActive set to {Active}", customerId, customer.IsActive);

            var vc = await _db.Vehicles.CountAsync(v => v.CustomerId == customerId);
            var bc = await _db.ServiceBookings.CountAsync(b => b.CustomerId == customerId);
            var ic = await _db.Invoices.CountAsync(i => i.CustomerId == customerId);

            return new AdminCustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName ?? string.Empty,
                LastName = customer.LastName ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
                VehicleCount = vc,
                BookingCount = bc,
                InvoiceCount = ic,
            };
        }

        // STAFF
        public async Task<List<AdminStaffDto>> GetAllStaffAsync()
        {
            var staff = await _repo.GetAllStaffAsync();
            var result = new List<AdminStaffDto>();

            foreach (var (user, role) in staff)
            {
                var activeJobs = role == "Mechanic"
                    ? await _db.ServiceBookings.CountAsync(b =>
                        b.AssignedMechanicId == user.Id &&
                        b.Status != BookingStatus.Cancelled &&
                        b.Status != BookingStatus.Paid &&
                        b.Status != BookingStatus.Completed)
                    : 0;

                result.Add(new AdminStaffDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    ActiveJobCount = activeJobs,
                });
            }

            return result;
        }

        public async Task<AdminStaffDto> ToggleStaffActiveAsync(string staffId)
        {
            var staff = await _repo.GetStaffByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Staff member not found.");

            staff.IsActive = !staff.IsActive;
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Staff {Id} IsActive set to {Active}", staffId, staff.IsActive);

            var roles = await _repo.GetRolesAsync(staffId);
            var role = roles.FirstOrDefault() ?? "Unknown";
            var activeJobs = role == "Mechanic"
                ? await _db.ServiceBookings.CountAsync(b =>
                    b.AssignedMechanicId == staffId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.Paid &&
                    b.Status != BookingStatus.Completed)
                : 0;

            return new AdminStaffDto
            {
                Id = staff.Id,
                FirstName = staff.FirstName ?? string.Empty,
                LastName = staff.LastName ?? string.Empty,
                Email = staff.Email ?? string.Empty,
                Role = role,
                IsActive = staff.IsActive,
                CreatedAt = staff.CreatedAt,
                ActiveJobCount = activeJobs,
            };
        }

        // Helper

        private static string GetStatusLabel(BookingStatus s) => s switch
        {
            BookingStatus.Pending => "Pending",
            BookingStatus.Confirmed => "Confirmed",
            BookingStatus.AssignedToMechanic => "Assigned",
            BookingStatus.InProgress => "In Progress",
            BookingStatus.WaitingForParts => "Waiting Parts",
            BookingStatus.QualityCheck => "Quality Check",
            BookingStatus.Completed => "Completed",
            BookingStatus.InvoiceGenerated => "Invoice Sent",
            BookingStatus.Paid => "Paid",
            BookingStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };
    }
}