using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentalLoadDistributor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskStatusAndCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "EmotionalLoadEstimate",
                table: "Tasks",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "ActivityDate",
                table: "ActivityLogs",
                newName: "OccurredAt");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tasks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MentalLoadEstimate",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "Category", "CompletedAt", "CreatedAt", "MentalLoadEstimate", "Status", "UserId" },
                values: new object[] { 9, null, new DateTime(2026, 7, 20, 13, 11, 52, 235, DateTimeKind.Utc).AddTicks(7218), 30, 0, null });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "Category", "CompletedAt", "CreatedAt", "MentalLoadEstimate", "Status", "UserId" },
                values: new object[] { 9, null, new DateTime(2026, 7, 20, 13, 11, 52, 235, DateTimeKind.Utc).AddTicks(7229), 70, 0, null });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId",
                table: "Tasks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_UserId",
                table: "Tasks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_UserId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "MentalLoadEstimate",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Tasks",
                newName: "EmotionalLoadEstimate");

            migrationBuilder.RenameColumn(
                name: "OccurredAt",
                table: "ActivityLogs",
                newName: "ActivityDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "EmotionalLoadEstimate", "IsCompleted" },
                values: new object[] { 30, false });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "EmotionalLoadEstimate", "IsCompleted" },
                values: new object[] { 70, false });
        }
    }
}
