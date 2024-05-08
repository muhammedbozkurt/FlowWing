using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowWing.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class deletionDateUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeletionDate",
                table: "ScheduledEmails",
                newName: "SenderDeletionDate");

            migrationBuilder.RenameColumn(
                name: "DeletionDate",
                table: "EmailLogs",
                newName: "SenderDeletionDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "RecieverDeletionDate",
                table: "ScheduledEmails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecieverDeletionDate",
                table: "EmailLogs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecieverDeletionDate",
                table: "ScheduledEmails");

            migrationBuilder.DropColumn(
                name: "RecieverDeletionDate",
                table: "EmailLogs");

            migrationBuilder.RenameColumn(
                name: "SenderDeletionDate",
                table: "ScheduledEmails",
                newName: "DeletionDate");

            migrationBuilder.RenameColumn(
                name: "SenderDeletionDate",
                table: "EmailLogs",
                newName: "DeletionDate");
        }
    }
}
