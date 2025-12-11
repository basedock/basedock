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
import { Separator } from "@/components/ui/separator"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Plus, Trash2 } from "lucide-react"
import type { CreateProjectFormData, PortMapping, EnvVar, VolumeMapping } from "../types"

interface StepDockerImageConfigProps {
  values: CreateProjectFormData
  onFieldChange: <K extends keyof CreateProjectFormData>(
    field: K,
    value: CreateProjectFormData[K]
  ) => void
  onBack: () => void
  onSubmit: () => void
  isSubmitting: boolean
}

export function StepDockerImageConfig({
  values,
  onFieldChange,
  onBack,
  onSubmit,
  isSubmitting,
}: StepDockerImageConfigProps) {
  const addPort = () => {
    onFieldChange("ports", [
      ...values.ports,
      { containerPort: 80, hostPort: undefined, protocol: "tcp" },
    ])
  }

  const updatePort = (index: number, updates: Partial<PortMapping>) => {
    const newPorts = [...values.ports]
    newPorts[index] = { ...newPorts[index], ...updates }
    onFieldChange("ports", newPorts)
  }

  const removePort = (index: number) => {
    onFieldChange(
      "ports",
      values.ports.filter((_, i) => i !== index)
    )
  }

  const addEnvVar = () => {
    onFieldChange("envVars", [...values.envVars, { key: "", value: "" }])
  }

  const updateEnvVar = (index: number, updates: Partial<EnvVar>) => {
    const newEnvVars = [...values.envVars]
    newEnvVars[index] = { ...newEnvVars[index], ...updates }
    onFieldChange("envVars", newEnvVars)
  }

  const removeEnvVar = (index: number) => {
    onFieldChange(
      "envVars",
      values.envVars.filter((_, i) => i !== index)
    )
  }

  const addVolume = () => {
    onFieldChange("volumes", [
      ...values.volumes,
      { hostPath: "", containerPath: "", readOnly: false },
    ])
  }

  const updateVolume = (index: number, updates: Partial<VolumeMapping>) => {
    const newVolumes = [...values.volumes]
    newVolumes[index] = { ...newVolumes[index], ...updates }
    onFieldChange("volumes", newVolumes)
  }

  const removeVolume = (index: number) => {
    onFieldChange(
      "volumes",
      values.volumes.filter((_, i) => i !== index)
    )
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
                    value={values.image}
                    onChange={(e) => onFieldChange("image", e.target.value)}
                    placeholder="nginx, postgres, your-registry/image"
                  />
                </Field>
              </div>
              <Field>
                <FieldLabel>Tag</FieldLabel>
                <Input
                  value={values.tag}
                  onChange={(e) => onFieldChange("tag", e.target.value)}
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
          {values.ports.length === 0 ? (
            <p className="text-sm text-muted-foreground">No ports configured</p>
          ) : (
            <div className="space-y-3">
              {values.ports.map((port, index) => (
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
          {values.envVars.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No environment variables
            </p>
          ) : (
            <div className="space-y-3">
              {values.envVars.map((envVar, index) => (
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
          {values.volumes.length === 0 ? (
            <p className="text-sm text-muted-foreground">No volumes configured</p>
          ) : (
            <div className="space-y-3">
              {values.volumes.map((volume, index) => (
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
                  value={values.restartPolicy}
                  onValueChange={(value) =>
                    onFieldChange("restartPolicy", value)
                  }
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
                  value={values.cpuLimit}
                  onChange={(e) => onFieldChange("cpuLimit", e.target.value)}
                  placeholder="e.g., 0.5, 1, 2"
                />
              </Field>
              <Field>
                <FieldLabel>Memory Limit</FieldLabel>
                <Input
                  value={values.memoryLimit}
                  onChange={(e) => onFieldChange("memoryLimit", e.target.value)}
                  placeholder="e.g., 512m, 1g"
                />
              </Field>
            </div>
          </FieldGroup>
        </CardContent>
      </Card>

      <Separator />

      <div className="flex justify-between">
        <Button variant="outline" onClick={onBack}>
          Back
        </Button>
        <Button onClick={onSubmit} disabled={isSubmitting || !values.image}>
          {isSubmitting ? "Creating..." : "Create Project"}
        </Button>
      </div>
    </div>
  )
}
