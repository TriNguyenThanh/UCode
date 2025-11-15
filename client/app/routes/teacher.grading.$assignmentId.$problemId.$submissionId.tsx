import { useState, useEffect } from 'react'
import { redirect, useLoaderData, useNavigate, useNavigation, useParams } from 'react-router'
import type { Route } from './+types/teacher.grading.$assignmentId.$problemId.$submissionId'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import { Loading } from '~/components/Loading'
import {
  getAssignment,
  gradeSubmission,
} from '~/services/assignmentService'
import { getSubmission } from '~/services/submissionService'
import { getProblem } from '~/services/problemService'
import { getClassById } from '~/services/classService'
import type { Submission } from '~/types'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import { vs } from 'react-syntax-highlighter/dist/esm/styles/prism'
import {
  Box,
  Container,
  Typography,
  Button,
  Paper,
  Chip,
  TextField,
  Divider,
  Card,
  CardContent,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'
import SaveIcon from '@mui/icons-material/Save'
import CodeIcon from '@mui/icons-material/Code'
import PersonIcon from '@mui/icons-material/Person'
import AssignmentIcon from '@mui/icons-material/Assignment'

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  if (!params.assignmentId || !params.problemId || !params.submissionId) {
    throw new Response('Missing required parameters', { status: 400 })
  }

  try {
    // Fetch all data in parallel
    const [assignmentData, submissionData, problemData] = await Promise.all([
      getAssignment(params.assignmentId),
      getSubmission(params.submissionId),
      getProblem(params.problemId),
    ])

    const classData = await getClassById(assignmentData.classId)

    return {
      user,
      assignment: assignmentData,
      classData,
      problem: problemData,
      submission: submissionData,
    }
  } catch (error: any) {
    console.error('Failed to load submission data:', error)
    throw new Response(error.message || 'Không thể tải dữ liệu bài nộp', { status: 404 })
  }
}

