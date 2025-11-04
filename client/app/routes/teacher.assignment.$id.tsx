import { useState } from 'react'
import { redirect, useLoaderData, Link } from 'react-router'
import type { Route } from './+types/teacher.assignment.$id'
import { auth } from '~/auth'
import { mockAssignments, mockClasses, mockProblems } from '~/data/mock'
import type { Problem } from '~/types/index'
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
  Tabs,
  Tab,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import CodeIcon from '@mui/icons-material/Code'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'

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

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  const assignment = mockAssignments.find((a) => a.id === params.id)
  if (!assignment) {
    throw new Response('Bài tập không tồn tại', { status: 404 })
  }

  const classData = mockClasses.find((c) => c.id === assignment.classId)

  // Mock student submissions
  const students = Array.from({ length: classData?.studentCount || 30 }, (_, i) => ({
    id: `student-${i + 1}`,
    name: `Sinh viên ${i + 1}`,
    studentId: `2021${(600000 + i).toString().padStart(6, '0')}`,
    submitted: Math.random() > 0.3,
    submittedAt: new Date(Date.now() - Math.random() * 7 * 24 * 60 * 60 * 1000).toISOString(),
    score: Math.random() > 0.3 ? Math.floor(Math.random() * 100) : null,
    problemsCompleted: Math.floor(Math.random() * assignment.problems.length),
  }))

  return { user, assignment, classData, students }
}

