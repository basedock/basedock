import { createFileRoute, useRouter } from "@tanstack/react-router"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useAuth } from "@/contexts/auth-context"
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
} from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Trash2, UserPlus, Wifi, WifiOff, MoreHorizontal } from "lucide-react"
import { getProjectById, updateProject, removeProjectMembers, addProjectMembers, getUsers, getProjectDockerStatus, deployProject } from "@/api/sdk.gen"
import type { ProjectDto, UserDto } from "@/api/types.gen"
import { useForm } from "@tanstack/react-form"
import { z } from "zod"
import { Field, FieldLabel, FieldError, FieldGroup } from "@/components/ui/field"
import { useState } from "react"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { DeploymentStatusBadge } from "@/components/deployment/deployment-status-badge"
import { ComposeEditor } from "@/components/compose/compose-editor"
import { LogsViewer } from "@/components/deployment/logs-viewer"
import { useDeploymentHub } from "@/hooks/use-deployment-hub"
import { ProjectStatsGrid } from "@/components/project/project-stats-grid"
import { ProjectQuickActions } from "@/components/project/project-quick-actions"
import { EnhancedContainerList } from "@/components/project/enhanced-container-list"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"

export const Route = createFileRoute("/_dashboard/projects/$id")({
  loader: async ({ params }) => {
    const response = await getProjectById({ path: { id: params.id } })
    if (response.error) throw new Error("Project not found")
    return response.data as ProjectDto
  },
  head: ({ loaderData }) => ({
    meta: [{ title: `${loaderData?.name ?? 'Project'} - Basedock` }],
  }),
  beforeLoad: () => ({
    getTitle: () => "Project",
  }),
  component: ProjectDetailPage,
})

const projectSchema = z.object({
  name: z.string().min(1, "Name is required").max(100, "Name must be 100 characters or less"),
  description: z.string().max(500, "Description must be 500 characters or less"),
})

