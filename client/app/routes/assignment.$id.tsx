import * as React from 'react'
import { useLoaderData, redirect, Link, useNavigation } from 'react-router'
import type { Route } from './+types/assignment.$id'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  CardActionArea,
  Chip,
  Paper,
  IconButton,
  LinearProgress,
  Alert,
  Skeleton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import CodeIcon from '@mui/icons-material/Code'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import { getMyAssignmentDetail, getAssignment, startAssignment } from '~/services/assignmentService'
import { getListBestSubmissions } from '~/services/submissionService'
import type { Assignment, AssignmentUser, BestSubmission, Problem } from '~/types'
import { Loading } from '~/components/Loading'
import { useNavigate } from 'react-router'
import { formatDateTime, getDaysUntil } from '~/utils/dateUtils'
import { useExamMonitoring } from '~/utils/useExamMonitoring'

export const meta: Route.MetaFunction = () => [
  { title: 'Bài tập | UCode' },
  { name: 'description', content: 'Chi tiết bài tập và danh sách câu hỏi.' },
]

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')

  if (!params.id) throw new Response('Assignment ID is required', { status: 400 })

  try {
    let assignment: Assignment
    let assignmentUser: AssignmentUser | undefined

    if (user.role === 'student') {
      assignmentUser = await getMyAssignmentDetail(params.id)
      assignment = await getAssignment(assignmentUser.assignmentId)
    } else {
      assignment = await getAssignment(params.id)
    }

    const problems = assignment.problems || []
    const problemSubmissions = new Map<string, BestSubmission | null>()

    if (problems.length > 0) {
      // Initialize submission map
      problems.forEach((problem) => {
        problemSubmissions.set(problem.problemId, null)
      })

      // Fetch best submissions for all problems in one API call
      try {
        const problemIds = problems.map(p => p.problemId)
        const submissions = await getListBestSubmissions(
          assignment.assignmentId,
          problemIds
        )
        
        // Map submissions to their corresponding problems using problemId
        submissions.forEach((submission) => {
          if (submission.problemId) {
            problemSubmissions.set(submission.problemId, submission)
          }
        })
      } catch (error) {
        console.error('Error loading submissions:', error)
      }
    }

    return { 
      user, 
      assignment,
      assignmentUser,
      problems, 
      problemSubmissions: Object.fromEntries(problemSubmissions) 
    }
  } catch (error) {
    console.error('Error loading assignment:', error)
    throw new Response('Failed to load assignment', { status: 500 })
  }
}

