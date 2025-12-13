import { useState } from "react"
import type { ConfigDto } from "@/api/types.gen"
import { getConfigs, createConfig, updateConfig, deleteConfig } from "@/api/sdk.gen"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
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
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Switch } from "@/components/ui/switch"
import { Badge } from "@/components/ui/badge"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
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
import { toast } from "sonner"
import { Plus, FileText, Loader2, Trash2, Pencil } from "lucide-react"

interface ConfigsTabProps {
  projectSlug: string
  envSlug: string
  isAdmin: boolean
}

export function ConfigsTab({ projectSlug, envSlug, isAdmin }: ConfigsTabProps) {
  const queryClient = useQueryClient()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingConfig, setEditingConfig] = useState<ConfigDto | null>(null)
  const [deleteConfigId, setDeleteConfigId] = useState<string | null>(null)

  // Form state
  const [name, setName] = useState("")
  const [content, setContent] = useState("")
  const [filePath, setFilePath] = useState("")
  const [external, setExternal] = useState(false)
  const [externalName, setExternalName] = useState("")

  const { data: configs = [], isLoading } = useQuery({
    queryKey: ["configs", projectSlug, envSlug],
    queryFn: async () => {
      const response = await getConfigs({ path: { projectSlug, envSlug } })
      if (response.error) throw new Error("Failed to fetch configs")
      return response.data as ConfigDto[]
    },
  })

  const createMutation = useMutation({
    mutationFn: async () => {
      const response = await createConfig({
        path: { projectSlug, envSlug },
        body: {
          name,
          content: content || null,
          filePath: filePath || null,
          external,
          externalName: external ? externalName || null : null,
        },
      })
      if (response.error) throw new Error("Failed to create config")
      return response.data
    },
    onSuccess: () => {
      toast.success("Config created")
      queryClient.invalidateQueries({ queryKey: ["configs", projectSlug, envSlug] })
      resetForm()
      setDialogOpen(false)
    },
    onError: () => {
      toast.error("Failed to create config")
    },
  })

  const updateMutation = useMutation({
    mutationFn: async (configId: string) => {
      const response = await updateConfig({
        path: { projectSlug, envSlug, configId },
        body: {
          content: content || null,
          filePath: filePath || null,
          external,
          externalName: external ? externalName || null : null,
        },
      })
      if (response.error) throw new Error("Failed to update config")
      return response.data
    },
    onSuccess: () => {
      toast.success("Config updated")
      queryClient.invalidateQueries({ queryKey: ["configs", projectSlug, envSlug] })
      resetForm()
      setEditingConfig(null)
      setDialogOpen(false)
    },
    onError: () => {
      toast.error("Failed to update config")
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (configId: string) => {
      const response = await deleteConfig({
        path: { projectSlug, envSlug, configId },
      })
      if (response.error) throw new Error("Failed to delete config")
    },
    onSuccess: () => {
      toast.success("Config deleted")
      queryClient.invalidateQueries({ queryKey: ["configs", projectSlug, envSlug] })
      setDeleteConfigId(null)
    },
    onError: () => {
      toast.error("Failed to delete config")
    },
  })

  const resetForm = () => {
    setName("")
    setContent("")
    setFilePath("")
    setExternal(false)
    setExternalName("")
  }

  const openEditDialog = (config: ConfigDto) => {
    setEditingConfig(config)
    setName(config.name)
    setContent(config.content ?? "")
    setFilePath(config.filePath ?? "")
    setExternal(config.external)
    setExternalName(config.externalName ?? "")
    setDialogOpen(true)
  }

  const handleDialogClose = (open: boolean) => {
    if (!open) {
      resetForm()
      setEditingConfig(null)
    }
    setDialogOpen(open)
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!editingConfig && !name.trim()) {
      toast.error("Config name is required")
      return
    }
    if (editingConfig) {
      updateMutation.mutate(editingConfig.id)
    } else {
      createMutation.mutate()
    }
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    )
  }

  return (
    <>
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Configs</CardTitle>
              <CardDescription>
                Configuration files for services
              </CardDescription>
            </div>
            {isAdmin && (
              <Button onClick={() => setDialogOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Add Config
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {configs.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <FileText className="h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-medium mb-2">No configs</h3>
              <p className="text-muted-foreground max-w-md mb-4">
                Create configuration files that can be mounted into services.
              </p>
              {isAdmin && (
                <Button onClick={() => setDialogOpen(true)}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add First Config
                </Button>
              )}
            </div>
          ) : (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Source</TableHead>
                    <TableHead>Type</TableHead>
                    {isAdmin && <TableHead className="w-[100px]">Actions</TableHead>}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {configs.map((config) => (
                    <TableRow key={config.id}>
                      <TableCell className="font-medium">
                        <div className="flex items-center gap-2">
                          <FileText className="h-4 w-4 text-muted-foreground" />
                          {config.name}
                        </div>
                      </TableCell>
                      <TableCell>
                        {config.filePath ? (
                          <span className="text-sm text-muted-foreground font-mono">
                            {config.filePath}
                          </span>
                        ) : config.content ? (
                          <span className="text-sm text-muted-foreground">
                            Inline content
                          </span>
                        ) : (
                          <span className="text-sm text-muted-foreground">-</span>
                        )}
                      </TableCell>
                      <TableCell>
                        {config.external ? (
                          <Badge variant="secondary">External</Badge>
                        ) : (
                          <Badge variant="outline">Managed</Badge>
                        )}
                      </TableCell>
                      {isAdmin && (
                        <TableCell>
                          <div className="flex gap-1">
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => openEditDialog(config)}
                            >
                              <Pencil className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => setDeleteConfigId(config.id)}
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </div>
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

      {/* Create/Edit Config Dialog */}
      <Dialog open={dialogOpen} onOpenChange={handleDialogClose}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>{editingConfig ? "Edit Config" : "Create Config"}</DialogTitle>
            <DialogDescription>
              {editingConfig
                ? "Update the configuration content"
                : "Add a new configuration file for services"}
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSubmit} className="space-y-4">
            {!editingConfig && (
              <div className="space-y-2">
                <Label htmlFor="name">Name *</Label>
                <Input
                  id="name"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="my-config"
                />
              </div>
            )}

            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>External Config</Label>
                <p className="text-xs text-muted-foreground">
                  Use an existing external config
                </p>
              </div>
              <Switch checked={external} onCheckedChange={setExternal} />
            </div>

            {external ? (
              <div className="space-y-2">
                <Label htmlFor="externalName">External Name</Label>
                <Input
                  id="externalName"
                  value={externalName}
                  onChange={(e) => setExternalName(e.target.value)}
                  placeholder="existing-config-name"
                />
              </div>
            ) : (
              <>
                <div className="space-y-2">
                  <Label htmlFor="content">Content</Label>
                  <Textarea
                    id="content"
                    value={content}
                    onChange={(e) => setContent(e.target.value)}
                    placeholder="Configuration file content..."
                    rows={10}
                    className="font-mono text-sm"
                  />
                  <p className="text-xs text-muted-foreground">
                    Inline content for the configuration file
                  </p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="filePath">File Path (alternative)</Label>
                  <Input
                    id="filePath"
                    value={filePath}
                    onChange={(e) => setFilePath(e.target.value)}
                    placeholder="./config/my-config.conf"
                  />
                  <p className="text-xs text-muted-foreground">
                    Path to a config file (relative to compose file)
                  </p>
                </div>
              </>
            )}

            <div className="flex justify-end gap-2 pt-4">
              <Button type="button" variant="outline" onClick={() => handleDialogClose(false)}>
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={createMutation.isPending || updateMutation.isPending}
              >
                {(createMutation.isPending || updateMutation.isPending) && (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                {editingConfig ? "Update" : "Create"}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={!!deleteConfigId} onOpenChange={() => setDeleteConfigId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Config</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this config? Services using this
              config may fail to start.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => deleteConfigId && deleteMutation.mutate(deleteConfigId)}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {deleteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  )
}
