import { createFileRoute, Outlet, redirect, useNavigate, useRouterState } from "@tanstack/react-router"
import { useEffect, type ReactNode } from "react"
import { useAuth } from "@/contexts/auth-context"
import { AppSidebar } from "@/components/app-sidebar"
import { DashboardHeader } from "@/components/dashboard-header"
import {
  SidebarInset,
  SidebarProvider,
} from "@/components/ui/sidebar"
import type { BreadcrumbItem } from "@/components/dashboard-header"

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
  const { user, isAuthenticated, isLoading } = useAuth()
  const navigate = useNavigate()
  const routerState = useRouterState()
  const matches = routerState.matches

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      navigate({ to: '/login', search: { redirect: undefined } })
    }
  }, [isLoading, isAuthenticated, navigate])

  // Build breadcrumbs from route context
  const rawBreadcrumbs = matches
    .filter((match) => (match.context as { getTitle?: () => string }).getTitle)
    .map((match) => ({
      label: (match.loaderData as { name?: string })?.name ?? (match.context as { getTitle: () => string }).getTitle(),
      href: match.pathname,
    }))

  // Dedupe consecutive breadcrumbs with the same label (e.g., layout + index both showing "Projects")
  const breadcrumbs: BreadcrumbItem[] = [
    { label: "Dashboard", href: "/" },
    ...rawBreadcrumbs.filter((crumb, index, arr) =>
      index === 0 || crumb.label !== arr[index - 1].label
    ),
  ]

  // Remove href from last breadcrumb (current page)
  if (breadcrumbs.length > 0) {
    breadcrumbs[breadcrumbs.length - 1] = {
      label: breadcrumbs[breadcrumbs.length - 1].label,
    }
  }

  // Get actions from deepest matching route
  const matchWithActions = [...matches].reverse().find(
    (m) => (m.context as { getActions?: unknown }).getActions
  )
  const actions = (matchWithActions?.context as { getActions?: (props: { isAdmin: boolean }) => ReactNode })
    ?.getActions?.({ isAdmin: user?.isAdmin ?? false })

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
        <DashboardHeader breadcrumbs={breadcrumbs} actions={actions} />
        <Outlet />
      </SidebarInset>
    </SidebarProvider>
  )
}
