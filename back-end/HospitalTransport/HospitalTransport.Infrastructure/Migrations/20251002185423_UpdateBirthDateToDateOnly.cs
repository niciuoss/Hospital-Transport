using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalTransport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBirthDateToDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("150c007b-8a58-4230-b89e-eaecebfee8a1"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Patients",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[] { new Guid("a71668e6-3753-4459-9be0-c4a10fd17ff4"), new DateTime(2025, 10, 2, 18, 54, 23, 589, DateTimeKind.Utc).AddTicks(9083), "Administrador do Sistema", true, "YWRtaW4xMjM=", "Admin", null, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a71668e6-3753-4459-9be0-c4a10fd17ff4"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Patients",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[] { new Guid("150c007b-8a58-4230-b89e-eaecebfee8a1"), new DateTime(2025, 10, 2, 13, 27, 20, 123, DateTimeKind.Utc).AddTicks(9693), "Administrador do Sistema", true, "YWRtaW4xMjM=", "Admin", null, "admin" });
        }
    }
}
