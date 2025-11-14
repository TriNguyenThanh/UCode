import * as React from 'react'
import { useLoaderData, redirect } from 'react-router'
import type { Route } from './+types/admin.logs'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  Button,
  Chip,
  TextField,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  Tab,
  Alert,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import BugReportIcon from '@mui/icons-material/BugReport'
import InfoIcon from '@mui/icons-material/Info'
import WarningIcon from '@mui/icons-material/Warning'
import ErrorIcon from '@mui/icons-material/Error'
import DownloadIcon from '@mui/icons-material/Download'
import FilterListIcon from '@mui/icons-material/FilterList'
import ConstructionIcon from '@mui/icons-material/Construction'

export const meta: Route.MetaFunction = () => [
  { title: 'Logs & Báo cáo | Admin | UCode' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'admin') throw redirect('/home')

  // Mock logs data
  const logs = [
    { id: 1, timestamp: '2024-11-04 14:23:45', level: 'info', message: 'User student1@utc2.edu.vn logged in successfully', source: 'Auth Service' },
    { id: 2, timestamp: '2024-11-04 14:22:12', level: 'info', message: 'New assignment created: "Cấu trúc dữ liệu - Bài 5"', source: 'Assignment Service' },
    { id: 3, timestamp: '2024-11-04 14:20:33', level: 'warning', message: 'High memory usage detected: 85%', source: 'System Monitor' },
    { id: 4, timestamp: '2024-11-04 14:18:56', level: 'error', message: 'Failed to compile submission #1234: Syntax error', source: 'Judge Service' },
    { id: 5, timestamp: '2024-11-04 14:15:22', level: 'info', message: 'Database backup completed successfully', source: 'Database Service' },
    { id: 6, timestamp: '2024-11-04 14:12:08', level: 'warning', message: 'Slow query detected: SELECT * FROM submissions (2.5s)', source: 'Database Service' },
    { id: 7, timestamp: '2024-11-04 14:10:45', level: 'error', message: 'API rate limit exceeded for IP 192.168.1.100', source: 'API Gateway' },
    { id: 8, timestamp: '2024-11-04 14:08:17', level: 'info', message: 'New user registered: teacher3@utc2.edu.vn', source: 'User Service' },
  ]

  return { user, logs }
}

