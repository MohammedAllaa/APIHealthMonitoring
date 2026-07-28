using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIHealthMonitoring.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApiEndpointRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiEndpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HealthEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HttpMethod = table.Column<int>(type: "int", nullable: false),
                    ExpectedStatusCode = table.Column<int>(type: "int", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    IntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    ServiceOwner = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Environment = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiEndpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiEndpointId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_ApiEndpoints_ApiEndpointId",
                        column: x => x.ApiEndpointId,
                        principalTable: "ApiEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiEndpointId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthChecks_ApiEndpoints_ApiEndpointId",
                        column: x => x.ApiEndpointId,
                        principalTable: "ApiEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiEndpointId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoringConfigurations_ApiEndpoints_ApiEndpointId",
                        column: x => x.ApiEndpointId,
                        principalTable: "ApiEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ApiEndpointId",
                table: "Alerts",
                column: "ApiEndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiEndpoints_Environment",
                table: "ApiEndpoints",
                column: "Environment");

            migrationBuilder.CreateIndex(
                name: "IX_ApiEndpoints_IsActive",
                table: "ApiEndpoints",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ApiEndpoints_Name_Unique",
                table: "ApiEndpoints",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiEndpoints_ServiceOwner",
                table: "ApiEndpoints",
                column: "ServiceOwner");

            migrationBuilder.CreateIndex(
                name: "IX_HealthChecks_ApiEndpointId",
                table: "HealthChecks",
                column: "ApiEndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringConfigurations_ApiEndpointId",
                table: "MonitoringConfigurations",
                column: "ApiEndpointId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "HealthChecks");

            migrationBuilder.DropTable(
                name: "MonitoringConfigurations");

            migrationBuilder.DropTable(
                name: "ApiEndpoints");
        }
    }
}
