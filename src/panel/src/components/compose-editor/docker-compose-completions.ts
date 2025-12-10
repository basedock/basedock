import type {
  CompletionContext,
  Completion,
  CompletionResult,
} from "@codemirror/autocomplete"

// Top-level keys in docker-compose.yaml
const topLevelKeys: Completion[] = [
  {
    label: "version",
    type: "keyword",
    detail: "Compose file version (optional)",
  },
  { label: "services", type: "keyword", detail: "Service definitions" },
  { label: "volumes", type: "keyword", detail: "Named volumes" },
  { label: "networks", type: "keyword", detail: "Network definitions" },
  { label: "configs", type: "keyword", detail: "Config definitions" },
  { label: "secrets", type: "keyword", detail: "Secret definitions" },
  { label: "name", type: "keyword", detail: "Project name" },
]

// Service-level keys
const serviceKeys: Completion[] = [
  { label: "image", type: "property", detail: "Docker image to use" },
  { label: "build", type: "property", detail: "Build configuration" },
  { label: "container_name", type: "property", detail: "Custom container name" },
  { label: "command", type: "property", detail: "Override default command" },
  { label: "entrypoint", type: "property", detail: "Override entrypoint" },
  { label: "environment", type: "property", detail: "Environment variables" },
  { label: "env_file", type: "property", detail: "Environment file(s)" },
  { label: "ports", type: "property", detail: "Port mappings" },
  { label: "expose", type: "property", detail: "Expose ports internally" },
  { label: "volumes", type: "property", detail: "Volume mounts" },
  { label: "networks", type: "property", detail: "Networks to join" },
  { label: "depends_on", type: "property", detail: "Service dependencies" },
  { label: "restart", type: "property", detail: "Restart policy" },
  { label: "healthcheck", type: "property", detail: "Health check config" },
  { label: "deploy", type: "property", detail: "Deployment configuration" },
  { label: "labels", type: "property", detail: "Container labels" },
  { label: "logging", type: "property", detail: "Logging configuration" },
  { label: "extra_hosts", type: "property", detail: "Extra /etc/hosts entries" },
  { label: "working_dir", type: "property", detail: "Working directory" },
  { label: "user", type: "property", detail: "User to run as" },
  { label: "privileged", type: "property", detail: "Run in privileged mode" },
  { label: "stdin_open", type: "property", detail: "Keep STDIN open (tty -i)" },
  { label: "tty", type: "property", detail: "Allocate TTY" },
  { label: "stop_signal", type: "property", detail: "Stop signal" },
  { label: "stop_grace_period", type: "property", detail: "Stop grace period" },
  { label: "sysctls", type: "property", detail: "Kernel parameters" },
  { label: "ulimits", type: "property", detail: "Resource limits" },
  { label: "cap_add", type: "property", detail: "Add container capabilities" },
  { label: "cap_drop", type: "property", detail: "Drop container capabilities" },
  { label: "devices", type: "property", detail: "Device mappings" },
  { label: "dns", type: "property", detail: "Custom DNS servers" },
  { label: "dns_search", type: "property", detail: "DNS search domains" },
  { label: "hostname", type: "property", detail: "Container hostname" },
  { label: "domainname", type: "property", detail: "Container domain name" },
  { label: "shm_size", type: "property", detail: "Shared memory size" },
  { label: "tmpfs", type: "property", detail: "Mount tmpfs" },
  { label: "security_opt", type: "property", detail: "Security options" },
  { label: "platform", type: "property", detail: "Target platform" },
  { label: "profiles", type: "property", detail: "Service profiles" },
  { label: "pull_policy", type: "property", detail: "Image pull policy" },
]

// Build configuration keys
const buildKeys: Completion[] = [
  { label: "context", type: "property", detail: "Build context path" },
  { label: "dockerfile", type: "property", detail: "Dockerfile path" },
  { label: "args", type: "property", detail: "Build arguments" },
  { label: "target", type: "property", detail: "Build target stage" },
  { label: "cache_from", type: "property", detail: "Cache sources" },
  { label: "labels", type: "property", detail: "Image labels" },
  { label: "network", type: "property", detail: "Build network" },
  { label: "shm_size", type: "property", detail: "Shared memory size" },
]

