import * as React from 'react'
import { useLoaderData, redirect, Link, useNavigate, useRevalidator } from 'react-router'
import type { Route } from './+types/teacher.problems'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  Chip,
  Button,
  TextField,
  InputAdornment,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  Snackbar,
  CircularProgress,
  Pagination,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Divider,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import SearchIcon from '@mui/icons-material/Search'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import VisibilityIcon from '@mui/icons-material/Visibility'
import CodeIcon from '@mui/icons-material/Code'
import LanguageIcon from '@mui/icons-material/Language'
import DatasetIcon from '@mui/icons-material/Dataset'
import CloseIcon from '@mui/icons-material/Close'
import { getMyProblems, deleteProblem, getAvailableLanguagesForProblem, addOrUpdateProblemLanguages, deleteProblemLanguage } from '~/services/problemService'
import { getDatasetsByProblem, createDataset, updateDataset, deleteDataset } from '~/services/datasetService'
import { getAllLanguages } from '~/services/languageService'
import type { Problem, Language, Dataset, ProblemLanguage, Difficulty } from '~/types'

export const meta: Route.MetaFunction = () => [
  { title: 'Ngân hàng bài | UCode' },
  { name: 'description', content: 'Quản lý ngân hàng bài tập.' },
]

export async function clientLoader({ request }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'teacher' && user.role !== 'admin') throw redirect('/home')

  const url = new URL(request.url)
  const page = parseInt(url.searchParams.get('page') || '1')
  const pageSize = parseInt(url.searchParams.get('pageSize') || '20')

  try {
    const problemsData = await getMyProblems(page, pageSize)
    const languages = await getAllLanguages(false)
    
    return { 
      user, 
      problemsData,
      languages,
      currentPage: page,
      pageSize
    }
  } catch (error: any) {
    console.error('Failed to load problems:', error)
    return { 
      user, 
      problemsData: { data: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0, hasPrevious: false, hasNext: false },
      languages: [],
      currentPage: 1,
      pageSize: 20
    }
  }
}

