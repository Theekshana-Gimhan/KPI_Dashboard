using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPI_Dashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKPIAuditTrailRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop indexes before altering columns
            migrationBuilder.DropIndex(
                name: "IX_VisaKPIs_UserId_EntryDate",
                table: "VisaKPIs");

            migrationBuilder.DropIndex(
                name: "IX_AdmissionKPIs_UserId_EntryDate",
                table: "AdmissionKPIs");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "VisaKPIs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "KPIAuditTrails",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AdmissionKPIs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Recreate indexes after altering columns
            migrationBuilder.CreateIndex(
                name: "IX_VisaKPIs_UserId_EntryDate",
                table: "VisaKPIs",
                columns: new[] { "UserId", "EntryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionKPIs_UserId_EntryDate",
                table: "AdmissionKPIs",
                columns: new[] { "UserId", "EntryDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "KPIAuditTrails");

            // Drop indexes before reverting columns
            migrationBuilder.DropIndex(
                name: "IX_VisaKPIs_UserId_EntryDate",
                table: "VisaKPIs");

            migrationBuilder.DropIndex(
                name: "IX_AdmissionKPIs_UserId_EntryDate",
                table: "AdmissionKPIs");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "VisaKPIs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AdmissionKPIs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            // Recreate indexes after reverting columns
            migrationBuilder.CreateIndex(
                name: "IX_VisaKPIs_UserId_EntryDate",
                table: "VisaKPIs",
                columns: new[] { "UserId", "EntryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionKPIs_UserId_EntryDate",
                table: "AdmissionKPIs",
                columns: new[] { "UserId", "EntryDate" });
        }
    }
}
