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
    detail: "Compose file version (deprecated)",
  },
  { label: "name", type: "keyword", detail: "Project name" },
  { label: "services", type: "keyword", detail: "Service definitions" },
  { label: "networks", type: "keyword", detail: "Network definitions" },
  { label: "volumes", type: "keyword", detail: "Named volumes" },
  { label: "configs", type: "keyword", detail: "Config definitions" },
  { label: "secrets", type: "keyword", detail: "Secret definitions" },
  { label: "include", type: "keyword", detail: "Include other Compose files" },
  { label: "models", type: "keyword", detail: "AI model definitions" },
]

// Service-level keys (comprehensive list from Docker Compose Specification)
const serviceKeys: Completion[] = [
  // Core configuration
  { label: "image", type: "property", detail: "Docker image to use" },
  { label: "build", type: "property", detail: "Build configuration" },
  { label: "container_name", type: "property", detail: "Custom container name" },
  { label: "command", type: "property", detail: "Override default command" },
  { label: "entrypoint", type: "property", detail: "Override entrypoint" },
  { label: "working_dir", type: "property", detail: "Working directory" },
  { label: "user", type: "property", detail: "User to run as" },

  // Environment & configuration
  { label: "environment", type: "property", detail: "Environment variables" },
  { label: "env_file", type: "property", detail: "Environment file(s)" },
  { label: "configs", type: "property", detail: "Config references" },
  { label: "secrets", type: "property", detail: "Secret references" },

  // Networking
  { label: "ports", type: "property", detail: "Port mappings" },
  { label: "expose", type: "property", detail: "Expose ports internally" },
  { label: "networks", type: "property", detail: "Networks to join" },
  { label: "network_mode", type: "property", detail: "Network mode (none, host, service:, container:)" },
  { label: "hostname", type: "property", detail: "Container hostname" },
  { label: "domainname", type: "property", detail: "Container domain name" },
  { label: "dns", type: "property", detail: "Custom DNS servers" },
  { label: "dns_opt", type: "property", detail: "DNS resolver options" },
  { label: "dns_search", type: "property", detail: "DNS search domains" },
  { label: "extra_hosts", type: "property", detail: "Extra /etc/hosts entries" },
  { label: "mac_address", type: "property", detail: "MAC address" },
  { label: "links", type: "property", detail: "Network links to other services" },
  { label: "external_links", type: "property", detail: "Links to external containers" },

  // Storage
  { label: "volumes", type: "property", detail: "Volume mounts" },
  { label: "volumes_from", type: "property", detail: "Mount volumes from other containers" },
  { label: "tmpfs", type: "property", detail: "Mount tmpfs" },
  { label: "shm_size", type: "property", detail: "Shared memory size" },
  { label: "storage_opt", type: "property", detail: "Storage driver options" },

  // Dependencies & lifecycle
  { label: "depends_on", type: "property", detail: "Service dependencies" },
  { label: "restart", type: "property", detail: "Restart policy" },
  { label: "healthcheck", type: "property", detail: "Health check config" },
  { label: "init", type: "property", detail: "Run init process (PID 1)" },
  { label: "stop_signal", type: "property", detail: "Stop signal" },
  { label: "stop_grace_period", type: "property", detail: "Stop grace period" },
  { label: "post_start", type: "property", detail: "Post-start lifecycle hooks" },
  { label: "pre_stop", type: "property", detail: "Pre-stop lifecycle hooks" },

  // Deployment & scaling
  { label: "deploy", type: "property", detail: "Deployment configuration" },
  { label: "scale", type: "property", detail: "Default container count" },
  { label: "profiles", type: "property", detail: "Service profiles" },
  { label: "pull_policy", type: "property", detail: "Image pull policy" },
  { label: "platform", type: "property", detail: "Target platform (os/arch)" },
  { label: "runtime", type: "property", detail: "OCI runtime" },

  // Development
  { label: "develop", type: "property", detail: "Development configuration (watch)" },
  { label: "extends", type: "property", detail: "Extend another service" },

  // Resource limits
  { label: "cpu_count", type: "property", detail: "Usable CPU count" },
  { label: "cpu_percent", type: "property", detail: "CPU percentage" },
  { label: "cpu_shares", type: "property", detail: "Relative CPU weight" },
  { label: "cpu_period", type: "property", detail: "CPU CFS period" },
  { label: "cpu_quota", type: "property", detail: "CPU CFS quota" },
  { label: "cpu_rt_runtime", type: "property", detail: "CPU real-time runtime" },
  { label: "cpu_rt_period", type: "property", detail: "CPU real-time period" },
  { label: "cpus", type: "property", detail: "Virtual CPU allocation" },
  { label: "cpuset", type: "property", detail: "Explicit CPU assignment" },
  { label: "mem_limit", type: "property", detail: "Memory limit" },
  { label: "mem_reservation", type: "property", detail: "Memory reservation" },
  { label: "mem_swappiness", type: "property", detail: "Memory swappiness (0-100)" },
  { label: "memswap_limit", type: "property", detail: "Total memory + swap limit" },
  { label: "pids_limit", type: "property", detail: "Process limit" },
  { label: "blkio_config", type: "property", detail: "Block I/O configuration" },
  { label: "ulimits", type: "property", detail: "Resource limits" },
  { label: "oom_kill_disable", type: "property", detail: "Disable OOM killer" },
  { label: "oom_score_adj", type: "property", detail: "OOM score adjustment" },

  // Security & isolation
  { label: "privileged", type: "property", detail: "Run in privileged mode" },
  { label: "read_only", type: "property", detail: "Read-only root filesystem" },
  { label: "cap_add", type: "property", detail: "Add container capabilities" },
  { label: "cap_drop", type: "property", detail: "Drop container capabilities" },
  { label: "security_opt", type: "property", detail: "Security options" },
  { label: "sysctls", type: "property", detail: "Kernel parameters" },
  { label: "isolation", type: "property", detail: "Container isolation technology" },
  { label: "userns_mode", type: "property", detail: "User namespace mode" },
  { label: "ipc", type: "property", detail: "IPC isolation mode" },
  { label: "pid", type: "property", detail: "PID namespace mode" },
  { label: "uts", type: "property", detail: "UTS namespace mode" },
  { label: "cgroup", type: "property", detail: "Cgroup namespace mode" },
  { label: "cgroup_parent", type: "property", detail: "Parent cgroup" },
  { label: "credential_spec", type: "property", detail: "Windows credential spec" },

  // Devices & hardware
  { label: "devices", type: "property", detail: "Device mappings" },
  { label: "device_cgroup_rules", type: "property", detail: "Device cgroup rules" },
  { label: "gpus", type: "property", detail: "GPU allocation" },

  // Terminal & I/O
  { label: "stdin_open", type: "property", detail: "Keep STDIN open" },
  { label: "tty", type: "property", detail: "Allocate pseudo-TTY" },
  { label: "attach", type: "property", detail: "Attach to container for logs" },

  // Metadata
  { label: "labels", type: "property", detail: "Container labels" },
  { label: "label_file", type: "property", detail: "External label file(s)" },
  { label: "annotations", type: "property", detail: "Container annotations" },
  { label: "logging", type: "property", detail: "Logging configuration" },

  // Advanced
  { label: "group_add", type: "property", detail: "Additional groups" },
  { label: "driver_opts", type: "property", detail: "Storage driver options" },
  { label: "models", type: "property", detail: "AI model references" },
  { label: "provider", type: "property", detail: "External service provider" },
  { label: "use_api_socket", type: "property", detail: "Access engine API socket" },
]

