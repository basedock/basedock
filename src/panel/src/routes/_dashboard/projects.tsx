import { createFileRoute, Outlet } from "@tanstack/react-router"

export const Route = createFileRoute("/_dashboard/projects")({
  head: () => ({
    meta: [{ title: 'Projects - Basedock' }],
  }),
  beforeLoad: () => ({
    getTitle: () => "Projects",
  }),
  component: ProjectsLayout,
})

function ProjectsLayout() {
  return <Outlet />
}
