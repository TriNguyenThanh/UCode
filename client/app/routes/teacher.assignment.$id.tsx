import { useState } from 'react'
import { redirect, useLoaderData, Link, useRevalidator } from 'react-router'
import type { Route } from './+types/teacher.assignment.$id'
import { auth } from '~/auth'
import type { Assignment, AssignmentUser, AssignmentStatistics } from '~/types/index'
import { Navigation } from '~/components/Navigation'
import { AddProblemDialog } from '~/components/AddProblemDialog'
import {
  getAssignment,
  getAssignmentStudents,
  getAssignmentStatistics,
  deleteAssignment,
  updateAssignment,
  getAssignmentsByClass
} from '~/services/assignmentService'
import { getAllStudents } from '~/services/studentService'
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
import VisibilityIcon from '@mui/icons-material/Visibility'

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

  try {
    // Fetch assignment data
    const assignment = await getAssignment(params.id)

    // Fetch students enrolled in this assignment
    const assignmentStudents = await getAssignmentStudents(params.id)

    // Fetch all students in the class using new API
    const classStudents = await getAllStudents({
      classId: assignment.classId,
      pageNumber: 1,
      pageSize: 1000, // Get all students
    })

    // Fetch statistics
    const statistics = await getAssignmentStatistics(params.id)

    return { 
      user, 
      assignment, 
      students: assignmentStudents,
      classStudents: classStudents.items,
      statistics
    }
  } catch (error) {
    console.error('Failed to load assignment:', error)
    throw new Response('Không thể tải thông tin bài tập', { status: 500 })
  }
}

