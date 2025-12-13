import { createFileRoute } from "@tanstack/react-router"
import { getEnvironmentBySlug } from "@/api/sdk.gen"
import type { EnvironmentDetailDto } from "@/api/types.gen"
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs"
import { useState } from "react"
import { useAuth } from "@/contexts/auth-context"
import { ServicesTab } from "@/components/services/services-tab"
import { VolumesTab } from "@/components/volumes/volumes-tab"
import { NetworksTab } from "@/components/networks/networks-tab"
import { ConfigsTab } from "@/components/configs/configs-tab"
import { SecretsTab } from "@/components/secrets/secrets-tab"
import { ComposeViewer } from "@/components/compose/compose-viewer"
import {
  Container,
  HardDrive,
  Network,
  FileText,
  Key,
  FileCode,
} from "lucide-react"

export const Route = createFileRoute("/_dashboard/projects/$slug/$env")({
  loader: async ({ params }) => {
    const response = await getEnvironmentBySlug({
      path: { projectSlug: params.slug, envSlug: params.env }
    })
    if (response.error) throw new Error("Environment not found")
    return response.data as EnvironmentDetailDto
  },
  beforeLoad: ({ params }) => ({
    getTitle: () => params.env,
  }),
  component: EnvironmentDetailPage,
})

function EnvironmentDetailPage() {
  const { slug, env } = Route.useParams()
  const environment = Route.useLoaderData()
  const [activeTab, setActiveTab] = useState("services")
  const { user } = useAuth()
  const isAdmin = user?.isAdmin ?? false

  return (
    <div className="flex flex-col gap-6 p-6">
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-6">
          <TabsTrigger value="services" className="gap-2">
            <Container className="h-4 w-4" />
            <span className="hidden sm:inline">Services</span>
            <span className="text-xs text-muted-foreground">
              ({environment.services.length})
            </span>
          </TabsTrigger>
          <TabsTrigger value="volumes" className="gap-2">
            <HardDrive className="h-4 w-4" />
            <span className="hidden sm:inline">Volumes</span>
            <span className="text-xs text-muted-foreground">
              ({environment.volumes.length})
            </span>
          </TabsTrigger>
          <TabsTrigger value="networks" className="gap-2">
            <Network className="h-4 w-4" />
            <span className="hidden sm:inline">Networks</span>
            <span className="text-xs text-muted-foreground">
              ({environment.networks.length})
            </span>
          </TabsTrigger>
          <TabsTrigger value="configs" className="gap-2">
            <FileText className="h-4 w-4" />
            <span className="hidden sm:inline">Configs</span>
            <span className="text-xs text-muted-foreground">
              ({environment.configs.length})
            </span>
          </TabsTrigger>
          <TabsTrigger value="secrets" className="gap-2">
            <Key className="h-4 w-4" />
            <span className="hidden sm:inline">Secrets</span>
            <span className="text-xs text-muted-foreground">
              ({environment.secrets.length})
            </span>
          </TabsTrigger>
          <TabsTrigger value="compose" className="gap-2">
            <FileCode className="h-4 w-4" />
            <span className="hidden sm:inline">Compose</span>
          </TabsTrigger>
        </TabsList>

        <TabsContent value="services" className="mt-6">
          <ServicesTab
            projectSlug={slug}
            envSlug={env}
            isAdmin={isAdmin}
          />
        </TabsContent>

        <TabsContent value="volumes" className="mt-6">
          <VolumesTab
            projectSlug={slug}
            envSlug={env}
            isAdmin={isAdmin}
          />
        </TabsContent>

        <TabsContent value="networks" className="mt-6">
          <NetworksTab
            projectSlug={slug}
            envSlug={env}
            isAdmin={isAdmin}
          />
        </TabsContent>

        <TabsContent value="configs" className="mt-6">
          <ConfigsTab
            projectSlug={slug}
            envSlug={env}
            isAdmin={isAdmin}
          />
        </TabsContent>

        <TabsContent value="secrets" className="mt-6">
          <SecretsTab
            projectSlug={slug}
            envSlug={env}
            isAdmin={isAdmin}
          />
        </TabsContent>

        <TabsContent value="compose" className="mt-6">
          <ComposeViewer
            projectSlug={slug}
            envSlug={env}
            services={environment.services}
            volumes={environment.volumes}
            networks={environment.networks}
            configs={environment.configs}
            secrets={environment.secrets}
          />
        </TabsContent>
      </Tabs>
    </div>
  )
}
