import { AlertCircle, Container } from "lucide-react"
import { useForm } from "@tanstack/react-form"
import { useMutation } from "@tanstack/react-query"
import { createFileRoute, useNavigate } from "@tanstack/react-router"
import { z } from "zod"
import { loginMutation } from "@/api/@tanstack/react-query.gen"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import { Alert, AlertDescription } from "@/components/ui/alert"

const loginSchema = z.object({
  email: z.email("Please enter a valid email address"),
  password: z.string().min(8, "Password must be at least 8 characters"),
})

export const Route = createFileRoute('/login')({
  component: RouteComponent,
})

function RouteComponent() {
  const navigate = useNavigate()
  const login = useMutation({
    ...loginMutation(),
    onSuccess: () => {
      navigate({ to: "/" })
    },
  })

  const form = useForm({
    defaultValues: {
      email: "",
      password: "",
    },
    onSubmit: async ({ value }) => {
      login.mutate({
        body: {
          email: value.email,
          password: value.password,
        },
      })
    },
    validators: {
      onSubmit: ({ value }) => {
        const result = loginSchema.safeParse(value)
        if (!result.success) {
          return {
            fields: Object.fromEntries(
              result.error.issues.map((issue) => [
                issue.path.map(String).join("."),
                issue.message,
              ])
            ),
          }
        }
        return undefined
      },
    },
  })

  return (
    <div className="bg-muted flex min-h-svh flex-col items-center justify-center gap-6 p-6 md:p-10">
      <div className="flex w-full max-w-sm flex-col gap-6">
        <a href="#" className="flex items-center gap-2 self-center font-medium">
          <div className="bg-primary text-primary-foreground flex size-6 items-center justify-center rounded-md">
            <Container className="size-4" />
          </div>
          basedock
        </a>
        <div className="flex flex-col gap-6">
          <Card>
            <CardHeader className="text-center">
              <CardTitle className="text-xl">Welcome back</CardTitle>
              <CardDescription>
                Sign in to your basedock account
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form
                onSubmit={(e) => {
                  e.preventDefault()
                  e.stopPropagation()
                  form.handleSubmit()
                }}
              >
                <FieldGroup>
                  <form.Field name="email">
                    {(field) => {
                      const hasError =
                        field.state.meta.isTouched &&
                        field.state.meta.errors.length > 0
                      return (
                        <Field data-invalid={hasError || undefined}>
                          <FieldLabel htmlFor="email">Email</FieldLabel>
                          <Input
                            id="email"
                            type="email"
                            value={field.state.value}
                            onChange={(e) => field.handleChange(e.target.value)}
                            onBlur={field.handleBlur}
                            aria-invalid={hasError || undefined}
                          />
                          {hasError && (
                            <FieldError>
                              {field.state.meta.errors.join(", ")}
                            </FieldError>
                          )}
                        </Field>
                      )
                    }}
                  </form.Field>
                  <form.Field name="password">
                    {(field) => {
                      const hasError =
                        field.state.meta.isTouched &&
                        field.state.meta.errors.length > 0
                      return (
                        <Field data-invalid={hasError || undefined}>
                          <FieldLabel htmlFor="password">Password</FieldLabel>
                          <Input
                            id="password"
                            type="password"
                            value={field.state.value}
                            onChange={(e) => field.handleChange(e.target.value)}
                            onBlur={field.handleBlur}
                            aria-invalid={hasError || undefined}
                          />
                          {hasError && (
                            <FieldError>
                              {field.state.meta.errors.join(", ")}
                            </FieldError>
                          )}
                        </Field>
                      )
                    }}
                  </form.Field>
                  {login.error && (
                    <Alert variant="destructive">
                      <AlertCircle />
                      <AlertDescription>
                        {login.error.detail || "Invalid email or password"}
                      </AlertDescription>
                    </Alert>
                  )}
                  <form.Subscribe
                    selector={(state) => [state.canSubmit, state.isSubmitting]}
                  >
                    {([canSubmit, isSubmitting]) => (
                      <Button
                        type="submit"
                        className="w-full"
                        disabled={!canSubmit || login.isPending}
                      >
                        {isSubmitting || login.isPending ? "Signing in..." : "Sign in"}
                      </Button>
                    )}
                  </form.Subscribe>
                </FieldGroup>
              </form>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
