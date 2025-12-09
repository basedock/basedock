import { createFileRoute, Outlet, redirect, useNavigate } from "@tanstack/react-router"
import { useEffect } from "react"
import { useAuth } from "@/contexts/auth-context"
import { DashboardHeaderProvider } from "@/contexts/dashboard-header-context"
import { AppSidebar } from "@/components/app-sidebar"
import { DashboardHeader } from "@/components/dashboard-header"
import {
  SidebarInset,
  SidebarProvider,
} from "@/components/ui/sidebar"

export const Route = createFileRoute("/_dashboard")({
  beforeLoad: ({ context, location }) => {
    if (context.auth.isLoading) return

    if (!context.auth.isAuthenticated) {
      throw redirect({
        to: "/login",
        search: { redirect: location.pathname },
      })
    }
  },
  component: AuthenticatedLayout,
})

function AuthenticatedLayout() {
  const { isAuthenticated, isLoading } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      navigate({ to: '/login', search: { redirect: undefined } })
    }
  }, [isLoading, isAuthenticated, navigate])

  if (isLoading) {
    return (
      <div className="flex min-h-svh items-center justify-center">
        <div className="text-muted-foreground">Loading...</div>
      </div>
    )
  }

  if (!isAuthenticated) {
    return null
  }

  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <DashboardHeaderProvider>
          <DashboardHeader />
          <Outlet />
        </DashboardHeaderProvider>
      </SidebarInset>
    </SidebarProvider>
  )
}
