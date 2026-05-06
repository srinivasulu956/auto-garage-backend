using Auto_Garage.Models.DomainModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.Data.AutoGarageAuthDb
{
    public class AutoGarageAuthDbContext(DbContextOptions<AutoGarageAuthDbContext> options) : IdentityDbContext<AutoGarageUser>(options)
    {
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
        public DbSet<BlacklistedTokenModel> BlacklistedTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                 new IdentityRole { Id = "d6a029f6-f39e-434c-a72b-171ef7d2560d", ConcurrencyStamp = "d6a029f6-f39e-434c-a72b-171ef7d2560d", Name = "Admin", NormalizedName = "ADMIN" },
                 new IdentityRole { Id = "c07538fb-40fc-4ba7-ade6-1e4450f78129", ConcurrencyStamp = "c07538fb-40fc-4ba7-ade6-1e4450f78129", Name = "Customer", NormalizedName = "CUSTOMER" },
                 new IdentityRole { Id = "09cd9546-8864-49f4-8771-51385d68edba", ConcurrencyStamp = "09cd9546-8864-49f4-8771-51385d68edba", Name = "Mechanic", NormalizedName = "MECHANIC" }
             );

            builder.Entity<RefreshTokenModel>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
