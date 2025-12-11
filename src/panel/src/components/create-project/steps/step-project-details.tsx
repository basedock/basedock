import { useState, useEffect } from "react"
import { useQuery } from "@tanstack/react-query"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import {
  Field,
  FieldLabel,
  FieldGroup,
  FieldDescription,
} from "@/components/ui/field"
import { Check, X, Loader2 } from "lucide-react"
import { generateSlug } from "@/lib/slug"
import { checkSlugAvailability } from "@/api/sdk.gen"
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
  const [slugEdited, setSlugEdited] = useState(false)
  const [debouncedSlug, setDebouncedSlug] = useState(values.slug)

  // Debounce slug for API check
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSlug(values.slug)
    }, 300)
    return () => clearTimeout(timer)
  }, [values.slug])

  // Check slug availability with debounce
  const { data: slugCheck, isLoading: isCheckingSlug } = useQuery({
    queryKey: ["check-slug", debouncedSlug],
    queryFn: async () => {
      if (!debouncedSlug) return null
      const response = await checkSlugAvailability({
        query: { Slug: debouncedSlug },
      })
      return response.data
    },
    enabled: debouncedSlug.length > 0,
    staleTime: 5000,
  })

  // Auto-generate slug from name
  const handleNameChange = (name: string) => {
    onFieldChange("name", name)
    if (!slugEdited) {
      onFieldChange("slug", generateSlug(name))
    }
  }

  const handleSlugChange = (newSlug: string) => {
    setSlugEdited(true)
    onFieldChange("slug", generateSlug(newSlug))
  }

  const canProceed =
    values.name.length > 0 &&
    values.slug.length > 0 &&
    slugCheck?.isAvailable === true

  return (
    <div className="space-y-6">
      <FieldGroup>
        <Field>
          <FieldLabel htmlFor="name">Project Name</FieldLabel>
          <Input
            id="name"
            value={values.name}
            onChange={(e) => handleNameChange(e.target.value)}
            placeholder="My Awesome Project"
            autoComplete="off"
          />
        </Field>

        <Field>
          <FieldLabel htmlFor="slug">Slug</FieldLabel>
          <div className="relative">
            <Input
              id="slug"
              value={values.slug}
              onChange={(e) => handleSlugChange(e.target.value)}
              placeholder="my-awesome-project"
              className="pr-10"
            />
            <div className="absolute right-3 top-1/2 -translate-y-1/2">
              {isCheckingSlug && (
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
              )}
              {!isCheckingSlug && slugCheck?.isAvailable && (
                <Check className="h-4 w-4 text-green-500" />
              )}
              {!isCheckingSlug && values.slug && !slugCheck?.isAvailable && (
                <X className="h-4 w-4 text-destructive" />
              )}
            </div>
          </div>
          <FieldDescription>
            Used in URLs and folder names. Must be unique.
          </FieldDescription>
          {!slugCheck?.isAvailable && slugCheck?.suggestedSlug && (
            <button
              type="button"
              className="text-sm text-primary hover:underline"
              onClick={() => {
                onFieldChange("slug", slugCheck.suggestedSlug!)
                setSlugEdited(true)
              }}
            >
              Use suggested: {slugCheck.suggestedSlug}
            </button>
          )}
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
