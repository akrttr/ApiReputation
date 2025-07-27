using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiReputation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScanResultAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScanResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApiInfoId = table.Column<int>(type: "int", nullable: false),
                    IsSslValid = table.Column<bool>(type: "bit", nullable: false),
                    SslIssuer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SslExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasHstsHeader = table.Column<bool>(type: "bit", nullable: false),
                    HasXFrameOptionsHeader = table.Column<bool>(type: "bit", nullable: false),
                    HasXContentTypeOptionsHeader = table.Column<bool>(type: "bit", nullable: false),
                    ServerInfoLeakDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OverallResult = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanResults_ApiInfos_ApiInfoId",
                        column: x => x.ApiInfoId,
                        principalTable: "ApiInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScanResults_ApiInfoId",
                table: "ScanResults",
                column: "ApiInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScanResults");
        }
    }
}
