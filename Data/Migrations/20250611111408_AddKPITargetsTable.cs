using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPI_Dashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKPITargetsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop indexes that depend on UserId before altering the column
            migrationBuilder.DropIndex(
                name: "IX_AdmissionKPIs_UserId",
                table: "AdmissionKPIs");

            migrationBuilder.DropIndex(
                name: "IX_VisaKPIs_UserId",
                table: "VisaKPIs");

            // Alter UserId columns to nvarchar(450)
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AdmissionKPIs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "VisaKPIs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Recreate indexes after altering the columns
            migrationBuilder.CreateIndex(
                name: "IX_AdmissionKPIs_UserId",
                table: "AdmissionKPIs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VisaKPIs_UserId",
                table: "VisaKPIs",
                column: "UserId");

            // Create the KPITargets table
            migrationBuilder.CreateTable(
                name: "KPITargets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KPIName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPITargets", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KPITargets");

            // Drop indexes before reverting the columns
            migrationBuilder.DropIndex(
                name: "IX_AdmissionKPIs_UserId",
                table: "AdmissionKPIs");

            migrationBuilder.DropIndex(
                name: "IX_VisaKPIs_UserId",
                table: "VisaKPIs");

            // Revert UserId columns to nvarchar(max)
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AdmissionKPIs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "VisaKPIs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            // Recreate indexes after reverting the columns
            migrationBuilder.CreateIndex(
                name: "IX_AdmissionKPIs_UserId",
                table: "AdmissionKPIs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VisaKPIs_UserId",
                table: "VisaKPIs",
                column: "UserId");
        }
    }
}