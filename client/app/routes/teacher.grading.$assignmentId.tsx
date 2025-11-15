import { useState, useEffect } from 'react'
import { redirect, useLoaderData, useNavigate } from 'react-router'
import type { Route } from './+types/teacher.grading.$assignmentId'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import { Loading } from '~/components/Loading'
import {
  getAssignment,
  getAssignmentStudents,
  gradeSubmission,
} from '~/services/assignmentService'
import { getBestSubmissionForStudent } from '~/services/submissionService'
import { getProblem } from '~/services/problemService'
import { getClassStudents } from '~/services/classService'
import type {
  Assignment,
  AssignmentUser,
  BestSubmission,
  Problem,
  Class,
  StudentResponse,
} from '~/types'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism'
import {
  Box,
  Container,
  Typography,
  Button,
  Paper,
  Chip,
  IconButton,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Divider,
  Card,
  CardContent,
} from '@mui/material'
import NavigateBeforeIcon from '@mui/icons-material/NavigateBefore'
import NavigateNextIcon from '@mui/icons-material/NavigateNext'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'
import SaveIcon from '@mui/icons-material/Save'

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  if (!params.assignmentId) {
    throw new Response('Assignment ID is required', { status: 400 })
  }

  try {
    // Fetch assignment and assignment students in parallel
    const [assignment, assignmentStudents] = await Promise.all([
      getAssignment(params.assignmentId),
      getAssignmentStudents(params.assignmentId),
    ])

    // Fetch class students to get full info (MSSV, name)
    const classStudents = await getClassStudents(assignment.classId)

    // Fetch problems in parallel
    const problems = await Promise.all(
      (assignment.problems || []).map((p) => getProblem(p.problemId)),
    )

    return {
      user,
      assignment,
      problems,
      assignmentStudents,
      classStudents,
    }
  } catch (error: any) {
    console.error('Failed to load grading data:', error)
    throw new Response(error.message || 'Không thể tải dữ liệu chấm bài', { status: 404 })
  }
}

