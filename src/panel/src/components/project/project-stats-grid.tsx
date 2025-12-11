import {
  Item,
  ItemMedia,
  ItemContent,
  ItemTitle,
  ItemDescription,
} from '@/components/ui/item'
import { formatRelativeTime, calculateUptime } from '@/lib/date-utils'
import type { DeploymentStatusDto, ProjectDto } from '@/api/types.gen'
import {
  Activity,
  Container,
  CheckCircle2,
  Users,
  Clock,
  TrendingUp,
  type LucideIcon,
} from 'lucide-react'

interface StatCardProps {
  title: string
  value: string | number
  description?: string
  icon: LucideIcon
  variant?: 'default' | 'success' | 'warning' | 'destructive' | 'info'
}

function StatCard({ title, value, description, icon: Icon, variant = 'default' }: StatCardProps) {
  const iconStyles = {
    default: 'bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-400 border-zinc-200 dark:border-zinc-700',
    success: 'bg-green-100 text-green-600 dark:bg-green-900/50 dark:text-green-400 border-green-200 dark:border-green-800',
    warning: 'bg-yellow-100 text-yellow-600 dark:bg-yellow-900/50 dark:text-yellow-400 border-yellow-200 dark:border-yellow-800',
    destructive: 'bg-red-100 text-red-600 dark:bg-red-900/50 dark:text-red-400 border-red-200 dark:border-red-800',
    info: 'bg-blue-100 text-blue-600 dark:bg-blue-900/50 dark:text-blue-400 border-blue-200 dark:border-blue-800',
  }

  return (
    <Item
      variant="outline"
      size="sm"
    >
      <ItemMedia variant="icon" className={iconStyles[variant]}>
        <Icon className="h-4 w-4" />
      </ItemMedia>
      <ItemContent>
        <ItemTitle>{value}</ItemTitle>
        <ItemDescription className="uppercase">
          {title}
        </ItemDescription>
        {description && (
          <ItemDescription className="truncate">{description}</ItemDescription>
        )}
      </ItemContent>
    </Item>
  )
}

interface ProjectStatsGridProps {
  project: ProjectDto
  dockerStatus: DeploymentStatusDto | null | undefined
  isConnected: boolean
}

export function ProjectStatsGrid({ project, dockerStatus, isConnected }: ProjectStatsGridProps) {
  const containers = dockerStatus?.containers ?? []
  const runningContainers = containers.filter(c => c.state?.toLowerCase() === 'running')

  // Map deployment status enum to display values
  const getStatusDisplay = () => {
    const status = dockerStatus?.status ?? project.deploymentStatus
    switch (status) {
      case 0: // NotDeployed
        return { text: 'Not Deployed', variant: 'default' as const }
      case 1: // Deploying
        return { text: 'Deploying', variant: 'info' as const }
      case 2: // Running
        return { text: 'Running', variant: 'success' as const }
      case 3: // Stopped
        return { text: 'Stopped', variant: 'warning' as const }
      case 4: // Error
        return { text: 'Error', variant: 'destructive' as const }
      case 5: // PartiallyRunning
        return { text: 'Partial', variant: 'warning' as const }
      default:
        return { text: 'Unknown', variant: 'default' as const }
    }
  }

  const statusDisplay = getStatusDisplay()
  const lastDeployedAt = dockerStatus?.lastDeployedAt ?? project.lastDeployedAt

  const stats: StatCardProps[] = [
    {
      title: 'Status',
      value: statusDisplay.text,
      description: isConnected ? 'Connected' : 'Connecting...',
      icon: Activity,
      variant: statusDisplay.variant,
    },
    {
      title: 'Containers',
      value: containers.length,
      description: containers.length === 1 ? '1 service' : `${containers.length} services`,
      icon: Container,
      variant: 'default',
    },
    {
      title: 'Active',
      value: runningContainers.length,
      description: runningContainers.length === containers.length && containers.length > 0
        ? 'All healthy'
        : `${runningContainers.length} of ${containers.length} running`,
      icon: CheckCircle2,
      variant: runningContainers.length === containers.length && containers.length > 0
        ? 'success'
        : runningContainers.length > 0
          ? 'warning'
          : 'default',
    },
    {
      title: 'Members',
      value: project.members?.length ?? 0,
      description: project.members?.length === 1 ? '1 collaborator' : `${project.members?.length ?? 0} collaborators`,
      icon: Users,
      variant: 'default',
    },
    {
      title: 'Last Deployed',
      value: formatRelativeTime(lastDeployedAt),
      description: lastDeployedAt ? new Date(lastDeployedAt).toLocaleDateString() : undefined,
      icon: Clock,
      variant: 'default',
    },
    {
      title: 'Uptime',
      value: calculateUptime(lastDeployedAt),
      description: dockerStatus?.status === 2 ? 'Currently running' : 'Not running',
      icon: TrendingUp,
      variant: dockerStatus?.status === 2 ? 'success' : 'default',
    },
  ]

  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
      {stats.map((stat, index) => (
        <StatCard key={index} {...stat} />
      ))}
    </div>
  )
}
