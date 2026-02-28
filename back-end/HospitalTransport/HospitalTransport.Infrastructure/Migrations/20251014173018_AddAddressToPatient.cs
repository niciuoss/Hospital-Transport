using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalTransport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("8894d286-47a6-4ee2-a32a-65dabc2bc4cc"));

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Patients",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "Endereço não informado");

            migrationBuilder.UpdateData(
                table: "SystemControl",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastChanged",
                value: new DateTime(2025, 10, 14, 17, 30, 17, 420, DateTimeKind.Utc).AddTicks(2375));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[] { new Guid("0c2001b5-4f87-41da-a437-980a31ce951e"), new DateTime(2025, 10, 14, 17, 30, 17, 420, DateTimeKind.Utc).AddTicks(2155), "Administrador do Sistema", true, "YWRtaW4xMjM=", "Admin", null, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0c2001b5-4f87-41da-a437-980a31ce951e"));

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Patients");

            migrationBuilder.UpdateData(
                table: "SystemControl",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastChanged",
                value: new DateTime(2025, 10, 7, 11, 33, 22, 573, DateTimeKind.Utc).AddTicks(551));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[] { new Guid("8894d286-47a6-4ee2-a32a-65dabc2bc4cc"), new DateTime(2025, 10, 7, 11, 33, 22, 573, DateTimeKind.Utc).AddTicks(482), "Administrador do Sistema", true, "YWRtaW4xMjM=", "Admin", null, "admin" });
        }
    }
}