export default function TeacherProblems() {
  const { problemsData, languages, currentPage, pageSize } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const revalidator = useRevalidator()
  
  const [searchQuery, setSearchQuery] = React.useState('')
  const [filterDifficulty, setFilterDifficulty] = React.useState<string>('all')
  const [selectedProblem, setSelectedProblem] = React.useState<Problem | null>(null)
  
  // Delete dialog
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false)
  const [deleting, setDeleting] = React.useState(false)
  
  // Language dialog
  const [languageDialogOpen, setLanguageDialogOpen] = React.useState(false)
  const [problemLanguages, setProblemLanguages] = React.useState<ProblemLanguage[]>([])
  const [loadingLanguages, setLoadingLanguages] = React.useState(false)
  
  // Dataset dialog
  const [datasetDialogOpen, setDatasetDialogOpen] = React.useState(false)
  const [datasets, setDatasets] = React.useState<Dataset[]>([])
  const [loadingDatasets, setLoadingDatasets] = React.useState(false)
  
  // Snackbar
  const [snackbar, setSnackbar] = React.useState<{open: boolean, message: string, severity: 'success' | 'error'}>({
    open: false,
    message: '',
    severity: 'success'
  })

  const handleDelete = async () => {
    if (!selectedProblem) return
    
    setDeleting(true)
    try {
      await deleteProblem(selectedProblem.problemId)
      setSnackbar({ open: true, message: 'Đã xóa bài tập thành công', severity: 'success' })
      setDeleteDialogOpen(false)
      setSelectedProblem(null)
      revalidator.revalidate()
    } catch (error: any) {
      setSnackbar({ open: true, message: error.message || 'Không thể xóa bài tập', severity: 'error' })
    } finally {
      setDeleting(false)
    }
  }

  const handleOpenLanguages = async (problem: Problem) => {
    setLoadingLanguages(true)
    setLanguageDialogOpen(true)
    setSelectedProblem(problem)
    
    try {
      const langs = await getAvailableLanguagesForProblem(problem.problemId)
      setProblemLanguages(langs)
    } catch (error: any) {
      setSnackbar({ open: true, message: error.message || 'Không thể tải languages', severity: 'error' })
    } finally {
      setLoadingLanguages(false)
    }
  }

  const handleOpenDatasets = async (problem: Problem) => {
    setLoadingDatasets(true)
    setDatasetDialogOpen(true)
    setSelectedProblem(problem)
    
    try {
      const ds = await getDatasetsByProblem(problem.problemId)
      setDatasets(ds)
    } catch (error: any) {
      setSnackbar({ open: true, message: error.message || 'Không thể tải datasets', severity: 'error' })
    } finally {
      setLoadingDatasets(false)
    }
  }

  const handlePageChange = (_event: React.ChangeEvent<unknown>, page: number) => {
    navigate(`?page=${page}&pageSize=${pageSize}`)
  }

  const getDifficultyColor = (difficulty: Difficulty) => {
    switch (difficulty) {
      case 'EASY':
        return '#34C759'
      case 'MEDIUM':
        return '#FF9500'
      case 'HARD':
        return '#FF3B30'
      default:
        return '#86868b'
    }
  }

  const getDifficultyLabel = (difficulty: Difficulty) => {
    switch (difficulty) {
      case 'EASY': return 'Dễ'
      case 'MEDIUM': return 'Trung bình'
      case 'HARD': return 'Khó'
      default: return difficulty
    }
  }

  const filteredProblems = (problemsData.data || []).filter((problem: Problem) => {
    const matchesSearch = problem.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (problem.statement && problem.statement.toLowerCase().includes(searchQuery.toLowerCase()))
    const matchesDifficulty = filterDifficulty === 'all' || problem.difficulty === filterDifficulty
    return matchesSearch && matchesDifficulty
  })

  const stats = {
    total: problemsData.totalCount || 0,
    easy: (problemsData.data || []).filter((p: Problem) => p.difficulty === 'EASY').length,
    medium: (problemsData.data || []).filter((p: Problem) => p.difficulty === 'MEDIUM').length,
    hard: (problemsData.data || []).filter((p: Problem) => p.difficulty === 'HARD').length,
  }

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
                {stats.total}
              </Typography>
            </CardContent>
          </Card>

          <Card elevation={0} sx={{ bgcolor: '#ffffff', border: '1px solid #d2d2d7' }}>
            <CardContent>
              <Typography variant='body2' sx={{ color: '#86868b', mb: 1 }}>
                Dễ
              </Typography>
              <Typography variant='h4' sx={{ fontWeight: 700, color: '#34C759' }}>
                {stats.easy}
              </Typography>
            </CardContent>
          </Card>

          <Card elevation={0} sx={{ bgcolor: '#ffffff', border: '1px solid #d2d2d7' }}>
            <CardContent>
              <Typography variant='body2' sx={{ color: '#86868b', mb: 1 }}>
                Trung bình
              </Typography>
              <Typography variant='h4' sx={{ fontWeight: 700, color: '#FF9500' }}>
                {stats.medium}
              </Typography>
            </CardContent>
          </Card>

          <Card elevation={0} sx={{ bgcolor: '#ffffff', border: '1px solid #d2d2d7' }}>
            <CardContent>
              <Typography variant='body2' sx={{ color: '#86868b', mb: 1 }}>
                Khó
              </Typography>
              <Typography variant='h4' sx={{ fontWeight: 700, color: '#FF3B30' }}>
                {stats.hard}
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
                onClick={() => setFilterDifficulty('EASY')}
                sx={{
                  bgcolor: filterDifficulty === 'EASY' ? '#34C759' : '#ffffff',
                  color: filterDifficulty === 'EASY' ? '#ffffff' : '#1d1d1f',
                  border: '1px solid #d2d2d7',
                  fontWeight: 600,
                  cursor: 'pointer',
                }}
              />
              <Chip
                label='Trung bình'
                onClick={() => setFilterDifficulty('MEDIUM')}
                sx={{
                  bgcolor: filterDifficulty === 'MEDIUM' ? '#FF9500' : '#ffffff',
                  color: filterDifficulty === 'MEDIUM' ? '#ffffff' : '#1d1d1f',
                  border: '1px solid #d2d2d7',
                  fontWeight: 600,
                  cursor: 'pointer',
                }}
              />
              <Chip
                label='Khó'
                onClick={() => setFilterDifficulty('HARD')}
                sx={{
                  bgcolor: filterDifficulty === 'HARD' ? '#FF3B30' : '#ffffff',
                  color: filterDifficulty === 'HARD' ? '#ffffff' : '#1d1d1f',
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
                <TableCell sx={{ fontWeight: 600, color: '#1d1d1f' }}>Mã / Tiêu đề</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#1d1d1f' }}>Độ khó</TableCell>
                <TableCell sx={{ fontWeight: 600, color: '#1d1d1f' }}>Visibility</TableCell>
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
                  key={problem.problemId}
                  sx={{
                    '&:hover': {
                      bgcolor: '#f5f5f7',
                    },
                  }}
                >
                  <TableCell>
                    <Box>
                      <Typography variant='body2' sx={{ color: '#86868b', fontSize: '0.75rem' }}>
                        #{problem.code}
                      </Typography>
                      <Typography 
                        variant='body1' 
                        component={Link}
                        to={`/problem/${problem.problemId}`}
                        sx={{ 
                          fontWeight: 600, 
                          color: '#007AFF',
                          textDecoration: 'none',
                          cursor: 'pointer',
                          '&:hover': {
                            textDecoration: 'underline',
                          }
                        }}
                      >
                        {problem.title}
                      </Typography>
                      {problem.statement && (
                        <Typography variant='body2' sx={{ color: '#86868b', mt: 0.5 }}>
                          {problem.statement.substring(0, 60)}...
                        </Typography>
                      )}
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={getDifficultyLabel(problem.difficulty)}
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
                      label={problem.visibility}
                      size='small'
                      variant='outlined'
                      color={problem.visibility === 'PUBLIC' ? 'success' : 'default'}
                      sx={{ borderColor: '#d2d2d7' }}
                    />
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                      {problem.tagNames && problem.tagNames.slice(0, 2).map((tag) => (
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
                      {problem.tagNames && problem.tagNames.length > 2 && (
                        <Chip
                          label={`+${problem.tagNames.length - 2}`}
                          size='small'
                          sx={{ bgcolor: '#f5f5f7', color: '#86868b' }}
                        />
                      )}
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Typography variant='body2' sx={{ color: '#1d1d1f', fontWeight: 600 }}>
                      {problem.timeLimitMs}ms / {problem.memoryLimitKb}KB
                    </Typography>
                  </TableCell>
                  <TableCell align='right'>
                    <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
                      <IconButton
                        size='small'
                        component={Link}
                        to={`/teacher/problem/${problem.problemId}/edit`}
                        sx={{ 
                          color: '#007AFF',
                          '&:hover': {
                            bgcolor: 'rgba(0, 122, 255, 0.1)',
                          }
                        }}
                        title="Chỉnh sửa"
                      >
                        <EditIcon fontSize='small' />
                      </IconButton>
                      <IconButton
                        size='small'
                        onClick={(e) => {
                          e.stopPropagation()
                          setSelectedProblem(problem)
                          setDeleteDialogOpen(true)
                        }}
                        sx={{ 
                          color: '#FF3B30',
                          '&:hover': {
                            bgcolor: 'rgba(255, 59, 48, 0.1)',
                          }
                        }}
                        title="Xóa"
                      >
                        <DeleteIcon fontSize='small' />
                      </IconButton>
                    </Box>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {/* Pagination */}
        {problemsData.totalPages > 1 && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <Pagination 
              count={problemsData.totalPages} 
              page={currentPage} 
              onChange={handlePageChange}
              color="primary"
              size="large"
            />
          </Box>
        )}

        {/* Action Menu */}
        {/* Menu removed - actions now visible as buttons in table */}

        {/* Delete Dialog */}
        <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
          <DialogTitle>Xác nhận xóa</DialogTitle>
          <DialogContent>
            <Typography>
              Bạn có chắc chắn muốn xóa bài tập <strong>{selectedProblem?.title}</strong>? 
              Hành động này không thể hoàn tác.
            </Typography>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDeleteDialogOpen(false)} disabled={deleting}>
              Hủy
            </Button>
            <Button 
              onClick={handleDelete} 
              color="error" 
              variant="contained"
              disabled={deleting}
              startIcon={deleting ? <CircularProgress size={20} /> : <DeleteIcon />}
            >
              {deleting ? 'Đang xóa...' : 'Xóa'}
            </Button>
          </DialogActions>
        </Dialog>

        {/* Language Dialog */}
        <Dialog 
          open={languageDialogOpen} 
          onClose={() => setLanguageDialogOpen(false)}
          maxWidth="md"
          fullWidth
        >
          <DialogTitle>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <LanguageIcon />
                Quản lý Languages - {selectedProblem?.title}
              </Box>
              <IconButton onClick={() => setLanguageDialogOpen(false)}>
                <CloseIcon />
              </IconButton>
            </Box>
          </DialogTitle>
          <DialogContent>
            {loadingLanguages ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                <CircularProgress />
              </Box>
            ) : (
              <>
                <Alert severity="info" sx={{ mb: 2 }}>
                  Danh sách các ngôn ngữ được phép sử dụng cho bài tập này. 
                  Bạn có thể điều chỉnh time/memory factor cho từng ngôn ngữ.
                </Alert>
                <List>
                  {problemLanguages.map((pl) => (
                    <ListItem key={pl.languageId}>
                      <ListItemText
                        primary={pl.languageDisplayName || pl.languageCode}
                        secondary={`Time Factor: ${pl.timeFactor || 1.0}x | Memory: ${pl.memoryKb || 'Default'} KB | Allowed: ${pl.isAllowed ? 'Yes' : 'No'}`}
                      />
                      <ListItemSecondaryAction>
                        <Chip 
                          label={pl.isAllowed ? 'Enabled' : 'Disabled'}
                          color={pl.isAllowed ? 'success' : 'default'}
                          size="small"
                        />
                      </ListItemSecondaryAction>
                    </ListItem>
                  ))}
                </List>
                {problemLanguages.length === 0 && (
                  <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
                    Chưa có ngôn ngữ nào được cấu hình
                  </Typography>
                )}
              </>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setLanguageDialogOpen(false)}>Đóng</Button>
            <Button 
              variant="contained" 
              component={Link}
              to={`/teacher/problem/${selectedProblem?.problemId}/languages`}
            >
              Chỉnh sửa
            </Button>
          </DialogActions>
        </Dialog>

        {/* Dataset Dialog */}
        <Dialog 
          open={datasetDialogOpen} 
          onClose={() => setDatasetDialogOpen(false)}
          maxWidth="md"
          fullWidth
        >
          <DialogTitle>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <DatasetIcon />
                Quản lý Test Cases - {selectedProblem?.title}
              </Box>
              <IconButton onClick={() => setDatasetDialogOpen(false)}>
                <CloseIcon />
              </IconButton>
            </Box>
          </DialogTitle>
          <DialogContent>
            {loadingDatasets ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                <CircularProgress />
              </Box>
            ) : (
              <>
                <Alert severity="info" sx={{ mb: 2 }}>
                  Datasets chứa các test case để chấm điểm bài làm của sinh viên.
                </Alert>
                <List>
                  {datasets.map((ds) => (
                    <ListItem key={ds.datasetId}>
                      <ListItemText
                        primary={ds.name}
                        secondary={`Type: ${ds.kind} | Test cases: ${ds.testCases?.length || 0}`}
                      />
                      <ListItemSecondaryAction>
                        <Chip 
                          label={ds.kind}
                          color={ds.kind === 'SAMPLE' ? 'info' : ds.kind === 'HIDDEN' ? 'warning' : 'default'}
                          size="small"
                        />
                      </ListItemSecondaryAction>
                    </ListItem>
                  ))}
                </List>
                {datasets.length === 0 && (
                  <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
                    Chưa có test case nào
                  </Typography>
                )}
              </>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDatasetDialogOpen(false)}>Đóng</Button>
            <Button 
              variant="contained" 
              component={Link}
              to={`/teacher/problem/${selectedProblem?.problemId}/datasets`}
            >
              Chỉnh sửa
            </Button>
          </DialogActions>
        </Dialog>

        {/* Snackbar */}
        <Snackbar
          open={snackbar.open}
          autoHideDuration={6000}
          onClose={() => setSnackbar({ ...snackbar, open: false })}
          anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
        >
          <Alert severity={snackbar.severity} onClose={() => setSnackbar({ ...snackbar, open: false })}>
            {snackbar.message}
          </Alert>
        </Snackbar>

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
