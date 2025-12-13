import { useMemo, useCallback, useEffect } from "react"
import {
  ReactFlow,
  Background,
  Controls,
  MiniMap,
  useNodesState,
  useEdgesState,
  type Node,
  type Edge,
  type NodeTypes,
} from "@xyflow/react"
import "@xyflow/react/dist/style.css"
import type { ServiceDto } from "@/api/types.gen"
import { ServiceNode, type ServiceNodeData } from "./service-node"

interface ServicesFlowProps {
  services: ServiceDto[]
}

const nodeTypes: NodeTypes = {
  service: ServiceNode,
}

// Parse dependsOn JSON to array of service names
function parseDependsOnToNames(dependsOn: string | null | undefined): string[] {
  if (!dependsOn) return []
  try {
    const deps = JSON.parse(dependsOn) as Record<string, unknown>
    return Object.keys(deps)
  } catch {
    // Fallback: comma-separated names
    return dependsOn.split(",").map((d) => d.trim()).filter(Boolean)
  }
}

// Calculate layer for each service based on dependencies (topological sort)
function getServiceLayers(services: ServiceDto[]): Map<string, number> {
  const layers = new Map<string, number>()
  const deps = new Map<string, string[]>()

  // Build dependency map (service ID -> array of dependency IDs)
  for (const service of services) {
    const depNames = parseDependsOnToNames(service.dependsOn)
    const depIds = depNames
      .map((name) => services.find((s) => s.name === name)?.id)
      .filter((id): id is string => id !== undefined)
    deps.set(service.id, depIds)
  }

  // Calculate layer for each service recursively
  function getLayer(id: string, visited = new Set<string>()): number {
    if (layers.has(id)) return layers.get(id)!
    if (visited.has(id)) return 0 // Circular dependency protection

    visited.add(id)
    const serviceDeps = deps.get(id) || []

    if (serviceDeps.length === 0) {
      layers.set(id, 0)
      return 0
    }

    const maxDepLayer = Math.max(...serviceDeps.map((d) => getLayer(d, new Set(visited))))
    const layer = maxDepLayer + 1
    layers.set(id, layer)
    return layer
  }

  for (const service of services) {
    getLayer(service.id)
  }

  return layers
}

// Calculate positions for horizontal (left-to-right) flow layout
function getServicePositions(
  services: ServiceDto[],
  layers: Map<string, number>
): Map<string, { x: number; y: number }> {
  const positions = new Map<string, { x: number; y: number }>()
  const layerWidth = 280   // Horizontal spacing between layers (columns)
  const nodeHeight = 120   // Vertical spacing within a layer (rows)

  // Build dependency map (service ID -> array of dependency IDs)
  const deps = new Map<string, string[]>()
  for (const service of services) {
    const depNames = parseDependsOnToNames(service.dependsOn)
    const depIds = depNames
      .map((name) => services.find((s) => s.name === name)?.id)
      .filter((id): id is string => id !== undefined)
    deps.set(service.id, depIds)
  }

  // Group services by layer
  const maxLayer = Math.max(...Array.from(layers.values()), 0)
  const servicesByLayer = new Map<number, ServiceDto[]>()
  for (let i = 0; i <= maxLayer; i++) {
    servicesByLayer.set(i, [])
  }
  for (const service of services) {
    const layer = layers.get(service.id) ?? 0
    servicesByLayer.get(layer)!.push(service)
  }

  // Position layer 0 (no dependencies) - leftmost column, stacked vertically
  const layer0 = servicesByLayer.get(0) ?? []
  layer0.forEach((service, index) => {
    positions.set(service.id, {
      x: 0,
      y: index * nodeHeight,
    })
  })

  // Position subsequent layers to the right
  for (let layer = 1; layer <= maxLayer; layer++) {
    const layerServices = servicesByLayer.get(layer) ?? []

    for (const service of layerServices) {
      const serviceDeps = deps.get(service.id) ?? []

      if (serviceDeps.length > 0) {
        // Center vertically relative to dependencies
        const depPositions = serviceDeps
          .map((depId) => positions.get(depId))
          .filter((pos): pos is { x: number; y: number } => pos !== undefined)

        if (depPositions.length > 0) {
          const avgY =
            depPositions.reduce((sum, p) => sum + p.y, 0) / depPositions.length
          positions.set(service.id, {
            x: layer * layerWidth,
            y: avgY,
          })
        } else {
          // Fallback: position sequentially
          const index = layerServices.indexOf(service)
          positions.set(service.id, {
            x: layer * layerWidth,
            y: index * nodeHeight,
          })
        }
      } else {
        // No dependencies, position sequentially
        const index = layerServices.indexOf(service)
        positions.set(service.id, {
          x: layer * layerWidth,
          y: index * nodeHeight,
        })
      }
    }

    // Resolve vertical overlaps within layer
    const layerPositions = layerServices.map((s) => ({
      id: s.id,
      y: positions.get(s.id)!.y,
    }))
    layerPositions.sort((a, b) => a.y - b.y)

    for (let i = 1; i < layerPositions.length; i++) {
      const prev = layerPositions[i - 1]
      const curr = layerPositions[i]
      if (curr.y - prev.y < nodeHeight) {
        // Overlap detected, shift down
        const newY = prev.y + nodeHeight
        positions.set(curr.id, {
          x: layer * layerWidth,
          y: newY,
        })
        layerPositions[i].y = newY
      }
    }
  }

  return positions
}

