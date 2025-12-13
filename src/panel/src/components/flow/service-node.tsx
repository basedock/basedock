import { memo } from "react"
import { Handle, Position } from "@xyflow/react"
import { Badge } from "@/components/ui/badge"
import { Container } from "lucide-react"

export interface ServiceNodeData extends Record<string, unknown> {
  name: string
  image: string | null
  status: string
}

function getStatusVariant(status: string): "default" | "secondary" | "destructive" | "outline" {
  switch (status) {
    case "Running":
      return "default"
    case "Deploying":
      return "secondary"
    case "Error":
      return "destructive"
    default:
      return "outline"
  }
}

interface ServiceNodeProps {
  data: ServiceNodeData
}

function ServiceNodeComponent({ data }: ServiceNodeProps) {
  return (
    <div className="px-4 py-3 shadow-md rounded-lg bg-card border min-w-[180px]">
      <Handle type="target" position={Position.Top} className="!bg-primary" />
      <div className="flex items-center gap-2 mb-1">
        <Container className="h-4 w-4 text-muted-foreground" />
        <span className="font-medium text-sm">{data.name}</span>
      </div>
      {data.image && (
        <div className="text-xs text-muted-foreground truncate mb-2 max-w-[160px]">
          {data.image}
        </div>
      )}
      <Badge variant={getStatusVariant(data.status)} className="text-xs">
        {data.status}
      </Badge>
      <Handle type="source" position={Position.Bottom} className="!bg-primary" />
    </div>
  )
}

export const ServiceNode = memo(ServiceNodeComponent)
