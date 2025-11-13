import React from 'react'
import { Box, CircularProgress } from '@mui/material'

interface CodeEditorProps {
  value: string
  onChange: (value: string | undefined) => void
  language: string
  height?: string
  theme?: 'vs-dark' | 'light' | 'vs'
  readOnly?: boolean
}

// Declare monaco global type
declare global {
  interface Window {
    monaco: any
    require: any
  }
}

export function CodeEditor({
  value,
  onChange,
  language,
  height = '100%',
  theme = 'vs-dark',
  readOnly = false,
}: CodeEditorProps) {
  const containerRef = React.useRef<HTMLDivElement>(null)
  const editorRef = React.useRef<any>(null)
  const [isLoading, setIsLoading] = React.useState(true)

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

  React.useEffect(() => {
    if (!containerRef.current) return

    // Load Monaco Editor from xaml.io
    const loadMonaco = () => {
      // Check if loader script already exists
      if (!document.getElementById('monaco-loader')) {
        const script = document.createElement('script')
        script.id = 'monaco-loader'
        script.src = 'https://xaml.io/monaco-editor/min/vs/loader.js'
        script.onload = () => initializeEditor()
        document.body.appendChild(script)
      } else if (window.require) {
        initializeEditor()
      }
    }

    const initializeEditor = () => {
      if (!window.require) {
        setTimeout(initializeEditor, 100)
        return
      }

      // Thiết lập đường dẫn cho AMD loader
      window.require.config({ 
        paths: { vs: 'https://xaml.io/monaco-editor/min/vs' } 
      })

      // Khởi tạo editor
      window.require(['vs/editor/editor.main'], () => {
        if (!containerRef.current || editorRef.current) return

        const editor = window.monaco.editor.create(containerRef.current, {
          value: value,
          language: getMonacoLanguage(language),
          theme: theme,
          automaticLayout: true,
          readOnly: readOnly,
        })

        editorRef.current = editor

        // Listen to content changes
        editor.onDidChangeModelContent(() => {
          const newValue = editor.getValue()
          onChange(newValue)
        })

        setIsLoading(false)
      })
    }

    loadMonaco()

    // Cleanup
    return () => {
      if (editorRef.current) {
        editorRef.current.dispose()
        editorRef.current = null
      }
    }
  }, []) // Empty dependency - only run once

  // Update editor value when prop changes
  React.useEffect(() => {
    if (editorRef.current && editorRef.current.getValue() !== value) {
      const position = editorRef.current.getPosition()
      editorRef.current.setValue(value)
      if (position) {
        editorRef.current.setPosition(position)
      }
    }
  }, [value])

  // Update language when prop changes
  React.useEffect(() => {
    if (editorRef.current && window.monaco) {
      const model = editorRef.current.getModel()
      if (model) {
        window.monaco.editor.setModelLanguage(model, getMonacoLanguage(language))
      }
    }
  }, [language])

  // Update theme when prop changes
  React.useEffect(() => {
    if (editorRef.current && window.monaco) {
      window.monaco.editor.setTheme(theme)
    }
  }, [theme])

  // Update readOnly when prop changes
  React.useEffect(() => {
    if (editorRef.current) {
      editorRef.current.updateOptions({ readOnly })
    }
  }, [readOnly])

  return (
    <Box sx={{ height, width: '100%', position: 'relative' }}>
      {isLoading && (
        <Box
          sx={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            bgcolor: '#1e1e1e',
            zIndex: 1,
          }}
        >
          <CircularProgress size={40} sx={{ color: '#007AFF' }} />
        </Box>
      )}
      <div ref={containerRef} style={{ height: '100%', width: '100%' }} />
    </Box>
  )
}

// Old implementation commented below
// export function CodeEditor({
//   value,
//   onChange,
//   language,
//   height = '100%',
//   theme = 'vs-dark',
//   readOnly = false,
// }: CodeEditorProps) {
//   const editorRef = React.useRef<any>(null)
//   const valueRef = React.useRef<string>(value)

//   // Map language names to Monaco language identifiers
//   const getMonacoLanguage = (lang: string): string => {
//     const languageMap: Record<string, string> = {
//       cpp: 'cpp',
//       java: 'java',
//       python: 'python',
//       javascript: 'javascript',
//       typescript: 'typescript',
//       c: 'c',
//       csharp: 'csharp',
//       go: 'go',
//       rust: 'rust',
//       php: 'php',
//       ruby: 'ruby',
//       swift: 'swift',
//       kotlin: 'kotlin',
//     }
//     return languageMap[lang.toLowerCase()] || 'plaintext'
//   }

//   const handleEditorDidMount = (editor: any, monaco: any) => {
//     editorRef.current = editor
    
//     // Configure language features
//     monaco.languages.registerCompletionItemProvider(getMonacoLanguage(language), {
//       provideCompletionItems: () => {
//         return { suggestions: [] }
//       }
//     })
    
