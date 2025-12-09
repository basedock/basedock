import { createFileRoute, Outlet } from "@tanstack/react-router"

export const Route = createFileRoute("/_dashboard/projects")({
  beforeLoad: () => ({
    getTitle: () => "Projects",
  }),
  component: ProjectsLayout,
})

function ProjectsLayout() {
  return <Outlet />
}
