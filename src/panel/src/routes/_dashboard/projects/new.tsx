import { createFileRoute, redirect } from "@tanstack/react-router"
import { CreateProjectWizard } from "@/components/create-project"

export const Route = createFileRoute("/_dashboard/projects/new")({
  head: () => ({
    meta: [{ title: "Create Project - Basedock" }],
  }),
  beforeLoad: ({ context }) => {
    // Ensure user is admin
    if (!context.auth.isLoading && !context.auth.user?.isAdmin) {
      throw redirect({ to: "/projects" })
    }
    return {
      getTitle: () => "New Project",
    }
  },
  component: CreateProjectPage,
})

function CreateProjectPage() {
  return (
    <div className="flex flex-1 flex-col p-6 pt-0">
      <CreateProjectWizard />
    </div>
  )
}
