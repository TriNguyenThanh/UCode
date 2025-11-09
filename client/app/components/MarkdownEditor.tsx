import React from 'react'
import {
  Box,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Stack,
  TextField,
  Tabs,
  Tab,
  CircularProgress,
} from '@mui/material'
import SimpleMdeReact from 'react-simplemde-editor'
import 'easymde/dist/easymde.min.css'
import { uploadFile } from '~/services/fileService'
import type { Options } from 'easymde'

interface MarkdownEditorProps {
  value: string
  onChange: (value: string) => void
  placeholder?: string
  rows?: number
  label?: string
  helperText?: string
  minHeight?: string
  maxHeight?: string
}

interface ImageDialogState {
  open: boolean
  url: string
  alt: string
  uploading: boolean
  tabValue: number
}

interface PdfDialogState {
  open: boolean
  url: string
  text: string
  uploading: boolean
  tabValue: number
}

export function MarkdownEditor({
  value,
  onChange,
  placeholder,
  rows = 10,
  label,
  helperText,
  minHeight,
  maxHeight,
}: MarkdownEditorProps) {
  const [imageDialog, setImageDialog] = React.useState<ImageDialogState>({
    open: false,
    url: '',
    alt: '',
    uploading: false,
    tabValue: 0,
  })
  
  const [pdfDialog, setPdfDialog] = React.useState<PdfDialogState>({
    open: false,
    url: '',
    text: '',
    uploading: false,
    tabValue: 0,
  })

  const editorRef = React.useRef<any>(null)

  const handleImageUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    try {
      setImageDialog(prev => ({ ...prev, uploading: true }))
      
      const response = await uploadFile(file, 'Image')
      
      setImageDialog(prev => ({ 
        ...prev, 
        url: response.fileUrl,
        alt: file.name,
        uploading: false,
        tabValue: 0,
      }))
    } catch (error) {
      console.error('Failed to upload image:', error)
      setImageDialog(prev => ({ ...prev, uploading: false }))
    }
  }

  const handleImageInsert = () => {
    if (imageDialog.url) {
      const markdown = `![${imageDialog.alt || 'image'}](${imageDialog.url})`
      const currentValue = value || ''
      onChange(currentValue + '\n' + markdown)
      setImageDialog({ open: false, url: '', alt: '', uploading: false, tabValue: 0 })
    }
  }

  const handlePdfUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    try {
      setPdfDialog(prev => ({ ...prev, uploading: true }))
      
      const response = await uploadFile(file, 'Document')
      
      setPdfDialog(prev => ({ 
        ...prev, 
        url: response.fileUrl,
        text: file.name.replace('.pdf', ''),
        uploading: false,
        tabValue: 0,
      }))
    } catch (error) {
      console.error('Failed to upload PDF:', error)
      setPdfDialog(prev => ({ ...prev, uploading: false }))
    }
  }

  const handlePdfInsert = () => {
    if (pdfDialog.url) {
      const linkText = pdfDialog.text || 'Download PDF'
      const markdown = `[${linkText}](${pdfDialog.url})`
      const currentValue = value || ''
      onChange(currentValue + '\n' + markdown)
      setPdfDialog({ open: false, url: '', text: '', uploading: false, tabValue: 0 })
    }
  }

  const options = React.useMemo<Options>(() => {
    return {
      spellChecker: false,
      placeholder: placeholder || 'Nhập nội dung...',
      status: false,
      toolbar: [
        'bold',
        'italic',
        'strikethrough',
        '|',
        'heading',
        'heading-smaller',
        'heading-bigger',
        '|',
        'code',
        'quote',
        'unordered-list',
        'ordered-list',
        '|',
        {
          name: 'upload-image',
          action: () => {
            setImageDialog(prev => ({ ...prev, open: true }))
          },
          className: 'fa fa-image',
          title: 'Insert Image',
        },
        'link',
        {
          name: 'upload-pdf',
          action: () => {
            setPdfDialog(prev => ({ ...prev, open: true }))
          },
          className: 'fa fa-file-pdf',
          title: 'Insert PDF',
        },
        '|',
        'preview',
        'side-by-side',
        'fullscreen',
        '|',
        'guide',
      ],
      minHeight: minHeight || `${rows * 24}px`,
      maxHeight: maxHeight || '600px',
      autofocus: false,
      lineWrapping: true,
      indentWithTabs: false,
      tabSize: 2,
    }
  }, [placeholder, rows])

  return (
    <Box>
      {label && (
        <Typography variant="h6" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
          {label}
        </Typography>
      )}
      
      <Box
        sx={{
          '& .EasyMDEContainer': {
            border: '1px solid #d2d2d7',
            borderRadius: 1,
          },
          '& .EasyMDEContainer .CodeMirror': {
            border: 'none',
            borderBottomLeftRadius: 4,
            borderBottomRightRadius: 4,
            fontFamily: 'monospace',
            fontSize: '14px',
            lineHeight: '1.6',
          },
          '& .editor-toolbar': {
            borderTop: 'none',
            borderLeft: 'none',
            borderRight: 'none',
            borderBottom: '1px solid #d2d2d7',
            borderTopLeftRadius: 4,
            borderTopRightRadius: 4,
            bgcolor: '#f5f5f7',
          },
          '& .editor-toolbar button': {
            color: '#1d1d1f !important',
            border: 'none !important',
            '&:hover': {
              bgcolor: 'rgba(0, 0, 0, 0.04)',
              borderColor: 'transparent !important',
            },
            '&.active': {
              bgcolor: 'rgba(0, 0, 0, 0.08)',
            },
          },
          '& .editor-toolbar i.separator': {
            borderLeft: '1px solid #d2d2d7',
            borderRight: 'none',
          },
          '& .CodeMirror-cursor': {
            borderLeft: '1px solid #1d1d1f',
          },
          '& .editor-preview-side, & .editor-preview': {
            bgcolor: '#ffffff',
            border: 'none',
            padding: '16px',
          },
        }}
      >
        <SimpleMdeReact
          ref={editorRef}
          value={value}
          onChange={onChange}
          options={options}
        />
      </Box>

      {helperText && (
        <Typography 
          variant="caption" 
          sx={{ 
            px: 1.5, 
            pt: 0.5, 
            display: 'block',
            color: 'text.secondary' 
          }}
        >
          {helperText}
        </Typography>
      )}

      {/* Image Dialog */}
      <Dialog
        open={imageDialog.open}
        onClose={() => setImageDialog({ 
          open: false, 
          url: '', 
          alt: '', 
          uploading: false, 
          tabValue: 0 
        })}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Insert Image</DialogTitle>
        <DialogContent>
          <Tabs
            value={imageDialog.tabValue}
            onChange={(e, newValue) => setImageDialog(prev => ({ ...prev, tabValue: newValue }))}
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
                onChange={(e) => setImageDialog(prev => ({ ...prev, url: e.target.value }))}
                placeholder="https://example.com/image.png"
                autoFocus
              />
              <TextField
                fullWidth
                label="Alt Text (Optional)"
                value={imageDialog.alt}
                onChange={(e) => setImageDialog(prev => ({ ...prev, alt: e.target.value }))}
                placeholder="Image description"
              />
            </Stack>
          ) : (
            <Stack spacing={2} sx={{ mt: 1 }}>
              <Button
                variant="outlined"
                component="label"
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
                    onChange={(e) => setImageDialog(prev => ({ ...prev, alt: e.target.value }))}
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

      {/* PDF Dialog */}
      <Dialog
        open={pdfDialog.open}
        onClose={() => setPdfDialog({ 
          open: false, 
          url: '', 
          text: '',
          uploading: false,
          tabValue: 0
        })}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Insert PDF</DialogTitle>
        <DialogContent>
          <Tabs
            value={pdfDialog.tabValue}
            onChange={(e, newValue) => setPdfDialog(prev => ({ ...prev, tabValue: newValue }))}
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
                onChange={(e) => setPdfDialog(prev => ({ ...prev, url: e.target.value }))}
                placeholder="https://example.com/document.pdf"
                autoFocus
              />
              <TextField
                fullWidth
                label="Link Text (Optional)"
                value={pdfDialog.text}
                onChange={(e) => setPdfDialog(prev => ({ ...prev, text: e.target.value }))}
                placeholder="Download PDF"
              />
            </Stack>
          ) : (
            <Stack spacing={2} sx={{ mt: 1 }}>
              <Button
                variant="outlined"
                component="label"
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
                    onChange={(e) => setPdfDialog(prev => ({ ...prev, text: e.target.value }))}
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
    </Box>
  )
}
