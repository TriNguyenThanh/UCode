import { useState } from 'react'
import { redirect, useLoaderData, useNavigate } from 'react-router'
import type { Route } from './+types/teacher.grading.$assignmentId.$problemId'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  getAssignment,
  getAssignmentStudents,
} from '~/services/assignmentService'
import { getBestSubmissions } from '~/services/submissionService'
import { getProblem } from '~/services/problemService'
import { getClassById } from '~/services/classService'
import { getStudentById } from '~/services/studentService'
import type {
  BestSubmission,
} from '~/types'
import {
  Box,
  Container,
  Typography,
  Button,
  Paper,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
} from '@mui/material'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  if (!params.assignmentId) {
    throw new Response('Assignment ID is required', { status: 400 })
  }

  if (!params.problemId) {
    throw new Response('Problem ID is required', { status: 400 })
  }

  try {
    // Fetch assignment, class, and students in parallel
    const [assignment, students] = await Promise.all([
      getAssignment(params.assignmentId),
      getAssignmentStudents(params.assignmentId),
    ])

    const classData = await getClassById(assignment.classId)

    // Fetch the specific problem
    const problem = await getProblem(params.problemId)

    // Fetch best submissions for this problem
    const bestSubmissions = await getBestSubmissions(params.assignmentId, params.problemId, 1, 100)

    return {
      user,
      assignment,
      classData,
      problem,
      students,
      bestSubmissions,
    }
  } catch (error: any) {
    console.error('Failed to load grading data:', error)
    throw new Response(error.message || 'Không thể tải dữ liệu chấm bài', { status: 404 })
  }
}