function parseEdges(services: ServiceDto[]): Edge[] {
  const edges: Edge[] = []

  for (const service of services) {
    if (!service.dependsOn) continue

    try {
      // dependsOn is JSON: {"service_name": {"condition": "service_healthy"}}
      const deps = JSON.parse(service.dependsOn) as Record<string, unknown>
      for (const depName of Object.keys(deps)) {
        const sourceService = services.find((s) => s.name === depName)
        if (sourceService) {
          edges.push({
            id: `${sourceService.id}-${service.id}`,
            source: sourceService.id,
            target: service.id,
            type: "smoothstep",
            animated: true,
            style: { stroke: "#3b82f6", strokeWidth: 2 },
          })
        }
      }
    } catch {
      // If parsing fails, try treating it as comma-separated names
      const depNames = service.dependsOn.split(",").map((d) => d.trim())
      for (const depName of depNames) {
        const sourceService = services.find((s) => s.name === depName)
        if (sourceService) {
          edges.push({
            id: `${sourceService.id}-${service.id}`,
            source: sourceService.id,
            target: service.id,
            type: "smoothstep",
            animated: true,
            style: { stroke: "#3b82f6", strokeWidth: 2 },
          })
        }
      }
    }
  }

  return edges
}

type ServiceNode = Node<ServiceNodeData, "service">

export function ServicesFlow({ services }: ServicesFlowProps) {
  const initialNodes = useMemo<ServiceNode[]>(() => {
    // Get layer for each service based on dependencies
    const layers = getServiceLayers(services)
    // Get optimized positions (centered under dependencies)
    const positions = getServicePositions(services, layers)

    return services.map((service) => {
      const pos = positions.get(service.id) ?? { x: 0, y: 0 }

      return {
        id: service.id,
        type: "service" as const,
        position: pos,
        data: {
          name: service.name,
          image: service.image,
          status: service.deploymentStatus,
        },
      }
    })
  }, [services])

  const initialEdges = useMemo(() => parseEdges(services), [services])

  const [nodes, setNodes, onNodesChange] = useNodesState(initialNodes)
  const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges)

  // Sync state when services change
  useEffect(() => {
    setNodes(initialNodes)
    setEdges(initialEdges)
  }, [initialNodes, initialEdges, setNodes, setEdges])

  const onInit = useCallback(() => {
    // Flow initialized
  }, [])

  return (
    <div className="h-[500px] w-full rounded-md border bg-background">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onInit={onInit}
        nodeTypes={nodeTypes}
        fitView
        fitViewOptions={{ padding: 0.2, maxZoom: 1 }}
        minZoom={0.5}
        maxZoom={2}
      >
        <Background />
        <Controls />
        <MiniMap
          nodeStrokeWidth={3}
          zoomable
          pannable
          className="!bg-background !border"
        />
      </ReactFlow>
    </div>
  )
}
