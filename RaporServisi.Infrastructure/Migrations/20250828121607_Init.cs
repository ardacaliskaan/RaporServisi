using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaporServisi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SickReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tckn = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SicilNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiagnosisCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceSystemId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SickReports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SickReports_Tckn_StartDate_EndDate",
                table: "SickReports",
                columns: new[] { "Tckn", "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SickReports");
        }
    }
}
