namespace BaseDock.Application.Features.Templates.DTOs;

public sealed record TemplateDto(
    string Id,
    string Name,
    string Description,
    string Category,
    string Icon,
    IEnumerable<TemplateServiceDto> Services,
    IEnumerable<TemplateParameterDto> Parameters);

public sealed record TemplateServiceDto(
    string Name,
    string Image,
    string? Description);

public sealed record TemplateParameterDto(
    string Name,
    string Key,
    string Type,
    string? DefaultValue,
    bool Required,
    string? Description);

public sealed record ApplyTemplateRequest(
    Dictionary<string, string> Parameters);

public sealed record ApplyTemplateResult(
    IEnumerable<CreatedServiceDto> Services);

public sealed record CreatedServiceDto(
    Guid Id,
    string Name,
    string Slug);
