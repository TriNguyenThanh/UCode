import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/admin.classes'
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
  IconButton,
  Menu,
  MenuItem,
  CardActionArea,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import AddIcon from '@mui/icons-material/Add'
import MoreVertIcon from '@mui/icons-material/MoreVert'
import ClassIcon from '@mui/icons-material/Class'
import PeopleIcon from '@mui/icons-material/People'
import AssignmentIcon from '@mui/icons-material/Assignment'
import FilterListIcon from '@mui/icons-material/FilterList'
import VisibilityIcon from '@mui/icons-material/Visibility'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'

export const meta: Route.MetaFunction = () => [
  { title: 'Quản lý lớp học | Admin | UCode' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'admin') throw redirect('/home')

  // Mock classes data
  const classes = [
    {
      id: 1,
      code: 'COMP101',
      name: 'Nhập môn lập trình',
      teacher: 'Phạm Văn C',
      semester: 'HK1 2024-2025',
      students: 28,
      assignments: 5,
      status: 'active',
      color: '#3B82F6',
    },
    {
      id: 2,
      code: 'COMP201',
      name: 'Cấu trúc dữ liệu và giải thuật',
      teacher: 'Lê Thị D',
      semester: 'HK1 2024-2025',
      students: 32,
      assignments: 4,
      status: 'active',
      color: '#8B5CF6',
    },
    {
      id: 3,
      code: 'COMP301',
      name: 'Lập trình hướng đối tượng',
      teacher: 'Phạm Văn C',
      semester: 'HK1 2024-2025',
      students: 25,
      assignments: 6,
      status: 'active',
      color: '#10B981',
    },
    {
      id: 4,
      code: 'COMP202',
      name: 'Cơ sở dữ liệu',
      teacher: 'Lê Thị D',
      semester: 'HK1 2024-2025',
      students: 30,
      assignments: 3,
      status: 'active',
      color: '#F59E0B',
    },
  ]

  return { user, classes }
}

export default function AdminClasses() {
  const { classes } = useLoaderData<typeof clientLoader>()
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null)
  const [selectedClass, setSelectedClass] = React.useState<number | null>(null)

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>, classId: number) => {
    event.preventDefault()
    event.stopPropagation()
    setAnchorEl(event.currentTarget)
    setSelectedClass(classId)
  }

  const handleMenuClose = () => {
    setAnchorEl(null)
    setSelectedClass(null)
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
                Quản lý lớp học
              </Typography>
              <Typography variant='body1' sx={{ color: '#6e6e73', fontSize: '1.125rem' }}>
                Quản lý tất cả các lớp học trong hệ thống
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
              Tạo lớp học
            </Button>
          </Box>

          {/* Search and Filter */}
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              placeholder='Tìm kiếm lớp học...'
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
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr 1fr 1fr' }, gap: 2, mb: 3 }}>
          {[
            { label: 'Tổng lớp học', value: classes.length, color: '#AF52DE', icon: ClassIcon },
            { label: 'Tổng sinh viên', value: classes.reduce((sum, c) => sum + c.students, 0), color: '#007AFF', icon: PeopleIcon },
            { label: 'Tổng bài tập', value: classes.reduce((sum, c) => sum + c.assignments, 0), color: '#34C759', icon: AssignmentIcon },
            { label: 'Trung bình SV/Lớp', value: Math.round(classes.reduce((sum, c) => sum + c.students, 0) / classes.length), color: '#FF9500', icon: PeopleIcon },
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

        {/* Classes Grid */}
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', lg: '1fr 1fr 1fr' }, gap: 2 }}>
          {classes.map((classItem) => (
            <Card
              key={classItem.id}
              elevation={0}
              sx={{
                borderRadius: 3,
                bgcolor: 'white',
                border: '1px solid #d2d2d7',
                transition: 'all 0.2s ease',
                '&:hover': {
                  transform: 'translateY(-2px)',
                  boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                },
              }}
            >
              <CardActionArea component={Link} to={`/class/${classItem.id}`}>
                {/* Header */}
                <Box
                  sx={{
                    height: 100,
                    bgcolor: classItem.color,
                    position: 'relative',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'space-between',
                    p: 3,
                  }}
                >
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <Typography variant='h5' sx={{ color: 'white', fontWeight: 700 }}>
                      {classItem.code}
                    </Typography>
                    <IconButton
                      size='small'
                      onClick={(e) => handleMenuOpen(e, classItem.id)}
                      sx={{
                        color: 'white',
                        '&:hover': {
                          bgcolor: 'rgba(255, 255, 255, 0.2)',
                        },
                      }}
                    >
                      <MoreVertIcon />
                    </IconButton>
                  </Box>
                  <Chip
                    label={classItem.semester}
                    size='small'
                    sx={{
                      bgcolor: 'rgba(255, 255, 255, 0.25)',
                      color: 'white',
                      fontWeight: 600,
                      alignSelf: 'flex-start',
                      border: 'none',
                    }}
                  />
                </Box>

                {/* Content */}
                <CardContent sx={{ p: 3 }}>
                  <Typography variant='h6' sx={{ fontWeight: 600, mb: 1, color: '#1d1d1f' }}>
                    {classItem.name}
                  </Typography>
                  <Typography variant='body2' sx={{ color: '#86868b', mb: 3 }}>
                    Giảng viên: {classItem.teacher}
                  </Typography>

                  {/* Stats */}
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Chip
                      icon={<PeopleIcon />}
                      label={`${classItem.students} SV`}
                      size='small'
                      sx={{
                        bgcolor: '#f5f5f7',
                        color: '#1d1d1f',
                        fontWeight: 600,
                        borderRadius: 2,
                        border: '1px solid #d2d2d7',
                      }}
                    />
                    <Chip
                      icon={<AssignmentIcon />}
                      label={`${classItem.assignments} bài tập`}
                      size='small'
                      sx={{
                        bgcolor: '#f5f5f7',
                        color: '#1d1d1f',
                        fontWeight: 600,
                        borderRadius: 2,
                        border: '1px solid #d2d2d7',
                      }}
                    />
                  </Box>
                </CardContent>
              </CardActionArea>
            </Card>
          ))}
        </Box>

        {/* Context Menu */}
        <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={handleMenuClose}>
          <MenuItem onClick={handleMenuClose}>
            <VisibilityIcon sx={{ mr: 1, fontSize: 20 }} />
            Xem chi tiết
          </MenuItem>
          <MenuItem onClick={handleMenuClose}>
            <EditIcon sx={{ mr: 1, fontSize: 20 }} />
            Chỉnh sửa
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
