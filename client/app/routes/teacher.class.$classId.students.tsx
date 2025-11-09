import { useState } from 'react'
import { redirect, useLoaderData, Link } from 'react-router'
import type { Route } from './+types/teacher.class.$classId.students'
import { auth } from '~/auth'
import * as ClassService from '~/services/classService'
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

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  try {
    // Get class detail and students from API
    const [classData, students] = await Promise.all([
      ClassService.getClassById(params.classId),
      ClassService.getClassStudents(params.classId),
    ])

    return { user, classData, students }
  } catch (error) {
    console.error('Error loading class students:', error)
    throw new Response('Lớp học không tồn tại', { status: 404 })
  }
}

export default function ManageStudents() {
  const { classData, students } = useLoaderData<typeof clientLoader>()
  const [searchQuery, setSearchQuery] = useState('')
  const [openDialog, setOpenDialog] = useState(false)
  const [studentIds, setStudentIds] = useState('')
  const [loading, setLoading] = useState(false)

  const filteredStudents = students.filter(
    (student) =>
      student.fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (student.studentCode && student.studentCode.includes(searchQuery)) ||
      student.email.toLowerCase().includes(searchQuery.toLowerCase())
  )

  const handleAddStudents = async () => {
    if (!studentIds.trim()) return

    setLoading(true)
    try {
      const ids = studentIds
        .split('\n')
        .map(id => id.trim())
        .filter(id => id.length > 0)

      for (const studentId of ids) {
        await ClassService.addStudentToClass(classData.classId, studentId)
      }

      alert(`Đã thêm ${ids.length} sinh viên vào lớp`)
      setOpenDialog(false)
      setStudentIds('')
      window.location.reload()
    } catch (error) {
      console.error('Error adding students:', error)
      alert('Không thể thêm sinh viên. Vui lòng thử lại.')
    } finally {
      setLoading(false)
    }
  }

  const handleRemoveStudent = async (studentId: string) => {
    if (!confirm('Bạn có chắc muốn xóa sinh viên này khỏi lớp?')) return

    try {
      await ClassService.removeStudentFromClass(classData.classId, studentId)
      alert('Đã xóa sinh viên khỏi lớp')
      window.location.reload()
    } catch (error) {
      console.error('Error removing student:', error)
      alert('Không thể xóa sinh viên. Vui lòng thử lại.')
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Button
            component={Link}
            to={`/teacher/class/${classData.classId}`}
            startIcon={<ArrowBackIcon />}
            sx={{ mb: 2, color: 'text.secondary' }}
          >
            Quay lại lớp học
          </Button>
          <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 1 }}>
            Quản lý sinh viên
          </Typography>
          <Typography variant="body1" color="text.secondary">
            {classData.className} ({classData.classCode}) • {students.length} sinh viên
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
                <TableRow key={student.userId} hover>
                  <TableCell>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 'bold' }}>
                      {student.studentCode || 'N/A'}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                      {student.fullName}
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
                      {student.dateOfBirth ? new Date(student.dateOfBirth).toLocaleDateString('vi-VN') : 'N/A'}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={student.status === 'Active' ? 'Đang học' : student.status === 'Inactive' ? 'Nghỉ' : 'Bị cấm'}
                      size="small"
                      color={student.status === 'Active' ? 'success' : 'default'}
                    />
                  </TableCell>
                  <TableCell align="right">
                    <IconButton
                      size="small"
                      sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}
                      onClick={() => handleRemoveStudent(student.userId)}
                      title="Xóa sinh viên"
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
              Nhập danh sách MSSV (mỗi MSSV một dòng)
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={8}
              placeholder="2021600001&#10;2021600002&#10;2021600003&#10;..."
              sx={{ mb: 2 }}
              value={studentIds}
              onChange={(e) => setStudentIds(e.target.value)}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenDialog(false)} disabled={loading}>
              Hủy
            </Button>
            <Button
              variant="contained"
              onClick={handleAddStudents}
              disabled={loading || !studentIds.trim()}
              sx={{
                bgcolor: 'secondary.main',
                color: 'primary.main',
                '&:hover': {
                  bgcolor: 'primary.main',
                  color: 'secondary.main',
                },
              }}
            >
              {loading ? 'Đang thêm...' : 'Thêm sinh viên'}
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </Box>
  )
}
