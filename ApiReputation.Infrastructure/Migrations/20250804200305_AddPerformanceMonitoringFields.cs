using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiReputation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceMonitoringFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HttpStatusCode",
                table: "ScanResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccessStatusCode",
                table: "ScanResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ResponseTimeMs",
                table: "ScanResults",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HttpStatusCode",
                table: "ScanResults");

            migrationBuilder.DropColumn(
                name: "IsSuccessStatusCode",
                table: "ScanResults");

            migrationBuilder.DropColumn(
                name: "ResponseTimeMs",
                table: "ScanResults");
        }
    }
}
