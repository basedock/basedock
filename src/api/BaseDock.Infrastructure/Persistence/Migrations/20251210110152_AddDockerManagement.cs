using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseDock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDockerManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "compose_file_content",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deployment_status",
                table: "projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "NotDeployed");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_deployed_at",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_deployment_error",
                table: "projects",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "compose_file_content",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "deployment_status",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "last_deployed_at",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "last_deployment_error",
                table: "projects");
        }
    }
}
