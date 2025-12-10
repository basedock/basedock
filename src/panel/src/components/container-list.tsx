import type { ContainerInfo } from "@/api/types.gen"
import { cn } from "@/lib/utils"
import { Container, Globe } from "lucide-react"

interface ContainerListProps {
  containers: ContainerInfo[]
}

export function ContainerList({ containers }: ContainerListProps) {
  if (containers.length === 0) {
    return (
      <div className="text-sm text-muted-foreground py-4 text-center">
        No containers running
      </div>
    )
  }

  return (
    <div className="space-y-2">
      {containers.map((container) => (
        <ContainerCard key={container.id} container={container} />
      ))}
    </div>
  )
}

function ContainerCard({ container }: { container: ContainerInfo }) {
  const isRunning = container.state.toLowerCase() === "running"

  return (
    <div className="flex items-start justify-between p-3 border rounded-lg bg-card">
      <div className="flex items-start gap-3">
        <div
          className={cn(
            "mt-1 p-1.5 rounded-md",
            isRunning ? "bg-green-100 text-green-700" : "bg-muted text-muted-foreground"
          )}
        >
          <Container className="h-4 w-4" />
        </div>
        <div className="space-y-1">
          <div className="font-medium text-sm">{container.service}</div>
          <div className="text-xs text-muted-foreground">{container.name}</div>
          <div className="text-xs text-muted-foreground">
            <span
              className={cn(
                "inline-flex items-center px-1.5 py-0.5 rounded text-xs font-medium",
                isRunning
                  ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300"
                  : "bg-muted text-muted-foreground"
              )}
            >
              {container.status}
            </span>
          </div>
        </div>
      </div>

      {container.ports.length > 0 && (
        <div className="text-right space-y-1">
          {container.ports
            .filter((p) => p.publicPort)
            .map((port, index) => (
              <div key={index} className="flex items-center gap-1 text-xs text-muted-foreground">
                <Globe className="h-3 w-3" />
                <span>
                  {port.publicPort}:{port.privatePort}/{port.protocol}
                </span>
              </div>
            ))}
        </div>
      )}
    </div>
  )
}
