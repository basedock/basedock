import { createFileRoute } from "@tanstack/react-router"
import { getEnvironmentBySlug } from "@/api/sdk.gen"
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
import { Box, Key, Plus, Database, Container, FileCode } from "lucide-react"
import { useState } from "react"
import { useAuth } from "@/contexts/auth-context"

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

function EnvironmentDetailPage() {
  const environment = Route.useLoaderData()
  const [activeTab, setActiveTab] = useState("resources")
  const { user } = useAuth()
  const isAdmin = user?.isAdmin ?? false

  return (
    <div className="flex flex-col gap-6 p-6">
      {/* Environment Info */}
      <div>
        <h2 className="text-2xl font-bold">{environment.name}</h2>
        {environment.description && (
          <p className="text-muted-foreground">{environment.description}</p>
        )}
      </div>

      {/* Tabs */}
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
                  <Button>
                    <Plus className="mr-2 h-4 w-4" />
                    Add Resource
                  </Button>
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
                    <Button>
                      <Plus className="mr-2 h-4 w-4" />
                      Add First Resource
                    </Button>
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
                      {environment.resources.map((resource) => (
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
                              <Button variant="ghost" size="sm">
                                Manage
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
    </div>
  )
}
