import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/admin.classes'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  getAllClassesForAdmin,
  archiveClassAdmin,
  unarchiveClassAdmin,
  deleteClassByAdmin,
  getClassStatistics,
  getClassStudents,
  bulkActionClasses,
  type AdminClassDetail,
  type ClassStatistics,
} from '~/services/adminService'
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
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
  Alert,
  Snackbar,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  TablePagination,
  Checkbox,
  Toolbar,
  Drawer,
  Tabs,
  Tab,
  List,
  ListItem,
  ListItemText,
  Avatar,
  Select,
  FormControl,
  InputLabel,
  Divider,
  Paper,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import AddIcon from '@mui/icons-material/Add'
import MoreVertIcon from '@mui/icons-material/MoreVert'
import ClassIcon from '@mui/icons-material/Class'
import FilterListIcon from '@mui/icons-material/FilterList'
import VisibilityIcon from '@mui/icons-material/Visibility'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import ArchiveIcon from '@mui/icons-material/Archive'
import UnarchiveIcon from '@mui/icons-material/Unarchive'
import CloseIcon from '@mui/icons-material/Close'
import PersonIcon from '@mui/icons-material/Person'
import PeopleIcon from '@mui/icons-material/People'
import AssignmentIcon from '@mui/icons-material/Assignment'

export const meta: Route.MetaFunction = () => [
  { title: 'Quản lý lớp học | Admin | UCode' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'admin') throw redirect('/home')

  return { user }
}

type Order = 'asc' | 'desc'

