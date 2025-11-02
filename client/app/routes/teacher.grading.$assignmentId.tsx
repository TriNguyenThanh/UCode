import { useState } from 'react'
import { redirect, useLoaderData, useSearchParams } from 'react-router'
import type { Route } from './+types/teacher.grading.$assignmentId'
import { auth } from '~/auth'
import { mockAssignments, mockProblems, mockClasses } from '~/data/mock'
import type { Problem } from '~/types/index'
import { Navigation } from '~/components/Navigation'
import {
  Box,
  Container,
  Typography,
  Button,
  Paper,
  Chip,
  IconButton,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Divider,
  Card,
  CardContent,
} from '@mui/material'
import NavigateBeforeIcon from '@mui/icons-material/NavigateBefore'
import NavigateNextIcon from '@mui/icons-material/NavigateNext'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'
import SaveIcon from '@mui/icons-material/Save'

// Mock student submission data
const mockSubmission = {
  studentId: 'student-1',
  studentName: 'Nguyễn Văn A',
  mssv: '2021600001',
  submittedAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
  problemResults: [
    {
      problemId: 'problem-1',
      code: `def twoSum(nums, target):
    seen = {}
    for i, num in enumerate(nums):
        complement = target - num
        if complement in seen:
            return [seen[complement], i]
        seen[num] = i
    return []

# Test cases
print(twoSum([2,7,11,15], 9))  # [0,1]
print(twoSum([3,2,4], 6))       # [1,2]
print(twoSum([3,3], 6))         # [0,1]`,
      language: 'python',
      testsPassed: 8,
      testsTotal: 10,
      status: 'partial',
    },
    {
      problemId: 'problem-2',
      code: `def isPalindrome(x):
    if x < 0:
        return False
    return str(x) == str(x)[::-1]

# Test cases
print(isPalindrome(121))   # True
print(isPalindrome(-121))  # False
print(isPalindrome(10))    # False`,
      language: 'python',
      testsPassed: 10,
      testsTotal: 10,
      status: 'accepted',
    },
  ],
}

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  const assignment = mockAssignments.find((a) => a.id === params.assignmentId)
  if (!assignment) {
    throw new Response('Bài tập không tồn tại', { status: 404 })
  }

  const classData = mockClasses.find((c) => c.id === assignment.classId)
  const problems = assignment.problems
    .map((id) => mockProblems.find((p) => p.id === id))
    .filter((p): p is Problem => p !== undefined)

  // Mock list of students with submissions
  const studentsWithSubmissions = Array.from({ length: 15 }, (_, i) => ({
    id: `student-${i + 1}`,
    name: `Sinh viên ${i + 1}`,
    mssv: `2021${(600000 + i).toString().padStart(6, '0')}`,
    submittedAt: new Date(Date.now() - Math.random() * 7 * 24 * 60 * 60 * 1000).toISOString(),
    score: null,
    graded: false,
  }))

  return { user, assignment, classData, problems, studentsWithSubmissions, mockSubmission }
}

