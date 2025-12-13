import { createFileRoute, useRouter } from "@tanstack/react-router"
import { getEnvironmentBySlug, deployResource, stopResource } from "@/api/sdk.gen"
import type { EnvironmentDetailDto } from "@/api/types.gen"
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs"
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
} from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Box, Key, Plus, Database, Container, FileCode, Layers, Play, Square, Loader2, Package } from "lucide-react"
import { useState } from "react"
import { useAuth } from "@/contexts/auth-context"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { CreateResourceDialog } from "@/components/create-resource-dialog"
import { toast } from "sonner"

export const Route = createFileRoute("/_dashboard/projects/$slug/$env")({
  loader: async ({ params }) => {
    const response = await getEnvironmentBySlug({
      path: { projectSlug: params.slug, envSlug: params.env }
    })
    if (response.error) throw new Error("Environment not found")
    return response.data as EnvironmentDetailDto
  },
  beforeLoad: ({ params }) => ({
    getTitle: () => params.env,
  }),
  component: EnvironmentDetailPage,
})

function getResourceIcon(type: string) {
  switch (type) {
    case "DockerImage":
      return <Container className="h-4 w-4" />
    case "Dockerfile":
      return <FileCode className="h-4 w-4" />
    case "DockerCompose":
      return <FileCode className="h-4 w-4" />
    case "PostgreSQL":
    case "Redis":
      return <Database className="h-4 w-4" />
    case "PreMadeApp":
      return <Package className="h-4 w-4" />
    default:
      return <Box className="h-4 w-4" />
  }
}

function getStatusVariant(status: string): "default" | "secondary" | "destructive" | "outline" {
  switch (status) {
    case "Running":
      return "default"
    case "Deploying":
      return "secondary"
    case "Error":
      return "destructive"
    default:
      return "outline"
  }
}

type ResourceCategory = "application" | "database" | "compose" | "premade"

