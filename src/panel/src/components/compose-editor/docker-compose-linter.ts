import type { Diagnostic } from "@codemirror/lint"
import { EditorView } from "@codemirror/view"
import { parse, YAMLParseError } from "yaml"

interface DockerComposeService {
  image?: string
  build?: string | Record<string, unknown>
  ports?: (string | number | Record<string, unknown>)[]
  volumes?: (string | Record<string, unknown>)[]
  restart?: string
  pull_policy?: string
  network_mode?: string
  depends_on?: string[] | Record<string, unknown>
  environment?: Record<string, unknown> | string[]
  networks?: string[] | Record<string, unknown>
  configs?: string[] | Record<string, unknown>[]
  secrets?: string[] | Record<string, unknown>[]
  deploy?: {
    mode?: string
    endpoint_mode?: string
    [key: string]: unknown
  }
  develop?: {
    watch?: {
      action?: string
      path?: string
      target?: string
      [key: string]: unknown
    }[]
  }
  [key: string]: unknown
}

interface DockerComposeConfig {
  file?: string
  environment?: string
  content?: string
  external?: boolean
  name?: string
}

interface DockerComposeSecret {
  file?: string
  environment?: string
  external?: boolean
  name?: string
}

interface DockerComposeFile {
  version?: string
  name?: string
  services?: Record<string, DockerComposeService>
  volumes?: Record<string, unknown>
  networks?: Record<string, unknown>
  configs?: Record<string, DockerComposeConfig | null>
  secrets?: Record<string, DockerComposeSecret | null>
  include?: string[] | Record<string, unknown>[]
  models?: Record<string, unknown>
  [key: string]: unknown
}

// Find line number for a key in the content
function findLineForKey(content: string, key: string, startLine = 0): number {
  const lines = content.split("\n")
  const regex = new RegExp(`^\\s*${key}\\s*:`)
  for (let i = startLine; i < lines.length; i++) {
    if (regex.test(lines[i])) {
      return i + 1
    }
  }
  return startLine + 1
}

// Find line number for a service definition
function findServiceLine(content: string, serviceName: string): number {
  const lines = content.split("\n")
  let inServices = false
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i]
    if (/^\s*services\s*:/.test(line)) {
      inServices = true
      continue
    }
    if (inServices && new RegExp(`^\\s{2}${serviceName}\\s*:`).test(line)) {
      return i + 1
    }
    // Stop if we hit another top-level key
    if (inServices && /^[a-z]/.test(line)) {
      break
    }
  }
  return 1
}

// Validate port mapping format
function isValidPortMapping(port: string | number): boolean {
  if (typeof port === "number") return port > 0 && port <= 65535

  // Formats: "80", "80:80", "127.0.0.1:80:80", "80:80/tcp", "8080-8090:80-90"
  const portRegex =
    /^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:)?(\d+(-\d+)?:)?(\d+(-\d+)?)(\/(?:tcp|udp))?$/
  return portRegex.test(port)
}

// Valid restart policies
const validRestartPolicies = ["no", "always", "on-failure", "unless-stopped"]

// Valid pull policies
const validPullPolicies = ["always", "never", "missing", "build", "daily", "weekly"]

// Valid network modes (prefixes for service: and container:)
const validNetworkModes = ["none", "host", "bridge"]
const validNetworkModePrefixes = ["service:", "container:"]

// Valid deploy modes
const validDeployModes = ["global", "replicated", "replicated-job", "global-job"]

// Valid deploy endpoint modes
const validEndpointModes = ["vip", "dnsrr"]

// Valid watch actions
const validWatchActions = ["rebuild", "restart", "sync", "sync+restart", "sync+exec"]

// Valid top-level keys
const validTopLevelKeys = [
  "version",
  "name",
  "services",
  "volumes",
  "networks",
  "configs",
  "secrets",
  "include",
  "models",
]

// Validate YAML syntax
function validateYamlSyntax(content: string): Diagnostic[] {
  const diagnostics: Diagnostic[] = []

  try {
    parse(content, { strict: true })
  } catch (e) {
    if (e instanceof YAMLParseError) {
      const line = e.linePos?.[0]?.line ?? 1
      const col = e.linePos?.[0]?.col ?? 1
      const lines = content.split("\n")
      const lineStart = lines.slice(0, line - 1).join("\n").length + (line > 1 ? 1 : 0)
      const lineEnd = lineStart + (lines[line - 1]?.length ?? 0)

      diagnostics.push({
        from: lineStart + Math.max(0, col - 1),
        to: lineEnd,
        severity: "error",
        message: e.message.split("\n")[0], // First line of error message
      })
    }
  }

  return diagnostics
}

