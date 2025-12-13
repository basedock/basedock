using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseDock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComposeGeneratorSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "compose_file_path",
                table: "environments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "premade_app_resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    template_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    configuration = table.Column<string>(type: "jsonb", nullable: true),
                    service_slugs = table.Column<string>(type: "jsonb", nullable: true),
                    deployment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "NotDeployed"),
                    last_deployed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_deployment_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_premade_app_resources", x => x.id);
                    table.ForeignKey(
                        name: "FK_premade_app_resources_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_premade_app_resources_environment_id_slug",
                table: "premade_app_resources",
                columns: new[] { "environment_id", "slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "premade_app_resources");

            migrationBuilder.DropColumn(
                name: "compose_file_path",
                table: "environments");
        }
    }
}
