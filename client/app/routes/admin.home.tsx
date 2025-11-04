import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/admin.home'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  CardActionArea,
  Button,
  Avatar,
  AvatarGroup,
  LinearProgress,
} from '@mui/material'
import PeopleIcon from '@mui/icons-material/People'
import SchoolIcon from '@mui/icons-material/School'
import ClassIcon from '@mui/icons-material/Class'
import AssignmentIcon from '@mui/icons-material/Assignment'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import SecurityIcon from '@mui/icons-material/Security'
import SettingsIcon from '@mui/icons-material/Settings'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import StorageIcon from '@mui/icons-material/Storage'
import BugReportIcon from '@mui/icons-material/BugReport'
import SpeedIcon from '@mui/icons-material/Speed'

export const meta: Route.MetaFunction = () => [
  { title: 'Admin Dashboard | UCode' },
  { name: 'description', content: 'Quản trị hệ thống UCode.' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'admin') throw redirect('/home')

  // Mock admin stats
  const stats = {
    totalUsers: 1247,
    totalTeachers: 45,
    totalStudents: 1202,
    totalClasses: 87,
    activeAssignments: 156,
    totalProblems: 542,
    serverUptime: '99.8%',
    storageUsed: 65,
    avgResponseTime: 142,
  }

  const recentActivities = [
    { id: 1, type: 'user', message: 'Nguyễn Văn A đã đăng ký tài khoản', time: '5 phút trước', avatar: '👤' },
    { id: 2, type: 'class', message: 'Lớp Java Technology 2024 đã được tạo', time: '15 phút trước', avatar: '📚' },
    { id: 3, type: 'assignment', message: 'Bài tập "Cấu trúc dữ liệu" đã được giao', time: '1 giờ trước', avatar: '📝' },
    { id: 4, type: 'system', message: 'Hệ thống đã được cập nhật phiên bản 2.1.0', time: '2 giờ trước', avatar: '⚙️' },
  ]

  const systemHealth = [
    { name: 'CPU Usage', value: 45, color: '#34D399' },
    { name: 'Memory', value: 68, color: '#FBBF24' },
    { name: 'Database', value: 32, color: '#60A5FA' },
    { name: 'API Response', value: 28, color: '#A78BFA' },
  ]

  return { user, stats, recentActivities, systemHealth }
}

