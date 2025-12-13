import { useState } from "react"
import type { SecretDto } from "@/api/types.gen"
import { getSecrets, createSecret, updateSecret, deleteSecret } from "@/api/sdk.gen"
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
import { Plus, Key, Loader2, Trash2, Pencil, Eye, EyeOff } from "lucide-react"

interface SecretsTabProps {
  projectSlug: string
  envSlug: string
  isAdmin: boolean
}

export function SecretsTab({ projectSlug, envSlug, isAdmin }: SecretsTabProps) {
  const queryClient = useQueryClient()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingSecret, setEditingSecret] = useState<SecretDto | null>(null)
  const [deleteSecretId, setDeleteSecretId] = useState<string | null>(null)
  const [showContent, setShowContent] = useState(false)

  // Form state
  const [name, setName] = useState("")
  const [content, setContent] = useState("")
  const [filePath, setFilePath] = useState("")
  const [external, setExternal] = useState(false)
  const [externalName, setExternalName] = useState("")

  const { data: secrets = [], isLoading } = useQuery({
    queryKey: ["secrets", projectSlug, envSlug],
    queryFn: async () => {
      const response = await getSecrets({ path: { projectSlug, envSlug } })
      if (response.error) throw new Error("Failed to fetch secrets")
      return response.data as SecretDto[]
    },
  })

  const createMutation = useMutation({
    mutationFn: async () => {
      const response = await createSecret({
        path: { projectSlug, envSlug },
        body: {
          name,
          content: content || null,
          filePath: filePath || null,
          external,
          externalName: external ? externalName || null : null,
        },
      })
      if (response.error) throw new Error("Failed to create secret")
      return response.data
    },
    onSuccess: () => {
      toast.success("Secret created")
      queryClient.invalidateQueries({ queryKey: ["secrets", projectSlug, envSlug] })
      resetForm()
      setDialogOpen(false)
    },
    onError: () => {
      toast.error("Failed to create secret")
    },
  })

  const updateMutation = useMutation({
    mutationFn: async (secretId: string) => {
      const response = await updateSecret({
        path: { projectSlug, envSlug, secretId },
        body: {
          content: content || null,
          filePath: filePath || null,
          external,
          externalName: external ? externalName || null : null,
        },
      })
      if (response.error) throw new Error("Failed to update secret")
      return response.data
    },
    onSuccess: () => {
      toast.success("Secret updated")
      queryClient.invalidateQueries({ queryKey: ["secrets", projectSlug, envSlug] })
      resetForm()
      setEditingSecret(null)
      setDialogOpen(false)
    },
    onError: () => {
      toast.error("Failed to update secret")
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (secretId: string) => {
      const response = await deleteSecret({
        path: { projectSlug, envSlug, secretId },
      })
      if (response.error) throw new Error("Failed to delete secret")
    },
    onSuccess: () => {
      toast.success("Secret deleted")
      queryClient.invalidateQueries({ queryKey: ["secrets", projectSlug, envSlug] })
      setDeleteSecretId(null)
    },
    onError: () => {
      toast.error("Failed to delete secret")
    },
  })

  const resetForm = () => {
    setName("")
    setContent("")
    setFilePath("")
    setExternal(false)
    setExternalName("")
    setShowContent(false)
  }

  const openEditDialog = (secret: SecretDto) => {
    setEditingSecret(secret)
    setName(secret.name)
    setContent("") // Don't show existing content for security
    setFilePath(secret.filePath ?? "")
    setExternal(secret.external)
    setExternalName(secret.externalName ?? "")
    setShowContent(false)
    setDialogOpen(true)
  }

  const handleDialogClose = (open: boolean) => {
    if (!open) {
      resetForm()
      setEditingSecret(null)
    }
    setDialogOpen(open)
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!editingSecret && !name.trim()) {
      toast.error("Secret name is required")
      return
    }
    if (editingSecret) {
      updateMutation.mutate(editingSecret.id)
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
              <CardTitle>Secrets</CardTitle>
              <CardDescription>
                Sensitive data for services (mounted at /run/secrets)
              </CardDescription>
            </div>
            {isAdmin && (
              <Button onClick={() => setDialogOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Add Secret
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {secrets.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Key className="h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-medium mb-2">No secrets</h3>
              <p className="text-muted-foreground max-w-md mb-4">
                Create secrets for sensitive data like passwords and API keys.
              </p>
              {isAdmin && (
                <Button onClick={() => setDialogOpen(true)}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add First Secret
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
                  {secrets.map((secret) => (
                    <TableRow key={secret.id}>
                      <TableCell className="font-medium">
                        <div className="flex items-center gap-2">
                          <Key className="h-4 w-4 text-muted-foreground" />
                          {secret.name}
                        </div>
                      </TableCell>
                      <TableCell>
                        {secret.filePath ? (
                          <span className="text-sm text-muted-foreground font-mono">
                            {secret.filePath}
                          </span>
                        ) : secret.hasContent ? (
                          <span className="text-sm text-muted-foreground">
                            Inline content
                          </span>
                        ) : (
                          <span className="text-sm text-muted-foreground">-</span>
                        )}
                      </TableCell>
                      <TableCell>
                        {secret.external ? (
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
                              onClick={() => openEditDialog(secret)}
                            >
                              <Pencil className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => setDeleteSecretId(secret.id)}
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

      {/* Create/Edit Secret Dialog */}
      <Dialog open={dialogOpen} onOpenChange={handleDialogClose}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>{editingSecret ? "Edit Secret" : "Create Secret"}</DialogTitle>
            <DialogDescription>
              {editingSecret
                ? "Update the secret value (leave empty to keep existing)"
                : "Add a new secret for services"}
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSubmit} className="space-y-4">
            {!editingSecret && (
              <div className="space-y-2">
                <Label htmlFor="name">Name *</Label>
                <Input
                  id="name"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="my-secret"
                />
              </div>
            )}

            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>External Secret</Label>
                <p className="text-xs text-muted-foreground">
                  Use an existing external secret
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
                  placeholder="existing-secret-name"
                />
              </div>
            ) : (
              <>
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <Label htmlFor="content">Content</Label>
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      onClick={() => setShowContent(!showContent)}
                    >
                      {showContent ? (
                        <EyeOff className="h-4 w-4" />
                      ) : (
                        <Eye className="h-4 w-4" />
                      )}
                    </Button>
                  </div>
                  {showContent ? (
                    <Textarea
                      id="content"
                      value={content}
                      onChange={(e) => setContent(e.target.value)}
                      placeholder={editingSecret ? "Enter new value to update..." : "Secret value..."}
                      rows={4}
                      className="font-mono text-sm"
                    />
                  ) : (
                    <Input
                      id="content"
                      type="password"
                      value={content}
                      onChange={(e) => setContent(e.target.value)}
                      placeholder={editingSecret ? "Enter new value to update..." : "Secret value..."}
                    />
                  )}
                  <p className="text-xs text-muted-foreground">
                    {editingSecret
                      ? "Leave empty to keep the existing value"
                      : "The secret value (will be stored securely)"}
                  </p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="filePath">File Path (alternative)</Label>
                  <Input
                    id="filePath"
                    value={filePath}
                    onChange={(e) => setFilePath(e.target.value)}
                    placeholder="./secrets/my-secret"
                  />
                  <p className="text-xs text-muted-foreground">
                    Path to a secret file (relative to compose file)
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
                {editingSecret ? "Update" : "Create"}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={!!deleteSecretId} onOpenChange={() => setDeleteSecretId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Secret</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this secret? Services using this
              secret may fail to start.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => deleteSecretId && deleteMutation.mutate(deleteSecretId)}
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
