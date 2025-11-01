import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/teacher.home'
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
  Button,
} from '@mui/material'
import ClassIcon from '@mui/icons-material/Class'
import PeopleIcon from '@mui/icons-material/People'
import AssignmentIcon from '@mui/icons-material/Assignment'
import GradingIcon from '@mui/icons-material/Grading'
import AddIcon from '@mui/icons-material/Add'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import { mockClasses, mockAssignments } from '~/data/mock'

export const meta: Route.MetaFunction = () => [
  { title: 'Giáo viên | UCode' },
  { name: 'description', content: 'Dashboard giáo viên.' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'teacher') throw redirect('/home')

  // Mock teacher stats
  const stats = {
    totalClasses: 3,
    totalStudents: 85,
    activeAssignments: 5,
    pendingGrading: 12,
  }

  return { user, classes: mockClasses, assignments: mockAssignments, stats }
}

export default function TeacherHome() {
  const { classes, assignments, stats } = useLoaderData<typeof clientLoader>()

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Welcome Section */}
        <Box sx={{ mb: 4 }}>
          <Typography variant='h4' sx={{ fontWeight: 700, mb: 1 }}>
            Dashboard Giáo viên 👨‍🏫
          </Typography>
          <Typography variant='body1' color='text.secondary'>
            Quản lý lớp học, bài tập và đánh giá sinh viên
          </Typography>
        </Box>

        {/* Stats Cards */}
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', lg: '1fr 1fr 1fr 1fr' }, gap: 3, mb: 4 }}>
          <Card elevation={0} sx={{ border: '2px solid', borderColor: 'primary.main', bgcolor: 'primary.main' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant='body2' sx={{ fontWeight: 600, color: 'secondary.main' }}>
                  Lớp học
                </Typography>
                <ClassIcon sx={{ color: 'secondary.main' }} />
              </Box>
              <Typography variant='h3' sx={{ fontWeight: 700, color: 'secondary.main' }}>
                {stats.totalClasses}
              </Typography>
              <Typography variant='body2' sx={{ color: 'secondary.main', opacity: 0.8 }}>
                Đang giảng dạy
              </Typography>
            </CardContent>
          </Card>

          <Card elevation={0} sx={{ border: '2px solid', borderColor: 'divider' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant='body2' color='text.secondary' sx={{ fontWeight: 600 }}>
                  Sinh viên
                </Typography>
                <PeopleIcon sx={{ color: 'info.main' }} />
              </Box>
              <Typography variant='h3' sx={{ fontWeight: 700, color: 'info.main' }}>
                {stats.totalStudents}
              </Typography>
              <Typography variant='body2' color='text.secondary'>
                Tổng số sinh viên
              </Typography>
            </CardContent>
          </Card>

          <Card elevation={0} sx={{ border: '2px solid', borderColor: 'divider' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant='body2' color='text.secondary' sx={{ fontWeight: 600 }}>
                  Bài tập
                </Typography>
                <AssignmentIcon sx={{ color: 'success.main' }} />
              </Box>
              <Typography variant='h3' sx={{ fontWeight: 700, color: 'success.main' }}>
                {stats.activeAssignments}
              </Typography>
              <Typography variant='body2' color='text.secondary'>
                Đang hoạt động
              </Typography>
            </CardContent>
          </Card>

          <Card elevation={0} sx={{ border: '2px solid', borderColor: 'divider' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant='body2' color='text.secondary' sx={{ fontWeight: 600 }}>
                  Chờ chấm
                </Typography>
                <GradingIcon sx={{ color: 'warning.main' }} />
              </Box>
              <Typography variant='h3' sx={{ fontWeight: 700, color: 'warning.main' }}>
                {stats.pendingGrading}
              </Typography>
              <Typography variant='body2' color='text.secondary'>
                Bài nộp chưa chấm
              </Typography>
            </CardContent>
          </Card>
        </Box>

        {/* Lớp học của tôi */}
        <Box sx={{ mb: 4 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <ClassIcon sx={{ mr: 1, color: 'primary.main' }} />
            <Typography variant='h5' sx={{ fontWeight: 600, flexGrow: 1 }}>
              Lớp học của tôi
            </Typography>
            <Button
              variant='contained'
              startIcon={<AddIcon />}
              component={Link}
              to='/teacher/class/create'
              sx={{
                bgcolor: 'primary.main',
                color: 'secondary.main',
                fontWeight: 600,
              }}
            >
              Tạo lớp mới
            </Button>
          </Box>

          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr' }, gap: 3 }}>
            {classes.map((classItem) => (
              <Card
                elevation={0}
                sx={{
                  border: '2px solid',
                  borderColor: 'divider',
                  height: '100%',
                  transition: 'all 0.2s',
                  '&:hover': {
                    borderColor: 'primary.main',
                    transform: 'translateY(-4px)',
                    boxShadow: 4,
                  },
                }}
                key={classItem.id}
              >
                <CardActionArea component={Link} to={`/teacher/class/${classItem.id}`}>
                  <Box
                    sx={{
                      height: 120,
                      bgcolor: 'secondary.main',
                      backgroundImage: 'linear-gradient(135deg, #00275e 0%, #003d8f 100%)',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    <Typography variant='h6' sx={{ color: 'primary.main', fontWeight: 700, px: 2, textAlign: 'center' }}>
                      {classItem.code}
                    </Typography>
                  </Box>
                  <CardContent>
                    <Typography variant='h6' sx={{ fontWeight: 600, mb: 1 }}>
                      {classItem.name}
                    </Typography>
                    <Typography variant='body2' color='text.secondary' sx={{ mb: 2 }}>
                      {classItem.semester}
                    </Typography>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Chip label='28 SV' size='small' icon={<PeopleIcon />} />
                      <Chip label='5 bài tập' size='small' variant='outlined' />
                    </Box>
                  </CardContent>
                </CardActionArea>
              </Card>
            ))}
          </Box>
        </Box>

        {/* Recent Assignments */}
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <AssignmentIcon sx={{ mr: 1, color: 'primary.main' }} />
            <Typography variant='h5' sx={{ fontWeight: 600, flexGrow: 1 }}>
              Bài tập gần đây
            </Typography>
            <IconButton component={Link} to='/teacher/assignments' size='small'>
              <ArrowForwardIcon />
            </IconButton>
          </Box>

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {assignments.slice(0, 3).map((assignment) => (
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
                <CardActionArea component={Link} to={`/teacher/assignment/${assignment.id}`}>
                  <CardContent sx={{ p: 3 }}>
                    <Box sx={{ display: 'flex', gap: 3 }}>
                      <Box
                        sx={{
                          width: 60,
                          height: 60,
                          borderRadius: 2,
                          bgcolor: 'primary.main',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          flexShrink: 0,
                        }}
                      >
                        <AssignmentIcon sx={{ color: 'secondary.main', fontSize: 32 }} />
                      </Box>

                      <Box sx={{ flexGrow: 1 }}>
                        <Typography variant='h6' sx={{ fontWeight: 600, mb: 0.5 }}>
                          {assignment.title}
                        </Typography>
                        <Typography variant='body2' color='text.secondary' sx={{ mb: 1 }}>
                          {assignment.className}
                        </Typography>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
                          <Chip label={`${assignment.problems.length} câu hỏi`} size='small' variant='outlined' />
                          <Chip label={`Hạn: ${new Date(assignment.dueDate).toLocaleDateString('vi-VN')}`} size='small' />
                          <Chip label='8/28 đã nộp' size='small' color='warning' />
                        </Box>
                      </Box>

                      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 1 }}>
                        <Button
                          size='small'
                          variant='outlined'
                          startIcon={<GradingIcon />}
                          sx={{ borderColor: 'primary.main', color: 'primary.main' }}
                        >
                          Chấm bài
                        </Button>
                        <Button size='small' variant='text' startIcon={<TrendingUpIcon />}>
                          Xem báo cáo
                        </Button>
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
