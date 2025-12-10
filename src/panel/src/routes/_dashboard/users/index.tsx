import { createFileRoute } from "@tanstack/react-router"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { useAuth } from "@/contexts/auth-context"
import {
  Empty,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
  EmptyDescription,
  EmptyContent,
} from "@/components/ui/empty"
import { Button } from "@/components/ui/button"
import { Users, Shield } from "lucide-react"
import { getUsers, deleteUser } from "@/api/sdk.gen"
import type { UserDto } from "@/api/types.gen"
import { CreateUserDialog } from "@/components/create-user-dialog"
import { useState, useMemo } from "react"
import { DataTable } from "./data-table"
import { getColumns } from "./columns"

function CreateUserButton() {
  const [open, setOpen] = useState(false)
  return (
    <>
      <Button onClick={() => setOpen(true)}>Create User</Button>
      <CreateUserDialog open={open} onOpenChange={setOpen} />
    </>
  )
}

export const Route = createFileRoute("/_dashboard/users/")({
  beforeLoad: () => ({
    getActions: ({ isAdmin }: { isAdmin: boolean }) =>
      isAdmin ? <CreateUserButton /> : null,
  }),
  component: UsersPage,
})

function UsersPage() {
  const { user, isAuthenticated } = useAuth()
  const queryClient = useQueryClient()

  const isAdmin = user?.isAdmin ?? false

  const { data: users, isLoading, error } = useQuery({
    queryKey: ["users"],
    queryFn: async () => {
      const response = await getUsers()
      if (response.error) throw new Error("Failed to fetch users")
      return response.data as UserDto[]
    },
    enabled: isAuthenticated && isAdmin,
  })

  const deleteMutation = useMutation({
    mutationFn: async (userId: string) => {
      const response = await deleteUser({ path: { id: userId } })
      if (response.error) throw new Error("Failed to delete user")
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] })
    },
  })

  const columns = useMemo(
    () =>
      getColumns({
        onDelete: (userToDelete) => deleteMutation.mutate(userToDelete.id),
        currentUserId: user?.id,
      }),
    [deleteMutation, user?.id]
  )

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

  if (isLoading) {
    return (
      <div className="flex min-h-svh items-center justify-center">
        <div className="text-muted-foreground">Loading...</div>
      </div>
    )
  }

  return (
    <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
      {error ? (
        <div className="text-destructive">Failed to load users</div>
      ) : !users || users.length === 0 ? (
        <Empty className="border">
          <EmptyHeader>
            <EmptyMedia variant="icon">
              <Users />
            </EmptyMedia>
            <EmptyTitle>No users</EmptyTitle>
            <EmptyDescription>
              No users have been created yet.
            </EmptyDescription>
          </EmptyHeader>
          <EmptyContent>
            <CreateUserButton />
          </EmptyContent>
        </Empty>
      ) : (
        <DataTable columns={columns} data={users} />
      )}
    </div>
  )
}
