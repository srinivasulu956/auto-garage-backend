using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auto_Garage.Migrations.AutoGarageDb
{
    /// <inheritdoc />
    public partial class AddVehicleSoftDeleteAndExtras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "Vehicles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Vehicles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NickName",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Vehicles");
        }
    }
}
