import { useState } from "react"
import type { NetworkDto } from "@/api/types.gen"
import { getNetworks, createNetwork, deleteNetwork } from "@/api/sdk.gen"
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
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
import { Plus, Network, Loader2, Trash2 } from "lucide-react"

interface NetworksTabProps {
  projectSlug: string
  envSlug: string
  isAdmin: boolean
}

export function NetworksTab({ projectSlug, envSlug, isAdmin }: NetworksTabProps) {
  const queryClient = useQueryClient()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [deleteNetworkId, setDeleteNetworkId] = useState<string | null>(null)

  // Form state
  const [name, setName] = useState("")
  const [driver, setDriver] = useState("bridge")
  const [internal, setInternal] = useState(false)
  const [attachable, setAttachable] = useState(false)
  const [external, setExternal] = useState(false)
  const [externalName, setExternalName] = useState("")

  const { data: networks = [], isLoading } = useQuery({
    queryKey: ["networks", projectSlug, envSlug],
    queryFn: async () => {
      const response = await getNetworks({ path: { projectSlug, envSlug } })
      if (response.error) throw new Error("Failed to fetch networks")
      return response.data as NetworkDto[]
    },
  })

  const createMutation = useMutation({
    mutationFn: async () => {
      const response = await createNetwork({
        path: { projectSlug, envSlug },
        body: {
          name,
          driver: driver || null,
          driverOpts: null,
          ipamDriver: null,
          ipamConfig: null,
          internal,
          attachable,
          labels: null,
          external,
          externalName: external ? externalName || null : null,
        },
      })
      if (response.error) throw new Error("Failed to create network")
      return response.data
    },
    onSuccess: () => {
      toast.success("Network created")
      queryClient.invalidateQueries({ queryKey: ["networks", projectSlug, envSlug] })
      resetForm()
      setDialogOpen(false)
    },
    onError: () => {
      toast.error("Failed to create network")
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (networkId: string) => {
      const response = await deleteNetwork({
        path: { projectSlug, envSlug, networkId },
      })
      if (response.error) throw new Error("Failed to delete network")
    },
    onSuccess: () => {
      toast.success("Network deleted")
      queryClient.invalidateQueries({ queryKey: ["networks", projectSlug, envSlug] })
      setDeleteNetworkId(null)
    },
    onError: () => {
      toast.error("Failed to delete network")
    },
  })

  const resetForm = () => {
    setName("")
    setDriver("bridge")
    setInternal(false)
    setAttachable(false)
    setExternal(false)
    setExternalName("")
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!name.trim()) {
      toast.error("Network name is required")
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
              <CardTitle>Networks</CardTitle>
              <CardDescription>
                Custom networks for service communication
              </CardDescription>
            </div>
            {isAdmin && (
              <Button onClick={() => setDialogOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Add Network
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {networks.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Network className="h-12 w-12 text-muted-foreground mb-4" />
              <h3 className="text-lg font-medium mb-2">No networks</h3>
              <p className="text-muted-foreground max-w-md mb-4">
                Create custom networks to control how services communicate.
              </p>
              {isAdmin && (
                <Button onClick={() => setDialogOpen(true)}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add First Network
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
                    <TableHead>Properties</TableHead>
                    <TableHead>Type</TableHead>
                    {isAdmin && <TableHead className="w-[80px]">Actions</TableHead>}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {networks.map((network) => (
                    <TableRow key={network.id}>
                      <TableCell className="font-medium">
                        <div className="flex items-center gap-2">
                          <Network className="h-4 w-4 text-muted-foreground" />
                          {network.name}
                        </div>
                      </TableCell>
                      <TableCell>{network.driver || "bridge"}</TableCell>
                      <TableCell>
                        <div className="flex gap-1">
                          {network.internal && (
                            <Badge variant="outline" className="text-xs">Internal</Badge>
                          )}
                          {network.attachable && (
                            <Badge variant="outline" className="text-xs">Attachable</Badge>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        {network.external ? (
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
                            onClick={() => setDeleteNetworkId(network.id)}
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

      {/* Create Network Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Network</DialogTitle>
            <DialogDescription>
              Add a new custom network for service communication
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="my-network"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="driver">Driver</Label>
              <Select value={driver} onValueChange={setDriver}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="bridge">Bridge</SelectItem>
                  <SelectItem value="host">Host</SelectItem>
                  <SelectItem value="overlay">Overlay</SelectItem>
                  <SelectItem value="macvlan">Macvlan</SelectItem>
                  <SelectItem value="none">None</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>Internal Network</Label>
                <p className="text-xs text-muted-foreground">
                  Restrict external access to this network
                </p>
              </div>
              <Switch checked={internal} onCheckedChange={setInternal} />
            </div>

            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>Attachable</Label>
                <p className="text-xs text-muted-foreground">
                  Allow manual container attachment
                </p>
              </div>
              <Switch checked={attachable} onCheckedChange={setAttachable} />
            </div>

            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>External Network</Label>
                <p className="text-xs text-muted-foreground">
                  Use an existing external network
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
                  placeholder="existing-network-name"
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
      <AlertDialog open={!!deleteNetworkId} onOpenChange={() => setDeleteNetworkId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Network</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this network? Services connected
              to this network may lose connectivity.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => deleteNetworkId && deleteMutation.mutate(deleteNetworkId)}
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