// Build configuration keys (comprehensive from Docker Compose Specification)
const buildKeys: Completion[] = [
  { label: "context", type: "property", detail: "Build context path or Git URL" },
  { label: "dockerfile", type: "property", detail: "Dockerfile path" },
  { label: "dockerfile_inline", type: "property", detail: "Inline Dockerfile content" },
  { label: "args", type: "property", detail: "Build arguments" },
  { label: "target", type: "property", detail: "Build target stage" },
  { label: "cache_from", type: "property", detail: "Cache sources" },
  { label: "cache_to", type: "property", detail: "Cache export locations" },
  { label: "additional_contexts", type: "property", detail: "Named build contexts" },
  { label: "labels", type: "property", detail: "Image labels" },
  { label: "tags", type: "property", detail: "Image tag mappings" },
  { label: "network", type: "property", detail: "Build network mode" },
  { label: "shm_size", type: "property", detail: "Shared memory size" },
  { label: "platforms", type: "property", detail: "Target platforms (linux/amd64, etc)" },
  { label: "privileged", type: "property", detail: "Build with elevated privileges" },
  { label: "no_cache", type: "property", detail: "Disable build cache" },
  { label: "pull", type: "property", detail: "Always pull base images" },
  { label: "extra_hosts", type: "property", detail: "Extra /etc/hosts entries" },
  { label: "isolation", type: "property", detail: "Container isolation technology" },
  { label: "entitlements", type: "property", detail: "Build entitlements (network.host, security.insecure)" },
  { label: "secrets", type: "property", detail: "Build secrets" },
  { label: "ssh", type: "property", detail: "SSH agent socket or keys" },
  { label: "ulimits", type: "property", detail: "Build resource limits" },
  { label: "provenance", type: "property", detail: "Provenance attestation" },
  { label: "sbom", type: "property", detail: "SBOM attestation" },
]

