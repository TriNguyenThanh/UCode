import { useState, useEffect } from 'react'
import { redirect, useLoaderData, useNavigate } from 'react-router'
import type { Route } from './+types/teacher.class.$classId.create-attendance'
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
  Checkbox,
  FormControlLabel,
  Alert,
  CircularProgress,
  Grid,
  Collapse,
  IconButton,
  InputAdornment,
} from '@mui/material'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker'
import { vi } from 'date-fns/locale/vi'
import SaveIcon from '@mui/icons-material/Save'
import CancelIcon from '@mui/icons-material/Cancel'
import RefreshIcon from '@mui/icons-material/Refresh'
import MapIcon from '@mui/icons-material/Map'
import InfoIcon from '@mui/icons-material/Info'
import { createAttendanceSession } from '~/services/attendanceService'
import type { ApiResponse, Class } from '~/types'

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  try {
    // Fetch class data from API
    const response = await API.get<ApiResponse<Class>>(`/api/v1/classes/${params.classId}`)
    const classData = response.data.data
    
    if (!classData) {
      throw new Response('Lớp học không tồn tại', { status: 404 })
    }

    return { user, classData }
  } catch (error) {
    console.error('Error loading class:', error)
    throw new Response('Không thể tải thông tin lớp học', { status: 500 })
  }
}

// Helper function to generate a unique session code
function generateSessionCode(): string {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'
  let code = ''
  for (let i = 0; i < 8; i++) {
    code += chars.charAt(Math.floor(Math.random() * chars.length))
  }
  return code
}

// Helper function to get current IP address (mock for now)
async function getCurrentIP(): Promise<string> {
  try {
    const response = await fetch('https://api.ipify.org?format=json')
    const data = await response.json()
    return data.ip
  } catch {
    return '192.168.1.1'
  }
}

