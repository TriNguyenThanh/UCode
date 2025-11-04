import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/admin.users'
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
  Avatar,
  IconButton,
  Menu,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import AddIcon from '@mui/icons-material/Add'
import MoreVertIcon from '@mui/icons-material/MoreVert'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import BlockIcon from '@mui/icons-material/Block'
import PeopleIcon from '@mui/icons-material/People'
import FilterListIcon from '@mui/icons-material/FilterList'

export const meta: Route.MetaFunction = () => [
  { title: 'Quản lý người dùng | Admin | UCode' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'admin') throw redirect('/home')

  // Mock users data
  const users = [
    { id: 1, name: 'Nguyễn Văn A', email: 'student1@utc2.edu.vn', role: 'student', status: 'active', joinDate: '2024-09-01', avatar: '👨' },
    { id: 2, name: 'Trần Thị B', email: 'student2@utc2.edu.vn', role: 'student', status: 'active', joinDate: '2024-09-01', avatar: '👩' },
    { id: 3, name: 'Phạm Văn C', email: 'teacher1@utc2.edu.vn', role: 'teacher', status: 'active', joinDate: '2024-08-15', avatar: '👨‍🏫' },
    { id: 4, name: 'Lê Thị D', email: 'teacher2@utc2.edu.vn', role: 'teacher', status: 'active', joinDate: '2024-08-15', avatar: '👩‍🏫' },
    { id: 5, name: 'Hoàng Văn E', email: 'student3@utc2.edu.vn', role: 'student', status: 'suspended', joinDate: '2024-09-05', avatar: '👨' },
    { id: 6, name: 'Đỗ Thị F', email: 'student4@utc2.edu.vn', role: 'student', status: 'active', joinDate: '2024-09-10', avatar: '👩' },
  ]

  return { user, users }
}

export default function AdminUsers() {
  const { users } = useLoaderData<typeof clientLoader>()
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null)
  const [selectedUser, setSelectedUser] = React.useState<number | null>(null)

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>, userId: number) => {
    setAnchorEl(event.currentTarget)
    setSelectedUser(userId)
  }

  const handleMenuClose = () => {
    setAnchorEl(null)
    setSelectedUser(null)
  }

  const getRoleColor = (role: string) => {
    switch (role) {
      case 'admin':
        return '#EF4444'
      case 'teacher':
        return '#8B5CF6'
      case 'student':
        return '#3B82F6'
      default:
        return '#6B7280'
    }
  }

  const getRoleLabel = (role: string) => {
    switch (role) {
      case 'admin':
        return 'Quản trị viên'
      case 'teacher':
        return 'Giảng viên'
      case 'student':
        return 'Sinh viên'
      default:
        return role
    }
  }

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
                Quản lý người dùng
              </Typography>
              <Typography variant='body1' sx={{ color: '#6e6e73', fontSize: '1.125rem' }}>
                Quản lý tài khoản sinh viên, giảng viên và quản trị viên
              </Typography>
            </Box>

            <Button
              variant='contained'
              startIcon={<AddIcon />}
              sx={{
                borderRadius: 2,
                px: 3,
                py: 1.5,
                bgcolor: '#007AFF',
                textTransform: 'none',
                fontWeight: 600,
                fontSize: '1rem',
                boxShadow: 'none',
                '&:hover': {
                  bgcolor: '#0051D5',
                  boxShadow: 'none',
                },
              }}
            >
              Thêm người dùng
            </Button>
          </Box>

          {/* Search and Filter */}
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              placeholder='Tìm kiếm theo tên hoặc email...'
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
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr 1fr' }, gap: 2, mb: 3 }}>
          {[
            { label: 'Tổng người dùng', value: users.length, color: '#007AFF' },
            { label: 'Đang hoạt động', value: users.filter((u) => u.status === 'active').length, color: '#34C759' },
            { label: 'Bị khóa', value: users.filter((u) => u.status === 'suspended').length, color: '#FF3B30' },
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
                <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', mb: 1, fontSize: '0.875rem' }}>
                  {stat.label}
                </Typography>
                <Typography variant='h3' sx={{ fontWeight: 600, color: stat.color }}>
                  {stat.value}
                </Typography>
              </CardContent>
            </Card>
          ))}
        </Box>

        {/* Users Table */}
        <Card
          elevation={0}
          sx={{
            borderRadius: 3,
            bgcolor: 'white',
            border: '1px solid #d2d2d7',
          }}
        >
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>Người dùng</TableCell>
                  <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>Email</TableCell>
                  <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>Vai trò</TableCell>
                  <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>Trạng thái</TableCell>
                  <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>Ngày tham gia</TableCell>
                  <TableCell align='right' sx={{ fontWeight: 700, fontSize: '0.875rem' }}>
                    Thao tác
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {users.map((user) => (
                  <TableRow
                    key={user.id}
                    sx={{
                      '&:hover': {
                        background: 'rgba(0, 0, 0, 0.02)',
                      },
                      transition: 'background 0.2s',
                    }}
                  >
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <Avatar sx={{ width: 36, height: 36, bgcolor: '#007AFF', fontSize: '1rem' }}>
                          {user.name.charAt(0)}
                        </Avatar>
                        <Typography variant='body2' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                          {user.name}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant='body2' color='text.secondary'>
                        {user.email}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={getRoleLabel(user.role)}
                        size='small'
                        sx={{
                          background: `${getRoleColor(user.role)}15`,
                          color: getRoleColor(user.role),
                          fontWeight: 600,
                          borderRadius: 2,
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={user.status === 'active' ? 'Hoạt động' : 'Bị khóa'}
                        size='small'
                        sx={{
                          background: user.status === 'active' ? '#10B98115' : '#EF444415',
                          color: user.status === 'active' ? '#10B981' : '#EF4444',
                          fontWeight: 600,
                          borderRadius: 2,
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant='body2' color='text.secondary'>
                        {new Date(user.joinDate).toLocaleDateString('vi-VN')}
                      </Typography>
                    </TableCell>
                    <TableCell align='right'>
                      <IconButton
                        size='small'
                        onClick={(e) => handleMenuOpen(e, user.id)}
                        sx={{
                          borderRadius: 2,
                          '&:hover': {
                            background: 'rgba(0, 0, 0, 0.04)',
                          },
                        }}
                      >
                        <MoreVertIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Card>

        {/* Context Menu */}
        <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={handleMenuClose}>
          <MenuItem onClick={handleMenuClose}>
            <EditIcon sx={{ mr: 1, fontSize: 20 }} />
            Chỉnh sửa
          </MenuItem>
          <MenuItem onClick={handleMenuClose}>
            <BlockIcon sx={{ mr: 1, fontSize: 20 }} />
            Khóa tài khoản
          </MenuItem>
          <MenuItem onClick={handleMenuClose} sx={{ color: 'error.main' }}>
            <DeleteIcon sx={{ mr: 1, fontSize: 20 }} />
            Xóa
          </MenuItem>
        </Menu>
      </Container>
    </Box>
  )
}
