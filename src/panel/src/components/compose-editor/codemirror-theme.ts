import { EditorView } from "@codemirror/view"
import type { Extension } from "@codemirror/state"
import { HighlightStyle, syntaxHighlighting } from "@codemirror/language"
import { tags } from "@lezer/highlight"

// Base theme settings shared between light and dark
const baseTheme = EditorView.baseTheme({
  "&": {
    fontSize: "14px",
  },
  ".cm-content": {
    fontFamily:
      "source-code-pro, Menlo, Monaco, Consolas, 'Courier New', monospace",
    padding: "8px 0",
  },
  ".cm-scroller": {
    overflow: "auto",
  },
  ".cm-gutters": {
    borderRight: "1px solid var(--border)",
    paddingRight: "4px",
  },
  ".cm-lineNumbers .cm-gutterElement": {
    padding: "0 8px 0 16px",
    minWidth: "40px",
  },
  ".cm-foldGutter .cm-gutterElement": {
    padding: "0 4px",
  },
  // Autocomplete panel
  ".cm-tooltip.cm-tooltip-autocomplete": {
    border: "1px solid var(--border)",
    borderRadius: "var(--radius)",
    backgroundColor: "var(--popover)",
    boxShadow:
      "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)",
  },
  ".cm-tooltip.cm-tooltip-autocomplete > ul": {
    fontFamily:
      "source-code-pro, Menlo, Monaco, Consolas, 'Courier New', monospace",
    fontSize: "13px",
  },
  ".cm-tooltip.cm-tooltip-autocomplete > ul > li": {
    padding: "4px 8px",
  },
  ".cm-tooltip.cm-tooltip-autocomplete > ul > li[aria-selected]": {
    backgroundColor: "var(--accent)",
    color: "var(--accent-foreground)",
  },
  ".cm-completionLabel": {
    color: "var(--foreground)",
  },
  ".cm-completionDetail": {
    color: "var(--muted-foreground)",
    marginLeft: "8px",
    fontStyle: "italic",
  },
  // Lint panel and tooltips
  ".cm-tooltip.cm-tooltip-hover": {
    border: "1px solid var(--border)",
    borderRadius: "var(--radius)",
    backgroundColor: "var(--popover)",
    color: "var(--popover-foreground)",
    padding: "4px 8px",
  },
  ".cm-lintPoint-error:after": {
    borderBottomColor: "var(--destructive)",
  },
  ".cm-lintPoint-warning:after": {
    borderBottomColor: "oklch(0.75 0.15 85)",
  },
  ".cm-diagnostic-error": {
    borderLeftColor: "var(--destructive)",
  },
  ".cm-diagnostic-warning": {
    borderLeftColor: "oklch(0.75 0.15 85)",
  },
  // Search panel
  ".cm-panel.cm-search": {
    backgroundColor: "var(--muted)",
    padding: "8px",
  },
  ".cm-panel.cm-search input, .cm-panel.cm-search button": {
    fontFamily: "inherit",
  },
  ".cm-searchMatch": {
    backgroundColor: "oklch(0.85 0.15 85 / 0.4)",
  },
  ".cm-searchMatch-selected": {
    backgroundColor: "oklch(0.75 0.15 85 / 0.6)",
  },
})

// Light theme
const lightTheme = EditorView.theme(
  {
    "&": {
      backgroundColor: "var(--background)",
      color: "var(--foreground)",
    },
    ".cm-cursor": {
      borderLeftColor: "var(--foreground)",
    },
    "&.cm-focused .cm-selectionBackground, .cm-selectionBackground": {
      backgroundColor: "var(--accent)",
    },
    ".cm-gutters": {
      backgroundColor: "var(--muted)",
      color: "var(--muted-foreground)",
    },
    ".cm-activeLineGutter": {
      backgroundColor: "var(--accent)",
    },
    ".cm-activeLine": {
      backgroundColor: "oklch(0.97 0.001 286 / 0.5)",
    },
    ".cm-foldPlaceholder": {
      backgroundColor: "var(--muted)",
      border: "1px solid var(--border)",
      color: "var(--muted-foreground)",
    },
  },
  { dark: false }
)

// Dark theme
const darkTheme = EditorView.theme(
  {
    "&": {
      backgroundColor: "var(--background)",
      color: "var(--foreground)",
    },
    ".cm-cursor": {
      borderLeftColor: "var(--foreground)",
    },
    "&.cm-focused .cm-selectionBackground, .cm-selectionBackground": {
      backgroundColor: "var(--accent)",
    },
    ".cm-gutters": {
      backgroundColor: "var(--muted)",
      color: "var(--muted-foreground)",
    },
    ".cm-activeLineGutter": {
      backgroundColor: "var(--accent)",
    },
    ".cm-activeLine": {
      backgroundColor: "oklch(0.27 0.006 286 / 0.5)",
    },
    ".cm-foldPlaceholder": {
      backgroundColor: "var(--muted)",
      border: "1px solid var(--border)",
      color: "var(--muted-foreground)",
    },
  },
  { dark: true }
)

// Light syntax highlighting
const lightHighlightStyle = HighlightStyle.define([
  { tag: tags.keyword, color: "#d73a49" },
  { tag: tags.string, color: "#22863a" },
  { tag: tags.number, color: "#005cc5" },
  { tag: tags.bool, color: "#005cc5" },
  { tag: tags.null, color: "#005cc5" },
  { tag: tags.propertyName, color: "#6f42c1" },
  { tag: tags.comment, color: "#6a737d", fontStyle: "italic" },
  { tag: tags.operator, color: "#d73a49" },
  { tag: tags.punctuation, color: "#24292e" },
  { tag: tags.bracket, color: "#24292e" },
  { tag: tags.meta, color: "#6a737d" },
])

// Dark syntax highlighting
const darkHighlightStyle = HighlightStyle.define([
  { tag: tags.keyword, color: "#ff7b72" },
  { tag: tags.string, color: "#a5d6ff" },
  { tag: tags.number, color: "#79c0ff" },
  { tag: tags.bool, color: "#79c0ff" },
  { tag: tags.null, color: "#79c0ff" },
  { tag: tags.propertyName, color: "#d2a8ff" },
  { tag: tags.comment, color: "#8b949e", fontStyle: "italic" },
  { tag: tags.operator, color: "#ff7b72" },
  { tag: tags.punctuation, color: "#c9d1d9" },
  { tag: tags.bracket, color: "#c9d1d9" },
  { tag: tags.meta, color: "#8b949e" },
])

export function createTheme(isDark: boolean): Extension {
  return isDark
    ? [baseTheme, darkTheme, syntaxHighlighting(darkHighlightStyle)]
    : [baseTheme, lightTheme, syntaxHighlighting(lightHighlightStyle)]
}
