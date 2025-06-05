using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPI_Dashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKpiModelsConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalRate",
                table: "VisaKPIs");

            migrationBuilder.DropColumn(
                name: "SuccessRate",
                table: "AdmissionKPIs");

            migrationBuilder.RenameColumn(
                name: "VisaApplicationsProcessed",
                table: "VisaKPIs",
                newName: "Inquiries");

            migrationBuilder.RenameColumn(
                name: "ApplicationsProcessed",
                table: "AdmissionKPIs",
                newName: "Consultations");

            migrationBuilder.AddColumn<int>(
                name: "Consultations",
                table: "VisaKPIs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Conversions",
                table: "VisaKPIs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Applications",
                table: "AdmissionKPIs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Consultations",
                table: "VisaKPIs");

            migrationBuilder.DropColumn(
                name: "Conversions",
                table: "VisaKPIs");

            migrationBuilder.DropColumn(
                name: "Applications",
                table: "AdmissionKPIs");

            migrationBuilder.RenameColumn(
                name: "Inquiries",
                table: "VisaKPIs",
                newName: "VisaApplicationsProcessed");

            migrationBuilder.RenameColumn(
                name: "Consultations",
                table: "AdmissionKPIs",
                newName: "ApplicationsProcessed");

            migrationBuilder.AddColumn<double>(
                name: "ApprovalRate",
                table: "VisaKPIs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SuccessRate",
                table: "AdmissionKPIs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
