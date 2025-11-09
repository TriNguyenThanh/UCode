import { useState } from 'react'
import { redirect, useLoaderData, Link, useRevalidator } from 'react-router'
import type { Route } from './+types/teacher.class.$classId.students'
import { auth } from '~/auth'
import * as ClassService from '~/services/classService'
import { Navigation } from '~/components/Navigation'
import AddStudentDialog from '~/components/AddStudentDialog'
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
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Snackbar,
  Alert,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import EmailIcon from '@mui/icons-material/Email'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'

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
  const revalidator = useRevalidator()
  const [searchQuery, setSearchQuery] = useState('')
  const [openDialog, setOpenDialog] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [studentToDelete, setStudentToDelete] = useState<{ id: string; name: string } | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const filteredStudents = students.filter(
    (student) =>
      student.fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (student.studentCode && student.studentCode.includes(searchQuery)) ||
      student.email.toLowerCase().includes(searchQuery.toLowerCase())
  )

  const handleDialogSuccess = () => {
    revalidator.revalidate() // Reload data
    setOpenDialog(false)
  }

  const handleRemoveStudent = async (studentId: string) => {
    setDeleteDialogOpen(false)
    setStudentToDelete(null)

    try {
      await ClassService.removeStudentFromClass(classData.classId, studentId)
      setSuccessMessage('Đã xóa sinh viên khỏi lớp!')
      revalidator.revalidate()
    } catch (error) {
      console.error('Error removing student:', error)
      setErrorMessage('Không thể xóa sinh viên. Vui lòng thử lại.')
    }
  }

  const openDeleteDialog = (studentId: string, studentName: string) => {
    setStudentToDelete({ id: studentId, name: studentName })
    setDeleteDialogOpen(true)
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
                      onClick={() => openDeleteDialog(student.userId, student.fullName)}
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

        {/* Add Student Dialog - New Smart Import System */}
        <AddStudentDialog
          open={openDialog}
          classId={classData.classId}
          onClose={() => setOpenDialog(false)}
          onSuccess={handleDialogSuccess}
        />

        {/* Delete Confirmation Dialog */}
        <Dialog
          open={deleteDialogOpen}
          onClose={() => setDeleteDialogOpen(false)}
          maxWidth="xs"
          fullWidth
        >
          <DialogTitle>Xác nhận xóa sinh viên</DialogTitle>
          <DialogContent>
            <DialogContentText>
              Bạn có chắc muốn xóa sinh viên <strong>{studentToDelete?.name}</strong> khỏi lớp?
            </DialogContentText>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDeleteDialogOpen(false)}>Hủy</Button>
            <Button
              onClick={() => studentToDelete && handleRemoveStudent(studentToDelete.id)}
              color="error"
              variant="contained"
            >
              Xóa
            </Button>
          </DialogActions>
        </Dialog>

        {/* Success Snackbar */}
        <Snackbar
          open={!!successMessage}
          autoHideDuration={3000}
          onClose={() => setSuccessMessage(null)}
          anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
        >
          <Alert
            onClose={() => setSuccessMessage(null)}
            severity="success"
            icon={<CheckCircleIcon />}
          >
            {successMessage}
          </Alert>
        </Snackbar>

        {/* Error Snackbar */}
        <Snackbar
          open={!!errorMessage}
          autoHideDuration={4000}
          onClose={() => setErrorMessage(null)}
          anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
        >
          <Alert onClose={() => setErrorMessage(null)} severity="error">
            {errorMessage}
          </Alert>
        </Snackbar>
      </Container>
    </Box>
  )
}
