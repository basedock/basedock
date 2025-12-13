using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseDock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_projects_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "environments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    compose_file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
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
                name: "project_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_members_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    external = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    external_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configs", x => x.id);
                    table.ForeignKey(
                        name: "FK_configs_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "networks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    driver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    driver_opts = table.Column<string>(type: "jsonb", nullable: true),
                    ipam_driver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ipam_config = table.Column<string>(type: "jsonb", nullable: true),
                    @internal = table.Column<bool>(name: "internal", type: "boolean", nullable: false, defaultValue: false),
                    attachable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    labels = table.Column<string>(type: "jsonb", nullable: true),
                    external = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    external_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_networks", x => x.id);
                    table.ForeignKey(
                        name: "FK_networks_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "secrets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    external = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    external_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secrets", x => x.id);
                    table.ForeignKey(
                        name: "FK_secrets_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    build_context = table.Column<string>(type: "text", nullable: true),
                    build_dockerfile = table.Column<string>(type: "text", nullable: true),
                    build_args = table.Column<string>(type: "jsonb", nullable: true),
                    command = table.Column<string[]>(type: "text[]", nullable: true),
                    entrypoint = table.Column<string[]>(type: "text[]", nullable: true),
                    working_dir = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    user = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ports = table.Column<string>(type: "jsonb", nullable: true),
                    expose = table.Column<int[]>(type: "integer[]", nullable: true),
                    hostname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    domainname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    dns = table.Column<string[]>(type: "text[]", nullable: true),
                    extra_hosts = table.Column<string>(type: "jsonb", nullable: true),
                    environment_variables = table.Column<string>(type: "jsonb", nullable: true),
                    env_file = table.Column<string[]>(type: "text[]", nullable: true),
                    volumes = table.Column<string>(type: "jsonb", nullable: true),
                    tmpfs = table.Column<string[]>(type: "text[]", nullable: true),
                    depends_on = table.Column<string>(type: "jsonb", nullable: true),
                    links = table.Column<string[]>(type: "text[]", nullable: true),
                    healthcheck_test = table.Column<string[]>(type: "text[]", nullable: true),
                    healthcheck_interval = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    healthcheck_timeout = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    healthcheck_retries = table.Column<int>(type: "integer", nullable: true),
                    healthcheck_start_period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    healthcheck_disabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    cpu_limit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    memory_limit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cpu_reservation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    memory_reservation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    restart = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    stop_grace_period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    stop_signal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    replicas = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    labels = table.Column<string>(type: "jsonb", nullable: true),
                    deployment_status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_deployed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.id);
                    table.ForeignKey(
                        name: "FK_services_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "volumes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    environment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    driver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    driver_opts = table.Column<string>(type: "jsonb", nullable: true),
                    labels = table.Column<string>(type: "jsonb", nullable: true),
                    external = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    external_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_volumes", x => x.id);
                    table.ForeignKey(
                        name: "FK_volumes_environments_environment_id",
                        column: x => x.environment_id,
                        principalTable: "environments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_configs",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    uid = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    gid = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    mode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_configs", x => new { x.service_id, x.config_id });
                    table.ForeignKey(
                        name: "FK_service_configs_configs_config_id",
                        column: x => x.config_id,
                        principalTable: "configs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_service_configs_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_networks",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    network_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aliases = table.Column<string[]>(type: "text[]", nullable: true),
                    ipv4_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ipv6_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_networks", x => new { x.service_id, x.network_id });
                    table.ForeignKey(
                        name: "FK_service_networks_networks_network_id",
                        column: x => x.network_id,
                        principalTable: "networks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_service_networks_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_secrets",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    secret_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    uid = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    gid = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    mode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_secrets", x => new { x.service_id, x.secret_id });
                    table.ForeignKey(
                        name: "FK_service_secrets_secrets_secret_id",
                        column: x => x.secret_id,
                        principalTable: "secrets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_service_secrets_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_configs_environment_id_name",
                table: "configs",
                columns: new[] { "environment_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_environments_project_id_slug",
                table: "environments",
                columns: new[] { "project_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_networks_environment_id_name",
                table: "networks",
                columns: new[] { "environment_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_members_project_id_user_id",
                table: "project_members",
                columns: new[] { "project_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_members_user_id",
                table: "project_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_created_by_user_id",
                table: "projects",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_name",
                table: "projects",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_slug",
                table: "projects",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_secrets_environment_id_name",
                table: "secrets",
                columns: new[] { "environment_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_configs_config_id",
                table: "service_configs",
                column: "config_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_networks_network_id",
                table: "service_networks",
                column: "network_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_secrets_secret_id",
                table: "service_secrets",
                column: "secret_id");

            migrationBuilder.CreateIndex(
                name: "IX_services_environment_id_slug",
                table: "services",
                columns: new[] { "environment_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_expires_at",
                table: "sessions",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_user_id",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_volumes_environment_id_name",
                table: "volumes",
                columns: new[] { "environment_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_members");

            migrationBuilder.DropTable(
                name: "service_configs");

            migrationBuilder.DropTable(
                name: "service_networks");

            migrationBuilder.DropTable(
                name: "service_secrets");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "volumes");

            migrationBuilder.DropTable(
                name: "configs");

            migrationBuilder.DropTable(
                name: "networks");

            migrationBuilder.DropTable(
                name: "secrets");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "environments");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
