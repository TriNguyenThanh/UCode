import { useState } from 'react'
import { redirect, useLoaderData, useNavigate, useNavigation, useRevalidator } from 'react-router'
import type { Route } from './+types/teacher.assignment.$id.edit'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import { Loading } from '~/components/Loading'
import { getAssignment, updateAssignment } from '~/services/assignmentService'
import { getClassById } from '~/services/classService'
import type { Assignment, Class, AssignmentType, AssignmentStatus } from '~/types'
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
  Select,
  FormControl,
  InputLabel,
} from '@mui/material'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker'
import { vi } from 'date-fns/locale/vi'
import SaveIcon from '@mui/icons-material/Save'
import CancelIcon from '@mui/icons-material/Cancel'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  if (!params.id) {
    throw new Response('Assignment ID is required', { status: 400 })
  }

  try {
    // Fetch assignment and class data in parallel
    const assignment = await getAssignment(params.id)
    const classData = await getClassById(assignment.classId)

    return { user, assignment, classData }
  } catch (error: any) {
    console.error('Failed to load assignment:', error)
    throw new Response(error.message || 'Không thể tải thông tin bài tập', { status: 404 })
  }
}



export default function EditAssignment() {
  const { assignment, classData } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const navigation = useNavigation()
  const revalidator = useRevalidator()
  const isLoading = navigation.state === 'loading'
  
  const [title, setTitle] = useState(assignment.title)
  const [description, setDescription] = useState(assignment.description || '')
  const [assignmentType, setAssignmentType] = useState<AssignmentType>(assignment.assignmentType)
  const [status, setStatus] = useState<AssignmentStatus>(assignment.status)
  const [startTime, setStartTime] = useState<Date | null>(
    assignment.startTime ? new Date(assignment.startTime) : new Date()
  )
  const [endTime, setEndTime] = useState<Date | null>(
    assignment.endTime ? new Date(assignment.endTime) : null
  )
  const [noEndTime, setNoEndTime] = useState(!assignment.endTime)
  const [allowLateSubmission, setAllowLateSubmission] = useState(
    assignment.allowLateSubmission || false
  )
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setIsSubmitting(true)

    try {
      // Validate required fields
      if (!title || !assignmentType) {
        setError('Vui lòng điền đầy đủ thông tin bắt buộc')
        setIsSubmitting(false)
        return
      }

      // Preserve existing problems to avoid data loss
      const problems = assignment.problems?.map(p => ({
        problemId: p.problemId,
        points: p.points,
        orderIndex: p.orderIndex,
      })) || []

      // Update assignment via API
      await updateAssignment(assignment.assignmentId, {
        assignmentType,
        classId: assignment.classId,
        title,
        description: description || undefined,
        startTime: startTime?.toISOString(),
        endTime: endTime && !noEndTime ? endTime.toISOString() : undefined,
        allowLateSubmission,
        status,
        problems, // Preserve existing problems
      })

      // Navigate back to assignment detail page
      navigate(`/teacher/assignment/${assignment.assignmentId}`)
    } catch (error: any) {
      console.error('Failed to update assignment:', error)
      setError(error.message || 'Không thể cập nhật bài tập')
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
        <Navigation />
        <Loading message="Đang tải thông tin bài tập..." fullScreen />
      </Box>
    )
  }

  if (!assignment || !classData) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
        <Navigation />
        <Loading message="Không tìm thấy bài tập hoặc lớp học" fullScreen />
      </Box>
    )
  }
  
  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      
      {isSubmitting && <Loading message="Đang lưu thay đổi..." fullScreen />}
      
      <Container maxWidth="md" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate(`/teacher/assignment/${assignment.assignmentId}`)}
            sx={{ mb: 2 }}
          >
            Quay lại
          </Button>
          <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 1 }}>
            Chỉnh sửa bài tập
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Lớp: {classData?.className} ({classData?.classCode})
          </Typography>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        <Paper sx={{ p: 4 }}>
          <form onSubmit={handleSubmit}>
            {/* Title */}
            <TextField
              fullWidth
              required
              label="Tên bài tập"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="VD: Bài tập tuần 3 - Mảng và Chuỗi"
              sx={{ mb: 3 }}
            />

            {/* Description */}
            <TextField
              fullWidth
              label="Mô tả"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Mô tả ngắn gọn về bài tập..."
              multiline
              rows={4}
              sx={{ mb: 3 }}
            />

            {/* Assignment Type */}
            <FormControl fullWidth sx={{ mb: 3 }}>
              <InputLabel>Loại bài tập</InputLabel>
              <Select
                value={assignmentType}
                onChange={(e) => setAssignmentType(e.target.value as AssignmentType)}
                label="Loại bài tập"
              >
                <MenuItem value="HOMEWORK">Bài tập về nhà</MenuItem>
                <MenuItem value="EXAM">Bài kiểm tra</MenuItem>
                <MenuItem value="PRACTICE">Luyện tập</MenuItem>
              </Select>
            </FormControl>

            {/* Status */}
            <FormControl fullWidth sx={{ mb: 3 }}>
              <InputLabel>Trạng thái</InputLabel>
              <Select
                value={status}
                onChange={(e) => setStatus(e.target.value as AssignmentStatus)}
                label="Trạng thái"
              >
                <MenuItem value="DRAFT">Nháp</MenuItem>
                <MenuItem value="PUBLISHED">Đã giao cho sinh viên</MenuItem>
                <MenuItem value="CLOSED">Đã đóng</MenuItem>
              </Select>
            </FormControl>

            <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={vi}>
              {/* Start Time */}
              <DateTimePicker
                label="Thời gian bắt đầu *"
                value={startTime}
                onChange={(newValue: Date | null) => setStartTime(newValue)}
                format="dd/MM/yyyy HH:mm"
                ampm={false}
                slotProps={{
                  textField: {
                    fullWidth: true,
                    sx: { mb: 3 },
                  },
                }}
              />

              {/* No End Time Checkbox */}
              <FormControlLabel
                control={
                  <Checkbox
                    checked={noEndTime}
                    onChange={(e) => setNoEndTime(e.target.checked)}
                  />
                }
                label="Không có thời gian kết thúc"
                sx={{ mb: 2, display: 'block' }}
              />

              {/* End Time */}
              {!noEndTime && (
                <DateTimePicker
                  label="Thời gian kết thúc"
                  value={endTime}
                  onChange={(newValue: Date | null) => setEndTime(newValue)}
                  format="dd/MM/yyyy HH:mm"
                  ampm={false}
                  minDateTime={startTime || undefined}
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
              label="Cho phép nộp bài trễ"
              sx={{ mb: 3, display: 'block' }}
            />

            <Alert severity="info" sx={{ mb: 3 }}>
              Các thay đổi sẽ được lưu và cập nhật cho tất cả sinh viên trong lớp.
            </Alert>

            {/* Actions */}
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button
                variant="outlined"
                startIcon={<CancelIcon />}
                onClick={() => navigate(`/teacher/assignment/${assignment.assignmentId}`)}
                disabled={isSubmitting}
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
                startIcon={<SaveIcon />}
                disabled={isSubmitting}
                sx={{
                  bgcolor: 'secondary.main',
                  color: 'primary.main',
                  '&:hover': {
                    bgcolor: 'primary.main',
                    color: 'secondary.main',
                  },
                }}
              >
                {isSubmitting ? 'Đang lưu...' : 'Lưu thay đổi'}
              </Button>
            </Box>
          </form>
        </Paper>
      </Container>
    </Box>
  )
}
