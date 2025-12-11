"use client"

import { useQuery } from "@tanstack/react-query"
import { Link } from "@tanstack/react-router"
import {
  Folder,
  MoreHorizontal,
  Pencil,
  Trash2,
} from "lucide-react"

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import {
  SidebarGroup,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuAction,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "@/components/ui/sidebar"
import { getProjects } from "@/api/sdk.gen"
import type { ProjectDto } from "@/api/types.gen"
import { useAuth } from "@/contexts/auth-context"
import { Skeleton } from "@/components/ui/skeleton"

export function NavProjects() {
  const { isMobile } = useSidebar()
  const { isAuthenticated, user } = useAuth()
  const isAdmin = user?.isAdmin ?? false

  const { data: projects, isLoading } = useQuery({
    queryKey: ["projects"],
    queryFn: async () => {
      const response = await getProjects()
      if (response.error) throw new Error("Failed to fetch projects")
      return response.data as ProjectDto[]
    },
    enabled: isAuthenticated,
  })

  return (
    <SidebarGroup className="group-data-[collapsible=icon]:hidden">
      <SidebarGroupLabel>Projects</SidebarGroupLabel>
      <SidebarMenu>
        {isLoading ? (
          <>
            <SidebarMenuItem>
              <Skeleton className="h-8 w-full" />
            </SidebarMenuItem>
            <SidebarMenuItem>
              <Skeleton className="h-8 w-full" />
            </SidebarMenuItem>
          </>
        ) : projects && projects.length > 0 ? (
          <>
            {projects.slice(0, 5).map((project) => (
              <SidebarMenuItem key={project.id}>
                <SidebarMenuButton asChild>
                  <Link to="/projects/$slug" params={{ slug: project.slug }}>
                    <Folder />
                    <span>{project.name}</span>
                  </Link>
                </SidebarMenuButton>
                {isAdmin && (
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <SidebarMenuAction showOnHover>
                        <MoreHorizontal />
                        <span className="sr-only">More</span>
                      </SidebarMenuAction>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent
                      className="w-48"
                      side={isMobile ? "bottom" : "right"}
                      align={isMobile ? "end" : "start"}
                    >
                      <DropdownMenuItem asChild>
                        <Link to="/projects/$slug" params={{ slug: project.slug }}>
                          <Pencil className="text-muted-foreground" />
                          <span>Edit Project</span>
                        </Link>
                      </DropdownMenuItem>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem className="text-destructive">
                        <Trash2 className="text-muted-foreground" />
                        <span>Delete Project</span>
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                )}
              </SidebarMenuItem>
            ))}
          </>
        ) : (
          <SidebarMenuItem>
            <SidebarMenuButton asChild>
              <Link to="/projects">
                <Folder />
                <span className="text-muted-foreground">No projects</span>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        )}
        <SidebarMenuItem>
          <SidebarMenuButton asChild>
            <Link to="/projects">
              <MoreHorizontal />
              <span>View All Projects</span>
            </Link>
          </SidebarMenuButton>
        </SidebarMenuItem>
      </SidebarMenu>
    </SidebarGroup>
  )
}
