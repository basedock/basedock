import { createFileRoute, redirect, useRouter } from '@tanstack/react-router'
import { useForm } from '@tanstack/react-form'
import { z } from 'zod'
import { useState } from 'react'
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
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert"
import { Checkbox } from "@/components/ui/checkbox"
import { AlertCircle, GalleryVerticalEnd } from "lucide-react"

export const Route = createFileRoute('/login')({
    validateSearch: (search: Record<string, unknown>) => ({
        redirect: (search.redirect as string) || '/',
    }),
    beforeLoad: ({ context, search }) => {
        if (context.auth.isAuthenticated) {
            throw redirect({ to: search.redirect })
        }
    },
    component: LoginPage,
})

const loginSchema = z.object({
    email: z.email('Invalid email address'),
    password: z.string().min(1, 'Password is required'),
    rememberMe: z.boolean(),
})

function LoginPage() {
    const router = useRouter()
    const { auth } = Route.useRouteContext()
    const { redirect: redirectUrl } = Route.useSearch()
    const [error, setError] = useState<string | null>(null)
    const [isLoading, setIsLoading] = useState(false)

    const form = useForm({
        defaultValues: {
            email: '',
            password: '',
            rememberMe: false,
        },
        validators: {
            onChange: loginSchema,
        },
        onSubmit: async ({ value }) => {
            setError(null)
            setIsLoading(true)
            try {
                await auth.login(value.email, value.password, value.rememberMe)
                router.history.push(redirectUrl)
            } catch {
                setError('Invalid email or password')
            } finally {
                setIsLoading(false)
            }
        },
    })

    return (
        <div className="bg-muted flex min-h-svh flex-col items-center justify-center gap-6 p-6 md:p-10">
            <div className="flex w-full max-w-sm flex-col gap-6">
                <a href="#" className="flex items-center gap-2 self-center font-medium">
                    <div className="bg-primary text-primary-foreground flex size-6 items-center justify-center rounded-md">
                        <GalleryVerticalEnd className="size-4" />
                    </div>
                    BaseDock
                </a>
                <Card>
                    <CardHeader className="text-center">
                        <CardTitle className="text-xl">Welcome back</CardTitle>
                        <CardDescription>
                            Enter your credentials to access your account
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
                            {error && (
                                <Alert variant="destructive" className="mb-4">
                                    <AlertCircle className="h-4 w-4" />
                                    <AlertTitle>Error</AlertTitle>
                                    <AlertDescription>{error}</AlertDescription>
                                </Alert>
                            )}
                            <FieldGroup>
                                <form.Field
                                    name="email"
                                    children={(field) => (
                                        <Field>
                                            <FieldLabel htmlFor="email">Email</FieldLabel>
                                            <Input
                                                id="email"
                                                type="email"
                                                value={field.state.value}
                                                onChange={(e) => field.handleChange(e.target.value)}
                                                onBlur={field.handleBlur}
                                                aria-invalid={field.state.meta.errors.length > 0}
                                            />
                                            {field.state.meta.isTouched && field.state.meta.errors.length > 0 && (
                                                <FieldError>
                                                    {field.state.meta.errors.map((e) => typeof e === 'string' ? e : e?.message).filter(Boolean).join(', ')}
                                                </FieldError>
                                            )}
                                        </Field>
                                    )}
                                />
                                <form.Field
                                    name="password"
                                    children={(field) => (
                                        <Field>
                                            <FieldLabel htmlFor="password">Password</FieldLabel>
                                            <Input
                                                id="password"
                                                type="password"
                                                value={field.state.value}
                                                onChange={(e) => field.handleChange(e.target.value)}
                                                onBlur={field.handleBlur}
                                                aria-invalid={field.state.meta.errors.length > 0}
                                            />
                                            {field.state.meta.isTouched && field.state.meta.errors.length > 0 && (
                                                <FieldError>
                                                    {field.state.meta.errors.map((e) => typeof e === 'string' ? e : e?.message).filter(Boolean).join(', ')}
                                                </FieldError>
                                            )}
                                        </Field>
                                    )}
                                />
                                <form.Field
                                    name="rememberMe"
                                    children={({ state, handleChange, handleBlur }) => (
                                        <div className="flex items-center gap-2">
                                            <Checkbox
                                                id="rememberMe"
                                                checked={state.value}
                                                onCheckedChange={(checked) => handleChange(checked === true)}
                                                onBlur={handleBlur}
                                            />
                                            <label htmlFor="rememberMe" className="text-sm cursor-pointer">
                                                Remember me
                                            </label>
                                        </div>
                                    )}
                                />
                                <Field>
                                    <form.Subscribe
                                        selector={(state) => [state.canSubmit, state.isSubmitting]}
                                        children={([canSubmit, isSubmitting]) => (
                                            <Button
                                                type="submit"
                                                disabled={!canSubmit || isLoading}
                                            >
                                                {isSubmitting || isLoading ? 'Logging in...' : 'Login'}
                                            </Button>
                                        )}
                                    />
                                </Field>
                            </FieldGroup>
                        </form>
                    </CardContent>
                </Card>
            </div>
        </div>
    )
}
