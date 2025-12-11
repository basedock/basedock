import type { ProjectType } from "@/api/types.gen"

// ProjectType enum values (matches API)
export const PROJECT_TYPE = {
  DockerImage: 0,
  ComposeFile: 1,
} as const

export interface PortMapping {
  containerPort: number
  hostPort?: number
  protocol: string
}

export interface EnvVar {
  key: string
  value: string
}

export interface VolumeMapping {
  hostPath: string
  containerPath: string
  readOnly: boolean
}

export interface CreateProjectFormData {
  // Step 1
  projectType: ProjectType | null

  // Step 2
  name: string
  description: string

  // Docker Image config
  image: string
  tag: string
  ports: PortMapping[]
  envVars: EnvVar[]
  volumes: VolumeMapping[]
  restartPolicy: string
  networks: string[]
  cpuLimit: string
  memoryLimit: string

  // Compose config
  composeFileContent: string
}

export interface DockerImageConfig {
  image: string
  tag?: string
  ports?: Array<{
    containerPort: number
    hostPort?: number
    protocol: string
  }>
  environmentVariables?: Record<string, string>
  volumes?: Array<{
    hostPath: string
    containerPath: string
    readOnly: boolean
  }>
  restartPolicy?: string
  networks?: string[]
  resourceLimits?: {
    cpuLimit?: string
    memoryLimit?: string
  }
}

export function buildDockerImageConfig(
  data: CreateProjectFormData
): string | null {
  if (data.projectType !== PROJECT_TYPE.DockerImage || !data.image) {
    return null
  }

  const config: DockerImageConfig = {
    image: data.image,
    tag: data.tag || "latest",
  }

  if (data.ports.length > 0) {
    config.ports = data.ports.filter((p) => p.containerPort > 0)
  }

  if (data.envVars.length > 0) {
    const envVars: Record<string, string> = {}
    data.envVars.filter((e) => e.key).forEach((e) => {
      envVars[e.key] = e.value
    })
    if (Object.keys(envVars).length > 0) {
      config.environmentVariables = envVars
    }
  }

  if (data.volumes.length > 0) {
    config.volumes = data.volumes.filter((v) => v.hostPath && v.containerPath)
  }

  if (data.restartPolicy && data.restartPolicy !== "no") {
    config.restartPolicy = data.restartPolicy
  }

  if (data.networks.length > 0) {
    config.networks = data.networks.filter((n) => n)
  }

  if (data.cpuLimit || data.memoryLimit) {
    config.resourceLimits = {}
    if (data.cpuLimit) config.resourceLimits.cpuLimit = data.cpuLimit
    if (data.memoryLimit) config.resourceLimits.memoryLimit = data.memoryLimit
  }

  return JSON.stringify(config)
}
