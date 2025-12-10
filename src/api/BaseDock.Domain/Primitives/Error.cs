namespace BaseDock.Domain.Primitives;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error NotFound(string entityName, object id) =>
        new($"{entityName}.NotFound", $"{entityName} with id '{id}' was not found.");

    public static Error Validation(string code, string message) =>
        new(code, message);

    public static Error Conflict(string code, string message) =>
        new(code, message);

    public static Error Unauthorized(string message = "Unauthorized access.") =>
        new("Error.Unauthorized", message);

    public static Error Forbidden(string message = "Access is forbidden.") =>
        new("Error.Forbidden", message);

    public static Error DockerError(string message) =>
        new("Docker.Error", message);

    public static Error DockerConnectionFailed(string message = "Failed to connect to Docker daemon.") =>
        new("Docker.ConnectionFailed", message);

    public static Error DockerComposeInvalid(string message) =>
        new("Docker.ComposeInvalid", message);
}
