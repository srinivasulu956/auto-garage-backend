using Auto_Garage.Data.AutoGarageAuthDb;
using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using Auto_Garage.Models.DtoModels.AdminBookingDtos;
using Auto_Garage.Models.DtoModels.ServiceTypeDtos;
using Auto_Garage.Models.DtoModels.VehicleDtos;
using Auto_Garage.Repositories.AdminBookingRepository;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Services.AdminBookingService
{
    public class AdminBookingService(
        IAdminBookingRepository adminBookingRepository,
        AutoGarageAuthDbContext authDb,
        ILogger<AdminBookingService> logger) : IAdminBookingService
    {
        private readonly IAdminBookingRepository _repo = adminBookingRepository;
        private readonly AutoGarageAuthDbContext _authDb = authDb;
        private readonly ILogger<AdminBookingService> _logger = logger;

        private static IReadOnlyCollection<BookingStatus> AdminAllowedStatuses =>
            new[] { BookingStatus.Completed };

        private static IReadOnlyCollection<BookingStatus> MechanicAllowedStatuses =>
            new[]
            {
                BookingStatus.InProgress,
                BookingStatus.WaitingForParts,
                BookingStatus.QualityCheck,
            };

        // ════════════════════════════════════════════════════════════════════
        // ADMIN
        // ════════════════════════════════════════════════════════════════════

        public async Task<List<AdminBookingResponseDto>> GetAllAsync()
        {
            var bookings = await _repo.GetAllAsync();
            return await MapListAsync(bookings);
        }

        public async Task<AdminBookingDetailResponseDto> GetByIdAsync(Guid id)
        {
            var booking = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Booking not found.");
            return await MapDetailAsync(booking);
        }

        // ── Confirm (Pending → Confirmed) ─────────────────────────────────────

        public async Task<AdminBookingResponseDto> ConfirmAsync(Guid id, string adminId, ConfirmBookingRequestDto dto)
        {
            var booking = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException(
                    $"Only Pending bookings can be confirmed. Current status: {GetStatusLabel(booking.Status)}");

            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;

            await _repo.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.Confirmed,
                ChangedByUserId = adminId,
                ChangedByRole = "Admin",
                Notes = dto.Notes ?? "Booking confirmed by admin."
            });

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Booking {Id} confirmed by Admin {AdminId}", id, adminId);
            return await MapAsync(booking);
        }

        // ── Assign Mechanic (Confirmed → AssignedToMechanic) ──────────────────

        public async Task<AdminBookingResponseDto> AssignMechanicAsync(Guid id, string adminId, AssignMechanicRequestDto dto)
        {
            var booking = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.Status != BookingStatus.Confirmed)
                throw new InvalidOperationException(
                    $"Only Confirmed bookings can have a mechanic assigned. Current status: {GetStatusLabel(booking.Status)}");

            var mechanic = await _authDb.Users.FirstOrDefaultAsync(u => u.Id == dto.MechanicId)
                ?? throw new ArgumentException("Mechanic not found.");

            var isMechanic = await _authDb.UserRoles
                .Join(_authDb.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .AnyAsync(x => x.UserId == dto.MechanicId && x.Name == "Mechanic");

            if (!isMechanic)
                throw new ArgumentException("The specified user is not a mechanic.");

            booking.Status = BookingStatus.AssignedToMechanic;
            booking.AssignedMechanicId = dto.MechanicId;
            booking.UpdatedAt = DateTime.UtcNow;

            await _repo.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.AssignedToMechanic,
                ChangedByUserId = adminId,
                ChangedByRole = "Admin",
                Notes = dto.Notes ?? $"Assigned to mechanic: {mechanic.FirstName} {mechanic.LastName}."
            });

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Booking {Id} assigned to Mechanic {MechanicId} by Admin {AdminId}",
                id, dto.MechanicId, adminId);

            return await MapAsync(booking);
        }

        // ── Reassign Mechanic (QualityCheck → AssignedToMechanic) ────────────
        // Admin sends the job back for rework — optionally to a different mechanic.

        public async Task<AdminBookingResponseDto> ReassignMechanicAsync(Guid id, string adminId, ReassignMechanicRequestDto dto)
        {
            var booking = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.Status != BookingStatus.QualityCheck)
                throw new InvalidOperationException(
                    $"Only bookings in Quality Check can be sent back for rework. Current status: {GetStatusLabel(booking.Status)}");

            var mechanic = await _authDb.Users.FirstOrDefaultAsync(u => u.Id == dto.MechanicId)
                ?? throw new ArgumentException("Mechanic not found.");

            var isMechanic = await _authDb.UserRoles
                .Join(_authDb.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .AnyAsync(x => x.UserId == dto.MechanicId && x.Name == "Mechanic");

            if (!isMechanic)
                throw new ArgumentException("The specified user is not a mechanic.");

            booking.Status = BookingStatus.AssignedToMechanic;
            booking.AssignedMechanicId = dto.MechanicId;
            booking.UpdatedAt = DateTime.UtcNow;

            await _repo.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.AssignedToMechanic,
                ChangedByUserId = adminId,
                ChangedByRole = "Admin",
                Notes = dto.Notes ?? $"Sent back for rework. Reassigned to: {mechanic.FirstName} {mechanic.LastName}."
            });

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Booking {Id} sent back for rework, reassigned to Mechanic {MechanicId} by Admin {AdminId}",
                id, dto.MechanicId, adminId);

            return await MapAsync(booking);
        }

        // ── Update Status (Admin — QualityCheck → Completed) ─────────────────

        public async Task<AdminBookingResponseDto> UpdateStatusAsync(Guid id, string adminId, UpdateBookingStatusRequestDto dto)
        {
            var booking = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (!AdminAllowedStatuses.Contains(dto.NewStatus))
                throw new InvalidOperationException(
                    $"Admin can only set status to: {string.Join(", ", AdminAllowedStatuses.Select(GetStatusLabel))}.");

            if (booking.Status != BookingStatus.QualityCheck)
                throw new InvalidOperationException(
                    $"Can only mark as Completed from QualityCheck. Current status: {GetStatusLabel(booking.Status)}");

            booking.Status = dto.NewStatus;
            booking.UpdatedAt = DateTime.UtcNow;

            await _repo.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingId = booking.Id,
                Status = dto.NewStatus,
                ChangedByUserId = adminId,
                ChangedByRole = "Admin",
                Notes = dto.Notes ?? $"Status updated to {GetStatusLabel(dto.NewStatus)} by admin."
            });

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Booking {Id} status → {Status} by Admin {AdminId}", id, dto.NewStatus, adminId);
            return await MapAsync(booking);
        }

        // ── Get Mechanics list ────────────────────────────────────────────────

        public async Task<List<MechanicStaffDto>> GetMechanicsAsync()
        {
            var mechanicRole = await _authDb.Roles.FirstOrDefaultAsync(r => r.Name == "Mechanic");
            if (mechanicRole is null) return new();

            var mechanicIds = await _authDb.UserRoles
                .Where(ur => ur.RoleId == mechanicRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var mechanics = await _authDb.Users
                .Where(u => mechanicIds.Contains(u.Id) && u.IsActive && !u.IsDeleted)
                .ToListAsync();

            return mechanics.Select(m => new MechanicStaffDto
            {
                Id = m.Id,
                Name = $"{m.FirstName} {m.LastName}".Trim(),
                Email = m.Email ?? string.Empty
            }).ToList();
        }

        // ════════════════════════════════════════════════════════════════════
        // MECHANIC
        // ════════════════════════════════════════════════════════════════════

        public async Task<List<AdminBookingResponseDto>> GetMyJobsAsync(string mechanicId)
        {
            var bookings = await _repo.GetByMechanicAsync(mechanicId);
            return await MapListAsync(bookings);
        }

        public async Task<AdminBookingDetailResponseDto> GetMyJobByIdAsync(Guid id, string mechanicId)
        {
            var booking = await _repo.GetByIdForMechanicAsync(id, mechanicId)
                ?? throw new KeyNotFoundException("Job not found or not assigned to you.");
            return await MapDetailAsync(booking);
        }

        public async Task<AdminBookingResponseDto> UpdateJobStatusAsync(Guid id, string mechanicId, MechanicUpdateStatusRequestDto dto)
        {
            var booking = await _repo.GetByIdForMechanicAsync(id, mechanicId)
                ?? throw new KeyNotFoundException("Job not found or not assigned to you.");

            if (!MechanicAllowedStatuses.Contains(dto.NewStatus))
                throw new InvalidOperationException(
                    $"Mechanic can set status to: {string.Join(", ", MechanicAllowedStatuses.Select(GetStatusLabel))}.");

            var validTransitions = new Dictionary<BookingStatus, HashSet<BookingStatus>>
            {
                [BookingStatus.AssignedToMechanic] = new() { BookingStatus.InProgress },
                [BookingStatus.InProgress] = new() { BookingStatus.WaitingForParts, BookingStatus.QualityCheck },
                [BookingStatus.WaitingForParts] = new() { BookingStatus.InProgress },
            };

            if (!validTransitions.TryGetValue(booking.Status, out var allowed) || !allowed.Contains(dto.NewStatus))
                throw new InvalidOperationException(
                    $"Cannot move from {GetStatusLabel(booking.Status)} to {GetStatusLabel(dto.NewStatus)}.");

            booking.Status = dto.NewStatus;
            booking.UpdatedAt = DateTime.UtcNow;

            await _repo.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingId = booking.Id,
                Status = dto.NewStatus,
                ChangedByUserId = mechanicId,
                ChangedByRole = "Mechanic",
                Notes = dto.Notes ?? $"Status updated to {GetStatusLabel(dto.NewStatus)} by mechanic."
            });

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Booking {Id} status → {Status} by Mechanic {MechanicId}",
                id, dto.NewStatus, mechanicId);

            return await MapAsync(booking);
        }

        // ════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════

        private static string GetStatusLabel(BookingStatus status) => status switch
        {
            BookingStatus.Pending => "Pending",
            BookingStatus.Confirmed => "Confirmed",
            BookingStatus.AssignedToMechanic => "Assigned to Mechanic",
            BookingStatus.InProgress => "In Progress",
            BookingStatus.WaitingForParts => "Waiting for Parts",
            BookingStatus.QualityCheck => "Quality Check",
            BookingStatus.Completed => "Completed",
            BookingStatus.InvoiceGenerated => "Invoice Generated",
            BookingStatus.Paid => "Paid",
            BookingStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };

        private async Task<string> ResolveMechanicNameAsync(string? mechanicId)
        {
            if (string.IsNullOrEmpty(mechanicId)) return string.Empty;
            var mechanic = await _authDb.Users.FirstOrDefaultAsync(u => u.Id == mechanicId);
            return mechanic is null ? string.Empty : $"{mechanic.FirstName} {mechanic.LastName}".Trim();
        }

        private async Task<AdminBookingResponseDto> MapAsync(ServiceBooking b) => new()
        {
            Id = b.Id,
            CustomerId = b.CustomerId,
            CustomerName = await ResolveCustomerNameAsync(b.CustomerId),
            CustomerEmail = await ResolveCustomerEmailAsync(b.CustomerId),
            Status = b.Status,
            StatusLabel = GetStatusLabel(b.Status),
            ScheduledDate = b.ScheduledDate,
            CustomerNotes = b.CustomerNotes,
            AssignedMechanicId = b.AssignedMechanicId,
            AssignedMechanicName = await ResolveMechanicNameAsync(b.AssignedMechanicId),
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt,
            Vehicle = new VehicleResponseDto
            {
                Id = b.Vehicle.Id,
                CustomerId = b.Vehicle.CustomerId,
                Make = b.Vehicle.Make,
                Model = b.Vehicle.Model,
                Year = b.Vehicle.Year,
                LicensePlate = b.Vehicle.LicensePlate,
                FuelType = b.Vehicle.FuelType,
                CreatedAt = b.Vehicle.CreatedAt
            },
            ServiceType = new ServiceTypeResponseDto
            {
                Id = b.ServiceType.Id,
                Name = b.ServiceType.Name,
                Description = b.ServiceType.Description,
                BasePrice = b.ServiceType.BasePrice,
                EstimatedHours = b.ServiceType.EstimatedHours,
                BookedBasePrice = b.BookedBasePrice
            }
        };

        private async Task<AdminBookingDetailResponseDto> MapDetailAsync(ServiceBooking b) => new()
        {
            Id = b.Id,
            CustomerId = b.CustomerId,
            CustomerName = await ResolveCustomerNameAsync(b.CustomerId),
            CustomerEmail = await ResolveCustomerEmailAsync(b.CustomerId),
            Status = b.Status,
            StatusLabel = GetStatusLabel(b.Status),
            ScheduledDate = b.ScheduledDate,
            CustomerNotes = b.CustomerNotes,
            AssignedMechanicId = b.AssignedMechanicId,
            AssignedMechanicName = await ResolveMechanicNameAsync(b.AssignedMechanicId),
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt,
            Vehicle = new VehicleResponseDto
            {
                Id = b.Vehicle.Id,
                CustomerId = b.Vehicle.CustomerId,
                Make = b.Vehicle.Make,
                Model = b.Vehicle.Model,
                Year = b.Vehicle.Year,
                LicensePlate = b.Vehicle.LicensePlate,
                FuelType = b.Vehicle.FuelType,
                CreatedAt = b.Vehicle.CreatedAt
            },
            ServiceType = new ServiceTypeResponseDto
            {
                Id = b.ServiceType.Id,
                Name = b.ServiceType.Name,
                Description = b.ServiceType.Description,
                BasePrice = b.ServiceType.BasePrice,
                EstimatedHours = b.ServiceType.EstimatedHours,
                BookedBasePrice = b.BookedBasePrice
            },
            StatusHistory = b.StatusHistories.Select(h => new AdminStatusHistoryDto
            {
                Status = h.Status,
                StatusLabel = GetStatusLabel(h.Status),
                ChangedByRole = h.ChangedByRole,
                Notes = h.Notes,
                ChangedAt = h.ChangedAt
            }).ToList()
        };

        private async Task<List<AdminBookingResponseDto>> MapListAsync(List<ServiceBooking> bookings)
        {
            var result = new List<AdminBookingResponseDto>();
            foreach (var b in bookings)
                result.Add(await MapAsync(b));
            return result;
        }

        private async Task<string> ResolveCustomerNameAsync(string customerId)
        {
            var user = await _authDb.Users.FirstOrDefaultAsync(u => u.Id == customerId);
            return user is null ? string.Empty : $"{user.FirstName} {user.LastName}".Trim();
        }

        private async Task<string> ResolveCustomerEmailAsync(string customerId)
        {
            var user = await _authDb.Users.FirstOrDefaultAsync(u => u.Id == customerId);
            return user?.Email ?? string.Empty;
        }
    }
}