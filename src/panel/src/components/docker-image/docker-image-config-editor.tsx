import { useState, useEffect } from "react"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Checkbox } from "@/components/ui/checkbox"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Field, FieldLabel, FieldGroup } from "@/components/ui/field"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Plus, Trash2, Save } from "lucide-react"
import type { DockerImageConfig, PortMapping, EnvVar, VolumeMapping } from "../create-project/types"
import { useRouter } from "@tanstack/react-router"

interface DockerImageConfigEditorProps {
  projectId: string
  initialConfig: string | null
}

export function DockerImageConfigEditor({
  projectId: _projectId,
  initialConfig,
}: DockerImageConfigEditorProps) {
  const queryClient = useQueryClient()
  const router = useRouter()

  // Parse initial config
  const parseConfig = (configString: string | null): DockerImageConfig | null => {
    if (!configString) return null
    try {
      return JSON.parse(configString) as DockerImageConfig
    } catch {
      return null
    }
  }

  const config = parseConfig(initialConfig)

  // Form state
  const [image, setImage] = useState(config?.image ?? "")
  const [tag, setTag] = useState(config?.tag ?? "latest")
  const [ports, setPorts] = useState<PortMapping[]>(
    config?.ports ?? []
  )
  const [envVars, setEnvVars] = useState<EnvVar[]>(
    config?.environmentVariables
      ? Object.entries(config.environmentVariables).map(([key, value]) => ({ key, value }))
      : []
  )
  const [volumes, setVolumes] = useState<VolumeMapping[]>(
    config?.volumes ?? []
  )
  const [restartPolicy, setRestartPolicy] = useState(config?.restartPolicy ?? "no")
  const [cpuLimit, setCpuLimit] = useState(config?.resourceLimits?.cpuLimit ?? "")
  const [memoryLimit, setMemoryLimit] = useState(config?.resourceLimits?.memoryLimit ?? "")

  // Track if changes have been made
  const [hasChanges, setHasChanges] = useState(false)

  // Mark as changed when any value changes
  useEffect(() => {
    setHasChanges(true)
  }, [image, tag, ports, envVars, volumes, restartPolicy, cpuLimit, memoryLimit])

  // Reset hasChanges when initial config changes
  useEffect(() => {
    setHasChanges(false)
  }, [initialConfig])

  // Build config JSON
  const buildConfig = (): string => {
    const newConfig: DockerImageConfig = {
      image,
      tag: tag || "latest",
    }

    if (ports.length > 0) {
      newConfig.ports = ports.filter((p) => p.containerPort > 0)
    }

    if (envVars.length > 0) {
      const envVarsObj: Record<string, string> = {}
      envVars.filter((e) => e.key).forEach((e) => {
        envVarsObj[e.key] = e.value
      })
      if (Object.keys(envVarsObj).length > 0) {
        newConfig.environmentVariables = envVarsObj
      }
    }

    if (volumes.length > 0) {
      newConfig.volumes = volumes.filter((v) => v.hostPath && v.containerPath)
    }

    if (restartPolicy && restartPolicy !== "no") {
      newConfig.restartPolicy = restartPolicy
    }

    if (cpuLimit || memoryLimit) {
      newConfig.resourceLimits = {}
      if (cpuLimit) newConfig.resourceLimits.cpuLimit = cpuLimit
      if (memoryLimit) newConfig.resourceLimits.memoryLimit = memoryLimit
    }

    return JSON.stringify(newConfig)
  }

  // Save mutation - placeholder until we add the backend endpoint
  const saveMutation = useMutation({
    mutationFn: async () => {
      // TODO: Call updateDockerImageConfig API when available
      // For now, just log the config that would be saved
      const newConfig = buildConfig()
      console.log("Would save config:", newConfig)
      // This will be replaced with actual API call:
      // const response = await updateDockerImageConfig({
      //   path: { projectId },
      //   body: { dockerImageConfig: newConfig }
      // })
      // if (response.error) throw new Error("Failed to save configuration")
      // return response.data
      throw new Error("Backend endpoint not yet implemented")
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projects"] })
      router.invalidate()
      setHasChanges(false)
    },
  })

  // Port helpers
  const addPort = () => {
    setPorts([...ports, { containerPort: 80, hostPort: undefined, protocol: "tcp" }])
  }

  const updatePort = (index: number, updates: Partial<PortMapping>) => {
    const newPorts = [...ports]
    newPorts[index] = { ...newPorts[index], ...updates }
    setPorts(newPorts)
  }

  const removePort = (index: number) => {
    setPorts(ports.filter((_, i) => i !== index))
  }

  // Env var helpers
  const addEnvVar = () => {
    setEnvVars([...envVars, { key: "", value: "" }])
  }

  const updateEnvVar = (index: number, updates: Partial<EnvVar>) => {
    const newEnvVars = [...envVars]
    newEnvVars[index] = { ...newEnvVars[index], ...updates }
    setEnvVars(newEnvVars)
  }

  const removeEnvVar = (index: number) => {
    setEnvVars(envVars.filter((_, i) => i !== index))
  }

  // Volume helpers
  const addVolume = () => {
    setVolumes([...volumes, { hostPath: "", containerPath: "", readOnly: false }])
  }

  const updateVolume = (index: number, updates: Partial<VolumeMapping>) => {
    const newVolumes = [...volumes]
    newVolumes[index] = { ...newVolumes[index], ...updates }
    setVolumes(newVolumes)
  }

  const removeVolume = (index: number) => {
    setVolumes(volumes.filter((_, i) => i !== index))
  }

  return (
    <div className="space-y-6">
      {/* Image Configuration */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Container Image</CardTitle>
        </CardHeader>
        <CardContent>
          <FieldGroup>
            <div className="grid gap-4 sm:grid-cols-3">
              <div className="sm:col-span-2">
                <Field>
                  <FieldLabel>Image</FieldLabel>
                  <Input
                    value={image}
                    onChange={(e) => setImage(e.target.value)}
                    placeholder="nginx, postgres, your-registry/image"
                  />
                </Field>
              </div>
              <Field>
                <FieldLabel>Tag</FieldLabel>
                <Input
                  value={tag}
                  onChange={(e) => setTag(e.target.value)}
                  placeholder="latest"
                />
              </Field>
            </div>
          </FieldGroup>
        </CardContent>
      </Card>

      {/* Ports */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-base">Port Mappings</CardTitle>
          <Button type="button" variant="outline" size="sm" onClick={addPort}>
            <Plus className="mr-1 h-4 w-4" /> Add Port
          </Button>
        </CardHeader>
        <CardContent>
          {ports.length === 0 ? (
            <p className="text-sm text-muted-foreground">No ports configured</p>
          ) : (
            <div className="space-y-3">
              {ports.map((port, index) => (
                <div key={index} className="flex items-center gap-3">
                  <Input
                    type="number"
                    value={port.hostPort ?? ""}
                    onChange={(e) =>
                      updatePort(index, {
                        hostPort: e.target.value
                          ? parseInt(e.target.value)
                          : undefined,
                      })
                    }
                    placeholder="Host"
                    className="w-24"
                  />
                  <span className="text-muted-foreground">:</span>
                  <Input
                    type="number"
                    value={port.containerPort}
                    onChange={(e) =>
                      updatePort(index, {
                        containerPort: parseInt(e.target.value) || 0,
                      })
                    }
                    placeholder="Container"
                    className="w-24"
                  />
                  <Select
                    value={port.protocol}
                    onValueChange={(value) =>
                      updatePort(index, { protocol: value })
                    }
                  >
                    <SelectTrigger className="w-24">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="tcp">TCP</SelectItem>
                      <SelectItem value="udp">UDP</SelectItem>
                    </SelectContent>
                  </Select>
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    onClick={() => removePort(index)}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Environment Variables */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-base">Environment Variables</CardTitle>
          <Button type="button" variant="outline" size="sm" onClick={addEnvVar}>
            <Plus className="mr-1 h-4 w-4" /> Add Variable
          </Button>
        </CardHeader>
        <CardContent>
          {envVars.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No environment variables
            </p>
          ) : (
            <div className="space-y-3">
              {envVars.map((envVar, index) => (
                <div key={index} className="flex items-center gap-3">
                  <Input
                    value={envVar.key}
                    onChange={(e) =>
                      updateEnvVar(index, { key: e.target.value })
                    }
                    placeholder="KEY"
                    className="flex-1"
                  />
                  <span className="text-muted-foreground">=</span>
                  <Input
                    value={envVar.value}
                    onChange={(e) =>
                      updateEnvVar(index, { value: e.target.value })
                    }
                    placeholder="value"
                    className="flex-1"
                  />
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    onClick={() => removeEnvVar(index)}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Volumes */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-base">Volumes</CardTitle>
          <Button type="button" variant="outline" size="sm" onClick={addVolume}>
            <Plus className="mr-1 h-4 w-4" /> Add Volume
          </Button>
        </CardHeader>
        <CardContent>
          {volumes.length === 0 ? (
            <p className="text-sm text-muted-foreground">No volumes configured</p>
          ) : (
            <div className="space-y-3">
              {volumes.map((volume, index) => (
                <div key={index} className="flex items-center gap-3">
                  <Input
                    value={volume.hostPath}
                    onChange={(e) =>
                      updateVolume(index, { hostPath: e.target.value })
                    }
                    placeholder="Host path"
                    className="flex-1"
                  />
                  <span className="text-muted-foreground">:</span>
                  <Input
                    value={volume.containerPath}
                    onChange={(e) =>
                      updateVolume(index, { containerPath: e.target.value })
                    }
                    placeholder="Container path"
                    className="flex-1"
                  />
                  <div className="flex items-center gap-2">
                    <Checkbox
                      id={`readonly-${index}`}
                      checked={volume.readOnly}
                      onCheckedChange={(checked) =>
                        updateVolume(index, { readOnly: checked === true })
                      }
                    />
                    <label
                      htmlFor={`readonly-${index}`}
                      className="text-sm text-muted-foreground"
                    >
                      RO
                    </label>
                  </div>
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    onClick={() => removeVolume(index)}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Restart Policy & Resource Limits */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Advanced Settings</CardTitle>
        </CardHeader>
        <CardContent>
          <FieldGroup>
            <div className="grid gap-4 sm:grid-cols-3">
              <Field>
                <FieldLabel>Restart Policy</FieldLabel>
                <Select
                  value={restartPolicy}
                  onValueChange={(value) => setRestartPolicy(value)}
                >
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
              </Field>
              <Field>
                <FieldLabel>CPU Limit</FieldLabel>
                <Input
                  value={cpuLimit}
                  onChange={(e) => setCpuLimit(e.target.value)}
                  placeholder="e.g., 0.5, 1, 2"
                />
              </Field>
              <Field>
                <FieldLabel>Memory Limit</FieldLabel>
                <Input
                  value={memoryLimit}
                  onChange={(e) => setMemoryLimit(e.target.value)}
                  placeholder="e.g., 512m, 1g"
                />
              </Field>
            </div>
          </FieldGroup>
        </CardContent>
      </Card>

      {/* Save Button */}
      <div className="flex justify-end">
        <Button
          onClick={() => saveMutation.mutate()}
          disabled={!hasChanges || saveMutation.isPending || !image}
        >
          <Save className="mr-2 h-4 w-4" />
          {saveMutation.isPending ? "Saving..." : "Save Changes"}
        </Button>
      </div>

      {/* Note about backend */}
      <p className="text-sm text-muted-foreground text-center">
        Note: Configuration editing requires redeployment to take effect.
      </p>
    </div>
  )
}
