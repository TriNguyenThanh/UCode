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
  major,
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
      // Column headers (row 1)
      ['Mã sinh viên', 'Họ và tên đệm', 'Tên', 'Email'],
      // Sample data
      ['6451071001', 'Nguyễn Văn', 'A', '6451071001@st.utc2.edu.vn'],
      ['6451071002', 'Trần Thị', 'B', ''],
      ['6451071003', 'Lê Văn', 'C', ''],
    ]

    const ws = XLSX.utils.aoa_to_sheet(template)
    
    // Set column widths
    ws['!cols'] = [
      { wch: 15 }, // Mã sinh viên
      { wch: 20 }, // Họ và tên đệm
      { wch: 10 }, // Tên
      { wch: 30 }, // Email
    ]
    
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
      
      // Read all data to find header row
      const allData = XLSX.utils.sheet_to_json(worksheet, { 
        header: 1, // Return as array of arrays
        defval: ''
      }) as any[][]

      // Find header row by looking for "Mã sinh viên" or similar keywords
      let headerRowIndex = -1
      const headerKeywords = ['mã sinh viên', 'mssv', 'studentcode', 'ma sinh vien', 'mã sv']
      
      for (let i = 0; i < Math.min(allData.length, 20); i++) { // Check first 20 rows
        const row = allData[i]
        if (!row) continue
        
        const rowText = row.map(cell => String(cell || '').toLowerCase()).join(' ')
        if (headerKeywords.some(keyword => rowText.includes(keyword))) {
          headerRowIndex = i
          break
        }
      }

      if (headerRowIndex === -1) {
        setError('Không tìm thấy dòng tiêu đề (cột "Mã sinh viên"). Vui lòng kiểm tra lại file Excel.')
        setLoading(false)
        return
      }

      // Detect column indices from header row
      const headerRow = allData[headerRowIndex].map(cell => String(cell || '').toLowerCase().trim())
      
      // Find column indices - use exact match first, then partial match
      const findColumnIndex = (exactKeywords: string[], partialKeywords: string[] = []) => {
        // First try exact match
        let idx = headerRow.findIndex(cell => exactKeywords.some(kw => cell === kw))
        if (idx !== -1) return idx
        
        // Then try partial match
        if (partialKeywords.length > 0) {
          idx = headerRow.findIndex(cell => partialKeywords.some(kw => cell.includes(kw)))
        }
        return idx
      }

      const studentCodeCol = findColumnIndex(
        ['mã sinh viên', 'mssv', 'studentcode', 'ma sinh vien', 'mã sv'],
        ['mã sinh viên', 'mssv']
      )
      const lastNameCol = findColumnIndex(
        ['họ và tên đệm', 'họ tên đệm', 'họ đệm', 'ho ten dem', 'họ và tên', 'họ tên', 'fullname', 'ho va ten'],
        ['họ và tên đệm', 'họ tên đệm', 'ho ten dem']
      )
      // For firstName, use exact match only to avoid matching "họ và tên đệm"
      const firstNameCol = findColumnIndex(
        ['tên', 'ten', 'firstname', 'first name'],
        [] // No partial match to avoid matching "họ và tên đệm"
      )
      const emailCol = findColumnIndex(
        ['email', 'e-mail', 'mail'],
        ['email', 'mail']
      )

      if (studentCodeCol === -1) {
        setError('Không tìm thấy cột "Mã sinh viên" trong file Excel.')
        setLoading(false)
        return
      }

      // Parse data rows (starting after header)
      const parsed: ParsedStudent[] = []
      
      for (let i = headerRowIndex + 1; i < allData.length; i++) {
        const row = allData[i]
        if (!row) continue
        
        const studentCode = String(row[studentCodeCol] || '').trim()
        
        // Skip empty rows
        if (!studentCode) {
          continue
        }

        // Build full name
        let fullName = ''
        if (lastNameCol !== -1 && firstNameCol !== -1 && firstNameCol !== lastNameCol) {
          // Separate lastName and firstName columns
          const lastName = String(row[lastNameCol] || '').trim()
          const firstName = String(row[firstNameCol] || '').trim()
          fullName = `${lastName} ${firstName}`.trim()
        } else if (lastNameCol !== -1) {
          // Single fullName column (or only lastName column found)
          fullName = String(row[lastNameCol] || '').trim()
        }
        
        // Skip if no name at all
        if (!fullName) {
          continue
        }

        // Auto-generate email if empty: mssv@st.utc2.edu.vn
        let email = emailCol !== -1 ? String(row[emailCol] || '').trim() : ''
        if (!email) {
          email = `${studentCode}@st.utc2.edu.vn`
        }

        parsed.push({
          rowNumber: i + 1, // Excel rows are 1-indexed
          studentCode: studentCode,
          fullName: fullName,
          email: email,
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

      // 1. Bulk create new students
      if (newStudents.length > 0) {
        const studentsToCreate = newStudents.map((student) => ({
          studentCode: student.studentCode,
          username: student.studentCode,
          email: student.email,
          password: '123456', // Default password
          fullName: student.fullName,
          major: 'CNTT',
          enrollmentYear: 2025,
          classYear: 1,
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
                  <TableCell>STT</TableCell>
                  <TableCell>MSSV</TableCell>
                  <TableCell>Họ tên</TableCell>
                  <TableCell>Email</TableCell>
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
                    <TableCell>{idx + 1}</TableCell>
                    <TableCell>{result.studentCode}</TableCell>
                    <TableCell>{result.fullName}</TableCell>
                    <TableCell>{result.email}</TableCell>
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
