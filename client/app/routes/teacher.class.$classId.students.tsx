import { useState } from 'react'
import { redirect, useLoaderData, Link } from 'react-router'
import type { Route } from './+types/teacher.class.$classId.students'
import { auth } from '~/auth'
import { mockClasses } from '~/data/mock'
import { Navigation } from '~/components/Navigation'
import {
  Box,
  Container,
  Typography,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  TextField,
  InputAdornment,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import EmailIcon from '@mui/icons-material/Email'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'

interface Student {
  id: string
  studentId: string
  name: string
  email: string
  enrolledDate: string
  status: 'active' | 'inactive'
}

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  const classData = mockClasses.find((c) => c.id === params.classId)
  if (!classData) {
    throw new Response('Lớp học không tồn tại', { status: 404 })
  }

  // Mock students data
  const students: Student[] = Array.from({ length: classData.studentCount }, (_, i) => ({
    id: `student-${i + 1}`,
    studentId: `2021${(600000 + i).toString().padStart(6, '0')}`,
    name: `Sinh viên ${i + 1}`,
    email: `student${i + 1}@utc2.edu.vn`,
    enrolledDate: new Date(Date.now() - Math.random() * 90 * 24 * 60 * 60 * 1000).toISOString(),
    status: Math.random() > 0.1 ? 'active' : 'inactive',
  }))

  return { user, classData, students }
}

export default function ManageStudents() {
  const { classData, students } = useLoaderData<typeof clientLoader>()
  const [searchQuery, setSearchQuery] = useState('')
  const [openDialog, setOpenDialog] = useState(false)

  const filteredStudents = students.filter(
    (student) =>
      student.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      student.studentId.includes(searchQuery) ||
      student.email.toLowerCase().includes(searchQuery.toLowerCase())
  )

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Button
            component={Link}
            to={`/teacher/class/${classData.id}`}
            startIcon={<ArrowBackIcon />}
            sx={{ mb: 2, color: 'text.secondary' }}
          >
            Quay lại lớp học
          </Button>
          <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 1 }}>
            Quản lý sinh viên
          </Typography>
          <Typography variant="body1" color="text.secondary">
            {classData.name} ({classData.code}) • {students.length} sinh viên
          </Typography>
        </Box>

        {/* Actions Bar */}
        <Box sx={{ mb: 3, display: 'flex', gap: 2, justifyContent: 'space-between', alignItems: 'center' }}>
          <TextField
            placeholder="Tìm theo tên, MSSV, email..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            sx={{ flex: 1, maxWidth: 500 }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            }}
          />
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setOpenDialog(true)}
            sx={{
              bgcolor: 'secondary.main',
              color: 'primary.main',
              '&:hover': {
                bgcolor: 'primary.main',
                color: 'secondary.main',
              },
            }}
          >
            Thêm sinh viên
          </Button>
        </Box>

        {/* Students Table */}
        <TableContainer component={Paper} sx={{ boxShadow: 3 }}>
          <Table>
            <TableHead sx={{ bgcolor: 'secondary.main' }}>
              <TableRow>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>MSSV</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Họ tên</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Email</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Ngày tham gia</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Trạng thái</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }} align="right">
                  Thao tác
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredStudents.map((student) => (
                <TableRow key={student.id} hover>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 'bold' }}>
                      {student.studentId}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      {student.name}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      <EmailIcon sx={{ fontSize: 16, color: 'text.secondary' }} />
                      <Typography variant="body2" color="text.secondary">
                        {student.email}
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">
                      {new Date(student.enrolledDate).toLocaleDateString('vi-VN')}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={student.status === 'active' ? 'Đang học' : 'Nghỉ'}
                      size="small"
                      color={student.status === 'active' ? 'success' : 'default'}
                    />
                  </TableCell>
                  <TableCell align="right">
                    <IconButton
                      size="small"
                      sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {filteredStudents.length === 0 && (
          <Paper sx={{ p: 6, textAlign: 'center', mt: 3 }}>
            <Typography variant="h6" color="text.secondary">
              {searchQuery ? 'Không tìm thấy sinh viên nào' : 'Chưa có sinh viên nào trong lớp'}
            </Typography>
          </Paper>
        )}

        {/* Add Student Dialog */}
        <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
          <DialogTitle sx={{ bgcolor: 'secondary.main', color: 'primary.main' }}>
            Thêm sinh viên vào lớp
          </DialogTitle>
          <DialogContent sx={{ mt: 2 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Nhập danh sách MSSV (mỗi MSSV một dòng) hoặc import từ file CSV
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={8}
              placeholder="2021600001&#10;2021600002&#10;2021600003&#10;..."
              sx={{ mb: 2 }}
            />
            <Button variant="outlined" component="label" fullWidth>
              Hoặc chọn file CSV
              <input type="file" accept=".csv" hidden />
            </Button>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenDialog(false)}>Hủy</Button>
            <Button
              variant="contained"
              onClick={() => setOpenDialog(false)}
              sx={{
                bgcolor: 'secondary.main',
                color: 'primary.main',
                '&:hover': {
                  bgcolor: 'primary.main',
                  color: 'secondary.main',
                },
              }}
            >
              Thêm sinh viên
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </Box>
  )
}
