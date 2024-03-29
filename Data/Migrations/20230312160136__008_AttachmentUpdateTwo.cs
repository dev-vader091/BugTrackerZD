﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugHunterBugTrackerZD.Data.Migrations
{
    /// <inheritdoc />
    public partial class _008_AttachmentUpdateTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Attachments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Attachments");
        }
    }
}