// Deploy configuration keys
const deployKeys: Completion[] = [
  { label: "replicas", type: "property", detail: "Number of replicas" },
  { label: "resources", type: "property", detail: "Resource constraints" },
  { label: "placement", type: "property", detail: "Placement constraints" },
  { label: "update_config", type: "property", detail: "Update configuration" },
  { label: "rollback_config", type: "property", detail: "Rollback configuration" },
  { label: "restart_policy", type: "property", detail: "Restart policy" },
  { label: "mode", type: "property", detail: "Deployment mode" },
  { label: "labels", type: "property", detail: "Service labels" },
]

// Healthcheck configuration keys
const healthcheckKeys: Completion[] = [
  { label: "test", type: "property", detail: "Health check command" },
  { label: "interval", type: "property", detail: "Check interval" },
  { label: "timeout", type: "property", detail: "Check timeout" },
  { label: "retries", type: "property", detail: "Retry count" },
  { label: "start_period", type: "property", detail: "Start period" },
  { label: "start_interval", type: "property", detail: "Start interval" },
  { label: "disable", type: "property", detail: "Disable healthcheck" },
]

// Logging configuration keys
const loggingKeys: Completion[] = [
  { label: "driver", type: "property", detail: "Logging driver" },
  { label: "options", type: "property", detail: "Driver options" },
]

// Volume configuration keys (named volumes)
const volumeConfigKeys: Completion[] = [
  { label: "driver", type: "property", detail: "Volume driver" },
  { label: "driver_opts", type: "property", detail: "Driver options" },
  { label: "external", type: "property", detail: "Use external volume" },
  { label: "labels", type: "property", detail: "Volume labels" },
  { label: "name", type: "property", detail: "Volume name" },
]

// Network configuration keys
const networkConfigKeys: Completion[] = [
  { label: "driver", type: "property", detail: "Network driver" },
  { label: "driver_opts", type: "property", detail: "Driver options" },
  { label: "external", type: "property", detail: "Use external network" },
  { label: "internal", type: "property", detail: "Internal network" },
  { label: "attachable", type: "property", detail: "Attachable network" },
  { label: "ipam", type: "property", detail: "IPAM configuration" },
  { label: "labels", type: "property", detail: "Network labels" },
  { label: "name", type: "property", detail: "Network name" },
]

// Restart policy values
const restartPolicies: Completion[] = [
  { label: "no", type: "value", detail: "Never restart" },
  { label: "always", type: "value", detail: "Always restart" },
  { label: "on-failure", type: "value", detail: "Restart on failure" },
  { label: "unless-stopped", type: "value", detail: "Restart unless stopped" },
]

// Pull policy values
const pullPolicies: Completion[] = [
  { label: "always", type: "value", detail: "Always pull" },
  { label: "never", type: "value", detail: "Never pull" },
  { label: "missing", type: "value", detail: "Pull if missing" },
  { label: "build", type: "value", detail: "Always build" },
]

// Logging driver values
const loggingDrivers: Completion[] = [
  { label: "json-file", type: "value", detail: "JSON file driver" },
  { label: "syslog", type: "value", detail: "Syslog driver" },
  { label: "journald", type: "value", detail: "Journald driver" },
  { label: "gelf", type: "value", detail: "GELF driver" },
  { label: "fluentd", type: "value", detail: "Fluentd driver" },
  { label: "awslogs", type: "value", detail: "AWS CloudWatch driver" },
  { label: "splunk", type: "value", detail: "Splunk driver" },
  { label: "none", type: "value", detail: "No logging" },
]

// Network driver values
const networkDrivers: Completion[] = [
  { label: "bridge", type: "value", detail: "Bridge network" },
  { label: "host", type: "value", detail: "Host network" },
  { label: "overlay", type: "value", detail: "Overlay network" },
  { label: "none", type: "value", detail: "No networking" },
]

