import { Card, CardContent } from '@/components/ui/card'
import { cn } from '@/lib/utils'
import { Play, FileCode, ScrollText, Settings, Container, type LucideIcon } from 'lucide-react'
import { PROJECT_TYPE } from '@/components/create-project/types'
import type { ProjectType } from '@/api/types.gen'

interface QuickActionProps {
  icon: LucideIcon
  label: string
  onClick: () => void
  disabled?: boolean
  primary?: boolean
}

function QuickAction({ icon: Icon, label, onClick, disabled, primary }: QuickActionProps) {
  return (
    <Card
      className={cn(
        'cursor-pointer transition-all duration-200 hover:shadow-md',
        primary
          ? 'bg-primary text-primary-foreground hover:bg-primary/90'
          : 'hover:border-primary/50 hover:bg-accent/50',
        disabled && 'opacity-50 cursor-not-allowed'
      )}
      onClick={disabled ? undefined : onClick}
    >
      <CardContent className="flex flex-col items-center justify-center gap-2 py-6">
        <div className={cn(
          'p-2.5 rounded-full',
          primary ? 'bg-white/20' : 'bg-muted'
        )}>
          <Icon className="h-5 w-5" />
        </div>
        <span className="text-sm font-medium">{label}</span>
      </CardContent>
    </Card>
  )
}

interface ProjectQuickActionsProps {
  isAdmin: boolean
  projectType: ProjectType
  onNavigateToTab: (tab: string) => void
  onDeploy?: () => void
  isDeploying?: boolean
}

export function ProjectQuickActions({
  isAdmin,
  projectType,
  onNavigateToTab,
  onDeploy,
  isDeploying = false,
}: ProjectQuickActionsProps) {
  const isComposeProject = projectType === PROJECT_TYPE.ComposeFile
  const isDockerImageProject = projectType === PROJECT_TYPE.DockerImage

  return (
    <div className="space-y-3">
      <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
        Quick Actions
      </h3>
      <div className="grid gap-3 grid-cols-2 lg:grid-cols-4">
        {isAdmin && (
          <>
            <QuickAction
              icon={Play}
              label="Deploy Now"
              onClick={() => onDeploy?.()}
              disabled={isDeploying}
              primary
            />
            {isComposeProject && (
              <QuickAction
                icon={FileCode}
                label="Edit Compose"
                onClick={() => onNavigateToTab('compose')}
              />
            )}
            {isDockerImageProject && (
              <QuickAction
                icon={Container}
                label="Edit Config"
                onClick={() => onNavigateToTab('image-config')}
              />
            )}
          </>
        )}
        <QuickAction
          icon={ScrollText}
          label="View Logs"
          onClick={() => onNavigateToTab('logs')}
        />
        <QuickAction
          icon={Settings}
          label="Settings"
          onClick={() => onNavigateToTab('settings')}
        />
      </div>
    </div>
  )
}
