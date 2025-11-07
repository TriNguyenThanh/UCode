import React from 'react'
import {
  Box,
  TextField,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  Tabs,
  Tab,
  Paper,
  Typography,
  CircularProgress,
} from '@mui/material'
import FormatBoldIcon from '@mui/icons-material/FormatBold'
import FormatItalicIcon from '@mui/icons-material/FormatItalic'
import FormatUnderlinedIcon from '@mui/icons-material/FormatUnderlined'
import FormatListBulletedIcon from '@mui/icons-material/FormatListBulleted'
import FormatListNumberedIcon from '@mui/icons-material/FormatListNumbered'
import ImageIcon from '@mui/icons-material/Image'
import LinkIcon from '@mui/icons-material/Link'
import CodeIcon from '@mui/icons-material/Code'
import VisibilityIcon from '@mui/icons-material/Visibility'
import CloudUploadIcon from '@mui/icons-material/CloudUpload'
import DescriptionIcon from '@mui/icons-material/Description'
import ReactMarkdown from 'react-markdown'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import { prism } from 'react-syntax-highlighter/dist/esm/styles/prism'
import { uploadFile } from '~/services/fileService'

interface MarkdownEditorProps {
  value: string
  onChange: (value: string) => void
  placeholder?: string
  rows?: number
  label?: string
  helperText?: string
}

interface ImageDialogState {
  open: boolean
  url: string
  alt: string
  uploading: boolean
  uploadProgress: number
  tabValue: number // 0 for URL, 1 for Upload
}

interface LinkDialogState {
  open: boolean
  url: string
  text: string
}

interface CodeDialogState {
  open: boolean
  code: string
  language: string
}

interface PdfDialogState {
  open: boolean
  url: string
  text: string
  uploading: boolean
  uploadProgress: number
  tabValue: number // 0 for URL, 1 for Upload
}

