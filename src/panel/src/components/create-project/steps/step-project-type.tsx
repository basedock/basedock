import { Container, FileCode2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { cn } from "@/lib/utils"
import type { ProjectType } from "@/api/types.gen"
import type { CreateProjectFormData } from "../types"
import { PROJECT_TYPE } from "../types"

interface StepProjectTypeProps {
  values: CreateProjectFormData
  onFieldChange: <K extends keyof CreateProjectFormData>(
    field: K,
    value: CreateProjectFormData[K]
  ) => void
  onNext: () => void
}

const PROJECT_TYPES: Array<{
  type: ProjectType
  title: string
  description: string
  icon: React.ComponentType<{ className?: string }>
}> = [
  {
    type: PROJECT_TYPE.DockerImage,
    title: "Docker Image",
    description:
      "Deploy a container from a Docker image with full configuration options",
    icon: Container,
  },
  {
    type: PROJECT_TYPE.ComposeFile,
    title: "Docker Compose",
    description:
      "Define multi-container applications with a docker-compose.yml file",
    icon: FileCode2,
  },
]

export function StepProjectType({
  values,
  onFieldChange,
  onNext,
}: StepProjectTypeProps) {
  const selectedType = values.projectType

  return (
    <div className="space-y-6">
      <div className="grid gap-4 md:grid-cols-2">
        {PROJECT_TYPES.map(({ type, title, description, icon: Icon }) => (
          <Card
            key={type}
            className={cn(
              "cursor-pointer transition-colors hover:border-primary/50",
              selectedType === type && "border-primary ring-2 ring-primary/20"
            )}
            onClick={() => onFieldChange("projectType", type)}
          >
            <CardHeader>
              <div className="flex items-center gap-3">
                <div
                  className={cn(
                    "rounded-lg p-2",
                    selectedType === type
                      ? "bg-primary text-primary-foreground"
                      : "bg-muted"
                  )}
                >
                  <Icon className="h-6 w-6" />
                </div>
                <CardTitle className="text-lg">{title}</CardTitle>
              </div>
            </CardHeader>
            <CardContent>
              <CardDescription>{description}</CardDescription>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="flex justify-end">
        <Button onClick={onNext} disabled={selectedType === null}>
          Continue
        </Button>
      </div>
    </div>
  )
}