export default function AdminClasses() {
  const {} = useLoaderData<typeof clientLoader>()
  
  // State
  const [classes, setClasses] = React.useState<AdminClassDetail[]>([])
  const [statistics, setStatistics] = React.useState<ClassStatistics | null>(null)
  const [loading, setLoading] = React.useState(true)
  const [error, setError] = React.useState<string | null>(null)
  const [page, setPage] = React.useState(0)
  const [rowsPerPage, setRowsPerPage] = React.useState(10)
  const [totalCount, setTotalCount] = React.useState(0)
  const [searchTerm, setSearchTerm] = React.useState('')
  const [debouncedSearchTerm, setDebouncedSearchTerm] = React.useState('')
  const [teacherFilter, setTeacherFilter] = React.useState<string>('')
  const [isArchivedFilter, setIsArchivedFilter] = React.useState<boolean | undefined>(undefined)
  const [order, setOrder] = React.useState<Order>('desc')
  const [orderBy, setOrderBy] = React.useState<keyof AdminClassDetail>('createdAt')
  
  // Selection
  const [selected, setSelected] = React.useState<string[]>([])
  
  // Menu & Dialog
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null)
  const [selectedClass, setSelectedClass] = React.useState<AdminClassDetail | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false)
  const [archiveDialogOpen, setArchiveDialogOpen] = React.useState(false)
  const [bulkArchiveDialogOpen, setBulkArchiveDialogOpen] = React.useState(false)
  const [bulkDeleteDialogOpen, setBulkDeleteDialogOpen] = React.useState(false)
  const [archiveReason, setArchiveReason] = React.useState('')
  const [snackbar, setSnackbar] = React.useState({ open: false, message: '', severity: 'success' as 'success' | 'error' })

  // Drawer
  const [drawerOpen, setDrawerOpen] = React.useState(false)
  const [drawerClass, setDrawerClass] = React.useState<AdminClassDetail | null>(null)
  const [drawerTab, setDrawerTab] = React.useState(0)
  const [drawerStudents, setDrawerStudents] = React.useState<any[]>([])
  const [drawerStudentsLoading, setDrawerStudentsLoading] = React.useState(false)
  const [drawerStudentsPage, setDrawerStudentsPage] = React.useState(0)
  const [drawerStudentsRowsPerPage, setDrawerStudentsRowsPerPage] = React.useState(10)
  const [drawerStudentsTotalCount, setDrawerStudentsTotalCount] = React.useState(0)

  // Get unique teachers
  const uniqueTeachers = React.useMemo(() => {
    const teachers = new Map<string, string>()
    classes.forEach(c => {
      if (!teachers.has(c.teacherId)) {
        teachers.set(c.teacherId, c.teacherName)
      }
    })
    return Array.from(teachers.entries()).map(([id, name]) => ({ id, name }))
  }, [classes])

  // Debounce search term
  React.useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm)
      setPage(0)
    }, 500)
    return () => clearTimeout(timer)
  }, [searchTerm])

  // Load statistics
  const loadStatistics = React.useCallback(async () => {
    try {
      const stats = await getClassStatistics()
      setStatistics(stats)
    } catch (err: any) {
      console.error('Failed to load statistics:', err)
    }
  }, [])

  React.useEffect(() => {
    loadStatistics()
  }, [loadStatistics])

  // Load classes
  const loadClasses = React.useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const response = await getAllClassesForAdmin(page + 1, rowsPerPage, {
        searchTerm: debouncedSearchTerm || undefined,
        isArchived: isArchivedFilter,
        teacherId: teacherFilter || undefined,
      })
      setClasses(response.items)
      setTotalCount(response.totalCount)
    } catch (err: any) {
      setError(err.message || 'Failed to load classes')
    } finally {
      setLoading(false)
    }
  }, [page, rowsPerPage, debouncedSearchTerm, isArchivedFilter, teacherFilter])

  React.useEffect(() => {
    loadClasses()
  }, [loadClasses])

  // Load drawer students
  const loadDrawerStudents = React.useCallback(async (classId: string, pageNum: number = 1, pageSize: number = 10) => {
    try {
      setDrawerStudentsLoading(true)
      const response = await getClassStudents(classId, pageNum, pageSize)
      setDrawerStudents(response.items)
      setDrawerStudentsTotalCount(response.totalCount)
    } catch (err: any) {
      console.error('Failed to load students:', err)
    } finally {
      setDrawerStudentsLoading(false)
    }
  }, [])

  // Handlers
  const handleRequestSort = (property: keyof AdminClassDetail) => {
    const isAsc = orderBy === property && order === 'asc'
    setOrder(isAsc ? 'desc' : 'asc')
    setOrderBy(property)
  }

  const handleSelectAllClick = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      const newSelected = classes.map((n) => n.classId)
      setSelected(newSelected)
      return
    }
    setSelected([])
  }

  const handleClick = (classId: string) => {
    const selectedIndex = selected.indexOf(classId)
    let newSelected: string[] = []

    if (selectedIndex === -1) {
      newSelected = newSelected.concat(selected, classId)
    } else if (selectedIndex === 0) {
      newSelected = newSelected.concat(selected.slice(1))
    } else if (selectedIndex === selected.length - 1) {
      newSelected = newSelected.concat(selected.slice(0, -1))
    } else if (selectedIndex > 0) {
      newSelected = newSelected.concat(
        selected.slice(0, selectedIndex),
        selected.slice(selectedIndex + 1),
      )
    }
    setSelected(newSelected)
  }

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>, cls: AdminClassDetail) => {
    event.stopPropagation()
    setAnchorEl(event.currentTarget)
    setSelectedClass(cls)
  }

  const handleMenuClose = () => {
    setAnchorEl(null)
    setSelectedClass(null)
  }

  const handleViewDetail = (cls: AdminClassDetail) => {
    setDrawerClass(cls)
    setDrawerTab(0)
    setDrawerOpen(true)
    setDrawerStudentsPage(0)
    setDrawerStudentsRowsPerPage(10)
    loadDrawerStudents(cls.classId, 1, 10)
    handleMenuClose()
  }

  const handleArchive = async () => {
    if (!selectedClass) return
    
    const classId = selectedClass.classId
    
    try {
      await archiveClassAdmin(classId, archiveReason)
      setSnackbar({ open: true, message: 'Lớp học đã được ngừng hoạt động thành công', severity: 'success' })
      await loadClasses()
      await loadStatistics()
    } catch (err: any) {
      setSnackbar({ open: true, message: err.message || 'Ngừng hoạt động lớp học thất bại', severity: 'error' })
    } finally {
      setArchiveDialogOpen(false)
      setArchiveReason('')
      setAnchorEl(null)
      setSelectedClass(null)
    }
  }

  const handleUnarchive = async () => {
    if (!selectedClass) return
    
    const classId = selectedClass.classId
    
    try {
      await unarchiveClassAdmin(classId)
      setSnackbar({ open: true, message: 'Khôi phục lớp học thành công', severity: 'success' })
      await loadClasses()
      await loadStatistics()
    } catch (err: any) {
      setSnackbar({ open: true, message: err.message || 'Khôi phục lớp học thất bại', severity: 'error' })
    } finally {
      setAnchorEl(null)
      setSelectedClass(null)
    }
  }

  const handleDelete = async () => {
    if (!selectedClass) return
    
    try {
      await deleteClassByAdmin(selectedClass.classId)
      setSnackbar({ open: true, message: 'Xóa lớp học thành công', severity: 'success' })
      loadClasses()
      loadStatistics()
    } catch (err: any) {
      setSnackbar({ open: true, message: err.message || 'Xóa lớp học thất bại', severity: 'error' })
    } finally {
      setDeleteDialogOpen(false)
      setAnchorEl(null)
      setSelectedClass(null)
    }
  }

  const handleBulkArchive = async () => {
    try {
      await bulkActionClasses({ action: 'archive', classIds: selected, reason: archiveReason })
      setSnackbar({ open: true, message: `Đã ngừng hoạt động ${selected.length} lớp học`, severity: 'success' })
      setSelected([])
      loadClasses()
      loadStatistics()
    } catch (err: any) {
      setSnackbar({ open: true, message: err.message || 'Bulk archive thất bại', severity: 'error' })
    } finally {
      setBulkArchiveDialogOpen(false)
      setArchiveReason('')
    }
  }

  const handleBulkUnarchive = async () => {
    try {
      await bulkActionClasses({ action: 'unarchive', classIds: selected })
      setSnackbar({ open: true, message: `Đã khôi phục ${selected.length} lớp học`, severity: 'success' })
      setSelected([])
      loadClasses()
      loadStatistics()
    } catch (err: any) {
      setSnackbar({ open: true, message: err.message || 'Bulk unarchive thất bại', severity: 'error' })
    }
  }

  const handleBulkDelete = async () => {
    try {
      await bulkActionClasses({ action: 'delete', classIds: selected })
      setSnackbar({ open: true, message: `Đã xóa ${selected.length} lớp học`, severity: 'success' })
      setSelected([])
      loadClasses()
      loadStatistics()
    } catch (err: any) {
      setSnackbar({ open: true, message: err.message || 'Bulk delete thất bại', severity: 'error' })
    } finally {
      setBulkDeleteDialogOpen(false)
    }
  }

  const handleQuickFilter = (filterType: 'all' | 'active' | 'inactive') => {
    if (filterType === 'all') {
      setIsArchivedFilter(undefined)
    } else if (filterType === 'active') {
      setIsArchivedFilter(false)
    } else {
      setIsArchivedFilter(true)
    }
    setPage(0)
  }

  const sortedClasses = React.useMemo(() => {
    return [...classes].sort((a, b) => {
      const aValue = a[orderBy]
      const bValue = b[orderBy]
      
      if (aValue === undefined && bValue === undefined) return 0
      if (aValue === undefined) return 1
      if (bValue === undefined) return -1
      
      if (aValue < bValue) return order === 'asc' ? -1 : 1
      if (aValue > bValue) return order === 'asc' ? 1 : -1
      return 0
    })
  }, [classes, order, orderBy])

  const isSelected = (classId: string) => selected.indexOf(classId) !== -1

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Box sx={{ mb: 3 }}>
            <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f', mb: 0.5 }}>
              Quản lý lớp học
            </Typography>
            <Typography variant='body1' sx={{ color: '#6e6e73', fontSize: '1.125rem' }}>
              Quản lý tất cả các lớp học trong hệ thống
            </Typography>
          </Box>

          {/* Search and Filter */}
          <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
            <TextField
              placeholder='Tìm kiếm lớp học...'
              variant='outlined'
              fullWidth
              size='small'
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
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
            
            <FormControl size='small' sx={{ minWidth: 200 }}>
              <InputLabel>Giảng viên</InputLabel>
              <Select
                value={teacherFilter}
                label='Giảng viên'
                onChange={(e) => {
                  setTeacherFilter(e.target.value)
                  setPage(0)
                }}
                sx={{
                  borderRadius: 2,
                  bgcolor: 'white',
                  '& fieldset': { borderColor: '#d2d2d7' },
                }}
              >
                <MenuItem value=''>Tất cả</MenuItem>
                {uniqueTeachers.map(teacher => (
                  <MenuItem key={teacher.id} value={teacher.id}>{teacher.name}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
        </Box>

        {/* Stats with Quick Filters */}
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr 1fr' }, gap: 2, mb: 3 }}>
          {[
            { 
              label: 'Tổng lớp học', 
              value: statistics?.totalClasses || 0, 
              color: '#AF52DE', 
              icon: ClassIcon,
              filter: 'all'
            },
            { 
              label: 'Lớp hoạt động', 
              value: statistics?.activeClasses || 0, 
              color: '#34C759', 
              icon: ClassIcon,
              filter: 'active'
            },
            { 
              label: 'Lớp không hoạt động', 
              value: statistics?.archivedClasses || 0, 
              color: '#FF9500', 
              icon: ArchiveIcon,
              filter: 'inactive'
            },
          ].map((stat, index) => (
            <Card
              key={index}
              elevation={0}
              onClick={() => handleQuickFilter(stat.filter as any)}
              sx={{
                borderRadius: 3,
                bgcolor: 'white',
                border: '1px solid #d2d2d7',
                cursor: 'pointer',
                transition: 'all 0.2s',
                '&:hover': {
                  transform: 'translateY(-2px)',
                  boxShadow: '0 4px 12px rgba(0,0,0,0.08)',
                },
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

        {/* Bulk Actions Toolbar */}
        {selected.length > 0 && (
          <Toolbar
            sx={{
              pl: { sm: 2 },
              pr: { xs: 1, sm: 1 },
              bgcolor: '#007AFF',
              color: 'white',
              borderRadius: 2,
              mb: 2,
            }}
          >
            <Typography sx={{ flex: '1 1 50%' }} variant='subtitle1' component='div'>
              {selected.length} lớp được chọn
            </Typography>
            <Button
              onClick={() => setBulkArchiveDialogOpen(true)}
              startIcon={<ArchiveIcon />}
              sx={{ color: 'white', textTransform: 'none', mr: 1 }}
            >
              Ngừng hoạt động
            </Button>
            <Button
              onClick={handleBulkUnarchive}
              startIcon={<UnarchiveIcon />}
              sx={{ color: 'white', textTransform: 'none', mr: 1 }}
            >
              Khôi phục
            </Button>
            <Button
              onClick={() => setBulkDeleteDialogOpen(true)}
              startIcon={<DeleteIcon />}
              sx={{ color: 'white', textTransform: 'none' }}
            >
              Xóa
            </Button>
          </Toolbar>
        )}

        {/* Table */}
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
            <CircularProgress />
          </Box>
        ) : error ? (
          <Alert severity='error'>{error}</Alert>
        ) : classes.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 8 }}>
            <ClassIcon sx={{ fontSize: 64, color: '#d2d2d7', mb: 2 }} />
            <Typography variant='h6' sx={{ color: '#86868b' }}>
              Không có lớp học nào
            </Typography>
          </Box>
        ) : (
          <Paper elevation={0} sx={{ borderRadius: 3, border: '1px solid #d2d2d7' }}>
            <TableContainer>
              <Table stickyHeader>
                <TableHead>
                  <TableRow>
                    <TableCell padding='checkbox'>
                      <Checkbox
                        indeterminate={selected.length > 0 && selected.length < classes.length}
                        checked={classes.length > 0 && selected.length === classes.length}
                        onChange={handleSelectAllClick}
                      />
                    </TableCell>
                    <TableCell>
                      <TableSortLabel
                        active={orderBy === 'name'}
                        direction={orderBy === 'name' ? order : 'asc'}
                        onClick={() => handleRequestSort('name')}
                      >
                        Tên lớp
                      </TableSortLabel>
                    </TableCell>
                    <TableCell>Mã lớp</TableCell>
                    <TableCell>Giảng viên</TableCell>
                    <TableCell align='center'>
                      <TableSortLabel
                        active={orderBy === 'studentCount'}
                        direction={orderBy === 'studentCount' ? order : 'asc'}
                        onClick={() => handleRequestSort('studentCount')}
                      >
                        SV
                      </TableSortLabel>
                    </TableCell>
                    <TableCell align='center'>Bài tập</TableCell>
                    <TableCell>Trạng thái</TableCell>
                    <TableCell>
                      <TableSortLabel
                        active={orderBy === 'createdAt'}
                        direction={orderBy === 'createdAt' ? order : 'asc'}
                        onClick={() => handleRequestSort('createdAt')}
                      >
                        Ngày tạo
                      </TableSortLabel>
                    </TableCell>
                    <TableCell align='right'>Hành động</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {sortedClasses.map((classItem) => {
                    const isItemSelected = isSelected(classItem.classId)
                    return (
                      <TableRow
                        key={classItem.classId}
                        hover
                        selected={isItemSelected}
                        sx={{
                          cursor: 'pointer',
                          opacity: classItem.isArchived ? 0.6 : 1,
                        }}
                      >
                        <TableCell padding='checkbox'>
                          <Checkbox
                            checked={isItemSelected}
                            onClick={(e) => {
                              e.stopPropagation()
                              handleClick(classItem.classId)
                            }}
                          />
                        </TableCell>
                        <TableCell onClick={() => handleViewDetail(classItem)}>
                          <Typography variant='body2' sx={{ fontWeight: 600 }}>
                            {classItem.name}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Chip label={classItem.classCode} size='small' sx={{ fontWeight: 600 }} />
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Avatar sx={{ width: 32, height: 32, bgcolor: '#007AFF' }}>
                              {classItem.teacherName.charAt(0)}
                            </Avatar>
                            <Typography variant='body2'>{classItem.teacherName}</Typography>
                          </Box>
                        </TableCell>
                        <TableCell align='center'>
                          <Chip icon={<PeopleIcon />} label={classItem.studentCount} size='small' />
                        </TableCell>
                        <TableCell align='center'>
                          <Chip icon={<AssignmentIcon />} label={classItem.assignmentCount} size='small' />
                        </TableCell>
                        <TableCell>
                          {classItem.isArchived ? (
                            <Chip icon={<ArchiveIcon />} label='Ngừng hoạt động' size='small' color='warning' />
                          ) : classItem.isActive ? (
                            <Chip label='Hoạt động' size='small' color='success' />
                          ) : (
                            <Chip label='Không hoạt động' size='small' color='error' />
                          )}
                        </TableCell>
                        <TableCell>
                          {new Date(classItem.createdAt).toLocaleDateString('vi-VN')}
                        </TableCell>
                        <TableCell align='right'>
                          <IconButton
                            size='small'
                            onClick={(e) => handleMenuOpen(e, classItem)}
                          >
                            <MoreVertIcon />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    )
                  })}
                </TableBody>
              </Table>
            </TableContainer>
            <TablePagination
              component='div'
              count={totalCount}
              page={page}
              onPageChange={(_, newPage) => setPage(newPage)}
              rowsPerPage={rowsPerPage}
              onRowsPerPageChange={(e) => {
                setRowsPerPage(parseInt(e.target.value, 10))
                setPage(0)
              }}
              labelRowsPerPage='Số lớp mỗi trang:'
              labelDisplayedRows={({ from, to, count }) => `${from}-${to} của ${count}`}
            />
          </Paper>
        )}

        {/* Context Menu */}
        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={handleMenuClose}
          PaperProps={{
            sx: { borderRadius: 2, boxShadow: '0 4px 12px rgba(0,0,0,0.15)' },
          }}
        >
          <MenuItem onClick={() => selectedClass && handleViewDetail(selectedClass)}>
            <VisibilityIcon sx={{ mr: 1, fontSize: 20 }} />
            Xem chi tiết
          </MenuItem>
          <MenuItem onClick={handleMenuClose}>
            <EditIcon sx={{ mr: 1, fontSize: 20 }} />
            Chỉnh sửa
          </MenuItem>
          {selectedClass?.isArchived ? (
            <MenuItem
              onClick={() => {
                handleUnarchive()
              }}
              sx={{ color: 'info.main' }}
            >
              <UnarchiveIcon sx={{ mr: 1, fontSize: 20 }} />
              Khôi phục
            </MenuItem>
          ) : (
            <MenuItem
              onClick={() => {
                setArchiveDialogOpen(true)
              }}
              sx={{ color: 'warning.main' }}
            >
              <ArchiveIcon sx={{ mr: 1, fontSize: 20 }} />
              Ngừng hoạt động
            </MenuItem>
          )}
          <MenuItem
            onClick={() => {
              handleMenuClose()
              setDeleteDialogOpen(true)
            }}
            sx={{ color: 'error.main' }}
          >
            <DeleteIcon sx={{ mr: 1, fontSize: 20 }} />
            Xóa vĩnh viễn
          </MenuItem>
        </Menu>

        {/* Detail Drawer */}
        <Drawer
          anchor='right'
          open={drawerOpen}
          onClose={() => setDrawerOpen(false)}
          PaperProps={{
            sx: { width: { xs: '100%', sm: 600 } },
          }}
        >
          {drawerClass && (
            <Box>
              {/* Drawer Header */}
              <Box sx={{ p: 3, borderBottom: '1px solid #d2d2d7', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography variant='h5' sx={{ fontWeight: 700 }}>
                    {drawerClass.name}
                  </Typography>
                  <Typography variant='body2' sx={{ color: '#86868b' }}>
                    {drawerClass.classCode}
                  </Typography>
                </Box>
                <IconButton onClick={() => setDrawerOpen(false)}>
                  <CloseIcon />
                </IconButton>
              </Box>

              {/* Tabs */}
              <Tabs value={drawerTab} onChange={(_, v) => setDrawerTab(v)} sx={{ borderBottom: '1px solid #d2d2d7' }}>
                <Tab label='Tổng quan' />
                <Tab label='Sinh viên' />
              </Tabs>

              {/* Tab Content */}
              <Box sx={{ p: 3 }}>
                {drawerTab === 0 && (
                  <Box>
                    <Box sx={{ mb: 3 }}>
                      <Typography variant='body2' sx={{ color: '#86868b', mb: 0.5 }}>
                        Giảng viên
                      </Typography>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Avatar sx={{ width: 32, height: 32, bgcolor: '#007AFF' }}>
                          {drawerClass.teacherName.charAt(0)}
                        </Avatar>
                        <Box>
                          <Typography variant='body2' sx={{ fontWeight: 600 }}>
                            {drawerClass.teacherName}
                          </Typography>
                          <Typography variant='caption' sx={{ color: '#86868b' }}>
                            {drawerClass.teacherEmail}
                          </Typography>
                        </Box>
                      </Box>
                    </Box>

                    <Divider sx={{ my: 2 }} />

                    <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
                      <Box>
                        <Typography variant='body2' sx={{ color: '#86868b', mb: 0.5 }}>
                          Tổng sinh viên
                        </Typography>
                        <Typography variant='h6' sx={{ fontWeight: 600 }}>
                          {drawerClass.studentCount}
                        </Typography>
                      </Box>
                      <Box>
                        <Typography variant='body2' sx={{ color: '#86868b', mb: 0.5 }}>
                          SV hoạt động
                        </Typography>
                        <Typography variant='h6' sx={{ fontWeight: 600 }}>
                          {drawerClass.activeStudentCount}
                        </Typography>
                      </Box>
                      <Box>
                        <Typography variant='body2' sx={{ color: '#86868b', mb: 0.5 }}>
                          Tổng bài tập
                        </Typography>
                        <Typography variant='h6' sx={{ fontWeight: 600 }}>
                          {drawerClass.assignmentCount}
                        </Typography>
                      </Box>
                      <Box>
                        <Typography variant='body2' sx={{ color: '#86868b', mb: 0.5 }}>
                          Tổng bài nộp
                        </Typography>
                        <Typography variant='h6' sx={{ fontWeight: 600 }}>
                          {drawerClass.submissionCount}
                        </Typography>
                      </Box>
                    </Box>

                    <Divider sx={{ my: 2 }} />

                    <Box>
                      <Typography variant='body2' sx={{ color: '#86868b', mb: 0.5 }}>
                        Mô tả
                      </Typography>
                      <Typography variant='body2'>{drawerClass.description}</Typography>
                    </Box>
                  </Box>
                )}

                {drawerTab === 1 && (
                  <Box>
                    {drawerStudentsLoading ? (
                      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                        <CircularProgress />
                      </Box>
                    ) : drawerStudents.length === 0 ? (
                      <Box sx={{ textAlign: 'center', py: 4 }}>
                        <PeopleIcon sx={{ fontSize: 48, color: '#d2d2d7', mb: 1 }} />
                        <Typography variant='body2' sx={{ color: '#86868b' }}>
                          Chưa có sinh viên nào
                        </Typography>
                      </Box>
                    ) : (
                      <>
                        <List>
                          {drawerStudents.map((student, idx) => (
                            <ListItem key={idx} sx={{ px: 0 }}>
                              <Avatar sx={{ mr: 2, bgcolor: '#34C759' }}>
                                {student.fullName.charAt(0)}
                              </Avatar>
                              <ListItemText
                                primary={student.fullName}
                                secondary={`${student.studentCode} • ${student.email}`}
                              />
                            </ListItem>
                          ))}
                        </List>
                        <TablePagination
                          component='div'
                          count={drawerStudentsTotalCount}
                          page={drawerStudentsPage}
                          onPageChange={(_, newPage) => {
                            setDrawerStudentsPage(newPage)
                            if (drawerClass) {
                              loadDrawerStudents(drawerClass.classId, newPage + 1, drawerStudentsRowsPerPage)
                            }
                          }}
                          rowsPerPage={drawerStudentsRowsPerPage}
                          onRowsPerPageChange={(e) => {
                            const newRowsPerPage = parseInt(e.target.value, 10)
                            setDrawerStudentsRowsPerPage(newRowsPerPage)
                            setDrawerStudentsPage(0)
                            if (drawerClass) {
                              loadDrawerStudents(drawerClass.classId, 1, newRowsPerPage)
                            }
                          }}
                          labelRowsPerPage='Số SV/trang:'
                          labelDisplayedRows={({ from, to, count }) => `${from}-${to} / ${count}`}
                        />
                      </>
                    )}
                  </Box>
                )}
              </Box>
            </Box>
          )}
        </Drawer>

        {/* Dialogs */}
        <Dialog open={archiveDialogOpen} onClose={() => setArchiveDialogOpen(false)} maxWidth='sm' fullWidth>
          <DialogTitle>Ngừng hoạt động lớp học</DialogTitle>
          <DialogContent>
            <Typography variant='body2' sx={{ mb: 2, color: '#86868b' }}>
              Ngừng hoạt động lớp học <strong>{selectedClass?.name}</strong>?
            </Typography>
            <TextField
              fullWidth
              multiline
              rows={3}
              label='Lý do'
              value={archiveReason}
              onChange={(e) => setArchiveReason(e.target.value)}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setArchiveDialogOpen(false)}>Hủy</Button>
            <Button onClick={handleArchive} variant='contained' color='warning'>
              Ngừng hoạt động
            </Button>
          </DialogActions>
        </Dialog>

        <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
          <DialogTitle>Xóa lớp học</DialogTitle>
          <DialogContent>
            <Typography variant='body2'>
              Xóa vĩnh viễn <strong>{selectedClass?.name}</strong>?
            </Typography>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDeleteDialogOpen(false)}>Hủy</Button>
            <Button onClick={handleDelete} variant='contained' color='error'>
              Xóa
            </Button>
          </DialogActions>
        </Dialog>

        <Dialog open={bulkArchiveDialogOpen} onClose={() => setBulkArchiveDialogOpen(false)} maxWidth='sm' fullWidth>
          <DialogTitle>Ngừng hoạt động {selected.length} lớp học</DialogTitle>
          <DialogContent>
            <TextField
              fullWidth
              multiline
              rows={3}
              label='Lý do'
              value={archiveReason}
              onChange={(e) => setArchiveReason(e.target.value)}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setBulkArchiveDialogOpen(false)}>Hủy</Button>
            <Button onClick={handleBulkArchive} variant='contained' color='warning'>
              Ngừng hoạt động
            </Button>
          </DialogActions>
        </Dialog>

        <Dialog open={bulkDeleteDialogOpen} onClose={() => setBulkDeleteDialogOpen(false)}>
          <DialogTitle>Xóa {selected.length} lớp học</DialogTitle>
          <DialogContent>
            <Typography variant='body2'>Xóa vĩnh viễn các lớp đã chọn?</Typography>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setBulkDeleteDialogOpen(false)}>Hủy</Button>
            <Button onClick={handleBulkDelete} variant='contained' color='error'>
              Xóa
            </Button>
          </DialogActions>
        </Dialog>

        <Snackbar
          open={snackbar.open}
          autoHideDuration={4000}
          onClose={() => setSnackbar({ ...snackbar, open: false })}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        >
          <Alert
            onClose={() => setSnackbar({ ...snackbar, open: false })}
            severity={snackbar.severity}
            sx={{ width: '100%' }}
          >
            {snackbar.message}
          </Alert>
        </Snackbar>
      </Container>
    </Box>
  )
}