// Deploy configuration keys (comprehensive from Docker Compose Specification)
const deployKeys: Completion[] = [
  { label: "mode", type: "property", detail: "Deployment mode (global, replicated, etc)" },
  { label: "replicas", type: "property", detail: "Number of replicas" },
  { label: "endpoint_mode", type: "property", detail: "Service discovery method (vip, dnsrr)" },
  { label: "resources", type: "property", detail: "Resource constraints" },
  { label: "placement", type: "property", detail: "Placement constraints & preferences" },
  { label: "update_config", type: "property", detail: "Update configuration" },
  { label: "rollback_config", type: "property", detail: "Rollback configuration" },
  { label: "restart_policy", type: "property", detail: "Restart policy" },
  { label: "labels", type: "property", detail: "Service labels" },
]

// Deploy resources nested keys
const deployResourcesKeys: Completion[] = [
  { label: "limits", type: "property", detail: "Maximum resource allocation" },
  { label: "reservations", type: "property", detail: "Minimum guaranteed resources" },
]

// Deploy resource limits/reservations nested keys
const deployResourceLimitKeys: Completion[] = [
  { label: "cpus", type: "property", detail: "CPU allocation (e.g., '0.5')" },
  { label: "memory", type: "property", detail: "Memory allocation (e.g., '512M')" },
  { label: "pids", type: "property", detail: "Process ID limit" },
  { label: "devices", type: "property", detail: "Device reservations (GPU, etc)" },
]

// Deploy device reservation keys
const deployDeviceKeys: Completion[] = [
  { label: "capabilities", type: "property", detail: "Device capabilities (gpu, tpu)" },
  { label: "driver", type: "property", detail: "Device driver" },
  { label: "count", type: "property", detail: "Number of devices (or 'all')" },
  { label: "device_ids", type: "property", detail: "Specific device IDs" },
  { label: "options", type: "property", detail: "Driver-specific options" },
]

// Deploy placement keys
const deployPlacementKeys: Completion[] = [
  { label: "constraints", type: "property", detail: "Node constraints" },
  { label: "preferences", type: "property", detail: "Placement preferences (spread)" },
]

