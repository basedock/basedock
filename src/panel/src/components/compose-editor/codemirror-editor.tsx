import { useEffect, useRef, useCallback } from "react"
import { EditorState, Compartment } from "@codemirror/state"
import {
  EditorView,
  keymap,
  lineNumbers,
  highlightActiveLine,
  highlightActiveLineGutter,
  placeholder as placeholderExt,
} from "@codemirror/view"
import {
  defaultKeymap,
  history,
  historyKeymap,
  indentWithTab,
} from "@codemirror/commands"
import { yaml } from "@codemirror/lang-yaml"
import {
  indentOnInput,
  bracketMatching,
  foldGutter,
  foldKeymap,
} from "@codemirror/language"
import {
  autocompletion,
  completionKeymap,
  closeBrackets,
  closeBracketsKeymap,
} from "@codemirror/autocomplete"
import { linter, lintGutter, lintKeymap } from "@codemirror/lint"
import { searchKeymap, highlightSelectionMatches } from "@codemirror/search"
import { cn } from "@/lib/utils"
import { createTheme } from "./codemirror-theme"
import { dockerComposeCompletions } from "./docker-compose-completions"
import { dockerComposeLinter } from "./docker-compose-linter"

interface CodeMirrorEditorProps {
  value: string
  onChange: (value: string) => void
  placeholder?: string
  className?: string
  minHeight?: string
}

export function CodeMirrorEditor({
  value,
  onChange,
  placeholder,
  className,
  minHeight = "400px",
}: CodeMirrorEditorProps) {
  const editorRef = useRef<HTMLDivElement>(null)
  const viewRef = useRef<EditorView | null>(null)
  const themeCompartment = useRef(new Compartment())
  const isExternalUpdate = useRef(false)

  // Detect dark mode
  const isDark = useCallback(() => {
    return (
      document.documentElement.classList.contains("dark") ||
      document.body.classList.contains("dark") ||
      document.querySelector(".dark") !== null
    )
  }, [])

  // Initialize editor
  useEffect(() => {
    if (!editorRef.current) return

    const updateListener = EditorView.updateListener.of((update) => {
      if (update.docChanged && !isExternalUpdate.current) {
        onChange(update.state.doc.toString())
      }
    })

    const extensions = [
      // Core features
      lineNumbers(),
      highlightActiveLine(),
      highlightActiveLineGutter(),
      history(),
      foldGutter(),
      bracketMatching(),
      closeBrackets(),
      indentOnInput(),
      highlightSelectionMatches(),

      // Theme (in compartment for dynamic switching)
      themeCompartment.current.of(createTheme(isDark())),

      // Language
      yaml(),

      // Autocomplete
      autocompletion({
        override: [dockerComposeCompletions],
        icons: true,
        defaultKeymap: true,
      }),

      // Linting
      linter(dockerComposeLinter, { delay: 300 }),
      lintGutter(),

      // Keymaps
      keymap.of([
        ...defaultKeymap,
        ...historyKeymap,
        ...foldKeymap,
        ...completionKeymap,
        ...closeBracketsKeymap,
        ...lintKeymap,
        ...searchKeymap,
        indentWithTab,
      ]),

      // Update listener
      updateListener,

      // Basic styling
      EditorView.theme({
        "&": { minHeight, height: "100%" },
        ".cm-scroller": { overflow: "auto" },
      }),
    ]

    // Add placeholder if provided
    if (placeholder) {
      extensions.push(placeholderExt(placeholder))
    }

    const state = EditorState.create({
      doc: value,
      extensions,
    })

    const view = new EditorView({
      state,
      parent: editorRef.current,
    })

    viewRef.current = view

    // Listen for theme changes on document and body
    const observer = new MutationObserver(() => {
      view.dispatch({
        effects: themeCompartment.current.reconfigure(createTheme(isDark())),
      })
    })

    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ["class"],
    })

    observer.observe(document.body, {
      attributes: true,
      attributeFilter: ["class"],
    })

    return () => {
      observer.disconnect()
      view.destroy()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []) // Only run once on mount

  // Sync external value changes
  useEffect(() => {
    const view = viewRef.current
    if (!view) return

    const currentContent = view.state.doc.toString()
    if (value !== currentContent) {
      isExternalUpdate.current = true
      view.dispatch({
        changes: { from: 0, to: currentContent.length, insert: value },
      })
      isExternalUpdate.current = false
    }
  }, [value])

  return (
    <div
      ref={editorRef}
      className={cn(
        "rounded-md border border-input bg-transparent shadow-xs",
        "focus-within:border-ring focus-within:ring-ring/50 focus-within:ring-[3px]",
        "overflow-hidden transition-[color,box-shadow]",
        className
      )}
    />
  )
}
