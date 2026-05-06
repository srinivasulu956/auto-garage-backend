using Auto_Garage.Models.DomainModels.VehicleModel;
using Auto_Garage.Models.DtoModels.VehicleDtos;
using Auto_Garage.Repositories.VehicleRepository;

namespace Auto_Garage.Services.VehicleService
{
    public class VehicleService(
        IVehicleRepository vehicleRepository,
        ILogger<VehicleService> logger) : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository = vehicleRepository;
        private readonly ILogger<VehicleService> _logger = logger;
        public async Task<List<VehicleResponseDto>> GetAllActiveAsync(string customerId)
        {
            var vehicles = await _vehicleRepository.GetActiveByCustomerAsync(customerId);
            var dtos = new List<VehicleResponseDto>();
            foreach (var v in vehicles)
                dtos.Add(await ToDtoAsync(v));
            return dtos;
        }
        public async Task<List<VehicleResponseDto>> GetAllInactiveAsync(string customerId)
        {
            var vehicles = await _vehicleRepository.GetInactiveByCustomerAsync(customerId);
            var dtos = new List<VehicleResponseDto>();
            foreach (var v in vehicles)
                dtos.Add(await ToDtoAsync(v));
            return dtos;
        }

        public async Task<VehicleResponseDto> GetByIdAsync(Guid id, string customerId)
        {
            var vehicle = await _vehicleRepository.GetByIdForCustomerAsync(id, customerId)
                ?? throw new KeyNotFoundException("Vehicle not found.");
            return await ToDtoAsync(vehicle);
        }

        public async Task<VehicleResponseDto> CreateAsync(string customerId, AddVehicleRequestDto dto)
        {
            if (await _vehicleRepository.PlateExistsForCustomerAsync(dto.LicensePlate, customerId))
                throw new InvalidOperationException(
                    "A vehicle with this license plate already exists (active or inactive).");

            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                LicensePlate = dto.LicensePlate.ToUpper(),
                VIN = dto.VIN,
                FuelType = dto.FuelType,
                Nickname = dto.Nickname,
                Notes = dto.Notes,
                IsActive = true
            };

            await _vehicleRepository.AddAsync(vehicle);
            await _vehicleRepository.SaveChangesAsync();

            _logger.LogInformation("Vehicle {VehicleId} added by Customer {CustomerId}", vehicle.Id, customerId);

            return await ToDtoAsync(vehicle);
        }

        public async Task<VehicleResponseDto> UpdateAsync(Guid id, string customerId, UpdateVehicleRequestDto dto)
        {
            var vehicle = await _vehicleRepository.GetByIdForCustomerAsync(id, customerId)
                ?? throw new KeyNotFoundException("Vehicle not found.");

            var hasHistory = await _vehicleRepository.HasAnyBookingAsync(id);

            // Always-editable fields
            if (dto.Nickname != null) vehicle.Nickname = dto.Nickname;
            if (dto.Notes != null) vehicle.Notes = dto.Notes;
            if (dto.IsActive.HasValue) vehicle.IsActive = dto.IsActive.Value;

            // Core fields — blocked when booking history exists
            if (hasHistory)
            {
                if (dto.Make != null || dto.Model != null || dto.Year != null ||
                    dto.LicensePlate != null || dto.VIN != null || dto.FuelType != null)
                    throw new InvalidOperationException(
                        "Cannot modify vehicle core details after booking history exists. " +
                        "Only nickname, notes, and active status can be updated.");
            }
            else
            {
                if (dto.LicensePlate != null)
                {
                    if (await _vehicleRepository.PlateExistsForCustomerAsync(dto.LicensePlate, customerId, id))
                        throw new InvalidOperationException(
                            "Another vehicle with this license plate already exists.");

                    vehicle.LicensePlate = dto.LicensePlate.ToUpper();
                }

                if (dto.Make != null) vehicle.Make = dto.Make;
                if (dto.Model != null) vehicle.Model = dto.Model;
                if (dto.Year.HasValue) vehicle.Year = dto.Year.Value;
                if (dto.VIN != null) vehicle.VIN = dto.VIN;
                if (dto.FuelType != null) vehicle.FuelType = dto.FuelType;
            }

            await _vehicleRepository.SaveChangesAsync();

            _logger.LogInformation("Vehicle {VehicleId} updated by Customer {CustomerId}", id, customerId);

            return await ToDtoAsync(vehicle);
        }

        public async Task<VehicleResponseDto> ReactivateAsync(Guid id, string customerId)
        {
            var vehicle = await _vehicleRepository.GetByIdForCustomerAsync(id, customerId)
                ?? throw new KeyNotFoundException("Vehicle not found.");

            if (vehicle.IsActive)
                throw new InvalidOperationException("Vehicle is already active.");

            vehicle.IsActive = true;
            await _vehicleRepository.SaveChangesAsync();

            _logger.LogInformation("Vehicle {VehicleId} reactivated by Customer {CustomerId}", id, customerId);

            return await ToDtoAsync(vehicle);
        }

        public async Task DeleteAsync(Guid id, string customerId)
        {
            var vehicle = await _vehicleRepository.GetByIdForCustomerAsync(id, customerId)
                ?? throw new KeyNotFoundException("Vehicle not found.");

            if (await _vehicleRepository.HasActiveBookingAsync(id))
                throw new InvalidOperationException("Cannot remove vehicle with active bookings.");

            vehicle.IsActive = false;
            await _vehicleRepository.SaveChangesAsync();

            _logger.LogInformation("Vehicle {VehicleId} soft-deleted by Customer {CustomerId}", id, customerId);
        }

        private async Task<VehicleResponseDto> ToDtoAsync(Vehicle v) => new()
        {
            Id = v.Id,
            CustomerId = v.CustomerId,
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            LicensePlate = v.LicensePlate,
            VIN = v.VIN,
            FuelType = v.FuelType,
            Nickname = v.Nickname,
            Notes = v.Notes,
            IsActive = v.IsActive,
            HasBookingHistory = await _vehicleRepository.HasAnyBookingAsync(v.Id),
            CreatedAt = v.CreatedAt
        };

        public async Task<IEnumerable<VehicleResponseDto>> GetAllByCustomerIdAsync(string customerId)
        {
            var vehicles = await _vehicleRepository.GetByCustomerIdAsync(customerId);

            return vehicles.Select(v => new VehicleResponseDto
            {
                Id = v.Id,
                Make = v.Make,
                Model = v.Model,
                LicensePlate = v.LicensePlate,
                IsActive = v.IsActive
            });
        }
    }
}