import { useState } from "react"
import { useForm } from "@tanstack/react-form"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Field, FieldLabel, FieldError, FieldGroup } from "@/components/ui/field"
import { Container, FileCode, Database, Layers, type LucideIcon } from "lucide-react"
import { cn } from "@/lib/utils"

type ResourceCategory = "application" | "database" | "compose"

interface ResourceType {
  type: string
  label: string
  icon: LucideIcon
  description: string
}

const resourceTypes: Record<ResourceCategory, ResourceType[]> = {
  application: [
    { type: "DockerImage", label: "Docker Image", icon: Container, description: "Deploy from container image" },
    { type: "Dockerfile", label: "Dockerfile", icon: FileCode, description: "Build from Dockerfile" },
  ],
  database: [
    { type: "PostgreSQL", label: "PostgreSQL", icon: Database, description: "Relational database" },
    { type: "Redis", label: "Redis", icon: Database, description: "In-memory data store" },
  ],
  compose: [
    { type: "DockerCompose", label: "Docker Compose", icon: Layers, description: "Multi-container app" },
  ],
}

const categoryTitles: Record<ResourceCategory, string> = {
  application: "Applications",
  database: "Databases",
  compose: "Compose",
}

const categoryDescriptions: Record<ResourceCategory, string> = {
  application: "Select an application type",
  database: "Select a database",
  compose: "Select a compose type",
}

