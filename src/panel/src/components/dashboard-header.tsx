import type { ReactNode } from "react"
import {
  Breadcrumb,
  BreadcrumbItem as BreadcrumbItemUI,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"

export interface BreadcrumbItem {
  label: string
  href?: string
}

interface DashboardHeaderProps {
  breadcrumbs: BreadcrumbItem[]
  actions?: ReactNode
}

export function DashboardHeader({ breadcrumbs, actions }: DashboardHeaderProps) {

  return (
    <header className="flex h-16 shrink-0 items-center gap-2">
      <div className="flex items-center gap-2 px-4">
        <SidebarTrigger className="-ml-1" />
        <Separator
          orientation="vertical"
          className="mr-2 data-[orientation=vertical]:h-4"
        />
        <Breadcrumb>
          <BreadcrumbList>
            {breadcrumbs.map((item, index) => {
              const isLast = index === breadcrumbs.length - 1
              return (
                <span key={item.label} className="contents">
                  <BreadcrumbItemUI className={isLast ? undefined : "hidden md:block"}>
                    {isLast || !item.href ? (
                      <BreadcrumbPage>{item.label}</BreadcrumbPage>
                    ) : (
                      <BreadcrumbLink href={item.href}>{item.label}</BreadcrumbLink>
                    )}
                  </BreadcrumbItemUI>
                  {!isLast && <BreadcrumbSeparator className="hidden md:block" />}
                </span>
              )
            })}
          </BreadcrumbList>
        </Breadcrumb>
      </div>
      {actions && <div className="ml-auto pr-4">{actions}</div>}
    </header>
  )
}
