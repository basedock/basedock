import { Button } from "@/components/ui/button"
import { Field, FieldLabel, FieldDescription } from "@/components/ui/field"
import { CodeMirrorEditor } from "@/components/compose-editor/codemirror-editor"
import type { CreateProjectFormData } from "../types"

const COMPOSE_TEMPLATE = `version: '3.8'

services:
  app:
    image: nginx:latest
    ports:
      - "80:80"
    restart: unless-stopped
`

interface StepComposeConfigProps {
  values: CreateProjectFormData
  onFieldChange: <K extends keyof CreateProjectFormData>(
    field: K,
    value: CreateProjectFormData[K]
  ) => void
  onBack: () => void
  onSubmit: () => void
  isSubmitting: boolean
}

export function StepComposeConfig({
  values,
  onFieldChange,
  onBack,
  onSubmit,
  isSubmitting,
}: StepComposeConfigProps) {
  const content = values.composeFileContent

  const handleLoadTemplate = () => {
    onFieldChange("composeFileContent", COMPOSE_TEMPLATE)
  }

  return (
    <div className="space-y-6">
      <Field>
        <div className="flex items-center justify-between">
          <FieldLabel>Docker Compose File</FieldLabel>
          {!content && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={handleLoadTemplate}
            >
              Load Template
            </Button>
          )}
        </div>
        <FieldDescription>
          Define your services, networks, and volumes using Docker Compose syntax
        </FieldDescription>
        <div className="mt-2">
          <CodeMirrorEditor
            value={content}
            onChange={(value) => onFieldChange("composeFileContent", value)}
            minHeight="300px"
          />
        </div>
      </Field>

      <div className="flex justify-between">
        <Button variant="outline" onClick={onBack}>
          Back
        </Button>
        <Button onClick={onSubmit} disabled={isSubmitting || !content}>
          {isSubmitting ? "Creating..." : "Create Project"}
        </Button>
      </div>
    </div>
  )
}