export default function TeacherGrading() {
  const { assignment, classData, problems, studentsWithSubmissions, mockSubmission } =
    useLoaderData<typeof clientLoader>()
  const [searchParams] = useSearchParams()
  const [currentStudentIndex, setCurrentStudentIndex] = useState(0)
  const [currentProblemIndex, setCurrentProblemIndex] = useState(0)
  const [score, setScore] = useState<number>(0)
  const [feedback, setFeedback] = useState('')

  const currentStudent = studentsWithSubmissions[currentStudentIndex]
  const currentProblem = problems[currentProblemIndex]
  const currentResult = mockSubmission.problemResults[currentProblemIndex]

  const handlePrevStudent = () => {
    if (currentStudentIndex > 0) {
      setCurrentStudentIndex(currentStudentIndex - 1)
      setScore(0)
      setFeedback('')
    }
  }

  const handleNextStudent = () => {
    if (currentStudentIndex < studentsWithSubmissions.length - 1) {
      setCurrentStudentIndex(currentStudentIndex + 1)
      setScore(0)
      setFeedback('')
    }
  }

  const handleSaveGrade = () => {
    // Save grade logic
    alert(`Đã lưu điểm ${score} cho ${currentStudent.name}`)
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50', pb: 4 }}>
      <Navigation />
      <Container maxWidth="xl" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 3 }}>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          Chấm bài: {classData?.name} / {assignment.title}
        </Typography>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="h5" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
            Chấm bài cho sinh viên ({currentStudentIndex + 1}/{studentsWithSubmissions.length})
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="outlined"
              startIcon={<NavigateBeforeIcon />}
              onClick={handlePrevStudent}
              disabled={currentStudentIndex === 0}
              sx={{ borderColor: 'secondary.main', color: 'secondary.main' }}
            >
              Sinh viên trước
            </Button>
            <Button
              variant="outlined"
              endIcon={<NavigateNextIcon />}
              onClick={handleNextStudent}
              disabled={currentStudentIndex === studentsWithSubmissions.length - 1}
              sx={{ borderColor: 'secondary.main', color: 'secondary.main' }}
            >
              Sinh viên sau
            </Button>
          </Box>
        </Box>
      </Box>

      {/* Student Info */}
      <Paper sx={{ p: 2, mb: 3, bgcolor: 'secondary.main' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box>
            <Typography variant="h6" sx={{ color: 'primary.main', fontWeight: 'bold' }}>
              {mockSubmission.studentName}
            </Typography>
            <Typography variant="body2" sx={{ color: 'primary.main' }}>
              MSSV: {mockSubmission.mssv} • Nộp lúc:{' '}
              {new Date(mockSubmission.submittedAt).toLocaleString('vi-VN')}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 2 }}>
            {mockSubmission.problemResults.map((result, idx) => (
              <Chip
                key={idx}
                label={`Bài ${idx + 1}`}
                onClick={() => setCurrentProblemIndex(idx)}
                color={
                  result.status === 'accepted'
                    ? 'success'
                    : result.status === 'partial'
                    ? 'warning'
                    : 'error'
                }
                sx={{
                  fontWeight: currentProblemIndex === idx ? 'bold' : 'normal',
                  border: currentProblemIndex === idx ? 2 : 0,
                }}
              />
            ))}
          </Box>
        </Box>
      </Paper>

      <Box sx={{ display: 'flex', gap: 3 }}>
        {/* Left: Problem Description */}
        <Box sx={{ flex: '0 0 400px', display: 'flex', flexDirection: 'column' }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 2 }}>
              {currentProblem?.title}
            </Typography>
            <Box sx={{ mb: 2 }}>
              <Chip label={currentProblem?.difficulty} size="small" color="warning" sx={{ mr: 1 }} />
              <Chip
                icon={
                  currentResult?.status === 'accepted' ? <CheckCircleIcon /> : <CancelIcon />
                }
                label={`${currentResult?.testsPassed}/${currentResult?.testsTotal} test cases`}
                size="small"
                color={currentResult?.status === 'accepted' ? 'success' : 'warning'}
              />
            </Box>
            <Divider sx={{ my: 2 }} />
            <Typography variant="body2" color="text.secondary" paragraph>
              {currentProblem?.description}
            </Typography>
            <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mt: 2, mb: 1 }}>
              Ví dụ:
            </Typography>
            <Paper sx={{ p: 2, bgcolor: 'grey.50' }}>
              <Typography variant="body2" component="pre" sx={{ fontFamily: 'monospace', m: 0 }}>
                Input: [2,7,11,15], target = 9{'\n'}
                Output: [0,1]
              </Typography>
            </Paper>
          </Paper>
        </Box>

        {/* Middle: Student Code */}
        <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
          <Paper sx={{ p: 2, mb: 2 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 'bold' }}>
                Code của sinh viên
              </Typography>
              <Chip label={currentResult?.language.toUpperCase()} size="small" />
            </Box>
            <Paper
              sx={{
                p: 2,
                bgcolor: '#1e1e1e',
                color: '#d4d4d4',
                fontFamily: 'monospace',
                fontSize: '14px',
                overflow: 'auto',
                maxHeight: '500px',
              }}
            >
              <pre style={{ margin: 0 }}>{currentResult?.code}</pre>
            </Paper>
          </Paper>

          {/* Test Results */}
          <Card sx={{ bgcolor: currentResult?.status === 'accepted' ? 'success.light' : 'warning.light' }}>
            <CardContent>
              <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mb: 1 }}>
                Kết quả chạy test:
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                {currentResult?.status === 'accepted' ? (
                  <CheckCircleIcon color="success" />
                ) : (
                  <CancelIcon color="warning" />
                )}
                <Typography variant="body2">
                  {currentResult?.testsPassed}/{currentResult?.testsTotal} test cases passed
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Box>

        {/* Right: Grading Panel */}
        <Box sx={{ flex: '0 0 350px', display: 'flex', flexDirection: 'column' }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 3 }}>
              Chấm điểm
            </Typography>

            <FormControl fullWidth sx={{ mb: 3 }}>
              <InputLabel>Điểm cho bài này</InputLabel>
              <Select
                value={score}
                onChange={(e) => setScore(Number(e.target.value))}
                label="Điểm cho bài này"
              >
                {[0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100].map((val) => (
                  <MenuItem key={val} value={val}>
                    {val} điểm
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <TextField
              fullWidth
              label="Nhận xét"
              multiline
              rows={6}
              value={feedback}
              onChange={(e) => setFeedback(e.target.value)}
              placeholder="Viết nhận xét cho sinh viên..."
              sx={{ mb: 3 }}
            />

            <Divider sx={{ my: 2 }} />

            <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 'bold' }}>
              Tổng quan bài làm:
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mb: 3 }}>
              {mockSubmission.problemResults.map((result, idx) => (
                <Box key={idx} sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2">Bài {idx + 1}:</Typography>
                  <Chip
                    size="small"
                    label={`${result.testsPassed}/${result.testsTotal}`}
                    color={result.status === 'accepted' ? 'success' : 'warning'}
                  />
                </Box>
              ))}
            </Box>

            <Button
              fullWidth
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSaveGrade}
              sx={{
                bgcolor: 'secondary.main',
                color: 'primary.main',
                py: 1.5,
                '&:hover': {
                  bgcolor: 'primary.main',
                  color: 'secondary.main',
                },
              }}
            >
              Lưu điểm và nhận xét
            </Button>
          </Paper>
        </Box>
      </Box>
      </Container>
    </Box>
  )
}
