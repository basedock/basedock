import { createContext, useContext, useState, type ReactNode } from "react"

export interface BreadcrumbItem {
  label: string
  href?: string
}

interface DashboardHeaderContextType {
  breadcrumbs: BreadcrumbItem[]
  setBreadcrumbs: (items: BreadcrumbItem[]) => void
  actions: ReactNode
  setActions: (actions: ReactNode) => void
}

const DashboardHeaderContext = createContext<DashboardHeaderContextType | null>(null)

export function DashboardHeaderProvider({ children }: { children: ReactNode }) {
  const [breadcrumbs, setBreadcrumbs] = useState<BreadcrumbItem[]>([])
  const [actions, setActions] = useState<ReactNode>(null)

  return (
    <DashboardHeaderContext.Provider value={{ breadcrumbs, setBreadcrumbs, actions, setActions }}>
      {children}
    </DashboardHeaderContext.Provider>
  )
}

export function useDashboardHeader() {
  const context = useContext(DashboardHeaderContext)
  if (!context) {
    throw new Error("useDashboardHeader must be used within a DashboardHeaderProvider")
  }
  return context
}
