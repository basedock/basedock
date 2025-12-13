import { useState } from "react"
import type { ServiceDetailDto, CreateServiceRequest, UpdateServiceRequest } from "@/api/types.gen"
import { createService, updateService } from "@/api/sdk.gen"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Switch } from "@/components/ui/switch"
import { toast } from "sonner"
import { Loader2 } from "lucide-react"

interface ServiceFormDialogProps {
  projectSlug: string
  envSlug: string
  service?: ServiceDetailDto
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
}

export function ServiceFormDialog({
  projectSlug,
  envSlug,
  service,
  open,
  onOpenChange,
  onSuccess,
}: ServiceFormDialogProps) {
  const isEdit = !!service
  const [loading, setLoading] = useState(false)
  const [activeTab, setActiveTab] = useState("basic")

  // Form state
  const [name, setName] = useState(service?.name ?? "")
  const [description, setDescription] = useState(service?.description ?? "")
  const [image, setImage] = useState(service?.image ?? "")
  const [ports, setPorts] = useState(service?.ports ?? "")
  const [environmentVariables, setEnvironmentVariables] = useState(service?.environmentVariables ?? "")
  const [volumes, setVolumes] = useState(service?.volumes ?? "")
  const [restart, setRestart] = useState(service?.restart ?? "unless-stopped")
  const [healthcheckTest, setHealthcheckTest] = useState(service?.healthcheckTest?.join(" ") ?? "")
  const [healthcheckInterval, setHealthcheckInterval] = useState(service?.healthcheckInterval ?? "30s")
  const [healthcheckTimeout, setHealthcheckTimeout] = useState(service?.healthcheckTimeout ?? "10s")
  const [healthcheckRetries, setHealthcheckRetries] = useState(service?.healthcheckRetries ?? 3)
  const [healthcheckDisabled, setHealthcheckDisabled] = useState(service?.healthcheckDisabled ?? false)
  const [cpuLimit, setCpuLimit] = useState(service?.cpuLimit ?? "")
  const [memoryLimit, setMemoryLimit] = useState(service?.memoryLimit ?? "")
  const [replicas, setReplicas] = useState(service?.replicas ?? 1)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!name.trim()) {
      toast.error("Service name is required")
      return
    }

    if (!image.trim()) {
      toast.error("Image is required")
      return
    }

    setLoading(true)

    try {
      const healthTest = healthcheckTest.trim()
        ? healthcheckTest.split(" ").filter(Boolean)
        : null

      if (isEdit && service) {
        const request: UpdateServiceRequest = {
          name,
          description: description || null,
          image: image || null,
          buildContext: null,
          buildDockerfile: null,
          buildArgs: null,
          command: null,
          entrypoint: null,
          workingDir: null,
          user: null,
          ports: ports || null,
          expose: null,
          hostname: null,
          domainname: null,
          dns: null,
          extraHosts: null,
          environmentVariables: environmentVariables || null,
          envFile: null,
          volumes: volumes || null,
          tmpfs: null,
          dependsOn: null,
          links: null,
          healthcheckTest: healthTest,
          healthcheckInterval: healthcheckInterval || null,
          healthcheckTimeout: healthcheckTimeout || null,
          healthcheckRetries: healthcheckRetries,
          healthcheckStartPeriod: null,
          healthcheckDisabled,
          cpuLimit: cpuLimit || null,
          memoryLimit: memoryLimit || null,
          cpuReservation: null,
          memoryReservation: null,
          restart: restart || null,
          stopGracePeriod: null,
          stopSignal: null,
          replicas: Number(replicas),
          labels: null,
        }

        const response = await updateService({
          path: { projectSlug, envSlug, serviceId: service.id },
          body: request,
        })

        if (response.error) {
          toast.error("Failed to update service")
          return
        }

        toast.success("Service updated")
      } else {
        const request: CreateServiceRequest = {
          name,
          description: description || null,
          image: image || null,
          buildContext: null,
          buildDockerfile: null,
          buildArgs: null,
          command: null,
          entrypoint: null,
          workingDir: null,
          user: null,
          ports: ports || null,
          expose: null,
          hostname: null,
          domainname: null,
          dns: null,
          extraHosts: null,
          environmentVariables: environmentVariables || null,
          envFile: null,
          volumes: volumes || null,
          tmpfs: null,
          dependsOn: null,
          links: null,
          healthcheckTest: healthTest,
          healthcheckInterval: healthcheckInterval || null,
          healthcheckTimeout: healthcheckTimeout || null,
          healthcheckRetries: healthcheckRetries,
          healthcheckStartPeriod: null,
          healthcheckDisabled,
          cpuLimit: cpuLimit || null,
          memoryLimit: memoryLimit || null,
          cpuReservation: null,
          memoryReservation: null,
          restart: restart || null,
          stopGracePeriod: null,
          stopSignal: null,
          replicas: Number(replicas),
          labels: null,
        }

        const response = await createService({
          path: { projectSlug, envSlug },
          body: request,
        })

        if (response.error) {
          toast.error("Failed to create service")
          return
        }

        toast.success("Service created")
      }

      onSuccess()
      onOpenChange(false)
    } catch {
      toast.error(isEdit ? "Failed to update service" : "Failed to create service")
    } finally {
      setLoading(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEdit ? "Edit Service" : "Create Service"}</DialogTitle>
          <DialogDescription>
            {isEdit
              ? "Update the service configuration"
              : "Add a new Docker service to this environment"}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-6">
          <Tabs value={activeTab} onValueChange={setActiveTab}>
            <TabsList className="grid w-full grid-cols-4">
              <TabsTrigger value="basic">Basic</TabsTrigger>
              <TabsTrigger value="networking">Networking</TabsTrigger>
              <TabsTrigger value="health">Health</TabsTrigger>
              <TabsTrigger value="resources">Resources</TabsTrigger>
            </TabsList>

            <TabsContent value="basic" className="space-y-4 mt-4">
              <div className="space-y-2">
                <Label htmlFor="name">Name *</Label>
                <Input
                  id="name"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="my-service"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="description">Description</Label>
                <Input
                  id="description"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Service description"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="image">Image *</Label>
                <Input
                  id="image"
                  value={image}
                  onChange={(e) => setImage(e.target.value)}
                  placeholder="nginx:latest"
                />
                <p className="text-xs text-muted-foreground">
                  Docker image to use (e.g., nginx:latest, postgres:16-alpine)
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="env">Environment Variables</Label>
                <Textarea
                  id="env"
                  value={environmentVariables}
                  onChange={(e) => setEnvironmentVariables(e.target.value)}
                  placeholder='{"KEY": "value", "ANOTHER_KEY": "another_value"}'
                  rows={3}
                />
                <p className="text-xs text-muted-foreground">
                  JSON object of environment variables
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="restart">Restart Policy</Label>
                <Select value={restart} onValueChange={setRestart}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="no">No</SelectItem>
                    <SelectItem value="always">Always</SelectItem>
                    <SelectItem value="on-failure">On Failure</SelectItem>
                    <SelectItem value="unless-stopped">Unless Stopped</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </TabsContent>

            <TabsContent value="networking" className="space-y-4 mt-4">
              <div className="space-y-2">
                <Label htmlFor="ports">Ports</Label>
                <Textarea
                  id="ports"
                  value={ports}
                  onChange={(e) => setPorts(e.target.value)}
                  placeholder='[{"host": 8080, "container": 80, "protocol": "tcp"}]'
                  rows={3}
                />
                <p className="text-xs text-muted-foreground">
                  JSON array of port mappings
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="volumes">Volumes</Label>
                <Textarea
                  id="volumes"
                  value={volumes}
                  onChange={(e) => setVolumes(e.target.value)}
                  placeholder='[{"source": "data_vol", "target": "/data", "type": "volume"}]'
                  rows={3}
                />
                <p className="text-xs text-muted-foreground">
                  JSON array of volume mounts
                </p>
              </div>
            </TabsContent>

            <TabsContent value="health" className="space-y-4 mt-4">
              <div className="flex items-center justify-between">
                <div className="space-y-0.5">
                  <Label>Disable Health Check</Label>
                  <p className="text-xs text-muted-foreground">
                    Skip health checks for this service
                  </p>
                </div>
                <Switch
                  checked={healthcheckDisabled}
                  onCheckedChange={setHealthcheckDisabled}
                />
              </div>

              {!healthcheckDisabled && (
                <>
                  <div className="space-y-2">
                    <Label htmlFor="healthTest">Health Check Command</Label>
                    <Input
                      id="healthTest"
                      value={healthcheckTest}
                      onChange={(e) => setHealthcheckTest(e.target.value)}
                      placeholder="CMD curl -f http://localhost/"
                    />
                    <p className="text-xs text-muted-foreground">
                      Command to check service health (space-separated)
                    </p>
                  </div>

                  <div className="grid grid-cols-3 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="interval">Interval</Label>
                      <Input
                        id="interval"
                        value={healthcheckInterval}
                        onChange={(e) => setHealthcheckInterval(e.target.value)}
                        placeholder="30s"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="timeout">Timeout</Label>
                      <Input
                        id="timeout"
                        value={healthcheckTimeout}
                        onChange={(e) => setHealthcheckTimeout(e.target.value)}
                        placeholder="10s"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="retries">Retries</Label>
                      <Input
                        id="retries"
                        type="number"
                        value={healthcheckRetries}
                        onChange={(e) => setHealthcheckRetries(Number(e.target.value))}
                        min={1}
                      />
                    </div>
                  </div>
                </>
              )}
            </TabsContent>

            <TabsContent value="resources" className="space-y-4 mt-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="cpuLimit">CPU Limit</Label>
                  <Input
                    id="cpuLimit"
                    value={cpuLimit}
                    onChange={(e) => setCpuLimit(e.target.value)}
                    placeholder="0.5"
                  />
                  <p className="text-xs text-muted-foreground">
                    Number of CPUs (e.g., 0.5, 1, 2)
                  </p>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="memoryLimit">Memory Limit</Label>
                  <Input
                    id="memoryLimit"
                    value={memoryLimit}
                    onChange={(e) => setMemoryLimit(e.target.value)}
                    placeholder="512M"
                  />
                  <p className="text-xs text-muted-foreground">
                    Memory limit (e.g., 512M, 1G)
                  </p>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="replicas">Replicas</Label>
                <Input
                  id="replicas"
                  type="number"
                  value={replicas}
                  onChange={(e) => setReplicas(Number(e.target.value))}
                  min={1}
                />
              </div>
            </TabsContent>
          </Tabs>

          <div className="flex justify-end gap-2 pt-4 border-t">
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={loading}>
              {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {isEdit ? "Update" : "Create"}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}
