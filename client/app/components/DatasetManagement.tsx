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
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  IconButton,
  CircularProgress,
  Card,
  CardContent,
  CardActions,
  Checkbox,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import SaveIcon from '@mui/icons-material/Save'
import PlaylistAddCheckIcon from '@mui/icons-material/PlaylistAddCheck'
import DragIndicatorIcon from '@mui/icons-material/DragIndicator'
import UploadFileIcon from '@mui/icons-material/UploadFile'
import DownloadIcon from '@mui/icons-material/Download'
import DeleteSweepIcon from '@mui/icons-material/DeleteSweep'
import type { Dataset, DatasetKind } from '~/types'
import {
  createDataset,
  updateDataset,
  deleteDataset,
  getDatasetsByProblem,
  type CreateDatasetRequest,
  type UpdateDatasetRequest,
} from '~/services/datasetService'
import { importExcelFile, downloadExcelTemplate, type ExcelColumn } from '~/utils/excelImport'

interface DatasetManagementProps {
  problemId: string
  initialDatasets: Dataset[]
  onSnackbar: (message: string, severity: 'success' | 'error') => void
}

interface TestCaseData {
  inputRef: string
  outputRef: string
  indexNo: number
}

// Memoized TestCaseRow component to prevent unnecessary re-renders
const TestCaseRow = React.memo(({
  tc,
  index,
  isSelected,
  isDragged,
  onToggleSelect,
  onDragStart,
  onDragOver,
  onDragEnd,
  onEdit,
  onDelete,
}: {
  tc: TestCaseData
  index: number
  isSelected: boolean
  isDragged: boolean
  onToggleSelect: (index: number) => void
  onDragStart: (index: number) => void
  onDragOver: (e: React.DragEvent, index: number) => void
  onDragEnd: () => void
  onEdit: (index: number) => void
  onDelete: (index: number) => void
}) => {
  const handleToggle = React.useCallback(() => {
    onToggleSelect(index)
  }, [index, onToggleSelect])

  const handleDragStart = React.useCallback(() => {
    onDragStart(index)
  }, [index, onDragStart])

  const handleDragOver = React.useCallback((e: React.DragEvent) => {
    onDragOver(e, index)
  }, [index, onDragOver])

  const handleEdit = React.useCallback(() => {
    onEdit(index)
  }, [index, onEdit])

  const handleDelete = React.useCallback(() => {
    onDelete(index)
  }, [index, onDelete])

  return (
    <TableRow 
      hover
      draggable
      onDragStart={handleDragStart}
      onDragOver={handleDragOver}
      onDragEnd={onDragEnd}
      sx={{
        cursor: 'move',
        bgcolor: isDragged ? 'action.hover' : 
                 isSelected ? 'rgba(0, 113, 227, 0.08)' : 'inherit',
        '&:hover': {
          bgcolor: isSelected ? 'rgba(0, 113, 227, 0.12)' : 'action.hover',
        },
      }}
    >
      <TableCell padding="checkbox">
        <Checkbox
          checked={isSelected}
          onChange={handleToggle}
        />
      </TableCell>
      <TableCell>
        <DragIndicatorIcon 
          sx={{ 
            color: 'text.secondary',
            cursor: 'grab',
            '&:active': { cursor: 'grabbing' },
          }} 
        />
      </TableCell>
      <TableCell>
        <Chip 
          label={`#${index + 1}`} 
          size="small" 
          color="primary"
          variant="outlined"
        />
      </TableCell>
      <TableCell>
        <Box
          component="pre"
          sx={{
            fontFamily: 'monospace',
            fontSize: '0.75rem',
            bgcolor: '#f5f5f7',
            p: 1,
            borderRadius: 1,
            maxHeight: 80,
            overflow: 'auto',
            whiteSpace: 'pre-wrap',
            wordBreak: 'break-word',
            m: 0,
          }}
        >
          {tc.inputRef.substring(0, 100)}
          {tc.inputRef.length > 100 && '...'}
        </Box>
      </TableCell>
      <TableCell>
        <Box
          component="pre"
          sx={{
            fontFamily: 'monospace',
            fontSize: '0.75rem',
            bgcolor: '#f5f5f7',
            p: 1,
            borderRadius: 1,
            maxHeight: 80,
            overflow: 'auto',
            whiteSpace: 'pre-wrap',
            wordBreak: 'break-word',
            m: 0,
          }}
        >
          {tc.outputRef.substring(0, 100)}
          {tc.outputRef.length > 100 && '...'}
        </Box>
      </TableCell>
      <TableCell align="right">
        <IconButton
          size="small"
          color="primary"
          onClick={handleEdit}
        >
          <EditIcon fontSize="small" />
        </IconButton>
        <IconButton
          size="small"
          color="error"
          onClick={handleDelete}
        >
          <DeleteIcon fontSize="small" />
        </IconButton>
      </TableCell>
    </TableRow>
  )
})

