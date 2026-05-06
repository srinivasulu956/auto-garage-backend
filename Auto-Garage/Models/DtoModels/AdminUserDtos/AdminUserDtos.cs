namespace Auto_Garage.Models.DtoModels.AdminUserDtos
{
    public class AdminCustomerDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VehicleCount { get; set; }
        public int BookingCount { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class AdminCustomerDetailDto : AdminCustomerDto
    {
        public List<AdminCustomerVehicleDto> Vehicles { get; set; } = new();
        public List<AdminCustomerBookingDto> Bookings { get; set; } = new();
        public List<AdminCustomerInvoiceDto> Invoices { get; set; } = new();
    }

    public class AdminCustomerVehicleDto
    {
        public Guid Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminCustomerBookingDto
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminCustomerInvoiceDto
    {
        public Guid Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class AdminStaffDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ActiveJobCount { get; set; }
    }

}