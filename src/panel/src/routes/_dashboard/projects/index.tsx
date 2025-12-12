import { useState } from "react"
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
import { Folder, MoreHorizontal, Pencil, Trash2, Users, Layers } from "lucide-react"
import { getProjects, deleteProject } from "@/api/sdk.gen"
import type { ProjectDto } from "@/api/types.gen"
import { CreateProjectDialog } from "@/components/create-project-dialog"

function CreateProjectButton({ onClick }: { onClick: () => void }) {
  return (
    <Button onClick={onClick}>
      Create Project
    </Button>
  )
}

export const Route = createFileRoute("/_dashboard/projects/")({
  component: ProjectsPage,
})

function ProjectsPage() {
  const { user, isAuthenticated } = useAuth()
  const queryClient = useQueryClient()
  const [createDialogOpen, setCreateDialogOpen] = useState(false)

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
        {isAdmin && (
          <div className="flex justify-end">
            <CreateProjectButton onClick={() => setCreateDialogOpen(true)} />
          </div>
        )}
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
                <CreateProjectButton onClick={() => setCreateDialogOpen(true)} />
              </EmptyContent>
            )}
          </Empty>
        ) : (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {projects.map((project) => {
              const defaultEnv = project.environments.find(e => e.isDefault) || project.environments[0]
              return (
              <Card key={project.id}>
                <CardHeader>
                  <CardTitle>
                    <Link
                      to="/projects/$slug/$env"
                      params={{ slug: project.slug, env: defaultEnv?.slug ?? '' }}
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
                            <Link to="/projects/$slug/settings" params={{ slug: project.slug }}>
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
                  <div className="flex items-center gap-4 text-sm text-muted-foreground">
                    <div className="flex items-center gap-2">
                      <Layers className="h-4 w-4" />
                      <span>{project.environmentCount} environment{project.environmentCount !== 1 ? "s" : ""}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Users className="h-4 w-4" />
                      <span>{project.members.length} member{project.members.length !== 1 ? "s" : ""}</span>
                    </div>
                  </div>
                </CardContent>
              </Card>
            )})}
          </div>
        )}
      </div>
      <CreateProjectDialog
        open={createDialogOpen}
        onOpenChange={setCreateDialogOpen}
      />
    </>
  )
}
