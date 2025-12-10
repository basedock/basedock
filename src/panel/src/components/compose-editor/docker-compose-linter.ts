import type { Diagnostic } from "@codemirror/lint"
import { EditorView } from "@codemirror/view"
import { parse, YAMLParseError } from "yaml"

interface DockerComposeService {
  image?: string
  build?: string | Record<string, unknown>
  ports?: (string | number | Record<string, unknown>)[]
  volumes?: (string | Record<string, unknown>)[]
  restart?: string
  depends_on?: string[] | Record<string, unknown>
  environment?: Record<string, unknown> | string[]
  networks?: string[] | Record<string, unknown>
  [key: string]: unknown
}

interface DockerComposeFile {
  version?: string
  services?: Record<string, DockerComposeService>
  volumes?: Record<string, unknown>
  networks?: Record<string, unknown>
  configs?: Record<string, unknown>
  secrets?: Record<string, unknown>
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

// Valid top-level keys
const validTopLevelKeys = [
  "version",
  "services",
  "volumes",
  "networks",
  "configs",
  "secrets",
  "name",
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
