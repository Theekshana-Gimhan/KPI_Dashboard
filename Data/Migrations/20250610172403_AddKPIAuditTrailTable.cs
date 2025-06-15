using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPI_Dashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKPIAuditTrailTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdmissionKPIs_AspNetUsers_UserId",
                table: "AdmissionKPIs");

            migrationBuilder.DropForeignKey(
                name: "FK_VisaKPIs_AspNetUsers_UserId",
                table: "VisaKPIs");

            // Drop composite indexes before altering columns
            migrationBuilder.DropIndex(
                name: "IX_VisaKPIs_UserId_EntryDate",
                table: "VisaKPIs");

            migrationBuilder.DropIndex(
                name: "IX_AdmissionKPIs_UserId_EntryDate",
                table: "AdmissionKPIs");

            migrationBuilder.DropIndex(
                name: "IX_VisaKPIs_UserId",
                table: "VisaKPIs");

            migrationBuilder.DropIndex(
                name: "IX_AdmissionKPIs_UserId",
                table: "AdmissionKPIs");

            // If you want UserId to remain indexable, use nvarchar(450)
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "VisaKPIs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AdmissionKPIs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "KPITargets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Department = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetApplications = table.Column<int>(type: "int", nullable: true),
                    TargetConsultations = table.Column<int>(type: "int", nullable: true),
                    TargetInquiries = table.Column<int>(type: "int", nullable: true),
                    TargetConversions = table.Column<int>(type: "int", nullable: true),
                    SetByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPITargets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPIAuditTrails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetId = table.Column<int>(type: "int", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPIAuditTrails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPIAuditTrails_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KPIAuditTrails_KPITargets_TargetId",
                        column: x => x.TargetId,
                        principalTable: "KPITargets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KPIAuditTrails_ActionType",
                table: "KPIAuditTrails",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_KPIAuditTrails_IsSuccess",
                table: "KPIAuditTrails",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_KPIAuditTrails_TargetId",
                table: "KPIAuditTrails",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_KPIAuditTrails_Timestamp",
                table: "KPIAuditTrails",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_KPIAuditTrails_UserId",
                table: "KPIAuditTrails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_KPITargets_Department_StartDate_EndDate",
                table: "KPITargets",
                columns: new[] { "Department", "StartDate", "EndDate" },
                unique: true);

            // Recreate composite indexes after altering columns
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
            migrationBuilder.DropIndex(
                name: "IX_VisaKPIs_UserId_EntryDate",
                table: "VisaKPIs");

            migrationBuilder.DropIndex(
                name: "IX_AdmissionKPIs_UserId_EntryDate",
                table: "AdmissionKPIs");

            migrationBuilder.DropTable(
                name: "KPIAuditTrails");

            migrationBuilder.DropTable(
                name: "KPITargets");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "VisaKPIs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AdmissionKPIs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_VisaKPIs_UserId",
                table: "VisaKPIs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionKPIs_UserId",
                table: "AdmissionKPIs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdmissionKPIs_AspNetUsers_UserId",
                table: "AdmissionKPIs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisaKPIs_AspNetUsers_UserId",
                table: "VisaKPIs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Recreate composite indexes after reverting columns
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
