import { createFileRoute, Link } from "@tanstack/react-router"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { useAuth } from "@/contexts/auth-context"
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardAction,
} from "@/components/ui/card"
import {
  Empty,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
  EmptyDescription,
  EmptyContent,
} from "@/components/ui/empty"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Button } from "@/components/ui/button"
import { Folder, MoreHorizontal, Pencil, Trash2, Users } from "lucide-react"
import { getProjects, deleteProject } from "@/api/sdk.gen"
import type { ProjectDto } from "@/api/types.gen"
import { CreateProjectDialog } from "@/components/create-project-dialog"
import { useState } from "react"

function CreateProjectButton() {
  const [open, setOpen] = useState(false)
  return (
    <>
      <Button onClick={() => setOpen(true)}>Create Project</Button>
      <CreateProjectDialog open={open} onOpenChange={setOpen} />
    </>
  )
}

export const Route = createFileRoute("/_dashboard/projects/")({
  beforeLoad: () => ({
    getActions: ({ isAdmin }: { isAdmin: boolean }) =>
      isAdmin ? <CreateProjectButton /> : null,
  }),
  component: ProjectsPage,
})

function ProjectsPage() {
  const { user, isAuthenticated } = useAuth()
  const queryClient = useQueryClient()

  const isAdmin = user?.isAdmin ?? false

  const { data: projects, isLoading, error } = useQuery({
    queryKey: ["projects"],
    queryFn: async () => {
      const response = await getProjects()
      if (response.error) throw new Error("Failed to fetch projects")
      return response.data as ProjectDto[]
    },
    enabled: isAuthenticated,
  })

  const deleteMutation = useMutation({
    mutationFn: async (projectId: string) => {
      const response = await deleteProject({ path: { id: projectId } })
      if (response.error) throw new Error("Failed to delete project")
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projects"] })
    },
  })

  if (isLoading) {
    return (
      <div className="flex min-h-svh items-center justify-center">
        <div className="text-muted-foreground">Loading...</div>
      </div>
    )
  }

  return (
    <>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        {error ? (
          <div className="text-destructive">Failed to load projects</div>
        ) : !projects || projects.length === 0 ? (
          <Empty className="border">
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <Folder />
              </EmptyMedia>
              <EmptyTitle>No projects</EmptyTitle>
              <EmptyDescription>
                You are not a member of any projects yet.
              </EmptyDescription>
            </EmptyHeader>
            {isAdmin && (
              <EmptyContent>
                <CreateProjectButton />
              </EmptyContent>
            )}
          </Empty>
        ) : (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {projects.map((project) => (
              <Card key={project.id}>
                <CardHeader>
                  <CardTitle>
                    <Link
                      to="/projects/$id"
                      params={{ id: project.id }}
                      className="hover:underline"
                    >
                      {project.name}
                    </Link>
                  </CardTitle>
                  {project.description && (
                    <CardDescription>{project.description}</CardDescription>
                  )}
                  {isAdmin && (
                    <CardAction>
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="icon">
                            <MoreHorizontal className="h-4 w-4" />
                            <span className="sr-only">Actions</span>
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem asChild>
                            <Link to="/projects/$id" params={{ id: project.id }}>
                              <Pencil className="mr-2 h-4 w-4" />
                              Edit
                            </Link>
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            className="text-destructive"
                            onClick={() => {
                              if (confirm(`Delete project "${project.name}"?`)) {
                                deleteMutation.mutate(project.id)
                              }
                            }}
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </CardAction>
                  )}
                </CardHeader>
                <CardContent>
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Users className="h-4 w-4" />
                    <span>{project.members.length} member{project.members.length !== 1 ? "s" : ""}</span>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>
    </>
  )
}