// Validate Docker Compose schema
function validateDockerComposeSchema(
  content: string,
  view: EditorView
): Diagnostic[] {
  const diagnostics: Diagnostic[] = []

  let doc: DockerComposeFile
  try {
    doc = parse(content) as DockerComposeFile
  } catch {
    // YAML parse errors handled separately
    return diagnostics
  }

  if (!doc || typeof doc !== "object") {
    return diagnostics
  }

  // Check for unknown top-level keys (excluding extension fields x-)
  for (const key of Object.keys(doc)) {
    if (!validTopLevelKeys.includes(key) && !key.startsWith("x-")) {
      const line = findLineForKey(content, key)
      const lineInfo = view.state.doc.line(Math.min(line, view.state.doc.lines))
      diagnostics.push({
        from: lineInfo.from,
        to: lineInfo.to,
        severity: "warning",
        message: `Unknown top-level key: "${key}"`,
      })
    }
  }

  // Check for required services section
  if (!doc.services) {
    diagnostics.push({
      from: 0,
      to: Math.min(50, content.length),
      severity: "info",
      message: 'Docker Compose file should have a "services" section',
    })
    return diagnostics
  }

  // Validate each service
  for (const [serviceName, service] of Object.entries(doc.services)) {
    if (!service || typeof service !== "object") continue

    const serviceLine = findServiceLine(content, serviceName)
    const serviceLineInfo = view.state.doc.line(
      Math.min(serviceLine, view.state.doc.lines)
    )

    // Service must have either 'image' or 'build'
    if (!service.image && !service.build) {
      diagnostics.push({
        from: serviceLineInfo.from,
        to: serviceLineInfo.to,
        severity: "error",
        message: `Service "${serviceName}" must have either "image" or "build" defined`,
      })
    }

    // Validate restart policy
    if (service.restart && !validRestartPolicies.includes(service.restart)) {
      const restartLine = findLineForKey(content, "restart", serviceLine - 1)
      const restartLineInfo = view.state.doc.line(
        Math.min(restartLine, view.state.doc.lines)
      )
      diagnostics.push({
        from: restartLineInfo.from,
        to: restartLineInfo.to,
        severity: "error",
        message: `Invalid restart policy: "${service.restart}". Valid values: ${validRestartPolicies.join(", ")}`,
      })
    }

    // Validate pull_policy
    if (service.pull_policy && !validPullPolicies.includes(service.pull_policy)) {
      const pullPolicyLine = findLineForKey(content, "pull_policy", serviceLine - 1)
      const pullPolicyLineInfo = view.state.doc.line(
        Math.min(pullPolicyLine, view.state.doc.lines)
      )
      diagnostics.push({
        from: pullPolicyLineInfo.from,
        to: pullPolicyLineInfo.to,
        severity: "error",
        message: `Invalid pull_policy: "${service.pull_policy}". Valid values: ${validPullPolicies.join(", ")}`,
      })
    }

    // Validate network_mode
    if (service.network_mode) {
      const isValidMode = validNetworkModes.includes(service.network_mode) ||
        validNetworkModePrefixes.some(prefix => service.network_mode!.startsWith(prefix))
      if (!isValidMode) {
        const networkModeLine = findLineForKey(content, "network_mode", serviceLine - 1)
        const networkModeLineInfo = view.state.doc.line(
          Math.min(networkModeLine, view.state.doc.lines)
        )
        diagnostics.push({
          from: networkModeLineInfo.from,
          to: networkModeLineInfo.to,
          severity: "error",
          message: `Invalid network_mode: "${service.network_mode}". Valid values: ${validNetworkModes.join(", ")}, service:<name>, container:<name>`,
        })
      }
    }

    // Validate deploy section
    if (service.deploy) {
      // Validate deploy mode
      if (service.deploy.mode && !validDeployModes.includes(service.deploy.mode)) {
        const deployLine = findLineForKey(content, "deploy", serviceLine - 1)
        const modeLine = findLineForKey(content, "mode", deployLine)
        const modeLineInfo = view.state.doc.line(
          Math.min(modeLine, view.state.doc.lines)
        )
        diagnostics.push({
          from: modeLineInfo.from,
          to: modeLineInfo.to,
          severity: "error",
          message: `Invalid deploy mode: "${service.deploy.mode}". Valid values: ${validDeployModes.join(", ")}`,
        })
      }

      // Validate deploy endpoint_mode
      if (service.deploy.endpoint_mode && !validEndpointModes.includes(service.deploy.endpoint_mode)) {
        const deployLine = findLineForKey(content, "deploy", serviceLine - 1)
        const endpointModeLine = findLineForKey(content, "endpoint_mode", deployLine)
        const endpointModeLineInfo = view.state.doc.line(
          Math.min(endpointModeLine, view.state.doc.lines)
        )
        diagnostics.push({
          from: endpointModeLineInfo.from,
          to: endpointModeLineInfo.to,
          severity: "error",
          message: `Invalid endpoint_mode: "${service.deploy.endpoint_mode}". Valid values: ${validEndpointModes.join(", ")}`,
        })
      }
    }

    // Validate develop.watch section
    if (service.develop?.watch && Array.isArray(service.develop.watch)) {
      const developLine = findLineForKey(content, "develop", serviceLine - 1)
      for (const watchRule of service.develop.watch) {
        if (watchRule && typeof watchRule === "object") {
          // Validate watch action
          if (watchRule.action && !validWatchActions.includes(watchRule.action)) {
            const watchLine = findLineForKey(content, "watch", developLine)
            const actionLine = findLineForKey(content, "action", watchLine)
            const actionLineInfo = view.state.doc.line(
              Math.min(actionLine, view.state.doc.lines)
            )
            diagnostics.push({
              from: actionLineInfo.from,
              to: actionLineInfo.to,
              severity: "error",
              message: `Invalid watch action: "${watchRule.action}". Valid values: ${validWatchActions.join(", ")}`,
            })
          }

          // Validate path is required
          if (!watchRule.path) {
            const watchLine = findLineForKey(content, "watch", developLine)
            const watchLineInfo = view.state.doc.line(
              Math.min(watchLine, view.state.doc.lines)
            )
            diagnostics.push({
              from: watchLineInfo.from,
              to: watchLineInfo.to,
              severity: "error",
              message: "Watch rule must have a 'path' defined",
            })
          }

          // Validate target is required for sync actions
          if (watchRule.action?.startsWith("sync") && !watchRule.target) {
            const watchLine = findLineForKey(content, "watch", developLine)
            const watchLineInfo = view.state.doc.line(
              Math.min(watchLine, view.state.doc.lines)
            )
            diagnostics.push({
              from: watchLineInfo.from,
              to: watchLineInfo.to,
              severity: "error",
              message: `Watch action "${watchRule.action}" requires a 'target' path`,
            })
          }
        }
      }
    }

    // Validate port mappings
    if (service.ports && Array.isArray(service.ports)) {
      for (const port of service.ports) {
        if (typeof port === "string" || typeof port === "number") {
          if (!isValidPortMapping(port)) {
            const portsLine = findLineForKey(content, "ports", serviceLine - 1)
            const portsLineInfo = view.state.doc.line(
              Math.min(portsLine, view.state.doc.lines)
            )
            diagnostics.push({
              from: portsLineInfo.from,
              to: portsLineInfo.to,
              severity: "error",
              message: `Invalid port mapping: "${port}"`,
            })
          }
        }
      }
    }

    // Validate depends_on references
    if (service.depends_on) {
      const deps = Array.isArray(service.depends_on)
        ? service.depends_on
        : Object.keys(service.depends_on)

      for (const dep of deps) {
        if (typeof dep === "string" && !doc.services[dep]) {
          const depsLine = findLineForKey(content, "depends_on", serviceLine - 1)
          const depsLineInfo = view.state.doc.line(
            Math.min(depsLine, view.state.doc.lines)
          )
          diagnostics.push({
            from: depsLineInfo.from,
            to: depsLineInfo.to,
            severity: "error",
            message: `Service "${serviceName}" depends on undefined service: "${dep}"`,
          })
        }
      }
    }

    // Validate network references
    if (service.networks && doc.networks) {
      const serviceNetworks = Array.isArray(service.networks)
        ? service.networks
        : Object.keys(service.networks)

      for (const network of serviceNetworks) {
        if (
          typeof network === "string" &&
          !doc.networks[network] &&
          network !== "default"
        ) {
          const networksLine = findLineForKey(content, "networks", serviceLine - 1)
          const networksLineInfo = view.state.doc.line(
            Math.min(networksLine, view.state.doc.lines)
          )
          diagnostics.push({
            from: networksLineInfo.from,
            to: networksLineInfo.to,
            severity: "warning",
            message: `Service "${serviceName}" references undefined network: "${network}"`,
          })
        }
      }
    }
  }

  // Validate named volumes are used
  if (doc.volumes) {
    for (const volumeName of Object.keys(doc.volumes)) {
      let volumeUsed = false
      if (doc.services) {
        for (const service of Object.values(doc.services)) {
          if (service.volumes) {
            for (const vol of service.volumes) {
              if (typeof vol === "string" && vol.startsWith(`${volumeName}:`)) {
                volumeUsed = true
                break
              }
            }
          }
          if (volumeUsed) break
        }
      }
      if (!volumeUsed && doc.volumes[volumeName] !== null) {
        const volumeLine = findLineForKey(content, volumeName)
        const volumeLineInfo = view.state.doc.line(
          Math.min(volumeLine, view.state.doc.lines)
        )
        diagnostics.push({
          from: volumeLineInfo.from,
          to: volumeLineInfo.to,
          severity: "info",
          message: `Volume "${volumeName}" is defined but not used by any service`,
        })
      }
    }
  }

  // Validate configs are defined correctly and used
  if (doc.configs) {
    for (const [configName, config] of Object.entries(doc.configs)) {
      // Check config has a valid source (file, environment, content, or external)
      if (config && typeof config === "object" && !config.external) {
        if (!config.file && !config.environment && !config.content) {
          const configLine = findLineForKey(content, configName)
          const configLineInfo = view.state.doc.line(
            Math.min(configLine, view.state.doc.lines)
          )
          diagnostics.push({
            from: configLineInfo.from,
            to: configLineInfo.to,
            severity: "warning",
            message: `Config "${configName}" should have "file", "environment", "content", or "external" defined`,
          })
        }
      }

      // Check if config is used by any service
      let configUsed = false
      if (doc.services) {
        for (const service of Object.values(doc.services)) {
          if (service.configs) {
            for (const cfg of service.configs) {
              const cfgName = typeof cfg === "string" ? cfg : (cfg as Record<string, unknown>)?.source
              if (cfgName === configName) {
                configUsed = true
                break
              }
            }
          }
          if (configUsed) break
        }
      }
      if (!configUsed && config !== null) {
        const configLine = findLineForKey(content, configName)
        const configLineInfo = view.state.doc.line(
          Math.min(configLine, view.state.doc.lines)
        )
        diagnostics.push({
          from: configLineInfo.from,
          to: configLineInfo.to,
          severity: "info",
          message: `Config "${configName}" is defined but not used by any service`,
        })
      }
    }
  }

  // Validate secrets are defined correctly and used
  if (doc.secrets) {
    for (const [secretName, secret] of Object.entries(doc.secrets)) {
      // Check secret has a valid source (file, environment, or external)
      if (secret && typeof secret === "object" && !secret.external) {
        if (!secret.file && !secret.environment) {
          const secretLine = findLineForKey(content, secretName)
          const secretLineInfo = view.state.doc.line(
            Math.min(secretLine, view.state.doc.lines)
          )
          diagnostics.push({
            from: secretLineInfo.from,
            to: secretLineInfo.to,
            severity: "warning",
            message: `Secret "${secretName}" should have "file", "environment", or "external" defined`,
          })
        }
      }

      // Check if secret is used by any service
      let secretUsed = false
      if (doc.services) {
        for (const service of Object.values(doc.services)) {
          if (service.secrets) {
            for (const sec of service.secrets) {
              const secName = typeof sec === "string" ? sec : (sec as Record<string, unknown>)?.source
              if (secName === secretName) {
                secretUsed = true
                break
              }
            }
          }
          if (secretUsed) break
        }
      }
      if (!secretUsed && secret !== null) {
        const secretLine = findLineForKey(content, secretName)
        const secretLineInfo = view.state.doc.line(
          Math.min(secretLine, view.state.doc.lines)
        )
        diagnostics.push({
          from: secretLineInfo.from,
          to: secretLineInfo.to,
          severity: "info",
          message: `Secret "${secretName}" is defined but not used by any service`,
        })
      }
    }
  }

  return diagnostics
}

// Main linter function
export function dockerComposeLinter(view: EditorView): Diagnostic[] {
  const content = view.state.doc.toString()

  // Skip empty content
  if (!content.trim()) {
    return []
  }

  // Combine syntax and schema validation
  const syntaxErrors = validateYamlSyntax(content)

  // Only run schema validation if there are no syntax errors
  if (syntaxErrors.length > 0) {
    return syntaxErrors
  }

  return validateDockerComposeSchema(content, view)
}
