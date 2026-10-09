using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentalLoadDistributor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeApprovedPlanTaskCreationRetrySafe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_ApprovedPlanItemId",
                table: "Tasks");

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 8, 18, 45, 934, DateTimeKind.Utc).AddTicks(103));

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 8, 18, 45, 934, DateTimeKind.Utc).AddTicks(116));

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ApprovedPlanItemId",
                table: "Tasks",
                column: "ApprovedPlanItemId",
                unique: true,
                filter: "[ApprovedPlanItemId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_ApprovedPlanItemId",
                table: "Tasks");

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 7, 48, 48, 238, DateTimeKind.Utc).AddTicks(7930));

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 7, 48, 48, 238, DateTimeKind.Utc).AddTicks(7952));

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ApprovedPlanItemId",
                table: "Tasks",
                column: "ApprovedPlanItemId");
        }
    }
}
