import { createFileRoute, Outlet, useNavigate, Link, useLocation } from "@tanstack/react-router"
import { getProjectById, getProjectBySlug, getEnvironments } from "@/api/sdk.gen"
import type { ProjectDto, EnvironmentDto } from "@/api/types.gen"
import { Button } from "@/components/ui/button"
import { Settings } from "lucide-react"
import { useEffect } from "react"

type LoaderData = {
  name: string
  project: ProjectDto
  environments: EnvironmentDto[]
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

    // Load environments to find default
    const envsResponse = await getEnvironments({ path: { projectSlug: project.slug } })
    const environments = (envsResponse.data as EnvironmentDto[]) || []
    const defaultEnv = environments.find(e => e.isDefault) || environments[0]

    return {
      name: project.name,
      project,
      environments,
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
      <div className="flex items-center justify-between gap-4 border-b px-6 py-4">
        <span className="text-xl font-semibold">{project.name}</span>
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
