import { useState } from 'react'
import { redirect, useLoaderData, useNavigate } from 'react-router'
import type { Route } from './+types/teacher.class.$classId.create-assignment'
import { auth } from '~/auth'
import { API } from '~/api'
import { Navigation } from '~/components/Navigation'
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  Checkbox,
  FormControlLabel,
  Alert,
  MenuItem,
  CircularProgress,
  Snackbar,
} from '@mui/material'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker'
import { vi } from 'date-fns/locale/vi'
import SaveIcon from '@mui/icons-material/Save'
import CancelIcon from '@mui/icons-material/Cancel'
import { createAssignment } from '~/services/assignmentService'
import type { ApiResponse, Class } from '~/types'

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  try {
    // Fetch class data from API
    const response = await API.get<ApiResponse<Class>>(`/api/v1/classes/${params.classId}`)
    const classData = response.data.data
    
    if (!classData) {
      throw new Response('Lớp học không tồn tại', { status: 404 })
    }

    return { user, classData }
  } catch (error) {
    console.error('Error loading class:', error)
    throw new Response('Không thể tải thông tin lớp học', { status: 500 })
  }
}

export default function CreateAssignment() {
  const { classData } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const [startDate, setStartDate] = useState<Date | null>(new Date())
  const [endDate, setEndDate] = useState<Date | null>(new Date(Date.now() + 7 * 24 * 60 * 60 * 1000))
  const [noEndDate, setNoEndDate] = useState(false)
  const [allowLateSubmission, setAllowLateSubmission] = useState(true)
  const [assignmentType, setAssignmentType] = useState<'HOMEWORK' | 'EXAM' | 'PRACTICE' | 'CONTEST'>('HOMEWORK')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    // Validate
    if (!title.trim()) {
      setError('Vui lòng nhập tên bài tập')
      return
    }
    
    if (!startDate) {
      setError('Vui lòng chọn thời gian bắt đầu')
      return
    }
    
    if (!noEndDate && !endDate) {
      setError('Vui lòng chọn thời gian kết thúc hoặc đánh dấu "Không có thời gian kết thúc"')
      return
    }
    
    if (!noEndDate && endDate && startDate && endDate <= startDate) {
      setError('Thời gian kết thúc phải sau thời gian bắt đầu')
      return
    }

    setLoading(true)
    setError(null)

    try {
      const newAssignment = await createAssignment({
        assignmentType,
        classId: classData.classId,
        title: title.trim(),
        description: description.trim() || undefined,
        startTime: startDate.toISOString(),
        endTime: noEndDate ? undefined : endDate?.toISOString(),
        allowLateSubmission,
        status: 'DRAFT',
        problems: [], // Will be added later
      })

      // Redirect to assignment detail page to add problems
      navigate(`/teacher/assignment/${newAssignment.assignmentId}`)
    } catch (err: any) {
      console.error('Error creating assignment:', err)
      setError(err.message || 'Có lỗi xảy ra khi tạo bài tập. Vui lòng thử lại.')
      setLoading(false)
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      
      <Container maxWidth="md" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 1 }}>
            Tạo bài tập mới
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Lớp: {classData.className} ({classData.classCode})
          </Typography>
        </Box>

        <Paper sx={{ p: 4 }}>
          <form onSubmit={handleSubmit}>
            {/* Assignment Type */}
            <TextField
              fullWidth
              select
              required
              label="Loại bài tập"
              value={assignmentType}
              onChange={(e) => setAssignmentType(e.target.value as any)}
              sx={{ mb: 3 }}
            >
              <MenuItem value="HOMEWORK">Bài tập về nhà</MenuItem>
              <MenuItem value="EXAM">Kiểm tra</MenuItem>
              <MenuItem value="PRACTICE">Luyện tập</MenuItem>
              <MenuItem value="CONTEST">Thi đấu</MenuItem>
            </TextField>

            {/* Title */}
            <TextField
              fullWidth
              required
              label="Tên bài tập"
              placeholder="VD: Bài tập tuần 3 - Mảng và Chuỗi"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              sx={{ mb: 3 }}
            />

            {/* Description */}
            <TextField
              fullWidth
              label="Mô tả"
              placeholder="Mô tả ngắn gọn về bài tập..."
              multiline
              rows={4}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              sx={{ mb: 3 }}
            />

            <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={vi}>
              {/* Start Date */}
              <DateTimePicker
                label="Thời gian bắt đầu *"
                value={startDate}
                onChange={(newValue: Date | null) => setStartDate(newValue)}
                format="dd/MM/yyyy HH:mm"
                ampm={false}
                slotProps={{
                  textField: {
                    fullWidth: true,
                    sx: { mb: 3 },
                  },
                }}
              />

              {/* No End Date Checkbox */}
              <FormControlLabel
                control={
                  <Checkbox
                    checked={noEndDate}
                    onChange={(e) => setNoEndDate(e.target.checked)}
                  />
                }
                label="Không có thời gian kết thúc"
                sx={{ mb: 2, display: 'block' }}
              />

              {/* End Date */}
              {!noEndDate && (
                <DateTimePicker
                  label="Thời gian kết thúc"
                  value={endDate}
                  onChange={(newValue: Date | null) => setEndDate(newValue)}
                  format="dd/MM/yyyy HH:mm"
                  ampm={false}
                  minDateTime={startDate || undefined}
                  slotProps={{
                    textField: {
                      fullWidth: true,
                      sx: { mb: 3 },
                    },
                  }}
                />
              )}
            </LocalizationProvider>

            {/* Allow Late Submission */}
            <FormControlLabel
              control={
                <Checkbox
                  checked={allowLateSubmission}
                  onChange={(e) => setAllowLateSubmission(e.target.checked)}
                />
              }
              label="Cho phép nộp bài muộn"
              sx={{ mb: 3, display: 'block' }}
            />

            {error && (
              <Alert severity="error" sx={{ mb: 3 }}>
                {error}
              </Alert>
            )}

            <Alert severity="info" sx={{ mb: 3 }}>
              Sau khi tạo bài tập, bạn sẽ được chuyển đến trang quản lý bài tập để thêm các bài toán.
            </Alert>

            {/* Actions */}
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button
                variant="outlined"
                startIcon={<CancelIcon />}
                onClick={() => navigate(`/teacher/class/${classData.classId}`)}
                disabled={loading}
                sx={{
                  borderColor: 'text.secondary',
                  color: 'text.secondary',
                }}
              >
                Hủy
              </Button>
              <Button
                type="submit"
                variant="contained"
                startIcon={loading ? <CircularProgress size={20} /> : <SaveIcon />}
                disabled={loading}
                sx={{
                  bgcolor: 'secondary.main',
                  color: 'primary.main',
                  '&:hover': {
                    bgcolor: 'primary.main',
                    color: 'secondary.main',
                  },
                }}
              >
                {loading ? 'Đang tạo...' : 'Tạo bài tập'}
              </Button>
            </Box>
          </form>
        </Paper>
      </Container>

      {/* Success/Error Snackbar */}
      <Snackbar
        open={!!error}
        autoHideDuration={6000}
        onClose={() => setError(null)}
        message={error}
      />
    </Box>
  )
}
