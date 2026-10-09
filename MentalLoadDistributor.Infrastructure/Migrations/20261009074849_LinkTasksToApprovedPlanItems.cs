using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentalLoadDistributor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkTasksToApprovedPlanItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedPlanItemId",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "ApprovedPlanItemId", "CreatedAt" },
                values: new object[] { null, new DateTime(2026, 10, 9, 7, 48, 48, 238, DateTimeKind.Utc).AddTicks(7930) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "ApprovedPlanItemId", "CreatedAt" },
                values: new object[] { null, new DateTime(2026, 10, 9, 7, 48, 48, 238, DateTimeKind.Utc).AddTicks(7952) });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ApprovedPlanItemId",
                table: "Tasks",
                column: "ApprovedPlanItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_ApprovedPlanItems_ApprovedPlanItemId",
                table: "Tasks",
                column: "ApprovedPlanItemId",
                principalTable: "ApprovedPlanItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_ApprovedPlanItems_ApprovedPlanItemId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ApprovedPlanItemId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ApprovedPlanItemId",
                table: "Tasks");

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 8, 12, 15, 45, 183, DateTimeKind.Utc).AddTicks(9940));

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 8, 12, 15, 45, 183, DateTimeKind.Utc).AddTicks(9975));
        }
    }
}