export default function TeacherGrading() {
  const { assignment, problems, assignmentStudents, classStudents } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const [selectedStudentId, setSelectedStudentId] = useState<string | null>(
    assignmentStudents.length > 0 ? assignmentStudents[0].userId : null
  )
  const [currentProblemIndex, setCurrentProblemIndex] = useState(0)
  const [score, setScore] = useState<number | string>('')
  const [feedback, setFeedback] = useState('')
  const [currentSubmission, setCurrentSubmission] = useState<BestSubmission | null>(null)
  const [sourceCode, setSourceCode] = useState<string>('')
  const [isLoading, setIsLoading] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Create map for quick lookup
  const studentMap = new Map(classStudents.map(s => [s.userId, s]))
  
  const currentStudent = assignmentStudents.find(s => s.userId === selectedStudentId)
  const currentStudentInfo = selectedStudentId ? studentMap.get(selectedStudentId) : null
  const currentProblem = problems[currentProblemIndex]

  // Load best submission for selected student and current problem
  useEffect(() => {
    async function loadBestSubmission() {
      if (!selectedStudentId || !assignment.assignmentId || !currentProblem?.problemId) return

      setIsLoading(true)
      setError(null)
      try {
        const submission = await getBestSubmissionForStudent(
          assignment.assignmentId, 
          currentProblem.problemId, 
          selectedStudentId
        )
        setCurrentSubmission(submission)
      } catch (err: any) {
        console.error(`Failed to load submission for problem ${currentProblem.problemId}:`, err)
        setCurrentSubmission(null)
        setError(err.message || 'Không thể tải bài làm của sinh viên')
      } finally {
        setIsLoading(false)
      }
    }

    loadBestSubmission()
  }, [selectedStudentId, currentProblemIndex, assignment.assignmentId, currentProblem?.problemId])

  // Load source code when submission changes
  useEffect(() => {
    async function loadSourceCode() {
      if (!currentSubmission?.submissionId) {
        setSourceCode('')
        return
      }

      setIsLoading(true)
      try {
        // If solutionCode is already in submission, use it
        if (currentSubmission.sourceCode) {
          setSourceCode(currentSubmission.sourceCode)
        } else {
          // Otherwise, try to download from file service (if sourceCodeRef exists)
          setSourceCode('// Source code not available')
        }
      } catch (err: any) {
        console.error('Failed to load source code:', err)
        setSourceCode('// Failed to load source code')
      } finally {
        setIsLoading(false)
      }
    }

    loadSourceCode()
    // Initialize score and feedback from current submission
    if (currentSubmission) {
      setScore(currentSubmission.score ?? '')
      // setFeedback(currentSubmission.teacherFeedback || '')
    } else {
      setScore('')
      setFeedback('')
    }
  }, [currentSubmission])

  const handleSaveGrade = async () => {
    if (!currentSubmission?.submissionId) {
      alert('Không tìm thấy bài làm để chấm điểm')
      return
    }

    setIsSaving(true)
    setError(null)
    try {
      await gradeSubmission(assignment.assignmentId, currentSubmission.submissionId, {
        score: typeof score === 'number' ? score : parseFloat(score as string) || undefined,
        teacherFeedback: feedback || undefined,
      })
      alert(`Đã lưu điểm cho ${currentStudentInfo?.fullName || 'sinh viên'}`)
      // Reload best submission for current problem
      if (selectedStudentId) {
        const updatedSub = await getBestSubmissionForStudent(
          assignment.assignmentId,
          currentProblem.problemId,
          selectedStudentId,
        )
        setCurrentSubmission(updatedSub)
      }
    } catch (err: any) {
      console.error('Failed to save grade:', err)
      setError(err.message || 'Không thể lưu điểm. API có thể chưa sẵn sàng.')
    } finally {
      setIsSaving(false)
    }
  }

  // Show loading while fetching data initially
  if (isLoading && !currentSubmission && selectedStudentId) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
        <Navigation />
        <Loading message="Đang tải bài làm của sinh viên..." fullScreen />
      </Box>
    )
  }

  // Check if there are any students
  if (assignmentStudents.length === 0) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
        <Navigation />
        <Container maxWidth="md" sx={{ py: 4, textAlign: 'center' }}>
          <Typography variant="h5" color="text.secondary">
            Chưa có sinh viên nào trong bài tập này
          </Typography>
          <Button
            variant="contained"
            onClick={() => navigate(`/teacher/assignment/${assignment.assignmentId}`)}
            sx={{ mt: 3, bgcolor: 'secondary.main', color: 'primary.main' }}
          >
            Quay lại
          </Button>
        </Container>
      </Box>
    )
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50', pb: 4 }}>
      <Navigation />
      {isSaving && <Loading message="Đang lưu điểm..." fullScreen />}
      
      <Container maxWidth="xl" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 3 }}>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            Chấm bài: {assignment.title}
          </Typography>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h5" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
              Chấm bài cho sinh viên
            </Typography>
            
            {/* Student Selection Combobox */}
            <FormControl sx={{ minWidth: 350 }}>
              <InputLabel>Chọn sinh viên</InputLabel>
              <Select
                value={selectedStudentId || ''}
                onChange={(e) => setSelectedStudentId(e.target.value)}
                label="Chọn sinh viên"
                disabled={isSaving}
              >
                {assignmentStudents.map((student) => {
                  const studentInfo = studentMap.get(student.userId)
                  
                  return (
                    <MenuItem key={student.userId} value={student.userId}>
                      <Box>
                        <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                          {studentInfo?.fullName || 'N/A'}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          MSSV: {studentInfo?.studentCode || 'N/A'}
                        </Typography>
                      </Box>
                    </MenuItem>
                  )
                })}
              </Select>
            </FormControl>
          </Box>
        </Box>

        {/* Student Info and Problems */}
        <Paper sx={{ p: 2, mb: 3, bgcolor: 'secondary.main' }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Box>
              <Typography variant="h6" sx={{ color: 'primary.main', fontWeight: 'bold' }}>
                {currentStudentInfo?.fullName || 'Sinh viên'}
              </Typography>
              <Typography variant="body2" sx={{ color: 'primary.main' }}>
                MSSV: {currentStudentInfo?.studentCode || 'N/A'}
                {currentSubmission?.submittedAt && (
                  <>
                    {' • Nộp lúc: '}
                    {new Date(currentSubmission.submittedAt).toLocaleString('vi-VN')}
                  </>
                )}
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              {problems.map((problem, idx) => {
                const submission = bestSubmissions.find((s) => s.problemId === problem.problemId)
                const isPassed = submission && submission.passedTestcase === submission.totalTestcase
                const hasSubmission = !!submission
                
                return (
                  <Chip
                    key={problem.problemId}
                    label={`Bài ${idx + 1}`}
                    onClick={() => setCurrentProblemIndex(idx)}
                    color={
                      isPassed
                        ? 'success'
                        : hasSubmission
                        ? 'warning'
                        : 'default'
                    }
                    sx={{
                      fontWeight: isCurrentProblem ? 'bold' : 'normal',
                      cursor: 'pointer',
                      color: 'white',
                      border: '2px solid',
                      borderColor: 'white',
                      ...(isCurrentProblem && {
                        border: '2px solid',
                        borderColor: 'white',
                        boxShadow: '0 0 0 2px',
                        boxShadowColor: isPassed ? 'success.main' : hasSubmission ? 'warning.main' : 'grey.500',
                        transform: 'scale(1.1)',
                      }),
                    }}
                  />
                )
              })}
            </Box>
          </Box>
        </Paper>

      <Box sx={{ display: 'flex', gap: 3 }}>
        {/* Left: Problem Description */}
        <Box sx={{ flex: '0 0 400px', display: 'flex', flexDirection: 'column' }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 2 }}>
              {currentProblem?.title}
            </Typography>
            <Box sx={{ mb: 2 }}>
              <Chip label={currentProblem?.difficulty} size="small" color="warning" sx={{ mr: 1 }} />
              {currentSubmission && (
                <Chip
                  icon={
                    currentSubmission.passedTestcase === currentSubmission.totalTestcase
                      ? <CheckCircleIcon />
                      : <CancelIcon />
                  }
                  label={`${currentSubmission.passedTestcase || 0}/${currentSubmission.totalTestcase || 0} test cases`}
                  size="small"
                  color={
                    currentSubmission.passedTestcase === currentSubmission.totalTestcase
                      ? 'success'
                      : 'warning'
                  }
                />
              )}
            </Box>
            <Divider sx={{ my: 2 }} />
            <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mb: 1 }}>
              Mô tả:
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              {currentProblem?.statement || 'Không có mô tả'}
            </Typography>
            {currentProblem?.inputFormat && (
              <>
                <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mt: 2, mb: 1 }}>
                  Input:
                </Typography>
                <Typography variant="body2" color="text.secondary" paragraph>
                  {currentProblem.inputFormat}
                </Typography>
              </>
            )}
            {currentProblem?.outputFormat && (
              <>
                <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mt: 2, mb: 1 }}>
                  Output:
                </Typography>
                <Typography variant="body2" color="text.secondary" paragraph>
                  {currentProblem.outputFormat}
                </Typography>
              </>
            )}
          </Paper>
        </Box>

        {/* Middle: Student Code */}
        <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
          {currentSubmission ? (
            <>
              <Paper sx={{ p: 2, mb: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="subtitle1" sx={{ fontWeight: 'bold' }}>
                    Code của sinh viên
                  </Typography>
                  <Chip label={currentSubmission.status} size="small" color="primary" />
                </Box>
                {sourceCode ? (
                  <Box
                    sx={{
                      maxHeight: '500px',
                      overflow: 'auto',
                      borderRadius: 1,
                    }}
                  >
                    <SyntaxHighlighter
                      language="python"
                      style={vscDarkPlus}
                      customStyle={{
                        margin: 0,
                        fontSize: '14px',
                      }}
                    >
                      {sourceCode}
                    </SyntaxHighlighter>
                  </Box>
                ) : (
                  <Typography variant="body2" color="text.secondary">
                    Đang tải code...
                  </Typography>
                )}
              </Paper>

              {/* Test Results */}
              <Card
                sx={{
                  bgcolor:
                    currentSubmission.status === 'Passed'
                      ? 'success.light'
                      : currentSubmission.status === 'Failed'
                      ? 'error.light'
                      : 'warning.light',
                }}
              >
                <CardContent>
                  <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mb: 1 }}>
                    Kết quả chạy test:
                  </Typography>
                  <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', mb: 1 }}>
                    {currentSubmission.status === 'Passed' ? (
                      <CheckCircleIcon color="success" />
                    ) : (
                      <CancelIcon color="error" />
                    )}
                    <Typography variant="body2">
                      {currentSubmission.passedTestcase || 0}/{currentSubmission.totalTestcase || 0} test cases passed
                    </Typography>
                  </Box>
                  {currentSubmission.totalTime && (
                    <Typography variant="body2" color="text.secondary">
                      Thời gian: {currentSubmission.totalTime}ms
                    </Typography>
                  )}
                  {currentSubmission.totalMemory && (
                    <Typography variant="body2" color="text.secondary">
                      Bộ nhớ: {currentSubmission.totalMemory}KB
                    </Typography>
                  )}
                </CardContent>
              </Card>
            </>
          ) : (
            <Paper sx={{ p: 3, textAlign: 'center' }}>
              <Typography variant="body1" color="text.secondary">
                Sinh viên chưa nộp bài cho câu hỏi này
              </Typography>
            </Paper>
          )}
        </Box>

        {/* Right: Grading Panel */}
        <Box sx={{ flex: '0 0 350px', display: 'flex', flexDirection: 'column' }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 3 }}>
              Chấm điểm
            </Typography>

            <TextField
              fullWidth
              label="Điểm cho bài này"
              type="number"
              value={score}
              onChange={(e) => setScore(e.target.value)}
              placeholder="Nhập điểm (0-100)"
              // inputProps={{ min: 0, max: currentSubmission?.maxScore || 100 }}
                // helperText={`Điểm tối đa: ${currentSubmission?.maxScore || 100}`}
              inputProps={{ min: 0, max: 150 }}
              helperText={`Điểm tối đa: 150`}
              sx={{ mb: 3 }}
              disabled={!currentSubmission || isSaving}
            />

            <TextField
              fullWidth
              label="Nhận xét"
              multiline
              rows={6}
              value={feedback}
              onChange={(e) => setFeedback(e.target.value)}
              placeholder="Viết nhận xét cho sinh viên..."
              sx={{ mb: 3 }}
              disabled={!currentSubmission || isSaving}
            />

            <Divider sx={{ my: 2 }} />

            <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 'bold' }}>
              Bài hiện tại:
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mb: 3 }}>
              {problems.map((problem, idx) => {
                const submission = bestSubmissions.find((s) => s.problemId === problem.problemId)
                return (
                  <Box key={problem.problemId} sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="body2">Bài {idx + 1}:</Typography>
                    {submission ? (
                      <Chip
                        size="small"
                        label={`${submission.passedTestcase || 0}/${submission.totalTestcase || 0}`}
                        color={
                          submission.passedTestcase === submission.totalTestcase
                            ? 'success'
                            : 'warning'
                        }
                      />
                    ) : (
                      <Chip size="small" label="Chưa nộp" color="default" />
                    )}
                  </Box>
                )
              })}
            </Box>

            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Tổng số lần nộp: {currentSubmission?.totalSubmission || 0}
            </Typography>

            <Button
              fullWidth
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSaveGrade}
              disabled={!currentSubmission || isSaving}
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

            {error && (
              <Typography variant="caption" color="error" sx={{ mt: 1, display: 'block' }}>
                {error}
              </Typography>
            )}
          </Paper>
        </Box>
      </Box>
      </Container>
    </Box>
  )
}
