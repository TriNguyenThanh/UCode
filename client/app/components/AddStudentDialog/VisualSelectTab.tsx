import { useState, useEffect } from 'react'
import {
  Box,
  TextField,
  Button,
  Typography,
  Alert,
  CircularProgress,
  Checkbox,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Paper,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Stack,
  Chip,
  Snackbar,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import { getAvailableStudents } from '../../services/studentService'
import { bulkEnrollStudents } from '../../services/classService'
import type { StudentResponse } from '../../types'

interface VisualSelectTabProps {
  classId: string
  onSuccess: () => void
}

export default function VisualSelectTab({ classId, onSuccess }: VisualSelectTabProps) {
  const [students, setStudents] = useState<StudentResponse[]>([])
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  
  // Pagination & Filters
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [totalCount, setTotalCount] = useState(0)
  const [searchQuery, setSearchQuery] = useState('')
  const [yearFilter, setYearFilter] = useState('')
  const [majorFilter, setMajorFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('') // Default to ALL students
  
  // Submitting
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    loadStudents()
  }, [classId, page, rowsPerPage, yearFilter, majorFilter, statusFilter])

  const loadStudents = async () => {
    setLoading(true)
    setError(null)
    try {
      console.log('🔍 Loading students with params:', {
        pageNumber: page + 1,
        pageSize: rowsPerPage,
        excludeClassId: classId,
        year: yearFilter ? parseInt(yearFilter) : undefined,
        major: majorFilter || undefined,
        status: statusFilter || undefined,
      })

      const result = await getAvailableStudents({
        pageNumber: page + 1,
        pageSize: rowsPerPage,
        excludeClassId: classId,
        year: yearFilter ? parseInt(yearFilter) : undefined,
        major: majorFilter || undefined,
        status: statusFilter || undefined,
      })

      console.log('✅ API Response:', result)
      console.log('📊 Students data:', result.data)
      console.log('📈 Total count:', result.totalCount)

      setStudents(result.data || [])
      setTotalCount(result.totalCount)
    } catch (err) {
      console.error('❌ Failed to load students:', err)
      setError(err instanceof Error ? err.message : 'Không thể tải danh sách sinh viên')
      setStudents([])
      setTotalCount(0)
    } finally {
      setLoading(false)
    }
  }

  // Client-side search filter
  const filteredStudents = students.filter((student) => {
    if (!searchQuery) return true
    const query = searchQuery.toLowerCase()
    return (
      student.fullName?.toLowerCase().includes(query) ||
      student.studentCode?.toLowerCase().includes(query) ||
      student.email?.toLowerCase().includes(query)
    )
  })

  const handleToggleAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      const newSelected = new Set(filteredStudents.map((s) => s.userId))
      setSelectedIds(newSelected)
    } else {
      setSelectedIds(new Set())
    }
  }

  const handleToggle = (userId: string) => {
    const newSelected = new Set(selectedIds)
    if (newSelected.has(userId)) {
      newSelected.delete(userId)
    } else {
      newSelected.add(userId)
    }
    setSelectedIds(newSelected)
  }

  const handleAdd = async () => {
    if (selectedIds.size === 0) {
      setError('Vui lòng chọn ít nhất 1 sinh viên')
      return
    }

    setSubmitting(true)
    setError(null)

    try {
      const result = await bulkEnrollStudents(classId, Array.from(selectedIds))
      
      if (result.successCount > 0) {
        setSuccessMessage(`Đã thêm ${result.successCount} sinh viên vào lớp!`)
        setTimeout(() => {
          onSuccess()
        }, 1500) // Delay to show success message
      }

      if (result.errors && result.errors.length > 0) {
        const errorMsg = result.errors.map((e) => e.errorMessage).join(', ')
        setError(`Có lỗi: ${errorMsg}`)
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không thể thêm sinh viên')
    } finally {
      setSubmitting(false)
    }
  }

  const currentYear = new Date().getFullYear()
  const years = Array.from({ length: 10 }, (_, i) => currentYear - i)

  return (
    <Box sx={{ py: 2 }}>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        Chọn sinh viên từ danh sách để thêm vào lớp
      </Typography>

      {/* Filters */}
      <Stack direction="row" spacing={2} sx={{ my: 2 }}>
        <TextField
          size="small"
          label="Tìm kiếm"
          placeholder="Tên, MSSV, hoặc Email"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          sx={{ flexGrow: 1 }}
          InputProps={{
            startAdornment: <SearchIcon sx={{ color: 'text.secondary', mr: 1 }} />,
          }}
        />
        
        <FormControl size="small" sx={{ minWidth: 150 }}>
          <InputLabel>Năm vào học</InputLabel>
          <Select 
            value={yearFilter} 
            onChange={(e) => {
              setYearFilter(e.target.value)
              setPage(0)
            }} 
            label="Năm vào học"
          >
            <MenuItem value="">Tất cả</MenuItem>
            {years.map((year) => (
              <MenuItem key={year} value={year.toString()}>
                {year}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <TextField
          size="small"
          label="Ngành"
          placeholder="VD: Công nghệ phần mềm"
          value={majorFilter}
          onChange={(e) => {
            setMajorFilter(e.target.value)
            setPage(0)
          }}
          sx={{ minWidth: 200 }}
        />

        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>Trạng thái</InputLabel>
          <Select 
            value={statusFilter} 
            onChange={(e) => {
              setStatusFilter(e.target.value)
              setPage(0)
            }} 
            label="Trạng thái"
          >
            <MenuItem value="">Tất cả</MenuItem>
            <MenuItem value="Active">Active</MenuItem>
            <MenuItem value="Inactive">Inactive</MenuItem>
          </Select>
        </FormControl>
      </Stack>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Student Table */}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : filteredStudents.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography color="text.secondary">
            {searchQuery ? 'Không tìm thấy sinh viên nào' : 'Không có sinh viên available'}
          </Typography>
        </Paper>
      ) : (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell padding="checkbox">
                    <Checkbox
                      indeterminate={selectedIds.size > 0 && selectedIds.size < filteredStudents.length}
                      checked={filteredStudents.length > 0 && selectedIds.size === filteredStudents.length}
                      onChange={handleToggleAll}
                      disabled={loading}
                    />
                  </TableCell>
                  <TableCell>Họ và tên</TableCell>
                  <TableCell>MSSV</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Ngành</TableCell>
                  <TableCell>Năm vào học</TableCell>
                  <TableCell>Trạng thái</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredStudents.map((student) => (
                  <TableRow 
                    key={student.userId} 
                    hover
                    onClick={() => handleToggle(student.userId)}
                    sx={{ cursor: 'pointer' }}
                  >
                    <TableCell padding="checkbox">
                      <Checkbox
                        checked={selectedIds.has(student.userId)}
                        onChange={() => handleToggle(student.userId)}
                      />
                    </TableCell>
                    <TableCell>{student.fullName}</TableCell>
                    <TableCell>{student.studentCode}</TableCell>
                    <TableCell>{student.email}</TableCell>
                    <TableCell>{student.major}</TableCell>
                    <TableCell>{student.enrollmentYear}</TableCell>
                    <TableCell>
                      <Chip 
                        label={student.status}
                        color={student.status === 'Active' ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <TablePagination
            component="div"
            count={totalCount}
            page={page}
            onPageChange={(_e, newPage) => {
              setPage(newPage)
              setSelectedIds(new Set()) // Clear selection when changing page
            }}
            rowsPerPage={rowsPerPage}
            onRowsPerPageChange={(e) => {
              setRowsPerPage(parseInt(e.target.value, 10))
              setPage(0)
              setSelectedIds(new Set())
            }}
            rowsPerPageOptions={[5, 10, 25, 50]}
            labelRowsPerPage="Số dòng mỗi trang:"
            labelDisplayedRows={({ from, to, count }) =>
              `${from}-${to} trong ${count !== -1 ? count : `hơn ${to}`}`
            }
          />
        </>
      )}

      {/* Action Buttons */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 2 }}>
        <Typography variant="body2" color="text.secondary">
          Đã chọn: <strong>{selectedIds.size}</strong> sinh viên
        </Typography>
        <Button
          variant="contained"
          color="success"
          onClick={handleAdd}
          disabled={loading || submitting || selectedIds.size === 0}
        >
          {submitting ? <CircularProgress size={24} /> : `Thêm ${selectedIds.size} sinh viên`}
        </Button>
      </Box>

      {/* Success Snackbar */}
      <Snackbar
        open={!!successMessage}
        autoHideDuration={3000}
        onClose={() => setSuccessMessage(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert
          onClose={() => setSuccessMessage(null)}
          severity="success"
          icon={<CheckCircleIcon />}
          sx={{ width: '100%', fontSize: '1rem' }}
        >
          {successMessage}
        </Alert>
      </Snackbar>
    </Box>
  )
}
