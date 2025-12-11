using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseDock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestructureProjectSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "compose_file_content",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "deployment_status",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "docker_image_config",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "last_deployed_at",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "last_deployment_error",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "project_type",
                table: "projects");

            migrationBuilder.CreateTable(
                name: "environments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_environments", x => x.id);
                    table.ForeignKey(
                        name: "FK_environments_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "docker_compose_resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    compose_file_content = table.Column<string>(type: "text", nullable: false),
                    deployment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "NotDeployed"),
                    last_deployed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_deployment_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docker_compose_resources", x => x.id);
                    table.ForeignKey(
                        name: "FK_docker_compose_resources_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "docker_image_resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    image = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: "latest"),
                    ports = table.Column<string>(type: "jsonb", nullable: true),
                    environment_variables = table.Column<string>(type: "jsonb", nullable: true),
                    volumes = table.Column<string>(type: "jsonb", nullable: true),
                    restart_policy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "unless-stopped"),
                    networks = table.Column<string>(type: "jsonb", nullable: true),
                    cpu_limit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    memory_limit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    deployment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "NotDeployed"),
                    last_deployed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_deployment_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docker_image_resources", x => x.id);
                    table.ForeignKey(
                        name: "FK_docker_image_resources_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dockerfile_resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    dockerfile_content = table.Column<string>(type: "text", nullable: false),
                    build_context = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    build_args = table.Column<string>(type: "jsonb", nullable: true),
                    ports = table.Column<string>(type: "jsonb", nullable: true),
                    environment_variables = table.Column<string>(type: "jsonb", nullable: true),
                    volumes = table.Column<string>(type: "jsonb", nullable: true),
                    restart_policy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "unless-stopped"),
                    networks = table.Column<string>(type: "jsonb", nullable: true),
                    cpu_limit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    memory_limit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    deployment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "NotDeployed"),
                    last_deployed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_deployment_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dockerfile_resources", x => x.id);
                    table.ForeignKey(
                        name: "FK_dockerfile_resources_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "environment_variables",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    is_secret = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_environment_variables", x => x.id);
                    table.ForeignKey(
                        name: "FK_environment_variables_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "postgresql_resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "16"),
                    port = table.Column<int>(type: "integer", nullable: false, defaultValue: 5432),
                    database_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "postgres"),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    deployment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "NotDeployed"),
                    last_deployed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_deployment_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_postgresql_resources", x => x.id);
                    table.ForeignKey(
                        name: "FK_postgresql_resources_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "redis_resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "7"),
                    port = table.Column<int>(type: "integer", nullable: false, defaultValue: 6379),
                    persistence_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    deployment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "NotDeployed"),
                    last_deployed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_deployment_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_redis_resources", x => x.id);
                    table.ForeignKey(
                        name: "FK_redis_resources_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_docker_compose_resources_environment_id_slug",
                table: "docker_compose_resources",
                columns: new[] { "environment_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_docker_image_resources_environment_id_slug",
                table: "docker_image_resources",
                columns: new[] { "environment_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dockerfile_resources_environment_id_slug",
                table: "dockerfile_resources",
                columns: new[] { "environment_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_environment_variables_environment_id_key",
                table: "environment_variables",
                columns: new[] { "environment_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_environments_project_id_slug",
                table: "environments",
                columns: new[] { "project_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_postgresql_resources_environment_id_slug",
                table: "postgresql_resources",
                columns: new[] { "environment_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_redis_resources_environment_id_slug",
                table: "redis_resources",
                columns: new[] { "environment_id", "slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "docker_compose_resources");

            migrationBuilder.DropTable(
                name: "docker_image_resources");

            migrationBuilder.DropTable(
                name: "dockerfile_resources");

            migrationBuilder.DropTable(
                name: "environment_variables");

            migrationBuilder.DropTable(
                name: "postgresql_resources");

            migrationBuilder.DropTable(
                name: "redis_resources");

            migrationBuilder.DropTable(
                name: "environments");

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

            migrationBuilder.AddColumn<string>(
                name: "docker_image_config",
                table: "projects",
                type: "jsonb",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "project_type",
                table: "projects",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ComposeFile");
        }
    }
}
