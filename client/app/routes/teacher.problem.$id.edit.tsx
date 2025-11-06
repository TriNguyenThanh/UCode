import React from 'react'
import { redirect, useNavigate, useLoaderData, useRevalidator } from 'react-router'
import type { Route } from './+types/teacher.problem.$id.edit'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import { MarkdownEditor } from '~/components/MarkdownEditor'
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  Chip,
  IconButton,
  InputAdornment,
  Tabs,
  Tab,
  Alert,
  Divider,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Snackbar,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Checkbox,
  CircularProgress,
  Card,
  CardContent,
  CardActions,
  Stack,
  Switch,
  FormControlLabel,
} from '@mui/material'
import SaveIcon from '@mui/icons-material/Save'
import CancelIcon from '@mui/icons-material/Cancel'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import CodeIcon from '@mui/icons-material/Code'
import PlaylistAddCheckIcon from '@mui/icons-material/PlaylistAddCheck'
import DragIndicatorIcon from '@mui/icons-material/DragIndicator'
import {
  getProblem,
  updateProblem,
  getAvailableLanguagesForProblem,
  addOrUpdateProblemLanguages,
  deleteProblemLanguage,
  addTagsToProblem,
  removeTagFromProblem,
  type UpdateProblemRequest,
  type ProblemLanguageRequest,
} from '~/services/problemService'
import {
  getAllLanguages,
  type Language,
} from '~/services/languageService'
import {
  getDatasetsByProblem,
  createDataset,
  updateDataset,
  deleteDataset,
  type CreateDatasetRequest,
  type UpdateDatasetRequest,
} from '~/services/datasetService'
import {
  getAllTags,
} from '~/services/tagService'
import type {
  Problem,
  ProblemLanguage,
  Dataset,
  Difficulty,
  Visibility,
  ProblemStatus,
  IoMode,
  DatasetKind,
  Tag,
} from '~/types'


interface TabPanelProps {
  children?: React.ReactNode
  index: number
  value: number
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <div role="tabpanel" hidden={value !== index}>
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  )
}

export const meta: Route.MetaFunction = () => [
  { title: 'Chỉnh sửa bài toán | UCode' },
  { name: 'description', content: 'Chỉnh sửa bài toán lập trình.' },
]

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'teacher' && user.role !== 'admin') throw redirect('/home')

  const problemId = params.id
  if (!problemId) {
    throw new Response('Problem ID is required', { status: 400 })
  }

  try {
    const [problem, allLanguages, problemLanguages, datasets] = await Promise.all([
      getProblem(problemId),
      getAllLanguages(false),
      getAvailableLanguagesForProblem(problemId),
      getDatasetsByProblem(problemId),
    ])

    return { user, problem, allLanguages, problemLanguages, datasets }
  } catch (error: any) {
    console.error('Failed to load problem:', error)
    throw new Response(error.message || 'Problem not found', { status: 404 })
  }
}