//     // Enable all language features
//     monaco.languages.setLanguageConfiguration(getMonacoLanguage(language), {
//       wordPattern: /(-?\d*\.\d\w*)|([^\`\~\!\@\#\%\^\&\*\(\)\-\=\+\[\{\]\}\\\|\;\:\'\"\,\.\<\>\/\?\s]+)/g,
//     })
    
//     // Set initial value
//     if (value && editor.getValue() !== value) {
//       editor.setValue(value)
//     }
    
//     // Focus editor to trigger IntelliSense
//     editor.focus()
//   }

//   const handleEditorChange = (newValue: string | undefined) => {
//     valueRef.current = newValue || ''
//     onChange(newValue)
//   }

//   // Update editor value only when external value changes (not from typing)
//   React.useEffect(() => {
//     if (editorRef.current && value !== valueRef.current) {
//       const editor = editorRef.current
//       const currentValue = editor.getValue()
//       if (currentValue !== value) {
//         const position = editor.getPosition()
//         editor.setValue(value)
//         if (position) {
//           editor.setPosition(position)
//         }
//       }
//     }
//   }, [value])

//   return (
//     <Box sx={{ height, width: '100%' }}>
//       <Editor
//         height='100%'
//         language={getMonacoLanguage(language)}
//         defaultValue={value}
//         onChange={handleEditorChange}
//         onMount={handleEditorDidMount}
//         theme={theme}
//         keepCurrentModel={true}
//         options={{
//           minimap: { enabled: true },
//           fontSize: 14,
//           fontFamily: "'Fira Code', 'Consolas', 'Monaco', monospace",
//           fontLigatures: true,
//           lineNumbers: 'on',
//           roundedSelection: true,
//           scrollBeyondLastLine: false,
//           readOnly: readOnly,
//           automaticLayout: true,
//           tabSize: 4,
//           wordWrap: 'off',
//           formatOnPaste: false,
//           formatOnType: false,
//           // Autocomplete & IntelliSense settings
//           autoClosingBrackets: 'always',
//           autoClosingQuotes: 'always',
//           autoIndent: 'full',
//           suggestOnTriggerCharacters: true,
//           acceptSuggestionOnCommitCharacter: true,
//           acceptSuggestionOnEnter: 'on',
//           tabCompletion: 'on',
//           wordBasedSuggestions: 'allDocuments',
//           quickSuggestions: {
//             other: 'on',
//             comments: 'on',
//             strings: 'on',
//           },
//           quickSuggestionsDelay: 10,
//           suggestSelection: 'first',
//           suggest: {
//             filterGraceful: true,
//             snippetsPreventQuickSuggestions: false,
//             localityBonus: true,
//             shareSuggestSelections: true,
//             showWords: true,
//             showMethods: true,
//             showFunctions: true,
//             showConstructors: true,
//             showFields: true,
//             showVariables: true,
//             showClasses: true,
//             showStructs: true,
//             showInterfaces: true,
//             showModules: true,
//             showProperties: true,
//             showEvents: true,
//             showOperators: true,
//             showUnits: true,
//             showValues: true,
//             showConstants: true,
//             showEnums: true,
//             showEnumMembers: true,
//             showKeywords: true,
//             showSnippets: true,
//             showColors: true,
//             showFiles: true,
//             showReferences: true,
//             showFolders: true,
//             showTypeParameters: true,
//             showIssues: true,
//             showUsers: true,
//           },
//           parameterHints: {
//             enabled: true,
//             cycle: true,
//           },
//           snippetSuggestions: 'top',
//           bracketPairColorization: {
//             enabled: true,
//           },
//           guides: {
//             bracketPairs: true,
//             indentation: true,
//           },
//           cursorBlinking: 'smooth',
//           cursorSmoothCaretAnimation: 'off',
//           smoothScrolling: false,
//           contextmenu: true,
//           mouseWheelZoom: true,
//           links: true,
//           folding: true,
//           foldingStrategy: 'indentation',
//           showFoldingControls: 'always',
//           matchBrackets: 'always',
//           renderLineHighlight: 'all',
//           scrollbar: {
//             vertical: 'visible',
//             horizontal: 'visible',
//             useShadows: true,
//             verticalHasArrows: false,
//             horizontalHasArrows: false,
//             verticalScrollbarSize: 10,
//             horizontalScrollbarSize: 10,
//           },
//         }}
//         loading={
//           <Box
//             sx={{
//               height: '100%',
//               display: 'flex',
//               alignItems: 'center',
//               justifyContent: 'center',
//               bgcolor: '#1e1e1e',
//             }}
//           >
//             <CircularProgress size={40} sx={{ color: '#007AFF' }} />
//           </Box>
//         }
//       />
//     </Box>
//   )
// }
