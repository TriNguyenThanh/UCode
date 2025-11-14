import * as React from 'react'
import { useLoaderData, redirect } from 'react-router'
import type { Route } from './+types/admin.settings'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  Button,
  Switch,
  TextField,
  Divider,
  FormControlLabel,
  Chip,
  Alert,
} from '@mui/material'
import SettingsIcon from '@mui/icons-material/Settings'
import SecurityIcon from '@mui/icons-material/Security'
import NotificationsIcon from '@mui/icons-material/Notifications'
import StorageIcon from '@mui/icons-material/Storage'
import EmailIcon from '@mui/icons-material/Email'
import SaveIcon from '@mui/icons-material/Save'
import RestartAltIcon from '@mui/icons-material/RestartAlt'
import ConstructionIcon from '@mui/icons-material/Construction'

export const meta: Route.MetaFunction = () => [
  { title: 'Cài đặt hệ thống | Admin | UCode' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'admin') throw redirect('/home')

  return { user }
}

export default function AdminSettings() {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        bgcolor: '#f5f5f7',
      }}
    >
      <Navigation />

      <Container maxWidth='lg' sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f', mb: 1 }}>
            Cài đặt hệ thống
          </Typography>
          <Typography variant='body1' sx={{ color: '#6e6e73', fontSize: '1.125rem' }}>
            Cấu hình và quản lý cài đặt hệ thống UCode
          </Typography>
        </Box>

        {/* Coming Soon Notice */}
        <Alert
          severity='info'
          icon={<ConstructionIcon />}
          sx={{
            borderRadius: 3,
            mb: 3,
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
            Tính năng cài đặt hệ thống sẽ được triển khai trong phiên bản tiếp theo. Vui lòng quay lại sau!
          </Typography>
        </Alert>

        {/* General Settings */}
        <Card
          elevation={0}
          sx={{
            mb: 2,
            borderRadius: 3,
            bgcolor: 'white',
            border: '1px solid #d2d2d7',
            opacity: 0.6,
            pointerEvents: 'none',
          }}
        >
          <CardContent sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
              <SettingsIcon sx={{ color: '#007AFF', mr: 1.5, fontSize: 24 }} />
              <Typography variant='h6' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                Cài đặt chung
              </Typography>
            </Box>

            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              <Box>
                <TextField
                  label='Tên hệ thống'
                  defaultValue='UCode Learning Platform'
                  fullWidth
                  disabled
                  sx={{
                    '& .MuiOutlinedInput-root': {
                      borderRadius: 2,
                    },
                  }}
                />
              </Box>

              <Box>
                <TextField
                  label='Email hỗ trợ'
                  defaultValue='support@ucode.edu.vn'
                  fullWidth
                  disabled
                  sx={{
                    '& .MuiOutlinedInput-root': {
                      borderRadius: 2,
                    },
                  }}
                />
              </Box>

              <Box>
                <TextField
                  label='Số lượng sinh viên tối đa mỗi lớp'
                  defaultValue='50'
                  type='number'
                  fullWidth
                  disabled
                  sx={{
                    '& .MuiOutlinedInput-root': {
                      borderRadius: 2,
                    },
                  }}
                />
              </Box>

              <Divider />

              <Box>
                <FormControlLabel
                  control={<Switch defaultChecked disabled />}
                  label={
                    <Box>
                      <Typography variant='body1' sx={{ fontWeight: 600 }}>
                        Cho phép đăng ký tài khoản mới
                      </Typography>
                      <Typography variant='caption' color='text.secondary'>
                        Sinh viên có thể tự đăng ký tài khoản
                      </Typography>
                    </Box>
                  }
                />
              </Box>

              <Box>
                <FormControlLabel
                  control={<Switch defaultChecked disabled />}
                  label={
                    <Box>
                      <Typography variant='body1' sx={{ fontWeight: 600 }}>
                        Bật chế độ bảo trì
                      </Typography>
                      <Typography variant='caption' color='text.secondary'>
                        Chỉ admin có thể truy cập hệ thống
                      </Typography>
                    </Box>
                  }
                />
              </Box>
            </Box>
          </CardContent>
        </Card>

        {/* Security Settings */}
        <Card
          elevation={0}
          sx={{
            mb: 2,
            borderRadius: 3,
            bgcolor: 'white',
            border: '1px solid #d2d2d7',
            opacity: 0.6,
            pointerEvents: 'none',
          }}
        >
          <CardContent sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
              <SecurityIcon sx={{ color: '#FF3B30', mr: 1.5, fontSize: 24 }} />
              <Typography variant='h6' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                Bảo mật
              </Typography>
            </Box>

            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              <Box>
                <TextField
                  label='Thời gian hết hạn phiên (phút)'
                  defaultValue='60'
                  type='number'
                  fullWidth
                  sx={{
                    '& .MuiOutlinedInput-root': {
                      borderRadius: 2,
                    },
                  }}
                />
              </Box>

              <Box>
                <TextField
                  label='Số lần đăng nhập sai tối đa'
                  defaultValue='5'
                  type='number'
                  fullWidth
                  sx={{
                    '& .MuiOutlinedInput-root': {
                      borderRadius: 2,
                    },
                  }}
                />
              </Box>

              <Divider />

              <Box>
                <FormControlLabel
                  control={<Switch defaultChecked disabled />}
                  label={
                    <Box>
                      <Typography variant='body1' sx={{ fontWeight: 600 }}>
                        Bật xác thực hai yếu tố (2FA)
                      </Typography>
                      <Typography variant='caption' color='text.secondary'>
                        Yêu cầu mã xác thực khi đăng nhập
                      </Typography>
                    </Box>
                  }
                />
              </Box>

              <Box>
                <FormControlLabel
                  control={<Switch defaultChecked disabled />}
                  label={
                    <Box>
                      <Typography variant='body1' sx={{ fontWeight: 600 }}>
                        Ghi log hoạt động người dùng
                      </Typography>
                      <Typography variant='caption' color='text.secondary'>
                        Lưu lại tất cả hoạt động của người dùng
                      </Typography>
                    </Box>
                  }
                />
              </Box>
            </Box>
          </CardContent>
        </Card>

        {/* Email Settings */}
        <Card
          elevation={0}
          sx={{
            mb: 2,
            borderRadius: 3,
            bgcolor: 'white',
            border: '1px solid #d2d2d7',
            opacity: 0.6,
            pointerEvents: 'none',
          }}
        >
          <CardContent sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
              <EmailIcon sx={{ color: '#007AFF', mr: 1.5, fontSize: 24 }} />
              <Typography variant='h6' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                Email & Thông báo
              </Typography>
            </Box>

            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              <Box>
                <FormControlLabel
                  control={<Switch defaultChecked disabled />}
                  label={
                    <Box>
                      <Typography variant='body1' sx={{ fontWeight: 600 }}>
                        Gửi email thông báo bài tập mới
                      </Typography>
                      <Typography variant='caption' color='text.secondary'>
                        Tự động gửi email khi có bài tập mới
                      </Typography>
                    </Box>
                  }
                />
              </Box>

              <Box>
                <FormControlLabel
                  control={<Switch defaultChecked disabled />}
                  label={
                    <Box>
                      <Typography variant='body1' sx={{ fontWeight: 600 }}>
                        Nhắc nhở hạn nộp bài
                      </Typography>
                      <Typography variant='caption' color='text.secondary'>
                        Gửi email nhắc nhở trước hạn 24 giờ
                      </Typography>
                    </Box>
                  }
                />
              </Box>

              <Box>
                <FormControlLabel
                  control={<Switch disabled />}
                  label={
                    <Box>
                      <Typography variant='body1' sx={{ fontWeight: 600 }}>
                        Thông báo kết quả chấm bài
                      </Typography>
                      <Typography variant='caption' color='text.secondary'>
                        Gửi email khi bài được chấm xong
                      </Typography>
                    </Box>
                  }
                />
              </Box>
            </Box>
          </CardContent>
        </Card>

        {/* Storage Settings */}
        <Card
          elevation={0}
          sx={{
            mb: 3,
            borderRadius: 3,
            bgcolor: 'white',
            border: '1px solid #d2d2d7',
            opacity: 0.6,
            pointerEvents: 'none',
          }}
        >
          <CardContent sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
              <StorageIcon sx={{ color: '#FF9500', mr: 1.5, fontSize: 24 }} />
              <Typography variant='h6' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                Lưu trữ & Database
              </Typography>
            </Box>

            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              <Box
                sx={{
                  p: 3,
                  borderRadius: 2,
                  background: 'rgba(0, 0, 0, 0.02)',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                }}
              >
                <Box>
                  <Typography variant='body1' sx={{ fontWeight: 600, mb: 0.5 }}>
                    Dung lượng đã sử dụng
                  </Typography>
                  <Typography variant='caption' color='text.secondary'>
                    65% của tổng 500GB
                  </Typography>
                </Box>
                <Chip label='325 GB' sx={{ background: '#F59E0B15', color: '#F59E0B', fontWeight: 700 }} />
              </Box>

              <Box sx={{ display: 'flex', gap: 2 }}>
                <Button
                  variant='outlined'
                  startIcon={<RestartAltIcon />}
                  sx={{
                    borderRadius: 2,
                    textTransform: 'none',
                    fontWeight: 600,
                    flex: 1,
                  }}
                >
                  Dọn dẹp cache
                </Button>
                <Button
                  variant='outlined'
                  startIcon={<StorageIcon />}
                  sx={{
                    borderRadius: 2,
                    textTransform: 'none',
                    fontWeight: 600,
                    flex: 1,
                  }}
                >
                  Backup database
                </Button>
              </Box>
            </Box>
          </CardContent>
        </Card>

        {/* Action Buttons */}
        <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
          <Button
            disabled
            variant='outlined'
            sx={{
              borderRadius: 3,
              px: 4,
              py: 1.5,
              textTransform: 'none',
              fontWeight: 600,
            }}
          >
            Hủy thay đổi
          </Button>
          <Button
            disabled
            variant='contained'
            startIcon={<SaveIcon />}
            sx={{
              borderRadius: 2,
              px: 4,
              py: 1.5,
              bgcolor: '#007AFF',
              boxShadow: 'none',
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': {
                bgcolor: '#0051D5',
                boxShadow: 'none',
              },
            }}
          >
            Lưu cài đặt
          </Button>
        </Box>
      </Container>
    </Box>
  )
}
