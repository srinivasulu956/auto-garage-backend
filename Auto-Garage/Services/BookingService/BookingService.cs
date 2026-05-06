using Auto_Garage.Data.AutoGarageDb;
using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using Auto_Garage.Models.DtoModels.BookingDtos;
using Auto_Garage.Models.DtoModels.ServiceTypeDtos;
using Auto_Garage.Models.DtoModels.VehicleDtos;
using Auto_Garage.Repositories.BookingRepository;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Services.BookingService
{
    public class BookingService(
        IBookingRepository bookingRepository,
        AutoGarageDbContext db,
        ILogger<BookingService> logger) : IBookingService
    {
        private readonly IBookingRepository _bookingRepository = bookingRepository;
        private readonly AutoGarageDbContext _db = db;
        private readonly ILogger<BookingService> _logger = logger;
        public async Task<List<BookingResponseDto>> GetAllAsync(string customerId)
        {
            var bookings = await _bookingRepository.GetAllByCustomerAsync(customerId);
            return bookings.Select(ToDto).ToList();
        }
        public async Task<BookingDetailResponseDto> GetByIdAsync(Guid id, string customerId)
        {
            var booking = await _bookingRepository.GetByIdForCustomerAsync(id, customerId)
                ?? throw new KeyNotFoundException("Booking not found.");

            return ToDetailDto(booking);
        }
        public async Task<BookingResponseDto> CreateAsync(string customerId, CreateBookingRequestDto dto)
        {
            var vehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Id == dto.VehicleId && v.CustomerId == customerId)
                ?? throw new ArgumentException("Vehicle not found or does not belong to you.");

            var serviceType = await _db.ServiceTypes
                .FirstOrDefaultAsync(s => s.Id == dto.ServiceTypeId && s.IsActive)
                ?? throw new ArgumentException("Service type not found or is no longer available.");

            if (dto.ScheduledDate.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("Scheduled date cannot be in the past.");

            if (await _bookingRepository.HasActiveBookingForVehicleAsync(dto.VehicleId))
                throw new InvalidOperationException("This vehicle already has an active booking.");

            var booking = new ServiceBooking
            {
                CustomerId = customerId,
                VehicleId = dto.VehicleId,
                ServiceTypeId = dto.ServiceTypeId,
                ScheduledDate = dto.ScheduledDate,
                CustomerNotes = dto.CustomerNotes,
                Status = BookingStatus.Pending,
                BookedBasePrice = serviceType.BasePrice
            };

            await _bookingRepository.AddAsync(booking);

            await _bookingRepository.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.Pending,
                ChangedByUserId = customerId,
                ChangedByRole = "Customer",
                Notes = "Booking created by customer."
            });

            await _bookingRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Booking {BookingId} created by Customer {CustomerId} for Vehicle {VehicleId} at price {Price}",
                booking.Id, customerId, dto.VehicleId, booking.BookedBasePrice);

            booking.Vehicle = vehicle;
            booking.ServiceType = serviceType;

            return ToDto(booking);
        }

        public async Task<BookingResponseDto> UpdateAsync(Guid id, string customerId, UpdateBookingRequestDto dto)
        {
            var booking = await _bookingRepository.GetByIdForCustomerAsync(id, customerId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException("Only pending bookings can be edited.");

            var vehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Id == dto.VehicleId && v.CustomerId == customerId)
                ?? throw new ArgumentException("Invalid vehicle.");

            var serviceType = await _db.ServiceTypes
                .FirstOrDefaultAsync(s => s.Id == dto.ServiceTypeId && s.IsActive)
                ?? throw new ArgumentException("Invalid service type.");

            if (dto.ScheduledDate.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("Scheduled date cannot be in the past.");

            booking.VehicleId = dto.VehicleId;
            booking.ServiceTypeId = dto.ServiceTypeId;
            booking.ScheduledDate = dto.ScheduledDate;
            booking.CustomerNotes = dto.CustomerNotes;
            booking.UpdatedAt = DateTime.UtcNow;

            if (booking.ServiceTypeId != dto.ServiceTypeId)
                booking.BookedBasePrice = serviceType.BasePrice;

            await _bookingRepository.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingId = booking.Id,
                Status = booking.Status,
                ChangedByUserId = customerId,
                ChangedByRole = "Customer",
                Notes = "Booking updated by customer."
            });

            await _bookingRepository.SaveChangesAsync();

            _logger.LogInformation("Booking {BookingId} updated by Customer {CustomerId}", id, customerId);

            booking.Vehicle = vehicle;
            booking.ServiceType = serviceType;

            return ToDto(booking);
        }

        public async Task CancelAsync(Guid id, string customerId)
        {
            var booking = await _bookingRepository.GetByIdForCustomerAsync(id, customerId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending bookings can be cancelled. Contact us for further assistance.");

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.Cancelled,
                ChangedByUserId = customerId,
                ChangedByRole = "Customer",
                Notes = "Booking cancelled by customer."
            });

            await _bookingRepository.SaveChangesAsync();

            _logger.LogInformation("Booking {BookingId} cancelled by Customer {CustomerId}", id, customerId);
        }

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

        private static BookingResponseDto ToDto(ServiceBooking b) => new()
        {
            Id = b.Id,
            CustomerId = b.CustomerId,
            Status = b.Status,
            StatusLabel = GetStatusLabel(b.Status),
            ScheduledDate = b.ScheduledDate,
            CustomerNotes = b.CustomerNotes,
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
                BookedBasePrice = b.BookedBasePrice    // expose snapshot
            }
        };

        private static BookingDetailResponseDto ToDetailDto(ServiceBooking b) => new()
        {
            Id = b.Id,
            CustomerId = b.CustomerId,
            Status = b.Status,
            StatusLabel = GetStatusLabel(b.Status),
            ScheduledDate = b.ScheduledDate,
            CustomerNotes = b.CustomerNotes,
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
                BookedBasePrice = b.BookedBasePrice    // expose snapshot
            },
            StatusHistory = b.StatusHistories.Select(h => new StatusHistoryDto
            {
                Status = h.Status,
                StatusLabel = GetStatusLabel(h.Status),
                ChangedByRole = h.ChangedByRole,
                Notes = h.Notes,
                ChangedAt = h.ChangedAt
            }).ToList()
        };
    }
}