import type { ServiceSummaryDto } from "@/api/types.gen"

/**
 * Parse dependsOn JSON string to array of service names
 */
export function parseDependsOnToNames(dependsOn: string | null | undefined): string[] {
  if (!dependsOn) return []
  try {
    const parsed = JSON.parse(dependsOn) as Record<string, unknown>
    return Object.keys(parsed)
  } catch {
    return []
  }
}

/**
 * Build a dependency graph from services
 * Optionally override a specific service's dependencies
 */
function buildDependencyGraph(
  services: ServiceSummaryDto[],
  overrideServiceName?: string,
  overrideDeps?: string[]
): Map<string, string[]> {
  const graph = new Map<string, string[]>()
  for (const service of services) {
    if (overrideServiceName && service.name === overrideServiceName) {
      graph.set(service.name, overrideDeps || [])
    } else {
      graph.set(service.name, parseDependsOnToNames(service.dependsOn))
    }
  }
  // If the service being edited is new (not in the list yet), add it
  if (overrideServiceName && !graph.has(overrideServiceName)) {
    graph.set(overrideServiceName, overrideDeps || [])
  }
  return graph
}

/**
 * Check if adding a dependency would create a circular dependency
 * @param services - All services in the environment
 * @param fromServiceName - The service we're editing
 * @param toServiceName - The potential dependency to add
 * @param currentSelectedDeps - Currently selected dependencies in the form
 */
export function wouldCreateCycle(
  services: ServiceSummaryDto[],
  fromServiceName: string,
  toServiceName: string,
  currentSelectedDeps: string[] = []
): boolean {
  // Build the graph, overriding the current service's deps with form state
  const graph = buildDependencyGraph(services, fromServiceName, currentSelectedDeps)

  // Add the potential new dependency temporarily
  const currentDeps = graph.get(fromServiceName) || []
  graph.set(fromServiceName, [...currentDeps, toServiceName])

  // DFS to check if we can reach fromServiceName starting from toServiceName
  const visited = new Set<string>()

  function canReach(current: string, target: string): boolean {
    if (current === target) return true
    if (visited.has(current)) return false

    visited.add(current)
    const deps = graph.get(current) || []

    for (const dep of deps) {
      if (canReach(dep, target)) return true
    }

    return false
  }

  // Check if toServiceName can reach fromServiceName (which would create a cycle)
  return canReach(toServiceName, fromServiceName)
}

/**
 * Get valid dependency options for a service
 * Filters out:
 * - The service itself
 * - Services that would create a circular dependency if selected
 * @param services - All services in the environment
 * @param currentServiceName - Name of the service being edited
 * @param currentServiceId - ID of the service being edited (for filtering self)
 * @param currentSelectedDeps - Currently selected dependencies in the form
 */
export function getValidDependencyOptions(
  services: ServiceSummaryDto[],
  currentServiceName: string,
  currentServiceId?: string,
  currentSelectedDeps: string[] = []
): ServiceSummaryDto[] {
  return services.filter((service) => {
    // Exclude self
    if (service.id === currentServiceId || service.name === currentServiceName) {
      return false
    }

    // Exclude services that would create a cycle
    if (wouldCreateCycle(services, currentServiceName, service.name, currentSelectedDeps)) {
      return false
    }

    return true
  })
}
