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
import { validateStudentsBulk, bulkCreateStudents } from '../../services/studentService'

interface ImportExcelTabProps {
  classId: string
  onSuccess: () => void
}

interface ParsedStudent {
  rowNumber: number
  studentCode: string
  fullName: string
  email: string
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
      // Header with instructions
      ['HƯỚNG DẪN IMPORT SINH VIÊN', '', '', '', ''],
      ['Mật khẩu mặc định cho tất cả sinh viên: 123456', '', '', '', ''],
      ['', '', '', '', ''],
      // Column headers
      ['Mã sinh viên', 'Họ và tên', 'Email', 'Chuyên ngành', 'Năm nhập học'],
      ['(Bắt buộc)', '(Bắt buộc)', '(Bắt buộc)', '(Bắt buộc)', '(Bắt buộc)'],
      // Sample data
      ['SV001', 'Nguyễn Văn A', 'sv001@example.com', 'Công nghệ phần mềm', 2024],
      ['SV002', 'Trần Thị B', 'sv002@example.com', 'Khoa học máy tính', 2024],
      ['SV003', 'Lê Văn C', 'sv003@example.com', 'An toàn thông tin', 2023],
    ]

    const ws = XLSX.utils.aoa_to_sheet(template)
    
    // Merge cells for title
    ws['!merges'] = [
      { s: { r: 0, c: 0 }, e: { r: 0, c: 4 } }, // Title row
      { s: { r: 1, c: 0 }, e: { r: 1, c: 4 } }, // Password info row
    ]
    
    // Set column widths
    ws['!cols'] = [
      { wch: 15 }, // StudentCode
      { wch: 25 }, // FullName
      { wch: 30 }, // Email
      { wch: 25 }, // Major
      { wch: 15 }, // EnrollmentYear
    ]
    
    // Style the cells (basic styling)
    // Title row - bold and centered
    if (ws['A1']) ws['A1'].s = { font: { bold: true, sz: 14 }, alignment: { horizontal: 'center' } }
    if (ws['A2']) ws['A2'].s = { font: { bold: true, color: { rgb: 'FF0000' } }, alignment: { horizontal: 'center' } }
    
    // Header row - bold and background color
    const headerCells = ['A4', 'B4', 'C4', 'D4', 'E4']
    headerCells.forEach(cell => {
      if (ws[cell]) ws[cell].s = { 
        font: { bold: true }, 
        fill: { fgColor: { rgb: '4472C4' } },
        alignment: { horizontal: 'center' }
      }
    })
    
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
      
      // Read from row 4 (skip instructions and header rows)
      // Range starts at A6 (row 6, after header at row 4-5)
      const jsonData = XLSX.utils.sheet_to_json(worksheet, { 
        range: 5, // Start from row 6 (0-indexed, so 5)
        header: ['StudentCode', 'FullName', 'Email', 'Major', 'EnrollmentYear'],
        defval: ''
      }) as any[]

      const parsed: ParsedStudent[] = []
      const currentYear = new Date().getFullYear()
      
      for (let i = 0; i < jsonData.length; i++) {
        const row = jsonData[i]
        
        // Validate required fields (removed password requirement)
        if (!row.StudentCode || !row.FullName || !row.Email) {
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
      // Extract all student codes
      const studentCodes = data.map((s) => s.studentCode)

      // Single API call to validate all students at once (optimized!)
      const bulkResults = await validateStudentsBulk(studentCodes)

      // Create a map for quick lookup
      const validationMap = new Map(bulkResults.map((r) => [r.studentCode, r]))

      // Map results to UI format
      const results: StudentValidation[] = data.map((student) => {
        const validation = validationMap.get(student.studentCode)

        if (!validation) {
          return {
            ...student,
            status: 'error',
            errorMessage: 'Không thể kiểm tra sinh viên',
          }
        }

        if (validation.exists) {
          return {
            ...student,
            status: 'exists',
            existingUserId: validation.userId,
          }
        }

        return {
          ...student,
          status: 'new',
        }
      })

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
      let createdUserIds: string[] = []

      // 1. Bulk create new students (SINGLE API CALL instead of N calls!)
      if (newStudents.length > 0) {
        const studentsToCreate = newStudents.map((student) => ({
          studentCode: student.studentCode,
          username: student.studentCode,
          email: student.email,
          password: '123456', // Default password, will be generated by backend
          fullName: student.fullName,
          major: student.major || '',
          enrollmentYear: student.enrollmentYear || new Date().getFullYear(),
          classYear: student.classYear || 1,
        }))

        const createResult = await bulkCreateStudents(studentsToCreate)
        
        // Get successfully created student IDs
        createdUserIds = createResult.results
          .filter((r) => r.success && r.userId)
          .map((r) => r.userId!)

        console.log(`✓ Created ${createResult.successCount}/${newStudents.length} students`)
        
        if (createResult.failureCount > 0) {
          console.warn('Failed students:', createResult.results.filter(r => !r.success))
        }
      }

      // 2. Bulk enroll all students to class (SINGLE API CALL!)
      const allUserIds = [
        ...existingStudents.map((s) => s.existingUserId!).filter(Boolean),
        ...createdUserIds,
      ]

      let enrollResult
      if (allUserIds.length > 0) {
        enrollResult = await bulkEnrollStudents(classId, allUserIds)
        console.log(`✓ Enrolled ${enrollResult.successCount}/${allUserIds.length} students`)
      }

      // Show success message
      const totalCreated = createdUserIds.length
      const totalEnrolled = enrollResult?.successCount || 0
      const totalFailed = (enrollResult?.failureCount || 0)

      setSuccessMessage(
        `✓ Đã tạo ${totalCreated} sinh viên mới và thêm ${totalEnrolled} sinh viên vào lớp!` +
        (totalFailed > 0 ? ` (${totalFailed} thất bại)` : '')
      )

      setTimeout(() => {
        onSuccess()
      }, 2000)
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
      
      <Alert severity="info" icon={false} sx={{ mt: 2, mb: 2 }}>
        <Typography variant="body2">
          <strong>Lưu ý:</strong> Mật khẩu mặc định cho tất cả sinh viên mới là <code style={{ 
            backgroundColor: '#e3f2fd', 
            padding: '2px 6px', 
            borderRadius: '4px',
            fontWeight: 'bold'
          }}>123456</code>
        </Typography>
      </Alert>

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
