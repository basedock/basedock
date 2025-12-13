import { createFileRoute, Outlet, useNavigate, Link, useLocation, useParams, useRouter } from "@tanstack/react-router"
import { getProjectById, getProjectBySlug, deleteEnvironment } from "@/api/sdk.gen"
import type { ProjectDto } from "@/api/types.gen"
import { Button } from "@/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { CreateEnvironmentDialog } from "@/components/create-environment-dialog"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog"
import { Settings, ChevronDown, Plus, Trash2 } from "lucide-react"
import { useEffect, useState } from "react"
import { useMutation } from "@tanstack/react-query"

type LoaderData = {
  name: string
  project: ProjectDto
  defaultEnvSlug: string | null
}

export const Route = createFileRoute("/_dashboard/projects/$slug")({
  loader: async ({ params }): Promise<LoaderData> => {
    // Check if it's a GUID (backwards compatibility) or a slug
    const isGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(params.slug)

    const projectResponse = isGuid
      ? await getProjectById({ path: { id: params.slug } })
      : await getProjectBySlug({ path: { slug: params.slug } })

    if (projectResponse.error) throw new Error("Project not found")
    const project = projectResponse.data as ProjectDto

    // Find default environment from project data
    const defaultEnv = project.environments.find(e => e.isDefault) || project.environments[0]

    return {
      name: project.name,
      project,
      defaultEnvSlug: defaultEnv?.slug || null
    }
  },
  head: ({ loaderData }) => ({
    meta: [{ title: `${loaderData?.project?.name ?? 'Project'} - Basedock` }],
  }),
  beforeLoad: () => ({
    getTitle: () => "Project",
  }),
  component: ProjectLayout,
})

function ProjectLayout() {
  const { project, defaultEnvSlug } = Route.useLoaderData()
  const navigate = useNavigate()
  const location = useLocation()
  const { env } = useParams({ strict: false })
  const currentEnv = project.environments.find(e => e.slug === env)
  const [createEnvDialogOpen, setCreateEnvDialogOpen] = useState(false)
  const [envToDelete, setEnvToDelete] = useState<{ slug: string; name: string } | null>(null)
  const router = useRouter()

  const deleteMutation = useMutation({
    mutationFn: async (envSlug: string) => {
      const response = await deleteEnvironment({
        path: { projectSlug: project.slug, envSlug }
      })
      if (response.error) throw new Error("Failed to delete environment")
      return response.data
    },
    onSuccess: (_, deletedEnvSlug) => {
      // Re-run the route loader to refresh project data
      router.invalidate()
      // If we deleted the current environment, navigate to default
      if (deletedEnvSlug === env && defaultEnvSlug && deletedEnvSlug !== defaultEnvSlug) {
        navigate({
          to: "/projects/$slug/$env",
          params: { slug: project.slug, env: defaultEnvSlug },
          replace: true
        })
      }
    }
  })

  // Check if we're at the base project URL (no env selected)
  const isBaseProjectUrl = location.pathname === `/projects/${project.slug}` ||
                          location.pathname === `/projects/${project.slug}/`

  // Redirect to default environment if at base project URL
  useEffect(() => {
    if (isBaseProjectUrl && defaultEnvSlug) {
      navigate({
        to: "/projects/$slug/$env",
        params: { slug: project.slug, env: defaultEnvSlug },
        replace: true
      })
    }
  }, [isBaseProjectUrl, defaultEnvSlug, project.slug, navigate])

  // Show nothing while redirecting
  if (isBaseProjectUrl && defaultEnvSlug) {
    return null
  }

  return (
    <div className="flex flex-1 flex-col">
      {/* Project Header */}
      <div className="flex items-center justify-between gap-4 px-6 py-4">
        <div className="flex items-center gap-2">
          <span className="text-xl font-semibold">{project.name}</span>
          {currentEnv && (
            <>
              <span className="text-muted-foreground">/</span>
              <DropdownMenu>
                <DropdownMenuTrigger className="flex items-center gap-1 text-lg">
                  {currentEnv.name}
                  <ChevronDown className="h-4 w-4" />
                </DropdownMenuTrigger>
                <DropdownMenuContent align="start">
                  {project.environments.map((e) => (
                    <DropdownMenuItem key={e.id} asChild>
                      <div className="flex items-center justify-between gap-4 w-full">
                        <Link
                          to="/projects/$slug/$env"
                          params={{ slug: project.slug, env: e.slug }}
                          className="flex-1"
                        >
                          {e.name}
                        </Link>
                        {!e.isDefault && (
                          <button
                            type="button"
                            className="p-1 rounded hover:bg-destructive/10"
                            onClick={(event) => {
                              event.preventDefault()
                              event.stopPropagation()
                              setEnvToDelete({ slug: e.slug, name: e.name })
                            }}
                          >
                            <Trash2 className="h-4 w-4 text-muted-foreground hover:text-destructive" />
                          </button>
                        )}
                      </div>
                    </DropdownMenuItem>
                  ))}
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={() => setCreateEnvDialogOpen(true)}>
                    <Plus className="mr-2 h-4 w-4" />
                    New Environment
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
              <CreateEnvironmentDialog
                projectSlug={project.slug}
                open={createEnvDialogOpen}
                onOpenChange={setCreateEnvDialogOpen}
                onSuccess={() => router.invalidate()}
              />
              <AlertDialog open={!!envToDelete} onOpenChange={(open) => !open && setEnvToDelete(null)}>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>Delete Environment</AlertDialogTitle>
                    <AlertDialogDescription>
                      Are you sure you want to delete "{envToDelete?.name}"? This action cannot be undone.
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>Cancel</AlertDialogCancel>
                    <AlertDialogAction
                      onClick={() => {
                        if (envToDelete) {
                          deleteMutation.mutate(envToDelete.slug)
                        }
                      }}
                    >
                      Delete
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            </>
          )}
        </div>
        <Button variant="outline" size="sm" asChild>
          <Link to="/projects/$slug/settings" params={{ slug: project.slug }}>
            <Settings className="mr-2 h-4 w-4" />
            Project Settings
          </Link>
        </Button>
      </div>

      {/* Nested content */}
      <Outlet />
    </div>
  )
}