export default function AdminHome() {
  const { stats, recentActivities, systemHealth } = useLoaderData<typeof clientLoader>()

  return (
    <Box
      sx={{
        minHeight: '100vh',
        bgcolor: '#f5f5f7',
      }}
    >
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Welcome Section - Apple style minimal */}
        <Box sx={{ mb: 4 }}>
          <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f', mb: 1 }}>
            Admin Dashboard
          </Typography>
          <Typography variant='body1' sx={{ color: '#6e6e73', fontSize: '1.125rem' }}>
            Quản trị và giám sát hệ thống UCode
          </Typography>
        </Box>

        {/* Main Stats - Clean Apple style */}
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', lg: '1fr 1fr 1fr 1fr' }, gap: 2, mb: 4 }}>
          {[
            { label: 'Tổng người dùng', value: stats.totalUsers, icon: PeopleIcon, color: '#007AFF' },
            { label: 'Giảng viên', value: stats.totalTeachers, icon: SchoolIcon, color: '#34C759' },
            { label: 'Sinh viên', value: stats.totalStudents, icon: PeopleIcon, color: '#FF9500' },
            { label: 'Lớp học', value: stats.totalClasses, icon: ClassIcon, color: '#AF52DE' },
          ].map((stat, index) => (
            <Card
              key={index}
              elevation={0}
              sx={{
                borderRadius: 3,
                bgcolor: 'white',
                border: '1px solid #d2d2d7',
                transition: 'all 0.2s ease',
                '&:hover': {
                  boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                  transform: 'translateY(-2px)',
                },
              }}
            >
              <CardContent sx={{ p: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <stat.icon sx={{ color: stat.color, fontSize: 24, mr: 1 }} />
                  <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', fontSize: '0.875rem' }}>
                    {stat.label}
                  </Typography>
                </Box>
                <Typography variant='h3' sx={{ fontWeight: 600, color: '#1d1d1f', fontSize: '2.5rem' }}>
                  {stat.value.toLocaleString()}
                </Typography>
              </CardContent>
            </Card>
          ))}
        </Box>

        {/* Secondary Stats */}
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr 1fr' }, gap: 2, mb: 4 }}>
          {[
            { label: 'Bài tập đang hoạt động', value: stats.activeAssignments, icon: AssignmentIcon, color: '#007AFF' },
            { label: 'Tổng bài lập trình', value: stats.totalProblems, icon: StorageIcon, color: '#34C759' },
            { label: 'Thời gian hoạt động', value: stats.serverUptime, icon: SpeedIcon, color: '#FF3B30' },
          ].map((stat, index) => (
            <Card
              key={index}
              elevation={0}
              sx={{
                borderRadius: 3,
                bgcolor: 'white',
                border: '1px solid #d2d2d7',
                transition: 'all 0.2s ease',
                '&:hover': {
                  boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                  transform: 'translateY(-2px)',
                },
              }}
            >
              <CardContent sx={{ p: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                  <Box>
                    <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', mb: 1, fontSize: '0.875rem' }}>
                      {stat.label}
                    </Typography>
                    <Typography variant='h4' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                      {typeof stat.value === 'number' ? stat.value.toLocaleString() : stat.value}
                    </Typography>
                  </Box>
                  <stat.icon sx={{ color: stat.color, fontSize: 28 }} />
                </Box>
              </CardContent>
            </Card>
          ))}
        </Box>

        {/* System Health & Recent Activities */}
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', lg: '1fr 1fr' }, gap: 2, mb: 4 }}>
          {/* System Health */}
          <Card
            elevation={0}
            sx={{
              borderRadius: 3,
              bgcolor: 'white',
              border: '1px solid #d2d2d7',
            }}
          >
            <CardContent sx={{ p: 3 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                <SpeedIcon sx={{ color: '#007AFF', mr: 1.5, fontSize: 24 }} />
                <Typography variant='h6' sx={{ fontWeight: 600, color: '#1d1d1f', fontSize: '1.25rem' }}>
                  Tình trạng hệ thống
                </Typography>
              </Box>

              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                {systemHealth.map((item, index) => (
                  <Box key={index}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                      <Typography variant='body2' sx={{ fontWeight: 600, color: 'text.secondary' }}>
                        {item.name}
                      </Typography>
                      <Typography variant='body2' sx={{ fontWeight: 700, color: item.color }}>
                        {item.value}%
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant='determinate'
                      value={item.value}
                      sx={{
                        height: 8,
                        borderRadius: 2,
                        backgroundColor: `${item.color}20`,
                        '& .MuiLinearProgress-bar': {
                          borderRadius: 2,
                          backgroundColor: item.color,
                        },
                      }}
                    />
                  </Box>
                ))}
              </Box>

              <Box
                sx={{
                  mt: 3,
                  p: 2.5,
                  borderRadius: 2,
                  bgcolor: '#34C759',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                }}
              >
                <Box>
                  <Typography variant='body2' sx={{ color: 'white', fontWeight: 600, mb: 0.5 }}>
                    Trạng thái: Hoạt động tốt
                  </Typography>
                  <Typography variant='caption' sx={{ color: 'rgba(255,255,255,0.95)' }}>
                    Tất cả các dịch vụ đang chạy bình thường
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>

          {/* Recent Activities */}
          <Card
            elevation={0}
            sx={{
              borderRadius: 3,
              bgcolor: 'white',
              border: '1px solid #d2d2d7',
            }}
          >
            <CardContent sx={{ p: 3 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <TrendingUpIcon sx={{ color: '#007AFF', mr: 1.5, fontSize: 24 }} />
                  <Typography variant='h6' sx={{ fontWeight: 600, color: '#1d1d1f', fontSize: '1.25rem' }}>
                    Hoạt động gần đây
                  </Typography>
                </Box>
                <Button
                  size='small'
                  endIcon={<ArrowForwardIcon />}
                  sx={{
                    borderRadius: 2,
                    textTransform: 'none',
                    fontWeight: 600,
                    color: '#007AFF',
                    '&:hover': {
                      bgcolor: 'transparent',
                    },
                  }}
                >
                  Xem tất cả
                </Button>
              </Box>

              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                {recentActivities.map((activity) => (
                  <Box
                    key={activity.id}
                    sx={{
                      p: 2,
                      borderRadius: 2,
                      bgcolor: '#f5f5f7',
                      transition: 'background 0.2s',
                      '&:hover': {
                        bgcolor: '#e8e8ed',
                      },
                    }}
                  >
                    <Typography variant='body2' sx={{ fontWeight: 500, mb: 0.5, color: '#1d1d1f' }}>
                      {activity.message}
                    </Typography>
                    <Typography variant='caption' sx={{ color: '#86868b' }}>
                      {activity.time}
                    </Typography>
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Box>

        {/* Quick Actions */}
        <Card
          elevation={0}
          sx={{
            borderRadius: 3,
            bgcolor: 'white',
            border: '1px solid #d2d2d7',
          }}
        >
          <CardContent sx={{ p: 3 }}>
            <Typography variant='h6' sx={{ fontWeight: 600, mb: 3, color: '#1d1d1f', fontSize: '1.25rem' }}>
              Thao tác nhanh
            </Typography>

            <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr 1fr' }, gap: 2 }}>
              {[
                { label: 'Quản lý người dùng', icon: PeopleIcon, color: '#007AFF', to: '/admin/users' },
                { label: 'Quản lý lớp học', icon: ClassIcon, color: '#AF52DE', to: '/admin/classes' },
                { label: 'Cài đặt hệ thống', icon: SettingsIcon, color: '#34C759', to: '/admin/settings' },
                { label: 'Báo cáo & Logs', icon: BugReportIcon, color: '#FF3B30', to: '/admin/logs' },
              ].map((action, index) => (
                <Button
                  key={index}
                  component={Link}
                  to={action.to}
                  sx={{
                    p: 2.5,
                    borderRadius: 2,
                    bgcolor: '#f5f5f7',
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    gap: 1.5,
                    textTransform: 'none',
                    transition: 'all 0.2s ease',
                    border: '1px solid transparent',
                    '&:hover': {
                      bgcolor: 'white',
                      border: '1px solid #d2d2d7',
                      transform: 'translateY(-2px)',
                      boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                    },
                  }}
                >
                  <action.icon sx={{ color: action.color, fontSize: 32 }} />
                  <Typography variant='body2' sx={{ fontWeight: 600, color: '#1d1d1f', textAlign: 'center' }}>
                    {action.label}
                  </Typography>
                </Button>
              ))}
            </Box>
          </CardContent>
        </Card>
      </Container>
    </Box>
  )
}
