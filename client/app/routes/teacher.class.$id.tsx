import { redirect, useLoaderData, Link } from 'react-router'
import type { Route } from './+types/teacher.class.$id'
import { auth } from '~/auth'
import { mockClasses, mockAssignments } from '~/data/mock'
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
  Chip,
  IconButton,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import PeopleIcon from '@mui/icons-material/People'

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  const classData = mockClasses.find((c) => c.id === params.id)
  if (!classData) {
    throw new Response('Lớp học không tồn tại', { status: 404 })
  }

  const assignments = mockAssignments.filter((a) => a.classId === params.id)

  return { user, classData, assignments }
}

export default function TeacherClassDetail() {
  const { classData, assignments } = useLoaderData<typeof clientLoader>()

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
          <Box
            sx={{
              width: 80,
              height: 80,
              borderRadius: 2,
              bgcolor: 'secondary.main',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <Typography variant="h4" sx={{ color: 'primary.main', fontWeight: 'bold' }}>
              {classData.code.charAt(0)}
            </Typography>
          </Box>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 0.5 }}>
              {classData.name}
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Mã lớp: {classData.code} • Học kỳ: {classData.semester}
            </Typography>
          </Box>
          <Button
            variant="outlined"
            startIcon={<PeopleIcon />}
            component={Link}
            to={`/teacher/class/${classData.id}/students`}
            sx={{
              borderColor: 'secondary.main',
              color: 'secondary.main',
              '&:hover': {
                borderColor: 'primary.main',
                bgcolor: 'primary.main',
                color: 'white',
              },
            }}
          >
            Quản lý sinh viên ({classData.studentCount})
          </Button>
        </Box>
        <Typography variant="body1" color="text.secondary">
          {classData.description}
        </Typography>
      </Box>

      {/* Assignments Section */}
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h5" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
          Danh sách bài tập
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          component={Link}
          to={`/teacher/class/${classData.id}/create-assignment`}
          sx={{
            bgcolor: 'secondary.main',
            color: 'primary.main',
            '&:hover': {
              bgcolor: 'primary.main',
              color: 'secondary.main',
            },
          }}
        >
          Tạo bài tập mới
        </Button>
      </Box>

      <TableContainer component={Paper} sx={{ boxShadow: 3 }}>
        <Table>
          <TableHead sx={{ bgcolor: 'secondary.main' }}>
            <TableRow>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Tên bài tập</TableCell>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Hạn nộp</TableCell>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Số bài</TableCell>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Đã nộp</TableCell>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Trạng thái</TableCell>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }} align="right">
                Thao tác
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {assignments.map((assignment) => {
              const submittedCount = Math.floor(Math.random() * classData.studentCount)
              const daysUntilDue = Math.ceil(
                (new Date(assignment.dueDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24)
              )
              const isOverdue = daysUntilDue < 0

              return (
                <TableRow key={assignment.id} hover>
                  <TableCell>
                    <Link
                      to={`/teacher/assignment/${assignment.id}`}
                      style={{ textDecoration: 'none', color: 'inherit', fontWeight: 500 }}
                    >
                      {assignment.title}
                    </Link>
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                      <Typography variant="body2">
                        {new Date(assignment.dueDate).toLocaleDateString('vi-VN')}
                      </Typography>
                      {isOverdue ? (
                        <Typography variant="caption" color="error">
                          Quá hạn {Math.abs(daysUntilDue)} ngày
                        </Typography>
                      ) : (
                        <Typography variant="caption" color="text.secondary">
                          Còn {daysUntilDue} ngày
                        </Typography>
                      )}
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Chip label={`${assignment.problems.length} bài`} size="small" />
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {submittedCount}/{classData.studentCount}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      ({Math.round((submittedCount / classData.studentCount) * 100)}%)
                    </Typography>
                  </TableCell>
                  <TableCell>
                    {isOverdue ? (
                      <Chip label="Đã đóng" color="error" size="small" />
                    ) : (
                      <Chip label="Đang mở" color="success" size="small" />
                    )}
                  </TableCell>
                  <TableCell align="right">
                    <IconButton
                      size="small"
                      component={Link}
                      to={`/teacher/assignment/${assignment.id}`}
                      sx={{ color: 'secondary.main' }}
                    >
                      <EditIcon fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      </TableContainer>

      {assignments.length === 0 && (
        <Paper sx={{ p: 6, textAlign: 'center', bgcolor: 'grey.50' }}>
          <Typography variant="h6" color="text.secondary" gutterBottom>
            Chưa có bài tập nào
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Tạo bài tập đầu tiên cho lớp học này
          </Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            component={Link}
            to={`/teacher/class/${classData.id}/create-assignment`}
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
        </Paper>
      )}
      </Container>
    </Box>
  )
}
