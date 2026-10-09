using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentConcurrencyIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_Appointments_Doctor_Scheduled_Date",
                table: "Appointments",
                columns: new[] { "DoctorId", "AppointmentDate" },
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Appointments_Patient_Scheduled_Date",
                table: "Appointments",
                columns: new[] { "PatientId", "AppointmentDate" },
                unique: true,
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Appointments_Doctor_Scheduled_Date",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "UX_Appointments_Patient_Scheduled_Date",
                table: "Appointments");
        }
    }
}