export default function TeacherGrading() {
  const { assignment, classData, problem, students, bestSubmissions } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)

  const handleSubmissionClick = (submissionId: string) => {
    // Navigate to grading page for this submission
    navigate(`/teacher/grading/${assignment.assignmentId}/${problem.problemId}/${submissionId}`)
  }

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  // Calculate paginated submissions
  const paginatedSubmissions = bestSubmissions.slice(
    page * rowsPerPage,
    page * rowsPerPage + rowsPerPage
  )

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50', pb: 4 }}>
      <Navigation />
      
      <Container maxWidth="xl" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 3 }}>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            <Box component="span" sx={{ cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }} onClick={() => navigate(`/teacher/assignment/${assignment.assignmentId}`)}>
              Chấm bài: {classData?.className} ({classData?.classCode})
            </Box>
            {' / '}
            <Box component="span" sx={{ cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }} onClick={() => navigate(`/teacher/assignment/${assignment.assignmentId}`)}>
              {assignment.title}
            </Box>
            {' / '}
            {problem.title}
          </Typography>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h5" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
              Danh sách bài nộp
            </Typography>
            <Button
              variant="outlined"
              startIcon={<ArrowBackIcon />}
              onClick={() => navigate(`/teacher/assignment/${assignment.assignmentId}`)}
              sx={{ borderColor: 'secondary.main', color: 'secondary.main' }}
            >
              Quay lại
            </Button>
          </Box>
        </Box>

        {/* Problem Info */}
        <Paper sx={{ p: 2, mb: 3, bgcolor: 'secondary.main' }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Box>
              <Typography variant="h6" sx={{ color: 'primary.main', fontWeight: 'bold' }}>
                {problem.title}
              </Typography>
              <Typography variant="body2" sx={{ color: 'primary.main' }}>
                Độ khó: {problem.difficulty} • Tổng số bài nộp: {bestSubmissions.length}
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h4" sx={{ color: 'success.main', fontWeight: 'bold' }}>
                  {bestSubmissions.filter(s => s.passedTestcase === s.totalTestcase).length}
                </Typography>
                <Typography variant="caption" sx={{ color: 'primary.main' }}>
                  Đạt
                </Typography>
              </Box>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h4" sx={{ color: 'warning.main', fontWeight: 'bold' }}>
                  {bestSubmissions.filter(s => s.passedTestcase !== s.totalTestcase && s.passedTestcase > 0).length}
                </Typography>
                <Typography variant="caption" sx={{ color: 'primary.main' }}>
                  Một phần
                </Typography>
              </Box>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h4" sx={{ color: 'error.main', fontWeight: 'bold' }}>
                  {bestSubmissions.filter(s => s.passedTestcase === 0).length}
                </Typography>
                <Typography variant="caption" sx={{ color: 'primary.main' }}>
                  Chưa đạt
                </Typography>
              </Box>
            </Box>
          </Box>
        </Paper>

        {/* Submissions Table */}
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow sx={{ bgcolor: 'grey.100' }}>
                <TableCell sx={{ fontWeight: 'bold' }}>STT</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Sinh viên</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>MSSV</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Trạng thái</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Test cases</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Thời gian</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Bộ nhớ</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Ngày nộp</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Điểm</TableCell>
                {/* <TableCell sx={{ fontWeight: 'bold' }}>Thao tác</TableCell> */}
              </TableRow>
            </TableHead>
            <TableBody>
              {bestSubmissions.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={10} align="center" sx={{ py: 4 }}>
                    <Typography variant="body1" color="text.secondary">
                      Chưa có sinh viên nào nộp bài
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                paginatedSubmissions.map((submission, index) => {
                  const isPassed = submission.passedTestcase === submission.totalTestcase
                  
                  return (
                    <TableRow
                      key={submission.submissionId}
                      hover
                      sx={{
                        cursor: 'pointer',
                        '&:hover': { bgcolor: 'action.hover' },
                      }}
                      onClick={() => handleSubmissionClick(submission.submissionId)}
                    >
                      <TableCell>{page * rowsPerPage + index + 1}</TableCell>
                      <TableCell>
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                          {submission.userFullName || 'N/A'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {submission.userCode || 'N/A'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={submission.status}
                          size="small"
                          color={
                            submission.status === 'Passed'
                              ? 'success'
                              : submission.status === 'Failed'
                              ? 'error'
                              : 'warning'
                          }
                        />
                      </TableCell>
                      <TableCell>
                        <Chip
                          icon={isPassed ? <CheckCircleIcon /> : <CancelIcon />}
                          label={`${submission.passedTestcase || 0}/${submission.totalTestcase || 0}`}
                          size="small"
                          color={isPassed ? 'success' : 'warning'}
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {submission.totalTime ? `${submission.totalTime}ms` : 'N/A'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {submission.totalMemory ? `${submission.totalMemory}KB` : 'N/A'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {submission.submittedAt
                            ? new Date(submission.submittedAt).toLocaleString('vi-VN')
                            : 'N/A'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {submission.score !== undefined && submission.score !== null ? (
                          <Chip
                            label={submission.score}
                            size="small"
                            color={submission.score >= 80 ? 'success' : submission.score >= 50 ? 'warning' : 'error'}
                          />
                        ) : (
                          <Typography variant="body2" color="text.secondary">
                            Chưa chấm
                          </Typography>
                        )}
                      </TableCell>
                      {/* <TableCell>
                        <Button
                          size="small"
                          variant="outlined"
                          onClick={(e) => {
                            e.stopPropagation()
                            handleSubmissionClick(submission.submissionId)
                          }}
                          sx={{ borderColor: 'secondary.main', color: 'secondary.main' }}
                        >
                          Chấm bài
                        </Button>
                      </TableCell> */}
                    </TableRow>
                  )
                })
              )}
            </TableBody>
          </Table>
          <TablePagination
            rowsPerPageOptions={[5, 10, 25, 50]}
            component="div"
            count={bestSubmissions.length}
            rowsPerPage={rowsPerPage}
            page={page}
            onPageChange={handleChangePage}
            onRowsPerPageChange={handleChangeRowsPerPage}
            labelRowsPerPage="Số hàng mỗi trang:"
            labelDisplayedRows={({ from, to, count }) => `${from}-${to} trong ${count}`}
          />
        </TableContainer>
      </Container>
    </Box>
  )
}
