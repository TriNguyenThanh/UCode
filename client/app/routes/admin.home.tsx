import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/admin.home'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import axios from 'axios'
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
  CircularProgress,
  Alert,
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

const API_BASE_URL = 'http://localhost:5000/api/v1'

export const meta: Route.MetaFunction = () => [
  { title: 'Admin Dashboard | UCode' },
  { name: 'description', content: 'Quản trị hệ thống UCode.' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'admin') throw redirect('/home')

  return { user }
}

export default function AdminHome() {
  const [loading, setLoading] = React.useState(true)
  const [error, setError] = React.useState<string | null>(null)
  const [stats, setStats] = React.useState({
    totalUsers: 0,
    totalTeachers: 0,
    totalStudents: 0,
    totalClasses: 0,
    activeClasses: 0,
    archivedClasses: 0,
    totalStudentsEnrolled: 0,
    avgStudentsPerClass: 0,
  })

  // Fetch statistics
  React.useEffect(() => {
    const fetchStats = async () => {
      try {
        setLoading(true)
        const token = localStorage.getItem('token')

        // Fetch user statistics
        const userStatsResponse = await axios.get(`${API_BASE_URL}/admin/users/statistics`, {
          headers: { Authorization: `Bearer ${token}` },
        })

        // Fetch class statistics
        const classStatsResponse = await axios.get(`${API_BASE_URL}/admin/classes/statistics`, {
          headers: { Authorization: `Bearer ${token}` },
        })

        const userStats = userStatsResponse.data.data
        const classStats = classStatsResponse.data.data

        setStats({
          totalUsers: userStats.totalUsers,
          totalTeachers: userStats.teachers,
          totalStudents: userStats.students,
          totalClasses: classStats.totalClasses,
          activeClasses: classStats.activeClasses,
          archivedClasses: classStats.archivedClasses,
          totalStudentsEnrolled: classStats.totalStudentsEnrolled,
          avgStudentsPerClass: classStats.averageStudentsPerClass,
        })

        setError(null)
      } catch (err: any) {
        console.error('Failed to fetch statistics:', err)
        setError(err.response?.data?.message || 'Không thể tải thống kê')
      } finally {
        setLoading(false)
      }
    }

    fetchStats()
  }, [])

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

        {/* Loading State */}
        {loading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
            <CircularProgress size={48} />
          </Box>
        )}

        {/* Error State */}
        {error && !loading && (
          <Alert severity='error' sx={{ mb: 4, borderRadius: 2 }}>
            {error}
          </Alert>
        )}

        {/* Content - Only show when loaded */}
        {!loading && !error && (
          <>
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
            { label: 'Lớp đang hoạt động', value: stats.activeClasses, icon: ClassIcon, color: '#007AFF' },
            { label: 'Lớp đã lưu trữ', value: stats.archivedClasses, icon: StorageIcon, color: '#34C759' },
            {
              label: 'Trung bình SV/Lớp',
              value: stats.avgStudentsPerClass.toFixed(1),
              icon: PeopleIcon,
              color: '#FF9500',
            },
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

        {/* Statistics Summary Card */}
        <Card
          elevation={0}
          sx={{
            borderRadius: 3,
            bgcolor: 'white',
            border: '1px solid #d2d2d7',
            mb: 4,
          }}
        >
          <CardContent sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
              <TrendingUpIcon sx={{ color: '#007AFF', mr: 1.5, fontSize: 24 }} />
              <Typography variant='h6' sx={{ fontWeight: 600, color: '#1d1d1f', fontSize: '1.25rem' }}>
                Tóm tắt thống kê
              </Typography>
            </Box>

            <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr' }, gap: 3 }}>
              <Box
                sx={{
                  p: 2.5,
                  borderRadius: 2,
                  bgcolor: '#f5f5f7',
                }}
              >
                <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', mb: 1 }}>
                  Tổng sinh viên đã ghi danh
                </Typography>
                <Typography variant='h4' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                  {stats.totalStudentsEnrolled.toLocaleString()}
                </Typography>
              </Box>

              <Box
                sx={{
                  p: 2.5,
                  borderRadius: 2,
                  bgcolor: '#f5f5f7',
                }}
              >
                <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', mb: 1 }}>
                  Tổng lớp học
                </Typography>
                <Typography variant='h4' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                  {stats.totalClasses.toLocaleString()}
                </Typography>
              </Box>

              <Box
                sx={{
                  p: 2.5,
                  borderRadius: 2,
                  bgcolor: '#f5f5f7',
                }}
              >
                <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', mb: 1 }}>
                  Tổng giảng viên
                </Typography>
                <Typography variant='h4' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                  {stats.totalTeachers.toLocaleString()}
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>

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
          </>
        )}
      </Container>
    </Box>
  )
}
