import React from 'react'
import { redirect, useNavigate, useLoaderData } from 'react-router'
import type { Route } from './+types/teacher.problem.$id.edit'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import { MarkdownEditor } from '~/components/MarkdownEditor'
import { DatasetManagement } from '~/components/DatasetManagement'
import { LanguageManagement } from '~/components/LanguageManagement'
import { TagManagement } from '~/components/TagManagement'
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  Chip,
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
  CircularProgress,
} from '@mui/material'
import SaveIcon from '@mui/icons-material/Save'
import CancelIcon from '@mui/icons-material/Cancel'
import {
  getProblem,
  updateProblem,
  getAvailableLanguagesForProblem,
  type UpdateProblemRequest,
} from '~/services/problemService'
import {
  getAllLanguages,
} from '~/services/languageService'
import {
  getDatasetsByProblem,
} from '~/services/datasetService'
import type {
  Difficulty,
  Visibility,
  ProblemStatus,
  IoMode,
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
  
  // UI state
  const [loading, setLoading] = React.useState(false)
  const [snackbar, setSnackbar] = React.useState<{
    open: boolean
    message: string
    severity: 'success' | 'error'
  }>({
    open: false,
    message: '',
    severity: 'success',
  })

  const handleSnackbar = (message: string, severity: 'success' | 'error') => {
    setSnackbar({ open: true, message, severity })
  }
  
  const handleUpdateProblem = async () => {
    if (!title.trim()) {
      handleSnackbar('Vui lòng nhập tên bài toán', 'error')
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
      handleSnackbar('Cập nhật bài toán thành công!', 'success')
    } catch (error: any) {
      handleSnackbar(error.message || 'Không thể cập nhật bài toán', 'error')
    } finally {
      setLoading(false)
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
                  bgcolor: '#FACB01',
                  height: 3,
                },
              }}
            >
              <Tab label="Thông tin cơ bản" />
              <Tab label="Nội dung bài toán" />
              <Tab label="Giới hạn & Cấu hình" />
              <Tab label={`Ngôn ngữ (${initialProblemLanguages.length})`} />
              <Tab label={`Test Cases (${initialDatasets.length})`} />
              <Tab label={`Tags (${initialProblem.tagNames.length})`} />
            </Tabs>
          </Box>

          {/* Tab 1: Thông tin cơ bản */}
          <TabPanel value={tabValue} index={0}>
            {tabValue === 0 && (
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
            )}
          </TabPanel>

          {/* Tab 2: Nội dung bài toán */}
          <TabPanel value={tabValue} index={1}>
            {tabValue === 1 && (
              <Box sx={{ px: 3 }}>
                <MarkdownEditor
                  label="Đề bài"
                  value={statement}
                  onChange={setStatement}
                  placeholder="Mô tả chi tiết về bài toán..."
                  minHeight="150px"
                  maxHeight="350px"
                  helperText="Hỗ trợ Markdown. Sử dụng toolbar để định dạng văn bản, thêm hình ảnh, code, v.v."
                />

                <Divider sx={{ my: 4 }} />

                <MarkdownEditor
                  label="Input Format"
                  value={inputFormat}
                  onChange={setInputFormat}
                  placeholder="Mô tả định dạng input..."
                  minHeight="50px"
                  maxHeight="50px"
                  helperText="Mô tả cách thức dữ liệu đầu vào được cung cấp"
                />

                <Divider sx={{ my: 4 }} />

                <MarkdownEditor
                  label="Output Format"
                  value={outputFormat}
                  onChange={setOutputFormat}
                  placeholder="Mô tả định dạng output..."
                  minHeight="50px"
                  maxHeight="50px"
                  helperText="Mô tả cách thức dữ liệu đầu ra cần được trình bày"
                />

                <Divider sx={{ my: 4 }} />

                <MarkdownEditor
                  label="Constraints (Ràng buộc)"
                  value={constraints}
                  onChange={setConstraints}
                  placeholder="VD: 1 ≤ n ≤ 10^5..."
                  minHeight="70px"
                  maxHeight="80px"
                  helperText="Các ràng buộc về giới hạn dữ liệu đầu vào"
                />

                <Divider sx={{ my: 4 }} />

                <MarkdownEditor
                  label="Solution (Lời giải)"
                  value={solution}
                  onChange={setSolution}
                  placeholder="Hướng dẫn giải chi tiết..."
                  minHeight="150px"
                  maxHeight="150px"
                  helperText="Lời giải chi tiết, thuật toán, và cách tiếp cận để giải bài toán"
                />
              </Box>
            )}
          </TabPanel>

          {/* Tab 3: Giới hạn & Cấu hình */}
          <TabPanel value={tabValue} index={2}>
            {tabValue === 2 && (
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
            )}
          </TabPanel>

          {/* Tab 4: Ngôn ngữ */}
          <TabPanel value={tabValue} index={3}>
            {tabValue === 3 && (
              <LanguageManagement
                problemId={initialProblem.problemId}
                allLanguages={allLanguages}
                initialProblemLanguages={initialProblemLanguages}
                onSnackbar={handleSnackbar}
              />
            )}
          </TabPanel>

          {/* Tab 5: Test Cases */}
          <TabPanel value={tabValue} index={4}>
            {tabValue === 4 && (
              <DatasetManagement
                problemId={initialProblem.problemId}
                initialDatasets={initialDatasets}
                onSnackbar={handleSnackbar}
              />
            )}
          </TabPanel>

          {/* Tab 6: Tags */}
          <TabPanel value={tabValue} index={5}>
            {tabValue === 5 && (
              <TagManagement
                problemId={initialProblem.problemId}
                initialTags={initialProblem.tagNames}
                onSnackbar={handleSnackbar}
              />
            )}
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
              bgcolor: '#FACB01',
              px: 4,
              '&:hover': { bgcolor: '#0077ed' },
            }}
          >
            {loading ? 'Đang lưu...' : 'Lưu thay đổi'}
          </Button>
        </Box>

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
      </Container>
    </Box>
  )
}
