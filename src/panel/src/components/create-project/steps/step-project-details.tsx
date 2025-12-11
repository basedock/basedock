import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import {
  Field,
  FieldLabel,
  FieldGroup,
} from "@/components/ui/field"
import type { CreateProjectFormData } from "../types"

interface StepProjectDetailsProps {
  values: CreateProjectFormData
  onFieldChange: <K extends keyof CreateProjectFormData>(
    field: K,
    value: CreateProjectFormData[K]
  ) => void
  onNext: () => void
  onBack: () => void
}

export function StepProjectDetails({
  values,
  onFieldChange,
  onNext,
  onBack,
}: StepProjectDetailsProps) {
  const canProceed = values.name.length > 0

  return (
    <div className="space-y-6">
      <FieldGroup>
        <Field>
          <FieldLabel htmlFor="name">Project Name</FieldLabel>
          <Input
            id="name"
            value={values.name}
            onChange={(e) => onFieldChange("name", e.target.value)}
            placeholder="My Awesome Project"
            autoComplete="off"
          />
        </Field>

        <Field>
          <FieldLabel htmlFor="description">Description (optional)</FieldLabel>
          <Textarea
            id="description"
            value={values.description}
            onChange={(e) => onFieldChange("description", e.target.value)}
            placeholder="A brief description of your project"
            rows={3}
          />
        </Field>
      </FieldGroup>

      <div className="flex justify-between">
        <Button variant="outline" onClick={onBack}>
          Back
        </Button>
        <Button onClick={onNext} disabled={!canProceed}>
          Continue
        </Button>
      </div>
    </div>
  )
}
