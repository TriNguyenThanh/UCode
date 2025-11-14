import React from 'react'
import { redirect, useNavigate } from 'react-router'
import type { Route } from './+types/teacher.problem.create'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Snackbar,
  Alert,
} from '@mui/material'
import SaveIcon from '@mui/icons-material/Save'
import CancelIcon from '@mui/icons-material/Cancel'
import { createProblem } from '~/services/problemService'
import type { Difficulty, Visibility } from '~/types'

export const meta: Route.MetaFunction = () => [
  { title: 'Tạo bài tập mới | UCode' },
  { name: 'description', content: 'Tạo bài tập lập trình mới.' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'teacher' && user.role !== 'admin') throw redirect('/home')
  return { user }
}

export default function CreateProblem() {
  const navigate = useNavigate()
  
  // Form fields - only basic information
  const [title, setTitle] = React.useState('')
  const [code, setCode] = React.useState('')
  const [difficulty, setDifficulty] = React.useState<Difficulty>('EASY')
  const [visibility, setVisibility] = React.useState<Visibility>('PRIVATE')
  
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
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
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
      const problem = await createProblem({
        title: title.trim(),
        code: code.trim() || undefined,
        difficulty,
        visibility,
      })
      
      setSnackbar({
        open: true,
        message: 'Tạo bài tập thành công!',
        severity: 'success',
      })
      
      // Navigate to edit page to continue adding details
      setTimeout(() => {
        navigate(`/teacher/problem/${problem.problemId}/edit`)
      }, 1000)
    } catch (error: any) {
      setSnackbar({
        open: true,
        message: error.message || 'Không thể tạo bài tập',
        severity: 'error',
      })
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
      <Navigation />

      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" sx={{ fontWeight: 700, color: '#1d1d1f', mb: 1 }}>
            Tạo bài toán mới
          </Typography>
          <Typography variant="body1" sx={{ color: '#86868b' }}>
            Tạo thông tin cơ bản, sau đó chuyển sang trang chỉnh sửa để thêm chi tiết
          </Typography>
        </Box>

        <form onSubmit={handleSubmit}>
          <Paper elevation={0} sx={{ p: 4, bgcolor: '#ffffff', border: '1px solid #d2d2d7', borderRadius: 2 }}>
            <TextField
              fullWidth
              required
              label="Tên bài toán"
              placeholder="VD: Two Sum, Binary Search, Merge Sort..."
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              disabled={loading}
              sx={{ mb: 3 }}
            />

            <TextField
              fullWidth
              label="Mã bài toán (tùy chọn)"
              placeholder="VD: P001"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              disabled={loading}
              helperText="Để trống để tự động tạo"
              sx={{ mb: 3 }}
            />

            <FormControl fullWidth sx={{ mb: 3 }}>
              <InputLabel>Độ khó</InputLabel>
              <Select
                value={difficulty}
                onChange={(e) => setDifficulty(e.target.value as Difficulty)}
                label="Độ khó"
                disabled={loading}
              >
                <MenuItem value="EASY">Dễ</MenuItem>
                <MenuItem value="MEDIUM">Trung bình</MenuItem>
                <MenuItem value="HARD">Khó</MenuItem>
              </Select>
            </FormControl>
            
            <FormControl fullWidth>
              <InputLabel>Hiển thị</InputLabel>
              <Select
                value={visibility}
                onChange={(e) => setVisibility(e.target.value as Visibility)}
                label="Hiển thị"
                disabled={loading}
              >
                <MenuItem value="PRIVATE">Riêng tư</MenuItem>
                <MenuItem value="PUBLIC">Công khai</MenuItem>
              </Select>
            </FormControl>
          </Paper>

          {/* Actions */}
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button
              variant="outlined"
              startIcon={<CancelIcon />}
              onClick={() => navigate('/teacher/problems')}
              disabled={loading}
              sx={{
                borderColor: '#d2d2d7',
                color: '#86868b',
                textTransform: 'none',
                fontWeight: 600,
              }}
            >
              Hủy
            </Button>

            <Button
              type="submit"
              variant="contained"
              startIcon={<SaveIcon />}
              disabled={loading}
              sx={{
                bgcolor: '#007AFF',
                color: '#ffffff',
                px: 4,
                textTransform: 'none',
                fontWeight: 600,
                '&:hover': {
                  bgcolor: '#0051D5',
                },
              }}
            >
              {loading ? 'Đang tạo...' : 'Tạo & Chỉnh sửa'}
            </Button>
          </Box>
        </form>
      </Container>
      
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
    </Box>
  )
}
