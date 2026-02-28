using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HospitalTransport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments");

            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Appointments_AppointmentDate_SeatNumber"";");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3948a484-5039-4c32-84a4-3a5e40af6445"));

            // 1. Adicionar coluna BusId
            migrationBuilder.AddColumn<Guid>(
                name: "BusId",
                table: "Appointments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // 2. Criar tabela Buses
            migrationBuilder.CreateTable(
                name: "Buses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Destination = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false),
                    SeatLayout = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buses", x => x.Id);
                });

            // 3. Inserir dados em Buses
            migrationBuilder.Sql(@"
        INSERT INTO ""Buses"" (""Id"", ""Name"", ""Destination"", ""TotalSeats"", ""SeatLayout"", ""IsActive"", ""CreatedAt"")
        SELECT '11111111-1111-1111-1111-111111111111', 'Ônibus Fortaleza', 'Fortaleza', 47, 'Standard', true, CURRENT_TIMESTAMP
        WHERE NOT EXISTS (SELECT 1 FROM ""Buses"" WHERE ""Id"" = '11111111-1111-1111-1111-111111111111');

        INSERT INTO ""Buses"" (""Id"", ""Name"", ""Destination"", ""TotalSeats"", ""SeatLayout"", ""IsActive"", ""CreatedAt"")
        SELECT '22222222-2222-2222-2222-222222222222', 'Microônibus Quixeramobim', 'Quixeramobim', 31, 'Microbus', true, CURRENT_TIMESTAMP
        WHERE NOT EXISTS (SELECT 1 FROM ""Buses"" WHERE ""Id"" = '22222222-2222-2222-2222-222222222222');
    ");

            // 4. Atualizar agendamentos existentes
            migrationBuilder.Sql(@"
        UPDATE ""Appointments""
        SET ""BusId"" = '11111111-1111-1111-1111-111111111111'
        WHERE ""BusId"" = '00000000-0000-0000-0000-000000000000';
    ");

            migrationBuilder.UpdateData(
                table: "SystemControl",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastChanged",
                value: new DateTime(2025, 11, 28, 1, 27, 41, 569, DateTimeKind.Utc).AddTicks(8370));

            // 5. Inserir User
            migrationBuilder.Sql(@"
        INSERT INTO ""Users"" (""Id"", ""Username"", ""PasswordHash"", ""FullName"", ""Role"", ""IsActive"", ""CreatedAt"")
        SELECT '3c31216b-7b1b-4043-923e-dee94404d848', 'admin', 'YWRtaW4xMjM=', 'Administrador do Sistema', 'Admin', true, CURRENT_TIMESTAMP
        WHERE NOT EXISTS (SELECT 1 FROM ""Users"" WHERE ""Username"" = 'admin');
    ");

            // 6. Criar índice e Foreign Key
            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BusId",
                table: "Appointments",
                column: "BusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Buses_BusId",
                table: "Appointments",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Buses_BusId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Buses");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_BusId",
                table: "Appointments");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3c31216b-7b1b-4043-923e-dee94404d848"));

            migrationBuilder.DropColumn(
                name: "BusId",
                table: "Appointments");

            migrationBuilder.UpdateData(
                table: "SystemControl",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastChanged",
                value: new DateTime(2025, 11, 18, 0, 17, 16, 673, DateTimeKind.Utc).AddTicks(8237));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[] { new Guid("3948a484-5039-4c32-84a4-3a5e40af6445"), new DateTime(2025, 11, 18, 0, 17, 16, 673, DateTimeKind.Utc).AddTicks(8017), "Administrador do Sistema", true, "YWRtaW4xMjM=", "Admin", null, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentDate_SeatNumber",
                table: "Appointments",
                columns: new[] { "AppointmentDate", "SeatNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