export default function AdminLogs() {
  const { logs } = useLoaderData<typeof clientLoader>()
  const [tabValue, setTabValue] = React.useState(0)

  const getLevelColor = (level: string) => {
    switch (level) {
      case 'error':
        return { bg: '#EF444415', color: '#EF4444' }
      case 'warning':
        return { bg: '#F59E0B15', color: '#F59E0B' }
      case 'info':
        return { bg: '#3B82F615', color: '#3B82F6' }
      default:
        return { bg: '#6B728015', color: '#6B7280' }
    }
  }

  const getLevelIcon = (level: string) => {
    switch (level) {
      case 'error':
        return <ErrorIcon sx={{ fontSize: 20 }} />
      case 'warning':
        return <WarningIcon sx={{ fontSize: 20 }} />
      case 'info':
        return <InfoIcon sx={{ fontSize: 20 }} />
      default:
        return <BugReportIcon sx={{ fontSize: 20 }} />
    }
  }

  const filteredLogs = tabValue === 0 ? logs : logs.filter((log) => {
    if (tabValue === 1) return log.level === 'error'
    if (tabValue === 2) return log.level === 'warning'
    if (tabValue === 3) return log.level === 'info'
    return true
  })

  return (
    <Box
      sx={{
        minHeight: '100vh',
        bgcolor: '#f5f5f7',
      }}
    >
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
            <Box>
              <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f', mb: 0.5 }}>
                System Logs & Reports
              </Typography>
              <Typography variant='body1' sx={{ color: '#6e6e73', fontSize: '1.125rem' }}>
                Theo dõi và phân tích logs hệ thống
              </Typography>
            </Box>

            <Button
              disabled
              variant='contained'
              startIcon={<DownloadIcon />}
              sx={{
                borderRadius: 2,
                px: 3,
                py: 1.5,
                bgcolor: '#007AFF',
                boxShadow: 'none',
                textTransform: 'none',
                fontWeight: 600,
                fontSize: '1rem',
                '&:hover': {
                  bgcolor: '#0051D5',
                  boxShadow: 'none',
                },
              }}
            >
              Export Logs
            </Button>
          </Box>

          {/* Coming Soon Notice */}
          <Alert
            severity='info'
            icon={<ConstructionIcon />}
            sx={{
              borderRadius: 3,
              mt: 3,
              border: '1px solid #007AFF',
              bgcolor: '#E3F2FD',
              '& .MuiAlert-icon': {
                color: '#007AFF',
              },
            }}
          >
            <Typography variant='h6' sx={{ fontWeight: 600, mb: 0.5, color: '#1d1d1f' }}>
              Tính năng đang phát triển
            </Typography>
            <Typography variant='body2' sx={{ color: '#6e6e73' }}>
              Tính năng logs và báo cáo hệ thống sẽ được triển khai trong phiên bản tiếp theo. Vui lòng quay lại sau!
            </Typography>
          </Alert>

          {/* Search and Filter */}
          <Box sx={{ display: 'flex', gap: 2, mt: 3, opacity: 0.6, pointerEvents: 'none' }}>
            <TextField
              disabled
              placeholder='Tìm kiếm logs...'
              variant='outlined'
              fullWidth
              size='small'
              InputProps={{
                startAdornment: (
                  <InputAdornment position='start'>
                    <SearchIcon sx={{ color: '#86868b' }} />
                  </InputAdornment>
                ),
                sx: {
                  borderRadius: 2,
                  bgcolor: 'white',
                  '& fieldset': { borderColor: '#d2d2d7' },
                },
              }}
            />
            <Button
              disabled
              variant='outlined'
              startIcon={<FilterListIcon />}
              sx={{
                borderRadius: 2,
                px: 3,
                borderColor: '#d2d2d7',
                color: '#1d1d1f',
                textTransform: 'none',
                fontWeight: 600,
                minWidth: 140,
                '&:hover': {
                  borderColor: '#86868b',
                  bgcolor: 'transparent',
                },
              }}
            >
              Bộ lọc
            </Button>
          </Box>
        </Box>

        {/* Stats */}
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr 1fr 1fr' }, gap: 2, mb: 3, opacity: 0.6, pointerEvents: 'none' }}>
          {[
            { label: 'Tổng logs', value: logs.length, color: '#86868b', icon: BugReportIcon },
            { label: 'Errors', value: logs.filter((l) => l.level === 'error').length, color: '#FF3B30', icon: ErrorIcon },
            { label: 'Warnings', value: logs.filter((l) => l.level === 'warning').length, color: '#FF9500', icon: WarningIcon },
            { label: 'Info', value: logs.filter((l) => l.level === 'info').length, color: '#007AFF', icon: InfoIcon },
          ].map((stat, index) => (
            <Card
              key={index}
              elevation={0}
              sx={{
                borderRadius: 3,
                bgcolor: 'white',
                border: '1px solid #d2d2d7',
              }}
            >
              <CardContent sx={{ p: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <stat.icon sx={{ color: stat.color, fontSize: 24, mr: 1 }} />
                  <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', fontSize: '0.875rem' }}>
                    {stat.label}
                  </Typography>
                </Box>
                <Typography variant='h3' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                  {stat.value}
                </Typography>
              </CardContent>
            </Card>
          ))}
        </Box>

        {/* Logs Table */}
        <Card
          elevation={0}
          sx={{
            borderRadius: 3,
            bgcolor: 'white',
            border: '1px solid #d2d2d7',
            opacity: 0.6,
            pointerEvents: 'none',
          }}
        >
          <Box sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}>
            <Tabs value={tabValue} onChange={(e, v) => setTabValue(v)}>
              <Tab label='Tất cả' sx={{ textTransform: 'none', fontWeight: 600 }} />
              <Tab label='Errors' sx={{ textTransform: 'none', fontWeight: 600 }} />
              <Tab label='Warnings' sx={{ textTransform: 'none', fontWeight: 600 }} />
              <Tab label='Info' sx={{ textTransform: 'none', fontWeight: 600 }} />
            </Tabs>
          </Box>

          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>Thời gian</TableCell>
                  <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>Level</TableCell>
                  <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>Message</TableCell>
                  <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>Source</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredLogs.map((log) => {
                  const levelStyle = getLevelColor(log.level)
                  return (
                    <TableRow
                      key={log.id}
                      sx={{
                        '&:hover': {
                          background: 'rgba(0, 0, 0, 0.02)',
                        },
                        transition: 'background 0.2s',
                      }}
                    >
                      <TableCell>
                        <Typography variant='body2' sx={{ fontFamily: 'monospace', color: 'text.secondary' }}>
                          {log.timestamp}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          icon={getLevelIcon(log.level)}
                          label={log.level.toUpperCase()}
                          size='small'
                          sx={{
                            background: levelStyle.bg,
                            color: levelStyle.color,
                            fontWeight: 700,
                            borderRadius: 2,
                            textTransform: 'uppercase',
                            fontSize: '0.75rem',
                          }}
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant='body2' sx={{ fontFamily: 'monospace' }}>
                          {log.message}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={log.source}
                          size='small'
                          variant='outlined'
                          sx={{
                            borderRadius: 2,
                            fontWeight: 600,
                          }}
                        />
                      </TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>
          </TableContainer>
        </Card>
      </Container>
    </Box>
  )
}
