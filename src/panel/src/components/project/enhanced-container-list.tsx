import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card'
import {
  Item,
  ItemMedia,
  ItemContent,
  ItemTitle,
  ItemDescription,
  ItemActions,
  ItemFooter,
  ItemGroup,
} from '@/components/ui/item'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip'
import { cn } from '@/lib/utils'
import type { ContainerInfo } from '@/api/types.gen'
import {
  Container,
  Globe,
  MoreVertical,
  Eye,
  Terminal,
  RotateCw,
  Square,
  Check,
} from 'lucide-react'
import { useState } from 'react'

interface EnhancedContainerCardProps {
  container: ContainerInfo
  isAdmin: boolean
  onViewLogs?: () => void
}

function EnhancedContainerCard({ container, isAdmin }: EnhancedContainerCardProps) {
  const [copiedPort, setCopiedPort] = useState<string | null>(null)
  const isRunning = container.state?.toLowerCase() === 'running'

  const copyToClipboard = (port: string) => {
    navigator.clipboard.writeText(`localhost:${port}`)
    setCopiedPort(port)
    setTimeout(() => setCopiedPort(null), 2000)
  }

  // Filter ports with publicPort and deduplicate by publicPort:privatePort
  const publicPorts = container.ports?.filter(p => p.publicPort) ?? []
  const uniquePorts = publicPorts.filter((port, index, self) =>
    index === self.findIndex(p =>
      p.publicPort === port.publicPort && p.privatePort === port.privatePort
    )
  )

  return (
    <Item
      variant="outline"
      className={cn(
        'transition-all duration-200 hover:shadow-sm',
        isRunning ? 'border-l-4 border-l-green-500' : 'border-l-4 border-l-muted-foreground/30'
      )}
    >
      <ItemMedia>
        <Container
          className={cn(
            'h-5 w-5',
            isRunning ? 'text-green-600 dark:text-green-400' : 'text-muted-foreground'
          )}
        />
      </ItemMedia>
      <ItemContent>
        <ItemTitle>{container.service}</ItemTitle>
        <ItemDescription className="font-mono">{container.name}</ItemDescription>
      </ItemContent>
      <ItemActions>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" className="h-8 w-8">
              <MoreVertical className="h-4 w-4" />
              <span className="sr-only">Open menu</span>
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem>
              <Eye className="mr-2 h-4 w-4" />
              View Logs
            </DropdownMenuItem>
            <DropdownMenuItem>
              <Terminal className="mr-2 h-4 w-4" />
              Exec Shell
            </DropdownMenuItem>
            {isAdmin && (
              <>
                <DropdownMenuSeparator />
                <DropdownMenuItem>
                  <RotateCw className="mr-2 h-4 w-4" />
                  Restart
                </DropdownMenuItem>
                <DropdownMenuItem className="text-destructive focus:text-destructive">
                  <Square className="mr-2 h-4 w-4" />
                  Stop
                </DropdownMenuItem>
              </>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </ItemActions>
      <ItemFooter>
        <Badge
          variant={isRunning ? 'default' : 'secondary'}
          className={cn(
            'gap-1.5',
            isRunning && 'bg-green-600 hover:bg-green-600'
          )}
        >
          <div
            className={cn(
              'h-1.5 w-1.5 rounded-full',
              isRunning ? 'bg-white animate-pulse' : 'bg-muted-foreground'
            )}
          />
          {container.status}
        </Badge>
        {uniquePorts.length > 0 && (
          <TooltipProvider>
            <div className="flex flex-wrap gap-2">
              {uniquePorts.map((port, idx) => (
                <Tooltip key={idx}>
                  <TooltipTrigger asChild>
                    <Button
                      variant="outline"
                      size="sm"
                      className="h-7 gap-1.5 text-xs font-mono"
                      onClick={() => copyToClipboard(port.publicPort!.toString())}
                    >
                      {copiedPort === port.publicPort?.toString() ? (
                        <Check className="h-3 w-3 text-green-600" />
                      ) : (
                        <Globe className="h-3 w-3" />
                      )}
                      {port.publicPort}:{port.privatePort}
                    </Button>
                  </TooltipTrigger>
                  <TooltipContent>
                    <p>Click to copy localhost:{port.publicPort} ({port.protocol})</p>
                  </TooltipContent>
                </Tooltip>
              ))}
            </div>
          </TooltipProvider>
        )}
      </ItemFooter>
    </Item>
  )
}

interface EnhancedContainerListProps {
  containers: ContainerInfo[]
  isAdmin: boolean
}

export function EnhancedContainerList({ containers, isAdmin }: EnhancedContainerListProps) {
  const runningCount = containers.filter(c => c.state?.toLowerCase() === 'running').length
  const totalCount = containers.length

  if (totalCount === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Containers</CardTitle>
          <CardDescription>No containers running</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-8 text-center">
            <Container className="h-12 w-12 text-muted-foreground mb-3" />
            <p className="text-sm text-muted-foreground">
              No containers are currently running for this project.
            </p>
            <p className="text-xs text-muted-foreground mt-1">
              Deploy your project to see containers here.
            </p>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Containers</CardTitle>
            <CardDescription>
              {runningCount} of {totalCount} running
            </CardDescription>
          </div>
          <Badge variant="outline" className="gap-1.5">
            <Container className="h-3 w-3" />
            {totalCount} total
          </Badge>
        </div>
      </CardHeader>
      <CardContent>
        <ItemGroup className="gap-3">
          {containers.map(container => (
            <EnhancedContainerCard
              key={container.id}
              container={container}
              isAdmin={isAdmin}
            />
          ))}
        </ItemGroup>
      </CardContent>
    </Card>
  )
}
