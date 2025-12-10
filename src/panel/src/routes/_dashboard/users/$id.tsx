import { createFileRoute, useRouter } from "@tanstack/react-router"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { useAuth } from "@/contexts/auth-context"
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
} from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Empty,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
  EmptyDescription,
} from "@/components/ui/empty"
import { Shield } from "lucide-react"
import { getUserById, updateUser } from "@/api/sdk.gen"
import type { UserDto } from "@/api/types.gen"
import { useForm } from "@tanstack/react-form"
import { z } from "zod"
import { Field, FieldLabel, FieldError, FieldGroup } from "@/components/ui/field"

export const Route = createFileRoute("/_dashboard/users/$id")({
  loader: async ({ params }) => {
    const response = await getUserById({ path: { id: params.id } })
    if (response.error) throw new Error("User not found")
    return response.data as UserDto
  },
  head: ({ loaderData }) => ({
    meta: [{ title: `${loaderData?.displayName ?? 'User'} - Basedock` }],
  }),
  beforeLoad: () => ({
    getTitle: () => "User",
  }),
  component: UserDetailPage,
})

const userSchema = z.object({
  displayName: z.string().min(1, "Name is required").max(100, "Name must be 100 characters or less"),
  email: z.string().email("Invalid email address"),
})

function UserDetailPage() {
  const { id } = Route.useParams()
  const loadedUser = Route.useLoaderData()
  const router = useRouter()
  const { user: currentUser } = useAuth()
  const queryClient = useQueryClient()

  const isAdmin = currentUser?.isAdmin ?? false

  const updateMutation = useMutation({
    mutationFn: async (data: { displayName: string; email: string }) => {
      const response = await updateUser({
        path: { id },
        body: { displayName: data.displayName, email: data.email },
      })
      if (response.error) throw new Error("Failed to update user")
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] })
      router.invalidate()
    },
  })

  const form = useForm({
    defaultValues: {
      displayName: loadedUser?.displayName ?? "",
      email: loadedUser?.email ?? "",
    },
    validators: {
      onSubmit: userSchema,
    },
    onSubmit: async ({ value }) => {
      await updateMutation.mutateAsync({
        displayName: value.displayName,
        email: value.email,
      })
    },
  })

  // Update form when user loads
  if (loadedUser && form.state.values.displayName !== loadedUser.displayName) {
    form.setFieldValue("displayName", loadedUser.displayName)
    form.setFieldValue("email", loadedUser.email)
  }

  // Non-admin access denied
  if (!isAdmin) {
    return (
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <Empty className="border">
          <EmptyHeader>
            <EmptyMedia variant="icon">
              <Shield />
            </EmptyMedia>
            <EmptyTitle>Access Denied</EmptyTitle>
            <EmptyDescription>
              Only administrators can manage users.
            </EmptyDescription>
          </EmptyHeader>
        </Empty>
      </div>
    )
  }

  return (
    <div className="flex flex-1 flex-col gap-6 p-4 pt-0">
      <Card>
        <CardHeader>
          <CardTitle>User Details</CardTitle>
          <CardDescription>Update user information</CardDescription>
        </CardHeader>
        <CardContent>
          <form
            onSubmit={(e) => {
              e.preventDefault()
              form.handleSubmit()
            }}
          >
            <FieldGroup>
              <form.Field
                name="displayName"
                children={(field) => {
                  const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor={field.name}>Display Name</FieldLabel>
                      <Input
                        id={field.name}
                        name={field.name}
                        value={field.state.value}
                        onBlur={field.handleBlur}
                        onChange={(e) => field.handleChange(e.target.value)}
                        aria-invalid={isInvalid}
                        placeholder="User's display name"
                      />
                      {isInvalid && <FieldError errors={field.state.meta.errors} />}
                    </Field>
                  )
                }}
              />
              <form.Field
                name="email"
                children={(field) => {
                  const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid
                  return (
                    <Field data-invalid={isInvalid}>
                      <FieldLabel htmlFor={field.name}>Email</FieldLabel>
                      <Input
                        id={field.name}
                        name={field.name}
                        type="email"
                        value={field.state.value}
                        onBlur={field.handleBlur}
                        onChange={(e) => field.handleChange(e.target.value)}
                        aria-invalid={isInvalid}
                        placeholder="user@example.com"
                      />
                      {isInvalid && <FieldError errors={field.state.meta.errors} />}
                    </Field>
                  )
                }}
              />
            </FieldGroup>
            <div className="mt-4">
              <Button type="submit" disabled={updateMutation.isPending}>
                {updateMutation.isPending ? "Saving..." : "Save Changes"}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>User Information</CardTitle>
          <CardDescription>Read-only user details</CardDescription>
        </CardHeader>
        <CardContent>
          <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <dt className="text-sm font-medium text-muted-foreground">Role</dt>
              <dd className="mt-1">
                <span
                  className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                    loadedUser.isAdmin
                      ? "bg-primary/10 text-primary"
                      : "bg-muted text-muted-foreground"
                  }`}
                >
                  {loadedUser.isAdmin ? "Admin" : "User"}
                </span>
              </dd>
            </div>
            <div>
              <dt className="text-sm font-medium text-muted-foreground">Created</dt>
              <dd className="mt-1">{new Date(loadedUser.createdAt).toLocaleDateString()}</dd>
            </div>
            {loadedUser.updatedAt && (
              <div>
                <dt className="text-sm font-medium text-muted-foreground">Last Updated</dt>
                <dd className="mt-1">{new Date(loadedUser.updatedAt).toLocaleDateString()}</dd>
              </div>
            )}
          </dl>
        </CardContent>
      </Card>
    </div>
  )
}
