import * as React from 'react'
import { Link, useNavigate } from 'react-router'
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  IconButton,
  Avatar,
  Menu,
  MenuItem,
  Box,
  Container,
} from '@mui/material'
import MenuIcon from '@mui/icons-material/Menu'
import AccountCircle from '@mui/icons-material/AccountCircle'
import { auth } from '~/auth'

export function Navigation() {
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null)
  const navigate = useNavigate()
  const user = auth.getUser()

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleLogout = () => {
    auth.logout()
    handleClose()
    navigate('/login')
  }

  return (
    <AppBar position='sticky' elevation={1} sx={{ bgcolor: 'secondary.main' }}>
      <Container maxWidth='xl'>
        <Toolbar disableGutters>
          {/* Logo */}
          <Typography
            variant='h6'
            component={Link}
            to={user?.role === 'admin' ? '/admin/home' : user?.role === 'teacher' ? '/teacher/home' : '/home'}
            sx={{
              mr: 4,
              fontWeight: 700,
              color: 'primary.main',
              textDecoration: 'none',
              display: 'flex',
              alignItems: 'center',
            }}
          >
            <Box component='span' sx={{ fontSize: '1.5rem', mr: 1 }}>
              💻
            </Box>
            UCode
          </Typography>

          {/* Navigation Links */}
          <Box sx={{ flexGrow: 1, display: { xs: 'none', md: 'flex' }, gap: 1 }}>
            {user?.role === 'admin' ? (
              <>
                <Button component={Link} to='/admin/home' sx={{ fontWeight: 500, color: 'primary.main' }}>
                  Dashboard
                </Button>
                <Button component={Link} to='/admin/users' sx={{ fontWeight: 500, color: 'primary.main' }}>
                  Người dùng
                </Button>
                <Button component={Link} to='/admin/classes' sx={{ fontWeight: 500, color: 'primary.main' }}>
                  Lớp học
                </Button>
                <Button component={Link} to='/admin/settings' sx={{ fontWeight: 500, color: 'primary.main' }}>
                  Cài đặt
                </Button>
                <Button component={Link} to='/admin/logs' sx={{ fontWeight: 500, color: 'primary.main' }}>
                  Logs
                </Button>
              </>
            ) : user?.role === 'teacher' ? (
              <>
                <Button component={Link} to='/teacher/home' sx={{ fontWeight: 500, color: 'primary.main' }}>
                  Dashboard
                </Button>
                <Button component={Link} to='/practice' sx={{ fontWeight: 500, color: 'primary.main' }}>
                  Ngân hàng bài
                </Button>
              </>
            ) : (
              <>
                <Button component={Link} to='/home' sx={{ fontWeight: 500, color: 'primary.main' }}>
                  Trang chủ
                </Button>
                <Button component={Link} to='/practice' sx={{ fontWeight: 500, color: 'primary.main' }}>
                  Luyện tập
                </Button>
              </>
            )}
          </Box>

          {/* User Menu */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography variant='body2' sx={{ display: { xs: 'none', sm: 'block' }, color: 'primary.main' }}>
              {user?.email}
            </Typography>
            <IconButton size='large' onClick={handleMenu} sx={{ color: 'primary.main' }}>
              <AccountCircle />
            </IconButton>
            <Menu
              anchorEl={anchorEl}
              anchorOrigin={{
                vertical: 'bottom',
                horizontal: 'right',
              }}
              keepMounted
              transformOrigin={{
                vertical: 'top',
                horizontal: 'right',
              }}
              open={Boolean(anchorEl)}
              onClose={handleClose}
            >
              <MenuItem onClick={handleClose}>
                <Typography variant='body2' color='text.secondary'>
                  {user?.email}
                </Typography>
              </MenuItem>
              <MenuItem component={Link} to='/profile' onClick={handleClose}>
                Hồ sơ
              </MenuItem>
              <MenuItem component={Link} to='/settings' onClick={handleClose}>
                Cài đặt
              </MenuItem>
              <MenuItem onClick={handleLogout}>Đăng xuất</MenuItem>
            </Menu>
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  )
}
