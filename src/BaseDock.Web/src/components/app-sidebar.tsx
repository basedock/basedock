import { Calendar, Home, Inbox, Search, Settings } from 'lucide-react'

import {
    Sidebar,
    SidebarContent,
    SidebarGroup,
    SidebarGroupContent,
    SidebarGroupLabel,
    SidebarMenu,
    SidebarMenuButton,
    SidebarMenuItem,
} from '../components/ui/sidebar'

// Menu items.
const items = [
    {
        title: 'Dashboard',
        url: '/dashboard',
        icon: Home,
    },
    {
        title: 'Images',
        url: '/images',
        icon: Inbox,
    },
    {
        title: 'Containers',
        url: '/containers',
        icon: Calendar,
    },
    {
        title: 'Settings',
        url: '/settings',
        icon: Settings,
    },
]

export function AppSidebar() {
    return (
        <Sidebar>
            <SidebarContent>
                <SidebarGroup>
                    <SidebarGroupLabel>BaseDock</SidebarGroupLabel>
                    <SidebarGroupContent>
                        <SidebarMenu>
                            {items.map((item) => (
                                <SidebarMenuItem key={item.title}>
                                    <SidebarMenuButton asChild>
                                        <a href={item.url}>
                                            <item.icon />
                                            <span>{item.title}</span>
                                        </a>
                                    </SidebarMenuButton>
                                </SidebarMenuItem>
                            ))}
                        </SidebarMenu>
                    </SidebarGroupContent>
                </SidebarGroup>
            </SidebarContent>
        </Sidebar>
    )
}
