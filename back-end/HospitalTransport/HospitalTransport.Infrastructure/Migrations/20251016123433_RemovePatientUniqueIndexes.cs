using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalTransport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePatientUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_Patients_CPF"";
                DROP INDEX IF EXISTS ""IX_Patients_SusCardNumber"";
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