export default function CreateAttendanceSession() {
  const { classData } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  
  // Form state
  const [sessionTitle, setSessionTitle] = useState(`Điểm danh ${new Date().toLocaleDateString('vi-VN')}`)
  const [sessionCode, setSessionCode] = useState(generateSessionCode())
  const [startTime, setStartTime] = useState<Date | null>(new Date())
  const [endTime, setEndTime] = useState<Date | null>(new Date(Date.now() + 2 * 60 * 60 * 1000)) // 2 hours from now
  const [isActive, setIsActive] = useState(true)
  
  // IP Check settings
  const [requireIpCheck, setRequireIpCheck] = useState(false)
  const [allowedIpSubnet, setAllowedIpSubnet] = useState('')
  
  // GPS Check settings
  const [requireGpsCheck, setRequireGpsCheck] = useState(false)
  const [allowedLatitude, setAllowedLatitude] = useState('')
  const [allowedLongitude, setAllowedLongitude] = useState('')
  const [allowedRadiusMeters, setAllowedRadiusMeters] = useState('100')
  
  // Face Check setting
  const [requireFaceCheck, setRequireFaceCheck] = useState(false)
  
  // UI state
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const handleRefreshIP = async () => {
    try {
      const ip = await getCurrentIP()
      setAllowedIpSubnet(ip)
    } catch (error) {
      console.error('Failed to get IP:', error)
    }
  }

  const handleOpenMap = () => {
    window.open('https://www.google.com/maps', '_blank')
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    // Validation
    if (!sessionTitle.trim()) {
      setError('Vui lòng nhập tiêu đề phiên điểm danh')
      return
    }
    
    if (!sessionCode.trim()) {
      setError('Vui lòng nhập mã phiên điểm danh')
      return
    }
    
    if (!startTime) {
      setError('Vui lòng chọn thời gian bắt đầu')
      return
    }
    
    if (!endTime) {
      setError('Vui lòng chọn thời gian kết thúc')
      return
    }
    
    if (endTime <= startTime) {
      setError('Thời gian kết thúc phải sau thời gian bắt đầu')
      return
    }
    
    if (requireIpCheck && !allowedIpSubnet.trim()) {
      setError('Vui lòng nhập địa chỉ IP cho phép')
      return
    }
    
    if (requireGpsCheck) {
      if (!allowedLatitude.trim() || !allowedLongitude.trim()) {
        setError('Vui lòng nhập tọa độ GPS đầy đủ')
        return
      }
      
      const lat = parseFloat(allowedLatitude)
      const lng = parseFloat(allowedLongitude)
      
      if (isNaN(lat) || lat < -90 || lat > 90) {
        setError('Vĩ độ không hợp lệ (phải từ -90 đến 90)')
        return
      }
      
      if (isNaN(lng) || lng < -180 || lng > 180) {
        setError('Kinh độ không hợp lệ (phải từ -180 đến 180)')
        return
      }
      
      const radius = parseInt(allowedRadiusMeters)
      if (isNaN(radius) || radius <= 0) {
        setError('Bán kính không hợp lệ')
        return
      }
    }

    setLoading(true)
    setError(null)

    try {
      await createAttendanceSession({
        classId: classData.classId,
        title: sessionTitle.trim(),
        sessionCode: sessionCode.trim(),
        startTime: startTime.toISOString(),
        endTime: endTime.toISOString(),
        requireIpCheck,
        allowedIpSubnet: requireIpCheck ? allowedIpSubnet.trim() : undefined,
        requireGpsCheck,
        allowedLatitude: requireGpsCheck ? parseFloat(allowedLatitude) : undefined,
        allowedLongitude: requireGpsCheck ? parseFloat(allowedLongitude) : undefined,
        allowedRadiusMeters: requireGpsCheck ? parseInt(allowedRadiusMeters) : undefined,
        requireFaceCheck,
        isActive,
      })

      setSuccess(true)
      setTimeout(() => {
        navigate(`/teacher/class/${classData.classId}`)
      }, 1500)
    } catch (err: any) {
      console.error('Failed to create attendance session:', err)
      setError(err.message || 'Không thể tạo phiên điểm danh')
    } finally {
      setLoading(false)
    }
  }

  const handleCancel = () => {
    navigate(`/teacher/class/${classData.classId}`)
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      <Container maxWidth="md" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 1 }}>
            Cấu hình điểm danh
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Lớp: <strong>{classData.className}</strong> ({classData.classCode})
          </Typography>
        </Box>

        {/* Success Message */}
        <Collapse in={success}>
          <Alert severity="success" sx={{ mb: 3 }}>
            Tạo phiên điểm danh thành công! Đang chuyển hướng...
          </Alert>
        </Collapse>

        {/* Error Message */}
        {error && (
          <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={vi}>
          <form onSubmit={handleSubmit}>
            {/* Session Title */}
            <Paper sx={{ p: 3, mb: 3 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <InfoIcon sx={{ color: 'secondary.main', mr: 1 }} />
                <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
                  Thông tin chung
                </Typography>
              </Box>

              <TextField
                label="Tiêu đề phiên điểm danh"
                value={sessionTitle}
                onChange={(e) => setSessionTitle(e.target.value)}
                fullWidth
                required
                sx={{ mb: 2 }}
              />

              <TextField
                label="Mã phiên điểm danh"
                value={sessionCode}
                onChange={(e) => setSessionCode(e.target.value.toUpperCase())}
                fullWidth
                required
                helperText="Mã duy nhất để sinh viên điểm danh (8 ký tự)"
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={() => setSessionCode(generateSessionCode())} edge="end">
                        <RefreshIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 2 }}
              />

              <FormControlLabel
                control={
                  <Checkbox
                    checked={isActive}
                    onChange={(e) => setIsActive(e.target.checked)}
                  />
                }
                label="Kích hoạt phiên điểm danh"
              />
            </Paper>

            {/* Time Settings */}
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 2 }}>
                Thời gian
              </Typography>

              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <DateTimePicker
                    label="Thời gian bắt đầu *"
                    value={startTime}
                    onChange={setStartTime}
                    slotProps={{
                      textField: {
                        fullWidth: true,
                        required: true,
                      },
                    }}
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <DateTimePicker
                    label="Thời gian kết thúc *"
                    value={endTime}
                    onChange={setEndTime}
                    slotProps={{
                      textField: {
                        fullWidth: true,
                        required: true,
                      },
                    }}
                  />
                </Grid>
              </Grid>
            </Paper>

            {/* IP Check Settings */}
            <Paper sx={{ p: 3, mb: 3 }}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={requireIpCheck}
                    onChange={(e) => setRequireIpCheck(e.target.checked)}
                  />
                }
                label={
                  <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
                    Kiểm tra IP Address
                  </Typography>
                }
              />

              <Collapse in={requireIpCheck}>
                <Box sx={{ mt: 2 }}>
                  <TextField
                    label="Địa chỉ IP cho phép"
                    value={allowedIpSubnet}
                    onChange={(e) => setAllowedIpSubnet(e.target.value)}
                    fullWidth
                    placeholder="VD: 192.168.1.0"
                    helperText="Định dạng: xxx.xxx.xxx.xxx"
                    InputProps={{
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton onClick={handleRefreshIP} edge="end">
                            <RefreshIcon />
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                  />
                </Box>
              </Collapse>
            </Paper>

            {/* GPS Check Settings */}
            <Paper sx={{ p: 3, mb: 3 }}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={requireGpsCheck}
                    onChange={(e) => setRequireGpsCheck(e.target.checked)}
                  />
                }
                label={
                  <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
                    Kiểm tra vị trí GPS
                  </Typography>
                }
              />

              <Collapse in={requireGpsCheck}>
                <Box sx={{ mt: 2 }}>
                  <Grid container spacing={2}>
                    <Grid item xs={12} md={4}>
                      <TextField
                        label="Latitude (Vĩ độ)"
                        value={allowedLatitude}
                        onChange={(e) => setAllowedLatitude(e.target.value)}
                        fullWidth
                        placeholder="VD: 10.762622"
                        type="number"
                        inputProps={{ step: 'any' }}
                      />
                    </Grid>
                    <Grid item xs={12} md={4}>
                      <TextField
                        label="Longitude (Kinh độ)"
                        value={allowedLongitude}
                        onChange={(e) => setAllowedLongitude(e.target.value)}
                        fullWidth
                        placeholder="VD: 106.660172"
                        type="number"
                        inputProps={{ step: 'any' }}
                      />
                    </Grid>
                    <Grid item xs={12} md={4}>
                      <TextField
                        label="Bán kính (m)"
                        value={allowedRadiusMeters}
                        onChange={(e) => setAllowedRadiusMeters(e.target.value)}
                        fullWidth
                        placeholder="VD: 100"
                        type="number"
                      />
                    </Grid>
                  </Grid>

                  <Button
                    startIcon={<MapIcon />}
                    onClick={handleOpenMap}
                    sx={{ mt: 2 }}
                  >
                    Mở Google Maps để lấy tọa độ
                  </Button>

                  <Alert severity="info" sx={{ mt: 2 }}>
                    <Typography variant="body2">
                      <strong>Cách lấy tọa độ từ Google Maps:</strong>
                      <br />
                      1. Click nút 'Mở Google Maps' ở trên
                      <br />
                      2. Click chuột phải vào vị trí bạn muốn trên bản đồ
                      <br />
                      3. Click vào tọa độ đầu tiên (dạng: 10.762622, 106.660172)
                      <br />
                      4. Tọa độ sẽ được copy, paste vào ô Vĩ độ và Kinh độ
                    </Typography>
                  </Alert>
                </Box>
              </Collapse>
            </Paper>

            {/* Face Check Settings */}
            <Paper sx={{ p: 3, mb: 3 }}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={requireFaceCheck}
                    onChange={(e) => setRequireFaceCheck(e.target.checked)}
                  />
                }
                label={
                  <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
                    Yêu cầu xác thực khuôn mặt
                  </Typography>
                }
              />
              {requireFaceCheck && (
                <Alert severity="warning" sx={{ mt: 2 }}>
                  Sinh viên sẽ cần đăng ký khuôn mặt trước khi điểm danh
                </Alert>
              )}
            </Paper>

            {/* Action Buttons */}
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button
                variant="outlined"
                startIcon={<CancelIcon />}
                onClick={handleCancel}
                disabled={loading}
              >
                Hủy
              </Button>
              <Button
                type="submit"
                variant="contained"
                startIcon={loading ? <CircularProgress size={20} /> : <SaveIcon />}
                disabled={loading}
              >
                {loading ? 'Đang lưu...' : 'Lưu cấu hình'}
              </Button>
            </Box>
          </form>
        </LocalizationProvider>
      </Container>
    </Box>
  )
}
