import { createFileRoute, Outlet } from "@tanstack/react-router"

export const Route = createFileRoute("/_dashboard/users")({
  head: () => ({
    meta: [{ title: 'Users - Basedock' }],
  }),
  beforeLoad: () => ({
    getTitle: () => "Users",
  }),
  component: UsersLayout,
})

function UsersLayout() {
  return <Outlet />
}
