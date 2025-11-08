import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
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
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import CodeIcon from '@mui/icons-material/Code'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import { getMyAssignmentDetail, getAssignment } from '~/services/assignmentService'
import { getProblemForStudent } from '~/services/problemService'
import type { Assignment, AssignmentUser, Problem } from '~/types'

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

    const problems: Problem[] = []
    const problemStatuses = new Map<string, boolean>() // Track completion status

    if (assignment.problems && assignment.problems.length > 0) {
      for (const problemDetail of assignment.problems) {
        try {
          const problem = await getProblemForStudent(problemDetail.problemId)
          problems.push(problem)
          
          // Note: To check completion status, we would need to fetch submissions for each problem
          // For now, we'll just mark all as not completed unless we have submission data
          // TODO: Implement submission checking via API endpoint
          problemStatuses.set(problem.problemId, false)
        } catch (error) {
          console.error(`Failed to load problem ${problemDetail.problemId}:`, error)
        }
      }
    }

    return { user, assignment, assignmentUser, problems, problemStatuses: Object.fromEntries(problemStatuses) }
  } catch (error) {
    console.error('Error loading assignment:', error)
    throw new Response('Failed to load assignment', { status: 500 })
  }
}

export default function AssignmentDetail() {
  const { assignment, assignmentUser, problems, problemStatuses } = useLoaderData<typeof clientLoader>()

  const getDaysUntilDue = (endTime?: string) => {
    if (!endTime) return null
    const now = new Date()
    const dueDate = new Date(endTime)
    const diff = dueDate.getTime() - now.getTime()
    return Math.ceil(diff / (1000 * 60 * 60 * 24))
  }

  const daysLeft = getDaysUntilDue(assignment.endTime)
  
  const totalProblems = assignment.totalProblems || problems.length
  const completedProblems = Object.values(problemStatuses || {}).filter(Boolean).length
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
                  label={`Điểm của bạn: ${assignmentUser.score}/${assignment.totalPoints}`}
                  sx={{ 
                    bgcolor: '#34C759',
                    color: '#ffffff',
                    fontWeight: 600
                  }}
                />
              )}
              {assignment.endTime && (
                <Chip
                  label={`Hạn: ${new Date(assignment.endTime).toLocaleString('vi-VN')}`}
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
                const isCompleted = problemStatuses?.[problem.problemId]
                
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
                    
                    <CardActionArea component={Link} to={`/problem/${problem.problemId}`}>
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
                                {problem.title}
                              </Typography>
                              <Chip
                                label={problem.difficulty}
                                size='small'
                                color={getDifficultyColor(problem.difficulty) as any}
                              />
                              {isCompleted && (
                                <Chip
                                  label='Đã hoàn thành'
                                  size='small'
                                  sx={{ bgcolor: '#34C759', color: '#ffffff' }}
                                />
                              )}
                            </Box>

                            {problem.statement && (
                              <Typography variant='body2' color='text.secondary' sx={{ mb: 2 }}>
                                {problem.statement.substring(0, 150)}
                                {problem.statement.length > 150 ? '...' : ''}
                              </Typography>
                            )}

                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                              {problem.tagNames?.map((tag: string) => (
                                <Chip
                                  key={tag}
                                  label={tag}
                                  size='small'
                                  variant='outlined'
                                  sx={{ borderStyle: 'dashed' }}
                                />
                              ))}
                              <Chip
                                label={`${Math.round(problem.timeLimitMs / 1000)}s / ${Math.round(problem.memoryLimitKb / 1024)}MB`}
                                size='small'
                                icon={<AccessTimeIcon />}
                              />
                            </Box>
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
    </Box>
  )
}
