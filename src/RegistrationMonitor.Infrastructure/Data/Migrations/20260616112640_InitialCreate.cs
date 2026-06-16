using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatusChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CheckedAt = table.Column<string>(type: "TEXT", nullable: false),
                    DetectedStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    NotificationSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    SourceMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusChecks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatusChecks_CheckedAt",
                table: "StatusChecks",
                column: "CheckedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatusChecks");
        }
    }
}
