namespace BaseDock.Application.Features.Templates;

using BaseDock.Application.Features.Templates.DTOs;

public static class TemplateDefinitions
{
    public static IReadOnlyList<TemplateDefinition> All { get; } = new List<TemplateDefinition>
    {
        new TemplateDefinition
        {
            Id = "postgresql",
            Name = "PostgreSQL",
            Description = "PostgreSQL database with health check and persistent storage",
            Category = "Database",
            Icon = "database",
            Parameters = new List<TemplateParameterDto>
            {
                new("Database Name", "POSTGRES_DB", "string", "app", true, "Name of the default database"),
                new("Username", "POSTGRES_USER", "string", "postgres", true, "Database superuser name"),
                new("Password", "POSTGRES_PASSWORD", "password", null, true, "Database superuser password"),
                new("Port", "PORT", "number", "5432", false, "Host port to expose")
            },
            ServiceTemplates = new List<ServiceTemplate>
            {
                new ServiceTemplate
                {
                    Name = "postgres",
                    Image = "postgres:16-alpine",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        ["POSTGRES_DB"] = "${POSTGRES_DB}",
                        ["POSTGRES_USER"] = "${POSTGRES_USER}",
                        ["POSTGRES_PASSWORD"] = "${POSTGRES_PASSWORD}"
                    },
                    Ports = "[{\"host\":${PORT},\"container\":5432,\"protocol\":\"tcp\"}]",
                    Volumes = "[{\"source\":\"postgres_data\",\"target\":\"/var/lib/postgresql/data\",\"type\":\"volume\"}]",
                    HealthcheckTest = new[] { "CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}" },
                    HealthcheckInterval = "10s",
                    HealthcheckTimeout = "5s",
                    HealthcheckRetries = 5,
                    HealthcheckStartPeriod = "30s",
                    Restart = "unless-stopped"
                }
            },
            Volumes = new List<string> { "postgres_data" }
        },
        new TemplateDefinition
        {
            Id = "mysql",
            Name = "MySQL",
            Description = "MySQL database with health check and persistent storage",
            Category = "Database",
            Icon = "database",
            Parameters = new List<TemplateParameterDto>
            {
                new("Database Name", "MYSQL_DATABASE", "string", "app", true, "Name of the default database"),
                new("Root Password", "MYSQL_ROOT_PASSWORD", "password", null, true, "MySQL root password"),
                new("Port", "PORT", "number", "3306", false, "Host port to expose")
            },
            ServiceTemplates = new List<ServiceTemplate>
            {
                new ServiceTemplate
                {
                    Name = "mysql",
                    Image = "mysql:8",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        ["MYSQL_DATABASE"] = "${MYSQL_DATABASE}",
                        ["MYSQL_ROOT_PASSWORD"] = "${MYSQL_ROOT_PASSWORD}"
                    },
                    Ports = "[{\"host\":${PORT},\"container\":3306,\"protocol\":\"tcp\"}]",
                    Volumes = "[{\"source\":\"mysql_data\",\"target\":\"/var/lib/mysql\",\"type\":\"volume\"}]",
                    HealthcheckTest = new[] { "CMD", "mysqladmin", "ping", "-h", "localhost" },
                    HealthcheckInterval = "10s",
                    HealthcheckTimeout = "5s",
                    HealthcheckRetries = 5,
                    HealthcheckStartPeriod = "30s",
                    Restart = "unless-stopped"
                }
            },
            Volumes = new List<string> { "mysql_data" }
        },
        new TemplateDefinition
        {
            Id = "mariadb",
            Name = "MariaDB",
            Description = "MariaDB database with health check and persistent storage",
            Category = "Database",
            Icon = "database",
            Parameters = new List<TemplateParameterDto>
            {
                new("Database Name", "MARIADB_DATABASE", "string", "app", true, "Name of the default database"),
                new("Root Password", "MARIADB_ROOT_PASSWORD", "password", null, true, "MariaDB root password"),
                new("Port", "PORT", "number", "3306", false, "Host port to expose")
            },
            ServiceTemplates = new List<ServiceTemplate>
            {
                new ServiceTemplate
                {
                    Name = "mariadb",
                    Image = "mariadb:11",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        ["MARIADB_DATABASE"] = "${MARIADB_DATABASE}",
                        ["MARIADB_ROOT_PASSWORD"] = "${MARIADB_ROOT_PASSWORD}"
                    },
                    Ports = "[{\"host\":${PORT},\"container\":3306,\"protocol\":\"tcp\"}]",
                    Volumes = "[{\"source\":\"mariadb_data\",\"target\":\"/var/lib/mysql\",\"type\":\"volume\"}]",
                    HealthcheckTest = new[] { "CMD", "healthcheck.sh", "--connect", "--innodb_initialized" },
                    HealthcheckInterval = "10s",
                    HealthcheckTimeout = "5s",
                    HealthcheckRetries = 5,
                    HealthcheckStartPeriod = "30s",
                    Restart = "unless-stopped"
                }
            },
            Volumes = new List<string> { "mariadb_data" }
        },
        new TemplateDefinition
        {
            Id = "redis",
            Name = "Redis",
            Description = "Redis in-memory data store with optional persistence",
            Category = "Database",
            Icon = "database",
            Parameters = new List<TemplateParameterDto>
            {
                new("Port", "PORT", "number", "6379", false, "Host port to expose"),
                new("Password", "REDIS_PASSWORD", "password", null, false, "Redis password (optional)")
            },
            ServiceTemplates = new List<ServiceTemplate>
            {
                new ServiceTemplate
                {
                    Name = "redis",
                    Image = "redis:7-alpine",
                    Command = new[] { "redis-server", "--appendonly", "yes" },
                    Ports = "[{\"host\":${PORT},\"container\":6379,\"protocol\":\"tcp\"}]",
                    Volumes = "[{\"source\":\"redis_data\",\"target\":\"/data\",\"type\":\"volume\"}]",
                    HealthcheckTest = new[] { "CMD", "redis-cli", "ping" },
                    HealthcheckInterval = "10s",
                    HealthcheckTimeout = "5s",
                    HealthcheckRetries = 5,
                    Restart = "unless-stopped"
                }
            },
            Volumes = new List<string> { "redis_data" }
        },
        new TemplateDefinition
        {
            Id = "mongodb",
            Name = "MongoDB",
            Description = "MongoDB NoSQL database with persistent storage",
            Category = "Database",
            Icon = "database",
            Parameters = new List<TemplateParameterDto>
            {
                new("Root Username", "MONGO_INITDB_ROOT_USERNAME", "string", "admin", true, "MongoDB root username"),
                new("Root Password", "MONGO_INITDB_ROOT_PASSWORD", "password", null, true, "MongoDB root password"),
                new("Port", "PORT", "number", "27017", false, "Host port to expose")
            },
            ServiceTemplates = new List<ServiceTemplate>
            {
                new ServiceTemplate
                {
                    Name = "mongodb",
                    Image = "mongo:7",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        ["MONGO_INITDB_ROOT_USERNAME"] = "${MONGO_INITDB_ROOT_USERNAME}",
                        ["MONGO_INITDB_ROOT_PASSWORD"] = "${MONGO_INITDB_ROOT_PASSWORD}"
                    },
                    Ports = "[{\"host\":${PORT},\"container\":27017,\"protocol\":\"tcp\"}]",
                    Volumes = "[{\"source\":\"mongo_data\",\"target\":\"/data/db\",\"type\":\"volume\"}]",
                    Restart = "unless-stopped"
                }
            },
            Volumes = new List<string> { "mongo_data" }
        },
        new TemplateDefinition
        {
            Id = "wordpress",
            Name = "WordPress",
            Description = "WordPress with MariaDB database",
            Category = "CMS",
            Icon = "globe",
            Parameters = new List<TemplateParameterDto>
            {
                new("Site Port", "WP_PORT", "number", "8080", false, "Host port for WordPress"),
                new("Database Name", "MYSQL_DATABASE", "string", "wordpress", true, "Database name"),
                new("Database Password", "MYSQL_ROOT_PASSWORD", "password", null, true, "MariaDB root password")
            },
            ServiceTemplates = new List<ServiceTemplate>
            {
                new ServiceTemplate
                {
                    Name = "wordpress",
                    Image = "wordpress:latest",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        ["WORDPRESS_DB_HOST"] = "wordpress-db:3306",
                        ["WORDPRESS_DB_NAME"] = "${MYSQL_DATABASE}",
                        ["WORDPRESS_DB_USER"] = "root",
                        ["WORDPRESS_DB_PASSWORD"] = "${MYSQL_ROOT_PASSWORD}"
                    },
                    Ports = "[{\"host\":${WP_PORT},\"container\":80,\"protocol\":\"tcp\"}]",
                    Volumes = "[{\"source\":\"wordpress_data\",\"target\":\"/var/www/html\",\"type\":\"volume\"}]",
                    DependsOn = "{\"wordpress-db\":{\"condition\":\"service_healthy\"}}",
                    Restart = "unless-stopped"
                },
                new ServiceTemplate
                {
                    Name = "wordpress-db",
                    Image = "mariadb:11",
                    EnvironmentVariables = new Dictionary<string, string>
                    {
                        ["MARIADB_DATABASE"] = "${MYSQL_DATABASE}",
                        ["MARIADB_ROOT_PASSWORD"] = "${MYSQL_ROOT_PASSWORD}"
                    },
                    Volumes = "[{\"source\":\"wordpress_db_data\",\"target\":\"/var/lib/mysql\",\"type\":\"volume\"}]",
                    HealthcheckTest = new[] { "CMD", "healthcheck.sh", "--connect", "--innodb_initialized" },
                    HealthcheckInterval = "10s",
                    HealthcheckTimeout = "5s",
                    HealthcheckRetries = 5,
                    HealthcheckStartPeriod = "30s",
                    Restart = "unless-stopped"
                }
            },
            Volumes = new List<string> { "wordpress_data", "wordpress_db_data" }
        },
        new TemplateDefinition
        {
            Id = "nginx",
            Name = "Nginx",
            Description = "Nginx web server / reverse proxy",
            Category = "Web Server",
            Icon = "server",
            Parameters = new List<TemplateParameterDto>
            {
                new("HTTP Port", "HTTP_PORT", "number", "80", false, "HTTP port"),
                new("HTTPS Port", "HTTPS_PORT", "number", "443", false, "HTTPS port")
            },
            ServiceTemplates = new List<ServiceTemplate>
            {
                new ServiceTemplate
                {
                    Name = "nginx",
                    Image = "nginx:alpine",
                    Ports = "[{\"host\":${HTTP_PORT},\"container\":80,\"protocol\":\"tcp\"},{\"host\":${HTTPS_PORT},\"container\":443,\"protocol\":\"tcp\"}]",
                    Volumes = "[{\"source\":\"nginx_config\",\"target\":\"/etc/nginx/conf.d\",\"type\":\"volume\"},{\"source\":\"nginx_html\",\"target\":\"/usr/share/nginx/html\",\"type\":\"volume\"}]",
                    HealthcheckTest = new[] { "CMD", "nginx", "-t" },
                    HealthcheckInterval = "30s",
                    HealthcheckTimeout = "10s",
                    HealthcheckRetries = 3,
                    Restart = "unless-stopped"
                }
            },
            Volumes = new List<string> { "nginx_config", "nginx_html" }
        }
    };

    public static TemplateDefinition? GetById(string id) =>
        All.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}

public class TemplateDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public required string Icon { get; init; }
    public required List<TemplateParameterDto> Parameters { get; init; }
    public required List<ServiceTemplate> ServiceTemplates { get; init; }
    public required List<string> Volumes { get; init; }

    public TemplateDto ToDto() => new(
        Id,
        Name,
        Description,
        Category,
        Icon,
        ServiceTemplates.Select(s => new TemplateServiceDto(s.Name, s.Image, null)),
        Parameters);
}

public class ServiceTemplate
{
    public required string Name { get; init; }
    public required string Image { get; init; }
    public Dictionary<string, string>? EnvironmentVariables { get; init; }
    public string[]? Command { get; init; }
    public string? Ports { get; init; }
    public string? Volumes { get; init; }
    public string[]? HealthcheckTest { get; init; }
    public string? HealthcheckInterval { get; init; }
    public string? HealthcheckTimeout { get; init; }
    public int? HealthcheckRetries { get; init; }
    public string? HealthcheckStartPeriod { get; init; }
    public string? DependsOn { get; init; }
    public string? Restart { get; init; }
}
