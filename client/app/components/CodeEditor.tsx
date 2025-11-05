import React from 'react'
import Editor from '@monaco-editor/react'
import { Box, CircularProgress } from '@mui/material'

interface CodeEditorProps {
  value: string
  onChange: (value: string | undefined) => void
  language: string
  height?: string
  theme?: 'vs-dark' | 'light' | 'vs'
  readOnly?: boolean
}

export function CodeEditor({
  value,
  onChange,
  language,
  height = '100%',
  theme = 'vs-dark',
  readOnly = false,
}: CodeEditorProps) {
  // Map language names to Monaco language identifiers
  const getMonacoLanguage = (lang: string): string => {
    const languageMap: Record<string, string> = {
      cpp: 'cpp',
      java: 'java',
      python: 'python',
      javascript: 'javascript',
      typescript: 'typescript',
      c: 'c',
      csharp: 'csharp',
      go: 'go',
      rust: 'rust',
      php: 'php',
      ruby: 'ruby',
      swift: 'swift',
      kotlin: 'kotlin',
    }
    return languageMap[lang.toLowerCase()] || 'plaintext'
  }

  const handleEditorChange = (value: string | undefined) => {
    onChange(value)
  }

  return (
    <Box sx={{ height, width: '100%' }}>
      <Editor
        height='100%'
        language={getMonacoLanguage(language)}
        value={value}
        onChange={handleEditorChange}
        theme={theme}
        options={{
          minimap: { enabled: true },
          fontSize: 14,
          fontFamily: "'Fira Code', 'Consolas', 'Monaco', monospace",
          fontLigatures: true,
          lineNumbers: 'on',
          roundedSelection: true,
          scrollBeyondLastLine: false,
          readOnly: readOnly,
          automaticLayout: true,
          tabSize: 4,
          wordWrap: 'off',
          formatOnPaste: true,
          formatOnType: true,
          suggestOnTriggerCharacters: true,
          acceptSuggestionOnEnter: 'on',
          quickSuggestions: {
            other: true,
            comments: false,
            strings: false,
          },
          parameterHints: {
            enabled: true,
          },
          bracketPairColorization: {
            enabled: true,
          },
          guides: {
            bracketPairs: true,
            indentation: true,
          },
          cursorBlinking: 'smooth',
          cursorSmoothCaretAnimation: 'on',
          smoothScrolling: true,
          contextmenu: true,
          mouseWheelZoom: true,
          links: true,
          folding: true,
          foldingStrategy: 'indentation',
          showFoldingControls: 'always',
          matchBrackets: 'always',
          renderLineHighlight: 'all',
          scrollbar: {
            vertical: 'visible',
            horizontal: 'visible',
            useShadows: true,
            verticalHasArrows: false,
            horizontalHasArrows: false,
            verticalScrollbarSize: 10,
            horizontalScrollbarSize: 10,
          },
        }}
        loading={
          <Box
            sx={{
              height: '100%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: '#1e1e1e',
            }}
          >
            <CircularProgress size={40} sx={{ color: '#007AFF' }} />
          </Box>
        }
      />
    </Box>
  )
}
