using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugHunterBugTrackerZD.Data.Migrations
{
    /// <inheritdoc />
    public partial class _013_updateCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Companies",
                newName: "CompanyName");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Companies",
                newName: "CompanyDescription");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "Companies",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CompanyDescription",
                table: "Companies",
                newName: "Description");
        }
    }
}
