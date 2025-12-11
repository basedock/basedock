import { cn } from "@/lib/utils"
import type { DeploymentStatus } from "@/api/types.gen"

// Map numeric enum values to status names
const statusNames: Record<number, string> = {
  0: "NotDeployed",
  1: "Deploying",
  2: "Running",
  3: "Stopped",
  4: "Error",
  5: "PartiallyRunning",
}

const statusConfig: Record<string, { label: string; className: string }> = {
  NotDeployed: {
    label: "Not Deployed",
    className: "bg-muted text-muted-foreground",
  },
  Deploying: {
    label: "Deploying...",
    className: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300 animate-pulse",
  },
  Running: {
    label: "Running",
    className: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300",
  },
  Stopped: {
    label: "Stopped",
    className: "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300",
  },
  Error: {
    label: "Error",
    className: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300",
  },
  PartiallyRunning: {
    label: "Partial",
    className: "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-300",
  },
}

interface DeploymentStatusBadgeProps {
  status: DeploymentStatus
  className?: string
}

export function DeploymentStatusBadge({ status, className }: DeploymentStatusBadgeProps) {
  const statusName = typeof status === "number" ? statusNames[status] : String(status)
  const config = statusConfig[statusName] ?? statusConfig.NotDeployed

  return (
    <span
      className={cn(
        "inline-flex items-center px-2 py-1 rounded-full text-xs font-medium",
        config.className,
        className
      )}
    >
      {config.label}
    </span>
  )
}

export function getStatusName(status: DeploymentStatus): string {
  return typeof status === "number" ? statusNames[status] : String(status)
}
