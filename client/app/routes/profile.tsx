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
  Button,
} from '@mui/material'
import EditIcon from '@mui/icons-material/Edit'
import EmailIcon from '@mui/icons-material/Email'
import SchoolIcon from '@mui/icons-material/School'
import EmojiEventsIcon from '@mui/icons-material/EmojiEvents'
import AssignmentTurnedInIcon from '@mui/icons-material/AssignmentTurnedIn'
import CodeIcon from '@mui/icons-material/Code'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import PersonIcon from '@mui/icons-material/Person'
import ClassIcon from '@mui/icons-material/Class'
import PhoneIcon from '@mui/icons-material/Phone'
import BadgeIcon from '@mui/icons-material/Badge'
import CalendarTodayIcon from '@mui/icons-material/CalendarToday'
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings'
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
  
  // Role-specific data
  const role = user.role
  const isStudent = role === 'student'
  const isTeacher = role === 'teacher'
  const isAdmin = role === 'admin'

  // Get role-specific info
  const getRoleInfo = () => {
    if (isStudent) {
      return {
        code: (profile as any)?.studentCode || 'N/A',
        codeLabel: 'Mã sinh viên',
        roleLabel: 'Sinh viên',
        roleColor: '#3B82F6',
        roleIcon: PersonIcon,
        additionalInfo: [
          { label: 'Năm nhập học', value: (profile as any)?.enrollmentYear || 'N/A', icon: CalendarTodayIcon },
          { label: 'Chuyên ngành', value: (profile as any)?.major || 'N/A', icon: SchoolIcon },
          { label: 'Năm học', value: (profile as any)?.classYear || 'N/A', icon: ClassIcon },
        ],
      }
    } else if (isTeacher) {
      return {
        code: (profile as any)?.teacherCode || 'N/A',
        codeLabel: 'Mã giảng viên',
        roleLabel: 'Giảng viên',
        roleColor: '#8B5CF6',
        roleIcon: SchoolIcon,
        additionalInfo: [
          { label: 'Khoa', value: (profile as any)?.department || 'N/A', icon: SchoolIcon },
          { label: 'Chức danh', value: (profile as any)?.title || 'N/A', icon: BadgeIcon },
          { label: 'Số điện thoại', value: (profile as any)?.phone || 'N/A', icon: PhoneIcon },
        ],
      }
    } else {
      return {
        code: 'ADMIN',
        codeLabel: 'Quản trị viên',
        roleLabel: 'Quản trị viên',
        roleColor: '#EF4444',
        roleIcon: AdminPanelSettingsIcon,
        additionalInfo: [
          { label: 'Quyền hạn', value: 'Toàn quyền hệ thống', icon: AdminPanelSettingsIcon },
          { label: 'Trạng thái', value: 'Đang hoạt động', icon: TrendingUpIcon },
        ],
      }
    }
  }

  const roleInfo = getRoleInfo()

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Profile Header - Apple Style */}
        <Card
          elevation={0}
          sx={{
            mb: 4,
            overflow: 'hidden',
            borderRadius: 3,
            border: '1px solid #d2d2d7',
            background: 'linear-gradient(135deg, #FFFFFF 0%, #F5F5F7 100%)',
          }}
        >
          <Box sx={{ p: 4 }}>
            <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 4, flexWrap: 'wrap' }}>
              {/* Avatar */}
              <Avatar
                sx={{
                  width: 140,
                  height: 140,
                  bgcolor: roleInfo.roleColor,
                  color: 'white',
                  fontSize: '3.5rem',
                  fontWeight: 700,
                  border: '4px solid white',
                  boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
                }}
              >
                {displayName.charAt(0).toUpperCase()}
              </Avatar>

              {/* User Info */}
              <Box sx={{ flexGrow: 1 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2, flexWrap: 'wrap' }}>
                  <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f' }}>
                    {displayName}
                  </Typography>
                  <Chip
                    icon={<roleInfo.roleIcon />}
                    label={roleInfo.roleLabel}
                    sx={{
                      bgcolor: roleInfo.roleColor,
                      color: 'white',
                      fontWeight: 600,
                      fontSize: '0.875rem',
                      height: 32,
                    }}
                  />
                  {isStudent && (
                    <Chip
                      icon={<EmojiEventsIcon />}
                      label={stats.rank}
                      sx={{
                        bgcolor: '#FF9500',
                        color: 'white',
                        fontWeight: 600,
                        height: 32,
                      }}
                    />
                  )}
                </Box>

                {/* Contact Info */}
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5, mb: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <EmailIcon sx={{ color: '#86868b', fontSize: 20 }} />
                    <Typography variant='body1' sx={{ color: '#1d1d1f', fontWeight: 500 }}>
                      {email}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <BadgeIcon sx={{ color: '#86868b', fontSize: 20 }} />
                    <Typography variant='body1' sx={{ color: '#1d1d1f', fontWeight: 500 }}>
                      {roleInfo.codeLabel}: <strong>{roleInfo.code}</strong>
                    </Typography>
                  </Box>
                </Box>

                {/* Points for Student only */}
                {isStudent && (
                  <Box
                    sx={{
                      display: 'inline-flex',
                      alignItems: 'center',
                      gap: 1,
                      px: 3,
                      py: 1.5,
                      borderRadius: 2,
                      bgcolor: roleInfo.roleColor,
                      color: 'white',
                    }}
                  >
                    <TrendingUpIcon />
                    <Typography variant='h6' sx={{ fontWeight: 700 }}>
                      {stats.points.toLocaleString()} điểm
                    </Typography>
                  </Box>
                )}
              </Box>

              {/* Edit Button */}
              <Button
                variant='outlined'
                startIcon={<EditIcon />}
                sx={{
                  borderRadius: 2,
                  borderColor: '#d2d2d7',
                  color: '#1d1d1f',
                  textTransform: 'none',
                  fontWeight: 600,
                  px: 3,
                  '&:hover': {
                    borderColor: '#86868b',
                    bgcolor: 'transparent',
                  },
                }}
              >
                Chỉnh sửa
              </Button>
            </Box>
          </Box>
        </Card>

        {/* Role-specific Additional Info */}
        <Card
          elevation={0}
          sx={{
            mb: 4,
            borderRadius: 3,
            border: '1px solid #d2d2d7',
            bgcolor: 'white',
          }}
        >
          <CardContent sx={{ p: 3 }}>
            <Typography variant='h6' sx={{ fontWeight: 600, mb: 3, color: '#1d1d1f' }}>
              Thông tin {roleInfo.roleLabel.toLowerCase()}
            </Typography>
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr' },
                gap: 3,
              }}
            >
              {roleInfo.additionalInfo.map((info, index) => (
                <Box key={index}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Box
                      sx={{
                        p: 1.5,
                        borderRadius: 2,
                        bgcolor: `${roleInfo.roleColor}15`,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                      }}
                    >
                      <info.icon sx={{ color: roleInfo.roleColor, fontSize: 24 }} />
                    </Box>
                    <Box>
                      <Typography variant='caption' sx={{ color: '#86868b', fontWeight: 600 }}>
                        {info.label}
                      </Typography>
                      <Typography variant='body1' sx={{ color: '#1d1d1f', fontWeight: 600 }}>
                        {info.value}
                      </Typography>
                    </Box>
                  </Box>
                </Box>
              ))}
            </Box>
          </CardContent>
        </Card>

        {/* Statistics Grid - Only for Student */}
        {isStudent && (
          <>
            <Typography variant='h5' sx={{ fontWeight: 700, mb: 3, color: '#1d1d1f' }}>
              Thống kê hoạt động
            </Typography>

            <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', lg: '1fr 1fr 1fr 1fr' }, gap: 3, mb: 4 }}>
              {/* Total Classes */}
              <Card
                elevation={0}
                sx={{
                  borderRadius: 3,
                  border: '1px solid #d2d2d7',
                  bgcolor: 'white',
                  transition: 'all 0.2s',
                  '&:hover': {
                    boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                    transform: 'translateY(-2px)',
                  },
                }}
              >
                <CardContent sx={{ p: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b' }}>
                      Lớp học
                    </Typography>
                    <Box
                      sx={{
                        p: 1,
                        borderRadius: 2,
                        bgcolor: '#007AFF15',
                      }}
                    >
                      <SchoolIcon sx={{ color: '#007AFF', fontSize: 24 }} />
                    </Box>
                  </Box>
                  <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f', mb: 0.5 }}>
                    {stats.totalClasses}
                  </Typography>
                  <Typography variant='body2' color='text.secondary'>
                    Đang tham gia
                  </Typography>
                </CardContent>
              </Card>

              {/* Completed Assignments */}
              <Card
                elevation={0}
                sx={{
                  borderRadius: 3,
                  border: '1px solid #d2d2d7',
                  bgcolor: 'white',
                  transition: 'all 0.2s',
                  '&:hover': {
                    boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                    transform: 'translateY(-2px)',
                  },
                }}
              >
                <CardContent sx={{ p: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b' }}>
                      Bài tập
                    </Typography>
                    <Box
                      sx={{
                        p: 1,
                        borderRadius: 2,
                        bgcolor: '#34C75915',
                      }}
                    >
                      <AssignmentTurnedInIcon sx={{ color: '#34C759', fontSize: 24 }} />
                    </Box>
                  </Box>
                  <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f', mb: 0.5 }}>
                    {stats.completedAssignments}/{stats.totalAssignments}
                  </Typography>
                  <Typography variant='body2' color='text.secondary'>
                    Hoàn thành {completionRate}%
                  </Typography>
                </CardContent>
              </Card>

              {/* Problems Solved */}
              <Card
                elevation={0}
                sx={{
                  borderRadius: 3,
                  border: '1px solid #d2d2d7',
                  bgcolor: 'white',
                  transition: 'all 0.2s',
                  '&:hover': {
                    boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                    transform: 'translateY(-2px)',
                  },
                }}
              >
                <CardContent sx={{ p: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b' }}>
                      Bài giải
                    </Typography>
                    <Box
                      sx={{
                        p: 1,
                        borderRadius: 2,
                        bgcolor: '#AF52DE15',
                      }}
                    >
                      <CodeIcon sx={{ color: '#AF52DE', fontSize: 24 }} />
                    </Box>
                  </Box>
                  <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f', mb: 0.5 }}>
                    {stats.problemsSolved}/{stats.totalProblems}
                  </Typography>
                  <Typography variant='body2' color='text.secondary'>
                    Đã giải {problemSolvedRate}%
                  </Typography>
                </CardContent>
              </Card>

              {/* Current Streak */}
              <Card
                elevation={0}
                sx={{
                  borderRadius: 3,
                  border: '1px solid #d2d2d7',
                  bgcolor: 'white',
                  transition: 'all 0.2s',
                  '&:hover': {
                    boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                    transform: 'translateY(-2px)',
                  },
                }}
              >
                <CardContent sx={{ p: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b' }}>
                      Chuỗi ngày
                    </Typography>
                    <Box
                      sx={{
                        p: 1,
                        borderRadius: 2,
                        bgcolor: '#FF950015',
                      }}
                    >
                      <EmojiEventsIcon sx={{ color: '#FF9500', fontSize: 24 }} />
                    </Box>
                  </Box>
                  <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f', mb: 0.5 }}>
                    {stats.currentStreak}
                  </Typography>
                  <Typography variant='body2' color='text.secondary'>
                    Ngày liên tiếp
                  </Typography>
                </CardContent>
              </Card>
            </Box>

            {/* Progress Details */}
            <Typography variant='h5' sx={{ fontWeight: 700, mb: 3, color: '#1d1d1f' }}>
              Tiến độ học tập
            </Typography>

            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              {/* Assignments Progress */}
              <Card
                elevation={0}
                sx={{
                  borderRadius: 3,
                  border: '1px solid #d2d2d7',
                  bgcolor: 'white',
                }}
              >
                <CardContent sx={{ p: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant='h6' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                      Bài tập đã hoàn thành
                    </Typography>
                    <Typography variant='h5' sx={{ fontWeight: 700, color: '#34C759' }}>
                      {completionRate}%
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant='determinate'
                    value={completionRate}
                    sx={{
                      height: 12,
                      borderRadius: 6,
                      bgcolor: '#f5f5f7',
                      '& .MuiLinearProgress-bar': {
                        bgcolor: '#34C759',
                        borderRadius: 6,
                      },
                    }}
                  />
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
                    <Typography variant='body2' sx={{ color: '#86868b', fontWeight: 600 }}>
                      {stats.completedAssignments} bài đã nộp
                    </Typography>
                    <Typography variant='body2' sx={{ color: '#86868b', fontWeight: 600 }}>
                      {stats.totalAssignments - stats.completedAssignments} bài còn lại
                    </Typography>
                  </Box>
                </CardContent>
              </Card>

              {/* Problems Progress */}
              <Card
                elevation={0}
                sx={{
                  borderRadius: 3,
                  border: '1px solid #d2d2d7',
                  bgcolor: 'white',
                }}
              >
                <CardContent sx={{ p: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                    <Typography variant='h6' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                      Bài tập lập trình đã giải
                    </Typography>
                    <Typography variant='h5' sx={{ fontWeight: 700, color: '#AF52DE' }}>
                      {problemSolvedRate}%
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant='determinate'
                    value={problemSolvedRate}
                    sx={{
                      height: 12,
                      borderRadius: 6,
                      bgcolor: '#f5f5f7',
                      '& .MuiLinearProgress-bar': {
                        bgcolor: '#AF52DE',
                        borderRadius: 6,
                      },
                    }}
                  />
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
                    <Typography variant='body2' sx={{ color: '#86868b', fontWeight: 600 }}>
                      {stats.problemsSolved} bài đã giải
                    </Typography>
                    <Typography variant='body2' sx={{ color: '#86868b', fontWeight: 600 }}>
                      {stats.totalProblems - stats.problemsSolved} bài còn lại
                    </Typography>
                  </Box>
                </CardContent>
              </Card>
            </Box>
          </>
        )}

        {/* Admin-specific content */}
        {isAdmin && (
          <Card
            elevation={0}
            sx={{
              borderRadius: 3,
              border: '1px solid #d2d2d7',
              bgcolor: 'white',
              p: 4,
              textAlign: 'center',
            }}
          >
            <AdminPanelSettingsIcon sx={{ fontSize: 64, color: '#EF4444', mb: 2 }} />
            <Typography variant='h5' sx={{ fontWeight: 700, color: '#1d1d1f', mb: 1 }}>
              Tài khoản Quản trị viên
            </Typography>
            <Typography variant='body1' sx={{ color: '#86868b', mb: 3 }}>
              Bạn có toàn quyền quản lý hệ thống UCode. Truy cập Dashboard để xem thống kê và quản lý người dùng.
            </Typography>
            <Button
              component={Link}
              to='/admin/home'
              variant='contained'
              size='large'
              sx={{
                borderRadius: 2,
                px: 4,
                py: 1.5,
                bgcolor: '#EF4444',
                textTransform: 'none',
                fontWeight: 600,
                fontSize: '1rem',
                '&:hover': {
                  bgcolor: '#DC2626',
                },
              }}
            >
              Đi tới Dashboard
            </Button>
          </Card>
        )}
      </Container>
    </Box>
  )
}
