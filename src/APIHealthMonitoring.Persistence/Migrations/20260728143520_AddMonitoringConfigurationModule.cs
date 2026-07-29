using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIHealthMonitoring.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMonitoringConfigurationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_MonitoringConfigurations_ApiEndpointId",
                table: "MonitoringConfigurations",
                newName: "IX_MonitoringConfigurations_ApiEndpointId_Unique");

            migrationBuilder.AddColumn<decimal>(
                name: "AvailabilityThreshold",
                table: "MonitoringConfigurations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 99.0m);

            migrationBuilder.AddColumn<int>(
                name: "CriticalThresholdMs",
                table: "MonitoringConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 2000);

            migrationBuilder.AddColumn<int>(
                name: "FailureCountLimit",
                table: "MonitoringConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "SlowThresholdMs",
                table: "MonitoringConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 1000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityThreshold",
                table: "MonitoringConfigurations");

            migrationBuilder.DropColumn(
                name: "CriticalThresholdMs",
                table: "MonitoringConfigurations");

            migrationBuilder.DropColumn(
                name: "FailureCountLimit",
                table: "MonitoringConfigurations");

            migrationBuilder.DropColumn(
                name: "SlowThresholdMs",
                table: "MonitoringConfigurations");

            migrationBuilder.RenameIndex(
                name: "IX_MonitoringConfigurations_ApiEndpointId_Unique",
                table: "MonitoringConfigurations",
                newName: "IX_MonitoringConfigurations_ApiEndpointId");
        }
    }
}
