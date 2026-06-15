using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MentalLoadDistributor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seedinitialdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Families",
                columns: new[] { "Id", "Name", "Preferences" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "Test Family", "{}" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvailabilityScore", "Email", "FamilyId", "Name", "Role", "Skills" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), 70, "mom@test.com", new Guid("11111111-1111-1111-1111-111111111111"), "Mom", 0, "[]" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 90, "dad@test.com", new Guid("11111111-1111-1111-1111-111111111111"), "Dad", 1, "[]" }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "AssignedToId", "CreatedById", "Description", "DueDate", "EmotionalLoadEstimate", "EstimatedMinutes", "IsCompleted", "Metadata", "Priority", "Tags", "Title" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("33333333-3333-3333-3333-333333333333"), new Guid("22222222-2222-2222-2222-222222222222"), null, null, 30, 45, false, "{}", 1, "[]", "Buy groceries" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), null, new Guid("33333333-3333-3333-3333-333333333333"), null, null, 70, 20, false, "{}", 2, "[]", "Doctor appointment" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Families",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
