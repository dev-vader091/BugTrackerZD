using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugHunterBugTrackerZD.Data.Migrations
{
    /// <inheritdoc />
    public partial class _002update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_AspNetUsers_UserId1",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Histories_UserId1",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Histories");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Histories",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_UserId",
                table: "Histories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_AspNetUsers_UserId",
                table: "Histories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_AspNetUsers_UserId",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Histories_UserId",
                table: "Histories");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Histories",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Histories",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Histories_UserId1",
                table: "Histories",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_AspNetUsers_UserId1",
                table: "Histories",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
