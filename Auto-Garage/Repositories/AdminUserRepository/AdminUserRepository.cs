using Auto_Garage.Data.AutoGarageAuthDb;
using Auto_Garage.Models.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Repositories.AdminUserRepository
{
    public class AdminUserRepository(AutoGarageAuthDbContext authDb) : IAdminUserRepository
    {
        private readonly AutoGarageAuthDbContext _authDb = authDb;

        // ── Customers ─────────────────────────────────────────────────────────

        public async Task<List<AutoGarageUser>> GetAllCustomersAsync()
        {
            var customerRole = await _authDb.Roles
                .FirstOrDefaultAsync(r => r.Name == "Customer");
            if (customerRole is null) return new();

            var customerIds = await _authDb.UserRoles
                .Where(ur => ur.RoleId == customerRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _authDb.Users
                .Where(u => customerIds.Contains(u.Id) && !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<AutoGarageUser?> GetCustomerByIdAsync(string customerId) =>
            await _authDb.Users
                .FirstOrDefaultAsync(u => u.Id == customerId && !u.IsDeleted);

        // ── Staff ─────────────────────────────────────────────────────────────

        public async Task<List<(AutoGarageUser User, string Role)>> GetAllStaffAsync()
        {
            // Staff = Admin + Mechanic roles (excluding Customer)
            var staffRoles = await _authDb.Roles
                .Where(r => r.Name == "Admin" || r.Name == "Mechanic")
                .ToListAsync();

            if (!staffRoles.Any()) return new();

            var staffRoleIds = staffRoles.Select(r => r.Id).ToList();

            var userRoles = await _authDb.UserRoles
                .Where(ur => staffRoleIds.Contains(ur.RoleId))
                .ToListAsync();

            var userIds = userRoles.Select(ur => ur.UserId).Distinct().ToList();

            var users = await _authDb.Users
                .Where(u => userIds.Contains(u.Id) && !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            // Build (user, role) pairs — one entry per user (first role wins)
            return users.Select(u =>
            {
                var ur = userRoles.FirstOrDefault(r => r.UserId == u.Id);
                var role = staffRoles.FirstOrDefault(r => r.Id == ur?.RoleId)?.Name ?? "Unknown";
                return (u, role);
            }).ToList();
        }

        public async Task<AutoGarageUser?> GetStaffByIdAsync(string staffId) =>
            await _authDb.Users
                .FirstOrDefaultAsync(u => u.Id == staffId && !u.IsDeleted);

        public async Task<bool> IsInRoleAsync(string userId, string role)
        {
            var roleEntity = await _authDb.Roles.FirstOrDefaultAsync(r => r.Name == role);
            if (roleEntity is null) return false;
            return await _authDb.UserRoles.AnyAsync(ur =>
                ur.UserId == userId && ur.RoleId == roleEntity.Id);
        }

        public async Task<List<string>> GetRolesAsync(string userId)
        {
            var roleIds = await _authDb.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            return await _authDb.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync();
        }

        public async Task SaveChangesAsync() =>
            await _authDb.SaveChangesAsync();
    }
}