export default function TeacherAssignmentDetail() {
  const { assignment, students, classStudents, statistics } = useLoaderData<typeof clientLoader>()
  const revalidator = useRevalidator()
  const [tabValue, setTabValue] = useState(0)
  const [openDialog, setOpenDialog] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [problemToDelete, setProblemToDelete] = useState<string | null>(null)
  const [deleteAssignmentDialogOpen, setDeleteAssignmentDialogOpen] = useState(false)


  // Calculate statistics from students data
  const submittedStudents = students.filter((s) => s.status === 'SUBMITTED' || s.status === 'GRADED')
  const gradedStudents = students.filter((s) => s.status === 'GRADED')
  const pendingGrading = submittedStudents.length - gradedStudents.length

  const HandleGradeProblem = (problemId: string) => {
    window.location.href = `/teacher/grading/${assignment.assignmentId}/${problemId}`
  }

  const handleDeleteProblem = (problemId: string) => {
    setProblemToDelete(problemId)
    setDeleteDialogOpen(true)
  }

  const confirmDeleteProblem = async () => {
    if (!problemToDelete) return

    try {
      // TODO: Implement API call to remove problem from assignment
      console.log('Deleting problem:', problemToDelete)
      setDeleteDialogOpen(false)
      setProblemToDelete(null)
      revalidator.revalidate()
    } catch (error) {
      console.error('Failed to delete problem:', error)
      alert('Không thể xóa bài khỏi assignment')
    }
  }

  const handleDeleteAssignment = async () => {
    try {
      await deleteAssignment(assignment.assignmentId)
      alert('Đã xóa assignment thành công')
      window.location.href = `/teacher/class/${assignment.classId}`
    } catch (error) {
      console.error('Failed to delete assignment:', error)
      alert('Không thể xóa assignment')
    }
  }


  const handleSaveProblems = async (problems: { problemId: string; points: number; orderIndex: number }[]) => {
    try {
      // Update assignment with new problems
      await updateAssignment(assignment.assignmentId, {
        assignmentType: assignment.assignmentType,
        classId: assignment.classId,
        title: assignment.title,
        description: assignment.description,
        startTime: assignment.startTime,
        endTime: assignment.endTime,
        allowLateSubmission: assignment.allowLateSubmission,
        status: assignment.status,
        problems,
      })

      // Revalidate to refresh data
      revalidator.revalidate()
    } catch (error) {
      console.error('Failed to save problems:', error)
      throw error
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            <Link to={`/teacher/class/${assignment.classId}`} style={{ textDecoration: 'none', color: 'inherit' }}>
              Lớp học
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
                  label={`Đã nộp: ${submittedStudents.length}/${statistics.totalStudents}`}
                  color="primary"
                  sx={{ bgcolor: 'primary.main', color: 'secondary.main' }}
                />
                {/* <Chip label={`Đã chấm: ${statistics.graded}`} color="success" /> */}
                {/* <Chip label={`Chưa chấm: ${pendingGrading}`} color="warning" /> */}
                {assignment.endTime && (
                  <Chip label={`Hạn: ${new Date(assignment.endTime).toLocaleDateString('vi-VN')}`} />
                )}
              </Box>
            </Box>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Button
                variant="outlined"
                startIcon={<EditIcon />}
                component={Link}
                to={`/teacher/assignment/${assignment.assignmentId}/edit`}
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

              {/* <Button
                variant="contained"
                component={Link}
                to={`/teacher/gradingv2/${assignment.assignmentId}`}
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
              </Button> */}

              <Button
                variant="outlined"
                color="error"
                startIcon={<DeleteIcon />}
                onClick={() => setDeleteAssignmentDialogOpen(true)}
              >
                Xóa
              </Button>
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
          <Tab label="Bảng điểm" />
          <Tab label="Thống kê" />
        </Tabs>
      </Box>

        {/* Problems Tab */}
        <TabPanel value={tabValue} index={0}>
          <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
              Danh sách bài ({assignment.problems?.length || 0} bài)
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
                  <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }} align="center">Submission</TableCell>
                  <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }} align="center">Thao tác</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {assignment.problems?.map((problem, index) => (
                  <TableRow key={problem.problemId} hover>
                    <TableCell>{index + 1}</TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <CodeIcon sx={{ color: 'secondary.main' }} />
                        <Link
                          to={`/problem/${problem.problemId}`}
                          style={{ textDecoration: 'none', color: 'inherit', fontWeight: 500 }}
                        >
                          {problem.title}
                        </Link>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={problem.difficulty}
                        size="small"
                        color={
                          problem.difficulty === 'EASY'
                            ? 'success'
                            : problem.difficulty === 'HARD'
                              ? 'error'
                              : 'warning'
                        }
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" fontWeight="bold">
                        {problem.points} điểm
                      </Typography>
                    </TableCell>
                    <TableCell align="center">

                      <IconButton
                        size="small"
                        onClick={() => HandleGradeProblem(problem.problemId)}
                        sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}
                        title="Xóa bài khỏi assignment"
                      >
                        <VisibilityIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                    <TableCell align="center">

                      <IconButton
                        size="small"
                        onClick={() => handleDeleteProblem(problem.problemId)}
                        sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}
                        title="Xóa bài khỏi assignment"
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </TabPanel>

      {/* Grade Tab */}
      <TabPanel value={tabValue} index={1}>
        <Box sx={{ mb: 2 }}>
          <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
            Bảng điểm ({classStudents.length} sinh viên)
          </Typography>
        </Box>

        <TableContainer component={Paper} sx={{ boxShadow: 3 }}>
          <Table>
            <TableHead sx={{ bgcolor: 'secondary.main' }}>
              <TableRow>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>STT</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>MSSV</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Họ tên</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Email</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }} align="center">Trạng thái</TableCell>
                <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }} align="center">Điểm</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {classStudents.map((student, index) => {
                // Find matching assignment student data
                const assignmentStudent = students.find(s => s.userId === student.userId)
                
                return (
                  <TableRow key={student.userId} hover>
                    <TableCell>{index + 1}</TableCell>
                    <TableCell>
                      <Typography variant="body2" fontWeight="500">
                        {student.studentCode || '-'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" fontWeight="500">
                        {student.fullName}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        {student.email}
                      </Typography>
                    </TableCell>
                    <TableCell align="center">
                      {assignmentStudent ? (
                        assignmentStudent.status === 'GRADED' ? (
                          <Chip
                            icon={<CheckCircleIcon />}
                            label="Đã chấm"
                            size="small"
                            color="success"
                          />
                        ) : assignmentStudent.status === 'SUBMITTED' ? (
                          <Chip label="Chờ chấm" size="small" color="warning" />
                        ) : assignmentStudent.status === 'IN_PROGRESS' ? (
                          <Chip label="Đang làm" size="small" color="info" />
                        ) : (
                          <Chip icon={<CancelIcon />} label="Chưa bắt đầu" size="small" />
                        )
                      ) : (
                        <Chip label="Chưa tham gia" size="small" />
                      )}
                    </TableCell>
                    <TableCell align="center">
                      <Typography
                        variant="body2"
                        fontWeight="bold"
                        sx={{
                          color: assignmentStudent?.score === assignmentStudent?.maxScore ? '#34C759' : '#FF3B30',
                        }}
                      >
                        {assignmentStudent?.score !== null && assignmentStudent?.score !== undefined
                          ? `${assignmentStudent.score}/${assignmentStudent.maxScore}`
                          : '-'}
                      </Typography>
                    </TableCell>
                  </TableRow>
                )
              })}
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
                  {statistics.totalStudents}
                </Typography>
              </Box>
              <Box sx={{ flex: '1 1 200px' }}>
                <Typography variant="body2" color="text.secondary">
                  Đã nộp
                </Typography>
                <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'success.main' }}>
                  {statistics.submitted}
                </Typography>
              </Box>
              <Box sx={{ flex: '1 1 200px' }}>
                <Typography variant="body2" color="text.secondary">
                  Đang làm
                </Typography>
                <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'info.main' }}>
                  {statistics.inProgress}
                </Typography>
              </Box>
              <Box sx={{ flex: '1 1 200px' }}>
                <Typography variant="body2" color="text.secondary">
                  Chưa bắt đầu
                </Typography>
                <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'warning.main' }}>
                  {statistics.notStarted}
                </Typography>
              </Box>
              <Box sx={{ flex: '1 1 200px' }}>
                <Typography variant="body2" color="text.secondary">
                  Đã chấm
                </Typography>
                <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'success.main' }}>
                  {statistics.graded}
                </Typography>
              </Box>
              <Box sx={{ flex: '1 1 200px' }}>
                <Typography variant="body2" color="text.secondary">
                  Điểm trung bình
                </Typography>
                <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'primary.main' }}>
                  {statistics.averageScore?.toFixed(1) || '0'}
                </Typography>
              </Box>
              <Box sx={{ flex: '1 1 200px' }}>
                <Typography variant="body2" color="text.secondary">
                  Tỷ lệ hoàn thành
                </Typography>
                <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'info.main' }}>
                  {statistics.completionRate?.toFixed(1) || '0'}%
                </Typography>
              </Box>
            </Box>
          </Paper>
        </TabPanel>

        {/* Add Problem Dialog */}
        <AddProblemDialog
          open={openDialog}
          onClose={() => setOpenDialog(false)}
          existingProblems={assignment.problems || []}
          onSave={handleSaveProblems}
        />

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

        {/* Delete Assignment Confirmation Dialog */}
        <Dialog open={deleteAssignmentDialogOpen} onClose={() => setDeleteAssignmentDialogOpen(false)}>
          <DialogTitle sx={{ bgcolor: 'error.main', color: 'white' }}>
            Xác nhận xóa Assignment
          </DialogTitle>
          <DialogContent sx={{ mt: 2 }}>
            <Typography>
              Bạn có chắc chắn muốn xóa assignment "{assignment.title}"?
              Tất cả dữ liệu liên quan sẽ bị xóa và không thể khôi phục.
            </Typography>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDeleteAssignmentDialogOpen(false)}>Hủy</Button>
            <Button
              variant="contained"
              color="error"
              onClick={handleDeleteAssignment}
            >
              Xóa Assignment
            </Button>
          </DialogActions>
        </Dialog>

      </Container>
    </Box>
  )
}
