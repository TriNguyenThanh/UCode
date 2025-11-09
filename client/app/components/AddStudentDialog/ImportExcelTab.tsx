import { useState } from 'react'
import {
  Box,
  Button,
  Typography,
  Alert,
  CircularProgress,
  Stepper,
  Step,
  StepLabel,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Stack,
  Snackbar,
} from '@mui/material'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import * as XLSX from 'xlsx'
import { API } from '../../api'
import type { ApiResponse } from '../../types'
import { bulkEnrollStudents } from '../../services/classService'

interface ImportExcelTabProps {
  classId: string
  onSuccess: () => void
}

interface ParsedStudent {
  rowNumber: number
  studentCode: string
  fullName: string
  email: string
  password: string
  major: string
  enrollmentYear: number
  classYear: number
}

interface StudentValidation extends ParsedStudent {
  status: 'exists' | 'new' | 'error'
  existingUserId?: string
  errorMessage?: string
}

const steps = ['Tải file', 'Kiểm tra dữ liệu', 'Xác nhận & Import']

export default function ImportExcelTab({ classId, onSuccess }: ImportExcelTabProps) {
  const [activeStep, setActiveStep] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [parsedData, setParsedData] = useState<ParsedStudent[]>([])
  const [validationResults, setValidationResults] = useState<StudentValidation[]>([])

  const handleDownloadTemplate = () => {
    const template = [
      ['StudentCode', 'FullName', 'Email', 'Password', 'Major', 'EnrollmentYear'],
      ['SV001', 'Nguyễn Văn A', 'sv001@example.com', '123456', 'Công nghệ phần mềm', 2024],
      ['SV002', 'Trần Thị B', 'sv002@example.com', '123456', 'Khoa học máy tính', 2023],
    ]

    const ws = XLSX.utils.aoa_to_sheet(template)
    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, 'Students')
    XLSX.writeFile(wb, `Mau_Import_SinhVien_${new Date().getTime()}.xlsx`)
  }

  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    setLoading(true)
    setError(null)
    setParsedData([])
    setValidationResults([])

    try {
      const arrayBuffer = await file.arrayBuffer()
      const workbook = XLSX.read(arrayBuffer, { type: 'array' })
      const worksheet = workbook.Sheets[workbook.SheetNames[0]]
      const jsonData = XLSX.utils.sheet_to_json(worksheet) as any[]

      const parsed: ParsedStudent[] = []
      const currentYear = new Date().getFullYear()
      
      for (let i = 0; i < jsonData.length; i++) {
        const row = jsonData[i]
        
        // Validate required fields
        if (!row.StudentCode || !row.FullName || !row.Email || !row.Password) {
          continue
        }

        const enrollmentYear = parseInt(row.EnrollmentYear) || currentYear
        // Tính ClassYear dựa trên năm hiện tại - năm nhập học + 1
        // Ví dụ: nhập học 2024, hiện tại 2025 => năm 2
        let classYear = currentYear - enrollmentYear + 1
        // Đảm bảo ClassYear trong khoảng 1-6 (yêu cầu của backend)
        classYear = Math.max(1, Math.min(6, classYear))

        parsed.push({
          rowNumber: i + 2, // +2 because Excel is 1-indexed and we skip header
          studentCode: row.StudentCode.toString().trim(),
          fullName: row.FullName.toString().trim(),
          email: row.Email.toString().trim(),
          password: row.Password.toString().trim(),
          major: row.Major?.toString().trim() || '',
          enrollmentYear: enrollmentYear,
          classYear: classYear,
        })
      }

      if (parsed.length === 0) {
        setError('Không tìm thấy dữ liệu hợp lệ trong file Excel')
        setLoading(false)
        return
      }

      setParsedData(parsed)
      setActiveStep(1)

      // Auto-validate
      await handleValidate(parsed)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không thể đọc file Excel')
      setLoading(false)
    }

    event.target.value = ''
  }

  const handleValidate = async (data: ParsedStudent[]) => {
    setLoading(true)
    setError(null)

    try {
      const results: StudentValidation[] = []

      // Check each student exists or not
      for (const student of data) {
        try {
          // Try to get student by code
          const response = await API.get<ApiResponse<any>>(
            `api/v1/students/by-student-code/${student.studentCode}`
          )

          if (response.data.success && response.data.data) {
            results.push({
              ...student,
              status: 'exists',
              existingUserId: response.data.data.userId,
            })
          }
        } catch (err: any) {
          // Student not found - will create new
          if (err.response?.status === 404) {
            results.push({
              ...student,
              status: 'new',
            })
          } else {
            results.push({
              ...student,
              status: 'error',
              errorMessage: 'Lỗi kiểm tra sinh viên',
            })
          }
        }
      }

      setValidationResults(results)
      setActiveStep(2)
    } catch (err) {
      setError('Không thể kiểm tra dữ liệu')
    } finally {
      setLoading(false)
    }
  }

  const handleImport = async () => {
    setLoading(true)
    setError(null)

    try {
      const newStudents = validationResults.filter((r) => r.status === 'new')
      const existingStudents = validationResults.filter((r) => r.status === 'exists')
      const createdUserIds: string[] = []

      // Create new students
      for (const student of newStudents) {
        try {
          const response = await API.post<ApiResponse<any>>('api/v1/students/create', {
            studentCode: student.studentCode,
            username: student.studentCode,
            email: student.email,
            password: student.password,
            fullName: student.fullName,
            major: student.major,
            enrollmentYear: student.enrollmentYear,
            classYear: student.classYear,
          })

          if (response.data.success && response.data.data) {
            createdUserIds.push(response.data.data.userId)
          }
        } catch (err: any) {
          console.error('Failed to create student:', student.studentCode)
          console.error('Error response:', err.response?.data)
          console.error('Validation errors:', err.response?.data?.errors)
          console.error('Request data:', {
            studentCode: student.studentCode,
            username: student.studentCode,
            email: student.email,
            fullName: student.fullName,
            major: student.major,
            enrollmentYear: student.enrollmentYear,
            classYear: student.classYear,
          })
        }
      }

      // Collect all user IDs (existing + newly created)
      const allUserIds = [
        ...existingStudents.map((s) => s.existingUserId!),
        ...createdUserIds,
      ]

      // Bulk enroll all students
      if (allUserIds.length > 0) {
        const result = await bulkEnrollStudents(classId, allUserIds)

        setSuccessMessage(
          `✓ Đã tạo ${createdUserIds.length} sinh viên mới và thêm ${result.successCount} sinh viên vào lớp!`
        )

        setTimeout(() => {
          onSuccess()
        }, 2000)
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không thể import sinh viên')
    } finally {
      setLoading(false)
    }
  }

  const handleReset = () => {
    setActiveStep(0)
    setParsedData([])
    setValidationResults([])
    setError(null)
  }

  const newCount = validationResults.filter((r) => r.status === 'new').length
  const existsCount = validationResults.filter((r) => r.status === 'exists').length
  const errorCount = validationResults.filter((r) => r.status === 'error').length

  return (
    <Box sx={{ py: 2 }}>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        Import sinh viên từ file Excel. Tải xuống file mẫu để bắt đầu.
      </Typography>

      <Stepper activeStep={activeStep} sx={{ my: 3 }}>
        {steps.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Step 0: Upload File */}
      {activeStep === 0 && (
        <Stack spacing={2}>
          <Button variant="outlined" onClick={handleDownloadTemplate} disabled={loading}>
            Tải xuống file mẫu
          </Button>

          <Button variant="contained" component="label" disabled={loading}>
            Tải lên file Excel
            <input type="file" accept=".xlsx,.xls" hidden onChange={handleFileUpload} />
          </Button>

          {loading && (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 2 }}>
              <CircularProgress />
            </Box>
          )}
        </Stack>
      )}

      {/* Step 1: Validating */}
      {activeStep === 1 && (
        <Box sx={{ textAlign: 'center', py: 4 }}>
          <CircularProgress />
          <Typography variant="body2" sx={{ mt: 2 }}>
            Đang kiểm tra {parsedData.length} sinh viên...
          </Typography>
        </Box>
      )}

      {/* Step 2: Confirm & Import */}
      {activeStep === 2 && (
        <Box>
          <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
            {newCount > 0 && (
              <Alert severity="info" sx={{ flexGrow: 1 }}>
                <strong>{newCount}</strong> sinh viên mới sẽ được tạo
              </Alert>
            )}
            {existsCount > 0 && (
              <Alert severity="success" sx={{ flexGrow: 1 }}>
                <strong>{existsCount}</strong> sinh viên đã tồn tại
              </Alert>
            )}
            {errorCount > 0 && (
              <Alert severity="error" sx={{ flexGrow: 1 }}>
                <strong>{errorCount}</strong> lỗi
              </Alert>
            )}
          </Stack>

          <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 400 }}>
            <Table size="small" stickyHeader>
              <TableHead>
                <TableRow>
                  <TableCell>Dòng</TableCell>
                  <TableCell>MSSV</TableCell>
                  <TableCell>Họ tên</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Ngành</TableCell>
                  <TableCell>Năm vào học</TableCell>
                  <TableCell>Năm học</TableCell>
                  <TableCell>Trạng thái</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {validationResults.map((result, idx) => (
                  <TableRow
                    key={idx}
                    sx={{
                      bgcolor:
                        result.status === 'error'
                          ? 'error.light'
                          : result.status === 'new'
                            ? 'info.light'
                            : 'success.light',
                    }}
                  >
                    <TableCell>{result.rowNumber}</TableCell>
                    <TableCell>{result.studentCode}</TableCell>
                    <TableCell>{result.fullName}</TableCell>
                    <TableCell>{result.email}</TableCell>
                    <TableCell>{result.major}</TableCell>
                    <TableCell>{result.enrollmentYear}</TableCell>
                    <TableCell>Năm {result.classYear}</TableCell>
                    <TableCell>
                      <Chip
                        label={
                          result.status === 'new'
                            ? 'Sẽ tạo mới'
                            : result.status === 'exists'
                              ? 'Đã tồn tại'
                              : result.errorMessage || 'Lỗi'
                        }
                        color={
                          result.status === 'new'
                            ? 'info'
                            : result.status === 'exists'
                              ? 'success'
                              : 'error'
                        }
                        size="small"
                      />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
            <Button onClick={handleReset} disabled={loading}>
              Làm lại
            </Button>
            <Button
              variant="contained"
              color="success"
              onClick={handleImport}
              disabled={loading || (newCount === 0 && existsCount === 0)}
            >
              {loading ? (
                <CircularProgress size={24} />
              ) : (
                `Import ${newCount + existsCount} sinh viên`
              )}
            </Button>
          </Box>
        </Box>
      )}

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
        >
          {successMessage}
        </Alert>
      </Snackbar>
    </Box>
  )
}