function EnvironmentDetailPage() {
  const { slug, env } = Route.useParams()
  const router = useRouter()
  const environment = Route.useLoaderData()
  const [activeTab, setActiveTab] = useState("resources")
  const [dialogOpen, setDialogOpen] = useState(false)
  const [selectedCategory, setSelectedCategory] = useState<ResourceCategory | null>(null)
  const [loadingResourceId, setLoadingResourceId] = useState<string | null>(null)
  const { user } = useAuth()
  const isAdmin = user?.isAdmin ?? false

  const openResourceDialog = (category: ResourceCategory) => {
    setSelectedCategory(category)
    setDialogOpen(true)
  }

  const handleDeploy = async (resourceId: string, resourceType: string) => {
    setLoadingResourceId(resourceId)
    try {
      const response = await deployResource({
        path: { projectSlug: slug, envSlug: env, resourceId },
        body: { resourceType },
      })
      if (response.error) {
        toast.error("Failed to deploy resource")
      } else {
        toast.success("Resource deployment started")
        router.invalidate()
      }
    } catch {
      toast.error("Failed to deploy resource")
    } finally {
      setLoadingResourceId(null)
    }
  }

  const handleStop = async (resourceId: string, resourceType: string) => {
    setLoadingResourceId(resourceId)
    try {
      const response = await stopResource({
        path: { projectSlug: slug, envSlug: env, resourceId },
        body: { resourceType },
      })
      if (response.error) {
        toast.error("Failed to stop resource")
      } else {
        toast.success("Resource stopped")
        router.invalidate()
      }
    } catch {
      toast.error("Failed to stop resource")
    } finally {
      setLoadingResourceId(null)
    }
  }

  return (
    <div className="flex flex-col gap-6 p-6">
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="resources">
            <Box className="mr-2 h-4 w-4" />
            Resources ({environment.resources.length})
          </TabsTrigger>
          <TabsTrigger value="variables">
            <Key className="mr-2 h-4 w-4" />
            Variables ({environment.variables.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="resources" className="mt-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Resources</CardTitle>
                  <CardDescription>
                    Applications and databases in this environment
                  </CardDescription>
                </div>
                {isAdmin && (
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button>
                        <Plus className="mr-2 h-4 w-4" />
                        Add Resource
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem onClick={() => openResourceDialog("application")}>
                        <Container className="mr-2 h-4 w-4" />
                        Application
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => openResourceDialog("database")}>
                        <Database className="mr-2 h-4 w-4" />
                        Database
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => openResourceDialog("compose")}>
                        <Layers className="mr-2 h-4 w-4" />
                        Compose
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => openResourceDialog("premade")}>
                        <Package className="mr-2 h-4 w-4" />
                        Pre-made Apps
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                )}
              </div>
            </CardHeader>
            <CardContent>
              {environment.resources.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-12 text-center">
                  <Box className="h-12 w-12 text-muted-foreground mb-4" />
                  <h3 className="text-lg font-medium mb-2">No resources</h3>
                  <p className="text-muted-foreground max-w-md mb-4">
                    Add applications, databases, or services to this environment.
                  </p>
                  {isAdmin && (
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button>
                          <Plus className="mr-2 h-4 w-4" />
                          Add First Resource
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent>
                        <DropdownMenuItem onClick={() => openResourceDialog("application")}>
                          <Container className="mr-2 h-4 w-4" />
                          Application
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => openResourceDialog("database")}>
                          <Database className="mr-2 h-4 w-4" />
                          Database
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => openResourceDialog("compose")}>
                          <Layers className="mr-2 h-4 w-4" />
                          Compose
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => openResourceDialog("premade")}>
                          <Package className="mr-2 h-4 w-4" />
                          Pre-made Apps
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  )}
                </div>
              ) : (
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Name</TableHead>
                        <TableHead>Type</TableHead>
                        <TableHead>Status</TableHead>
                        {isAdmin && <TableHead className="w-[100px]">Actions</TableHead>}
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {environment.resources.map((resource) => {
                        const isLoading = loadingResourceId === resource.id
                        const isRunning = resource.status === "Running"
                        const isStopped = resource.status === "Stopped" || resource.status === "NotDeployed"
                        return (
                          <TableRow key={resource.id}>
                            <TableCell className="font-medium">
                              <div className="flex items-center gap-2">
                                {getResourceIcon(resource.type)}
                                {resource.name}
                              </div>
                            </TableCell>
                            <TableCell>{resource.type}</TableCell>
                            <TableCell>
                              <Badge variant={getStatusVariant(resource.status)}>
                                {resource.status}
                              </Badge>
                            </TableCell>
                            {isAdmin && (
                              <TableCell>
                                <div className="flex items-center gap-1">
                                  {isStopped && (
                                    <Button
                                      variant="ghost"
                                      size="sm"
                                      onClick={() => handleDeploy(resource.id, resource.type)}
                                      disabled={isLoading}
                                    >
                                      {isLoading ? (
                                        <Loader2 className="h-4 w-4 animate-spin" />
                                      ) : (
                                        <Play className="h-4 w-4" />
                                      )}
                                    </Button>
                                  )}
                                  {isRunning && (
                                    <Button
                                      variant="ghost"
                                      size="sm"
                                      onClick={() => handleStop(resource.id, resource.type)}
                                      disabled={isLoading}
                                    >
                                      {isLoading ? (
                                        <Loader2 className="h-4 w-4 animate-spin" />
                                      ) : (
                                        <Square className="h-4 w-4" />
                                      )}
                                    </Button>
                                  )}
                                </div>
                              </TableCell>
                            )}
                          </TableRow>
                        )
                      })}
                    </TableBody>
                  </Table>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="variables" className="mt-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Environment Variables</CardTitle>
                  <CardDescription>
                    Configuration values for this environment
                  </CardDescription>
                </div>
                {isAdmin && (
                  <Button>
                    <Plus className="mr-2 h-4 w-4" />
                    Add Variable
                  </Button>
                )}
              </div>
            </CardHeader>
            <CardContent>
              {environment.variables.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-12 text-center">
                  <Key className="h-12 w-12 text-muted-foreground mb-4" />
                  <h3 className="text-lg font-medium mb-2">No variables</h3>
                  <p className="text-muted-foreground max-w-md mb-4">
                    Add environment variables to configure your applications.
                  </p>
                  {isAdmin && (
                    <Button>
                      <Plus className="mr-2 h-4 w-4" />
                      Add First Variable
                    </Button>
                  )}
                </div>
              ) : (
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Key</TableHead>
                        <TableHead>Value</TableHead>
                        <TableHead>Type</TableHead>
                        {isAdmin && <TableHead className="w-[100px]">Actions</TableHead>}
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {environment.variables.map((variable) => (
                        <TableRow key={variable.id}>
                          <TableCell className="font-mono text-sm">
                            {variable.key}
                          </TableCell>
                          <TableCell className="font-mono text-sm">
                            {variable.isSecret ? (
                              <span className="text-muted-foreground">••••••••</span>
                            ) : (
                              variable.value
                            )}
                          </TableCell>
                          <TableCell>
                            {variable.isSecret ? (
                              <Badge variant="secondary">Secret</Badge>
                            ) : (
                              <Badge variant="outline">Plain</Badge>
                            )}
                          </TableCell>
                          {isAdmin && (
                            <TableCell>
                              <Button variant="ghost" size="sm">
                                Edit
                              </Button>
                            </TableCell>
                          )}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <CreateResourceDialog
        category={selectedCategory}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
      />
    </div>
  )
}