export default function EditProblem() {
  const { problem: initialProblem, allLanguages, problemLanguages: initialProblemLanguages, datasets: initialDatasets } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const revalidator = useRevalidator()
  
  const [tabValue, setTabValue] = React.useState(0)
  
  // Problem form state
  const [title, setTitle] = React.useState(initialProblem.title)
  const [code, setCode] = React.useState(initialProblem.code || '')
  const [difficulty, setDifficulty] = React.useState<Difficulty>(initialProblem.difficulty)
  const [visibility, setVisibility] = React.useState<Visibility>(initialProblem.visibility)
  const [status, setStatus] = React.useState<ProblemStatus>(initialProblem.status)
  const [statement, setStatement] = React.useState(initialProblem.statement || '')
  const [inputFormat, setInputFormat] = React.useState(initialProblem.inputFormat || '')
  const [outputFormat, setOutputFormat] = React.useState(initialProblem.outputFormat || '')
  const [constraints, setConstraints] = React.useState(initialProblem.constraints || '')
  const [solution, setSolution] = React.useState(initialProblem.solution || '')
  const [timeLimitMs, setTimeLimitMs] = React.useState(initialProblem.timeLimitMs)
  const [memoryLimitKb, setMemoryLimitKb] = React.useState(initialProblem.memoryLimitKb)
  const [ioMode, setIoMode] = React.useState<IoMode>(initialProblem.ioMode)
  
  // Language management
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
  
  // Dataset management
  const [datasets, setDatasets] = React.useState<Dataset[]>(initialDatasets)
  const [datasetDialogOpen, setDatasetDialogOpen] = React.useState(false)
  const [editingDataset, setEditingDataset] = React.useState<Dataset | null>(null)
  const [datasetName, setDatasetName] = React.useState('')
  const [datasetKind, setDatasetKind] = React.useState<DatasetKind>('SAMPLE')
  const [testCases, setTestCases] = React.useState<Array<{ inputRef: string; outputRef: string; indexNo: number; score?: number }>>([])
  const [draggedIndex, setDraggedIndex] = React.useState<number | null>(null)
  
  // Test case edit dialog
  const [testCaseDialogOpen, setTestCaseDialogOpen] = React.useState(false)
  const [editingTestCaseIndex, setEditingTestCaseIndex] = React.useState<number | null>(null)
  const [testCaseInput, setTestCaseInput] = React.useState('')
  const [testCaseOutput, setTestCaseOutput] = React.useState('')
  
  // UI state
  const [loading, setLoading] = React.useState(false)
  const [savingLanguages, setSavingLanguages] = React.useState(false)
  const [savingDataset, setSavingDataset] = React.useState(false)
  const [snackbar, setSnackbar] = React.useState<{
    open: boolean
    message: string
    severity: 'success' | 'error'
  }>({
    open: false,
    message: '',
    severity: 'success',
  })

  // Tag management
  const [tagDialogOpen, setTagDialogOpen] = React.useState(false)
  const [availableTags, setAvailableTags] = React.useState<Tag[]>([])
  const [currentTags, setCurrentTags] = React.useState<string[]>(initialProblem.tagNames)
  const [selectedTagIds, setSelectedTagIds] = React.useState<string[]>([]) // Temporary selection in dialog
  const [loadingTags, setLoadingTags] = React.useState(false)
  const [savingTags, setSavingTags] = React.useState(false)

  // Load available tags when tag dialog opens
  React.useEffect(() => {
    if (tagDialogOpen) {
      setLoadingTags(true)
      getAllTags()
        .then(tags => {
          setAvailableTags(tags)
          // Initialize selected tags based on current problem tags
          const selectedIds = tags
            .filter(tag => currentTags.includes(tag.name))
            .map(tag => tag.tagId)
          setSelectedTagIds(selectedIds)
        })
        .catch(error => {
          console.error('Failed to load tags:', error)
          setSnackbar({
            open: true,
            message: 'Không thể tải danh sách tags',
            severity: 'error',
          })
        })
        .finally(() => {
          setLoadingTags(false)
        })
    }
  }, [tagDialogOpen, currentTags])
  
  const handleUpdateProblem = async () => {
    if (!title.trim()) {
      setSnackbar({
        open: true,
        message: 'Vui lòng nhập tên bài toán',
        severity: 'error',
      })
      return
    }

    setLoading(true)
    try {
      const updateData: UpdateProblemRequest = {
        problemId: initialProblem.problemId,
        title: title.trim(),
        code: code.trim() || undefined,
        difficulty,
        visibility,
        status,
        statement: statement || undefined,
        inputFormat: inputFormat || undefined,
        outputFormat: outputFormat || undefined,
        constraints: constraints || undefined,
        solution: solution || undefined,
        timeLimitMs,
        memoryLimitKb,
        ioMode,
      }
      
      await updateProblem(updateData)
      
      setSnackbar({
        open: true,
        message: 'Cập nhật bài toán thành công!',
        severity: 'success',
      })
      
      revalidator.revalidate()
    } catch (error: any) {
      setSnackbar({
        open: true,
        message: error.message || 'Không thể cập nhật bài toán',
        severity: 'error',
      })
    } finally {
      setLoading(false)
    }
  }
  
  // Language Management Functions
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
      // Cập nhật thông tin ngôn ngữ vừa chỉnh sửa trong problemLanguages
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
      
      // Gửi tất cả ngôn ngữ đang được chọn, bao gồm cả ngôn ngữ vừa sửa
      const languageRequests: ProblemLanguageRequest[] = updatedProblemLanguages.map(pl => ({
        problemId: initialProblem.problemId,
        languageId: pl.languageId,
        isAllowed: true,
        timeFactor: pl.timeFactor,
        memoryKb: pl.memoryKb,
        head: pl.head,
        body: pl.body,
        tail: pl.tail,
      }))
      
      await addOrUpdateProblemLanguages(initialProblem.problemId, languageRequests)
      
      // Refresh problem languages
      const updated = await getAvailableLanguagesForProblem(initialProblem.problemId)
      setProblemLanguages(updated)
      
      setSnackbar({
        open: true,
        message: 'Cập nhật cấu hình ngôn ngữ thành công!',
        severity: 'success',
      })
      
      handleCloseLanguageDetailDialog()
    } catch (error: any) {
      setSnackbar({
        open: true,
        message: error.message || 'Không thể cập nhật cấu hình ngôn ngữ',
        severity: 'error',
      })
    } finally {
      setSavingLanguages(false)
    }
  }
  
  const handleSaveLanguages = async () => {
    setSavingLanguages(true)
    try {
      // Chỉ gửi các languages được chọn
      const languageRequests: ProblemLanguageRequest[] = Array.from(selectedLanguages).map(languageId => {
        const lang = allLanguages.find(l => l.languageId === languageId)
        const existing = problemLanguages.find(pl => pl.languageId === languageId)
        
        if (!lang) {
          throw new Error(`Language with ID ${languageId} not found`)
        }
        
        return {
          problemId: initialProblem.problemId,
          languageId: lang.languageId,
          isAllowed: true, // Vì đã được chọn nên luôn là true
          timeFactor: existing?.timeFactor || lang.defaultTimeFactor,
          memoryKb: existing?.memoryKb || lang.defaultMemoryKb,
          head: existing?.head || lang.defaultHead,
          body: existing?.body || lang.defaultBody,
          tail: existing?.tail || lang.defaultTail,
        }
      })
      
      await addOrUpdateProblemLanguages(initialProblem.problemId, languageRequests)
      
      // Refresh problem languages
      const updated = await getAvailableLanguagesForProblem(initialProblem.problemId)
      setProblemLanguages(updated)
      
      setSnackbar({
        open: true,
        message: 'Cập nhật ngôn ngữ thành công!',
        severity: 'success',
      })
      
      handleCloseLanguageDialog()
    } catch (error: any) {
      setSnackbar({
        open: true,
        message: error.message || 'Không thể cập nhật ngôn ngữ',
        severity: 'error',
      })
    } finally {
      setSavingLanguages(false)
    }
  }
  
  // Tag Management Functions
  const handleToggleTag = (tagId: string) => {
    setSelectedTagIds(prev => {
      if (prev.includes(tagId)) {
        return prev.filter(id => id !== tagId)
      } else {
        return [...prev, tagId]
      }
    })
  }

  const handleSaveTags = async () => {
    setSavingTags(true)
    try {
      // Get current tag IDs
      const currentTagIds = availableTags
        .filter(tag => currentTags.includes(tag.name))
        .map(tag => tag.tagId)

      // Find tags to add (in selectedTagIds but not in currentTagIds)
      const tagsToAdd = selectedTagIds.filter(id => !currentTagIds.includes(id))

      // Find tags to remove (in currentTagIds but not in selectedTagIds)
      const tagsToRemove = currentTagIds.filter(id => !selectedTagIds.includes(id))

      // Remove tags first
      for (const tagId of tagsToRemove) {
        await removeTagFromProblem(initialProblem.problemId, tagId)
      }

      // Then add new tags (if any)
      if (tagsToAdd.length > 0) {
        await addTagsToProblem(initialProblem.problemId, tagsToAdd)
      }

      // Update current tags display
      const newTagNames = availableTags
        .filter(tag => selectedTagIds.includes(tag.tagId))
        .map(tag => tag.name)
      setCurrentTags(newTagNames)

      setSnackbar({
        open: true,
        message: 'Cập nhật tags thành công!',
        severity: 'success',
      })

      setTagDialogOpen(false)
    } catch (error: any) {
      setSnackbar({
        open: true,
        message: error.message || 'Không thể cập nhật tags',
        severity: 'error',
      })
    } finally {
      setSavingTags(false)
    }
  }

  const handleRemoveTagFromChip = async (tagName: string) => {
    try {
      const tag = availableTags.find(t => t.name === tagName)
      if (!tag) {
        // If availableTags is empty, load them first
        const tags = await getAllTags()
        setAvailableTags(tags)
        const foundTag = tags.find(t => t.name === tagName)
        if (foundTag) {
          await removeTagFromProblem(initialProblem.problemId, foundTag.tagId)
          setCurrentTags(prev => prev.filter(t => t !== tagName))
          setSnackbar({
            open: true,
            message: `Đã xóa tag "${tagName}"`,
            severity: 'success',
          })
        }
      } else {
        await removeTagFromProblem(initialProblem.problemId, tag.tagId)
        setCurrentTags(prev => prev.filter(t => t !== tagName))
        setSnackbar({
          open: true,
          message: `Đã xóa tag "${tagName}"`,
          severity: 'success',
        })
      }
    } catch (error: any) {
      setSnackbar({
        open: true,
        message: error.message || 'Không thể xóa tag',
        severity: 'error',
      })
    }
  }

  // Dataset Management Functions
  const handleOpenDatasetDialog = (dataset?: Dataset) => {
    if (dataset) {
      setEditingDataset(dataset)
      setDatasetName(dataset.name)
      setDatasetKind(dataset.kind)
      setTestCases(dataset.testCases.map((tc, idx) => ({
        inputRef: tc.inputRef,
        outputRef: tc.outputRef,
        indexNo: idx + 1,
        score: tc.score || 0,
      })))
    } else {
      setEditingDataset(null)
      setDatasetName('')
      setDatasetKind('SAMPLE')
      setTestCases([])
    }
    setDatasetDialogOpen(true)
  }
  
  const handleCloseDatasetDialog = () => {
    setDatasetDialogOpen(false)
    setEditingDataset(null)
    setDatasetName('')
    setDatasetKind('SAMPLE')
    setTestCases([])
  }
  
  const handleOpenTestCaseDialog = (index?: number) => {
    if (index !== undefined && index >= 0) {
      // Edit existing test case
      setEditingTestCaseIndex(index)
      setTestCaseInput(testCases[index].inputRef)
      setTestCaseOutput(testCases[index].outputRef)
    } else {
      // Add new test case
      setEditingTestCaseIndex(null)
      setTestCaseInput('')
      setTestCaseOutput('')
    }
    setTestCaseDialogOpen(true)
  }
  
  const handleCloseTestCaseDialog = () => {
    setTestCaseDialogOpen(false)
    setEditingTestCaseIndex(null)
    setTestCaseInput('')
    setTestCaseOutput('')
  }
  
  const handleSaveTestCase = () => {
    if (!testCaseInput.trim() || !testCaseOutput.trim()) {
      setSnackbar({
        open: true,
        message: 'Vui lòng điền đầy đủ input và output',
        severity: 'error',
      })
      return
    }
    
    if (editingTestCaseIndex !== null) {
      // Update existing test case
      const updated = [...testCases]
      updated[editingTestCaseIndex] = {
        inputRef: testCaseInput,
        outputRef: testCaseOutput,
        indexNo: editingTestCaseIndex + 1,
        score: updated[editingTestCaseIndex].score || 0,
      }
      setTestCases(updated)
    } else {
      // Add new test case
      setTestCases([
        ...testCases,
        {
          inputRef: testCaseInput,
          outputRef: testCaseOutput,
          indexNo: testCases.length + 1,
          score: 0,
        },
      ])
    }
    
    handleCloseTestCaseDialog()
  }
  
  const handleRemoveTestCase = (index: number) => {
    const updated = testCases.filter((_, i) => i !== index)
    // Update indexNo to start from 1
    setTestCases(updated.map((tc, idx) => ({ ...tc, indexNo: idx + 1 })))
  }
  
  const handleDragStart = (index: number) => {
    setDraggedIndex(index)
  }
  
  const handleDragOver = (e: React.DragEvent, index: number) => {
    e.preventDefault()
    if (draggedIndex === null || draggedIndex === index) return
    
    const updated = [...testCases]
    const draggedItem = updated[draggedIndex]
    updated.splice(draggedIndex, 1)
    updated.splice(index, 0, draggedItem)
    
    // Update indexNo
    const reindexed = updated.map((tc, idx) => ({ ...tc, indexNo: idx + 1 }))
    setTestCases(reindexed)
    setDraggedIndex(index)
  }
  
  const handleDragEnd = () => {
    setDraggedIndex(null)
  }
  
  const handleScoreChange = (index: number, score: string) => {
    const scoreValue = parseFloat(score) || 0
    const updated = [...testCases]
    updated[index] = { ...updated[index], score: scoreValue }
    setTestCases(updated)
  }
  
  const handleSaveDataset = async () => {
    if (!datasetName.trim()) {
      setSnackbar({
        open: true,
        message: 'Vui lòng nhập tên dataset',
        severity: 'error',
      })
      return
    }
    
    if (testCases.some(tc => !tc.inputRef.trim() || !tc.outputRef.trim())) {
      setSnackbar({
        open: true,
        message: 'Vui lòng điền đầy đủ input và output cho tất cả test cases',
        severity: 'error',
      })
      return
    }
    
    setSavingDataset(true)
    try {
      if (editingDataset) {
        // Update existing dataset
        const updateData: UpdateDatasetRequest = {
          datasetId: editingDataset.datasetId!,
          name: datasetName.trim(),
          kind: datasetKind,
          problemId: initialProblem.problemId,
          testCases: testCases.map((tc, idx) => ({
            inputRef: tc.inputRef,
            outputRef: tc.outputRef,
            orderIndex: idx + 1,
            score: tc.score || 0,
          })),
        }
        await updateDataset(updateData)
      } else {
        // Create new dataset
        const createData: CreateDatasetRequest = {
          problemId: initialProblem.problemId,
          name: datasetName.trim(),
          kind: datasetKind,
          testCases: testCases.map((tc, idx) => ({
            inputRef: tc.inputRef,
            outputRef: tc.outputRef,
            orderIndex: idx + 1,
            score: tc.score || 0,
          })),
        }
        await createDataset(createData)
      }
      
      // Refresh datasets
      const updatedDatasets = await getDatasetsByProblem(initialProblem.problemId)
      setDatasets(updatedDatasets)
      
      setSnackbar({
        open: true,
        message: editingDataset ? 'Cập nhật dataset thành công!' : 'Tạo dataset thành công!',
        severity: 'success',
      })
      
      handleCloseDatasetDialog()
    } catch (error: any) {
      setSnackbar({
        open: true,
        message: error.message || 'Không thể lưu dataset',
        severity: 'error',
      })
    } finally {
      setSavingDataset(false)
    }
  }
  
  const handleDeleteDataset = async (datasetId: string) => {
    if (!confirm('Bạn có chắc chắn muốn xóa dataset này?')) return
    
    try {
      await deleteDataset(datasetId)
      
      // Refresh datasets
      const updatedDatasets = await getDatasetsByProblem(initialProblem.problemId)
      setDatasets(updatedDatasets)
      
      setSnackbar({
        open: true,
        message: 'Xóa dataset thành công!',
        severity: 'success',
      })
    } catch (error: any) {
      setSnackbar({
        open: true,
        message: error.message || 'Không thể xóa dataset',
        severity: 'error',
      })
    }
  }
  
  const getDifficultyColor = (diff: Difficulty) => {
    switch (diff) {
      case 'EASY': return '#34C759'
      case 'MEDIUM': return '#FF9500'
      case 'HARD': return '#FF3B30'
      default: return '#86868b'
    }
  }
  
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
    <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
      <Navigation />
      
      <Container maxWidth="xl" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Box>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 1 }}>
                Chỉnh sửa bài toán
              </Typography>
              <Typography variant="body1" color="text.secondary">
                ID: {initialProblem.problemId} • Code: {initialProblem.code}
              </Typography>
            </Box>
            
            <Button
              variant="outlined"
              startIcon={<CancelIcon />}
              onClick={() => navigate('/teacher/problems')}
              sx={{
                borderColor: '#d2d2d7',
                color: '#1d1d1f',
              }}
            >
              Quay lại
            </Button>
          </Box>
          
          <Chip 
            label={difficulty}
            sx={{ 
              bgcolor: getDifficultyColor(difficulty),
              color: 'white',
              fontWeight: 'bold',
              mr: 1,
            }}
          />
          <Chip label={visibility} color={visibility === 'PUBLIC' ? 'success' : 'default'} sx={{ mr: 1 }} />
          <Chip label={status} color={status === 'PUBLISHED' ? 'primary' : 'default'} />
        </Box>

        <Paper
          elevation={0}
          sx={{
            mb: 3,
            bgcolor: '#ffffff',
            border: '1px solid #d2d2d7',
            borderRadius: 2,
          }}
        >
          {/* Tabs */}
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tabs
              value={tabValue}
              onChange={(_, newValue) => setTabValue(newValue)}
              sx={{
                px: 2,
                '& .MuiTab-root': { 
                  color: '#86868b', 
                  textTransform: 'none', 
                  fontSize: '1rem',
                  fontWeight: 500,
                },
                '& .Mui-selected': { 
                  color: '#1d1d1f',
                  fontWeight: 600,
                },
                '& .MuiTabs-indicator': { 
                  bgcolor: '#0071e3',
                  height: 3,
                },
              }}
            >
              <Tab label="Thông tin cơ bản" />
              <Tab label="Nội dung bài toán" />
              <Tab label="Giới hạn & Cấu hình" />
              <Tab label={`Ngôn ngữ (${problemLanguages.length})`} />
              <Tab label={`Test Cases (${datasets.length})`} />
              <Tab label={`Tags (${initialProblem.tagNames.length})`} />
            </Tabs>
          </Box>

          {/* Tab 1: Thông tin cơ bản */}
          <TabPanel value={tabValue} index={0}>
            <Box sx={{ px: 3 }}>
              <TextField
                fullWidth
                required
                label="Tên bài toán"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="VD: Two Sum, Binary Search, Merge Sort..."
                sx={{ mb: 3 }}
              />

              <TextField
                fullWidth
                label="Code (Mã bài toán)"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                placeholder="VD: two-sum, binary-search..."
                helperText="Code dùng để định danh bài toán, nếu để trống sẽ tự động tạo từ title"
                sx={{ mb: 3 }}
              />

              <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 2, mb: 3 }}>
                <FormControl fullWidth>
                  <InputLabel>Độ khó</InputLabel>
                  <Select
                    value={difficulty}
                    onChange={(e) => setDifficulty(e.target.value as Difficulty)}
                    label="Độ khó"
                  >
                    <MenuItem value="EASY">Easy</MenuItem>
                    <MenuItem value="MEDIUM">Medium</MenuItem>
                    <MenuItem value="HARD">Hard</MenuItem>
                  </Select>
                </FormControl>

                <FormControl fullWidth>
                  <InputLabel>Trạng thái</InputLabel>
                  <Select
                    value={status}
                    onChange={(e) => setStatus(e.target.value as ProblemStatus)}
                    label="Trạng thái"
                  >
                    <MenuItem value="DRAFT">Draft</MenuItem>
                    <MenuItem value="PUBLISHED">Published</MenuItem>
                    <MenuItem value="ARCHIVED">Archived</MenuItem>
                  </Select>
                </FormControl>

                <FormControl fullWidth>
                  <InputLabel>Hiển thị</InputLabel>
                  <Select
                    value={visibility}
                    onChange={(e) => setVisibility(e.target.value as Visibility)}
                    label="Hiển thị"
                  >
                    <MenuItem value="PRIVATE">Private</MenuItem>
                    <MenuItem value="PUBLIC">Public</MenuItem>
                  </Select>
                </FormControl>
              </Box>
            </Box>
          </TabPanel>

          {/* Tab 2: Nội dung bài toán */}
          <TabPanel value={tabValue} index={1}>
            <Box sx={{ px: 3 }}>
              <MarkdownEditor
                label="Đề bài"
                value={statement}
                onChange={setStatement}
                placeholder="Mô tả chi tiết về bài toán..."
                rows={10}
                helperText="Hỗ trợ Markdown. Sử dụng toolbar để định dạng văn bản, thêm hình ảnh, code, v.v."
              />

              <Divider sx={{ my: 4 }} />

              <MarkdownEditor
                label="Input Format"
                value={inputFormat}
                onChange={setInputFormat}
                placeholder="Mô tả định dạng input..."
                rows={4}
                helperText="Mô tả cách thức dữ liệu đầu vào được cung cấp"
              />

              <Divider sx={{ my: 4 }} />

              <MarkdownEditor
                label="Output Format"
                value={outputFormat}
                onChange={setOutputFormat}
                placeholder="Mô tả định dạng output..."
                rows={4}
                helperText="Mô tả cách thức dữ liệu đầu ra cần được trình bày"
              />

              <Divider sx={{ my: 4 }} />

              <MarkdownEditor
                label="Constraints (Ràng buộc)"
                value={constraints}
                onChange={setConstraints}
                placeholder="VD: 1 ≤ n ≤ 10^5..."
                rows={6}
                helperText="Các ràng buộc về giới hạn dữ liệu đầu vào"
              />

              <Divider sx={{ my: 4 }} />

              <MarkdownEditor
                label="Solution (Lời giải)"
                value={solution}
                onChange={setSolution}
                placeholder="Hướng dẫn giải chi tiết..."
                rows={8}
                helperText="Lời giải chi tiết, thuật toán, và cách tiếp cận để giải bài toán"
              />
            </Box>
          </TabPanel>

          {/* Tab 3: Giới hạn & Cấu hình */}
          <TabPanel value={tabValue} index={2}>
            <Box sx={{ px: 3 }}>
              <Typography variant="h6" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
                Giới hạn tài nguyên
              </Typography>
              
              <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2, mb: 3 }}>
                <TextField
                  fullWidth
                  type="number"
                  label="Time Limit (ms)"
                  value={timeLimitMs}
                  onChange={(e) => setTimeLimitMs(Number(e.target.value))}
                  InputProps={{
                    inputProps: { min: 100, max: 10000, step: 100 },
                    endAdornment: <InputAdornment position="end">ms</InputAdornment>,
                  }}
                />
                
                <TextField
                  fullWidth
                  type="number"
                  label="Memory Limit (KB)"
                  value={memoryLimitKb}
                  onChange={(e) => setMemoryLimitKb(Number(e.target.value))}
                  InputProps={{
                    inputProps: { min: 65536, max: 524288, step: 65536 },
                    endAdornment: <InputAdornment position="end">KB</InputAdornment>,
                  }}
                />
              </Box>

              <FormControl fullWidth sx={{ mb: 3 }}>
                <InputLabel>I/O Mode</InputLabel>
                <Select
                  value={ioMode}
                  onChange={(e) => setIoMode(e.target.value as IoMode)}
                  label="I/O Mode"
                >
                  <MenuItem value="STDIO">Standard I/O</MenuItem>
                  <MenuItem value="FILE">File I/O</MenuItem>
                </Select>
              </FormControl>

              <Alert severity="info">
                Time limit áp dụng cho mỗi test case. Memory limit áp dụng cho toàn bộ quá trình chạy.
              </Alert>
            </Box>
          </TabPanel>

          {/* Tab 4: Ngôn ngữ */}
          <TabPanel value={tabValue} index={3}>
            <Box sx={{ px: 3 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6" sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                  Ngôn ngữ lập trình được phép
                </Typography>
                <Button
                  variant="contained"
                  startIcon={<CodeIcon />}
                  onClick={() => handleOpenLanguageDialog()}
                  sx={{
                    bgcolor: '#0071e3',
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
          </TabPanel>

          {/* Tab 5: Test Cases */}
          <TabPanel value={tabValue} index={4}>
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
                    bgcolor: '#0071e3',
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
                          borderColor: '#0071e3',
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
          </TabPanel>

          {/* Tab 6: Tags */}
          <TabPanel value={tabValue} index={5}>
            <Box sx={{ px: 3 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6" sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                  Tags ({currentTags.length})
                </Typography>
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => setTagDialogOpen(true)}
                  sx={{
                    bgcolor: '#007AFF',
                    textTransform: 'none',
                    fontWeight: 600,
                    '&:hover': {
                      bgcolor: '#0051D5',
                    },
                  }}
                >
                  Quản lý Tags
                </Button>
              </Box>

              {currentTags.length === 0 ? (
                <Alert severity="info">
                  Chưa có tag nào được gán cho bài toán này. Nhấn "Quản lý Tags" để thêm tags.
                </Alert>
              ) : (
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                  {currentTags.map((tagName) => (
                    <Chip
                      key={tagName}
                      label={tagName}
                      onDelete={() => handleRemoveTagFromChip(tagName)}
                      sx={{
                        bgcolor: '#007AFF',
                        color: '#ffffff',
                        fontWeight: 600,
                        '& .MuiChip-deleteIcon': {
                          color: 'rgba(255, 255, 255, 0.7)',
                          '&:hover': {
                            color: '#ffffff',
                          },
                        },
                      }}
                    />
                  ))}
                </Box>
              )}
            </Box>
          </TabPanel>
        </Paper>

        {/* Actions */}
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
          <Button
            variant="contained"
            startIcon={loading ? <CircularProgress size={20} /> : <SaveIcon />}
            onClick={handleUpdateProblem}
            disabled={loading}
            sx={{
              bgcolor: '#0071e3',
              px: 4,
              '&:hover': { bgcolor: '#0077ed' },
            }}
          >
            {loading ? 'Đang lưu...' : 'Lưu thay đổi'}
          </Button>
        </Box>

        {/* Language Management Dialog */}
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
                </Typography>
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

              {testCases.length === 0 ? (
                <Alert severity="info">
                  Chưa có test case nào. Nhấn "Thêm Test Case" để bắt đầu.
                </Alert>
              ) : (
                <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 400 }}>
                  <Table stickyHeader size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell sx={{ fontWeight: 600, width: 50 }}></TableCell>
                        <TableCell sx={{ fontWeight: 600, width: 80 }}>#</TableCell>
                        <TableCell sx={{ fontWeight: 600 }}>Input Preview</TableCell>
                        <TableCell sx={{ fontWeight: 600 }}>Output Preview</TableCell>
                        <TableCell sx={{ fontWeight: 600, width: 100 }}>Điểm</TableCell>
                        <TableCell sx={{ fontWeight: 600, width: 120 }} align="right">
                          Thao tác
                        </TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {testCases.map((tc, index) => (
                        <TableRow 
                          key={index} 
                          hover
                          draggable
                          onDragStart={() => handleDragStart(index)}
                          onDragOver={(e) => handleDragOver(e, index)}
                          onDragEnd={handleDragEnd}
                          sx={{
                            cursor: 'move',
                            bgcolor: draggedIndex === index ? 'action.hover' : 'inherit',
                            '&:hover': {
                              bgcolor: 'action.hover',
                            },
                          }}
                        >
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
                            <Typography
                              variant="body2"
                              sx={{
                                fontFamily: 'monospace',
                                fontSize: '0.75rem',
                                whiteSpace: 'pre-wrap',
                                maxWidth: 250,
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                display: '-webkit-box',
                                WebkitLineClamp: 2,
                                WebkitBoxOrient: 'vertical',
                                color: '#1d1d1f',
                                bgcolor: '#f5f5f7',
                                p: 1,
                                borderRadius: 1,
                              }}
                            >
                              {tc.inputRef || '(Empty)'}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Typography
                              variant="body2"
                              sx={{
                                fontFamily: 'monospace',
                                fontSize: '0.75rem',
                                whiteSpace: 'pre-wrap',
                                maxWidth: 250,
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                display: '-webkit-box',
                                WebkitLineClamp: 2,
                                WebkitBoxOrient: 'vertical',
                                color: '#1d1d1f',
                                bgcolor: '#f5f5f7',
                                p: 1,
                                borderRadius: 1,
                              }}
                            >
                              {tc.outputRef || '(Empty)'}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <TextField
                              type="number"
                              size="small"
                              value={tc.score || 0}
                              onChange={(e) => handleScoreChange(index, e.target.value)}
                              inputProps={{
                                min: 0,
                                step: 0.5,
                                style: { textAlign: 'right' },
                              }}
                              sx={{
                                width: '80px',
                                '& input': {
                                  fontSize: '0.875rem',
                                  py: 0.5,
                                },
                              }}
                            />
                          </TableCell>
                          <TableCell align="right">
                            <IconButton
                              size="small"
                              color="primary"
                              onClick={() => handleOpenTestCaseDialog(index)}
                              sx={{ mr: 0.5 }}
                            >
                              <EditIcon fontSize="small" />
                            </IconButton>
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => handleRemoveTestCase(index)}
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </TableCell>
                        </TableRow>
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
                bgcolor: '#0071e3',
                '&:hover': { bgcolor: '#0077ed' },
              }}
            >
              {editingTestCaseIndex !== null ? 'Cập nhật' : 'Thêm'}
            </Button>
          </DialogActions>
        </Dialog>

        {/* Tag Management Dialog */}
        <Dialog 
          open={tagDialogOpen} 
          onClose={() => setTagDialogOpen(false)}
          maxWidth="md"
          fullWidth
        >
          <DialogTitle>
            <Typography variant="h6" sx={{ fontWeight: 600 }}>
              Quản lý Tags
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Chọn các tag phù hợp cho bài toán này
            </Typography>
          </DialogTitle>
          <DialogContent>
            <Box sx={{ pt: 2 }}>
              {loadingTags ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                  <CircularProgress />
                </Box>
              ) : (
                <>
                  <Typography variant="subtitle2" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
                    Tags đã chọn ({selectedTagIds.length})
                  </Typography>
                  <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 3, minHeight: 40 }}>
                    {selectedTagIds.length === 0 ? (
                      <Typography variant="body2" color="text.secondary">
                        Chưa có tag nào được chọn
                      </Typography>
                    ) : (
                      selectedTagIds.map((tagId) => {
                        const tag = availableTags.find(t => t.tagId === tagId)
                        return tag ? (
                          <Chip
                            key={tagId}
                            label={tag.name}
                            sx={{
                              bgcolor: '#34C759',
                              color: '#ffffff',
                              fontWeight: 600,
                            }}
                          />
                        ) : null
                      })
                    )}
                  </Box>

                  <Divider sx={{ my: 2 }} />

                  <Typography variant="subtitle2" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
                    Tất cả các tags ({availableTags.length})
                  </Typography>
                  <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', maxHeight: 400, overflowY: 'auto', p: 1 }}>
                    {availableTags.map((tag) => {
                      const isSelected = selectedTagIds.includes(tag.tagId)
                      return (
                        <Chip
                          key={tag.tagId}
                          label={tag.name}
                          onClick={() => handleToggleTag(tag.tagId)}
                          sx={{
                            bgcolor: isSelected ? '#007AFF' : '#f5f5f7',
                            color: isSelected ? '#ffffff' : '#1d1d1f',
                            fontWeight: isSelected ? 600 : 400,
                            border: isSelected ? 'none' : '1px solid #d2d2d7',
                            cursor: 'pointer',
                            '&:hover': {
                              bgcolor: isSelected ? '#0051D5' : '#e8e8ed',
                            },
                          }}
                        />
                      )
                    })}
                  </Box>
                </>
              )}
            </Box>
          </DialogContent>
          <DialogActions>
            <Button 
              onClick={() => setTagDialogOpen(false)}
              sx={{ textTransform: 'none', fontWeight: 600 }}
            >
              Hủy
            </Button>
            <Button 
              variant="contained"
              onClick={handleSaveTags}
              disabled={savingTags}
              startIcon={savingTags ? <CircularProgress size={20} /> : <SaveIcon />}
              sx={{
                bgcolor: '#007AFF',
                textTransform: 'none',
                fontWeight: 600,
                '&:hover': {
                  bgcolor: '#0051D5',
                },
              }}
            >
              {savingTags ? 'Đang lưu...' : 'Lưu'}
            </Button>
          </DialogActions>
        </Dialog>

        {/* Snackbar */}
        <Snackbar
          open={snackbar.open}
          autoHideDuration={6000}
          onClose={() => setSnackbar({ ...snackbar, open: false })}
          anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
        >
          <Alert 
            onClose={() => setSnackbar({ ...snackbar, open: false })} 
            severity={snackbar.severity}
            sx={{ width: '100%' }}
          >
            {snackbar.message}
          </Alert>
        </Snackbar>

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
                bgcolor: '#0071e3',
                '&:hover': { bgcolor: '#0077ed' },
              }}
            >
              {savingLanguages ? 'Đang lưu...' : 'Lưu cấu hình'}
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </Box>
  )
}

