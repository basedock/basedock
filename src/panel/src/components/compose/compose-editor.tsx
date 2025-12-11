import { useState } from "react"
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { Button } from "@/components/ui/button"
import { CodeMirrorEditor } from "@/components/compose-editor/codemirror-editor"
import { updateComposeFile } from "@/api/sdk.gen"
import { Loader2, Save } from "lucide-react"

interface ComposeEditorProps {
  projectId: string
  initialContent: string | null
  onSave?: () => void
}

export function ComposeEditor({ projectId, initialContent, onSave }: ComposeEditorProps) {
  const [content, setContent] = useState(initialContent ?? "")
  const [hasChanges, setHasChanges] = useState(false)
  const queryClient = useQueryClient()

  const saveMutation = useMutation({
    mutationFn: async () => {
      const response = await updateComposeFile({
        path: { projectId },
        body: { composeFileContent: content },
      })
      if (response.error) {
        const errorDetail = (response.error as { detail?: string })?.detail
        throw new Error(errorDetail || "Failed to save compose file")
      }
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["projects", projectId] })
      setHasChanges(false)
      onSave?.()
    },
  })

  const handleChange = (value: string) => {
    setContent(value)
    setHasChanges(value !== (initialContent ?? ""))
  }

  return (
    <div className="space-y-4">
      <CodeMirrorEditor
        value={content}
        onChange={handleChange}
        minHeight="400px"
      />
      {saveMutation.isError && (
        <div className="text-sm text-destructive">{saveMutation.error.message}</div>
      )}
      <div className="flex items-center justify-between">
        <div className="text-sm text-muted-foreground">
          {hasChanges ? "You have unsaved changes" : "No changes"}
        </div>
        <Button
          onClick={() => saveMutation.mutate()}
          disabled={saveMutation.isPending || !hasChanges}
          size="sm"
        >
          {saveMutation.isPending ? (
            <Loader2 className="h-4 w-4 mr-2 animate-spin" />
          ) : (
            <Save className="h-4 w-4 mr-2" />
          )}
          Save Compose File
        </Button>
      </div>
    </div>
  )
}
