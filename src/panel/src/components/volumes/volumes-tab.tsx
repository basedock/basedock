import { useState } from "react"
import type { VolumeDto } from "@/api/types.gen"
import { getVolumes, createVolume, deleteVolume } from "@/api/sdk.gen"
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
import { Plus, HardDrive, Loader2, Trash2 } from "lucide-react"

interface VolumesTabProps {
  projectSlug: string
  envSlug: string
  isAdmin: boolean
}

export function VolumesTab({ projectSlug, envSlug, isAdmin }: VolumesTabProps) {
  const queryClient = useQueryClient()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [deleteVolumeId, setDeleteVolumeId] = useState<string | null>(null)

  // Form state
  const [name, setName] = useState("")
  const [driver, setDriver] = useState("")
  const [driverOpts, setDriverOpts] = useState("")
  const [labels, setLabels] = useState("")
  const [external, setExternal] = useState(false)
  const [externalName, setExternalName] = useState("")

  const { data: volumes = [], isLoading } = useQuery({
    queryKey: ["volumes", projectSlug, envSlug],
    queryFn: async () => {
      const response = await getVolumes({ path: { projectSlug, envSlug } })
      if (response.error) throw new Error("Failed to fetch volumes")
      return response.data as VolumeDto[]
    },
  })

  const createMutation = useMutation({
    mutationFn: async () => {
      const response = await createVolume({
        path: { projectSlug, envSlug },
        body: {
          name,
          driver: driver || null,
          driverOpts: driverOpts || null,
          labels: labels || null,
          external,
          externalName: external ? externalName || null : null,
        },
      })
      if (response.error) throw new Error("Failed to create volume")
      return response.data
    },
    onSuccess: () => {
      toast.success("Volume created")
      queryClient.invalidateQueries({ queryKey: ["volumes", projectSlug, envSlug] })
      resetForm()
      setDialogOpen(false)
    },
    onError: () => {
      toast.error("Failed to create volume")
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (volumeId: string) => {
      const response = await deleteVolume({
        path: { projectSlug, envSlug, volumeId },
      })
      if (response.error) throw new Error("Failed to delete volume")
    },
    onSuccess: () => {
      toast.success("Volume deleted")
      queryClient.invalidateQueries({ queryKey: ["volumes", projectSlug, envSlug] })
      setDeleteVolumeId(null)
    },
    onError: () => {
      toast.error("Failed to delete volume")
    },
  })

  const resetForm = () => {
    setName("")
    setDriver("")
    setDriverOpts("")
    setLabels("")
    setExternal(false)
    setExternalName("")
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!name.trim()) {
      toast.error("Volume name is required")
      return
    }
    createMutation.mutate()
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
              <CardTitle>Volumes</CardTitle>
              <CardDescription>
                Named volumes for persistent data storage
              </CardDescription>
            </div>
            {isAdmin && (
              <Button onClick={() => setDialogOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Add Volume
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {volumes.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <HardDrive className="h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-medium mb-2">No volumes</h3>
              <p className="text-muted-foreground max-w-md mb-4">
                Create named volumes to persist data across container restarts.
              </p>
              {isAdmin && (
                <Button onClick={() => setDialogOpen(true)}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add First Volume
                </Button>
              )}
            </div>
          ) : (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Driver</TableHead>
                    <TableHead>Type</TableHead>
                    {isAdmin && <TableHead className="w-[80px]">Actions</TableHead>}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {volumes.map((volume) => (
                    <TableRow key={volume.id}>
                      <TableCell className="font-medium">
                        <div className="flex items-center gap-2">
                          <HardDrive className="h-4 w-4 text-muted-foreground" />
                          {volume.name}
                        </div>
                      </TableCell>
                      <TableCell>{volume.driver || "local"}</TableCell>
                      <TableCell>
                        {volume.external ? (
                          <Badge variant="secondary">External</Badge>
                        ) : (
                          <Badge variant="outline">Managed</Badge>
                        )}
                      </TableCell>
                      {isAdmin && (
                        <TableCell>
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => setDeleteVolumeId(volume.id)}
                          >
                            <Trash2 className="h-4 w-4" />
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

      {/* Create Volume Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Volume</DialogTitle>
            <DialogDescription>
              Add a new named volume for persistent storage
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="my-volume"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="driver">Driver</Label>
              <Input
                id="driver"
                value={driver}
                onChange={(e) => setDriver(e.target.value)}
                placeholder="local"
              />
              <p className="text-xs text-muted-foreground">
                Volume driver to use (defaults to local)
              </p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="driverOpts">Driver Options</Label>
              <Input
                id="driverOpts"
                value={driverOpts}
                onChange={(e) => setDriverOpts(e.target.value)}
                placeholder='{"type": "nfs", "o": "addr=..."}'
              />
              <p className="text-xs text-muted-foreground">
                JSON object of driver options
              </p>
            </div>

            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>External Volume</Label>
                <p className="text-xs text-muted-foreground">
                  Use an existing external volume
                </p>
              </div>
              <Switch checked={external} onCheckedChange={setExternal} />
            </div>

            {external && (
              <div className="space-y-2">
                <Label htmlFor="externalName">External Name</Label>
                <Input
                  id="externalName"
                  value={externalName}
                  onChange={(e) => setExternalName(e.target.value)}
                  placeholder="existing-volume-name"
                />
              </div>
            )}

            <div className="flex justify-end gap-2 pt-4">
              <Button type="button" variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={createMutation.isPending}>
                {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Create
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={!!deleteVolumeId} onOpenChange={() => setDeleteVolumeId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Volume</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this volume? This action cannot be undone
              and may result in data loss if the volume contains data.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => deleteVolumeId && deleteMutation.mutate(deleteVolumeId)}
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
