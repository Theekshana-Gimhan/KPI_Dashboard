using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPI_Dashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKpiTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdmissionKPIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationsProcessed = table.Column<int>(type: "int", nullable: false),
                    SuccessRate = table.Column<double>(type: "float", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdmissionKPIs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdmissionKPIs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisaKPIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VisaApplicationsProcessed = table.Column<int>(type: "int", nullable: false),
                    ApprovalRate = table.Column<double>(type: "float", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaKPIs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisaKPIs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionKPIs_UserId",
                table: "AdmissionKPIs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VisaKPIs_UserId",
                table: "VisaKPIs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdmissionKPIs");

            migrationBuilder.DropTable(
                name: "VisaKPIs");
        }
    }
}
