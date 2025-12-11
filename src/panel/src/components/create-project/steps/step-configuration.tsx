import { StepDockerImageConfig } from "./step-docker-image-config"
import { StepComposeConfig } from "./step-compose-config"
import type { CreateProjectFormData } from "../types"
import { PROJECT_TYPE } from "../types"

interface StepConfigurationProps {
  values: CreateProjectFormData
  onFieldChange: <K extends keyof CreateProjectFormData>(
    field: K,
    value: CreateProjectFormData[K]
  ) => void
  onBack: () => void
  onSubmit: () => void
  isSubmitting: boolean
}

export function StepConfiguration(props: StepConfigurationProps) {
  const projectType = props.values.projectType

  if (projectType === PROJECT_TYPE.DockerImage) {
    return <StepDockerImageConfig {...props} />
  }

  return <StepComposeConfig {...props} />
}
