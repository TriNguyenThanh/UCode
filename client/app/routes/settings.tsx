import * as React from 'react'
import { useLoaderData, redirect } from 'react-router'
import type { Route } from './+types/settings'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  Container,
  Typography,
  Box,
  Paper,
  TextField,
  Button,
  Divider,
  Switch,
  FormControlLabel,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Alert,
} from '@mui/material'
import SaveIcon from '@mui/icons-material/Save'
import LockIcon from '@mui/icons-material/Lock'
import NotificationsIcon from '@mui/icons-material/Notifications'
import LanguageIcon from '@mui/icons-material/Language'
import PaletteIcon from '@mui/icons-material/Palette'

export const meta: Route.MetaFunction = () => [
  { title: 'Cài đặt | UCode' },
  { name: 'description', content: 'Cài đặt tài khoản và ứng dụng.' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  return { user }
}

export default function Settings() {
  const { user } = useLoaderData<typeof clientLoader>()
  
  // States
  const [currentPassword, setCurrentPassword] = React.useState('')
  const [newPassword, setNewPassword] = React.useState('')
  const [confirmPassword, setConfirmPassword] = React.useState('')
  const [emailNotifications, setEmailNotifications] = React.useState(true)
  const [pushNotifications, setPushNotifications] = React.useState(true)
  const [assignmentReminders, setAssignmentReminders] = React.useState(true)
  const [language, setLanguage] = React.useState('vi')
  const [theme, setTheme] = React.useState('light')
  const [saveSuccess, setSaveSuccess] = React.useState(false)

  const handlePasswordChange = (e: React.FormEvent) => {
    e.preventDefault()
    // TODO: Implement password change logic
    console.log('Password change:', { currentPassword, newPassword, confirmPassword })
    setSaveSuccess(true)
    setTimeout(() => setSaveSuccess(false), 3000)
  }

  const handleNotificationSave = () => {
    // TODO: Implement notification settings save
    console.log('Notification settings:', { emailNotifications, pushNotifications, assignmentReminders })
    setSaveSuccess(true)
    setTimeout(() => setSaveSuccess(false), 3000)
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />

      <Container maxWidth='md' sx={{ py: 4 }}>
        {/* Header */}
        <Typography variant='h4' sx={{ fontWeight: 700, mb: 4 }}>
          Cài đặt
        </Typography>

        {/* Success Alert */}
        {saveSuccess && (
          <Alert severity='success' sx={{ mb: 3 }}>
            Đã lưu thay đổi thành công!
          </Alert>
        )}

        {/* Account Info Section */}
        <Paper elevation={0} sx={{ p: 3, mb: 3, border: '1px solid', borderColor: 'divider' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
            <LockIcon sx={{ color: 'primary.main' }} />
            <Typography variant='h6' sx={{ fontWeight: 600 }}>
              Thông tin tài khoản
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              label='Email'
              value={user.email}
              disabled
              fullWidth
              helperText='Email không thể thay đổi'
            />
            <TextField
              label='Họ và tên'
              defaultValue='Nguyễn Văn A'
              fullWidth
            />
            <TextField
              label='Mã số sinh viên'
              defaultValue='UTC2123456'
              fullWidth
            />
            <Button
              variant='contained'
              startIcon={<SaveIcon />}
              sx={{ 
                bgcolor: 'primary.main', 
                color: 'secondary.main',
                fontWeight: 600,
                alignSelf: 'flex-start'
              }}
            >
              Lưu thay đổi
            </Button>
          </Box>
        </Paper>

        {/* Change Password Section */}
        <Paper elevation={0} sx={{ p: 3, mb: 3, border: '1px solid', borderColor: 'divider' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
            <LockIcon sx={{ color: 'primary.main' }} />
            <Typography variant='h6' sx={{ fontWeight: 600 }}>
              Đổi mật khẩu
            </Typography>
          </Box>

          <form onSubmit={handlePasswordChange}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField
                label='Mật khẩu hiện tại'
                type='password'
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                fullWidth
                required
              />
              <TextField
                label='Mật khẩu mới'
                type='password'
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                fullWidth
                required
                helperText='Tối thiểu 8 ký tự'
              />
              <TextField
                label='Xác nhận mật khẩu mới'
                type='password'
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                fullWidth
                required
                error={confirmPassword !== '' && newPassword !== confirmPassword}
                helperText={
                  confirmPassword !== '' && newPassword !== confirmPassword
                    ? 'Mật khẩu không khớp'
                    : ''
                }
              />
              <Button
                type='submit'
                variant='contained'
                startIcon={<SaveIcon />}
                disabled={newPassword !== confirmPassword || newPassword.length < 8}
                sx={{ 
                  bgcolor: 'primary.main', 
                  color: 'secondary.main',
                  fontWeight: 600,
                  alignSelf: 'flex-start'
                }}
              >
                Đổi mật khẩu
              </Button>
            </Box>
          </form>
        </Paper>

        <Divider sx={{ my: 4 }} />

        {/* Notification Settings */}
        <Paper elevation={0} sx={{ p: 3, mb: 3, border: '1px solid', borderColor: 'divider' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
            <NotificationsIcon sx={{ color: 'primary.main' }} />
            <Typography variant='h6' sx={{ fontWeight: 600 }}>
              Thông báo
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={emailNotifications}
                  onChange={(e) => setEmailNotifications(e.target.checked)}
                  sx={{
                    '& .MuiSwitch-switchBase.Mui-checked': {
                      color: 'primary.main',
                    },
                    '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                      bgcolor: 'primary.main',
                    },
                  }}
                />
              }
              label='Nhận thông báo qua Email'
            />
            <FormControlLabel
              control={
                <Switch
                  checked={pushNotifications}
                  onChange={(e) => setPushNotifications(e.target.checked)}
                  sx={{
                    '& .MuiSwitch-switchBase.Mui-checked': {
                      color: 'primary.main',
                    },
                    '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                      bgcolor: 'primary.main',
                    },
                  }}
                />
              }
              label='Nhận thông báo đẩy'
            />
            <FormControlLabel
              control={
                <Switch
                  checked={assignmentReminders}
                  onChange={(e) => setAssignmentReminders(e.target.checked)}
                  sx={{
                    '& .MuiSwitch-switchBase.Mui-checked': {
                      color: 'primary.main',
                    },
                    '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                      bgcolor: 'primary.main',
                    },
                  }}
                />
              }
              label='Nhắc nhở về bài tập sắp đến hạn'
            />
            <Button
              variant='contained'
              startIcon={<SaveIcon />}
              onClick={handleNotificationSave}
              sx={{ 
                bgcolor: 'primary.main', 
                color: 'secondary.main',
                fontWeight: 600,
                alignSelf: 'flex-start',
                mt: 1
              }}
            >
              Lưu cài đặt
            </Button>
          </Box>
        </Paper>

        {/* Appearance Settings */}
        <Paper elevation={0} sx={{ p: 3, mb: 3, border: '1px solid', borderColor: 'divider' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
            <PaletteIcon sx={{ color: 'primary.main' }} />
            <Typography variant='h6' sx={{ fontWeight: 600 }}>
              Giao diện
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            <FormControl fullWidth>
              <InputLabel>Theme</InputLabel>
              <Select
                value={theme}
                label='Theme'
                onChange={(e) => setTheme(e.target.value)}
              >
                <MenuItem value='light'>Sáng</MenuItem>
                <MenuItem value='dark'>Tối</MenuItem>
                <MenuItem value='auto'>Tự động</MenuItem>
              </Select>
            </FormControl>

            <FormControl fullWidth>
              <InputLabel>Ngôn ngữ</InputLabel>
              <Select
                value={language}
                label='Ngôn ngữ'
                onChange={(e) => setLanguage(e.target.value)}
              >
                <MenuItem value='vi'>Tiếng Việt</MenuItem>
                <MenuItem value='en'>English</MenuItem>
              </Select>
            </FormControl>

            <Button
              variant='contained'
              startIcon={<SaveIcon />}
              sx={{ 
                bgcolor: 'primary.main', 
                color: 'secondary.main',
                fontWeight: 600,
                alignSelf: 'flex-start'
              }}
            >
              Lưu cài đặt
            </Button>
          </Box>
        </Paper>

        {/* Danger Zone */}
        <Paper 
          elevation={0} 
          sx={{ 
            p: 3, 
            border: '2px solid', 
            borderColor: 'error.main',
            bgcolor: 'error.lighter'
          }}
        >
          <Typography variant='h6' sx={{ fontWeight: 600, mb: 2, color: 'error.main' }}>
            Vùng nguy hiểm
          </Typography>
          <Typography variant='body2' color='text.secondary' sx={{ mb: 2 }}>
            Các hành động trong phần này không thể hoàn tác.
          </Typography>
          <Button
            variant='outlined'
            color='error'
            sx={{ fontWeight: 600 }}
          >
            Xóa tài khoản
          </Button>
        </Paper>
      </Container>
    </Box>
  )
}