export default function AssignmentDetail() {
  const { user, assignment, assignmentUser, problems, problemSubmissions } = useLoaderData<typeof clientLoader>()
  const navigation = useNavigation()
  const navigate = useNavigate()
  const isLoading = navigation.state === 'loading'
  
  const [startDialogOpen, setStartDialogOpen] = React.useState(false)
  const [selectedProblemId, setSelectedProblemId] = React.useState<string | null>(null)
  const [isStarting, setIsStarting] = React.useState(false)

  // Exam monitoring for EXAMINATION type assignments
  const isExamination = assignment.assignmentType === 'EXAMINATION'
  const isStudent = user.role === 'student'
  const hasStarted = assignmentUser?.status !== 'NOT_STARTED'
  
  const { startMonitoring } = useExamMonitoring({
    assignmentId: assignment.assignmentId,
    isExamination,
    enabled: isStudent && hasStarted
  })

  // Start monitoring when component mounts if assignment has already started
  React.useEffect(() => {
    if (isStudent && isExamination && hasStarted) {
      startMonitoring()
    }
  }, [isStudent, isExamination, hasStarted, startMonitoring])

  // Handle problem click - check if assignment is started
  const handleProblemClick = (e: React.MouseEvent, problemId: string) => {
    // Only check for students
    if (user.role !== 'student') return
    
    // Check if assignment has been started
    if (assignmentUser && assignmentUser.status === 'NOT_STARTED') {
      e.preventDefault()
      setSelectedProblemId(problemId)
      setStartDialogOpen(true)
    }
  }

  // Handle start assignment
  const handleStartAssignment = async () => {
    if (!selectedProblemId) return
    
    setIsStarting(true)
    try {
      await startAssignment(assignment.assignmentId)
      setStartDialogOpen(false)
      // Navigate to problem after starting
      navigate(`/student/assignment/${assignment.assignmentId}/problem/${selectedProblemId}`)
    } catch (error) {
      console.error('Failed to start assignment:', error)
      alert('Không thể bắt đầu bài tập. Vui lòng thử lại!')
    } finally {
      setIsStarting(false)
    }
  }

  const daysLeft = getDaysUntil(assignment.endTime)
  
  const totalProblems = assignment.totalProblems || problems.length
  // Count problems that have BestSubmission with Passed status
  const completedProblems = Object.values(problemSubmissions || {}).filter(
    (submission) => submission?.status === 'Passed'
  ).length
  const progress = totalProblems > 0 ? Math.round((completedProblems / totalProblems) * 100) : 0

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy':
        return 'success'
      case 'Medium':
        return 'warning'
      case 'Hard':
        return 'error'
      default:
        return 'default'
    }
  }

  const isOverdue = daysLeft !== null && daysLeft < 0

  // Show loading screen while navigation is in progress
  if (isLoading) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
        <Navigation />
        <Loading fullScreen message="Đang tải thông tin bài tập..." />
      </Box>
    )
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        <IconButton component={Link} to={`/class/${assignment.classId}`} sx={{ mb: 2 }}>
          <ArrowBackIcon />
        </IconButton>

        {/* Overdue Alert */}
        {isOverdue && (
          <Alert severity='error' sx={{ mb: 3 }}>
            Bài tập này đã quá hạn nộp!
          </Alert>
        )}

        <Paper
          sx={{
            mb: 4,
            overflow: 'hidden',
            bgcolor: '#ffffff',
            border: '1px solid #d2d2d7',
          }}
        >
          <Box sx={{ p: 4 }}>
            <Chip
              label={assignment.assignmentType}
              sx={{
                mb: 2,
                bgcolor: '#007AFF',
                color: '#ffffff',
                fontWeight: 600,
              }}
            />
            <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f', mb: 2 }}>
              {assignment.title}
            </Typography>
            {assignment.description && (
              <Typography variant='body1' sx={{ color: '#86868b', mb: 3 }}>
                {assignment.description}
              </Typography>
            )}

            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap', mb: 3 }}>
              {daysLeft !== null && (
                <Chip
                  icon={<AccessTimeIcon />}
                  label={daysLeft > 0 ? `Còn ${daysLeft} ngày` : `Quá hạn ${Math.abs(daysLeft)} ngày`}
                  sx={{ 
                    bgcolor: daysLeft > 2 ? '#007AFF' : '#FF3B30',
                    color: '#ffffff'
                  }}
                />
              )}
              <Chip
                label={`${totalProblems} câu hỏi`}
                variant='outlined'
                sx={{ borderColor: '#d2d2d7', color: '#1d1d1f' }}
              />
              {assignment.totalPoints && (
                <Chip
                  label={`Tổng: ${assignment.totalPoints} điểm`}
                  variant='outlined'
                  sx={{ borderColor: '#d2d2d7', color: '#1d1d1f' }}
                />
              )}
              {assignmentUser?.score !== undefined && (
                <Chip
                  label={`Điểm của bạn: ${assignmentUser?.score}/${assignment.totalPoints}`}
                  sx={{ 
                    bgcolor: '#34C759',
                    color: '#ffffff',
                    fontWeight: 600
                  }}
                />
              )}
              {assignment.endTime && (
                <Chip
                  label={`Hạn: ${formatDateTime(assignment.endTime, 'long')}`}
                  variant='outlined'
                  sx={{ borderColor: '#d2d2d7', color: '#1d1d1f' }}
                />
              )}
            </Box>

            <Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                <Typography variant='body2' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                  Tiến độ hoàn thành
                </Typography>
                <Typography variant='body2' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                  {progress}% ({completedProblems}/{totalProblems})
                </Typography>
              </Box>
              <LinearProgress
                variant='determinate'
                value={progress}
                sx={{
                  height: 8,
                  borderRadius: 4,
                  bgcolor: '#e5e5ea',
                  '& .MuiLinearProgress-bar': {
                    bgcolor: '#34C759',
                  },
                }}
              />
            </Box>
          </Box>
        </Paper>

        <Box>
          <Typography variant='h5' sx={{ fontWeight: 600, mb: 3, display: 'flex', alignItems: 'center', gap: 1, color: '#1d1d1f' }}>
            <CodeIcon sx={{ color: '#007AFF' }} />
            Danh sách câu hỏi
          </Typography>

          {problems.length === 0 ? (
            <Alert severity='info'>Chưa có câu hỏi nào trong bài tập này.</Alert>
          ) : (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {problems.map((problem, index) => {
                const submission = problemSubmissions?.[problem.problemId]
                const isCompleted = submission?.status === 'Passed'
                
                return (
                  <Card
                    key={problem.problemId}
                    elevation={0}
                    sx={{
                      bgcolor: '#ffffff',
                      border: '1px solid #d2d2d7',
                      borderRadius: 2,
                      transition: 'all 0.2s',
                      position: 'relative',
                      opacity: isCompleted ? 0.9 : 1,
                      '&:hover': {
                        borderColor: '#007AFF',
                        transform: 'translateY(-2px)',
                        boxShadow: '0 4px 12px rgba(0, 0, 0, 0.08)',
                      },
                    }}
                  >
                    {isCompleted && (
                      <Box
                        sx={{
                          position: 'absolute',
                          top: 16,
                          right: 16,
                          zIndex: 1,
                        }}
                      >
                        <CheckCircleIcon sx={{ color: '#34C759', fontSize: 32 }} />
                      </Box>
                    )}
                    
                    <CardActionArea 
                      component={Link} 
                      to={`/student/assignment/${assignment.assignmentId}/problem/${problem.problemId}`}
                      onClick={(e) => handleProblemClick(e, problem.problemId)}
                    >
                      <CardContent sx={{ p: 3 }}>
                        <Box sx={{ display: 'flex', gap: 3 }}>
                          <Box
                            sx={{
                              width: 50,
                              height: 50,
                              borderRadius: '50%',
                              bgcolor: isCompleted ? '#34C759' : '#007AFF',
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              flexShrink: 0,
                            }}
                          >
                            <Typography variant='h6' sx={{ fontWeight: 700, color: '#ffffff' }}>
                              {index + 1}
                            </Typography>
                          </Box>

                          <Box sx={{ flexGrow: 1 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1, flexWrap: 'wrap' }}>
                              <Typography variant='h6' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                                {problem.code} - {problem.title}
                              </Typography>
                              <Chip
                                label={problem.difficulty}
                                size='small'
                                color={getDifficultyColor(problem.difficulty) as any}
                              />
                              <Chip
                                label={`${problem.points} điểm`}
                                size='small'
                                sx={{ bgcolor: '#FF9500', color: '#ffffff' }}
                              />
                              {isCompleted && (
                                <Chip
                                  label='Đã hoàn thành'
                                  size='small'
                                  sx={{ bgcolor: '#34C759', color: '#ffffff' }}
                                />
                              )}
                            </Box>

                            {/* Submission Status */}
                            {submission && (
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2, flexWrap: 'wrap' }}>
                                <Typography 
                                  variant='body2' 
                                  sx={{ 
                                    fontWeight: 500 
                                  }}
                                >
                                  {/* {submission.status === 'Passed' && '✓ Đã qua'}
                                  {submission.status === 'Failed' && '✗ Chưa qua'}
                                  {submission.status === 'CompilationError' && '⚠ Lỗi biên dịch'}
                                  {submission.status === 'RuntimeError' && '⚠ Lỗi runtime'}
                                  {submission.status === 'TimeLimitExceeded' && '⏱ Vượt quá thời gian'}
                                  {submission.status === 'MemoryLimitExceeded' && '💾 Vượt quá bộ nhớ'}
                                  {submission.status === 'Pending' && '⏳ Đang chờ'}
                                  {submission.status === 'Running' && '▶ Đang chạy'} */}
                                </Typography>
                                {submission.passedTestcase !== undefined && submission.totalTestcase !== undefined && (
                                  <Chip
                                    label={`${submission.passedTestcase}/${submission.totalTestcase} testcases`}
                                    size='small'
                                    color={submission.passedTestcase === submission.totalTestcase ? 'success' : 'default'}
                                  />
                                )}
                                {submission.score !== undefined && submission.score !== null && (
                                  <Chip
                                    label={`Điểm: ${submission.score}/${problem.points}`}
                                    size='small'
                                    sx={{ 
                                      bgcolor: submission.status === 'Passed' ? '#34C759' : '#FF9500',
                                      color: '#ffffff',
                                      fontWeight: 600
                                    }}
                                  />
                                )}
                                {submission.totalSubmission !== undefined && submission.totalSubmission > 0 && (
                                  <Chip
                                    label={`${submission.totalSubmission} lần nộp`}
                                    size='small'
                                    variant='outlined'
                                    sx={{ borderColor: '#d2d2d7', color: '#1d1d1f' }}
                                  />
                                )}
                              </Box>
                            )}
                          </Box>
                        </Box>
                      </CardContent>
                    </CardActionArea>
                  </Card>
                )
              })}
            </Box>
          )}
        </Box>
      </Container>

      {/* Start Assignment Dialog */}
      <Dialog open={startDialogOpen} onClose={() => !isStarting && setStartDialogOpen(false)}>
        <DialogTitle sx={{ bgcolor: 'secondary.main', color: 'primary.main' }}>
          Bắt đầu bài tập
        </DialogTitle>
        <DialogContent sx={{ mt: 2 }}>
          <Typography variant="body1" gutterBottom>
            Bạn chưa bắt đầu bài tập này. Bạn có muốn bắt đầu làm bài tập ngay bây giờ không?
          </Typography>
          <Alert severity="info" sx={{ mt: 2 }}>
            Sau khi bắt đầu, thời gian làm bài sẽ được tính từ thời điểm này.
          </Alert>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setStartDialogOpen(false)} disabled={isStarting}>
            Hủy
          </Button>
          <Button
            variant="contained"
            onClick={handleStartAssignment}
            disabled={isStarting}
            sx={{
              bgcolor: 'secondary.main',
              color: 'primary.main',
              '&:hover': {
                bgcolor: 'primary.main',
                color: 'secondary.main',
              },
            }}
          >
            {isStarting ? 'Đang bắt đầu...' : 'Bắt đầu'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
