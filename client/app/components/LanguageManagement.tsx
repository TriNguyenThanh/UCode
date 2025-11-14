import React from 'react'
import {
  Box,
  Typography,
  Button,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  InputAdornment,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Checkbox,
  CircularProgress,
  Chip,
  IconButton,
} from '@mui/material'
import CodeIcon from '@mui/icons-material/Code'
import EditIcon from '@mui/icons-material/Edit'
import SaveIcon from '@mui/icons-material/Save'
import type { ProblemLanguage, Language } from '~/types'
import {
  getAvailableLanguagesForProblem,
  addOrUpdateProblemLanguages,
  type ProblemLanguageRequest,
} from '~/services/problemService'

interface LanguageManagementProps {
  problemId: string
  allLanguages: Language[]
  initialProblemLanguages: ProblemLanguage[]
  onSnackbar: (message: string, severity: 'success' | 'error') => void
}

export function LanguageManagement({ 
  problemId, 
  allLanguages, 
  initialProblemLanguages, 
  onSnackbar 
}: LanguageManagementProps) {
  const [problemLanguages, setProblemLanguages] = React.useState<ProblemLanguage[]>(initialProblemLanguages)
  const [languageDialogOpen, setLanguageDialogOpen] = React.useState(false)
  const [selectedLanguages, setSelectedLanguages] = React.useState<Set<string>>(
    new Set(initialProblemLanguages.map(pl => pl.languageId))
  )
  const [editingLanguageId, setEditingLanguageId] = React.useState<string | null>(null)
  const [languageDetailDialogOpen, setLanguageDetailDialogOpen] = React.useState(false)
  const [editTimeFactor, setEditTimeFactor] = React.useState<number>(1.0)
  const [editMemoryKb, setEditMemoryKb] = React.useState<number>(65536)
  const [editHead, setEditHead] = React.useState<string>('')
  const [editBody, setEditBody] = React.useState<string>('')
  const [editTail, setEditTail] = React.useState<string>('')
  const [savingLanguages, setSavingLanguages] = React.useState(false)

  React.useEffect(() => {
    setProblemLanguages(initialProblemLanguages)
    setSelectedLanguages(new Set(initialProblemLanguages.map(pl => pl.languageId)))
  }, [initialProblemLanguages])

  const handleOpenLanguageDialog = () => {
    setLanguageDialogOpen(true)
  }
  
  const handleCloseLanguageDialog = () => {
    setLanguageDialogOpen(false)
  }
  
  const handleToggleLanguage = (languageId: string) => {
    const newSelected = new Set(selectedLanguages)
    if (newSelected.has(languageId)) {
      newSelected.delete(languageId)
    } else {
      newSelected.add(languageId)
    }
    setSelectedLanguages(newSelected)
  }
  
  const handleOpenLanguageDetailDialog = (languageId: string) => {
    const lang = problemLanguages.find(pl => pl.languageId === languageId)
    const defaultLang = allLanguages.find(l => l.languageId === languageId)
    
    if (!lang && !defaultLang) return
    
    setEditingLanguageId(languageId)
    setEditTimeFactor(lang?.timeFactor || defaultLang?.defaultTimeFactor || 1.0)
    setEditMemoryKb(lang?.memoryKb || defaultLang?.defaultMemoryKb || 65536)
    setEditHead(lang?.head || defaultLang?.defaultHead || '')
    setEditBody(lang?.body || defaultLang?.defaultBody || '')
    setEditTail(lang?.tail || defaultLang?.defaultTail || '')
    setLanguageDetailDialogOpen(true)
  }
  
  const handleCloseLanguageDetailDialog = () => {
    setLanguageDetailDialogOpen(false)
    setEditingLanguageId(null)
    setEditTimeFactor(1.0)
    setEditMemoryKb(65536)
    setEditHead('')
    setEditBody('')
    setEditTail('')
  }
  
  const handleSaveLanguageDetail = async () => {
    if (!editingLanguageId) return
    
    setSavingLanguages(true)
    try {
      const updatedProblemLanguages = problemLanguages.map(pl => {
        if (pl.languageId === editingLanguageId) {
          return {
            ...pl,
            timeFactor: editTimeFactor,
            memoryKb: editMemoryKb,
            head: editHead,
            body: editBody,
            tail: editTail,
          }
        }
        return pl
      })
      
      const languageRequests: ProblemLanguageRequest[] = updatedProblemLanguages.map(pl => ({
        problemId: problemId,
        languageId: pl.languageId,
        isAllowed: true,
        timeFactor: pl.timeFactor,
        memoryKb: pl.memoryKb,
        head: pl.head,
        body: pl.body,
        tail: pl.tail,
      }))
      
      await addOrUpdateProblemLanguages(problemId, languageRequests)
      
      const updated = await getAvailableLanguagesForProblem(problemId)
      setProblemLanguages(updated)
      
      onSnackbar('Cập nhật cấu hình ngôn ngữ thành công!', 'success')
      handleCloseLanguageDetailDialog()
    } catch (error: any) {
      onSnackbar(error.message || 'Không thể cập nhật cấu hình ngôn ngữ', 'error')
    } finally {
      setSavingLanguages(false)
    }
  }
  
  const handleSaveLanguages = async () => {
    setSavingLanguages(true)
    try {
      const languageRequests: ProblemLanguageRequest[] = Array.from(selectedLanguages).map(languageId => {
        const lang = allLanguages.find(l => l.languageId === languageId)
        const existing = problemLanguages.find(pl => pl.languageId === languageId)
        
        if (!lang) {
          throw new Error(`Language with ID ${languageId} not found`)
        }
        
        return {
          problemId: problemId,
          languageId: lang.languageId,
          isAllowed: true,
          timeFactor: existing?.timeFactor || lang.defaultTimeFactor,
          memoryKb: existing?.memoryKb || lang.defaultMemoryKb,
          head: existing?.head || lang.defaultHead,
          body: existing?.body || lang.defaultBody,
          tail: existing?.tail || lang.defaultTail,
        }
      })
      
      await addOrUpdateProblemLanguages(problemId, languageRequests)
      
      const updated = await getAvailableLanguagesForProblem(problemId)
      setProblemLanguages(updated)
      
      onSnackbar('Cập nhật ngôn ngữ thành công!', 'success')
      handleCloseLanguageDialog()
    } catch (error: any) {
      onSnackbar(error.message || 'Không thể cập nhật ngôn ngữ', 'error')
    } finally {
      setSavingLanguages(false)
    }
  }

  return (
    <>
      <Box sx={{ px: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, color: '#1d1d1f' }}>
            Ngôn ngữ lập trình được phép
          </Typography>
          <Button
            variant="contained"
            startIcon={<CodeIcon />}
            onClick={handleOpenLanguageDialog}
            sx={{
              bgcolor: '#FACB01',
              '&:hover': { bgcolor: '#0077ed' },
            }}
          >
            Quản lý ngôn ngữ
          </Button>
        </Box>

        {problemLanguages.length === 0 ? (
          <Alert severity="warning">
            Chưa có ngôn ngữ nào được kích hoạt. Nhấn "Quản lý ngôn ngữ" để thêm.
          </Alert>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell sx={{ fontWeight: 600 }}>Ngôn ngữ</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Code</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Time Factor</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Memory (KB)</TableCell>
                  <TableCell sx={{ fontWeight: 600 }} align="right">Thao tác</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {problemLanguages.map((pl) => (
                  <TableRow key={pl.languageId} hover>
                    <TableCell>{pl.languageDisplayName}</TableCell>
                    <TableCell>
                      <Chip label={pl.languageCode} size="small" />
                    </TableCell>
                    <TableCell>{pl.timeFactor?.toFixed(2) || '1.00'}x</TableCell>
                    <TableCell>{pl.memoryKb?.toLocaleString() || 'Default'}</TableCell>
                    <TableCell align="right">
                      <IconButton
                        size="small"
                        color="primary"
                        onClick={() => handleOpenLanguageDetailDialog(pl.languageId)}
                      >
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Box>

      {/* Language Selection Dialog */}
      <Dialog 
        open={languageDialogOpen} 
        onClose={handleCloseLanguageDialog}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            Quản lý ngôn ngữ lập trình
          </Typography>
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Chọn các ngôn ngữ được phép sử dụng cho bài toán này
          </Typography>
          
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell padding="checkbox">
                    <Checkbox
                      checked={selectedLanguages.size === allLanguages.length}
                      indeterminate={selectedLanguages.size > 0 && selectedLanguages.size < allLanguages.length}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setSelectedLanguages(new Set(allLanguages.map(l => l.languageId)))
                        } else {
                          setSelectedLanguages(new Set())
                        }
                      }}
                    />
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Ngôn ngữ</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Code</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Time Factor</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {allLanguages.map((lang) => (
                  <TableRow key={lang.languageId} hover>
                    <TableCell padding="checkbox">
                      <Checkbox
                        checked={selectedLanguages.has(lang.languageId)}
                        onChange={() => handleToggleLanguage(lang.languageId)}
                      />
                    </TableCell>
                    <TableCell>{lang.displayName}</TableCell>
                    <TableCell>
                      <Chip label={lang.code} size="small" />
                    </TableCell>
                    <TableCell>{lang.defaultTimeFactor}x</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseLanguageDialog}>Hủy</Button>
          <Button 
            variant="contained" 
            onClick={handleSaveLanguages}
            disabled={savingLanguages}
            startIcon={savingLanguages ? <CircularProgress size={20} /> : <SaveIcon />}
          >
            {savingLanguages ? 'Đang lưu...' : 'Lưu'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Language Detail Edit Dialog */}
      <Dialog 
        open={languageDetailDialogOpen} 
        onClose={handleCloseLanguageDetailDialog}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            Cấu hình chi tiết ngôn ngữ
          </Typography>
          {editingLanguageId && (
            <Typography variant="body2" color="text.secondary">
              {allLanguages.find(l => l.languageId === editingLanguageId)?.displayName}
            </Typography>
          )}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Typography variant="subtitle1" sx={{ mb: 2, fontWeight: 600 }}>
              Giới hạn tài nguyên
            </Typography>
            
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2, mb: 3 }}>
              <TextField
                fullWidth
                type="number"
                label="Time Factor"
                value={editTimeFactor}
                onChange={(e) => setEditTimeFactor(Number(e.target.value))}
                InputProps={{
                  inputProps: { min: 0.1, max: 10, step: 0.1 },
                  endAdornment: <InputAdornment position="end">x</InputAdornment>,
                }}
                helperText="Hệ số nhân với time limit của bài toán"
              />
              
              <TextField
                fullWidth
                type="number"
                label="Memory Limit"
                value={editMemoryKb}
                onChange={(e) => setEditMemoryKb(Number(e.target.value))}
                InputProps={{
                  inputProps: { min: 32768, max: 524288, step: 32768 },
                  endAdornment: <InputAdornment position="end">KB</InputAdornment>,
                }}
                helperText="Giới hạn bộ nhớ cho ngôn ngữ này"
              />
            </Box>

            <Divider sx={{ my: 3 }} />

            <Typography variant="subtitle1" sx={{ mb: 2, fontWeight: 600 }}>
              Code Template
            </Typography>
            
            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
              Head (Code đầu chương trình)
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={4}
              value={editHead}
              onChange={(e) => setEditHead(e.target.value)}
              placeholder="// Imports, includes..."
              sx={{
                mb: 2,
                '& .MuiInputBase-input': {
                  fontFamily: 'monospace',
                  fontSize: '0.875rem',
                },
              }}
            />

            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
              Body (Code chính - vị trí người dùng viết)
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={6}
              value={editBody}
              onChange={(e) => setEditBody(e.target.value)}
              placeholder="// Main code here..."
              sx={{
                mb: 2,
                '& .MuiInputBase-input': {
                  fontFamily: 'monospace',
                  fontSize: '0.875rem',
                },
              }}
            />

            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
              Tail (Code cuối chương trình)
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={4}
              value={editTail}
              onChange={(e) => setEditTail(e.target.value)}
              placeholder="// Cleanup, closing..."
              sx={{
                '& .MuiInputBase-input': {
                  fontFamily: 'monospace',
                  fontSize: '0.875rem',
                },
              }}
            />

            <Alert severity="info" sx={{ mt: 2 }}>
              Code template sẽ được sử dụng để tạo khung code mặc định cho người dùng. 
              Body là phần người dùng sẽ viết code.
            </Alert>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseLanguageDetailDialog}>Hủy</Button>
          <Button 
            variant="contained" 
            onClick={handleSaveLanguageDetail}
            disabled={savingLanguages}
            startIcon={savingLanguages ? <CircularProgress size={20} /> : <SaveIcon />}
            sx={{
              bgcolor: '#FACB01',
              '&:hover': { bgcolor: '#0077ed' },
            }}
          >
            {savingLanguages ? 'Đang lưu...' : 'Lưu cấu hình'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