function ProjectDetailPage() {
  const { id } = Route.useParams()
  const project = Route.useLoaderData()
  const router = useRouter()
  const { user, isAuthenticated } = useAuth()
  const queryClient = useQueryClient()
  const [addMemberDialogOpen, setAddMemberDialogOpen] = useState(false)
  const [selectedUserId, setSelectedUserId] = useState<string>("")
  const [activeTab, setActiveTab] = useState("overview")

  // SignalR connection for real-time updates
  const { isConnected } = useDeploymentHub({ projectId: id })

  // Query for live Docker status
  const { data: dockerStatus } = useQuery({
    queryKey: ["project-status", id],
    queryFn: async () => {
      const response = await getProjectDockerStatus({ path: { projectId: id } })
      if (response.error) return null
      return response.data
    },
    refetchInterval: 30000, // Fallback polling every 30s
  })

  const { data: users } = useQuery({
    queryKey: ["users"],
    queryFn: async () => {
      const response = await getUsers()
      if (response.error) throw new Error("Failed to fetch users")
      return response.data as UserDto[]
    },
    enabled: isAuthenticated && user?.isAdmin,
  })

  const updateMutation = useMutation({
    mutationFn: async (data: { name: string; description?: string }) => {
      const response = await updateProject({
        path: { id },
        body: { name: data.name, description: data.description ?? null },
      })
      if (response.error) throw new Error("Failed to update project")
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projects"] })
      router.invalidate()
    },
  })

  const removeMemberMutation = useMutation({
    mutationFn: async (userId: string) => {
      const response = await removeProjectMembers({
        path: { id },
        body: { userIds: [userId] },
      })
      if (response.error) throw new Error("Failed to remove member")
      return response.data
    },
    onSuccess: () => {
      router.invalidate()
    },
  })

  const addMemberMutation = useMutation({
    mutationFn: async (userId: string) => {
      const response = await addProjectMembers({
        path: { id },
        body: { userIds: [userId] },
      })
      if (response.error) throw new Error("Failed to add member")
      return response.data
    },
    onSuccess: () => {
      router.invalidate()
      setAddMemberDialogOpen(false)
      setSelectedUserId("")
    },
  })

  const deployMutation = useMutation({
    mutationFn: async () => {
      const response = await deployProject({ path: { projectId: id } })
      if (response.error) throw new Error("Failed to deploy project")
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["project-status", id] })
    },
  })

  const form = useForm({
    defaultValues: {
      name: project?.name ?? "",
      description: project?.description ?? "",
    },
    validators: {
      onSubmit: projectSchema,
    },
    onSubmit: async ({ value }) => {
      await updateMutation.mutateAsync({
        name: value.name,
        description: value.description || undefined,
      })
    },
  })

  // Update form when project loads
  if (project && form.state.values.name !== project.name) {
    form.setFieldValue("name", project.name)
    form.setFieldValue("description", project.description ?? "")
  }

  const isAdmin = user?.isAdmin ?? false
  const memberIds = new Set(project.members.map((m) => m.userId))
  const availableUsers = users?.filter((u) => !memberIds.has(u.id)) ?? []

  return (
    <>
      <div className="flex flex-1 flex-col space-y-6 p-6 pt-0">
        {/* Modern Header - No card wrapper */}
        <div className="flex items-start justify-between">
          <div className="space-y-1">
            <div className="flex items-center gap-3">
              <h1 className="text-3xl font-bold tracking-tight">{project.name}</h1>
              <DeploymentStatusBadge status={dockerStatus?.status ?? project.deploymentStatus} />
              <div className="flex items-center gap-2 text-sm">
                {isConnected ? (
                  <>
                    <Wifi className="h-3.5 w-3.5 text-green-500" />
                    <span className="text-green-600 dark:text-green-400">Live</span>
                  </>
                ) : (
                  <>
                    <WifiOff className="h-3.5 w-3.5 text-muted-foreground" />
                    <span className="text-muted-foreground">Connecting...</span>
                  </>
                )}
              </div>
            </div>
            {project.description && (
              <p className="text-muted-foreground text-base">{project.description}</p>
            )}
          </div>

          {/* Admin controls in dropdown */}
          {isAdmin && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" size="icon">
                  <MoreHorizontal className="h-4 w-4" />
                  <span className="sr-only">Project actions</span>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => deployMutation.mutate()}>
                  Deploy
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => setActiveTab('compose')}>
                  Edit Compose File
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setActiveTab('settings')}>
                  Project Settings
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </div>

        {/* Error banner */}
        {dockerStatus?.lastError && (
          <Alert variant="destructive">
            <AlertDescription>{dockerStatus.lastError}</AlertDescription>
          </Alert>
        )}

        {/* Tabs for content organization */}
        <Tabs value={activeTab} onValueChange={setActiveTab} className="flex-1">
          <TabsList className="grid w-full grid-cols-4">
            <TabsTrigger value="overview">Overview</TabsTrigger>
            <TabsTrigger value="compose">Compose</TabsTrigger>
            <TabsTrigger value="logs">Logs</TabsTrigger>
            <TabsTrigger value="settings">Settings</TabsTrigger>
          </TabsList>

          <TabsContent value="overview" className="space-y-6">
            {/* Stats Grid */}
            <ProjectStatsGrid
              project={project}
              dockerStatus={dockerStatus}
            />

            {/* Quick Actions */}
            <ProjectQuickActions
              isAdmin={isAdmin}
              onNavigateToTab={setActiveTab}
              onDeploy={() => deployMutation.mutate()}
              isDeploying={deployMutation.isPending}
            />

            {/* Enhanced Container Display */}
            <EnhancedContainerList
              containers={dockerStatus?.containers ?? []}
              isAdmin={isAdmin}
            />
          </TabsContent>

          <TabsContent value="compose" className="space-y-4">
            {isAdmin ? (
              <Card>
                <CardHeader>
                  <CardTitle>Docker Compose</CardTitle>
                  <CardDescription>
                    Define your project's services using Docker Compose
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <ComposeEditor
                    projectId={id}
                    initialContent={project.composeFileContent}
                  />
                </CardContent>
              </Card>
            ) : (
              <Card>
                <CardContent className="pt-6">
                  <div className="text-center text-muted-foreground">
                    You don't have permission to edit the Docker Compose file.
                  </div>
                </CardContent>
              </Card>
            )}
          </TabsContent>

          <TabsContent value="logs" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Container Logs</CardTitle>
                <CardDescription>View real-time container logs</CardDescription>
              </CardHeader>
              <CardContent>
                <LogsViewer projectId={id} autoRefresh={isConnected} />
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="settings" className="space-y-4">
            {isAdmin && (
              <Card>
                <CardHeader>
                  <CardTitle>Project Details</CardTitle>
                  <CardDescription>Update project information</CardDescription>
                </CardHeader>
                <CardContent>
                  <form
                    onSubmit={(e) => {
                      e.preventDefault()
                      form.handleSubmit()
                    }}
                  >
                    <FieldGroup>
                      <form.Field
                        name="name"
                        children={(field) => {
                          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                          return (
                            <Field data-invalid={isInvalid}>
                              <FieldLabel htmlFor={field.name}>Name</FieldLabel>
                              <Input
                                id={field.name}
                                name={field.name}
                                value={field.state.value}
                                onBlur={field.handleBlur}
                                onChange={(e) => field.handleChange(e.target.value)}
                                aria-invalid={isInvalid}
                                placeholder="Project name"
                              />
                              {isInvalid && <FieldError errors={field.state.meta.errors} />}
                            </Field>
                          )
                        }}
                      />
                      <form.Field
                        name="description"
                        children={(field) => {
                          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                          return (
                            <Field data-invalid={isInvalid}>
                              <FieldLabel htmlFor={field.name}>Description</FieldLabel>
                              <Textarea
                                id={field.name}
                                name={field.name}
                                value={field.state.value}
                                onBlur={field.handleBlur}
                                onChange={(e) => field.handleChange(e.target.value)}
                                aria-invalid={isInvalid}
                                placeholder="Project description (optional)"
                                rows={3}
                              />
                              {isInvalid && <FieldError errors={field.state.meta.errors} />}
                            </Field>
                          )
                        }}
                      />
                    </FieldGroup>
                    <div className="mt-4">
                      <Button type="submit" disabled={updateMutation.isPending}>
                        {updateMutation.isPending ? "Saving..." : "Save Changes"}
                      </Button>
                    </div>
                  </form>
                </CardContent>
              </Card>
            )}

            <Card>
              <CardHeader>
                <CardTitle>Members</CardTitle>
                <CardDescription>
                  {project.members.length} member{project.members.length !== 1 ? "s" : ""}
                </CardDescription>
              </CardHeader>
              <CardContent>
                {isAdmin && availableUsers.length > 0 && (
                  <div className="mb-4">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setAddMemberDialogOpen(true)}
                    >
                      <UserPlus className="mr-2 h-4 w-4" />
                      Add Member
                    </Button>
                  </div>
                )}
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Name</TableHead>
                        <TableHead>Email</TableHead>
                        <TableHead>Joined</TableHead>
                        {isAdmin && <TableHead className="w-[50px]"></TableHead>}
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {project.members.map((member) => (
                        <TableRow key={member.userId}>
                          <TableCell className="font-medium">{member.displayName}</TableCell>
                          <TableCell>{member.email}</TableCell>
                          <TableCell>
                            {new Date(member.joinedAt).toLocaleDateString()}
                          </TableCell>
                          {isAdmin && (
                            <TableCell>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="text-destructive hover:text-destructive"
                                onClick={() => {
                                  if (confirm(`Remove ${member.displayName} from this project?`)) {
                                    removeMemberMutation.mutate(member.userId)
                                  }
                                }}
                                disabled={removeMemberMutation.isPending}
                              >
                                <Trash2 className="h-4 w-4" />
                                <span className="sr-only">Remove</span>
                              </Button>
                            </TableCell>
                          )}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>

      <Dialog open={addMemberDialogOpen} onOpenChange={setAddMemberDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Member</DialogTitle>
          </DialogHeader>
          <div className="py-4">
            <Select value={selectedUserId} onValueChange={setSelectedUserId}>
              <SelectTrigger>
                <SelectValue placeholder="Select a user" />
              </SelectTrigger>
              <SelectContent>
                {availableUsers.map((user) => (
                  <SelectItem key={user.id} value={user.id}>
                    {user.displayName} ({user.email})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setAddMemberDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={() => {
                if (selectedUserId) {
                  addMemberMutation.mutate(selectedUserId)
                }
              }}
              disabled={!selectedUserId || addMemberMutation.isPending}
            >
              {addMemberMutation.isPending ? "Adding..." : "Add"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}
