import * as React from 'react'
import { redirect } from 'react-router'
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
  TableSortLabel,
  Checkbox,
  Pagination,
  Stack,
  FormControl,
  InputLabel,
  Select,
  Grid,
  Drawer,
  Tabs,
  Tab,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
  Alert,
  Paper,
  Collapse,
  DialogContentText,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import AddIcon from '@mui/icons-material/Add'
import MoreVertIcon from '@mui/icons-material/MoreVert'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import BlockIcon from '@mui/icons-material/Block'
import PeopleIcon from '@mui/icons-material/People'
import FilterListIcon from '@mui/icons-material/FilterList'
import RefreshIcon from '@mui/icons-material/Refresh'
import ClearIcon from '@mui/icons-material/Clear'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'
import SchoolIcon from '@mui/icons-material/School'
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings'
import PersonIcon from '@mui/icons-material/Person'
import VerifiedUserIcon from '@mui/icons-material/VerifiedUser'
import VisibilityIcon from '@mui/icons-material/Visibility'
import CloseIcon from '@mui/icons-material/Close'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft'
import ChevronRightIcon from '@mui/icons-material/ChevronRight'
import axios from 'axios'

export const meta: Route.MetaFunction = () => [
  { title: 'Quản lý người dùng | Admin | UCode' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  if (user.role !== 'admin') throw redirect('/home')
  return { user }
}

// Types
interface UserStatistics {
  totalUsers: number
  teachers: number
  students: number
  admins: number
  activeUsers: number
  inactiveUsers: number
}

interface User {
  userId: string
  fullName: string
  email: string
  role: string
  isActive: boolean
  createdAt: string
  studentCode?: string
  teacherCode?: string
  classCount?: number
  enrolledClassCount?: number
}

interface UserDetail extends User {
  phone?: string
  lastLoginAt?: string
  emailVerified: boolean
  totalAssignments?: number
  totalSubmissions?: number
  averageScore?: number
}

interface PagedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

type SortField = 'fullName' | 'email' | 'createdAt' | 'role' | 'isActive'
type SortOrder = 'asc' | 'desc'


export default function AdminUsers() {
  // API Base URL
  const API_BASE_URL = 'http://localhost:5000/api/v1'

  // State - Dashboard Stats
  const [stats, setStats] = React.useState<UserStatistics | null>(null)
  const [statsLoading, setStatsLoading] = React.useState(true)

  // State - Table Data
  const [users, setUsers] = React.useState<User[]>([])
  const [loading, setLoading] = React.useState(true)
  const [currentPage, setCurrentPage] = React.useState(1)
  const [pageSize] = React.useState(10)
  const [totalPages, setTotalPages] = React.useState(0)
  const [totalRecords, setTotalRecords] = React.useState(0)

  // State - Sorting
  const [sortField, setSortField] = React.useState<SortField>('createdAt')
  const [sortOrder, setSortOrder] = React.useState<SortOrder>('desc')

  // State - Selection
  const [selectedUserIds, setSelectedUserIds] = React.useState<string[]>([])

  // State - Search & Filter
  const [filters, setFilters] = React.useState({
    searchTerm: '',
    roleFilter: '',
    statusFilter: '',
  })
  const [debouncedSearchTerm, setDebouncedSearchTerm] = React.useState('')
  const [filterPanelOpen, setFilterPanelOpen] = React.useState(false)

  // State - Actions Menu
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null)
  const [selectedUserId, setSelectedUserId] = React.useState('')

  // State - User Detail Drawer
  const [drawerOpen, setDrawerOpen] = React.useState(false)
  const [userDetail, setUserDetail] = React.useState<UserDetail | null>(null)
  const [drawerLoading, setDrawerLoading] = React.useState(false)
  const [drawerTab, setDrawerTab] = React.useState<'overview' | 'edit'>('overview')

  // State - Dialogs
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false)
  const [bulkActionDialogOpen, setBulkActionDialogOpen] = React.useState(false)
  const [bulkActionType, setBulkActionType] = React.useState<'activate' | 'deactivate' | 'delete' | 'changeRole'>('activate')
  const [bulkNewRole, setBulkNewRole] = React.useState('')
  
  // State - Add User Dialog
  const [addUserDialogOpen, setAddUserDialogOpen] = React.useState(false)
  const [addUserForm, setAddUserForm] = React.useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
    role: '',
    phone: '',
    studentCode: '',
    teacherCode: '',
    isActive: true,
  })
  const [addUserErrors, setAddUserErrors] = React.useState<Record<string, string>>({})
  const [addUserLoading, setAddUserLoading] = React.useState(false)

  // Fetch Dashboard Statistics
  const fetchStats = async () => {
    try {
      setStatsLoading(true)
      const token = localStorage.getItem('token')
      const response = await axios.get(`${API_BASE_URL}/admin/users/statistics`, {
        headers: { Authorization: `Bearer ${token}` },
      })
      setStats(response.data.data)
    } catch (error) {
      console.error('Failed to fetch user statistics:', error)
    } finally {
      setStatsLoading(false)
    }
  }

  // Fetch Users with Filters
  const fetchUsers = async () => {
    try {
      setLoading(true)
      const token = localStorage.getItem('token')
      const params: any = {
        pageNumber: currentPage,
        pageSize,
      }
      if (debouncedSearchTerm) params.searchTerm = debouncedSearchTerm
      if (filters.roleFilter) params.role = filters.roleFilter
      if (filters.statusFilter !== '') params.isActive = filters.statusFilter === 'active'

      console.log('🔍 Fetching users with params:', params) // Debug log

      const response = await axios.get(`${API_BASE_URL}/admin/users`, {
        headers: { Authorization: `Bearer ${token}` },
        params,
      })

      const data: PagedResult<User> = response.data.data
      console.log('📦 Received users:', data.items.length, 'items') // Debug log
      setUsers(data.items)
      setTotalPages(data.totalPages)
      setTotalRecords(data.totalCount)
    } catch (error) {
      console.error('Failed to fetch users:', error)
    } finally {
      setLoading(false)
    }
  }

  // Fetch User Detail
  const fetchUserDetail = async (userId: string) => {
    try {
      setDrawerLoading(true)
      const token = localStorage.getItem('token')
      const response = await axios.get(`${API_BASE_URL}/admin/users/${userId}`, {
        headers: { Authorization: `Bearer ${token}` },
      })
      setUserDetail(response.data.data)
    } catch (error) {
      console.error('Failed to fetch user detail:', error)
    } finally {
      setDrawerLoading(false)
    }
  }

  // Delete User
  const deleteUser = async (userId: string) => {
    try {
      const token = localStorage.getItem('token')
      await axios.delete(`${API_BASE_URL}/admin/users/${userId}`, {
        headers: { Authorization: `Bearer ${token}` },
      })
      fetchUsers()
      fetchStats()
      setDeleteDialogOpen(false)
      setDrawerOpen(false)
    } catch (error) {
      console.error('Failed to delete user:', error)
    }
  }

  // Bulk Action
  const executeBulkAction = async () => {
    try {
      const token = localStorage.getItem('token')
      const payload: any = {
        action: bulkActionType,
        userIds: selectedUserIds,
      }
      if (bulkActionType === 'changeRole') {
        payload.newRole = bulkNewRole
      }

      await axios.post(`${API_BASE_URL}/admin/users/bulk-action`, payload, {
        headers: { Authorization: `Bearer ${token}` },
      })

      fetchUsers()
      fetchStats()
      setSelectedUserIds([])
      setBulkActionDialogOpen(false)
    } catch (error) {
      console.error('Failed to execute bulk action:', error)
    }
  }

  // Effects
  React.useEffect(() => {
    fetchStats()
  }, [])

  // Debounce search term
  React.useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(filters.searchTerm)
      console.log('⏱️ Debounced search term updated to:', filters.searchTerm) // Debug log
    }, 500) // 500ms delay

    return () => clearTimeout(timer)
  }, [filters.searchTerm])

  React.useEffect(() => {
    fetchUsers()
  }, [currentPage, debouncedSearchTerm, filters.roleFilter, filters.statusFilter])

  // Handlers - Sorting
  const handleSort = (field: SortField) => {
    const isAsc = sortField === field && sortOrder === 'asc'
    setSortOrder(isAsc ? 'desc' : 'asc')
    setSortField(field)
  }

  // Handlers - Selection
  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      setSelectedUserIds(users.map((u) => u.userId))
    } else {
      setSelectedUserIds([])
    }
  }

  const handleSelectUser = (userId: string) => {
    const selectedIndex = selectedUserIds.indexOf(userId)
    let newSelected: string[] = []

    if (selectedIndex === -1) {
      newSelected = newSelected.concat(selectedUserIds, userId)
    } else if (selectedIndex === 0) {
      newSelected = newSelected.concat(selectedUserIds.slice(1))
    } else if (selectedIndex === selectedUserIds.length - 1) {
      newSelected = newSelected.concat(selectedUserIds.slice(0, -1))
    } else if (selectedIndex > 0) {
      newSelected = newSelected.concat(selectedUserIds.slice(0, selectedIndex), selectedUserIds.slice(selectedIndex + 1))
    }

    setSelectedUserIds(newSelected)
  }

  // Handlers - Actions Menu
  const handleOpenMenu = (event: React.MouseEvent<HTMLElement>, userId: string) => {
    setAnchorEl(event.currentTarget)
    setSelectedUserId(userId)
  }

  const handleCloseMenu = () => {
    setAnchorEl(null)
    setSelectedUserId('')
  }

  const handleViewDetail = (userId: string) => {
    fetchUserDetail(userId)
    setDrawerOpen(true)
    handleCloseMenu()
  }

  const handleDeleteClick = (userId: string) => {
    setSelectedUserId(userId)
    setDeleteDialogOpen(true)
  }

  const handleCloseDrawer = () => {
    setDrawerOpen(false)
    setUserDetail(null)
    setDrawerTab('overview')
  }

  const handleDelete = async () => {
    if (selectedUserId) {
      await deleteUser(selectedUserId)
    }
  }

  const handleBulkAction = (action: 'activate' | 'deactivate' | 'delete' | 'changeRole') => {
    setBulkActionType(action)
    setBulkActionDialogOpen(true)
  }

  const handleExecuteBulkAction = async () => {
    await executeBulkAction()
  }

  // Handlers - Quick Filter from Stats
  const handleStatClick = (filterType: string) => {
    setCurrentPage(1)
    switch (filterType) {
      case 'teachers':
        setFilters({ searchTerm: '', roleFilter: 'Teacher', statusFilter: '' })
        break
      case 'students':
        setFilters({ searchTerm: '', roleFilter: 'Student', statusFilter: '' })
        break
      case 'admins':
        setFilters({ searchTerm: '', roleFilter: 'Admin', statusFilter: '' })
        break
      case 'active':
        setFilters({ searchTerm: '', roleFilter: '', statusFilter: 'active' })
        break
      case 'inactive':
        setFilters({ searchTerm: '', roleFilter: '', statusFilter: 'inactive' })
        break
      default:
        setFilters({ searchTerm: '', roleFilter: '', statusFilter: '' })
    }
  }

  // Handlers - Add User Dialog
  const handleOpenAddUserDialog = () => {
    setAddUserDialogOpen(true)
    setAddUserForm({
      fullName: '',
      email: '',
      password: '',
      confirmPassword: '',
      role: '',
      phone: '',
      studentCode: '',
      teacherCode: '',
      isActive: true,
    })
    setAddUserErrors({})
  }

  const handleCloseAddUserDialog = () => {
    setAddUserDialogOpen(false)
  }

  const handleAddUserFormChange = (field: string, value: any) => {
    setAddUserForm((prev) => ({ ...prev, [field]: value }))
    // Clear error when user types
    if (addUserErrors[field]) {
      setAddUserErrors((prev) => {
        const newErrors = { ...prev }
        delete newErrors[field]
        return newErrors
      })
    }
  }

  const validateAddUserForm = (): boolean => {
    const errors: Record<string, string> = {}

    if (!addUserForm.fullName.trim()) errors.fullName = 'Họ tên không được để trống'
    if (!addUserForm.email.trim()) {
      errors.email = 'Email không được để trống'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(addUserForm.email)) {
      errors.email = 'Email không đúng định dạng'
    }
    if (!addUserForm.password) {
      errors.password = 'Mật khẩu không được để trống'
    } else if (addUserForm.password.length < 6) {
      errors.password = 'Mật khẩu phải có ít nhất 6 ký tự'
    }
    if (addUserForm.password !== addUserForm.confirmPassword) {
      errors.confirmPassword = 'Mật khẩu xác nhận không khớp'
    }
    if (!addUserForm.role) errors.role = 'Vui lòng chọn vai trò'
    if (addUserForm.role === 'Student' && !addUserForm.studentCode.trim()) {
      errors.studentCode = 'Mã sinh viên không được để trống'
    }
    if (addUserForm.role === 'Teacher' && !addUserForm.teacherCode.trim()) {
      errors.teacherCode = 'Mã giảng viên không được để trống'
    }

    setAddUserErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleCreateUser = async () => {
    if (!validateAddUserForm()) return

    try {
      setAddUserLoading(true)
      const token = localStorage.getItem('token')

      const payload: any = {
        fullName: addUserForm.fullName.trim(),
        email: addUserForm.email.trim(),
        password: addUserForm.password,
        role: addUserForm.role,
        isActive: addUserForm.isActive,
      }

      if (addUserForm.phone.trim()) payload.phone = addUserForm.phone.trim()
      if (addUserForm.role === 'Student') payload.studentCode = addUserForm.studentCode.trim()
      if (addUserForm.role === 'Teacher') payload.teacherCode = addUserForm.teacherCode.trim()

      const response = await axios.post(`${API_BASE_URL}/admin/users`, payload, {
        headers: { Authorization: `Bearer ${token}` },
      })

      console.log('✅ User created successfully:', response.data)

      // Refresh data
      await fetchUsers()
      await fetchStats()

      // Close dialog
      handleCloseAddUserDialog()

      alert('Thêm người dùng thành công!')
    } catch (error: any) {
      console.error('❌ Failed to create user:', error)
      const errorMessage = error.response?.data?.message || error.message || 'Có lỗi xảy ra khi thêm người dùng'
      alert(errorMessage)
    } finally {
      setAddUserLoading(false)
    }
  }

  // Sorted Users
  const sortedUsers = React.useMemo(() => {
    return [...users].sort((a, b) => {
      let aValue: any = a[sortField]
      let bValue: any = b[sortField]

      if (sortField === 'createdAt') {
        aValue = new Date(aValue).getTime()
        bValue = new Date(bValue).getTime()
      }

      if (aValue < bValue) return sortOrder === 'asc' ? -1 : 1
      if (aValue > bValue) return sortOrder === 'asc' ? 1 : -1
      return 0
    })
  }, [users, sortField, sortOrder])

  // Role Helpers
  const getRoleColor = (role: string): string => {
    switch (role) {
      case 'Admin':
        return '#EF4444'
      case 'Teacher':
        return '#8B5CF6'
      case 'Student':
        return '#3B82F6'
      default:
        return '#007AFF'
    }
  }

  const getRoleIcon = (role: string) => {
    switch (role) {
      case 'Admin':
        return <AdminPanelSettingsIcon sx={{ fontSize: 16 }} />
      case 'Teacher':
        return <SchoolIcon sx={{ fontSize: 16 }} />
      case 'Student':
        return <PersonIcon sx={{ fontSize: 16 }} />
      default:
        return <PersonIcon sx={{ fontSize: 16 }} />
    }
  }

  const getRoleLabel = (role: string) => {
    switch (role) {
      case 'Admin':
        return 'Quản trị viên'
      case 'Teacher':
        return 'Giảng viên'
      case 'Student':
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
              onClick={handleOpenAddUserDialog}
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
        </Box>

        {/* Stats */}
        {statsLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', mb: 3 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr 1fr 1fr 1fr 1fr' }, gap: 2, mb: 3 }}>
            <Card
              elevation={0}
              sx={{ borderRadius: 3, bgcolor: 'white', border: '1px solid #d2d2d7', cursor: 'pointer', '&:hover': { boxShadow: 2 } }}
              onClick={() => handleStatClick('all')}
            >
              <CardContent sx={{ p: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <PeopleIcon sx={{ color: '#007AFF', mr: 1 }} />
                  <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', fontSize: '0.875rem' }}>
                    Tổng
                  </Typography>
                </Box>
                <Typography variant='h4' sx={{ fontWeight: 600, color: '#007AFF' }}>
                  {stats?.totalUsers || 0}
                </Typography>
              </CardContent>
            </Card>

            <Card
              elevation={0}
              sx={{ borderRadius: 3, bgcolor: 'white', border: '1px solid #d2d2d7', cursor: 'pointer', '&:hover': { boxShadow: 2 } }}
              onClick={() => handleStatClick('teachers')}
            >
              <CardContent sx={{ p: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <SchoolIcon sx={{ color: '#8B5CF6', mr: 1 }} />
                  <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', fontSize: '0.875rem' }}>
                    GV
                  </Typography>
                </Box>
                <Typography variant='h4' sx={{ fontWeight: 600, color: '#8B5CF6' }}>
                  {stats?.teachers || 0}
                </Typography>
              </CardContent>
            </Card>

            <Card
              elevation={0}
              sx={{ borderRadius: 3, bgcolor: 'white', border: '1px solid #d2d2d7', cursor: 'pointer', '&:hover': { boxShadow: 2 } }}
              onClick={() => handleStatClick('students')}
            >
              <CardContent sx={{ p: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <PersonIcon sx={{ color: '#3B82F6', mr: 1 }} />
                  <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', fontSize: '0.875rem' }}>
                    SV
                  </Typography>
                </Box>
                <Typography variant='h4' sx={{ fontWeight: 600, color: '#3B82F6' }}>
                  {stats?.students || 0}
                </Typography>
              </CardContent>
            </Card>

            <Card
              elevation={0}
              sx={{ borderRadius: 3, bgcolor: 'white', border: '1px solid #d2d2d7', cursor: 'pointer', '&:hover': { boxShadow: 2 } }}
              onClick={() => handleStatClick('admins')}
            >
              <CardContent sx={{ p: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <AdminPanelSettingsIcon sx={{ color: '#EF4444', mr: 1 }} />
                  <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', fontSize: '0.875rem' }}>
                    Admin
                  </Typography>
                </Box>
                <Typography variant='h4' sx={{ fontWeight: 600, color: '#EF4444' }}>
                  {stats?.admins || 0}
                </Typography>
              </CardContent>
            </Card>

            <Card
              elevation={0}
              sx={{ borderRadius: 3, bgcolor: 'white', border: '1px solid #d2d2d7', cursor: 'pointer', '&:hover': { boxShadow: 2 } }}
              onClick={() => handleStatClick('active')}
            >
              <CardContent sx={{ p: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <CheckCircleIcon sx={{ color: '#34C759', mr: 1 }} />
                  <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', fontSize: '0.875rem' }}>
                    Hoạt động
                  </Typography>
                </Box>
                <Typography variant='h4' sx={{ fontWeight: 600, color: '#34C759' }}>
                  {stats?.activeUsers || 0}
                </Typography>
              </CardContent>
            </Card>

            <Card
              elevation={0}
              sx={{ borderRadius: 3, bgcolor: 'white', border: '1px solid #d2d2d7', cursor: 'pointer', '&:hover': { boxShadow: 2 } }}
              onClick={() => handleStatClick('inactive')}
            >
              <CardContent sx={{ p: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <CancelIcon sx={{ color: '#FF3B30', mr: 1 }} />
                  <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b', fontSize: '0.875rem' }}>
                    Không HĐ
                  </Typography>
                </Box>
                <Typography variant='h4' sx={{ fontWeight: 600, color: '#FF3B30' }}>
                  {stats?.inactiveUsers || 0}
                </Typography>
              </CardContent>
            </Card>
          </Box>
        )}

        {/* Search & Filter Bar */}
        <Card
          elevation={0}
          sx={{
            borderRadius: 3,
            bgcolor: 'white',
            border: '1px solid #d2d2d7',
            mb: 2,
          }}
        >
          <CardContent sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', gap: 2, alignItems: 'flex-start', flexDirection: 'column' }}>
              <Box sx={{ display: 'flex', gap: 2, width: '100%' }}>
                <TextField
                  size='small'
                  placeholder='Tìm theo tên, email, mã SV/GV...'
                  value={filters.searchTerm}
                  onChange={(e) => setFilters((prev) => ({ ...prev, searchTerm: e.target.value }))}
                  InputProps={{
                    startAdornment: <SearchIcon sx={{ mr: 1, color: '#86868b' }} />,
                  }}
                  sx={{ flex: 1 }}
                />
                <Button
                  variant='outlined'
                  startIcon={filterPanelOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                  onClick={() => setFilterPanelOpen(!filterPanelOpen)}
                  sx={{
                    borderColor: '#d2d2d7',
                    color: '#1d1d1f',
                    textTransform: 'none',
                    fontWeight: 600,
                    '&:hover': { borderColor: '#007AFF', color: '#007AFF' },
                  }}
                >
                  Bộ lọc
                </Button>
              </Box>

              <Collapse in={filterPanelOpen} sx={{ width: '100%' }}>
                <Box sx={{ display: 'flex', gap: 2, pt: 2, borderTop: '1px solid #d2d2d7' }}>
                  <FormControl size='small' sx={{ minWidth: 200 }}>
                    <InputLabel>Vai trò</InputLabel>
                    <Select
                      value={filters.roleFilter}
                      label='Vai trò'
                      onChange={(e) => setFilters((prev) => ({ ...prev, roleFilter: e.target.value }))}
                    >
                      <MenuItem value=''>Tất cả</MenuItem>
                      <MenuItem value='Admin'>Admin</MenuItem>
                      <MenuItem value='Teacher'>Giáo viên</MenuItem>
                      <MenuItem value='Student'>Sinh viên</MenuItem>
                    </Select>
                  </FormControl>

                  <FormControl size='small' sx={{ minWidth: 200 }}>
                    <InputLabel>Trạng thái</InputLabel>
                    <Select
                      value={filters.statusFilter}
                      label='Trạng thái'
                      onChange={(e) => setFilters((prev) => ({ ...prev, statusFilter: e.target.value }))}
                    >
                      <MenuItem value=''>Tất cả</MenuItem>
                      <MenuItem value='active'>Hoạt động</MenuItem>
                      <MenuItem value='inactive'>Không hoạt động</MenuItem>
                    </Select>
                  </FormControl>

                  <Button
                    variant='text'
                    onClick={() => {
                      setFilters({ searchTerm: '', roleFilter: '', statusFilter: '' })
                      setCurrentPage(1)
                    }}
                    sx={{
                      color: '#FF3B30',
                      textTransform: 'none',
                      fontWeight: 600,
                      ml: 'auto',
                    }}
                  >
                    Xóa bộ lọc
                  </Button>
                </Box>
              </Collapse>
            </Box>
          </CardContent>
        </Card>

        {/* Bulk Actions Toolbar */}
        {selectedUserIds.length > 0 && (
          <Card
            elevation={0}
            sx={{
              borderRadius: 3,
              bgcolor: '#007AFF10',
              border: '1px solid #007AFF',
              mb: 2,
            }}
          >
            <CardContent sx={{ p: 2 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant='body2' sx={{ fontWeight: 600, color: '#007AFF' }}>
                  Đã chọn {selectedUserIds.length} người dùng
                </Typography>
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <Button
                    size='small'
                    variant='contained'
                    startIcon={<CheckCircleIcon />}
                    onClick={() => handleBulkAction('activate')}
                    sx={{
                      bgcolor: '#34C759',
                      textTransform: 'none',
                      fontWeight: 600,
                      '&:hover': { bgcolor: '#2CA74C' },
                    }}
                  >
                    Kích hoạt
                  </Button>
                  <Button
                    size='small'
                    variant='contained'
                    startIcon={<CancelIcon />}
                    onClick={() => handleBulkAction('deactivate')}
                    sx={{
                      bgcolor: '#FF9500',
                      textTransform: 'none',
                      fontWeight: 600,
                      '&:hover': { bgcolor: '#E68600' },
                    }}
                  >
                    Vô hiệu
                  </Button>
                  <Button
                    size='small'
                    variant='contained'
                    startIcon={<AdminPanelSettingsIcon />}
                    onClick={() => handleBulkAction('changeRole')}
                    sx={{
                      bgcolor: '#8B5CF6',
                      textTransform: 'none',
                      fontWeight: 600,
                      '&:hover': { bgcolor: '#7C3AED' },
                    }}
                  >
                    Đổi vai trò
                  </Button>
                  <Button
                    size='small'
                    variant='contained'
                    startIcon={<DeleteIcon />}
                    onClick={() => handleBulkAction('delete')}
                    sx={{
                      bgcolor: '#FF3B30',
                      textTransform: 'none',
                      fontWeight: 600,
                      '&:hover': { bgcolor: '#E6342A' },
                    }}
                  >
                    Xóa
                  </Button>
                </Box>
              </Box>
            </CardContent>
          </Card>
        )}

        {/* Users Table */}
        <Card
          elevation={0}
          sx={{
            borderRadius: 3,
            bgcolor: 'white',
            border: '1px solid #d2d2d7',
          }}
        >
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell padding='checkbox'>
                      <Checkbox
                        checked={selectedUserIds.length === sortedUsers.length && sortedUsers.length > 0}
                        indeterminate={selectedUserIds.length > 0 && selectedUserIds.length < sortedUsers.length}
                        onChange={handleSelectAll}
                      />
                    </TableCell>
                    <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>
                      <TableSortLabel
                        active={sortField === 'fullName'}
                        direction={sortField === 'fullName' ? sortOrder : 'asc'}
                        onClick={() => handleSort('fullName')}
                      >
                        Người dùng
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>
                      <TableSortLabel
                        active={sortField === 'email'}
                        direction={sortField === 'email' ? sortOrder : 'asc'}
                        onClick={() => handleSort('email')}
                      >
                        Email
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>
                      <TableSortLabel
                        active={sortField === 'role'}
                        direction={sortField === 'role' ? sortOrder : 'asc'}
                        onClick={() => handleSort('role')}
                      >
                        Vai trò
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>Mã</TableCell>
                    <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>
                      <TableSortLabel
                        active={sortField === 'isActive'}
                        direction={sortField === 'isActive' ? sortOrder : 'asc'}
                        onClick={() => handleSort('isActive')}
                      >
                        Trạng thái
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ fontWeight: 700, fontSize: '0.875rem' }}>
                      <TableSortLabel
                        active={sortField === 'createdAt'}
                        direction={sortField === 'createdAt' ? sortOrder : 'asc'}
                        onClick={() => handleSort('createdAt')}
                      >
                        Ngày tạo
                      </TableSortLabel>
                    </TableCell>
                    <TableCell align='right' sx={{ fontWeight: 700, fontSize: '0.875rem' }}>
                      Thao tác
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {sortedUsers.map((user) => (
                    <TableRow
                      key={user.userId}
                      sx={{
                        '&:hover': {
                          background: 'rgba(0, 0, 0, 0.02)',
                        },
                        transition: 'background 0.2s',
                      }}
                    >
                      <TableCell padding='checkbox'>
                        <Checkbox
                          checked={selectedUserIds.includes(user.userId)}
                          onChange={() => handleSelectUser(user.userId)}
                        />
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                          <Avatar sx={{ width: 36, height: 36, bgcolor: getRoleColor(user.role), fontSize: '1rem' }}>
                            {user.fullName.charAt(0)}
                          </Avatar>
                          <Typography variant='body2' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
                            {user.fullName}
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
                          icon={getRoleIcon(user.role)}
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
                        <Typography variant='body2' color='text.secondary' sx={{ fontFamily: 'monospace' }}>
                          {user.role === 'Student' ? user.studentCode || '-' : user.role === 'Teacher' ? user.teacherCode || '-' : '-'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={user.isActive ? 'Hoạt động' : 'Vô hiệu'}
                          size='small'
                          sx={{
                            background: user.isActive ? '#10B98115' : '#EF444415',
                            color: user.isActive ? '#10B981' : '#EF4444',
                            fontWeight: 600,
                            borderRadius: 2,
                          }}
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant='body2' color='text.secondary'>
                          {new Date(user.createdAt).toLocaleDateString('vi-VN')}
                        </Typography>
                      </TableCell>
                      <TableCell align='right'>
                        <IconButton
                          size='small'
                          onClick={() => handleViewDetail(user.userId)}
                          sx={{
                            borderRadius: 2,
                            mr: 1,
                            '&:hover': {
                              background: 'rgba(0, 122, 255, 0.1)',
                              color: '#007AFF',
                            },
                          }}
                        >
                          <VisibilityIcon fontSize='small' />
                        </IconButton>
                        <IconButton
                          size='small'
                          onClick={() => handleDeleteClick(user.userId)}
                          sx={{
                            borderRadius: 2,
                            '&:hover': {
                              background: 'rgba(255, 59, 48, 0.1)',
                              color: '#FF3B30',
                            },
                          }}
                        >
                          <DeleteIcon fontSize='small' />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}

          {/* Pagination */}
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', p: 2, borderTop: '1px solid #d2d2d7' }}>
            <Typography variant='body2' color='text.secondary'>
              Hiển thị {(currentPage - 1) * pageSize + 1}-{Math.min(currentPage * pageSize, totalRecords)} / {totalRecords}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <IconButton size='small' disabled={currentPage === 1} onClick={() => setCurrentPage(currentPage - 1)}>
                <ChevronLeftIcon />
              </IconButton>
              <Typography variant='body2' sx={{ px: 2, py: 1, fontWeight: 600 }}>
                {currentPage} / {totalPages}
              </Typography>
              <IconButton size='small' disabled={currentPage === totalPages} onClick={() => setCurrentPage(currentPage + 1)}>
                <ChevronRightIcon />
              </IconButton>
            </Box>
          </Box>
        </Card>

        {/* User Detail Drawer */}
        <Drawer anchor='right' open={drawerOpen} onClose={handleCloseDrawer}>
          <Box sx={{ width: 500, p: 3 }}>
            {drawerLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', pt: 4 }}>
                <CircularProgress />
              </Box>
            ) : userDetail ? (
              <>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                  <Typography variant='h5' sx={{ fontWeight: 700 }}>
                    Chi tiết người dùng
                  </Typography>
                  <IconButton onClick={handleCloseDrawer}>
                    <CloseIcon />
                  </IconButton>
                </Box>

                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3, p: 3, bgcolor: '#F5F5F7', borderRadius: 3 }}>
                  <Avatar sx={{ width: 64, height: 64, bgcolor: getRoleColor(userDetail.role), fontSize: '1.5rem' }}>
                    {userDetail.fullName.charAt(0)}
                  </Avatar>
                  <Box>
                    <Typography variant='h6' sx={{ fontWeight: 700 }}>
                      {userDetail.fullName}
                    </Typography>
                    <Typography variant='body2' color='text.secondary'>
                      {userDetail.email}
                    </Typography>
                    <Chip
                      icon={getRoleIcon(userDetail.role)}
                      label={getRoleLabel(userDetail.role)}
                      size='small'
                      sx={{
                        background: `${getRoleColor(userDetail.role)}15`,
                        color: getRoleColor(userDetail.role),
                        fontWeight: 600,
                        borderRadius: 2,
                        mt: 1,
                      }}
                    />
                  </Box>
                </Box>

                <Tabs value={drawerTab} onChange={(_, v) => setDrawerTab(v)} sx={{ mb: 3 }}>
                  <Tab label='Tổng quan' value='overview' />
                  <Tab label='Chỉnh sửa' value='edit' />
                </Tabs>

                {drawerTab === 'overview' && (
                  <Box>
                    <Typography variant='subtitle2' sx={{ fontWeight: 700, mb: 2, color: '#86868b' }}>
                      THÔNG TIN CƠ BẢN
                    </Typography>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mb: 3 }}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant='body2' color='text.secondary'>
                          Mã sinh viên/giáo viên:
                        </Typography>
                        <Typography variant='body2' sx={{ fontWeight: 600, fontFamily: 'monospace' }}>
                          {userDetail.role === 'Student'
                            ? userDetail.studentCode || '-'
                            : userDetail.role === 'Teacher'
                              ? userDetail.teacherCode || '-'
                              : '-'}
                        </Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant='body2' color='text.secondary'>
                          Số điện thoại:
                        </Typography>
                        <Typography variant='body2' sx={{ fontWeight: 600 }}>
                          {userDetail.phone || '-'}
                        </Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant='body2' color='text.secondary'>
                          Email xác thực:
                        </Typography>
                        <Chip
                          label={userDetail.emailVerified ? 'Đã xác thực' : 'Chưa xác thực'}
                          size='small'
                          sx={{
                            background: userDetail.emailVerified ? '#34C75915' : '#FF3B3015',
                            color: userDetail.emailVerified ? '#34C759' : '#FF3B30',
                            fontWeight: 600,
                          }}
                        />
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant='body2' color='text.secondary'>
                          Trạng thái:
                        </Typography>
                        <Chip
                          label={userDetail.isActive ? 'Hoạt động' : 'Vô hiệu'}
                          size='small'
                          sx={{
                            background: userDetail.isActive ? '#10B98115' : '#EF444415',
                            color: userDetail.isActive ? '#10B981' : '#EF4444',
                            fontWeight: 600,
                          }}
                        />
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant='body2' color='text.secondary'>
                          Ngày tạo:
                        </Typography>
                        <Typography variant='body2' sx={{ fontWeight: 600 }}>
                          {new Date(userDetail.createdAt).toLocaleDateString('vi-VN')}
                        </Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant='body2' color='text.secondary'>
                          Đăng nhập gần nhất:
                        </Typography>
                        <Typography variant='body2' sx={{ fontWeight: 600 }}>
                          {userDetail.lastLoginAt ? new Date(userDetail.lastLoginAt).toLocaleString('vi-VN') : 'Chưa có'}
                        </Typography>
                      </Box>
                    </Box>

                    <Typography variant='subtitle2' sx={{ fontWeight: 700, mb: 2, color: '#86868b' }}>
                      THỐNG KÊ HOẠT ĐỘNG
                    </Typography>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant='body2' color='text.secondary'>
                          Lớp học:
                        </Typography>
                        <Typography variant='body2' sx={{ fontWeight: 600 }}>
                          {userDetail.role === 'Teacher'
                            ? `${userDetail.classCount || 0} lớp`
                            : userDetail.role === 'Student'
                              ? `${userDetail.enrolledClassCount || 0} lớp`
                              : '-'}
                        </Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant='body2' color='text.secondary'>
                          Bài tập:
                        </Typography>
                        <Typography variant='body2' sx={{ fontWeight: 600 }}>
                          {userDetail.totalAssignments || 0}
                        </Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant='body2' color='text.secondary'>
                          Bài nộp:
                        </Typography>
                        <Typography variant='body2' sx={{ fontWeight: 600 }}>
                          {userDetail.totalSubmissions || 0}
                        </Typography>
                      </Box>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant='body2' color='text.secondary'>
                          Điểm TB:
                        </Typography>
                        <Typography variant='body2' sx={{ fontWeight: 600 }}>
                          {userDetail.averageScore != null ? userDetail.averageScore.toFixed(2) : '-'}
                        </Typography>
                      </Box>
                    </Box>
                  </Box>
                )}

                {drawerTab === 'edit' && (
                  <Box>
                    <Typography variant='body2' color='text.secondary'>
                      Tính năng chỉnh sửa đang được phát triển...
                    </Typography>
                  </Box>
                )}
              </>
            ) : null}
          </Box>
        </Drawer>

        {/* Delete Confirmation Dialog */}
        <Dialog
          open={deleteDialogOpen}
          onClose={() => setDeleteDialogOpen(false)}
          PaperProps={{
            sx: { borderRadius: 3 },
          }}
        >
          <DialogTitle sx={{ fontWeight: 700 }}>Xác nhận xóa</DialogTitle>
          <DialogContent>
            <DialogContentText>Bạn có chắc chắn muốn xóa người dùng này? Hành động này không thể hoàn tác.</DialogContentText>
          </DialogContent>
          <DialogActions sx={{ p: 2 }}>
            <Button onClick={() => setDeleteDialogOpen(false)} sx={{ textTransform: 'none', fontWeight: 600 }}>
              Hủy
            </Button>
            <Button
              onClick={handleDelete}
              variant='contained'
              sx={{
                bgcolor: '#FF3B30',
                textTransform: 'none',
                fontWeight: 600,
                '&:hover': { bgcolor: '#E6342A' },
              }}
            >
              Xóa
            </Button>
          </DialogActions>
        </Dialog>

        {/* Add User Dialog */}
        <Dialog
          open={addUserDialogOpen}
          onClose={handleCloseAddUserDialog}
          maxWidth='sm'
          fullWidth
          PaperProps={{
            sx: { borderRadius: 3 },
          }}
        >
          <DialogTitle sx={{ fontWeight: 700, fontSize: '1.5rem' }}>Thêm người dùng mới</DialogTitle>
          <DialogContent sx={{ pt: 3 }}>
            <Stack spacing={3}>
              {/* Full Name */}
              <TextField
                label='Họ và tên'
                fullWidth
                required
                value={addUserForm.fullName}
                onChange={(e) => handleAddUserFormChange('fullName', e.target.value)}
                error={!!addUserErrors.fullName}
                helperText={addUserErrors.fullName}
                size='medium'
              />

              {/* Email */}
              <TextField
                label='Email'
                type='email'
                fullWidth
                required
                value={addUserForm.email}
                onChange={(e) => handleAddUserFormChange('email', e.target.value)}
                error={!!addUserErrors.email}
                helperText={addUserErrors.email}
                size='medium'
              />

              {/* Password */}
              <TextField
                label='Mật khẩu'
                type='password'
                fullWidth
                required
                value={addUserForm.password}
                onChange={(e) => handleAddUserFormChange('password', e.target.value)}
                error={!!addUserErrors.password}
                helperText={addUserErrors.password || 'Tối thiểu 6 ký tự'}
                size='medium'
              />

              {/* Confirm Password */}
              <TextField
                label='Xác nhận mật khẩu'
                type='password'
                fullWidth
                required
                value={addUserForm.confirmPassword}
                onChange={(e) => handleAddUserFormChange('confirmPassword', e.target.value)}
                error={!!addUserErrors.confirmPassword}
                helperText={addUserErrors.confirmPassword}
                size='medium'
              />

              {/* Role */}
              <FormControl fullWidth required error={!!addUserErrors.role}>
                <InputLabel>Vai trò</InputLabel>
                <Select value={addUserForm.role} label='Vai trò' onChange={(e) => handleAddUserFormChange('role', e.target.value)}>
                  <MenuItem value='Admin'>Quản trị viên</MenuItem>
                  <MenuItem value='Teacher'>Giảng viên</MenuItem>
                  <MenuItem value='Student'>Sinh viên</MenuItem>
                </Select>
                {addUserErrors.role && (
                  <Typography variant='caption' color='error' sx={{ mt: 0.5, ml: 1.75 }}>
                    {addUserErrors.role}
                  </Typography>
                )}
              </FormControl>

              {/* Conditional: Student Code */}
              {addUserForm.role === 'Student' && (
                <TextField
                  label='Mã sinh viên'
                  fullWidth
                  required
                  value={addUserForm.studentCode}
                  onChange={(e) => handleAddUserFormChange('studentCode', e.target.value)}
                  error={!!addUserErrors.studentCode}
                  helperText={addUserErrors.studentCode}
                  size='medium'
                />
              )}

              {/* Conditional: Teacher Code & Phone */}
              {addUserForm.role === 'Teacher' && (
                <>
                  <TextField
                    label='Mã giảng viên'
                    fullWidth
                    required
                    value={addUserForm.teacherCode}
                    onChange={(e) => handleAddUserFormChange('teacherCode', e.target.value)}
                    error={!!addUserErrors.teacherCode}
                    helperText={addUserErrors.teacherCode}
                    size='medium'
                  />
                  <TextField
                    label='Số điện thoại'
                    fullWidth
                    value={addUserForm.phone}
                    onChange={(e) => handleAddUserFormChange('phone', e.target.value)}
                    error={!!addUserErrors.phone}
                    helperText={addUserErrors.phone}
                    size='medium'
                  />
                </>
              )}

              {/* Active Status */}
              <FormControl component='fieldset'>
                <Stack direction='row' spacing={1} alignItems='center'>
                  <Checkbox
                    checked={addUserForm.isActive}
                    onChange={(e) => handleAddUserFormChange('isActive', e.target.checked)}
                    sx={{
                      color: '#007AFF',
                      '&.Mui-checked': {
                        color: '#007AFF',
                      },
                    }}
                  />
                  <Typography variant='body2' color='text.secondary'>
                    Kích hoạt tài khoản ngay
                  </Typography>
                </Stack>
              </FormControl>
            </Stack>
          </DialogContent>
          <DialogActions sx={{ p: 3, pt: 2 }}>
            <Button onClick={handleCloseAddUserDialog} disabled={addUserLoading} sx={{ textTransform: 'none', fontWeight: 600 }}>
              Hủy
            </Button>
            <Button
              onClick={handleCreateUser}
              variant='contained'
              disabled={addUserLoading}
              sx={{
                bgcolor: '#007AFF',
                textTransform: 'none',
                fontWeight: 600,
                px: 3,
                '&:hover': { bgcolor: '#0051D5' },
              }}
            >
              {addUserLoading ? <CircularProgress size={24} color='inherit' /> : 'Thêm người dùng'}
            </Button>
          </DialogActions>
        </Dialog>

        {/* Bulk Action Confirmation Dialog */}
        <Dialog
          open={bulkActionDialogOpen}
          onClose={() => setBulkActionDialogOpen(false)}
          PaperProps={{
            sx: { borderRadius: 3 },
          }}
        >
          <DialogTitle sx={{ fontWeight: 700 }}>Xác nhận thao tác hàng loạt</DialogTitle>
          <DialogContent>
            <DialogContentText sx={{ mb: 2 }}>
              {bulkActionType === 'activate' && `Bạn có chắc chắn muốn kích hoạt ${selectedUserIds.length} người dùng?`}
              {bulkActionType === 'deactivate' && `Bạn có chắc chắn muốn vô hiệu hóa ${selectedUserIds.length} người dùng?`}
              {bulkActionType === 'delete' &&
                `Bạn có chắc chắn muốn xóa ${selectedUserIds.length} người dùng? Hành động này không thể hoàn tác.`}
              {bulkActionType === 'changeRole' && `Chọn vai trò mới cho ${selectedUserIds.length} người dùng:`}
            </DialogContentText>

            {bulkActionType === 'changeRole' && (
              <FormControl fullWidth size='small'>
                <InputLabel>Vai trò mới</InputLabel>
                <Select value={bulkNewRole} label='Vai trò mới' onChange={(e) => setBulkNewRole(e.target.value)}>
                  <MenuItem value='Admin'>Admin</MenuItem>
                  <MenuItem value='Teacher'>Giáo viên</MenuItem>
                  <MenuItem value='Student'>Sinh viên</MenuItem>
                </Select>
              </FormControl>
            )}
          </DialogContent>
          <DialogActions sx={{ p: 2 }}>
            <Button onClick={() => setBulkActionDialogOpen(false)} sx={{ textTransform: 'none', fontWeight: 600 }}>
              Hủy
            </Button>
            <Button
              onClick={handleExecuteBulkAction}
              variant='contained'
              disabled={bulkActionType === 'changeRole' && !bulkNewRole}
              sx={{
                bgcolor:
                  bulkActionType === 'activate'
                    ? '#34C759'
                    : bulkActionType === 'deactivate'
                      ? '#FF9500'
                      : bulkActionType === 'changeRole'
                        ? '#8B5CF6'
                        : '#FF3B30',
                textTransform: 'none',
                fontWeight: 600,
                '&:hover': {
                  bgcolor:
                    bulkActionType === 'activate'
                      ? '#2CA74C'
                      : bulkActionType === 'deactivate'
                        ? '#E68600'
                        : bulkActionType === 'changeRole'
                          ? '#7C3AED'
                          : '#E6342A',
                },
              }}
            >
              Xác nhận
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </Box>
  )
}