export default function SubmissionGrading() {
  const { assignment, classData, problem, submission } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const navigation = useNavigation()
  const params = useParams()
  
  const [score, setScore] = useState<number | string>(submission.score ?? '')
  const [feedback, setFeedback] = useState('')
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [isTransitioning, setIsTransitioning] = useState(false)

  const isLoading = navigation.state === 'loading'

  // Detect when submissionId changes to show loading
  useEffect(() => {
    setIsTransitioning(false)
  }, [params.submissionId])

  // Update form when submission changes
  useEffect(() => {
    setScore(submission.score ?? '')
    setFeedback('')
    setError(null)
  }, [submission.submissionId])

  const handleSaveGrade = async () => {
    if (!submission?.submissionId || !assignment?.assignmentId) {
      alert('Không tìm thấy bài làm để chấm điểm')
      return
    }

    setIsSaving(true)
    setError(null)
    try {
      await gradeSubmission(assignment.assignmentId, submission.submissionId, {
        score: typeof score === 'number' ? score : parseFloat(score as string) || undefined,
        teacherFeedback: feedback || undefined,
      })
      alert(`Đã lưu điểm cho ${submission.userFullName}`)
    } catch (err: any) {
      console.error('Failed to save grade:', err)
      setError(err.message || 'Không thể lưu điểm')
    } finally {
      setIsSaving(false)
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Passed':
        return 'success'
      case 'Failed':
      case 'CompilationError':
      case 'RuntimeError':
        return 'error'
      case 'TimeLimitExceeded':
      case 'MemoryLimitExceeded':
        return 'warning'
      default:
        return 'default'
    }
  }

  const isPassed = submission.passedTestcase === submission.totalTestcase

  // Show loading screen while navigation is in progress
  if (isLoading || isTransitioning) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
        <Navigation />
        <Loading fullScreen message="Đang tải dữ liệu..." />
      </Box>
    )
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50', pb: 4 }}>
      <Navigation />
      {isSaving && <Loading message="Đang lưu điểm..." fullScreen />}
      
      <Container maxWidth="xl" sx={{ py: 4 }}>
          {/* Breadcrumb */}
          <Box sx={{ mb: 3 }}>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            <Box 
              component="span" 
              sx={{ cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }} 
              onClick={() => navigate(`/teacher/assignment/${assignment.assignmentId}`)}
            >
              Chấm bài: {classData?.className} ({classData?.classCode})
            </Box>
            {' / '}
            <Box 
              component="span" 
              sx={{ cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }} 
              onClick={() => navigate(`/teacher/assignment/${assignment.assignmentId}`)}
            >
              {assignment.title}
            </Box>
            {' / '}
            <Box 
              component="span" 
              sx={{ cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }} 
              onClick={() => navigate(`/teacher/grading/${assignment.assignmentId}/${problem.problemId}`)}
            >
              {problem.title}
            </Box>
            {' / '}
            Chi tiết bài nộp
          </Typography>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h5" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
              Chi tiết bài nộp - {submission.userFullName}
            </Typography>
            <Button
              variant="outlined"
              startIcon={<ArrowBackIcon />}
              onClick={() => navigate(`/teacher/grading/${assignment.assignmentId}/${problem.problemId}`)}
              sx={{ borderColor: 'secondary.main', color: 'secondary.main' }}
            >
              Quay lại danh sách
            </Button>
          </Box>
        </Box>

        <Box sx={{ display: 'flex', gap: 3, flexDirection: { xs: 'column', lg: 'row' } }}>
          {/* Left Column: Problem & Code */}
          <Box sx={{ flex: '2' }}>
            {/* Student Info Card */}
            <Paper sx={{ p: 2, mb: 3, bgcolor: 'secondary.main' }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                  <PersonIcon sx={{ color: 'primary.main', fontSize: 40 }} />
                  <Box>
                    <Typography variant="h6" sx={{ color: 'primary.main', fontWeight: 'bold' }}>
                      {submission.userFullName}
                    </Typography>
                    <Typography variant="body2" sx={{ color: 'primary.main' }}>
                      MSSV: {submission.userCode}
                    </Typography>
                  </Box>
                </Box>
                <Box sx={{ textAlign: 'right' }}>
                  <Typography variant="caption" sx={{ color: 'primary.main', display: 'block' }}>
                    Nộp lúc
                  </Typography>
                  <Typography variant="body2" sx={{ color: 'primary.main', fontWeight: 'bold' }}>
                    {new Date(submission.submittedAt).toLocaleString('vi-VN')}
                  </Typography>
                </Box>
              </Box>
            </Paper>

            {/* Problem Description */}
            <Paper sx={{ p: 3, mb: 3 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <AssignmentIcon sx={{ color: 'secondary.main' }} />
                <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
                  {problem.title}
                </Typography>
                <Chip label={problem.difficulty} size="small" color="warning" />
              </Box>
              <Divider sx={{ my: 2 }} />
              <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mb: 1 }}>
                Mô tả:
              </Typography>
              <Typography variant="body2" color="text.secondary" paragraph>
                {problem.statement || 'Không có mô tả'}
              </Typography>
              {problem.inputFormat && (
                <>
                  <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mt: 2, mb: 1 }}>
                    Input:
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    {problem.inputFormat}
                  </Typography>
                </>
              )}
              {problem.outputFormat && (
                <>
                  <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mt: 2, mb: 1 }}>
                    Output:
                  </Typography>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    {problem.outputFormat}
                  </Typography>
                </>
              )}
            </Paper>

            {/* Source Code */}
            <Paper sx={{ p: 3, mb: 3 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <CodeIcon sx={{ color: 'secondary.main' }} />
                <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
                  Code của sinh viên
                </Typography>
                <Chip 
                  label={submission.languageCode.toUpperCase()} 
                  size="small" 
                  color="primary" 
                />
              </Box>
              <Box
                sx={{
                  maxHeight: '600px',
                  overflow: 'auto',
                  borderRadius: 1,
                  border: '1px solid',
                  borderColor: 'divider',
                }}
              >
                <SyntaxHighlighter
                  language={submission.languageCode === 'cpp' ? 'cpp' : submission.languageCode}
                  style={vs}
                  customStyle={{
                    margin: 0,
                    fontSize: '14px',
                    borderRadius: '4px',
                  }}
                  showLineNumbers
                >
                  {submission.sourceCode || '// No source code available'}
                </SyntaxHighlighter>
              </Box>
            </Paper>
          </Box>

          {/* Right Column: Test Results & Grading */}
          <Box sx={{ flex: '1' }}>
            {/* Test Results */}
            <Card
              sx={{
                mb: 3,
                bgcolor:
                  submission.status === 'Passed'
                    ? 'success.light'
                    : submission.status === 'Failed'
                    ? 'error.light'
                    : 'warning.light',
              }}
            >
              <CardContent>
                <Typography variant="h6" sx={{ fontWeight: 'bold', mb: 2 }}>
                  Kết quả chạy test
                </Typography>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      Trạng thái
                    </Typography>
                    <Box sx={{ mt: 0.5 }}>
                      <Chip
                        label={submission.status}
                        color={getStatusColor(submission.status)}
                        icon={isPassed ? <CheckCircleIcon /> : <CancelIcon />}
                      />
                    </Box>
                  </Box>

                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      Test cases
                    </Typography>
                    <Typography variant="h5" sx={{ fontWeight: 'bold', mt: 0.5 }}>
                      {submission.passedTestcase}/{submission.totalTestcase}
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
                      <Chip
                        label={`${submission.passedTestcase} Đạt`}
                        size="small"
                        color="success"
                      />
                      <Chip
                        label={`${submission.totalTestcase - submission.passedTestcase} Lỗi`}
                        size="small"
                        color="error"
                      />
                    </Box>
                  </Box>

                  {submission.totalTime && (
                    <Box>
                      <Typography variant="caption" color="text.secondary">
                        Thời gian thực thi
                      </Typography>
                      <Typography variant="body1" sx={{ fontWeight: 'bold', mt: 0.5 }}>
                        {submission.totalTime}ms
                      </Typography>
                    </Box>
                  )}

                  {submission.totalMemory && (
                    <Box>
                      <Typography variant="caption" color="text.secondary">
                        Bộ nhớ sử dụng
                      </Typography>
                      <Typography variant="body1" sx={{ fontWeight: 'bold', mt: 0.5 }}>
                        {submission.totalMemory}KB
                      </Typography>
                    </Box>
                  )}

                  {submission.errorMessage && (
                    <Box>
                      <Typography variant="caption" color="error">
                        Lỗi
                      </Typography>
                      <Typography variant="body2" color="error" sx={{ mt: 0.5 }}>
                        {submission.errorMessage}
                      </Typography>
                    </Box>
                  )}
                </Box>
              </CardContent>
            </Card>

            {/* Grading Panel */}
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 3 }}>
                Chấm điểm
              </Typography>

              <TextField
                fullWidth
                label="Điểm"
                type="number"
                value={score}
                onChange={(e) => setScore(e.target.value)}
                placeholder="Nhập điểm (0-150)"
                inputProps={{ min: 0, max: 150, step: 0.5 }}
                helperText="Điểm tối đa: 150"
                sx={{ mb: 3 }}
              />

              <TextField
                fullWidth
                label="Nhận xét"
                multiline
                rows={8}
                value={feedback}
                onChange={(e) => setFeedback(e.target.value)}
                placeholder="Viết nhận xét cho sinh viên..."
                sx={{ mb: 3 }}
              />

              <Divider sx={{ my: 2 }} />

              <Box sx={{ mb: 3 }}>
                <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 'bold' }}>
                  Thông tin bài nộp:
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                  Ngôn ngữ: {submission.languageCode.toUpperCase()}
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                  Điểm hệ thống: {submission.score || 'Chưa có'}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Tỷ lệ đạt: {((submission.passedTestcase / submission.totalTestcase) * 100).toFixed(1)}%
                </Typography>
              </Box>

              {error && (
                <Typography variant="caption" color="error" sx={{ display: 'block', mb: 2 }}>
                  {error}
                </Typography>
              )}

              <Button
                fullWidth
                variant="contained"
                startIcon={<SaveIcon />}
                onClick={handleSaveGrade}
                disabled={isSaving}
                sx={{
                  bgcolor: 'secondary.main',
                  color: 'primary.main',
                  py: 1.5,
                  '&:hover': {
                    bgcolor: 'primary.main',
                    color: 'secondary.main',
                  },
                }}
              >
                {isSaving ? 'Đang lưu...' : 'Lưu điểm và nhận xét'}
              </Button>
            </Paper>
          </Box>
        </Box>
      </Container>
    </Box>
  )
}
