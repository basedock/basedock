import { useState } from "react"
import { useRouter } from "@tanstack/react-router"
import type { ServiceDto, ServiceDetailDto } from "@/api/types.gen"
import { getServices, getServiceById, deleteService } from "@/api/sdk.gen"
import { useQuery } from "@tanstack/react-query"
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu"
import { Container, Plus, MoreHorizontal, Trash2, Edit, LayoutTemplate } from "lucide-react"
import { toast } from "sonner"
import { ServiceFormDialog } from "./service-form-dialog"
import { TemplateDialog } from "@/components/templates/template-dialog"

interface ServicesTabProps {
  projectSlug: string
  envSlug: string
  isAdmin: boolean
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

export function ServicesTab({ projectSlug, envSlug, isAdmin }: ServicesTabProps) {
  const router = useRouter()
  const [createDialogOpen, setCreateDialogOpen] = useState(false)
  const [templateDialogOpen, setTemplateDialogOpen] = useState(false)
  const [editingService, setEditingService] = useState<ServiceDetailDto | null>(null)
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [loadingEditId, setLoadingEditId] = useState<string | null>(null)

  const { data: services = [], refetch } = useQuery({
    queryKey: ["services", projectSlug, envSlug],
    queryFn: async () => {
      const response = await getServices({
        path: { projectSlug, envSlug }
      })
      if (response.error) throw new Error("Failed to fetch services")
      return response.data as ServiceDto[]
    },
  })

  const handleDelete = async (serviceId: string, serviceName: string) => {
    if (!confirm(`Are you sure you want to delete service "${serviceName}"?`)) return

    setDeletingId(serviceId)
    try {
      const response = await deleteService({
        path: { projectSlug, envSlug, serviceId }
      })
      if (response.error) {
        toast.error("Failed to delete service")
      } else {
        toast.success(`Service "${serviceName}" deleted`)
        refetch()
        router.invalidate()
      }
    } catch {
      toast.error("Failed to delete service")
    } finally {
      setDeletingId(null)
    }
  }

  const handleCreated = () => {
    refetch()
    router.invalidate()
  }

  const handleEdit = async (serviceId: string) => {
    setLoadingEditId(serviceId)
    try {
      const response = await getServiceById({
        path: { projectSlug, envSlug, serviceId }
      })
      if (response.error) {
        toast.error("Failed to load service details")
        return
      }
      setEditingService(response.data as ServiceDetailDto)
    } catch {
      toast.error("Failed to load service details")
    } finally {
      setLoadingEditId(null)
    }
  }

  return (
    <>
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Services</CardTitle>
              <CardDescription>
                Docker containers and applications in this environment
              </CardDescription>
            </div>
            {isAdmin && (
              <div className="flex gap-2">
                <Button variant="outline" onClick={() => setTemplateDialogOpen(true)}>
                  <LayoutTemplate className="mr-2 h-4 w-4" />
                  From Template
                </Button>
                <Button onClick={() => setCreateDialogOpen(true)}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Service
                </Button>
              </div>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {services.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Container className="h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-medium mb-2">No services</h3>
              <p className="text-muted-foreground max-w-md mb-4">
                Add services to run containers in this environment.
              </p>
              {isAdmin && (
                <div className="flex gap-2">
                  <Button variant="outline" onClick={() => setTemplateDialogOpen(true)}>
                    <LayoutTemplate className="mr-2 h-4 w-4" />
                    From Template
                  </Button>
                  <Button onClick={() => setCreateDialogOpen(true)}>
                    <Plus className="mr-2 h-4 w-4" />
                    Add Service
                  </Button>
                </div>
              )}
            </div>
          ) : (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Image</TableHead>
                    <TableHead>Status</TableHead>
                    {isAdmin && <TableHead className="w-[70px]"></TableHead>}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {services.map((service) => (
                    <TableRow key={service.id}>
                      <TableCell className="font-medium">
                        <div className="flex items-center gap-2">
                          <Container className="h-4 w-4 text-muted-foreground" />
                          {service.name}
                        </div>
                      </TableCell>
                      <TableCell className="font-mono text-sm text-muted-foreground">
                        {service.image || "Dockerfile"}
                      </TableCell>
                      <TableCell>
                        <Badge variant={getStatusVariant(service.deploymentStatus)}>
                          {service.deploymentStatus}
                        </Badge>
                      </TableCell>
                      {isAdmin && (
                        <TableCell>
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" size="icon" disabled={deletingId === service.id || loadingEditId === service.id}>
                                <MoreHorizontal className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuItem onClick={() => handleEdit(service.id)} disabled={loadingEditId === service.id}>
                                <Edit className="mr-2 h-4 w-4" />
                                {loadingEditId === service.id ? "Loading..." : "Edit"}
                              </DropdownMenuItem>
                              <DropdownMenuSeparator />
                              <DropdownMenuItem
                                className="text-destructive"
                                onClick={() => handleDelete(service.id, service.name)}
                              >
                                <Trash2 className="mr-2 h-4 w-4" />
                                Delete
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
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

      <ServiceFormDialog
        projectSlug={projectSlug}
        envSlug={envSlug}
        open={createDialogOpen}
        onOpenChange={setCreateDialogOpen}
        onSuccess={handleCreated}
      />

      {editingService && (
        <ServiceFormDialog
          projectSlug={projectSlug}
          envSlug={envSlug}
          service={editingService}
          open={!!editingService}
          onOpenChange={(open) => !open && setEditingService(null)}
          onSuccess={handleCreated}
        />
      )}

      <TemplateDialog
        projectSlug={projectSlug}
        envSlug={envSlug}
        open={templateDialogOpen}
        onOpenChange={setTemplateDialogOpen}
        onSuccess={handleCreated}
      />
    </>
  )
}