// Common Docker images
const commonImages: Completion[] = [
  { label: "nginx", type: "value", detail: "Web server" },
  { label: "nginx:alpine", type: "value", detail: "Nginx Alpine" },
  { label: "postgres", type: "value", detail: "PostgreSQL database" },
  { label: "postgres:16-alpine", type: "value", detail: "PostgreSQL 16 Alpine" },
  { label: "mysql", type: "value", detail: "MySQL database" },
  { label: "mysql:8", type: "value", detail: "MySQL 8" },
  { label: "redis", type: "value", detail: "Redis cache" },
  { label: "redis:alpine", type: "value", detail: "Redis Alpine" },
  { label: "mongo", type: "value", detail: "MongoDB database" },
  { label: "node", type: "value", detail: "Node.js runtime" },
  { label: "node:20-alpine", type: "value", detail: "Node.js 20 Alpine" },
  { label: "python", type: "value", detail: "Python runtime" },
  { label: "python:3.12-slim", type: "value", detail: "Python 3.12 slim" },
  { label: "golang", type: "value", detail: "Go runtime" },
  { label: "alpine", type: "value", detail: "Alpine Linux" },
  { label: "ubuntu", type: "value", detail: "Ubuntu Linux" },
  { label: "debian", type: "value", detail: "Debian Linux" },
  { label: "rabbitmq", type: "value", detail: "RabbitMQ message broker" },
  { label: "elasticsearch", type: "value", detail: "Elasticsearch" },
  { label: "traefik", type: "value", detail: "Traefik proxy" },
]

// Analyze the current context in the YAML document
function analyzeContext(
  state: { doc: { toString: () => string; lineAt: (pos: number) => { from: number; text: string } } },
  pos: number
): {
  indent: number
  path: string[]
  afterColon: boolean
  currentKey: string | null
} {
  const content = state.doc.toString()
  const lines = content.slice(0, pos).split("\n")
  const currentLine = state.doc.lineAt(pos).text
  const cursorCol = pos - state.doc.lineAt(pos).from

  // Calculate indent of current line
  const currentIndent = currentLine.match(/^(\s*)/)?.[1].length ?? 0

  // Check if we're after a colon on the current line
  const beforeCursor = currentLine.slice(0, cursorCol)
  const afterColon = beforeCursor.includes(":")

  // Extract current key if after colon
  const keyMatch = beforeCursor.match(/^\s*(\w+)\s*:\s*$/)
  const currentKey = keyMatch ? keyMatch[1] : null

  // Build the path by analyzing indentation levels
  const path: string[] = []
  let lastIndent = -1

  for (let i = lines.length - 1; i >= 0; i--) {
    const line = lines[i]
    const match = line.match(/^(\s*)(\w+)\s*:/)
    if (match) {
      const indent = match[1].length
      if (indent < currentIndent && indent > lastIndent) {
        path.unshift(match[2])
        lastIndent = indent
        if (indent === 0) break
      }
    }
  }

  return { indent: currentIndent, path, afterColon, currentKey }
}

// Main completion function
export function dockerComposeCompletions(
  context: CompletionContext
): CompletionResult | null {
  // Get the word being typed
  const word = context.matchBefore(/[\w-]*/)
  if (!word && !context.explicit) return null

  const { indent, path, afterColon, currentKey } = analyzeContext(
    context.state,
    context.pos
  )

  let completions: Completion[] = []

  // If we're after a colon, provide value completions
  if (afterColon && currentKey) {
    switch (currentKey) {
      case "restart":
        completions = restartPolicies
        break
      case "pull_policy":
        completions = pullPolicies
        break
      case "driver":
        if (path.includes("logging")) {
          completions = loggingDrivers
        } else if (path.includes("networks")) {
          completions = networkDrivers
        }
        break
      case "image":
        completions = commonImages
        break
      default:
        return null
    }
  } else {
    // Provide key completions based on context
    if (indent === 0 || path.length === 0) {
      // Root level
      completions = topLevelKeys
    } else if (path[0] === "services") {
      if (path.length === 1) {
        // Under a service name
        completions = serviceKeys
      } else if (path.length === 2) {
        // Nested under service key
        const serviceKey = path[1]
        switch (serviceKey) {
          case "build":
            completions = buildKeys
            break
          case "deploy":
            completions = deployKeys
            break
          case "healthcheck":
            completions = healthcheckKeys
            break
          case "logging":
            completions = loggingKeys
            break
          default:
            completions = serviceKeys
        }
      }
    } else if (path[0] === "volumes" && path.length === 1) {
      completions = volumeConfigKeys
    } else if (path[0] === "networks" && path.length === 1) {
      completions = networkConfigKeys
    }
  }

  if (completions.length === 0) return null

  return {
    from: word?.from ?? context.pos,
    options: completions,
    validFor: /^[\w-]*$/,
  }
}
