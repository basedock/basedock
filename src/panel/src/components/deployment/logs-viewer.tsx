import { useQuery } from "@tanstack/react-query"
import { Button } from "@/components/ui/button"
import { RefreshCw, Loader2 } from "lucide-react"
import { getProjectLogs } from "@/api/sdk.gen"
import { cn } from "@/lib/utils"

interface LogsViewerProps {
  projectId: string
  serviceName?: string
  tailLines?: number
  autoRefresh?: boolean
}

export function LogsViewer({
  projectId,
  serviceName,
  tailLines = 100,
  autoRefresh = false,
}: LogsViewerProps) {
  const {
    data: logs,
    refetch,
    isLoading,
    isFetching,
    error,
  } = useQuery({
    queryKey: ["project-logs", projectId, serviceName, tailLines],
    queryFn: async () => {
      const response = await getProjectLogs({
        path: { projectId },
        query: { serviceName: serviceName ?? undefined, tailLines },
      })
      if (response.error) throw new Error("Failed to fetch logs")
      return response.data
    },
    refetchInterval: autoRefresh ? 5000 : false,
    staleTime: 0,
  })

  return (
    <div className="space-y-2">
      <div className="flex justify-between items-center">
        <h3 className="font-medium text-sm">Container Logs</h3>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => refetch()}
          disabled={isFetching}
        >
          {isFetching ? (
            <Loader2 className={cn("h-4 w-4 animate-spin")} />
          ) : (
            <RefreshCw className="h-4 w-4" />
          )}
        </Button>
      </div>

      {error && (
        <div className="text-sm text-destructive">
          Failed to load logs: {error.message}
        </div>
      )}

      {isLoading ? (
        <div className="flex items-center justify-center h-48 bg-muted rounded-lg">
          <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
        </div>
      ) : (
        <pre className="bg-muted p-4 rounded-lg overflow-auto max-h-96 text-xs font-mono whitespace-pre-wrap break-all">
          {logs || "No logs available. Deploy the project to see logs."}
        </pre>
      )}
    </div>
  )
}
