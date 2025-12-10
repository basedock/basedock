import { useEffect, useState, useCallback } from "react"
import * as signalR from "@microsoft/signalr"
import { useQueryClient } from "@tanstack/react-query"
import { authStore } from "@/lib/auth-store"
import type { DeploymentStatusDto } from "@/api/types.gen"

interface UseDeploymentHubOptions {
  projectId: string
  onStatusChange?: (status: DeploymentStatusDto) => void
  onLogUpdate?: (logLine: string) => void
}

export function useDeploymentHub({
  projectId,
  onStatusChange,
  onLogUpdate,
}: UseDeploymentHubOptions) {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null)
  const [isConnected, setIsConnected] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const queryClient = useQueryClient()

  const connect = useCallback(async () => {
    try {
      const baseUrl = import.meta.env.VITE_API_URL || "https://localhost:7073"
      const conn = new signalR.HubConnectionBuilder()
        .withUrl(`${baseUrl}/hubs/deployment`, {
          accessTokenFactory: () => authStore.getAccessToken() ?? "",
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build()

      conn.on("DeploymentStatusChanged", (newStatus: DeploymentStatusDto) => {
        // Invalidate queries to refresh data
        queryClient.invalidateQueries({ queryKey: ["projects", projectId] })
        queryClient.invalidateQueries({ queryKey: ["project-status", projectId] })
        onStatusChange?.(newStatus)
      })

      conn.on("LogUpdate", (logLine: string) => {
        onLogUpdate?.(logLine)
      })

      conn.onclose((err) => {
        setIsConnected(false)
        if (err) {
          setError(err.message)
        }
      })

      conn.onreconnecting((err) => {
        setIsConnected(false)
        if (err) {
          setError(`Reconnecting: ${err.message}`)
        }
      })

      conn.onreconnected(() => {
        setIsConnected(true)
        setError(null)
        // Rejoin the project group after reconnection
        conn.invoke("JoinProjectGroup", projectId)
      })

      await conn.start()
      await conn.invoke("JoinProjectGroup", projectId)
      setConnection(conn)
      setIsConnected(true)
      setError(null)
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to connect")
      setIsConnected(false)
    }
  }, [projectId, queryClient, onStatusChange, onLogUpdate])

  const disconnect = useCallback(async () => {
    if (connection) {
      try {
        await connection.invoke("LeaveProjectGroup", projectId)
        await connection.stop()
      } catch {
        // Ignore errors during disconnect
      }
      setConnection(null)
      setIsConnected(false)
    }
  }, [connection, projectId])

  useEffect(() => {
    connect()

    return () => {
      disconnect()
    }
  }, [projectId]) // Only reconnect when projectId changes

  return {
    isConnected,
    error,
    reconnect: connect,
  }
}
