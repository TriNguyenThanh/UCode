import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/teacher.problems'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  CardActionArea,
  Chip,
  Button,
  TextField,
  InputAdornment,
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
import AddIcon from '@mui/icons-material/Add'
import SearchIcon from '@mui/icons-material/Search'
import FilterListIcon from '@mui/icons-material/FilterList'
import MoreVertIcon from '@mui/icons-material/MoreVert'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import VisibilityIcon from '@mui/icons-material/Visibility'
import CodeIcon from '@mui/icons-material/Code'
import { mockProblems } from '~/data/mock'

export const meta: Route.MetaFunction = () => [
  { title: 'Ngân hàng bài | UCode' },
  { name: 'description', content: 'Quản lý ngân hàng bài tập.' },
]

export async function clientLoader({ }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'teacher') throw redirect('/home')

  return { user, problems: mockProblems }
}

export default function TeacherProblems() {
  const { problems } = useLoaderData<typeof clientLoader>()
  const [searchQuery, setSearchQuery] = React.useState('')
  const [filterDifficulty, setFilterDifficulty] = React.useState<string>('all')
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null)
  const [selectedProblem, setSelectedProblem] = React.useState<string | null>(null)

  const handleMenuClick = (event: React.MouseEvent<HTMLElement>, problemId: string) => {
    setAnchorEl(event.currentTarget)
    setSelectedProblem(problemId)
  }

  const handleMenuClose = () => {
    setAnchorEl(null)
    setSelectedProblem(null)
  }

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy':
        return '#34C759'
      case 'Medium':
        return '#FF9500'
      case 'Hard':
        return '#FF3B30'
      default:
        return '#86868b'
    }
  }

  const filteredProblems = problems.filter((problem) => {
    const matchesSearch = problem.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      problem.description.toLowerCase().includes(searchQuery.toLowerCase())
    const matchesDifficulty = filterDifficulty === 'all' || problem.difficulty === filterDifficulty
    return matchesSearch && matchesDifficulty
  })

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Box>
              <Typography variant='h4' sx={{ fontWeight: 700, mb: 1, color: '#1d1d1f', display: 'flex', alignItems: 'center', gap: 1 }}>
                <CodeIcon sx={{ color: '#007AFF', fontSize: 36 }} />
                Ngân hàng bài
              </Typography>
              <Typography variant='body1' sx={{ color: '#86868b' }}>
                Quản lý và tạo bài tập cho sinh viên
              </Typography>
            </Box>
            <Button
              variant='contained'
              startIcon={<AddIcon />}
              component={Link}
              to='/teacher/problem/create'
              sx={{
                bgcolor: '#007AFF',
                color: '#ffffff',
                fontWeight: 600,
                px: 3,
                py: 1.5,
                borderRadius: 2,
                textTransform: 'none',
                fontSize: '1rem',
                '&:hover': {
                  bgcolor: '#0051D5',
                },
              }}
            >
              Tạo bài mới
            </Button>
          </Box>
        </Box>

        {/* Stats Cards */}
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr 1fr 1fr' }, gap: 2, mb: 4 }}>
          <Card elevation={0} sx={{ bgcolor: '#ffffff', border: '1px solid #d2d2d7' }}>
            <CardContent>
              <Typography variant='body2' sx={{ color: '#86868b', mb: 1 }}>
                Tổng số bài
              </Typography>
              <Typography variant='h4' sx={{ fontWeight: 700, color: '#1d1d1f' }}>
                {problems.length}
              </Typography>
            </CardContent>
          </Card>

          <Card elevation={0} sx={{ bgcolor: '#ffffff', border: '1px solid #d2d2d7' }}>
            <CardContent>
              <Typography variant='body2' sx={{ color: '#86868b', mb: 1 }}>
                Dễ
              </Typography>
              <Typography variant='h4' sx={{ fontWeight: 700, color: '#34C759' }}>
                {problems.filter(p => p.difficulty === 'Easy').length}
              </Typography>
            </CardContent>
          </Card>

          <Card elevation={0} sx={{ bgcolor: '#ffffff', border: '1px solid #d2d2d7' }}>
            <CardContent>
              <Typography variant='body2' sx={{ color: '#86868b', mb: 1 }}>
                Trung bình
              </Typography>
              <Typography variant='h4' sx={{ fontWeight: 700, color: '#FF9500' }}>
                {problems.filter(p => p.difficulty === 'Medium').length}
              </Typography>
            </CardContent>
          </Card>

          <Card elevation={0} sx={{ bgcolor: '#ffffff', border: '1px solid #d2d2d7' }}>
            <CardContent>
              <Typography variant='body2' sx={{ color: '#86868b', mb: 1 }}>
                Khó
              </Typography>
              <Typography variant='h4' sx={{ fontWeight: 700, color: '#FF3B30' }}>
                {problems.filter(p => p.difficulty === 'Hard').length}
              </Typography>
            </CardContent>
          </Card>
        </Box>

        {/* Filter Bar */}
        <Paper
          elevation={0}
          sx={{
            mb: 3,
            p: 2,
            bgcolor: '#ffffff',
            border: '1px solid #d2d2d7',
            borderRadius: 2,
          }}
        >
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <TextField
              placeholder='Tìm kiếm bài tập...'
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              sx={{ flexGrow: 1, minWidth: 300 }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position='start'>
                    <SearchIcon sx={{ color: '#86868b' }} />
                  </InputAdornment>
                ),
              }}
            />
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Chip
                label='Tất cả'
                onClick={() => setFilterDifficulty('all')}
                sx={{
                  bgcolor: filterDifficulty === 'all' ? '#007AFF' : '#ffffff',
                  color: filterDifficulty === 'all' ? '#ffffff' : '#1d1d1f',
                  border: '1px solid #d2d2d7',
                  fontWeight: 600,
                  cursor: 'pointer',
                }}
              />
              <Chip
                label='Dễ'
                onClick={() => setFilterDifficulty('Easy')}
                sx={{
                  bgcolor: filterDifficulty === 'Easy' ? '#34C759' : '#ffffff',
                  color: filterDifficulty === 'Easy' ? '#ffffff' : '#1d1d1f',
                  border: '1px solid #d2d2d7',
                  fontWeight: 600,
                  cursor: 'pointer',
                }}
              />
              <Chip
                label='Trung bình'
                onClick={() => setFilterDifficulty('Medium')}
                sx={{
                  bgcolor: filterDifficulty === 'Medium' ? '#FF9500' : '#ffffff',
                  color: filterDifficulty === 'Medium' ? '#ffffff' : '#1d1d1f',
                  border: '1px solid #d2d2d7',
                  fontWeight: 600,
                  cursor: 'pointer',
                }}
              />
              <Chip
                label='Khó'
                onClick={() => setFilterDifficulty('Hard')}
                sx={{
                  bgcolor: filterDifficulty === 'Hard' ? '#FF3B30' : '#ffffff',
                  color: filterDifficulty === 'Hard' ? '#ffffff' : '#1d1d1f',
                  border: '1px solid #d2d2d7',
                  fontWeight: 600,
                  cursor: 'pointer',
                }}
              />
            </Box>
          </Box>
        </Paper>

        {/* Problems Table */}
        <TableContainer
          component={Paper}
          elevation={0}
          sx={{
            bgcolor: '#ffffff',
            border: '1px solid #d2d2d7',
            borderRadius: 2,
          }}
        >
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600, color: '#1d1d1f' }}>Tiêu đề</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#1d1d1f' }}>Độ khó</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#1d1d1f' }}>Thể loại</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#1d1d1f' }}>Tags</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#1d1d1f' }}>Giới hạn</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#1d1d1f' }} align='right'>
                  Thao tác
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredProblems.map((problem) => (
                <TableRow
                  key={problem.id}
                  sx={{
                    '&:hover': {
                      bgcolor: '#f5f5f7',
                    },
                  }}
                >
                  <TableCell>
                    <Box>
                      <Typography variant='body1' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                        {problem.title}
                      </Typography>
                      <Typography variant='body2' sx={{ color: '#86868b', mt: 0.5 }}>
                        {problem.description.substring(0, 60)}...
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={problem.difficulty}
                      size='small'
                      sx={{
                        bgcolor: getDifficultyColor(problem.difficulty),
                        color: '#ffffff',
                        fontWeight: 600,
                      }}
                    />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={problem.category}
                      size='small'
                      variant='outlined'
                      sx={{ borderColor: '#d2d2d7', color: '#1d1d1f' }}
                    />
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                      {problem.tags.slice(0, 2).map((tag) => (
                        <Chip
                          key={tag}
                          label={tag}
                          size='small'
                          variant='outlined'
                          sx={{
                            borderColor: '#d2d2d7',
                            color: '#86868b',
                            borderStyle: 'dashed',
                          }}
                        />
                      ))}
                      {problem.tags.length > 2 && (
                        <Chip
                          label={`+${problem.tags.length - 2}`}
                          size='small'
                          sx={{ bgcolor: '#f5f5f7', color: '#86868b' }}
                        />
                      )}
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Typography variant='body2' sx={{ color: '#1d1d1f', fontWeight: 600 }}>
                      {problem.timeLimit}s / {problem.memoryLimit}MB
                    </Typography>
                  </TableCell>
                  <TableCell align='right'>
                    <IconButton
                      size='small'
                      onClick={(e) => handleMenuClick(e, problem.id)}
                      sx={{ color: '#86868b' }}
                    >
                      <MoreVertIcon />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {/* Action Menu */}
        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={handleMenuClose}
          PaperProps={{
            sx: {
              border: '1px solid #d2d2d7',
              borderRadius: 2,
            },
          }}
        >
          <MenuItem
            component={Link}
            to={`/problem/${selectedProblem}`}
            onClick={handleMenuClose}
          >
            <VisibilityIcon sx={{ mr: 1, fontSize: 20, color: '#007AFF' }} />
            Xem chi tiết
          </MenuItem>
          <MenuItem
            component={Link}
            to={`/teacher/problem/${selectedProblem}/edit`}
            onClick={handleMenuClose}
          >
            <EditIcon sx={{ mr: 1, fontSize: 20, color: '#FF9500' }} />
            Chỉnh sửa
          </MenuItem>
          <MenuItem onClick={handleMenuClose}>
            <DeleteIcon sx={{ mr: 1, fontSize: 20, color: '#FF3B30' }} />
            Xóa
          </MenuItem>
        </Menu>

        {/* Empty State */}
        {filteredProblems.length === 0 && (
          <Box
            sx={{
              textAlign: 'center',
              py: 8,
              bgcolor: '#ffffff',
              border: '1px solid #d2d2d7',
              borderRadius: 2,
              mt: 3,
            }}
          >
            <CodeIcon sx={{ fontSize: 64, color: '#d2d2d7', mb: 2 }} />
            <Typography variant='h6' sx={{ color: '#86868b', mb: 1 }}>
              Không tìm thấy bài tập
            </Typography>
            <Typography variant='body2' sx={{ color: '#86868b' }}>
              Thử thay đổi bộ lọc hoặc tìm kiếm
            </Typography>
          </Box>
        )}
      </Container>
    </Box>
  )
}
