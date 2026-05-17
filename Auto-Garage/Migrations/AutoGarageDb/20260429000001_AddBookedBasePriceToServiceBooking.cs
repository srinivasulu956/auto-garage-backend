using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auto_Garage.Migrations.AutoGarageDb
{
    public partial class AddBookedBasePriceToServiceBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BookedBasePrice",
                table: "ServiceBookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookedBasePrice",
                table: "ServiceBookings");
        }
    }
}