export function MarkdownEditor({
  value,
  onChange,
  placeholder,
  rows = 10,
  label,
  helperText,
}: MarkdownEditorProps) {
  const textareaRef = React.useRef<HTMLTextAreaElement>(null)
  const [previewDialogOpen, setPreviewDialogOpen] = React.useState(false)
  
  // Local state for smooth typing
  const [localValue, setLocalValue] = React.useState(value)
  
  // Sync local value when prop changes externally
  React.useEffect(() => {
    setLocalValue(value)
  }, [value])
  
  // Debounce timer ref
  const debounceTimerRef = React.useRef<NodeJS.Timeout | null>(null)
  
  const [imageDialog, setImageDialog] = React.useState<ImageDialogState>({
    open: false,
    url: '',
    alt: '',
    uploading: false,
    uploadProgress: 0,
    tabValue: 0,
  })
  
  const [linkDialog, setLinkDialog] = React.useState<LinkDialogState>({
    open: false,
    url: '',
    text: '',
  })
  
  const [codeDialog, setCodeDialog] = React.useState<CodeDialogState>({
    open: false,
    code: '',
    language: 'javascript',
  })

  const [pdfDialog, setPdfDialog] = React.useState<PdfDialogState>({
    open: false,
    url: '',
    text: '',
    uploading: false,
    uploadProgress: 0,
    tabValue: 0,
  })

  const insertText = (before: string, after: string = '', placeholder: string = '') => {
    const textarea = textareaRef.current
    if (!textarea) return

    const start = textarea.selectionStart
    const end = textarea.selectionEnd
    const selectedText = localValue.substring(start, end)
    const textToInsert = selectedText || placeholder

    const newText =
      localValue.substring(0, start) +
      before +
      textToInsert +
      after +
      localValue.substring(end)

    setLocalValue(newText)
    onChange(newText)

    // Set cursor position and select the inserted text
    setTimeout(() => {
      textarea.focus()
      const textStart = start + before.length
      const textEnd = textStart + textToInsert.length
      textarea.setSelectionRange(textStart, textEnd)
    }, 0)
  }

  // No longer need handleTextChange callback since we inline it

  const handleBold = () => {
    insertText('**', '**', 'bold text')
  }

  const handleItalic = () => {
    insertText('*', '*', 'italic text')
  }

  const handleBulletList = () => {
    const textarea = textareaRef.current
    if (!textarea) return

    const start = textarea.selectionStart
    const end = textarea.selectionEnd
    const selectedText = localValue.substring(start, end)

    if (selectedText) {
      const lines = selectedText.split('\n')
      const bulletedLines = lines.map((line) => (line.trim() ? `- ${line}` : line)).join('\n')
      
      const newText =
        localValue.substring(0, start) +
        bulletedLines +
        localValue.substring(end)
      
      setLocalValue(newText)
      onChange(newText)
    } else {
      insertText('- ', '', 'List item')
    }
  }

  const handleNumberedList = () => {
    const textarea = textareaRef.current
    if (!textarea) return

    const start = textarea.selectionStart
    const end = textarea.selectionEnd
    const selectedText = localValue.substring(start, end)

    if (selectedText) {
      const lines = selectedText.split('\n')
      const numberedLines = lines
        .map((line, index) => (line.trim() ? `${index + 1}. ${line}` : line))
        .join('\n')
      
      const newText =
        localValue.substring(0, start) +
        numberedLines +
        localValue.substring(end)
      
      setLocalValue(newText)
      onChange(newText)
    } else {
      insertText('1. ', '', 'List item')
    }
  }

  const handleImageInsert = () => {
    if (imageDialog.url) {
      insertText(`![${imageDialog.alt || 'image'}](${imageDialog.url})`)
      setImageDialog({ 
        open: false, 
        url: '', 
        alt: '', 
        uploading: false, 
        uploadProgress: 0, 
        tabValue: 0 
      })
    }
  }

  const handleImageUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    try {
      setImageDialog({ ...imageDialog, uploading: true, uploadProgress: 0 })
      
      const response = await uploadFile(file, 'Image')
      
      setImageDialog({ 
        ...imageDialog, 
        url: response.fileUrl,
        alt: file.name,
        uploading: false,
        uploadProgress: 100,
        tabValue: 0 // Switch to URL tab to show the uploaded URL
      })
    } catch (error) {
      console.error('Failed to upload image:', error)
      setImageDialog({ ...imageDialog, uploading: false, uploadProgress: 0 })
      // You may want to show an error snackbar here
    }
  }

  const handleLinkInsert = () => {
    if (linkDialog.url) {
      const linkText = linkDialog.text || linkDialog.url
      insertText(`[${linkText}](${linkDialog.url})`)
      setLinkDialog({ open: false, url: '', text: '' })
    }
  }

  const handleCodeInsert = () => {
    if (codeDialog.code) {
      const codeBlock = `\`\`\`${codeDialog.language}\n${codeDialog.code}\n\`\`\``
      insertText(codeBlock)
      setCodeDialog({ open: false, code: '', language: 'javascript' })
    }
  }

  const handleInlineCode = () => {
    insertText('`', '`', 'code')
  }

  const handlePdfInsert = () => {
    if (pdfDialog.url) {
      const linkText = pdfDialog.text || 'Download PDF'
      insertText(`[${linkText}](${pdfDialog.url})`)
      setPdfDialog({ 
        open: false, 
        url: '', 
        text: '',
        uploading: false,
        uploadProgress: 0,
        tabValue: 0
      })
    }
  }

  const handlePdfUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    try {
      setPdfDialog({ ...pdfDialog, uploading: true, uploadProgress: 0 })
      
      const response = await uploadFile(file, 'Document')
      
      setPdfDialog({ 
        ...pdfDialog, 
        url: response.fileUrl,
        text: file.name.replace('.pdf', ''),
        uploading: false,
        uploadProgress: 100,
        tabValue: 0 // Switch to URL tab to show the uploaded URL
      })
    } catch (error) {
      console.error('Failed to upload PDF:', error)
      setPdfDialog({ ...pdfDialog, uploading: false, uploadProgress: 0 })
    }
  }

  return (
    <Box>
      {label && (
        <Typography variant="h6" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
          {label}
        </Typography>
      )}
      
      {/* Toolbar */}
      <Paper
        elevation={0}
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 0.5,
          p: 1,
          bgcolor: '#f5f5f7',
          border: '1px solid #d2d2d7',
          borderBottom: 'none',
          borderTopLeftRadius: 1,
          borderTopRightRadius: 1,
        }}
      >
        <Tooltip title="Bold (Ctrl+B)">
          <IconButton size="medium" onClick={handleBold} sx={{ color: '#1d1d1f' }}>
            <FormatBoldIcon fontSize="medium" />
          </IconButton>
        </Tooltip>

        <Tooltip title="Italic (Ctrl+I)">
          <IconButton size="medium" onClick={handleItalic} sx={{ color: '#1d1d1f' }}>
            <FormatItalicIcon fontSize="medium" />
          </IconButton>
        </Tooltip>


        <Tooltip title="Bullet List">
          <IconButton size="medium" onClick={handleBulletList} sx={{ color: '#1d1d1f' }}>
            <FormatListBulletedIcon fontSize="medium" />
          </IconButton>
        </Tooltip>

        <Tooltip title="Numbered List">
          <IconButton size="medium" onClick={handleNumberedList} sx={{ color: '#1d1d1f' }}>
            <FormatListNumberedIcon fontSize="medium" />
          </IconButton>
        </Tooltip>


        <Tooltip title="Insert Image">
          <IconButton
            size="medium"
            onClick={() => setImageDialog({ ...imageDialog, open: true })}
            sx={{ color: '#1d1d1f' }}
          >
            <ImageIcon fontSize="medium" />
          </IconButton>
        </Tooltip>

        <Tooltip title="Insert Link">
          <IconButton
            size="medium"
            onClick={() => setLinkDialog({ ...linkDialog, open: true })}
            sx={{ color: '#1d1d1f' }}
          >
            <LinkIcon fontSize="medium" />
          </IconButton>
        </Tooltip>

        <Tooltip title="Code Snippet">
          <IconButton
            size="medium"
            onClick={() => setCodeDialog({ ...codeDialog, open: true })}
            sx={{ color: '#1d1d1f' }}
          >
            <CodeIcon fontSize="medium" />
          </IconButton>
        </Tooltip>

        <Tooltip title="Insert PDF">
          <IconButton
            size="medium"
            onClick={() => setPdfDialog({ ...pdfDialog, open: true })}
            sx={{ color: '#1d1d1f' }}
          >
            <DescriptionIcon fontSize="medium" />
          </IconButton>
        </Tooltip>

        <Box sx={{ flexGrow: 1 }} />

        <Button
          size="medium"
          startIcon={<VisibilityIcon />}
          onClick={() => setPreviewDialogOpen(true)}
          sx={{
            color: '#1d1d1f',
            textTransform: 'none',
            fontWeight: 400,
          }}
        >
          Preview
        </Button>
      </Paper>

      {/* Editor */}
      <Box
        sx={{
          border: '1px solid #d2d2d7',
          borderTop: 'none',
          borderBottomLeftRadius: 1,
          borderBottomRightRadius: 1,
        }}
      >
        <textarea
          ref={textareaRef}
          rows={rows}
          value={localValue}
          onChange={(e) => {
            const newValue = e.target.value
            setLocalValue(newValue)
            
            if (debounceTimerRef.current) {
              clearTimeout(debounceTimerRef.current)
            }
            
            debounceTimerRef.current = setTimeout(() => {
              onChange(newValue)
            }, 300)
          }}
          placeholder={placeholder}
          onKeyDown={(e) => {
            if (e.ctrlKey || e.metaKey) {
              if (e.key === 'b') {
                e.preventDefault()
                handleBold()
              } else if (e.key === 'i') {
                e.preventDefault()
                handleItalic()
              } else if (e.key === 'k') {
                e.preventDefault()
                setLinkDialog({ ...linkDialog, open: true })
              }
            }
          }}
          style={{
            width: '100%',
            padding: '16.5px 14px',
            fontFamily: 'monospace',
            fontSize: '0.875rem',
            lineHeight: '1.5',
            border: 'none',
            outline: 'none',
            resize: 'vertical',
            backgroundColor: '#ffffff',
            color: '#1d1d1f',
          }}
        />
        {helperText && (
          <Typography 
            variant="caption" 
            sx={{ 
              px: 1.75, 
              pb: 0.5, 
              display: 'block',
              color: 'text.secondary' 
            }}
          >
            {helperText}
          </Typography>
        )}
      </Box>

      {/* Image Dialog */}
      <Dialog
        open={imageDialog.open}
        onClose={() => setImageDialog({ 
          open: false, 
          url: '', 
          alt: '', 
          uploading: false, 
          uploadProgress: 0, 
          tabValue: 0 
        })}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Insert Image</DialogTitle>
        <DialogContent>
          <Tabs
            value={imageDialog.tabValue}
            onChange={(e, newValue) => setImageDialog({ ...imageDialog, tabValue: newValue })}
            sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}
          >
            <Tab label="URL" />
            <Tab label="Upload" />
          </Tabs>

          {imageDialog.tabValue === 0 ? (
            <Stack spacing={2} sx={{ mt: 1 }}>
              <TextField
                fullWidth
                label="Image URL"
                value={imageDialog.url}
                onChange={(e) => setImageDialog({ ...imageDialog, url: e.target.value })}
                placeholder="https://example.com/image.png"
                autoFocus
              />
              <TextField
                fullWidth
                label="Alt Text (Optional)"
                value={imageDialog.alt}
                onChange={(e) => setImageDialog({ ...imageDialog, alt: e.target.value })}
                placeholder="Image description"
              />
            </Stack>
          ) : (
            <Stack spacing={2} sx={{ mt: 1 }}>
              <Button
                variant="outlined"
                component="label"
                startIcon={<CloudUploadIcon />}
                disabled={imageDialog.uploading}
                fullWidth
              >
                Choose Image
                <input
                  type="file"
                  hidden
                  accept="image/*"
                  onChange={handleImageUpload}
                />
              </Button>
              
              {imageDialog.uploading && (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <CircularProgress size={24} />
                  <Typography variant="body2" color="text.secondary">
                    Uploading...
                  </Typography>
                </Box>
              )}

              {imageDialog.url && !imageDialog.uploading && (
                <Stack spacing={1}>
                  <Typography variant="body2" color="success.main">
                    ✓ Image uploaded successfully
                  </Typography>
                  <TextField
                    fullWidth
                    label="Alt Text (Optional)"
                    value={imageDialog.alt}
                    onChange={(e) => setImageDialog({ ...imageDialog, alt: e.target.value })}
                    placeholder="Image description"
                  />
                </Stack>
              )}
            </Stack>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setImageDialog({ 
            open: false, 
            url: '', 
            alt: '', 
            uploading: false, 
            uploadProgress: 0, 
            tabValue: 0 
          })}>
            Cancel
          </Button>
          <Button 
            onClick={handleImageInsert} 
            variant="contained" 
            disabled={!imageDialog.url || imageDialog.uploading}
          >
            Insert
          </Button>
        </DialogActions>
      </Dialog>

      {/* Link Dialog */}
      <Dialog
        open={linkDialog.open}
        onClose={() => setLinkDialog({ open: false, url: '', text: '' })}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Insert Link</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              fullWidth
              label="URL"
              value={linkDialog.url}
              onChange={(e) => setLinkDialog({ ...linkDialog, url: e.target.value })}
              placeholder="https://example.com"
              autoFocus
            />
            <TextField
              fullWidth
              label="Link Text (Optional)"
              value={linkDialog.text}
              onChange={(e) => setLinkDialog({ ...linkDialog, text: e.target.value })}
              placeholder="Click here"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setLinkDialog({ open: false, url: '', text: '' })}>Cancel</Button>
          <Button onClick={handleLinkInsert} variant="contained" disabled={!linkDialog.url}>
            Insert
          </Button>
        </DialogActions>
      </Dialog>

      {/* Code Dialog */}
      <Dialog
        open={codeDialog.open}
        onClose={() => setCodeDialog({ open: false, code: '', language: 'javascript' })}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Insert Code Snippet</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              select
              fullWidth
              label="Language"
              value={codeDialog.language}
              onChange={(e) => setCodeDialog({ ...codeDialog, language: e.target.value })}
              SelectProps={{ native: true }}
            >
              <option value="javascript">JavaScript</option>
              <option value="typescript">TypeScript</option>
              <option value="python">Python</option>
              <option value="java">Java</option>
              <option value="cpp">C++</option>
              <option value="c">C</option>
              <option value="csharp">C#</option>
              <option value="go">Go</option>
              <option value="rust">Rust</option>
              <option value="php">PHP</option>
              <option value="ruby">Ruby</option>
              <option value="sql">SQL</option>
              <option value="bash">Bash</option>
              <option value="json">JSON</option>
              <option value="html">HTML</option>
              <option value="css">CSS</option>
            </TextField>
            <TextField
              fullWidth
              multiline
              rows={10}
              label="Code"
              value={codeDialog.code}
              onChange={(e) => setCodeDialog({ ...codeDialog, code: e.target.value })}
              placeholder="Enter your code here..."
              autoFocus
              sx={{
                '& .MuiInputBase-input': {
                  fontFamily: 'monospace',
                  fontSize: '0.875rem',
                },
              }}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button
            onClick={() => setCodeDialog({ open: false, code: '', language: 'javascript' })}
          >
            Cancel
          </Button>
          <Button onClick={handleCodeInsert} variant="contained" disabled={!codeDialog.code}>
            Insert
          </Button>
        </DialogActions>
      </Dialog>

      {/* PDF Dialog */}
      <Dialog
        open={pdfDialog.open}
        onClose={() => setPdfDialog({ 
          open: false, 
          url: '', 
          text: '',
          uploading: false,
          uploadProgress: 0,
          tabValue: 0
        })}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Insert PDF</DialogTitle>
        <DialogContent>
          <Tabs
            value={pdfDialog.tabValue}
            onChange={(e, newValue) => setPdfDialog({ ...pdfDialog, tabValue: newValue })}
            sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}
          >
            <Tab label="URL" />
            <Tab label="Upload" />
          </Tabs>

          {pdfDialog.tabValue === 0 ? (
            <Stack spacing={2} sx={{ mt: 1 }}>
              <TextField
                fullWidth
                label="PDF URL"
                value={pdfDialog.url}
                onChange={(e) => setPdfDialog({ ...pdfDialog, url: e.target.value })}
                placeholder="https://example.com/document.pdf"
                autoFocus
              />
              <TextField
                fullWidth
                label="Link Text (Optional)"
                value={pdfDialog.text}
                onChange={(e) => setPdfDialog({ ...pdfDialog, text: e.target.value })}
                placeholder="Download PDF"
              />
            </Stack>
          ) : (
            <Stack spacing={2} sx={{ mt: 1 }}>
              <Button
                variant="outlined"
                component="label"
                startIcon={<CloudUploadIcon />}
                disabled={pdfDialog.uploading}
                fullWidth
              >
                Choose PDF
                <input
                  type="file"
                  hidden
                  accept="application/pdf"
                  onChange={handlePdfUpload}
                />
              </Button>
              
              {pdfDialog.uploading && (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <CircularProgress size={24} />
                  <Typography variant="body2" color="text.secondary">
                    Uploading...
                  </Typography>
                </Box>
              )}

              {pdfDialog.url && !pdfDialog.uploading && (
                <Stack spacing={1}>
                  <Typography variant="body2" color="success.main">
                    ✓ PDF uploaded successfully
                  </Typography>
                  <TextField
                    fullWidth
                    label="Link Text (Optional)"
                    value={pdfDialog.text}
                    onChange={(e) => setPdfDialog({ ...pdfDialog, text: e.target.value })}
                    placeholder="Download PDF"
                  />
                </Stack>
              )}
            </Stack>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPdfDialog({ 
            open: false, 
            url: '', 
            text: '',
            uploading: false,
            uploadProgress: 0,
            tabValue: 0
          })}>
            Cancel
          </Button>
          <Button 
            onClick={handlePdfInsert} 
            variant="contained" 
            disabled={!pdfDialog.url || pdfDialog.uploading}
          >
            Insert
          </Button>
        </DialogActions>
      </Dialog>

      {/* Preview Dialog */}
      <Dialog
        open={previewDialogOpen}
        onClose={() => setPreviewDialogOpen(false)}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>
          Preview
        </DialogTitle>
        <DialogContent>
          <Paper
            elevation={0}
            sx={{
              p: 3,
              minHeight: 400,
              maxHeight: '70vh',
              overflow: 'auto',
              bgcolor: '#ffffff',
            }}
          >
            <ReactMarkdown
              components={{
                code({ className, children }) {
                  const match = /language-(\w+)/.exec(className || '')
                  const isInline = !match
                  
                  return !isInline && match ? (
                    <SyntaxHighlighter
                      style={prism as any}
                      language={match[1]}
                      PreTag="div"
                      customStyle={{
                        backgroundColor: '#f5f5f7',
                        border: '1px solid #d2d2d7',
                        marginBottom: '16px',
                      }}
                    >
                      {String(children).replace(/\n$/, '')}
                    </SyntaxHighlighter>
                  ) : (
                    <code
                      style={{
                        background: '#f5f5f7',
                        padding: '2px 6px',
                        borderRadius: '3px',
                        fontSize: '0.875em',
                        fontFamily: 'monospace',
                      }}
                    >
                      {children}
                    </code>
                  )
                },
                p({ children }) {
                  return <p style={{ marginBottom: '16px', lineHeight: '1.6' }}>{children}</p>
                },
                h1({ children }) {
                  return <h1 style={{ fontWeight: 600, marginBottom: '16px', marginTop: '24px' }}>{children}</h1>
                },
                h2({ children }) {
                  return <h2 style={{ fontWeight: 600, marginBottom: '16px', marginTop: '24px' }}>{children}</h2>
                },
                h3({ children }) {
                  return <h3 style={{ fontWeight: 600, marginBottom: '16px', marginTop: '24px' }}>{children}</h3>
                },
                h4({ children }) {
                  return <h4 style={{ fontWeight: 600, marginBottom: '16px', marginTop: '24px' }}>{children}</h4>
                },
                h5({ children }) {
                  return <h5 style={{ fontWeight: 600, marginBottom: '16px', marginTop: '24px' }}>{children}</h5>
                },
                h6({ children }) {
                  return <h6 style={{ fontWeight: 600, marginBottom: '16px', marginTop: '24px' }}>{children}</h6>
                },
                ul({ children }) {
                  return <ul style={{ marginBottom: '16px', paddingLeft: '32px', listStyleType: 'disc' }}>{children}</ul>
                },
                ol({ children }) {
                  return <ol style={{ marginBottom: '16px', paddingLeft: '32px', listStyleType: 'decimal' }}>{children}</ol>
                },
                li({ children }) {
                  return <li style={{ marginBottom: '8px', lineHeight: '1.6' }}>{children}</li>
                },
                img({ src, alt }) {
                  return <img src={src} alt={alt} style={{ maxWidth: '100%', height: 'auto' }} />
                },
              }}
            >
              {localValue || '*No content to preview*'}
            </ReactMarkdown>
          </Paper>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPreviewDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

