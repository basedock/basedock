using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseDock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTypeAndSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "docker_image_config",
                table: "projects",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "project_type",
                table: "projects",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ComposeFile");

            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "projects",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_projects_slug",
                table: "projects",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_projects_slug",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "docker_image_config",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "project_type",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "slug",
                table: "projects");
        }
    }
}
