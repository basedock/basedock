import { useMemo, useCallback } from "react"
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
            animated: true,
            style: { stroke: "hsl(var(--primary))" },
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
            animated: true,
            style: { stroke: "hsl(var(--primary))" },
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
    const columns = 3
    const columnWidth = 280
    const rowHeight = 150

    return services.map((service, index) => ({
      id: service.id,
      type: "service" as const,
      position: {
        x: (index % columns) * columnWidth,
        y: Math.floor(index / columns) * rowHeight,
      },
      data: {
        name: service.name,
        image: service.image,
        status: service.deploymentStatus,
      },
    }))
  }, [services])

  const initialEdges = useMemo(() => parseEdges(services), [services])

  const [nodes, , onNodesChange] = useNodesState(initialNodes)
  const [edges, , onEdgesChange] = useEdgesState(initialEdges)

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
        fitViewOptions={{ padding: 0.2 }}
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