export default function TeacherAssignmentDetail() {
  const { assignment, classData, students } = useLoaderData<typeof clientLoader>()
  const [tabValue, setTabValue] = useState(0)
  const [openDialog, setOpenDialog] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [problemToDelete, setProblemToDelete] = useState<string | null>(null)

  const submittedStudents = students.filter((s) => s.submitted)
  const gradedStudents = submittedStudents.filter((s) => s.score !== null)
  const pendingGrading = submittedStudents.length - gradedStudents.length

  const handleDeleteProblem = (problemId: string) => {
    setProblemToDelete(problemId)
    setDeleteDialogOpen(true)
  }

  const confirmDeleteProblem = () => {
    console.log('Deleting problem:', problemToDelete)
    // TODO: Call API to remove problem from assignment
    setDeleteDialogOpen(false)
    setProblemToDelete(null)
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          <Link to={`/teacher/class/${classData?.id}`} style={{ textDecoration: 'none', color: 'inherit' }}>
            {classData?.name}
          </Link>{' '}
          / {assignment.title}
        </Typography>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
          <Box>
            <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 1 }}>
              {assignment.title}
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Chip
                icon={<CheckCircleIcon />}
                label={`Đã nộp: ${submittedStudents.length}/${students.length}`}
                color="primary"
                sx={{ bgcolor: 'primary.main', color: 'secondary.main' }}
              />
              <Chip label={`Đã chấm: ${gradedStudents.length}`} color="success" />
              <Chip label={`Chưa chấm: ${pendingGrading}`} color="warning" />
              <Chip label={`Hạn: ${new Date(assignment.dueDate).toLocaleDateString('vi-VN')}`} />
            </Box>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="outlined"
              startIcon={<EditIcon />}
              component={Link}
              to={`/teacher/assignment/${assignment.id}/edit`}
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
              Chỉnh sửa
            </Button>
            {pendingGrading > 0 && (
              <Button
                variant="contained"
                component={Link}
                to={`/teacher/grading/${assignment.id}`}
                sx={{
                  bgcolor: 'secondary.main',
                  color: 'primary.main',
                  '&:hover': {
                    bgcolor: 'primary.main',
                    color: 'secondary.main',
                  },
                }}
              >
                Chấm bài ({pendingGrading})
              </Button>
            )}
          </Box>
        </Box>
        <Typography variant="body1" color="text.secondary">
          {assignment.description}
        </Typography>
      </Box>

      {/* Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs
          value={tabValue}
          onChange={(_, newValue) => setTabValue(newValue)}
          sx={{
            '& .MuiTab-root': { color: 'text.secondary' },
            '& .Mui-selected': { color: 'secondary.main' },
            '& .MuiTabs-indicator': { bgcolor: 'primary.main' },
          }}
        >
          <Tab label="Danh sách bài" />
          <Tab label="Bài nộp sinh viên" />
          <Tab label="Thống kê" />
        </Tabs>
      </Box>

      {/* Problems Tab */}
      <TabPanel value={tabValue} index={0}>
        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
            Danh sách bài ({assignment.problems.length} bài)
          </Typography>
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
            Thêm bài
          </Button>
        </Box>

        <TableContainer component={Paper} sx={{ boxShadow: 3 }}>
          <Table>
            <TableHead sx={{ bgcolor: 'secondary.main' }}>
              <TableRow>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>STT</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Tên bài</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Độ khó</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Điểm</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }} align="right">
                  Thao tác
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {assignment.problems.map((problemId, index) => {
                const problem = mockProblems.find((p) => p.id === problemId)
                return (
                  <TableRow key={problemId} hover>
                    <TableCell>{index + 1}</TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <CodeIcon sx={{ color: 'secondary.main' }} />
                        <Link
                          to={`/problem/${problemId}`}
                          style={{ textDecoration: 'none', color: 'inherit', fontWeight: 500 }}
                        >
                          {problem?.title || `Problem ${problemId}`}
                        </Link>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={problem?.difficulty || 'Medium'}
                        size="small"
                        color={
                          problem?.difficulty === 'Easy'
                            ? 'success'
                            : problem?.difficulty === 'Hard'
                            ? 'error'
                            : 'warning'
                        }
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" fontWeight="bold">
                        {Math.floor(100 / assignment.problems.length)} điểm
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <IconButton
                        size="small"
                        component={Link}
                        to={`/teacher/problem/${problemId}/edit`}
                        sx={{ color: 'secondary.main' }}
                      >
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={() => handleDeleteProblem(problemId)}
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
      </TabPanel>

      {/* Submissions Tab */}
      <TabPanel value={tabValue} index={1}>
        <TableContainer component={Paper} sx={{ boxShadow: 3 }}>
          <Table>
            <TableHead sx={{ bgcolor: 'secondary.main' }}>
              <TableRow>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>MSSV</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Họ tên</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Trạng thái</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Thời gian nộp</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Hoàn thành</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Điểm</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }} align="right">
                  Thao tác
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {students.map((student) => (
                <TableRow key={student.id} hover>
                  <TableCell>{student.studentId}</TableCell>
                  <TableCell>{student.name}</TableCell>
                  <TableCell>
                    {student.submitted ? (
                      student.score !== null ? (
                        <Chip
                          icon={<CheckCircleIcon />}
                          label="Đã chấm"
                          size="small"
                          color="success"
                        />
                      ) : (
                        <Chip label="Chờ chấm" size="small" color="warning" />
                      )
                    ) : (
                      <Chip icon={<CancelIcon />} label="Chưa nộp" size="small" />
                    )}
                  </TableCell>
                  <TableCell>
                    {student.submitted
                      ? new Date(student.submittedAt).toLocaleString('vi-VN')
                      : '-'}
                  </TableCell>
                  <TableCell>
                    {student.submitted ? (
                      <Typography variant="body2">
                        {student.problemsCompleted}/{assignment.problems.length}
                      </Typography>
                    ) : (
                      '-'
                    )}
                  </TableCell>
                  <TableCell>
                    <Typography
                      variant="body2"
                      fontWeight="bold"
                      sx={{ color: student.score !== null ? 'success.main' : 'text.secondary' }}
                    >
                      {student.score !== null ? `${student.score} điểm` : '-'}
                    </Typography>
                  </TableCell>
                  <TableCell align="right">
                    {student.submitted && (
                      <Button
                        size="small"
                        variant="outlined"
                        component={Link}
                        to={`/teacher/grading/${assignment.id}?student=${student.id}`}
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
                        {student.score !== null ? 'Xem lại' : 'Chấm bài'}
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </TabPanel>

      {/* Statistics Tab */}
      <TabPanel value={tabValue} index={2}>
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 3 }}>
            Thống kê chi tiết
          </Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3 }}>
            <Box sx={{ flex: '1 1 200px' }}>
              <Typography variant="body2" color="text.secondary">
                Tổng sinh viên
              </Typography>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
                {students.length}
              </Typography>
            </Box>
            <Box sx={{ flex: '1 1 200px' }}>
              <Typography variant="body2" color="text.secondary">
                Đã nộp
              </Typography>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'success.main' }}>
                {submittedStudents.length}
              </Typography>
            </Box>
            <Box sx={{ flex: '1 1 200px' }}>
              <Typography variant="body2" color="text.secondary">
                Điểm trung bình
              </Typography>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'primary.main' }}>
                {gradedStudents.length > 0
                  ? Math.round(
                      gradedStudents.reduce((sum, s) => sum + (s.score || 0), 0) / gradedStudents.length
                    )
                  : 0}
              </Typography>
            </Box>
            <Box sx={{ flex: '1 1 200px' }}>
              <Typography variant="body2" color="text.secondary">
                Tỷ lệ nộp bài
              </Typography>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'info.main' }}>
                {Math.round((submittedStudents.length / students.length) * 100)}%
              </Typography>
            </Box>
          </Box>
        </Paper>
      </TabPanel>

      {/* Add Problem Dialog */}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ bgcolor: 'secondary.main', color: 'primary.main' }}>
          Thêm bài vào assignment
        </DialogTitle>
        <DialogContent sx={{ mt: 2 }}>
          <TextField
            fullWidth
            label="Tìm kiếm bài theo tên hoặc ID"
            variant="outlined"
            placeholder="VD: Two Sum, problem-1..."
          />
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
            Thêm bài
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Problem Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle sx={{ bgcolor: 'error.main', color: 'white' }}>
          Xác nhận xóa bài
        </DialogTitle>
        <DialogContent sx={{ mt: 2 }}>
          <Typography>
            Bạn có chắc chắn muốn xóa bài này khỏi assignment? 
            Hành động này không thể hoàn tác.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Hủy</Button>
          <Button
            variant="contained"
            color="error"
            onClick={confirmDeleteProblem}
          >
            Xóa
          </Button>
        </DialogActions>
      </Dialog>
      </Container>
    </Box>
  )
}
