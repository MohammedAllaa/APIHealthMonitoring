using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIHealthMonitoring.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthCheckEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedAt",
                table: "HealthChecks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "HealthChecks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccessful",
                table: "HealthChecks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ResponseSizeBytes",
                table: "HealthChecks",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponseTimeMs",
                table: "HealthChecks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "HealthChecks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveFailures",
                table: "ApiEndpoints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStatus",
                table: "ApiEndpoints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCheckedAt",
                table: "ApiEndpoints",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthChecks_ApiEndpointId_CheckedAt",
                table: "HealthChecks",
                columns: new[] { "ApiEndpointId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HealthChecks_CheckedAt",
                table: "HealthChecks",
                column: "CheckedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HealthChecks_ApiEndpointId_CheckedAt",
                table: "HealthChecks");

            migrationBuilder.DropIndex(
                name: "IX_HealthChecks_CheckedAt",
                table: "HealthChecks");

            migrationBuilder.DropColumn(
                name: "CheckedAt",
                table: "HealthChecks");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "HealthChecks");

            migrationBuilder.DropColumn(
                name: "IsSuccessful",
                table: "HealthChecks");

            migrationBuilder.DropColumn(
                name: "ResponseSizeBytes",
                table: "HealthChecks");

            migrationBuilder.DropColumn(
                name: "ResponseTimeMs",
                table: "HealthChecks");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "HealthChecks");

            migrationBuilder.DropColumn(
                name: "ConsecutiveFailures",
                table: "ApiEndpoints");

            migrationBuilder.DropColumn(
                name: "CurrentStatus",
                table: "ApiEndpoints");

            migrationBuilder.DropColumn(
                name: "LastCheckedAt",
                table: "ApiEndpoints");
        }
    }
}