// Deploy restart_policy keys
const deployRestartPolicyKeys: Completion[] = [
  { label: "condition", type: "property", detail: "When to restart (none, on-failure, any)" },
  { label: "delay", type: "property", detail: "Delay between restart attempts" },
  { label: "max_attempts", type: "property", detail: "Maximum restart attempts" },
  { label: "window", type: "property", detail: "Time window for restart evaluation" },
]

// Deploy update_config/rollback_config keys
const deployUpdateConfigKeys: Completion[] = [
  { label: "parallelism", type: "property", detail: "Containers to update at once" },
  { label: "delay", type: "property", detail: "Delay between updates" },
  { label: "failure_action", type: "property", detail: "Action on failure (continue, rollback, pause)" },
  { label: "monitor", type: "property", detail: "Duration to monitor after update" },
  { label: "max_failure_ratio", type: "property", detail: "Acceptable failure rate" },
  { label: "order", type: "property", detail: "Update order (stop-first, start-first)" },
]

// Healthcheck configuration keys
const healthcheckKeys: Completion[] = [
  { label: "test", type: "property", detail: "Health check command" },
  { label: "interval", type: "property", detail: "Time between checks (default: 30s)" },
  { label: "timeout", type: "property", detail: "Check timeout (default: 30s)" },
  { label: "retries", type: "property", detail: "Consecutive failures needed (default: 3)" },
  { label: "start_period", type: "property", detail: "Grace period before starting checks" },
  { label: "start_interval", type: "property", detail: "Interval during start period" },
  { label: "disable", type: "property", detail: "Disable inherited healthcheck" },
]

// Develop configuration keys (for watch-based development)
const developKeys: Completion[] = [
  { label: "watch", type: "property", detail: "File watch rules for auto-updates" },
]

// Develop watch rule keys
const developWatchKeys: Completion[] = [
  { label: "action", type: "property", detail: "Action on change (rebuild, sync, restart, etc)" },
  { label: "path", type: "property", detail: "Path to monitor for changes" },
  { label: "target", type: "property", detail: "Container path for sync actions" },
  { label: "ignore", type: "property", detail: "Patterns to ignore (.dockerignore syntax)" },
  { label: "include", type: "property", detail: "Patterns to include" },
  { label: "initial_sync", type: "property", detail: "Sync files at watch start" },
  { label: "exec", type: "property", detail: "Command to run (sync+exec action)" },
]

