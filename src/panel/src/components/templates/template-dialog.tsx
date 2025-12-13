import { useState } from "react"
import type { TemplateDto } from "@/api/types.gen"
import { getTemplates, applyTemplate } from "@/api/sdk.gen"
import { useQuery } from "@tanstack/react-query"
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
import { Badge } from "@/components/ui/badge"
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card"
import { toast } from "sonner"
import { Loader2, Database, Globe, Server, ArrowLeft } from "lucide-react"

interface TemplateDialogProps {
  projectSlug: string
  envSlug: string
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
}

function getTemplateIcon(icon: string) {
  switch (icon) {
    case "database":
      return <Database className="h-5 w-5" />
    case "globe":
      return <Globe className="h-5 w-5" />
    case "server":
      return <Server className="h-5 w-5" />
    default:
      return <Server className="h-5 w-5" />
  }
}

export function TemplateDialog({
  projectSlug,
  envSlug,
  open,
  onOpenChange,
  onSuccess,
}: TemplateDialogProps) {
  const [selectedTemplate, setSelectedTemplate] = useState<TemplateDto | null>(null)
  const [parameters, setParameters] = useState<Record<string, string>>({})
  const [loading, setLoading] = useState(false)

  const { data: templates = [], isLoading } = useQuery({
    queryKey: ["templates"],
    queryFn: async () => {
      const response = await getTemplates()
      if (response.error) throw new Error("Failed to fetch templates")
      return response.data as TemplateDto[]
    },
    enabled: open,
  })

  const handleSelectTemplate = (template: TemplateDto) => {
    setSelectedTemplate(template)
    // Initialize parameters with defaults
    const defaults: Record<string, string> = {}
    template.parameters.forEach((param) => {
      if (param.defaultValue) {
        defaults[param.key] = param.defaultValue
      }
    })
    setParameters(defaults)
  }

  const handleApply = async () => {
    if (!selectedTemplate) return

    // Validate required parameters
    for (const param of selectedTemplate.parameters) {
      if (param.required && !parameters[param.key]) {
        toast.error(`${param.name} is required`)
        return
      }
    }

    setLoading(true)
    try {
      const response = await applyTemplate({
        path: { projectSlug, envSlug, templateId: selectedTemplate.id },
        body: { parameters },
      })

      if (response.error) {
        toast.error("Failed to apply template")
        return
      }

      toast.success(`${selectedTemplate.name} deployed successfully`)
      onSuccess()
      onOpenChange(false)
      setSelectedTemplate(null)
      setParameters({})
    } catch {
      toast.error("Failed to apply template")
    } finally {
      setLoading(false)
    }
  }

  const handleBack = () => {
    setSelectedTemplate(null)
    setParameters({})
  }

  const handleClose = (open: boolean) => {
    if (!open) {
      setSelectedTemplate(null)
      setParameters({})
    }
    onOpenChange(open)
  }

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {selectedTemplate ? (
              <div className="flex items-center gap-2">
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-6 w-6"
                  onClick={handleBack}
                >
                  <ArrowLeft className="h-4 w-4" />
                </Button>
                Configure {selectedTemplate.name}
              </div>
            ) : (
              "Deploy from Template"
            )}
          </DialogTitle>
          <DialogDescription>
            {selectedTemplate
              ? selectedTemplate.description
              : "Choose a pre-configured template to quickly deploy services"}
          </DialogDescription>
        </DialogHeader>

        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
          </div>
        ) : selectedTemplate ? (
          <div className="space-y-6">
            <div className="space-y-4">
              <div className="text-sm text-muted-foreground">
                Services that will be created:
              </div>
              <div className="flex flex-wrap gap-2">
                {selectedTemplate.services.map((service) => (
                  <Badge key={service.name} variant="secondary">
                    {service.name} ({service.image})
                  </Badge>
                ))}
              </div>
            </div>

            <div className="space-y-4">
              <div className="text-sm font-medium">Configuration</div>
              {selectedTemplate.parameters.map((param) => (
                <div key={param.key} className="space-y-2">
                  <Label htmlFor={param.key}>
                    {param.name}
                    {param.required && <span className="text-destructive ml-1">*</span>}
                  </Label>
                  <Input
                    id={param.key}
                    type={param.type === "password" ? "password" : "text"}
                    value={parameters[param.key] ?? ""}
                    onChange={(e) =>
                      setParameters({ ...parameters, [param.key]: e.target.value })
                    }
                    placeholder={param.defaultValue ?? undefined}
                  />
                  {param.description && (
                    <p className="text-xs text-muted-foreground">{param.description}</p>
                  )}
                </div>
              ))}
            </div>

            <div className="flex justify-end gap-2 pt-4 border-t">
              <Button variant="outline" onClick={handleBack}>
                Back
              </Button>
              <Button onClick={handleApply} disabled={loading}>
                {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Deploy
              </Button>
            </div>
          </div>
        ) : (
          <div className="grid gap-4 md:grid-cols-2">
            {templates.map((template) => (
              <Card
                key={template.id}
                className="cursor-pointer hover:border-primary transition-colors"
                onClick={() => handleSelectTemplate(template)}
              >
                <CardHeader>
                  <div className="flex items-center gap-3">
                    <div className="p-2 bg-muted rounded-md">
                      {getTemplateIcon(template.icon)}
                    </div>
                    <div className="flex-1">
                      <CardTitle className="text-base">{template.name}</CardTitle>
                      <CardDescription className="text-sm">
                        {template.description}
                      </CardDescription>
                    </div>
                  </div>
                  <Badge variant="outline" className="w-fit mt-2">
                    {template.category}
                  </Badge>
                </CardHeader>
              </Card>
            ))}
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}
