import { useState } from 'react'
import { redirect, useLoaderData, useNavigate, Form } from 'react-router'
import type { Route } from './+types/teacher.class.$classId.create-assignment'
import { auth } from '~/auth'
import { mockClasses } from '~/data/mock'
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
} from '@mui/material'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker'
import { vi } from 'date-fns/locale/vi'
import SaveIcon from '@mui/icons-material/Save'
import CancelIcon from '@mui/icons-material/Cancel'

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  const classData = mockClasses.find((c) => c.id === params.classId)
  if (!classData) {
    throw new Response('Lớp học không tồn tại', { status: 404 })
  }

  return { user, classData }
}

export async function clientAction({ request, params }: Route.ClientActionArgs) {
  const formData = await request.formData()
  const title = formData.get('title') as string
  const description = formData.get('description') as string
  const startDate = formData.get('startDate') as string
  const endDate = formData.get('endDate') as string
  const noEndDate = formData.get('noEndDate') === 'true'

  // Validate
  if (!title || !startDate) {
    return { error: 'Vui lòng điền đầy đủ thông tin bắt buộc' }
  }

  // Create assignment (mock - in real app, this would call API)
  const newAssignmentId = `assignment-${Date.now()}`
  
  // Redirect to assignment detail page
  return redirect(`/teacher/assignment/${newAssignmentId}`)
}

export default function CreateAssignment() {
  const { classData } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const [startDate, setStartDate] = useState<Date | null>(new Date())
  const [endDate, setEndDate] = useState<Date | null>(new Date(Date.now() + 7 * 24 * 60 * 60 * 1000))
  const [noEndDate, setNoEndDate] = useState(false)

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
            Lớp: {classData.name} ({classData.code})
          </Typography>
        </Box>

        <Paper sx={{ p: 4 }}>
          <Form method="post">
            {/* Title */}
            <TextField
              fullWidth
              required
              name="title"
              label="Tên bài tập"
              placeholder="VD: Bài tập tuần 3 - Mảng và Chuỗi"
              sx={{ mb: 3 }}
            />

            {/* Description */}
            <TextField
              fullWidth
              name="description"
              label="Mô tả"
              placeholder="Mô tả ngắn gọn về bài tập..."
              multiline
              rows={4}
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
                    name: 'startDate',
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
                    name="noEndDate"
                    value="true"
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
                      name: 'endDate',
                      sx: { mb: 3 },
                    },
                  }}
                />
              )}
            </LocalizationProvider>

            <Alert severity="info" sx={{ mb: 3 }}>
              Sau khi tạo bài tập, bạn sẽ được chuyển đến trang quản lý bài tập để thêm các bài toán.
            </Alert>

            {/* Actions */}
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button
                variant="outlined"
                startIcon={<CancelIcon />}
                onClick={() => navigate(`/teacher/class/${classData.id}`)}
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
                sx={{
                  bgcolor: 'secondary.main',
                  color: 'primary.main',
                  '&:hover': {
                    bgcolor: 'primary.main',
                    color: 'secondary.main',
                  },
                }}
              >
                Tạo bài tập
              </Button>
            </Box>
          </Form>
        </Paper>
      </Container>
    </Box>
  )
}