// Develop watch exec keys
const developWatchExecKeys: Completion[] = [
  { label: "command", type: "property", detail: "Command to execute" },
  { label: "user", type: "property", detail: "User to run command as" },
  { label: "privileged", type: "property", detail: "Run with elevated privileges" },
  { label: "working_dir", type: "property", detail: "Working directory for command" },
  { label: "environment", type: "property", detail: "Environment variables for command" },
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

// Network configuration keys (top-level networks section)
const networkConfigKeys: Completion[] = [
  { label: "driver", type: "property", detail: "Network driver (bridge, overlay, etc)" },
  { label: "driver_opts", type: "property", detail: "Driver-specific options" },
  { label: "external", type: "property", detail: "Use pre-existing network" },
  { label: "internal", type: "property", detail: "Isolated network (no external access)" },
  { label: "attachable", type: "property", detail: "Allow standalone containers to attach" },
  { label: "enable_ipv4", type: "property", detail: "Enable IPv4 addressing" },
  { label: "enable_ipv6", type: "property", detail: "Enable IPv6 addressing" },
  { label: "ipam", type: "property", detail: "IP Address Management config" },
  { label: "labels", type: "property", detail: "Network metadata labels" },
  { label: "name", type: "property", detail: "Custom network name" },
]

// IPAM configuration keys
const ipamConfigKeys: Completion[] = [
  { label: "driver", type: "property", detail: "IPAM driver" },
  { label: "config", type: "property", detail: "IPAM configuration blocks" },
  { label: "options", type: "property", detail: "Driver-specific options" },
]

// IPAM config block keys
const ipamConfigBlockKeys: Completion[] = [
  { label: "subnet", type: "property", detail: "Subnet in CIDR format" },
  { label: "ip_range", type: "property", detail: "IP range for allocation" },
  { label: "gateway", type: "property", detail: "Gateway IP address" },
  { label: "aux_addresses", type: "property", detail: "Auxiliary addresses" },
]

// Config definition keys (top-level configs section)
const configKeys: Completion[] = [
  { label: "file", type: "property", detail: "Config file path" },
  { label: "environment", type: "property", detail: "Environment variable source" },
  { label: "content", type: "property", detail: "Inline config content" },
  { label: "external", type: "property", detail: "Use pre-existing config" },
  { label: "name", type: "property", detail: "Config object name" },
]

// Secret definition keys (top-level secrets section)
const secretKeys: Completion[] = [
  { label: "file", type: "property", detail: "Secret file path" },
  { label: "environment", type: "property", detail: "Environment variable source" },
  { label: "external", type: "property", detail: "Use pre-existing secret" },
  { label: "name", type: "property", detail: "Secret object name" },
]

// Include configuration keys
const includeKeys: Completion[] = [
  { label: "path", type: "property", detail: "Compose file path(s) to include" },
  { label: "project_directory", type: "property", detail: "Base path for relative paths" },
  { label: "env_file", type: "property", detail: "Environment file(s) for interpolation" },
]

// Model configuration keys (top-level models section)
const modelKeys: Completion[] = [
  { label: "model", type: "property", detail: "OCI artifact identifier" },
  { label: "context_size", type: "property", detail: "Maximum token context size" },
  { label: "runtime_flags", type: "property", detail: "Inference engine flags" },
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
  { label: "missing", type: "value", detail: "Pull if missing (default)" },
  { label: "build", type: "value", detail: "Always build" },
  { label: "daily", type: "value", detail: "Pull daily" },
  { label: "weekly", type: "value", detail: "Pull weekly" },
]

// Network mode values
const networkModeValues: Completion[] = [
  { label: "none", type: "value", detail: "No networking" },
  { label: "host", type: "value", detail: "Use host networking" },
  { label: "service:", type: "value", detail: "Share network with another service" },
  { label: "container:", type: "value", detail: "Share network with a container" },
]

// Deploy mode values
const deployModeValues: Completion[] = [
  { label: "replicated", type: "value", detail: "Run specified replicas (default)" },
  { label: "global", type: "value", detail: "One container per node" },
  { label: "replicated-job", type: "value", detail: "Run tasks until completion" },
  { label: "global-job", type: "value", detail: "One task per node until completion" },
]

// Deploy endpoint mode values
const endpointModeValues: Completion[] = [
  { label: "vip", type: "value", detail: "Virtual IP load balancing" },
  { label: "dnsrr", type: "value", detail: "DNS round-robin" },
]

// Deploy restart policy condition values
const restartConditionValues: Completion[] = [
  { label: "none", type: "value", detail: "Never restart" },
  { label: "on-failure", type: "value", detail: "Restart on non-zero exit" },
  { label: "any", type: "value", detail: "Always restart (default)" },
]

// Deploy update/rollback failure action values
const failureActionValues: Completion[] = [
  { label: "pause", type: "value", detail: "Pause on failure (default)" },
  { label: "continue", type: "value", detail: "Continue on failure" },
  { label: "rollback", type: "value", detail: "Rollback on failure" },
]

// Deploy update/rollback order values
const updateOrderValues: Completion[] = [
  { label: "stop-first", type: "value", detail: "Stop old before starting new (default)" },
  { label: "start-first", type: "value", detail: "Start new before stopping old" },
]

// Watch action values
const watchActionValues: Completion[] = [
  { label: "rebuild", type: "value", detail: "Rebuild image and recreate service" },
  { label: "restart", type: "value", detail: "Restart the container" },
  { label: "sync", type: "value", detail: "Sync files to container" },
  { label: "sync+restart", type: "value", detail: "Sync files then restart" },
  { label: "sync+exec", type: "value", detail: "Sync files then execute command" },
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
      case "network_mode":
        completions = networkModeValues
        break
      case "driver":
        if (path.includes("logging")) {
          completions = loggingDrivers
        } else if (path.includes("networks")) {
          completions = networkDrivers
        } else if (path.includes("ipam")) {
          // IPAM driver - no specific completions
          return null
        }
        break
      case "image":
        completions = commonImages
        break
      // Deploy section values
      case "mode":
        if (path.includes("deploy")) {
          completions = deployModeValues
        }
        break
      case "endpoint_mode":
        completions = endpointModeValues
        break
      case "condition":
        if (path.includes("restart_policy")) {
          completions = restartConditionValues
        }
        break
      case "failure_action":
        completions = failureActionValues
        break
      case "order":
        if (path.includes("update_config") || path.includes("rollback_config")) {
          completions = updateOrderValues
        }
        break
      // Develop watch action
      case "action":
        if (path.includes("watch") || path.includes("develop")) {
          completions = watchActionValues
        }
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
        // Under a service name - service-level keys
        completions = serviceKeys
      } else if (path.length >= 2) {
        // Nested under service key
        const serviceKey = path[1]
        const nestedKey = path[2]

        switch (serviceKey) {
          case "build":
            completions = buildKeys
            break
          case "deploy":
            if (path.length === 2) {
              completions = deployKeys
            } else {
              // Nested deploy keys
              switch (nestedKey) {
                case "resources":
                  if (path.length === 3) {
                    completions = deployResourcesKeys
                  } else if (path[3] === "limits" || path[3] === "reservations") {
                    if (path.length === 4) {
                      completions = deployResourceLimitKeys
                    } else if (path[4] === "devices") {
                      completions = deployDeviceKeys
                    }
                  }
                  break
                case "placement":
                  completions = deployPlacementKeys
                  break
                case "restart_policy":
                  completions = deployRestartPolicyKeys
                  break
                case "update_config":
                case "rollback_config":
                  completions = deployUpdateConfigKeys
                  break
                default:
                  completions = deployKeys
              }
            }
            break
          case "healthcheck":
            completions = healthcheckKeys
            break
          case "logging":
            completions = loggingKeys
            break
          case "develop":
            if (path.length === 2) {
              completions = developKeys
            } else if (nestedKey === "watch") {
              if (path.length === 3) {
                completions = developWatchKeys
              } else if (path[3] === "exec") {
                completions = developWatchExecKeys
              }
            }
            break
          default:
            completions = serviceKeys
        }
      }
    } else if (path[0] === "volumes" && path.length === 1) {
      completions = volumeConfigKeys
    } else if (path[0] === "networks") {
      if (path.length === 1) {
        completions = networkConfigKeys
      } else if (path.length >= 2) {
        const networkKey = path[1]
        if (networkKey === "ipam") {
          if (path.length === 2) {
            completions = ipamConfigKeys
          } else if (path[2] === "config") {
            completions = ipamConfigBlockKeys
          }
        } else {
          completions = networkConfigKeys
        }
      }
    } else if (path[0] === "configs" && path.length === 1) {
      completions = configKeys
    } else if (path[0] === "secrets" && path.length === 1) {
      completions = secretKeys
    } else if (path[0] === "include") {
      completions = includeKeys
    } else if (path[0] === "models" && path.length === 1) {
      completions = modelKeys
    }
  }

  if (completions.length === 0) return null

  return {
    from: word?.from ?? context.pos,
    options: completions,
    validFor: /^[\w-]*$/,
  }
}
