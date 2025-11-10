import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/class.$id'
import { auth } from '~/auth'
import * as ClassService from '~/services/classService'
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
  Divider,
  Avatar,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import AssignmentIcon from '@mui/icons-material/Assignment'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import PendingIcon from '@mui/icons-material/Pending'

export const meta: Route.MetaFunction = ({ params }) => [
  { title: `Lớp học | UCode` },
  { name: 'description', content: 'Chi tiết lớp học và danh sách bài tập.' },
]

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')

  try {
    // Student chỉ cần thông tin cơ bản của class, không cần detail với danh sách sinh viên
    const classData = await ClassService.getClassById(params.id)
    
    // TODO: assignments need to come from assignment-service
    // For now, assignments will be an empty array
    const assignments: any[] = []

    return { user, classData, assignments }
  } catch (error) {
    console.error('Error loading class detail:', error)
    throw new Response('Lớp học không tồn tại', { status: 404 })
  }
}

export default function ClassDetail() {
  const { classData, assignments } = useLoaderData<typeof clientLoader>()

  const getDaysUntilDue = (dueDate: Date) => {
    const now = new Date()
    const diff = dueDate.getTime() - now.getTime()
    return Math.ceil(diff / (1000 * 60 * 60 * 24))
  }

  const getStatusInfo = (assignment: typeof assignments[0]) => {
    const daysLeft = getDaysUntilDue(assignment.dueDate)
    if (daysLeft < 0) return { label: 'Quá hạn', color: 'error' as const, icon: <PendingIcon /> }
    if (daysLeft <= 2) return { label: `Còn ${daysLeft} ngày`, color: 'error' as const, icon: <AccessTimeIcon /> }
    if (daysLeft <= 7) return { label: `Còn ${daysLeft} ngày`, color: 'warning' as const, icon: <AccessTimeIcon /> }
    return { label: `Còn ${daysLeft} ngày`, color: 'info' as const, icon: <AccessTimeIcon /> }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Back Button */}
        <IconButton component={Link} to='/home' sx={{ mb: 2 }}>
          <ArrowBackIcon />
        </IconButton>

        {/* Class Header */}
        <Paper
          sx={{
            mb: 4,
            overflow: 'hidden',
            background: 'linear-gradient(135deg, #00275e 0%, #003d8f 100%)',
          }}
        >
          <Box sx={{ p: 4 }}>
            <Chip
              label={classData.classCode}
              sx={{
                mb: 2,
                bgcolor: 'primary.main',
                color: 'secondary.main',
                fontWeight: 700,
                fontSize: '0.9rem',
              }}
            />
            <Typography variant='h3' sx={{ fontWeight: 700, color: 'white', mb: 2 }}>
              {classData.className}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main' }}>
                  {classData.teacherName.charAt(0)}
                </Avatar>
                <Typography variant='body1' sx={{ color: 'primary.main', fontWeight: 500 }}>
                  {classData.teacherName}
                </Typography>
              </Box>
              <Chip
                label={classData.semester}
                sx={{ bgcolor: 'rgba(250, 203, 1, 0.1)', color: 'primary.main', borderColor: 'primary.main' }}
                variant='outlined'
              />
            </Box>
            {classData.description && (
              <Typography variant='body1' sx={{ mt: 2, color: 'rgba(255,255,255,0.8)' }}>
                {classData.description}
              </Typography>
            )}
          </Box>
        </Paper>

        {/* Assignments List */}
        <Box>
          <Typography variant='h5' sx={{ fontWeight: 600, mb: 3, display: 'flex', alignItems: 'center', gap: 1 }}>
            <AssignmentIcon sx={{ color: 'primary.main' }} />
            Danh sách bài tập ({assignments.length})
          </Typography>

          {assignments.length === 0 ? (
            <Paper sx={{ p: 4, textAlign: 'center' }}>
              <Typography color='text.secondary'>Chưa có bài tập nào được giao 📝</Typography>
            </Paper>
          ) : (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {assignments.map((assignment) => {
                const statusInfo = getStatusInfo(assignment)
                return (
                  <Card
                    key={assignment.id}
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
                    <CardActionArea component={Link} to={`/assignment/${assignment.id}`}>
                      <CardContent sx={{ p: 3 }}>
                        <Box sx={{ display: 'flex', gap: 3 }}>
                          {/* Icon */}
                          <Box
                            sx={{
                              width: 60,
                              height: 60,
                              borderRadius: 2,
                              bgcolor: 'secondary.main',
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              flexShrink: 0,
                            }}
                          >
                            <AssignmentIcon sx={{ color: 'primary.main', fontSize: 32 }} />
                          </Box>

                          {/* Content */}
                          <Box sx={{ flexGrow: 1 }}>
                            <Typography variant='h6' sx={{ fontWeight: 600, mb: 1 }}>
                              {assignment.title}
                            </Typography>
                            <Typography variant='body2' color='text.secondary' sx={{ mb: 2 }}>
                              {assignment.description}
                            </Typography>

                            {/* Stats */}
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
                              <Chip icon={statusInfo.icon} label={statusInfo.label} size='small' color={statusInfo.color} />
                              <Chip label={`${assignment.problems.length} bài`} size='small' variant='outlined' />
                              <Chip label={`${assignment.totalPoints} điểm`} size='small' variant='outlined' />
                              <Chip
                                label={`Bắt đầu: ${new Date(assignment.startDate).toLocaleDateString('vi-VN')}`}
                                size='small'
                                variant='outlined'
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
