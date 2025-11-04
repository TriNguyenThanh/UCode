import * as React from 'react'
import { Form, useActionData, useNavigation, redirect } from 'react-router'
import type { Route } from './+types/login'
import { auth } from '~/auth'
import {
  Button,
  TextField,
  Typography,
  Box,
  Alert,
  Avatar,
  Paper,
  CircularProgress,
  IconButton,
  InputAdornment,
} from '@mui/material'
import Visibility from '@mui/icons-material/Visibility'
import VisibilityOff from '@mui/icons-material/VisibilityOff'

export const meta: Route.MetaFunction = () => [
  { title: 'Login | UCode' },
  { name: 'description', content: 'Sign in to UCode dashboard.' },
]

export async function clientAction({ request }: Route.ClientActionArgs) {
  const form = await request.formData()
  const email = String(form.get('email') ?? '').trim()
  const password = String(form.get('password') ?? '')

  if (!email || !password) {
    return { error: 'Email and password are required.' } as const
  }

  try {
    const user = await auth.login(email, password)
    // Redirect based on role
    if (user.role === 'admin') {
      return redirect('/admin/home')
    } else if (user.role === 'teacher') {
      return redirect('/teacher/home')
    }
    return redirect('/home')
  } catch (e) {
    const message = e instanceof Error ? e.message : 'Login failed'
    return { error: message } as const
  }
}

export default function Login() {
  const data = useActionData<typeof clientAction>()
  const nav = useNavigation()
  const isSubmitting = nav.state === 'submitting'
  const [showPassword, setShowPassword] = React.useState<boolean>(false)

  const handleClickShowPassword = (): void => setShowPassword((show) => !show)
  const handleMouseDownPassword = (event: React.MouseEvent<HTMLButtonElement>): void => {
    event.preventDefault()
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        width: '100%',
        position: 'relative',
        backgroundImage: "url('/background.png')",
        backgroundSize: 'cover',
        backgroundRepeat: 'no-repeat',
        backgroundPosition: 'center',
        overflow: 'hidden',
        display: 'flex',
        alignItems: 'center',
      }}
    >
      {/* Lớp phủ mờ */}
      <Box sx={{ position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, bgcolor: 'rgba(0, 0, 0, 0.2)', zIndex: 1 }} />

      {/* Text bên trái */}
      <Box
        sx={{
          position: 'absolute',
          top: '50%',
          left: { xs: '5%', sm: '8%', md: '10%', lg: '15%' },
          transform: 'translateY(-50%)',
          color: 'common.white',
          textShadow: '1px 1px 4px rgba(0,0,0,0.8)',
          maxWidth: { xs: 'calc(90% - 50px)', sm: '45%', md: '40%' },
          zIndex: 2,
          display: { xs: 'none', md: 'block' },
        }}
      >
        <Typography variant='h6' component='div' sx={{ mb: 6, fontWeight: 500 }}>
          TRƯỜNG ĐẠI HỌC GIAO THÔNG VẬN TẢI <br /> PHÂN HIỆU TẠI TP. HỒ CHÍ MINH
        </Typography>
        <Typography variant='h2' component='h1' sx={{ fontWeight: 'bold', textTransform: 'uppercase' }}>
          Chào mừng
        </Typography>
        <Typography variant='h5' sx={{ mb: 1, opacity: 0.9 }}>
          Rất vui được gặp lại !
        </Typography>
      </Box>

      {/* Form Đăng nhập */}
      <Paper
        elevation={10}
        sx={{
          position: 'absolute',
          top: '50%',
          right: { xs: '50%', sm: '15%', md: '10%', lg: '10%' },
          transform: { xs: 'translate(50%, -50%)', sm: 'translateY(-50%)' },
          width: { xs: '90%', sm: '400px', md: '450px' },
          maxWidth: '450px',
          p: { xs: 3, sm: 4 },
          borderRadius: '12px',
          bgcolor: 'rgba(255, 255, 255, 0.9)',
          zIndex: 2,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Avatar src='/logo.png' alt='UCode' sx={{ m: 2, bgcolor: 'primary.main', width: 80, height: 80 }} />
        <Typography component='h1' variant='h4' sx={{ mb: 3, fontWeight: 'bold', color: 'text.primary' }}>
          ĐĂNG NHẬP
        </Typography>

        {/* Form */}
        <Box component={Form} method='post' replace sx={{ mt: 1, width: '100%' }}>
          <TextField
            margin='normal'
            required
            fullWidth
            id='email'
            label='Email / Tên đăng nhập'
            name='email'
            autoComplete='email'
            autoFocus
            error={!!data?.error}
            size='medium'
            color='primary'
          />
          <TextField
            margin='normal'
            required
            fullWidth
            name='password'
            label='Mật khẩu'
            type={showPassword ? 'text' : 'password'}
            id='password'
            autoComplete='current-password'
            error={!!data?.error}
            size='medium'
            color='primary'
            InputProps={{
              endAdornment: (
                <InputAdornment position='end'>
                  <IconButton
                    aria-label='toggle password visibility'
                    onClick={handleClickShowPassword}
                    onMouseDown={handleMouseDownPassword}
                    edge='end'
                  >
                    {showPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />

          {/* Hiển thị lỗi */}
          {data?.error && (
            <Alert severity='error' sx={{ width: '100%', mt: 1.5, mb: 1 }}>
              {data.error}
            </Alert>
          )}

          <Button
            type='submit'
            fullWidth
            variant='contained'
            sx={{ mt: 3, mb: 2, py: 1.5, fontSize: '1.1rem', fontWeight: 'bold', letterSpacing: '1px' }}
            disabled={isSubmitting}
          >
            {isSubmitting ? <CircularProgress size={24} color='inherit' /> : 'ĐĂNG NHẬP'}
          </Button>

          <Typography variant='body2' sx={{ mt: 2, textAlign: 'center', color: 'text.secondary' }}>
            Demo Admin: <code>admin@example.com</code> / <code>123456</code>
            <br />
            Demo Student: <code>student@example.com</code> / <code>123456</code>
            <br />
            Demo Teacher: <code>teacher@example.com</code> / <code>123456</code>
          </Typography>
        </Box>
      </Paper>
    </Box>
  )
}
