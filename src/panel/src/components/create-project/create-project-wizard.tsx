import { useState } from "react"
import { useNavigate } from "@tanstack/react-router"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card"
import { Progress } from "@/components/ui/progress"
import { WizardStepIndicator } from "./wizard-step-indicator"
import { StepProjectType } from "./steps/step-project-type"
import { StepProjectDetails } from "./steps/step-project-details"
import { StepConfiguration } from "./steps/step-configuration"
import { createProject } from "@/api/sdk.gen"
import type { CreateProjectFormData } from "./types"
import { buildDockerImageConfig, PROJECT_TYPE } from "./types"

const STEPS = [
  { id: "type", title: "Project Type", description: "Choose how to deploy" },
  { id: "details", title: "Project Details", description: "Name and description" },
  { id: "config", title: "Configuration", description: "Setup your project" },
] as const

const initialFormData: CreateProjectFormData = {
  projectType: null,
  name: "",
  slug: "",
  description: "",
  // Docker Image fields
  image: "",
  tag: "latest",
  ports: [],
  envVars: [],
  volumes: [],
  restartPolicy: "unless-stopped",
  networks: [],
  cpuLimit: "",
  memoryLimit: "",
  // Compose File fields
  composeFileContent: "",
}

export function CreateProjectWizard() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [currentStep, setCurrentStep] = useState(0)
  const [formData, setFormData] = useState<CreateProjectFormData>(initialFormData)

  const createMutation = useMutation({
    mutationFn: async (data: CreateProjectFormData) => {
      const response = await createProject({
        body: {
          name: data.name,
          slug: data.slug,
          description: data.description || null,
          projectType: data.projectType!,
          composeFileContent:
            data.projectType === PROJECT_TYPE.ComposeFile
              ? data.composeFileContent
              : null,
          dockerImageConfig: buildDockerImageConfig(data),
          memberIds: null,
        },
      })
      if (response.error) {
        const errorDetail = (response.error as { detail?: string })?.detail
        throw new Error(errorDetail || "Failed to create project")
      }
      return response.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["projects"] })
      navigate({ to: "/projects/$slug", params: { slug: data!.slug } })
    },
  })

  const handleFieldChange = <K extends keyof CreateProjectFormData>(
    field: K,
    value: CreateProjectFormData[K]
  ) => {
    setFormData((prev) => ({ ...prev, [field]: value }))
  }

  const handleNext = () => {
    if (currentStep < STEPS.length - 1) {
      setCurrentStep(currentStep + 1)
    }
  }

  const handleBack = () => {
    if (currentStep > 0) {
      setCurrentStep(currentStep - 1)
    }
  }

  const handleSubmit = () => {
    createMutation.mutate(formData)
  }

  const progress = ((currentStep + 1) / STEPS.length) * 100

  return (
    <div className="mx-auto w-full max-w-3xl space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Create New Project</h1>
        <p className="text-muted-foreground">
          Set up a new deployment in a few simple steps
        </p>
      </div>

      {/* Progress */}
      <div className="space-y-4">
        <Progress value={progress} className="h-2" />
        <WizardStepIndicator
          steps={STEPS}
          currentStep={currentStep}
          onStepClick={setCurrentStep}
        />
      </div>

      {/* Step Content */}
      <Card>
        <CardHeader>
          <CardTitle>{STEPS[currentStep].title}</CardTitle>
          <CardDescription>{STEPS[currentStep].description}</CardDescription>
        </CardHeader>
        <CardContent>
          {currentStep === 0 && (
            <StepProjectType
              values={formData}
              onFieldChange={handleFieldChange}
              onNext={handleNext}
            />
          )}
          {currentStep === 1 && (
            <StepProjectDetails
              values={formData}
              onFieldChange={handleFieldChange}
              onNext={handleNext}
              onBack={handleBack}
            />
          )}
          {currentStep === 2 && (
            <StepConfiguration
              values={formData}
              onFieldChange={handleFieldChange}
              onBack={handleBack}
              onSubmit={handleSubmit}
              isSubmitting={createMutation.isPending}
            />
          )}
        </CardContent>
      </Card>

      {/* Error display */}
      {createMutation.isError && (
        <div className="rounded-md border border-destructive bg-destructive/10 p-4 text-sm text-destructive">
          {createMutation.error.message}
        </div>
      )}
    </div>
  )
}
