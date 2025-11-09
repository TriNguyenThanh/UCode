import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/profile'
import { auth } from '~/auth'
import * as StudentService from '~/services/studentService'
import * as TeacherService from '~/services/teacherService'
import { Navigation } from '~/components/Navigation'
import {
  Container,
  Typography,
  Box,
  Paper,
  Avatar,
  Chip,
  Divider,
  Card,
  CardContent,
  LinearProgress,
  IconButton,
} from '@mui/material'
import EditIcon from '@mui/icons-material/Edit'
import EmailIcon from '@mui/icons-material/Email'
import SchoolIcon from '@mui/icons-material/School'
import EmojiEventsIcon from '@mui/icons-material/EmojiEvents'
import AssignmentTurnedInIcon from '@mui/icons-material/AssignmentTurnedIn'
import CodeIcon from '@mui/icons-material/Code'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import { mockClasses, mockAssignments } from '~/data/mock'

export const meta: Route.MetaFunction = () => [
  { title: 'Hồ sơ | UCode' },
  { name: 'description', content: 'Thông tin cá nhân và thống kê.' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')

  try {
    let profile = null
    
    // Lấy profile dựa vào role
    if (user.role === 'student') {
      profile = await StudentService.getMyProfile()
    } else if (user.role === 'teacher' || user.role === 'admin') {
      profile = await TeacherService.getMyProfile()
    }
    
    // Mock statistics (TODO: Thay bằng real API khi backend có endpoint)
    const stats = {
      totalClasses: mockClasses.length,
      completedAssignments: 5,
      totalAssignments: mockAssignments.length,
      problemsSolved: 23,
      totalProblems: 50,
      currentStreak: 7,
      rank: 'Silver',
      points: 1250,
    }

    return { user, profile, stats }
  } catch (error) {
    console.error('Error loading profile:', error)
    
    // Fallback nếu API fail
    const stats = {
      totalClasses: mockClasses.length,
      completedAssignments: 5,
      totalAssignments: mockAssignments.length,
      problemsSolved: 23,
      totalProblems: 50,
      currentStreak: 7,
      rank: 'Silver',
      points: 1250,
    }
    
    return { user, profile: null, stats }
  }
}

export default function Profile() {
  const { user, profile, stats } = useLoaderData<typeof clientLoader>()

  const completionRate = Math.round((stats.completedAssignments / stats.totalAssignments) * 100)
  const problemSolvedRate = Math.round((stats.problemsSolved / stats.totalProblems) * 100)
  
  // Display name từ profile hoặc fallback to email
  const displayName = profile?.fullName || user.email.split('@')[0]
  const email = profile?.email || user.email
  
  // Hiển thị mã tùy theo role
  const isStudent = user.role === 'student'
  const code = isStudent 
    ? (profile as any)?.studentCode || 'N/A' 
    : (profile as any)?.teacherCode || 'N/A'
  const codeLabel = isStudent ? 'MSSV' : 'MSGV'

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Profile Header */}
        <Paper
          sx={{
            mb: 4,
            overflow: 'hidden',
            background: 'linear-gradient(135deg, #00275e 0%, #003d8f 100%)',
          }}
        >
          <Box sx={{ p: 4 }}>
            <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 3, flexWrap: 'wrap' }}>
              {/* Avatar */}
              <Avatar
                sx={{
                  width: 120,
                  height: 120,
                  bgcolor: 'primary.main',
                  color: 'secondary.main',
                  fontSize: '3rem',
                  fontWeight: 700,
                  border: '4px solid',
                  borderColor: 'primary.main',
                }}
              >
                {displayName.charAt(0).toUpperCase()}
              </Avatar>

              {/* User Info */}
              <Box sx={{ flexGrow: 1 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                  <Typography variant='h4' sx={{ fontWeight: 700, color: 'primary.main' }}>
                    {displayName}
                  </Typography>
                  <Chip
                    label={stats.rank}
                    icon={<EmojiEventsIcon />}
                    sx={{
                      bgcolor: 'primary.main',
                      color: 'secondary.main',
                      fontWeight: 700,
                    }}
                  />
                  <IconButton size='small' sx={{ color: 'primary.main' }}>
                    <EditIcon />
                  </IconButton>
                </Box>

                <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, mb: 2, flexWrap: 'wrap' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <EmailIcon sx={{ color: 'primary.main', fontSize: 20 }} />
                    <Typography variant='body1' sx={{ color: 'primary.main' }}>
                      {email}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <SchoolIcon sx={{ color: 'primary.main', fontSize: 20 }} />
                    <Typography variant='body1' sx={{ color: 'primary.main' }}>
                      {codeLabel}: {code}
                    </Typography>
                  </Box>
                </Box>

                {/* Points */}
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <TrendingUpIcon sx={{ color: 'primary.main' }} />
                  <Typography variant='h6' sx={{ color: 'primary.main', fontWeight: 600 }}>
                    {stats.points} điểm
                  </Typography>
                </Box>
              </Box>
            </Box>
          </Box>
        </Paper>

        {/* Statistics Grid */}
        <Box sx={{ mb: 4 }}>
          <Typography variant='h5' sx={{ fontWeight: 600, mb: 3 }}>
            Thống kê
          </Typography>

          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', lg: '1fr 1fr 1fr 1fr' }, gap: 3 }}>
            {/* Total Classes */}
            <Card elevation={0} sx={{ border: '2px solid', borderColor: 'divider' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                  <Typography variant='body2' color='text.secondary' sx={{ fontWeight: 600 }}>
                    Lớp học
                  </Typography>
                  <SchoolIcon sx={{ color: 'primary.main' }} />
                </Box>
                <Typography variant='h4' sx={{ fontWeight: 700, color: 'primary.main' }}>
                  {stats.totalClasses}
                </Typography>
                <Typography variant='body2' color='text.secondary'>
                  Đang tham gia
                </Typography>
              </CardContent>
            </Card>

            {/* Completed Assignments */}
            <Card elevation={0} sx={{ border: '2px solid', borderColor: 'divider' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                  <Typography variant='body2' color='text.secondary' sx={{ fontWeight: 600 }}>
                    Bài tập
                  </Typography>
                  <AssignmentTurnedInIcon sx={{ color: 'success.main' }} />
                </Box>
                <Typography variant='h4' sx={{ fontWeight: 700, color: 'success.main' }}>
                  {stats.completedAssignments}/{stats.totalAssignments}
                </Typography>
                <Typography variant='body2' color='text.secondary'>
                  Hoàn thành {completionRate}%
                </Typography>
              </CardContent>
            </Card>

            {/* Problems Solved */}
            <Card elevation={0} sx={{ border: '2px solid', borderColor: 'divider' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                  <Typography variant='body2' color='text.secondary' sx={{ fontWeight: 600 }}>
                    Bài giải
                  </Typography>
                  <CodeIcon sx={{ color: 'info.main' }} />
                </Box>
                <Typography variant='h4' sx={{ fontWeight: 700, color: 'info.main' }}>
                  {stats.problemsSolved}/{stats.totalProblems}
                </Typography>
                <Typography variant='body2' color='text.secondary'>
                  Đã giải {problemSolvedRate}%
                </Typography>
              </CardContent>
            </Card>

            {/* Current Streak */}
            <Card elevation={0} sx={{ border: '2px solid', borderColor: 'divider' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                  <Typography variant='body2' color='text.secondary' sx={{ fontWeight: 600 }}>
                    Chuỗi ngày
                  </Typography>
                  <EmojiEventsIcon sx={{ color: 'warning.main' }} />
                </Box>
                <Typography variant='h4' sx={{ fontWeight: 700, color: 'warning.main' }}>
                  {stats.currentStreak}
                </Typography>
                <Typography variant='body2' color='text.secondary'>
                  Ngày liên tiếp
                </Typography>
              </CardContent>
            </Card>
          </Box>
        </Box>

        <Divider sx={{ my: 4 }} />

        {/* Progress Details */}
        <Box>
          <Typography variant='h5' sx={{ fontWeight: 600, mb: 3 }}>
            Tiến độ chi tiết
          </Typography>

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            {/* Assignments Progress */}
            <Card elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                  <Typography variant='h6' sx={{ fontWeight: 600 }}>
                    Bài tập đã hoàn thành
                  </Typography>
                  <Typography variant='h6' sx={{ fontWeight: 700, color: 'primary.main' }}>
                    {completionRate}%
                  </Typography>
                </Box>
                <LinearProgress
                  variant='determinate'
                  value={completionRate}
                  sx={{
                    height: 10,
                    borderRadius: 5,
                    bgcolor: 'grey.200',
                    '& .MuiLinearProgress-bar': {
                      bgcolor: 'primary.main',
                      borderRadius: 5,
                    },
                  }}
                />
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
                  <Typography variant='body2' color='text.secondary'>
                    {stats.completedAssignments} bài đã nộp
                  </Typography>
                  <Typography variant='body2' color='text.secondary'>
                    {stats.totalAssignments - stats.completedAssignments} bài còn lại
                  </Typography>
                </Box>
              </CardContent>
            </Card>

            {/* Problems Progress */}
            <Card elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                  <Typography variant='h6' sx={{ fontWeight: 600 }}>
                    Bài tập đã giải
                  </Typography>
                  <Typography variant='h6' sx={{ fontWeight: 700, color: 'info.main' }}>
                    {problemSolvedRate}%
                  </Typography>
                </Box>
                <LinearProgress
                  variant='determinate'
                  value={problemSolvedRate}
                  sx={{
                    height: 10,
                    borderRadius: 5,
                    bgcolor: 'grey.200',
                    '& .MuiLinearProgress-bar': {
                      bgcolor: 'info.main',
                      borderRadius: 5,
                    },
                  }}
                />
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
                  <Typography variant='body2' color='text.secondary'>
                    {stats.problemsSolved} bài đã giải
                  </Typography>
                  <Typography variant='body2' color='text.secondary'>
                    {stats.totalProblems - stats.problemsSolved} bài còn lại
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Box>
        </Box>
      </Container>
    </Box>
  )
}
