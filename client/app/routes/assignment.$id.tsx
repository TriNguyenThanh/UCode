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
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import CodeIcon from '@mui/icons-material/Code'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import { mockAssignments } from '~/data/mock'

export const meta: Route.MetaFunction = () => [
  { title: 'Bài tập | UCode' },
  { name: 'description', content: 'Chi tiết bài tập và danh sách câu hỏi.' },
]

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')

  const assignment = mockAssignments.find((a) => a.id === params.id)
  if (!assignment) throw new Response('Not Found', { status: 404 })

  return { user, assignment }
}

export default function AssignmentDetail() {
  const { assignment } = useLoaderData<typeof clientLoader>()

  const getDaysUntilDue = (dueDate: Date) => {
    const now = new Date()
    const diff = dueDate.getTime() - now.getTime()
    return Math.ceil(diff / (1000 * 60 * 60 * 24))
  }

  const daysLeft = getDaysUntilDue(assignment.dueDate)
  const progress = 0 // TODO: Calculate from submissions

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

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Back Button */}
        <IconButton component={Link} to={`/class/${assignment.classId}`} sx={{ mb: 2 }}>
          <ArrowBackIcon />
        </IconButton>

        {/* Assignment Header */}
        <Paper
          sx={{
            mb: 4,
            overflow: 'hidden',
            background: 'linear-gradient(135deg, #FACB01 0%, #ffd54f 100%)',
          }}
        >
          <Box sx={{ p: 4 }}>
            <Chip
              label={assignment.className}
              sx={{
                mb: 2,
                bgcolor: 'secondary.main',
                color: 'primary.main',
                fontWeight: 600,
              }}
            />
            <Typography variant='h3' sx={{ fontWeight: 700, color: 'secondary.main', mb: 2 }}>
              {assignment.title}
            </Typography>
            <Typography variant='body1' sx={{ color: 'secondary.main', mb: 3, opacity: 0.9 }}>
              {assignment.description}
            </Typography>

            {/* Stats Row */}
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap', mb: 3 }}>
              <Chip
                icon={<AccessTimeIcon />}
                label={daysLeft > 0 ? `Còn ${daysLeft} ngày` : 'Quá hạn'}
                color={daysLeft > 2 ? 'info' : 'error'}
                sx={{ bgcolor: 'secondary.main', color: 'primary.main' }}
              />
              <Chip
                label={`${assignment.problems.length} câu hỏi`}
                variant='outlined'
                sx={{ borderColor: 'secondary.main', color: 'secondary.main', bgcolor: 'rgba(255,255,255,0.7)' }}
              />
              <Chip
                label={`${assignment.totalPoints} điểm`}
                variant='outlined'
                sx={{ borderColor: 'secondary.main', color: 'secondary.main', bgcolor: 'rgba(255,255,255,0.7)' }}
              />
              <Chip
                label={`Hạn: ${new Date(assignment.dueDate).toLocaleString('vi-VN')}`}
                variant='outlined'
                sx={{ borderColor: 'secondary.main', color: 'secondary.main', bgcolor: 'rgba(255,255,255,0.7)' }}
              />
            </Box>

            {/* Progress */}
            <Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                <Typography variant='body2' sx={{ fontWeight: 600, color: 'secondary.main' }}>
                  Tiến độ
                </Typography>
                <Typography variant='body2' sx={{ fontWeight: 600, color: 'secondary.main' }}>
                  {progress}% ({Math.floor((progress / 100) * assignment.problems.length)}/{assignment.problems.length})
                </Typography>
              </Box>
              <LinearProgress
                variant='determinate'
                value={progress}
                sx={{
                  height: 8,
                  borderRadius: 4,
                  bgcolor: 'rgba(0, 39, 94, 0.2)',
                  '& .MuiLinearProgress-bar': {
                    bgcolor: 'secondary.main',
                  },
                }}
              />
            </Box>
          </Box>
        </Paper>

        {/* Problems List */}
        <Box>
          <Typography variant='h5' sx={{ fontWeight: 600, mb: 3, display: 'flex', alignItems: 'center', gap: 1 }}>
            <CodeIcon sx={{ color: 'primary.main' }} />
            Danh sách câu hỏi
          </Typography>

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {assignment.problems.map((problem, index) => (
              <Card
                key={problem.id}
                elevation={0}
                sx={{
                  border: '2px solid',
                  borderColor: 'divider',
                  transition: 'all 0.2s',
                  '&:hover': {
                    borderColor: 'primary.main',
                    transform: 'translateY(-2px)',
                    boxShadow: 3,
                  },
                }}
              >
                <CardActionArea component={Link} to={`/problem/${problem.id}`}>
                  <CardContent sx={{ p: 3 }}>
                    <Box sx={{ display: 'flex', gap: 3 }}>
                      {/* Number Badge */}
                      <Box
                        sx={{
                          width: 50,
                          height: 50,
                          borderRadius: '50%',
                          bgcolor: 'primary.main',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          flexShrink: 0,
                        }}
                      >
                        <Typography variant='h6' sx={{ fontWeight: 700, color: 'secondary.main' }}>
                          {index + 1}
                        </Typography>
                      </Box>

                      {/* Content */}
                      <Box sx={{ flexGrow: 1 }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                          <Typography variant='h6' sx={{ fontWeight: 600 }}>
                            {problem.title}
                          </Typography>
                          <Chip
                            label={problem.difficulty}
                            size='small'
                            color={getDifficultyColor(problem.difficulty) as any}
                          />
                        </Box>

                        <Typography variant='body2' color='text.secondary' sx={{ mb: 2 }}>
                          {problem.description}
                        </Typography>

                        {/* Tags */}
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                          <Chip label={problem.category} size='small' variant='outlined' />
                          {problem.tags.map((tag) => (
                            <Chip
                              key={tag}
                              label={tag}
                              size='small'
                              variant='outlined'
                              sx={{ borderStyle: 'dashed' }}
                            />
                          ))}
                          <Chip
                            label={`${problem.timeLimit}s / ${problem.memoryLimit}MB`}
                            size='small'
                            icon={<AccessTimeIcon />}
                          />
                        </Box>
                      </Box>
                    </Box>
                  </CardContent>
                </CardActionArea>
              </Card>
            ))}
          </Box>
        </Box>
      </Container>
    </Box>
  )
}
