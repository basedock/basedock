import { useMutation, useQueryClient } from "@tanstack/react-query"
import { Button } from "@/components/ui/button"
import { ButtonGroup, ButtonGroupSeparator } from "@/components/ui/button-group"
import { Play, Square, RotateCw, Trash2, Loader2 } from "lucide-react"
import {
  deployProject,
  stopProject,
  restartProject,
  removeProjectContainers,
} from "@/api/sdk.gen"
import type { DeploymentStatus } from "@/api/types.gen"
import { getStatusName } from "./deployment-status-badge"

interface DeploymentControlsProps {
  projectId: string
  status: DeploymentStatus
  hasComposeFile: boolean
}

export function DeploymentControls({
  projectId,
  status,
  hasComposeFile,
}: DeploymentControlsProps) {
  const queryClient = useQueryClient()
  const statusName = getStatusName(status)

  const invalidateQueries = () => {
    queryClient.invalidateQueries({ queryKey: ["projects", projectId] })
    queryClient.invalidateQueries({ queryKey: ["project-status", projectId] })
  }

  const deployMutation = useMutation({
    mutationFn: async () => {
      const response = await deployProject({ path: { projectId } })
      if (response.error) throw new Error("Failed to deploy")
      return response.data
    },
    onSuccess: invalidateQueries,
  })

  const stopMutation = useMutation({
    mutationFn: async () => {
      const response = await stopProject({ path: { projectId } })
      if (response.error) throw new Error("Failed to stop")
      return response.data
    },
    onSuccess: invalidateQueries,
  })

  const restartMutation = useMutation({
    mutationFn: async () => {
      const response = await restartProject({ path: { projectId } })
      if (response.error) throw new Error("Failed to restart")
      return response.data
    },
    onSuccess: invalidateQueries,
  })

  const removeMutation = useMutation({
    mutationFn: async () => {
      const response = await removeProjectContainers({ path: { projectId } })
      if (response.error) throw new Error("Failed to remove")
      return response.data
    },
    onSuccess: invalidateQueries,
  })

  const isDeploying = statusName === "Deploying"
  const isRunning = statusName === "Running" || statusName === "PartiallyRunning"
  const isStopped = statusName === "Stopped"
  const isNotDeployed = statusName === "NotDeployed"
  const hasError = statusName === "Error"

  const isAnyMutationPending =
    deployMutation.isPending ||
    stopMutation.isPending ||
    restartMutation.isPending ||
    removeMutation.isPending

  return (
    <ButtonGroup>
      {/* Deploy / Redeploy button */}
      <Button
        onClick={() => deployMutation.mutate()}
        disabled={!hasComposeFile || isDeploying || isAnyMutationPending}
        variant={isRunning ? "outline" : "default"}
        size="sm"
      >
        {deployMutation.isPending ? (
          <Loader2 className="h-4 w-4 mr-2 animate-spin" />
        ) : (
          <Play className="h-4 w-4 mr-2" />
        )}
        {isRunning || isStopped || hasError ? "Redeploy" : "Deploy"}
      </Button>

      {/* Stop button - only show when running */}
      {(isRunning || isDeploying) && (
        <Button
          variant="outline"
          size="sm"
          onClick={() => stopMutation.mutate()}
          disabled={isAnyMutationPending}
        >
          {stopMutation.isPending ? (
            <Loader2 className="h-4 w-4 mr-2 animate-spin" />
          ) : (
            <Square className="h-4 w-4 mr-2" />
          )}
          Stop
        </Button>
      )}

      {/* Restart button - only show when running or stopped */}
      {(isRunning || isStopped) && (
        <Button
          variant="outline"
          size="sm"
          onClick={() => restartMutation.mutate()}
          disabled={isAnyMutationPending}
        >
          {restartMutation.isPending ? (
            <Loader2 className="h-4 w-4 mr-2 animate-spin" />
          ) : (
            <RotateCw className="h-4 w-4 mr-2" />
          )}
          Restart
        </Button>
      )}

      {/* Separator before destructive action */}
      {!isNotDeployed && <ButtonGroupSeparator />}

      {/* Remove button - show when deployed */}
      {!isNotDeployed && (
        <Button
          variant="destructive"
          size="sm"
          onClick={() => removeMutation.mutate()}
          disabled={isAnyMutationPending}
        >
          {removeMutation.isPending ? (
            <Loader2 className="h-4 w-4 mr-2 animate-spin" />
          ) : (
            <Trash2 className="h-4 w-4 mr-2" />
          )}
          Remove
        </Button>
      )}
    </ButtonGroup>
  )
}
