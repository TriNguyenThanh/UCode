import { useState, useMemo } from 'react'
import { redirect, useLoaderData, useNavigate, useRevalidator } from 'react-router'
import type { Route } from './+types/teacher.class.$classId.attendance.$sessionId'
import { auth } from '~/auth'
import { API } from '~/api'
import { Navigation } from '~/components/Navigation'
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  InputAdornment,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  IconButton,
  Tooltip,
  CircularProgress,
  Divider,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import RefreshIcon from '@mui/icons-material/Refresh'
import FileDownloadIcon from '@mui/icons-material/FileDownload'
import QrCodeIcon from '@mui/icons-material/QrCode'
import EditIcon from '@mui/icons-material/Edit'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import LanguageIcon from '@mui/icons-material/Language'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'
import PeopleIcon from '@mui/icons-material/People'
import PercentIcon from '@mui/icons-material/Percent'
import PersonOffIcon from '@mui/icons-material/PersonOff'
import { getAttendanceSessionById, getAttendanceRecordsBySession } from '~/services/attendanceService'
import { getClassStudents } from '~/services/classService'
import { AttendanceQRDialog } from '~/components/AttendanceQRDialog'
import type { ApiResponse, Class, AttendanceSession, StudentResponse } from '~/types'

// Extended AttendanceRecord with student info
export interface AttendanceRecordDetail {
  id: string
  sessionId: string
  userId: string
  studentCode: string
  fullName: string
  attendedAt: string | null
  ipAddress: string | null
  latitude: number | null
  longitude: number | null
  isValid: boolean
  invalidReason: string | null
}

// Raw attendance record from API
interface AttendanceRecordRaw {
  id: string
  sessionId: string
  userId: string
  attendedAt: string
  ipAddress?: string
  latitude?: number
  longitude?: number
  isValid: boolean
  invalidReason?: string
}

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  try {
    // Fetch class data from API
    const classResponse = await API.get<ApiResponse<Class>>(`/api/v1/classes/${params.classId}`)
    const classData = classResponse.data.data
    
    if (!classData) {
      throw new Response('Lớp học không tồn tại', { status: 404 })
    }

    // Fetch attendance session
    const session = await getAttendanceSessionById(params.sessionId)
    
    if (!session) {
      throw new Response('Phiên điểm danh không tồn tại', { status: 404 })
    }

    // Fetch class students and attendance records in parallel
    let students: StudentResponse[] = []
    let rawRecords: AttendanceRecordRaw[] = []
    
    try {
      const [studentsResult, recordsResult] = await Promise.all([
        getClassStudents(params.classId),
        getAttendanceRecordsBySession(params.sessionId).catch(() => [] as AttendanceRecordRaw[])
      ])
      students = studentsResult
      rawRecords = recordsResult as AttendanceRecordRaw[]
    } catch (error) {
      console.warn('Failed to load data:', error)
    }

    // Create a map of attendance records by userId for quick lookup
    const recordsMap = new Map<string, AttendanceRecordRaw>()
    for (const record of rawRecords) {
      recordsMap.set(record.userId, record)
    }

    // Map students with their attendance records
    const records: AttendanceRecordDetail[] = students
      .sort((a, b) => (a.studentCode || '').localeCompare(b.studentCode || ''))
      .map(student => {
        const attendanceRecord = recordsMap.get(student.userId)
        
        return {
          id: attendanceRecord?.id || '',
          sessionId: params.sessionId,
          userId: student.userId,
          studentCode: student.studentCode || 'N/A',
          fullName: student.fullName || 'N/A',
          attendedAt: attendanceRecord?.attendedAt || null,
          ipAddress: attendanceRecord?.ipAddress || null,
          latitude: attendanceRecord?.latitude || null,
          longitude: attendanceRecord?.longitude || null,
          isValid: attendanceRecord?.isValid || false,
          invalidReason: attendanceRecord?.invalidReason || null,
        }
      })

    return { user, classData, session, records }
  } catch (error) {
    console.error('Error loading data:', error)
    throw new Response('Không thể tải thông tin', { status: 500 })
  }
}

