namespace BaseDock.Application.Features.Services.Validators;

using System.Text.Json;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;

/// <summary>
/// Validates that service dependencies do not form circular references.
/// </summary>
public static class CircularDependencyValidator
{
    /// <summary>
    /// Validates that adding the specified dependencies to a service would not create a circular dependency.
    /// </summary>
    /// <param name="serviceName">The name of the service being created/updated.</param>
    /// <param name="dependsOn">The JSON string of dependencies (format: {"service_name": {"condition": "..."}}).</param>
    /// <param name="allServices">All services in the environment.</param>
    /// <param name="excludeServiceId">The ID of the service being updated (to exclude from graph when updating).</param>
    /// <returns>An Error if a circular dependency is detected, null otherwise.</returns>
    public static Error? Validate(
        string serviceName,
        string? dependsOn,
        IEnumerable<Service> allServices,
        Guid? excludeServiceId = null)
    {
        var newDependencies = ParseDependsOn(dependsOn);
        if (newDependencies.Count == 0)
        {
            return null; // No dependencies, no cycle possible
        }

        // Build the dependency graph
        var graph = BuildDependencyGraph(allServices, serviceName, newDependencies, excludeServiceId);

        // Check for cycles using DFS
        var cyclePath = DetectCycle(graph, serviceName);
        if (cyclePath != null)
        {
            var cycleDescription = string.Join(" -> ", cyclePath);
            return Error.Validation(
                "Service.CircularDependency",
                $"Circular dependency detected: {cycleDescription}");
        }

        return null;
    }

    private static List<string> ParseDependsOn(string? dependsOn)
    {
        if (string.IsNullOrWhiteSpace(dependsOn))
        {
            return [];
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(dependsOn);
            return parsed?.Keys.ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static Dictionary<string, List<string>> BuildDependencyGraph(
        IEnumerable<Service> allServices,
        string currentServiceName,
        List<string> currentServiceDeps,
        Guid? excludeServiceId)
    {
        var graph = new Dictionary<string, List<string>>();

        foreach (var service in allServices)
        {
            // Skip the service being updated (we'll add it with new deps)
            if (excludeServiceId.HasValue && service.Id == excludeServiceId.Value)
            {
                continue;
            }

            var deps = ParseDependsOn(service.DependsOn);
            graph[service.Name] = deps;
        }

        // Add/update the current service with its new dependencies
        graph[currentServiceName] = currentServiceDeps;

        return graph;
    }

    /// <summary>
    /// Detects a cycle starting from the given service using DFS.
    /// Returns the cycle path if found, null otherwise.
    /// </summary>
    private static List<string>? DetectCycle(Dictionary<string, List<string>> graph, string startService)
    {
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        var path = new List<string>();

        if (HasCycleDfs(graph, startService, visited, recursionStack, path))
        {
            // Find where the cycle starts and build the cycle path
            var cycleStart = path[^1];
            var cycleStartIndex = path.IndexOf(cycleStart);
            var cyclePath = path.Skip(cycleStartIndex).ToList();
            cyclePath.Add(cycleStart); // Close the cycle
            return cyclePath;
        }

        return null;
    }

    private static bool HasCycleDfs(
        Dictionary<string, List<string>> graph,
        string current,
        HashSet<string> visited,
        HashSet<string> recursionStack,
        List<string> path)
    {
        visited.Add(current);
        recursionStack.Add(current);
        path.Add(current);

        if (graph.TryGetValue(current, out var dependencies))
        {
            foreach (var dep in dependencies)
            {
                if (!visited.Contains(dep))
                {
                    if (HasCycleDfs(graph, dep, visited, recursionStack, path))
                    {
                        return true;
                    }
                }
                else if (recursionStack.Contains(dep))
                {
                    // Found a cycle
                    path.Add(dep);
                    return true;
                }
            }
        }

        recursionStack.Remove(current);
        path.RemoveAt(path.Count - 1);
        return false;
    }
}
