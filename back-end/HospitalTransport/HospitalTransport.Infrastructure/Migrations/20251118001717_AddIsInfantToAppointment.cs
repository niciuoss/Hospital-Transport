using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalTransport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsInfantToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_Patients_CPF"";
                DROP INDEX IF EXISTS ""IX_Patients_SusCardNumber"";
            ");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a9fee50d-b848-4164-8e10-428d5321cc3e"));

            migrationBuilder.AddColumn<bool>(
                name: "IsInfant",
                table: "Appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "SystemControl",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastChanged",
                value: new DateTime(2025, 11, 18, 0, 17, 16, 673, DateTimeKind.Utc).AddTicks(8237));

            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""Id"", ""Username"", ""PasswordHash"", ""FullName"", ""Role"", ""IsActive"", ""CreatedAt"")
                SELECT '3948a484-5039-4c32-84a4-3a5e40af6445', 'admin', 'YWRtaW4xMjM=', 'Administrador do Sistema', 0, true, CURRENT_TIMESTAMP
                WHERE NOT EXISTS (SELECT 1 FROM ""Users"" WHERE ""Username"" = 'admin');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3948a484-5039-4c32-84a4-3a5e40af6445"));

            migrationBuilder.DropColumn(
                name: "IsInfant",
                table: "Appointments");

            migrationBuilder.UpdateData(
                table: "SystemControl",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastChanged",
                value: new DateTime(2025, 10, 16, 12, 34, 32, 803, DateTimeKind.Utc).AddTicks(7966));

            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""Id"", ""Username"", ""PasswordHash"", ""FullName"", ""Role"", ""IsActive"", ""CreatedAt"")
                SELECT '3948a484-5039-4c32-84a4-3a5e40af6445', 'admin', 'YWRtaW4xMjM=', 'Administrador do Sistema', 0, true, CURRENT_TIMESTAMP
                WHERE NOT EXISTS (SELECT 1 FROM ""Users"" WHERE ""Username"" = 'admin');
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_CPF",
                table: "Patients",
                column: "CPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_SusCardNumber",
                table: "Patients",
                column: "SusCardNumber",
                unique: true);
        }
    }
}