export default function AttendanceDetail() {
  const { classData, session, records: initialRecords } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const revalidator = useRevalidator()
  
  const [searchText, setSearchText] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [qrDialogOpen, setQrDialogOpen] = useState(false)
  const [loading, setLoading] = useState(false)

  // Calculate statistics
  const stats = useMemo(() => {
    const totalStudents = initialRecords.length
    const attendedCount = initialRecords.filter(r => r.attendedAt !== null).length
    const validCount = initialRecords.filter(r => r.attendedAt !== null && r.isValid).length
    const invalidCount = initialRecords.filter(r => r.attendedAt !== null && !r.isValid).length
    const notAttendedCount = initialRecords.filter(r => r.attendedAt === null).length
    const attendanceRate = totalStudents > 0 ? (validCount / totalStudents) * 100 : 0

    return {
      totalStudents,
      attendedCount,
      validCount,
      invalidCount,
      notAttendedCount,
      attendanceRate,
    }
  }, [initialRecords])

  // Filter records based on search and status
  const filteredRecords = useMemo(() => {
    return initialRecords.filter(record => {
      // Search filter
      const searchLower = searchText.toLowerCase()
      const matchesSearch = !searchText || 
        record.studentCode?.toLowerCase().includes(searchLower) ||
        record.fullName?.toLowerCase().includes(searchLower)

      // Status filter
      let matchesStatus = true
      if (statusFilter === 'valid') {
        matchesStatus = record.attendedAt !== null && record.isValid
      } else if (statusFilter === 'invalid') {
        matchesStatus = record.attendedAt !== null && !record.isValid
      } else if (statusFilter === 'not-attended') {
        matchesStatus = record.attendedAt === null
      }

      return matchesSearch && matchesStatus
    })
  }, [initialRecords, searchText, statusFilter])

  // Determine session status
  const getSessionStatus = () => {
    const now = new Date()
    const startTime = new Date(session.startTime)
    const endTime = new Date(session.endTime)

    if (!session.isActive) {
      return { text: 'Đã tắt', color: 'default' as const }
    }
    if (now < startTime) {
      return { text: 'Chưa bắt đầu', color: 'info' as const }
    }
    if (now >= startTime && now <= endTime) {
      return { text: 'Đang diễn ra', color: 'success' as const }
    }
    return { text: 'Đã kết thúc', color: 'warning' as const }
  }

  const sessionStatus = getSessionStatus()

  const handleRefresh = async () => {
    setLoading(true)
    revalidator.revalidate()
    setLoading(false)
  }

  const handleExport = async () => {
    // Create CSV content
    const headers = ['STT', 'MSSV', 'Họ tên', 'Thời gian điểm danh', 'Trạng thái', 'Ghi chú']
    const rows = filteredRecords.map((record, index) => [
      index + 1,
      record.studentCode || '',
      record.fullName || '',
      record.attendedAt ? new Date(record.attendedAt).toLocaleString('vi-VN') : 'Chưa điểm danh',
      record.attendedAt ? (record.isValid ? 'Hợp lệ' : 'Không hợp lệ') : 'Chưa điểm danh',
      record.invalidReason || '',
    ])

    const csvContent = [
      headers.join(','),
      ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
    ].join('\n')

    // Add BOM for UTF-8
    const bom = '\uFEFF'
    const blob = new Blob([bom + csvContent], { type: 'text/csv;charset=utf-8;' })
    const link = document.createElement('a')
    link.href = URL.createObjectURL(blob)
    link.download = `diemdanh_${session.sessionCode}_${new Date().toISOString().split('T')[0]}.csv`
    link.click()
  }

  const handleBack = () => {
    navigate(`/teacher/class/${classData.classId}`)
  }

  const handleEdit = () => {
    navigate(`/teacher/class/${classData.classId}/attendance/${session.id}/edit`)
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      
      {/* Header */}
      <Box sx={{ bgcolor: 'secondary.main', py: 3, px: 4 }}>
        <Container maxWidth="xl">
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Box>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'primary.main', mb: 1 }}>
                {session.title}
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Typography variant="body1" sx={{ color: 'white', opacity: 0.8 }}>
                  Mã phiên: <strong style={{ color: 'white' }}>{session.sessionCode}</strong>
                </Typography>
                <Typography sx={{ color: 'white', opacity: 0.5 }}>•</Typography>
                <Chip
                  label={sessionStatus.text}
                  color={sessionStatus.color}
                  size="small"
                  sx={{ fontWeight: 600 }}
                />
              </Box>
            </Box>
            <Button
              variant="outlined"
              startIcon={<ArrowBackIcon />}
              onClick={handleBack}
              sx={{
                borderColor: 'primary.main',
                color: 'primary.main',
                '&:hover': {
                  borderColor: 'white',
                  bgcolor: 'rgba(255,255,255,0.1)',
                  color: 'white',
                },
              }}
            >
              Quay lại
            </Button>
          </Box>
        </Container>
      </Box>

      <Container maxWidth="xl" sx={{ py: 3 }}>
        {/* Statistics Cards */}
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mb: 3 }}>
          {/* Total Attendance */}
          <Box sx={{ flex: '1 1 180px', minWidth: 180 }}>
            <Paper
              sx={{
                p: 2.5,
                bgcolor: '#FFF9E6',
                borderLeft: '4px solid #FACB01',
                height: '100%',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <PeopleIcon sx={{ color: '#856404', fontSize: 20, mr: 0.5 }} />
                <Typography variant="body2" sx={{ fontWeight: 600, color: '#856404' }}>
                  Tổng số điểm danh
                </Typography>
              </Box>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: '#856404' }}>
                {stats.attendedCount}
              </Typography>
            </Paper>
          </Box>

          {/* Valid Count */}
          <Box sx={{ flex: '1 1 180px', minWidth: 180 }}>
            <Paper
              sx={{
                p: 2.5,
                bgcolor: '#E8F5E9',
                borderLeft: '4px solid #34C759',
                height: '100%',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <CheckCircleIcon sx={{ color: '#2E7D32', fontSize: 20, mr: 0.5 }} />
                <Typography variant="body2" sx={{ fontWeight: 600, color: '#2E7D32' }}>
                  Hợp lệ
                </Typography>
              </Box>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: '#2E7D32' }}>
                {stats.validCount}
              </Typography>
            </Paper>
          </Box>

          {/* Invalid Count */}
          <Box sx={{ flex: '1 1 180px', minWidth: 180 }}>
            <Paper
              sx={{
                p: 2.5,
                bgcolor: '#FFEBEE',
                borderLeft: '4px solid #FF3B30',
                height: '100%',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <CancelIcon sx={{ color: '#C62828', fontSize: 20, mr: 0.5 }} />
                <Typography variant="body2" sx={{ fontWeight: 600, color: '#C62828' }}>
                  Không hợp lệ
                </Typography>
              </Box>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: '#C62828' }}>
                {stats.invalidCount}
              </Typography>
            </Paper>
          </Box>

          {/* Not Attended Count */}
          <Box sx={{ flex: '1 1 180px', minWidth: 180 }}>
            <Paper
              sx={{
                p: 2.5,
                bgcolor: '#F3E5F5',
                borderLeft: '4px solid #9C27B0',
                height: '100%',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <PersonOffIcon sx={{ color: '#6A1B9A', fontSize: 20, mr: 0.5 }} />
                <Typography variant="body2" sx={{ fontWeight: 600, color: '#6A1B9A' }}>
                  Chưa điểm danh
                </Typography>
              </Box>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: '#6A1B9A' }}>
                {stats.notAttendedCount}
              </Typography>
            </Paper>
          </Box>

          {/* Attendance Rate */}
          <Box sx={{ flex: '1 1 180px', minWidth: 180 }}>
            <Paper
              sx={{
                p: 2.5,
                bgcolor: '#E3F2FD',
                borderLeft: '4px solid #2196F3',
                height: '100%',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <PercentIcon sx={{ color: '#1565C0', fontSize: 20, mr: 0.5 }} />
                <Typography variant="body2" sx={{ fontWeight: 600, color: '#1565C0' }}>
                  Tỷ lệ hợp lệ
                </Typography>
              </Box>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: '#1565C0' }}>
                {stats.attendanceRate.toFixed(1)}%
              </Typography>
            </Paper>
          </Box>
        </Box>

        {/* Session Details */}
        <Paper sx={{ p: 2, mb: 3 }}>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3, alignItems: 'center' }}>
            {/* Time Info */}
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <AccessTimeIcon sx={{ color: 'grey.600', fontSize: 18, mr: 0.5 }} />
              <Typography variant="body2" sx={{ fontWeight: 600, color: 'secondary.main' }}>
                {new Date(session.startTime).toLocaleString('vi-VN')}
              </Typography>
              <Typography variant="body2" sx={{ mx: 1, color: 'grey.500' }}>→</Typography>
              <Typography variant="body2" sx={{ fontWeight: 600, color: 'secondary.main' }}>
                {new Date(session.endTime).toLocaleString('vi-VN')}
              </Typography>
            </Box>

            <Divider orientation="vertical" flexItem />

            {/* IP Check */}
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <LanguageIcon sx={{ color: 'grey.600', fontSize: 18, mr: 0.5 }} />
              <Typography variant="body2" sx={{ color: 'grey.600' }}>IP: </Typography>
              <Typography variant="body2" sx={{ fontWeight: 600, color: 'secondary.main', ml: 0.5 }}>
                {session.requireIpCheck ? 'Có' : 'Không'}
              </Typography>
              {session.requireIpCheck && session.allowedIpSubnet && (
                <Typography variant="body2" sx={{ color: 'grey.500', ml: 0.5 }}>
                  ({session.allowedIpSubnet})
                </Typography>
              )}
            </Box>

            <Divider orientation="vertical" flexItem />

            {/* GPS Check */}
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <LocationOnIcon sx={{ color: 'grey.600', fontSize: 18, mr: 0.5 }} />
              <Typography variant="body2" sx={{ color: 'grey.600' }}>GPS: </Typography>
              <Typography variant="body2" sx={{ fontWeight: 600, color: 'secondary.main', ml: 0.5 }}>
                {session.requireGpsCheck ? 'Có' : 'Không'}
              </Typography>
              {session.requireGpsCheck && session.allowedLatitude && session.allowedLongitude && (
                <Typography variant="body2" sx={{ color: 'grey.500', ml: 0.5 }}>
                  ({session.allowedLatitude?.toFixed(4)}, {session.allowedLongitude?.toFixed(4)}, {session.allowedRadiusMeters}m)
                </Typography>
              )}
            </Box>
          </Box>
        </Paper>

        {/* Records Table */}
        <Paper sx={{ p: 3 }}>
          {/* Title and Actions */}
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
            <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
              Danh sách điểm danh
            </Typography>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Tooltip title="Làm mới">
                <IconButton onClick={handleRefresh} disabled={loading}>
                  {loading ? <CircularProgress size={24} /> : <RefreshIcon />}
                </IconButton>
              </Tooltip>
              <Tooltip title="Xuất Excel">
                <IconButton onClick={handleExport}>
                  <FileDownloadIcon />
                </IconButton>
              </Tooltip>
              <Tooltip title="Mã QR">
                <IconButton onClick={() => setQrDialogOpen(true)}>
                  <QrCodeIcon />
                </IconButton>
              </Tooltip>
              <Button
                variant="outlined"
                startIcon={<EditIcon />}
                onClick={handleEdit}
                sx={{
                  borderColor: 'secondary.main',
                  color: 'secondary.main',
                  '&:hover': {
                    borderColor: 'primary.main',
                    bgcolor: 'primary.main',
                    color: 'white',
                  },
                }}
              >
                Chỉnh sửa cấu hình
              </Button>
            </Box>
          </Box>

          {/* Search and Filter */}
          <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
            <TextField
              placeholder="Tìm kiếm theo MSSV hoặc họ tên..."
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              size="small"
              sx={{ flex: 1 }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon sx={{ color: 'grey.500' }} />
                  </InputAdornment>
                ),
              }}
            />
            <FormControl size="small" sx={{ minWidth: 200 }}>
              <InputLabel>Trạng thái</InputLabel>
              <Select
                value={statusFilter}
                label="Trạng thái"
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <MenuItem value="all">Tất cả</MenuItem>
                <MenuItem value="valid">Đã điểm danh hợp lệ</MenuItem>
                <MenuItem value="invalid">Đã điểm danh không hợp lệ</MenuItem>
                <MenuItem value="not-attended">Chưa điểm danh</MenuItem>
              </Select>
            </FormControl>
          </Box>

          {/* Table */}
          <TableContainer>
            <Table>
              <TableHead sx={{ bgcolor: 'grey.100' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 'bold', width: 60 }}>STT</TableCell>
                  <TableCell sx={{ fontWeight: 'bold' }}>MSSV</TableCell>
                  <TableCell sx={{ fontWeight: 'bold' }}>Họ và tên</TableCell>
                  <TableCell sx={{ fontWeight: 'bold' }}>Thời gian điểm danh</TableCell>
                  <TableCell sx={{ fontWeight: 'bold' }}>IP Address</TableCell>
                  <TableCell sx={{ fontWeight: 'bold' }}>Vị trí</TableCell>
                  <TableCell sx={{ fontWeight: 'bold' }}>Trạng thái</TableCell>
                  <TableCell sx={{ fontWeight: 'bold' }}>Ghi chú</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredRecords.map((record, index) => (
                  <TableRow key={record.id || record.userId} hover>
                    <TableCell>{index + 1}</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>{record.studentCode || 'N/A'}</TableCell>
                    <TableCell>{record.fullName || 'N/A'}</TableCell>
                    <TableCell>
                      {record.attendedAt 
                        ? new Date(record.attendedAt).toLocaleString('vi-VN')
                        : <Typography variant="body2" sx={{ color: 'grey.500', fontStyle: 'italic' }}>Chưa điểm danh</Typography>
                      }
                    </TableCell>
                    <TableCell>
                      {record.ipAddress || '-'}
                    </TableCell>
                    <TableCell>
                      {record.latitude && record.longitude 
                        ? `${record.latitude.toFixed(4)}, ${record.longitude.toFixed(4)}`
                        : '-'
                      }
                    </TableCell>
                    <TableCell>
                      {record.attendedAt ? (
                        record.isValid ? (
                          <Chip
                            icon={<CheckCircleIcon />}
                            label="Hợp lệ"
                            color="success"
                            size="small"
                            sx={{ fontWeight: 600 }}
                          />
                        ) : (
                          <Chip
                            icon={<CancelIcon />}
                            label="Không hợp lệ"
                            color="error"
                            size="small"
                            sx={{ fontWeight: 600 }}
                          />
                        )
                      ) : (
                        <Chip
                          label="Chưa điểm danh"
                          color="default"
                          size="small"
                          variant="outlined"
                        />
                      )}
                    </TableCell>
                    <TableCell>
                      <Typography
                        variant="body2"
                        sx={{
                          color: record.invalidReason ? 'error.main' : 'grey.500',
                          fontStyle: record.invalidReason ? 'normal' : 'italic',
                        }}
                      >
                        {record.invalidReason || '-'}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))}
                {filteredRecords.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={8} align="center" sx={{ py: 4 }}>
                      <Typography variant="body1" color="text.secondary">
                        Không có dữ liệu điểm danh
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      </Container>

      {/* QR Code Dialog */}
      <AttendanceQRDialog
        open={qrDialogOpen}
        session={session}
        onClose={() => setQrDialogOpen(false)}
      />
    </Box>
  )
}
