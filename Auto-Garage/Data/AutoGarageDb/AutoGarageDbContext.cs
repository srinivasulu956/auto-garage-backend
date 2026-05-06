using Auto_Garage.Models.DomainModels.InvoiceModel;
using Auto_Garage.Models.DomainModels.JobWorkLogModel;
using Auto_Garage.Models.DomainModels.ServiceBookingModel;
using Auto_Garage.Models.DomainModels.ServiceTypeModel;
using Auto_Garage.Models.DomainModels.VehicleModel;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Data.AutoGarageDb
{
    public class AutoGarageDbContext(DbContextOptions<AutoGarageDbContext> options) : DbContext(options)
    {
        // DbSets 
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<ServiceBooking> ServiceBookings { get; set; }
        public DbSet<BookingStatusHistory> BookingStatusHistories { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<JobWorkLog> JobWorkLogs { get; set; } 

        // Relationships

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Vehicle
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasIndex(v => v.LicensePlate).IsUnique();
                entity.Property(v => v.FuelType).HasDefaultValue("Petrol");
            });

            // ServiceBooking → Vehicle
            modelBuilder.Entity<ServiceBooking>()
                .HasOne(b => b.Vehicle)
                .WithMany(v => v.ServiceBookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceBooking → ServiceType
            modelBuilder.Entity<ServiceBooking>()
                .HasOne(b => b.ServiceType)
                .WithMany(s => s.ServiceBookings)
                .HasForeignKey(b => b.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // BookingStatusHistory → ServiceBooking
            modelBuilder.Entity<BookingStatusHistory>()
                .HasOne(h => h.ServiceBooking)
                .WithMany(b => b.StatusHistories)
                .HasForeignKey(h => h.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Invoice → ServiceBooking (one-to-one)
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.ServiceBooking)
                .WithOne()
                .HasForeignKey<Invoice>(i => i.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.BookingId)
                .IsUnique();

            // Decimal precision for Invoice
            modelBuilder.Entity<Invoice>(e =>
            {
                e.Property(i => i.SubTotal).HasColumnType("decimal(10,2)");
                e.Property(i => i.TaxRate).HasColumnType("decimal(5,2)");
                e.Property(i => i.TaxAmount).HasColumnType("decimal(10,2)");
                e.Property(i => i.TotalAmount).HasColumnType("decimal(10,2)");
            });

            // InvoiceItem → Invoice
            modelBuilder.Entity<InvoiceItem>()
                .HasOne(item => item.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(item => item.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceItem>(e =>
            {
                e.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
                e.Ignore(i => i.Total);
            });

            // JobWorkLog
            // No navigation property on ServiceBooking — kept simple intentionally.
            // BookingId is a plain FK, no cascade needed (work log is independent).
            modelBuilder.Entity<JobWorkLog>(e =>
            {
                e.Property(w => w.UnitCost).HasColumnType("decimal(10,2)");
                e.HasIndex(w => w.BookingId);  // for fast lookup by booking
            });

            // Seed Data - Service Types
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<ServiceType>().HasData(
               new ServiceType { Id = Guid.Parse("11111111-0000-0000-0000-000000000001"), Name = "Oil Change", Description = "Full engine oil drain and refill with filter replacement.", BasePrice = 799, EstimatedHours = 1, IsActive = true, CreatedAt = seedDate },
               new ServiceType { Id = Guid.Parse("11111111-0000-0000-0000-000000000002"), Name = "Tyre Rotation", Description = "Rotate tyres for even wear and extended tyre life.", BasePrice = 499, EstimatedHours = 0.5, IsActive = true, CreatedAt = seedDate },
               new ServiceType { Id = Guid.Parse("11111111-0000-0000-0000-000000000003"), Name = "Brake Service", Description = "Inspect and replace brake pads, check rotors and brake fluid.", BasePrice = 1999, EstimatedHours = 2, IsActive = true, CreatedAt = seedDate },
               new ServiceType { Id = Guid.Parse("11111111-0000-0000-0000-000000000004"), Name = "Battery Check & Replacement", Description = "Battery health check and replacement if required.", BasePrice = 299, EstimatedHours = 0.5, IsActive = true, CreatedAt = seedDate },
               new ServiceType { Id = Guid.Parse("11111111-0000-0000-0000-000000000005"), Name = "AC Service", Description = "AC gas refill, filter cleaning, and cooling system check.", BasePrice = 1499, EstimatedHours = 1.5, IsActive = true, CreatedAt = seedDate },
               new ServiceType { Id = Guid.Parse("11111111-0000-0000-0000-000000000006"), Name = "Full Service", Description = "Comprehensive vehicle service including oil, filters, tyres, brakes, and inspection.", BasePrice = 3999, EstimatedHours = 4, IsActive = true, CreatedAt = seedDate }
           );
        }
    }
}