TestCaseRow.displayName = 'TestCaseRow'

export function DatasetManagement({ problemId, initialDatasets, onSnackbar }: DatasetManagementProps) {
  const [datasets, setDatasets] = React.useState<Dataset[]>(initialDatasets)
  const [datasetDialogOpen, setDatasetDialogOpen] = React.useState(false)
  const [editingDataset, setEditingDataset] = React.useState<Dataset | null>(null)
  const [datasetName, setDatasetName] = React.useState('')
  const [datasetKind, setDatasetKind] = React.useState<DatasetKind>('SAMPLE')
  const [testCases, setTestCases] = React.useState<TestCaseData[]>([])
  const [draggedIndex, setDraggedIndex] = React.useState<number | null>(null)
  
  // Test case edit dialog
  const [testCaseDialogOpen, setTestCaseDialogOpen] = React.useState(false)
  const [editingTestCaseIndex, setEditingTestCaseIndex] = React.useState<number | null>(null)
  const [testCaseInput, setTestCaseInput] = React.useState('')
  const [testCaseOutput, setTestCaseOutput] = React.useState('')
  
  const [savingDataset, setSavingDataset] = React.useState(false)
  const fileInputRef = React.useRef<HTMLInputElement>(null)
  const [selectedTestCases, setSelectedTestCases] = React.useState<Set<number>>(new Set())

  React.useEffect(() => {
    setDatasets(initialDatasets)
  }, [initialDatasets])

  const handleOpenDatasetDialog = React.useCallback((dataset?: Dataset) => {
    if (dataset) {
      setEditingDataset(dataset)
      setDatasetName(dataset.name)
      setDatasetKind(dataset.kind)
      setTestCases(dataset.testCases.map((tc, idx) => ({
        inputRef: tc.inputRef,
        outputRef: tc.outputRef,
        indexNo: idx + 1,
      })))
    } else {
      setEditingDataset(null)
      setDatasetName('')
      setDatasetKind('SAMPLE')
      setTestCases([])
    }
    setSelectedTestCases(new Set())
    setDatasetDialogOpen(true)
  }, [])
  
  const handleCloseDatasetDialog = React.useCallback(() => {
    setDatasetDialogOpen(false)
    setEditingDataset(null)
    setDatasetName('')
    setDatasetKind('SAMPLE')
    setTestCases([])
    setSelectedTestCases(new Set())
  }, [])
  
  const handleOpenTestCaseDialog = React.useCallback((index?: number) => {
    if (index !== undefined && index >= 0) {
      setEditingTestCaseIndex(index)
      setTestCaseInput(testCases[index].inputRef)
      setTestCaseOutput(testCases[index].outputRef)
    } else {
      setEditingTestCaseIndex(null)
      setTestCaseInput('')
      setTestCaseOutput('')
    }
    setTestCaseDialogOpen(true)
  }, [testCases])
  
  const handleCloseTestCaseDialog = React.useCallback(() => {
    setTestCaseDialogOpen(false)
    setEditingTestCaseIndex(null)
    setTestCaseInput('')
    setTestCaseOutput('')
  }, [])
  
  const handleSaveTestCase = React.useCallback(() => {
    if (!testCaseInput.trim() || !testCaseOutput.trim()) {
      onSnackbar('Vui lòng điền đầy đủ input và output', 'error')
      return
    }
    
    if (editingTestCaseIndex !== null) {
      setTestCases(prev => {
        const updated = [...prev]
        updated[editingTestCaseIndex] = {
          inputRef: testCaseInput,
          outputRef: testCaseOutput,
          indexNo: editingTestCaseIndex + 1,
        }
        return updated
      })
    } else {
      setTestCases(prev => [
        ...prev,
        {
          inputRef: testCaseInput,
          outputRef: testCaseOutput,
          indexNo: prev.length + 1,
        },
      ])
    }
    
    handleCloseTestCaseDialog()
  }, [testCaseInput, testCaseOutput, editingTestCaseIndex, onSnackbar, handleCloseTestCaseDialog])
  
  const handleRemoveTestCase = React.useCallback((index: number) => {
    setTestCases(prev => {
      const updated = prev.filter((_, i) => i !== index)
      return updated.map((tc, idx) => ({ ...tc, indexNo: idx + 1 }))
    })
    
    setSelectedTestCases(prev => {
      const newSelected = new Set<number>()
      prev.forEach(selectedIdx => {
        if (selectedIdx < index) {
          newSelected.add(selectedIdx)
        } else if (selectedIdx > index) {
          newSelected.add(selectedIdx - 1)
        }
      })
      return newSelected
    })
  }, [])
  
  const handleToggleSelectTestCase = React.useCallback((index: number) => {
    setSelectedTestCases(prev => {
      const newSelected = new Set(prev)
      if (newSelected.has(index)) {
        newSelected.delete(index)
      } else {
        newSelected.add(index)
      }
      return newSelected
    })
  }, [])
  
  const handleToggleSelectAll = React.useCallback(() => {
    setSelectedTestCases(prev => {
      if (prev.size === testCases.length) {
        return new Set()
      } else {
        return new Set(testCases.map((_, idx) => idx))
      }
    })
  }, [testCases.length])
  
  const handleDeleteSelectedTestCases = React.useCallback(() => {
    if (selectedTestCases.size === 0) return
    
    if (!confirm(`Bạn có chắc chắn muốn xóa ${selectedTestCases.size} test case(s) đã chọn?`)) return
    
    setTestCases(prev => {
      const updated = prev.filter((_, idx) => !selectedTestCases.has(idx))
      return updated.map((tc, idx) => ({ ...tc, indexNo: idx + 1 }))
    })
    setSelectedTestCases(new Set())
  }, [selectedTestCases])
  
  const handleDragStart = React.useCallback((index: number) => {
    setDraggedIndex(index)
  }, [])
  
  const handleDragOver = React.useCallback((e: React.DragEvent, index: number) => {
    e.preventDefault()
    if (draggedIndex === null || draggedIndex === index) return
    
    setTestCases(prev => {
      const updated = [...prev]
      const draggedItem = updated[draggedIndex]
      updated.splice(draggedIndex, 1)
      updated.splice(index, 0, draggedItem)
      return updated.map((tc, idx) => ({ ...tc, indexNo: idx + 1 }))
    })
    setDraggedIndex(index)
  }, [draggedIndex])
  
  const handleDragEnd = React.useCallback(() => {
    setDraggedIndex(null)
  }, [])
  
  const handleImportExcel = React.useCallback(async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    try {
      const columns: ExcelColumn[] = [
        { key: 'input', label: 'Input', required: true },
        { key: 'output', label: 'Output', required: true },
      ]

      const result = await importExcelFile<{ input: string; output: string }>(file, {
        columns,
        validateRow: (row, rowIndex) => {
          if (!row.input || row.input.toString().trim() === '') {
            return 'Input không được để trống'
          }
          if (!row.output || row.output.toString().trim() === '') {
            return 'Output không được để trống'
          }
          return null
        },
      })

      if (!result.success) {
        onSnackbar(result.errors.join(', '), 'error')
        return
      }

      if (result.warnings.length > 0) {
        console.warn('Import warnings:', result.warnings)
      }

      setTestCases(prev => {
        const newTestCases = result.data.map((row, idx) => ({
          inputRef: row.input.toString(),
          outputRef: row.output.toString(),
          indexNo: prev.length + idx + 1,
        }))
        return [...prev, ...newTestCases]
      })

      onSnackbar(`Đã nhập thành công ${result.data.length} test case(s)`, 'success')

      if (result.warnings.length > 0) {
        onSnackbar(`Cảnh báo: ${result.warnings.length} dòng bị bỏ qua`, 'error')
      }
    } catch (error: any) {
      onSnackbar(error.message || 'Lỗi khi nhập file Excel', 'error')
    } finally {
      if (fileInputRef.current) {
        fileInputRef.current.value = ''
      }
    }
  }, [onSnackbar])

  const handleDownloadTemplate = React.useCallback(() => {
    const columns: ExcelColumn[] = [
      { key: 'input', label: 'Input', required: true },
      { key: 'output', label: 'Output', required: true },
    ]
    downloadExcelTemplate(columns, 'testcase-template.xlsx')
  }, [])
  
  const handleSaveDataset = React.useCallback(async () => {
    if (!datasetName.trim()) {
      onSnackbar('Vui lòng nhập tên dataset', 'error')
      return
    }
    
    if (testCases.some(tc => !tc.inputRef.trim() || !tc.outputRef.trim())) {
      onSnackbar('Vui lòng điền đầy đủ input và output cho tất cả test cases', 'error')
      return
    }
    
    setSavingDataset(true)
    try {
      if (editingDataset) {
        const updateData: UpdateDatasetRequest = {
          datasetId: editingDataset.datasetId!,
          name: datasetName.trim(),
          kind: datasetKind,
          problemId: problemId,
          testCases: testCases.map((tc, idx) => ({
            inputRef: tc.inputRef,
            outputRef: tc.outputRef,
            indexNo: idx + 1,
          })),
        }
        await updateDataset(updateData)
      } else {
        const createData: CreateDatasetRequest = {
          problemId: problemId,
          name: datasetName.trim(),
          kind: datasetKind,
          testCases: testCases.map((tc, idx) => ({
            inputRef: tc.inputRef,
            outputRef: tc.outputRef,
            indexNo: idx + 1,
          })),
        }
        await createDataset(createData)
      }
      
      const updatedDatasets = await getDatasetsByProblem(problemId)
      setDatasets(updatedDatasets)
      
      onSnackbar(editingDataset ? 'Cập nhật dataset thành công!' : 'Tạo dataset thành công!', 'success')
      handleCloseDatasetDialog()
    } catch (error: any) {
      onSnackbar(error.message || 'Không thể lưu dataset', 'error')
    } finally {
      setSavingDataset(false)
    }
  }, [datasetName, datasetKind, testCases, editingDataset, problemId, onSnackbar, handleCloseDatasetDialog])
  
  const handleDeleteDataset = React.useCallback(async (datasetId: string) => {
    if (!confirm('Bạn có chắc chắn muốn xóa dataset này?')) return
    
    try {
      await deleteDataset(datasetId)
      const updatedDatasets = await getDatasetsByProblem(problemId)
      setDatasets(updatedDatasets)
      onSnackbar('Xóa dataset thành công!', 'success')
    } catch (error: any) {
      onSnackbar(error.message || 'Không thể xóa dataset', 'error')
    }
  }, [problemId, onSnackbar])
  
  const getDatasetKindLabel = (kind: DatasetKind) => {
    switch (kind) {
      case 'SAMPLE': return 'Test mẫu'
      case 'PUBLIC': return 'Test công khai'
      case 'PRIVATE': return 'Test riêng'
      case 'OFFICIAL': return 'Test chính thức'
      default: return kind
    }
  }
  
  const getDatasetKindColor = (kind: DatasetKind) => {
    switch (kind) {
      case 'SAMPLE': return 'success'
      case 'PUBLIC': return 'info'
      case 'PRIVATE': return 'warning'
      case 'OFFICIAL': return 'error'
      default: return 'default'
    }
  }

  return (
    <>
      <Box sx={{ px: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, color: '#1d1d1f' }}>
            Datasets & Test Cases
          </Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDatasetDialog()}
            sx={{
              bgcolor: '#FACB01',
              '&:hover': { bgcolor: '#0077ed' },
            }}
          >
            Thêm Dataset
          </Button>
        </Box>

        {datasets.length === 0 ? (
          <Alert severity="warning">
            Chưa có dataset nào. Nhấn "Thêm Dataset" để tạo test cases.
          </Alert>
        ) : (
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(2, 1fr)' }, gap: 2 }}>
            {datasets.map((dataset) => (
              <Card
                key={dataset.datasetId}
                variant="outlined"
                sx={{
                  borderColor: '#d2d2d7',
                  '&:hover': {
                    borderColor: '#FACB01',
                    boxShadow: '0 2px 8px rgba(0,113,227,0.1)',
                  },
                }}
              >
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', mb: 2 }}>
                    <Box>
                      <Typography variant="h6" sx={{ fontWeight: 600, mb: 0.5 }}>
                        {dataset.name}
                      </Typography>
                      <Chip 
                        label={getDatasetKindLabel(dataset.kind)} 
                        size="small"
                        color={getDatasetKindColor(dataset.kind) as any}
                      />
                    </Box>
                  </Box>
                  
                  <Typography variant="body2" color="text.secondary">
                    <PlaylistAddCheckIcon sx={{ fontSize: 16, verticalAlign: 'middle', mr: 0.5 }} />
                    {dataset.testCases.length} test case(s)
                  </Typography>
                </CardContent>
                <CardActions>
                  <Button 
                    size="small" 
                    startIcon={<EditIcon />}
                    onClick={() => handleOpenDatasetDialog(dataset)}
                  >
                    Sửa
                  </Button>
                  <Button 
                    size="small" 
                    color="error"
                    startIcon={<DeleteIcon />}
                    onClick={() => handleDeleteDataset(dataset.datasetId!)}
                  >
                    Xóa
                  </Button>
                </CardActions>
              </Card>
            ))}
          </Box>
        )}
      </Box>

      {/* Dataset Management Dialog */}
      <Dialog 
        open={datasetDialogOpen} 
        onClose={handleCloseDatasetDialog}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            {editingDataset ? 'Chỉnh sửa Dataset' : 'Thêm Dataset mới'}
          </Typography>
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Box sx={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 2, mb: 3 }}>
              <TextField
                fullWidth
                label="Tên Dataset"
                value={datasetName}
                onChange={(e) => setDatasetName(e.target.value)}
                placeholder="VD: Sample Test Cases, Edge Cases..."
                required
              />
              
              <FormControl fullWidth>
                <InputLabel>Loại Dataset</InputLabel>
                <Select
                  value={datasetKind}
                  onChange={(e) => setDatasetKind(e.target.value as DatasetKind)}
                  label="Loại Dataset"
                >
                  <MenuItem value="SAMPLE">SAMPLE - Test mẫu (hiển thị cho student)</MenuItem>
                  <MenuItem value="PUBLIC">PUBLIC - Test công khai</MenuItem>
                  <MenuItem value="PRIVATE">PRIVATE - Test riêng (không hiển thị)</MenuItem>
                  <MenuItem value="OFFICIAL">OFFICIAL - Test chính thức để chấm điểm</MenuItem>
                </Select>
              </FormControl>
            </Box>

            <Divider sx={{ my: 3 }} />

            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                Test Cases ({testCases.length})
                {selectedTestCases.size > 0 && (
                  <Chip 
                    label={`${selectedTestCases.size} đã chọn`} 
                    size="small" 
                    color="primary"
                    sx={{ ml: 2 }}
                  />
                )}
              </Typography>
              <Box sx={{ display: 'flex', gap: 1 }}>
                {selectedTestCases.size > 0 && (
                  <Button
                    variant="outlined"
                    color="error"
                    startIcon={<DeleteSweepIcon />}
                    onClick={handleDeleteSelectedTestCases}
                    size="small"
                    sx={{
                      borderColor: '#FF3B30',
                      color: '#FF3B30',
                      '&:hover': { 
                        borderColor: '#FF1F10',
                        bgcolor: 'rgba(255, 59, 48, 0.04)',
                      },
                    }}
                  >
                    Xóa đã chọn ({selectedTestCases.size})
                  </Button>
                )}
                <Button
                  variant="outlined"
                  startIcon={<DownloadIcon />}
                  onClick={handleDownloadTemplate}
                  size="small"
                  sx={{
                    borderColor: '#0071e3',
                    color: '#0071e3',
                    '&:hover': { 
                      borderColor: '#0077ed',
                      bgcolor: 'rgba(0, 113, 227, 0.04)',
                    },
                  }}
                >
                  Tải Template
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<UploadFileIcon />}
                  onClick={() => fileInputRef.current?.click()}
                  size="small"
                  sx={{
                    borderColor: '#34C759',
                    color: '#34C759',
                    '&:hover': { 
                      borderColor: '#2FB350',
                      bgcolor: 'rgba(52, 199, 89, 0.04)',
                    },
                  }}
                >
                  Nhập Excel
                </Button>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept=".xlsx,.xls,.csv"
                  style={{ display: 'none' }}
                  onChange={handleImportExcel}
                />
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => handleOpenTestCaseDialog()}
                  size="small"
                  sx={{
                    bgcolor: '#34C759',
                    '&:hover': { bgcolor: '#2FB350' },
                  }}
                >
                  Thêm Test Case
                </Button>
              </Box>
            </Box>

            {testCases.length === 0 ? (
              <Alert severity="info">
                Chưa có test case nào. Nhấn "Thêm Test Case" để bắt đầu.
              </Alert>
            ) : (
              <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 400 }}>
                <Table stickyHeader size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell padding="checkbox" sx={{ fontWeight: 600 }}>
                        <Checkbox
                          indeterminate={selectedTestCases.size > 0 && selectedTestCases.size < testCases.length}
                          checked={testCases.length > 0 && selectedTestCases.size === testCases.length}
                          onChange={handleToggleSelectAll}
                        />
                      </TableCell>
                      <TableCell sx={{ fontWeight: 600, width: 50 }}></TableCell>
                      <TableCell sx={{ fontWeight: 600, width: 80 }}>#</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Input Preview</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Output Preview</TableCell>
                      <TableCell sx={{ fontWeight: 600, width: 120 }} align="right">
                        Thao tác
                      </TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {testCases.map((tc, index) => (
                      <TestCaseRow
                        key={index}
                        tc={tc}
                        index={index}
                        isSelected={selectedTestCases.has(index)}
                        isDragged={draggedIndex === index}
                        onToggleSelect={handleToggleSelectTestCase}
                        onDragStart={handleDragStart}
                        onDragOver={handleDragOver}
                        onDragEnd={handleDragEnd}
                        onEdit={handleOpenTestCaseDialog}
                        onDelete={handleRemoveTestCase}
                      />
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDatasetDialog}>Hủy</Button>
          <Button 
            variant="contained" 
            onClick={handleSaveDataset}
            disabled={savingDataset}
            startIcon={savingDataset ? <CircularProgress size={20} /> : <SaveIcon />}
          >
            {savingDataset ? 'Đang lưu...' : 'Lưu Dataset'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Test Case Edit Dialog */}
      <Dialog 
        open={testCaseDialogOpen} 
        onClose={handleCloseTestCaseDialog}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            {editingTestCaseIndex !== null ? `Chỉnh sửa Test Case #${editingTestCaseIndex + 1}` : 'Thêm Test Case mới'}
          </Typography>
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600, color: '#1d1d1f' }}>
              Input
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={8}
              value={testCaseInput}
              onChange={(e) => setTestCaseInput(e.target.value)}
              placeholder="Nhập input cho test case..."
              required
              sx={{
                mb: 3,
                '& .MuiInputBase-input': {
                  fontFamily: 'monospace',
                  fontSize: '0.875rem',
                },
              }}
            />

            <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600, color: '#1d1d1f' }}>
              Expected Output
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={8}
              value={testCaseOutput}
              onChange={(e) => setTestCaseOutput(e.target.value)}
              placeholder="Nhập output mong đợi..."
              required
              sx={{
                '& .MuiInputBase-input': {
                  fontFamily: 'monospace',
                  fontSize: '0.875rem',
                },
              }}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseTestCaseDialog}>Hủy</Button>
          <Button 
            variant="contained" 
            onClick={handleSaveTestCase}
            startIcon={<SaveIcon />}
            sx={{
              bgcolor: '#FACB01',
              '&:hover': { bgcolor: '#0077ed' },
            }}
          >
            {editingTestCaseIndex !== null ? 'Cập nhật' : 'Thêm'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
