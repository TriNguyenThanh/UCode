import { useState } from 'react'
import { redirect, useNavigate, Form, useActionData } from 'react-router'
import type { Route } from './+types/teacher.class.create'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  Alert,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material'
import SaveIcon from '@mui/icons-material/Save'
import CancelIcon from '@mui/icons-material/Cancel'

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }
  return { user }
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const formData = await request.formData()
  const name = formData.get('name') as string
  const code = formData.get('code') as string
  const semester = formData.get('semester') as string
  const description = formData.get('description') as string

  if (!name || !code || !semester) {
    return { error: 'Vui lòng điền đầy đủ thông tin bắt buộc' }
  }

  // Create class (mock - in real app, this would call API)
  const newClassId = `class-${Date.now()}`
  
  // Redirect to class detail page
  return redirect(`/teacher/class/${newClassId}`)
}

export default function CreateClass() {
  const navigate = useNavigate()
  const actionData = useActionData<typeof clientAction>()
  const [semester, setSemester] = useState('HK1 2024-2025')

  const currentYear = new Date().getFullYear()
  const semesters = [
    `HK1 ${currentYear}-${currentYear + 1}`,
    `HK2 ${currentYear}-${currentYear + 1}`,
    `HK3 ${currentYear}-${currentYear + 1}`,
  ]

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      
      <Container maxWidth="md" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 1 }}>
            Tạo lớp học mới
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Tạo lớp học để quản lý sinh viên và bài tập
          </Typography>
        </Box>

        {actionData?.error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {actionData.error}
          </Alert>
        )}

        <Paper sx={{ p: 4 }}>
          <Form method="post">
            {/* Class Name */}
            <TextField
              fullWidth
              required
              name="name"
              label="Tên lớp học"
              placeholder="VD: Cấu trúc dữ liệu và Giải thuật"
              sx={{ mb: 3 }}
            />

            {/* Class Code */}
            <TextField
              fullWidth
              required
              name="code"
              label="Mã lớp"
              placeholder="VD: CS201, IT301..."
              sx={{ mb: 3 }}
              helperText="Mã lớp học (viết tắt), dùng để định danh lớp"
            />

            {/* Semester */}
            <FormControl fullWidth sx={{ mb: 3 }}>
              <InputLabel>Học kỳ *</InputLabel>
              <Select
                value={semester}
                onChange={(e) => setSemester(e.target.value)}
                label="Học kỳ *"
                name="semester"
              >
                {semesters.map((sem) => (
                  <MenuItem key={sem} value={sem}>
                    {sem}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {/* Description */}
            <TextField
              fullWidth
              name="description"
              label="Mô tả"
              placeholder="Mô tả ngắn gọn về nội dung lớp học..."
              multiline
              rows={4}
              sx={{ mb: 3 }}
            />

            <Alert severity="info" sx={{ mb: 3 }}>
              Sau khi tạo lớp học, bạn có thể thêm sinh viên và tạo bài tập cho lớp.
            </Alert>

            {/* Actions */}
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button
                variant="outlined"
                startIcon={<CancelIcon />}
                onClick={() => navigate('/teacher/home')}
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
                Tạo lớp học
              </Button>
            </Box>
          </Form>
        </Paper>
      </Container>
    </Box>
  )
}