interface CreateResourceDialogProps {
  category: ResourceCategory | null
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function CreateResourceDialog({
  category,
  open,
  onOpenChange,
}: CreateResourceDialogProps) {
  const [selectedType, setSelectedType] = useState<string | null>(null)

  const form = useForm({
    defaultValues: {
      name: "",
      description: "",
      // PostgreSQL fields
      databaseName: "",
      databaseUser: "postgres",
      databasePassword: "",
      // Redis fields
      password: "",
      // Shared version field
      version: selectedType === "PostgreSQL" ? "16" : selectedType === "Redis" ? "7" : "",
      // DockerImage fields
      image: "",
      tag: "latest",
      // Dockerfile fields
      dockerfileContent: "",
      // DockerCompose fields
      composeFileContent: "",
    },
    onSubmit: async ({ value }) => {
      // TODO: Implement API call when endpoint is available
      console.log("Creating resource:", { type: selectedType, ...value })
      handleClose()
    },
  })

  const handleClose = () => {
    setSelectedType(null)
    form.reset()
    onOpenChange(false)
  }

  const handleTypeSelect = (type: string) => {
    setSelectedType(type)
    // Set default version based on type
    if (type === "PostgreSQL") {
      form.setFieldValue("version", "16")
    } else if (type === "Redis") {
      form.setFieldValue("version", "7")
    }
  }

  if (!category) return null

  const types = resourceTypes[category]
  const title = categoryTitles[category]
  const description = categoryDescriptions[category]
  const selectedTypeInfo = types.find((t) => t.type === selectedType)

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>

        <form
          onSubmit={(e) => {
            e.preventDefault()
            form.handleSubmit()
          }}
        >
          {/* Type Selection Grid */}
          <div className="grid grid-cols-2 gap-3 py-4">
            {types.map((resource) => {
              const Icon = resource.icon
              const isSelected = selectedType === resource.type
              return (
                <button
                  key={resource.type}
                  type="button"
                  onClick={() => handleTypeSelect(resource.type)}
                  className={cn(
                    "flex flex-col items-center justify-center gap-2 rounded-lg border-2 bg-popover p-6 text-center transition-all hover:bg-accent",
                    isSelected
                      ? "border-primary bg-accent"
                      : "border-muted hover:border-primary/50"
                  )}
                >
                  <Icon className={cn("h-8 w-8", isSelected ? "text-primary" : "text-muted-foreground")} />
                  <span className="font-medium">{resource.label}</span>
                </button>
              )
            })}
          </div>

          {/* Form Fields */}
          <FieldGroup className="py-4 space-y-4">
            {/* Common Fields: Name & Description */}
            <form.Field
              name="name"
              children={(field) => {
                const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                return (
                  <Field data-invalid={isInvalid}>
                    <FieldLabel htmlFor={field.name}>Name</FieldLabel>
                    <Input
                      id={field.name}
                      name={field.name}
                      value={field.state.value}
                      onBlur={field.handleBlur}
                      onChange={(e) => field.handleChange(e.target.value)}
                      aria-invalid={isInvalid}
                      placeholder="my-resource"
                      autoComplete="off"
                    />
                    {isInvalid && <FieldError errors={field.state.meta.errors} />}
                  </Field>
                )
              }}
            />

            <form.Field
              name="description"
              children={(field) => {
                const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                return (
                  <Field data-invalid={isInvalid}>
                    <FieldLabel htmlFor={field.name}>Description (optional)</FieldLabel>
                    <Textarea
                      id={field.name}
                      name={field.name}
                      value={field.state.value}
                      onBlur={field.handleBlur}
                      onChange={(e) => field.handleChange(e.target.value)}
                      aria-invalid={isInvalid}
                      placeholder="A brief description..."
                      rows={2}
                    />
                    {isInvalid && <FieldError errors={field.state.meta.errors} />}
                  </Field>
                )
              }}
            />

            {/* PostgreSQL Fields */}
            {selectedType === "PostgreSQL" && (
              <>
                <form.Field
                  name="databaseName"
                  children={(field) => {
                    const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name}>Database Name</FieldLabel>
                        <Input
                          id={field.name}
                          name={field.name}
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          placeholder="mydb"
                          autoComplete="off"
                        />
                        {isInvalid && <FieldError errors={field.state.meta.errors} />}
                      </Field>
                    )
                  }}
                />
                <form.Field
                  name="databaseUser"
                  children={(field) => {
                    const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name}>Database User</FieldLabel>
                        <Input
                          id={field.name}
                          name={field.name}
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          placeholder="postgres"
                          autoComplete="off"
                        />
                        {isInvalid && <FieldError errors={field.state.meta.errors} />}
                      </Field>
                    )
                  }}
                />
                <form.Field
                  name="databasePassword"
                  children={(field) => {
                    const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name}>Database Password</FieldLabel>
                        <Input
                          id={field.name}
                          name={field.name}
                          type="password"
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          placeholder="••••••••"
                          autoComplete="new-password"
                        />
                        {isInvalid && <FieldError errors={field.state.meta.errors} />}
                      </Field>
                    )
                  }}
                />
                <form.Field
                  name="version"
                  children={(field) => {
                    const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name}>Version</FieldLabel>
                        <Input
                          id={field.name}
                          name={field.name}
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          placeholder="16"
                          autoComplete="off"
                        />
                        {isInvalid && <FieldError errors={field.state.meta.errors} />}
                      </Field>
                    )
                  }}
                />
              </>
            )}

            {/* Redis Fields */}
            {selectedType === "Redis" && (
              <>
                <form.Field
                  name="password"
                  children={(field) => {
                    const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name}>Password (optional)</FieldLabel>
                        <Input
                          id={field.name}
                          name={field.name}
                          type="password"
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          placeholder="••••••••"
                          autoComplete="new-password"
                        />
                        {isInvalid && <FieldError errors={field.state.meta.errors} />}
                      </Field>
                    )
                  }}
                />
                <form.Field
                  name="version"
                  children={(field) => {
                    const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name}>Version</FieldLabel>
                        <Input
                          id={field.name}
                          name={field.name}
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          placeholder="7"
                          autoComplete="off"
                        />
                        {isInvalid && <FieldError errors={field.state.meta.errors} />}
                      </Field>
                    )
                  }}
                />
              </>
            )}

            {/* DockerImage Fields */}
            {selectedType === "DockerImage" && (
              <>
                <form.Field
                  name="image"
                  children={(field) => {
                    const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name}>Image</FieldLabel>
                        <Input
                          id={field.name}
                          name={field.name}
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          placeholder="nginx"
                          autoComplete="off"
                        />
                        {isInvalid && <FieldError errors={field.state.meta.errors} />}
                      </Field>
                    )
                  }}
                />
                <form.Field
                  name="tag"
                  children={(field) => {
                    const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                    return (
                      <Field data-invalid={isInvalid}>
                        <FieldLabel htmlFor={field.name}>Tag</FieldLabel>
                        <Input
                          id={field.name}
                          name={field.name}
                          value={field.state.value}
                          onBlur={field.handleBlur}
                          onChange={(e) => field.handleChange(e.target.value)}
                          aria-invalid={isInvalid}
                          placeholder="latest"
                          autoComplete="off"
                        />
                        {isInvalid && <FieldError errors={field.state.meta.errors} />}
                      </Field>
                    )
                  }}
                />
              </>
            )}

            {/* Dockerfile Fields */}
            {selectedType === "Dockerfile" && (
              <form.Field
                name="dockerfileContent"
                children={(field) => {
                  const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor={field.name}>Dockerfile</FieldLabel>
                      <Textarea
                        id={field.name}
                        name={field.name}
                        value={field.state.value}
                        onBlur={field.handleBlur}
                        onChange={(e) => field.handleChange(e.target.value)}
                        aria-invalid={isInvalid}
                        placeholder="FROM node:18-alpine&#10;WORKDIR /app&#10;..."
                        rows={6}
                        className="font-mono text-sm"
                      />
                      {isInvalid && <FieldError errors={field.state.meta.errors} />}
                    </Field>
                  )
                }}
              />
            )}

            {/* DockerCompose Fields */}
            {selectedType === "DockerCompose" && (
              <form.Field
                name="composeFileContent"
                children={(field) => {
                  const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor={field.name}>Compose File</FieldLabel>
                      <Textarea
                        id={field.name}
                        name={field.name}
                        value={field.state.value}
                        onBlur={field.handleBlur}
                        onChange={(e) => field.handleChange(e.target.value)}
                        aria-invalid={isInvalid}
                        placeholder="version: '3.8'&#10;services:&#10;  web:&#10;    image: nginx"
                        rows={6}
                        className="font-mono text-sm"
                      />
                      {isInvalid && <FieldError errors={field.state.meta.errors} />}
                    </Field>
                  )
                }}
              />
            )}
          </FieldGroup>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={!selectedType}>
              {selectedType ? `Create ${selectedTypeInfo?.label}` : "Create"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
