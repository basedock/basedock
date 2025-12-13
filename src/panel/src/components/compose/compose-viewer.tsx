import { useMemo } from "react"
import type {
  ServiceSummaryDto,
  VolumeSummaryDto,
  NetworkSummaryDto,
  ConfigSummaryDto,
  SecretSummaryDto,
} from "@/api/types.gen"
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
} from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { FileCode, Copy, Download, Info } from "lucide-react"
import { toast } from "sonner"

interface ComposeViewerProps {
  projectSlug: string
  envSlug: string
  services: ServiceSummaryDto[]
  volumes: VolumeSummaryDto[]
  networks: NetworkSummaryDto[]
  configs: ConfigSummaryDto[]
  secrets: SecretSummaryDto[]
}

export function ComposeViewer({
  projectSlug,
  envSlug,
  services,
  volumes,
  networks,
  configs,
  secrets,
}: ComposeViewerProps) {
  // Generate a preview of the compose file structure
  const composePreview = useMemo(() => {
    const lines: string[] = []

    lines.push("# Docker Compose file preview")
    lines.push(`# Project: ${projectSlug}`)
    lines.push(`# Environment: ${envSlug}`)
    lines.push("")
    lines.push("services:")

    if (services.length === 0) {
      lines.push("  # No services defined")
    } else {
      for (const service of services) {
        lines.push(`  ${service.name}:`)
        if (service.image) {
          lines.push(`    image: ${service.image}`)
        }
        if (service.dependsOn) {
          try {
            const deps = JSON.parse(service.dependsOn)
            const depNames = Object.keys(deps)
            if (depNames.length > 0) {
              lines.push("    depends_on:")
              for (const dep of depNames) {
                lines.push(`      - ${dep}`)
              }
            }
          } catch {
            // Invalid JSON, skip
          }
        }
        lines.push("")
      }
    }

    if (volumes.length > 0) {
      lines.push("volumes:")
      for (const volume of volumes) {
        lines.push(`  ${volume.name}:`)
        if (volume.driver !== "local") {
          lines.push(`    driver: ${volume.driver}`)
        }
      }
      lines.push("")
    }

    if (networks.length > 0) {
      lines.push("networks:")
      for (const network of networks) {
        lines.push(`  ${network.name}:`)
        if (network.driver !== "bridge") {
          lines.push(`    driver: ${network.driver}`)
        }
      }
      lines.push("")
    }

    if (configs.length > 0) {
      lines.push("configs:")
      for (const config of configs) {
        lines.push(`  ${config.name}:`)
        lines.push("    # Content managed by BaseDock")
      }
      lines.push("")
    }

    if (secrets.length > 0) {
      lines.push("secrets:")
      for (const secret of secrets) {
        lines.push(`  ${secret.name}:`)
        lines.push("    # Value managed by BaseDock")
      }
      lines.push("")
    }

    return lines.join("\n")
  }, [projectSlug, envSlug, services, volumes, networks, configs, secrets])

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(composePreview)
      toast.success("Copied to clipboard")
    } catch {
      toast.error("Failed to copy")
    }
  }

  const handleDownload = () => {
    const blob = new Blob([composePreview], { type: "text/yaml" })
    const url = URL.createObjectURL(blob)
    const a = document.createElement("a")
    a.href = url
    a.download = `docker-compose-${projectSlug}-${envSlug}.yml`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
    toast.success("Downloaded compose file")
  }

  const isEmpty = services.length === 0 && volumes.length === 0 &&
    networks.length === 0 && configs.length === 0 && secrets.length === 0

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Docker Compose</CardTitle>
            <CardDescription>
              Preview of the generated docker-compose.yml
            </CardDescription>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" onClick={handleCopy}>
              <Copy className="mr-2 h-4 w-4" />
              Copy
            </Button>
            <Button variant="outline" size="sm" onClick={handleDownload}>
              <Download className="mr-2 h-4 w-4" />
              Download
            </Button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        {isEmpty ? (
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <FileCode className="h-12 w-12 text-muted-foreground mb-4" />
            <h3 className="text-lg font-medium mb-2">No resources</h3>
            <p className="text-muted-foreground max-w-md">
              Add services, volumes, networks, configs, or secrets to generate
              a Docker Compose file.
            </p>
          </div>
        ) : (
          <>
            <div className="flex items-center gap-2 mb-4 p-3 bg-muted/50 rounded-md">
              <Info className="h-4 w-4 text-muted-foreground" />
              <p className="text-sm text-muted-foreground">
                This is a simplified preview. The actual compose file includes
                all service configurations, environment variables, and dependencies.
              </p>
            </div>
            <div className="rounded-md border bg-muted/30 p-4 overflow-x-auto">
              <pre className="text-sm font-mono whitespace-pre">{composePreview}</pre>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  )
}
