# Assignment Service API - Client Services

This directory contains TypeScript services for interacting with the Assignment Service API. All services handle errors automatically and unwrap `ApiResponse<T>` to return clean data.

## 📁 Service Files

- **`utils.ts`** - Helper functions for error handling and API response processing
- **`assignmentService.ts`** - Assignment management (CRUD, student assignments, grading)
- **`problemService.ts`** - Problem management (CRUD, assets, languages, tags)
- **`submissionService.ts`** - Code submission and execution
- **`datasetService.ts`** - Test case management
- **`languageService.ts`** - Programming language configuration
- **`tagService.ts`** - Tag management for categorizing problems
- **`index.ts`** - Central export point for all services

## 🔧 Utils (`utils.ts`)

### Error Handling

```typescript
import { handleApiError, unwrapApiResponse } from './utils'

// Automatically handles errors and throws descriptive messages
try {
  const response = await API.get<ApiResponse<Problem>>('/api/v1/problems/123')
  const problem = unwrapApiResponse(response.data) // Returns Problem or throws
} catch (error) {
  handleApiError(error) // Throws formatted error message
}
```

### Helper Functions

- **`handleApiError(error: any): never`** - Processes axios errors and throws descriptive messages
- **`unwrapApiResponse<T>(response: ApiResponse<T>): T`** - Unwraps API response or throws error
- **`buildQueryString(params: Record<string, any>): string`** - Builds query strings from objects

## 📝 Assignment Service (`assignmentService.ts`)

### Teacher Endpoints

```typescript
import {
  createAssignment,
  updateAssignment,
  deleteAssignment,
  getAssignment,
  getMyAssignments,
  getAssignmentsByClass,
  getAssignmentStudents,
  getAssignmentStatistics,
  gradeSubmission,
} from '@/services'

// Create assignment
const assignment = await createAssignment({
  assignmentType: 'HOMEWORK',
  classId: 'class-id',
  title: 'Week 1 Assignment',
  description: 'Complete problems 1-3',
  startTime: '2024-01-01T00:00:00Z',
  endTime: '2024-01-08T23:59:59Z',
  allowLateSubmission: false,
  status: 'DRAFT',
  problems: [
    { problemId: 'problem-1', points: 100, orderIndex: 0 },
    { problemId: 'problem-2', points: 150, orderIndex: 1 },
  ],
})

// Update assignment
await updateAssignment('assignment-id', { title: 'Updated Title' })

// Get assignment with basic problem info
const assignment = await getAssignment('assignment-id')

// Get all my assignments
const myAssignments = await getMyAssignments()

// Get assignments for a class
const classAssignments = await getAssignmentsByClass('class-id')

// Get students and their status
const students = await getAssignmentStudents('assignment-id')

// Get assignment statistics
const stats = await getAssignmentStatistics('assignment-id')

// Grade submission
await gradeSubmission('assignment-id', 'submission-id', {
  score: 95,
  teacherFeedback: 'Great work!',
})
```

### Student Endpoints

```typescript
import {
  getStudentAssignments,
  getMyAssignmentDetail,
  startAssignment,
  updateMyAssignmentStatus,
} from '@/services'

// Get all assignments assigned to me
const assignments = await getStudentAssignments()

// Get my assignment detail
const detail = await getMyAssignmentDetail('assignment-id')

// Start assignment
const started = await startAssignment('assignment-id')

// Update status
await updateMyAssignmentStatus('assignment-id', {
  status: 'SUBMITTED',
})
```

## 🎯 Problem Service (`problemService.ts`)

### Problem CRUD

```typescript
import {
  createProblem,
  getProblem,
  getMyProblems,
  updateProblem,
  deleteProblem,
  searchProblems,
  getPublicProblems,
  getProblemForStudent,
  getProblemsByTagName,
} from '@/services'

// Create problem (Teacher)
const problem = await createProblem({
  code: 'P001',
  title: 'Two Sum',
  difficulty: 'EASY',
  visibility: 'PUBLIC',
})

// Get problem (Teacher)
const problem = await getProblem('problem-id')

// Get problem (Student - public only)
const problem = await getProblemForStudent('problem-id')

// Get my problems with pagination
const pagedProblems = await getMyProblems(1, 20)

// Update problem
await updateProblem({
  problemId: 'problem-id',
  title: 'Updated Title',
  difficulty: 'MEDIUM',
  timeLimitMs: 2000,
  memoryLimitKb: 262144,
  inputFormat: 'First line contains N...',
  outputFormat: 'Print the result...',
  constraints: '1 <= N <= 10^5',
  problemAssets: [
    {
      type: 'STATEMENT',
      objectRef: 's3://bucket/problem-1/statement.md',
      format: 'MARKDOWN',
      orderIndex: 0,
    },
  ],
})

// Delete problem
await deleteProblem('problem-id')

// Search problems
const results = await searchProblems({
  keyword: 'array',
  difficulty: 'EASY',
  page: 1,
  pageSize: 20,
})

// Get public problems
const publicProblems = await getPublicProblems()

// Get problems by tag name
const problems = await getProblemsByTagName('dynamic-programming')
```

### Problem Assets

```typescript
import {
  getProblemAssets,
  addProblemAsset,
  updateProblemAsset,
  deleteProblemAsset,
} from '@/services'

// Get all assets
const assets = await getProblemAssets('problem-id')

// Add asset
await addProblemAsset('problem-id', {
  type: 'SOLUTION',
  objectRef: 's3://bucket/problem-1/solution.md',
  title: 'Official Solution',
  format: 'MARKDOWN',
  orderIndex: 1,
  isActive: true,
})

// Update asset
await updateProblemAsset('problem-id', 'asset-id', {
  title: 'Updated Solution',
  isActive: false,
})

// Delete asset
await deleteProblemAsset('problem-id', 'asset-id')
```

### Problem Languages

```typescript
import {
  getAvailableLanguagesForProblem,
  addOrUpdateProblemLanguages,
  deleteProblemLanguage,
} from '@/services'

// Get available languages
const languages = await getAvailableLanguagesForProblem('problem-id')

// Add/update language configurations (batch)
await addOrUpdateProblemLanguages('problem-id', [
  {
    problemId: 'problem-id',
    languageId: 'cpp-id',
    timeFactor: 1.0,
    memoryKb: 262144,
    isAllowed: true,
    head: '#include <iostream>\nusing namespace std;',
    body: 'int main() {\n    // Your code here\n}',
    tail: '',
  },
])

// Delete language override
await deleteProblemLanguage('problem-id', 'language-id')
```

### Problem Tags

```typescript
import { addTagsToProblem, removeTagFromProblem } from '@/services'

// Add tags
await addTagsToProblem('problem-id', ['tag-id-1', 'tag-id-2'])

// Remove tag
await removeTagFromProblem('problem-id', 'tag-id')
```

## 🚀 Submission Service (`submissionService.ts`)

```typescript
import {
  submitCode,
  runCode,
  getSubmission,
  getUserSubmissions,
  getSubmissionsByProblem,
  getBestSubmissions,
  getUserSubmissionCount,
  getSubmissionCountPerProblem,
} from '@/services'

// Submit code
const submission = await submitCode({
  problemId: 'problem-id',
  languageId: 'cpp-id',
  sourceCode: '#include <iostream>\n...',
})

// Run code (test without submitting)
const result = await runCode({
  problemId: 'problem-id',
  languageId: 'python-id',
  sourceCode: 'def solve():\n    ...',
})

// Get submission by ID
const submission = await getSubmission('submission-id')

// Get user submissions (paginated)
const submissions = await getUserSubmissions(1, 10)

// Get submissions for a problem
const problemSubmissions = await getSubmissionsByProblem('problem-id', 1, 10)

// Get best submissions (leaderboard)
const bestSubmissions = await getBestSubmissions('assignment-user-id', 'problem-id', 1, 10)

// Get submission counts
const totalCount = await getUserSubmissionCount()
const problemCount = await getSubmissionCountPerProblem('assignment-id', 'problem-id')
```

## 🧪 Dataset Service (`datasetService.ts`)

```typescript
import {
  createDataset,
  updateDataset,
  deleteDataset,
  getDataset,
  getDatasetsByProblem,
} from '@/services'

// Create dataset
const dataset = await createDataset({
  problemId: 'problem-id',
  name: 'Sample Test Cases',
  kind: 'SAMPLE',
  testCases: [
    {
      input: '5 10',
      expectedOutput: '15',
      orderIndex: 0,
    },
  ],
})

// Update dataset
await updateDataset({
  datasetId: 'dataset-id',
  name: 'Updated Name',
  testCases: [
    {
      testCaseId: 'testcase-id',
      input: '5 10',
      expectedOutput: '15',
      orderIndex: 0,
    },
  ],
})

// Get dataset
const dataset = await getDataset('dataset-id')

// Get datasets for problem
const datasets = await getDatasetsByProblem('problem-id')

// Delete dataset
await deleteDataset('dataset-id')
```

## 💻 Language Service (`languageService.ts`)

```typescript
import {
  getAllLanguages,
  getLanguage,
  getLanguageByCode,
  createLanguage,
  updateLanguage,
  deleteLanguage,
  enableLanguage,
} from '@/services'

// Get all languages
const languages = await getAllLanguages(false) // excludeDisabled

// Get language by ID
const language = await getLanguage('language-id')

// Get language by code
const cpp = await getLanguageByCode('cpp')

// Create language (Admin only)
await createLanguage({
  code: 'rust',
  displayName: 'Rust',
  defaultTimeFactor: 1.0,
  defaultMemoryKb: 262144,
  defaultHead: '',
  defaultBody: 'fn main() {\n    // Your code\n}',
  defaultTail: '',
  isEnabled: true,
  displayOrder: 10,
})

// Update language (Admin only)
await updateLanguage('language-id', {
  displayName: 'C++ (Updated)',
  defaultTimeFactor: 1.2,
})

// Delete (disable) language (Admin only)
await deleteLanguage('language-id')

// Enable language (Admin only)
await enableLanguage('language-id')
```

## 🏷️ Tag Service (`tagService.ts`)

```typescript
import { getAllTags, getTag, createTag, updateTag, deleteTag, getProblemsByTag } from '@/services'

// Get all tags
const allTags = await getAllTags()

// Get tags by category
const algoTags = await getAllTags('ALGORITHM')

// Get tag by ID
const tag = await getTag('tag-id')

// Create tag (Teacher only)
await createTag({
  name: 'Dynamic Programming',
  category: 'ALGORITHM',
  color: '#3B82F6',
})

// Update tag (Teacher only)
await updateTag('tag-id', {
  name: 'DP',
  color: '#10B981',
})

// Delete tag (Admin only)
await deleteTag('tag-id')

// Get problems by tag
const problems = await getProblemsByTag('tag-id')
```

## ⚠️ Error Handling

All services automatically handle errors and throw descriptive messages:

```typescript
try {
  const problem = await getProblem('invalid-id')
} catch (error) {
  // Error messages are already formatted:
  // - "Problem not found"
  // - "You do not have permission to access this problem"
  // - "Validation failed: title: Title is required"
  console.error(error.message)
}
```

## 🔐 Authentication

All services automatically include the authentication token from `localStorage`:

```typescript
// Set in api.ts
API.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})
```

## 📊 Response Types

All services return unwrapped data. The `ApiResponse<T>` wrapper is handled internally:

```typescript
// Backend returns: { success: true, data: Problem, message: "..." }
// Service returns: Problem (unwrapped)

const problem: Problem = await getProblem('problem-id')
// Not: ApiResponse<Problem>
```

## 🎯 Usage Examples

### Creating a Complete Assignment Flow

```typescript
// 1. Create problems
const problem1 = await createProblem({
  title: 'Two Sum',
  difficulty: 'EASY',
  visibility: 'PUBLIC',
})

const problem2 = await createProblem({
  title: 'Three Sum',
  difficulty: 'MEDIUM',
  visibility: 'PUBLIC',
})

// 2. Add test cases
await createDataset({
  problemId: problem1.problemId,
  name: 'Sample Cases',
  kind: 'SAMPLE',
  testCases: [{ input: '1 2', expectedOutput: '3', orderIndex: 0 }],
})

// 3. Create assignment
const assignment = await createAssignment({
  assignmentType: 'HOMEWORK',
  classId: 'class-id',
  title: 'Week 1 Homework',
  problems: [
    { problemId: problem1.problemId, points: 100, orderIndex: 0 },
    { problemId: problem2.problemId, points: 150, orderIndex: 1 },
  ],
})
```

### Student Submission Flow

```typescript
// 1. Get assignment
const assignments = await getStudentAssignments()

// 2. Start assignment
await startAssignment(assignments[0].assignmentId)

// 3. Get problem details
const problem = await getProblemForStudent(assignments[0].problems![0].problemId)

// 4. Submit code
const submission = await submitCode({
  problemId: problem.problemId,
  languageId: 'cpp-id',
  sourceCode: '#include <iostream>...',
})

// 5. Check submission status
const status = await getSubmission(submission.submissionId)
```

## 📦 Export Structure

```typescript
// Import specific functions
import { createProblem, updateProblem } from '@/services'

// Import types
import type { CreateProblemRequest, UpdateProblemRequest } from '@/services'

// All exports available from index.ts
import * as services from '@/services'
```
# User Service API - Client Services
## Examples - Cách sử dụng Services trong React Components
### 🎯 **Authentication Examples**

```typescript
// app/routes/login.tsx
import { useState } from 'react'
import { useNavigate } from 'react-router'
import { AuthService } from '~/services'
import { auth } from '~/auth'

export default function Login() {
  const navigate = useNavigate()
  const [formData, setFormData] = useState({
    emailOrUsername: '',
    password: ''
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const response = await AuthService.login(formData)
      
      // Save to localStorage
      localStorage.setItem('token', response.accessToken)
      localStorage.setItem('refreshToken', response.refreshToken)
      localStorage.setItem('user', JSON.stringify(response.user))

      // Redirect based on role
      if (response.user.role === 'Teacher') {
        navigate('/teacher/home')
      } else if (response.user.role === 'Admin') {
        navigate('/admin/home')
      } else {
        navigate('/home')
      }
    } catch (err: any) {
      setError(err.message || 'Đăng nhập thất bại')
    } finally {
      setLoading(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      {error && <Alert severity="error">{error}</Alert>}
      
      <TextField
        label="Email hoặc tên đăng nhập"
        value={formData.emailOrUsername}
        onChange={(e) => setFormData({ ...formData, emailOrUsername: e.target.value })}
        required
      />

      <TextField
        label="Mật khẩu"
        type="password"
        value={formData.password}
        onChange={(e) => setFormData({ ...formData, password: e.target.value })}
        required
      />

      <Button type="submit" disabled={loading}>
        {loading ? 'Đang đăng nhập...' : 'Đăng nhập'}
      </Button>
    </form>
  )
}
```

---

## 👨‍🎓 **Student Examples**

### **Student Profile Page**

```typescript
// app/routes/profile.tsx
import { useState } from 'react'
import { useLoaderData } from 'react-router'
import type { Route } from './+types/profile'
import { StudentService } from '~/services'

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const profile = await StudentService.getMyProfile()
  return { profile }
}

export default function Profile() {
  const { profile } = useLoaderData<typeof clientLoader>()
  const [formData, setFormData] = useState({
    fullName: profile.fullName,
    email: profile.email,
    phone: profile.phone || '',
    major: profile.major || '',
    classYear: profile.classYear || 1
  })
  const [success, setSuccess] = useState(false)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setSuccess(false)
    setLoading(true)

    try {
      await StudentService.updateMyProfile(formData)
      setSuccess(true)
    } catch (err: any) {
      setError(err.message || 'Cập nhật thất bại')
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box sx={{ p: 3, maxWidth: 800, mx: 'auto' }}>
      <Typography variant="h4">Hồ sơ cá nhân</Typography>

      {success && <Alert severity="success">Cập nhật thành công!</Alert>}
      {error && <Alert severity="error">{error}</Alert>}

      <form onSubmit={handleSubmit}>
        <TextField
          fullWidth
          label="Mã sinh viên"
          value={profile.studentCode}
          disabled
        />

        <TextField
          fullWidth
          label="Họ và tên"
          value={formData.fullName}
          onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
          required
        />

        <TextField
          fullWidth
          label="Email"
          type="email"
          value={formData.email}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          required
        />

        <Button type="submit" disabled={loading}>
          {loading ? 'Đang lưu...' : 'Lưu thay đổi'}
        </Button>
      </form>
    </Box>
  )
}
```

### **Student Home - Enrolled Classes**

```typescript
// app/routes/home.tsx
import { useLoaderData, Link } from 'react-router'
import type { Route } from './+types/home'
import { ClassService } from '~/services'

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const classes = await ClassService.getEnrolledClasses()
  return { classes }
}

export default function Home() {
  const { classes } = useLoaderData<typeof clientLoader>()

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4">Lớp học của bạn</Typography>

      <Grid container spacing={3} sx={{ mt: 2 }}>
        {classes.map((classItem) => (
          <Grid item xs={12} md={6} lg={4} key={classItem.classId}>
            <Card component={Link} to={`/class/${classItem.classId}`}>
              <CardContent>
                <Typography variant="h6">{classItem.className}</Typography>
                <Typography color="text.secondary">
                  {classItem.teacherName}
                </Typography>
                <Chip label={classItem.semester} size="small" />
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {classes.length === 0 && (
        <Typography>Bạn chưa tham gia lớp học nào.</Typography>
      )}
    </Box>
  )
}
```

---

## 👨‍🏫 **Teacher Examples**

### **Teacher Home - My Classes**

```typescript
// app/routes/teacher.home.tsx
import { useLoaderData, Link } from 'react-router'
import type { Route } from './+types/teacher.home'
import { TeacherService } from '~/services'

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const [profile, classes] = await Promise.all([
    TeacherService.getMyProfile(),
    TeacherService.getMyClasses()
  ])
  return { profile, classes }
}

export default function TeacherHome() {
  const { profile, classes } = useLoaderData<typeof clientLoader>()

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4">Xin chào, {profile.fullName}!</Typography>

      <Box sx={{ mt: 3, display: 'flex', justifyContent: 'space-between' }}>
        <Typography variant="h5">Lớp học của tôi</Typography>
        <Button component={Link} to="/teacher/class/create" variant="contained">
          Tạo lớp mới
        </Button>
      </Box>

      <Grid container spacing={3} sx={{ mt: 1 }}>
        {classes.map((classItem) => (
          <Grid item xs={12} md={6} lg={4} key={classItem.classId}>
            <Card component={Link} to={`/teacher/class/${classItem.classId}`}>
              <CardContent>
                <Typography variant="h6">{classItem.className}</Typography>
                <Typography>{classItem.classCode}</Typography>
                <Chip label={classItem.semester} />
                <Typography variant="body2">
                  {classItem.studentCount} sinh viên
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  )
}
```

### **Create Class**

```typescript
// app/routes/teacher.class.create.tsx
import { useState } from 'react'
import { useNavigate } from 'react-router'
import { ClassService } from '~/services'
import { auth } from '~/auth'

export default function CreateClass() {
  const navigate = useNavigate()
  const user = auth.getUser()
  
  const [formData, setFormData] = useState({
    className: '',
    classCode: '',
    teacherId: user?.userId || '',
    semester: '',
    description: ''
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const newClass = await ClassService.createClass(formData)
      navigate(`/teacher/class/${newClass.classId}`)
    } catch (err: any) {
      setError(err.message || 'Tạo lớp thất bại')
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box sx={{ p: 3, maxWidth: 800, mx: 'auto' }}>
      <Typography variant="h4">Tạo lớp học mới</Typography>

      {error && <Alert severity="error">{error}</Alert>}

      <form onSubmit={handleSubmit}>
        <TextField
          fullWidth
          label="Tên lớp học"
          value={formData.className}
          onChange={(e) => setFormData({ ...formData, className: e.target.value })}
          required
        />

        <TextField
          fullWidth
          label="Mã lớp"
          value={formData.classCode}
          onChange={(e) => setFormData({ ...formData, classCode: e.target.value })}
          required
        />

        <TextField
          fullWidth
          label="Học kỳ"
          placeholder="2024.1"
          value={formData.semester}
          onChange={(e) => setFormData({ ...formData, semester: e.target.value })}
          required
        />

        <TextField
          fullWidth
          label="Mô tả"
          multiline
          rows={4}
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
        />

        <Button type="submit" variant="contained" disabled={loading}>
          {loading ? 'Đang tạo...' : 'Tạo lớp học'}
        </Button>
      </form>
    </Box>
  )
}
```

### **Manage Students in Class**

```typescript
// app/routes/teacher.class.$classId.students.tsx
import { useState } from 'react'
import { useLoaderData } from 'react-router'
import type { Route } from './+types/teacher.class.$classId.students'
import { ClassService, StudentService } from '~/services'

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const [classInfo, students] = await Promise.all([
    ClassService.getClassById(params.classId),
    ClassService.getClassStudents(params.classId)
  ])
  return { classInfo, students }
}

export default function ManageStudents() {
  const { classInfo, students: initialStudents } = useLoaderData<typeof clientLoader>()
  const [students, setStudents] = useState(initialStudents)
  const [selectedStudent, setSelectedStudent] = useState('')
  const [loading, setLoading] = useState(false)

  const handleAddStudent = async () => {
    if (!selectedStudent) return
    setLoading(true)

    try {
      await ClassService.addStudentToClass(classInfo.classId, selectedStudent)
      
      // Reload students
      const updatedStudents = await ClassService.getClassStudents(classInfo.classId)
      setStudents(updatedStudents)
      setSelectedStudent('')
      alert('Thêm sinh viên thành công!')
    } catch (err: any) {
      alert(err.message || 'Thêm sinh viên thất bại')
    } finally {
      setLoading(false)
    }
  }

  const handleRemoveStudent = async (studentId: string) => {
    if (!confirm('Bạn có chắc muốn xóa sinh viên này khỏi lớp?')) return
    setLoading(true)

    try {
      await ClassService.removeStudentFromClass(classInfo.classId, studentId)
      
      // Update local state
      setStudents(students.filter(s => s.userId !== studentId))
      alert('Xóa sinh viên thành công!')
    } catch (err: any) {
      alert(err.message || 'Xóa sinh viên thất bại')
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4">
        Quản lý sinh viên - {classInfo.className}
      </Typography>

      {/* Add student section */}
      <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
        <TextField
          select
          label="Chọn sinh viên"
          value={selectedStudent}
          onChange={(e) => setSelectedStudent(e.target.value)}
          sx={{ flex: 1 }}
        >
          {/* Populate from all students */}
        </TextField>
        <Button onClick={handleAddStudent} disabled={loading || !selectedStudent}>
          Thêm
        </Button>
      </Box>

      {/* Students list */}
      <TableContainer sx={{ mt: 3 }}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Mã SV</TableCell>
              <TableCell>Họ tên</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Hành động</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {students.map((student) => (
              <TableRow key={student.userId}>
                <TableCell>{student.studentCode}</TableCell>
                <TableCell>{student.fullName}</TableCell>
                <TableCell>{student.email}</TableCell>
                <TableCell>
                  <IconButton
                    onClick={() => handleRemoveStudent(student.userId)}
                    color="error"
                  >
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  )
}
```

---

## 👤 **Admin Examples**

### **User Management**

```typescript
// app/routes/admin.users.tsx
import { useState } from 'react'
import { useLoaderData } from 'react-router'
import type { Route } from './+types/admin.users'
import { UserService, StudentService, TeacherService } from '~/services'

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const users = await UserService.getAllUsers({
    pageNumber: 1,
    pageSize: 20
  })
  return { users }
}

export default function AdminUsers() {
  const { users: initialUsers } = useLoaderData<typeof clientLoader>()
  const [users, setUsers] = useState(initialUsers)
  const [selectedRole, setSelectedRole] = useState<'Student' | 'Teacher' | 'Admin'>('Student')
  const [createDialogOpen, setCreateDialogOpen] = useState(false)

  const handleCreateUser = async (formData: any) => {
    try {
      if (selectedRole === 'Student') {
        await StudentService.createStudent(formData)
      } else if (selectedRole === 'Teacher') {
        await TeacherService.createTeacher(formData)
      } else {
        await TeacherService.createAdmin(formData)
      }

      // Reload users
      const updated = await UserService.getAllUsers({ pageNumber: 1, pageSize: 20 })
      setUsers(updated)
      setCreateDialogOpen(false)
      alert('Tạo user thành công!')
    } catch (err: any) {
      alert(err.message || 'Tạo user thất bại')
    }
  }

  const handleDeleteUser = async (userId: string) => {
    if (!confirm('Bạn có chắc muốn xóa user này?')) return

    try {
      await UserService.deleteUser(userId)
      setUsers({
        ...users,
        items: users.items.filter(u => u.userId !== userId)
      })
      alert('Xóa user thành công!')
    } catch (err: any) {
      alert(err.message || 'Xóa user thất bại')
    }
  }

  const handleUpdateStatus = async (userId: string, status: 'Active' | 'Banned') => {
    try {
      await UserService.updateUserStatus(userId, status)
      
      // Update local state
      setUsers({
        ...users,
        items: users.items.map(u => 
          u.userId === userId ? { ...u, status } : u
        )
      })
      alert('Cập nhật trạng thái thành công!')
    } catch (err: any) {
      alert(err.message || 'Cập nhật thất bại')
    }
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4">Quản lý Users</Typography>
        <Button variant="contained" onClick={() => setCreateDialogOpen(true)}>
          Tạo User Mới
        </Button>
      </Box>

      {/* Filter tabs */}
      <Tabs value={selectedRole} onChange={(_, v) => setSelectedRole(v)}>
        <Tab label="Sinh viên" value="Student" />
        <Tab label="Giáo viên" value="Teacher" />
        <Tab label="Admin" value="Admin" />
      </Tabs>

      {/* Users table */}
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Họ tên</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Role</TableCell>
              <TableCell>Trạng thái</TableCell>
              <TableCell>Hành động</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {users.items.map((user) => (
              <TableRow key={user.userId}>
                <TableCell>{user.userId.slice(0, 8)}...</TableCell>
                <TableCell>{user.fullName}</TableCell>
                <TableCell>{user.email}</TableCell>
                <TableCell>
                  <Chip label={user.role} size="small" />
                </TableCell>
                <TableCell>
                  <Chip 
                    label={user.status} 
                    color={user.status === 'Active' ? 'success' : 'error'}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  <IconButton onClick={() => handleDeleteUser(user.userId)}>
                    <DeleteIcon />
                  </IconButton>
                  <IconButton 
                    onClick={() => handleUpdateStatus(
                      user.userId, 
                      user.status === 'Active' ? 'Banned' : 'Active'
                    )}
                  >
                    <BlockIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  )
}
```

---

## 🔄 **Token Refresh Example**

```typescript
// app/api.ts - Axios interceptor
import axios from 'axios'
import { AuthService } from '~/services'

API.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    // If 401 and not already retried
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        // Get refresh token
        const refreshToken = localStorage.getItem('refreshToken')
        if (!refreshToken) throw new Error('No refresh token')

        // Call refresh token API
        const response = await AuthService.refreshToken(refreshToken)

        // Update tokens
        localStorage.setItem('token', response.accessToken)
        localStorage.setItem('refreshToken', response.refreshToken)

        // Retry original request
        originalRequest.headers.Authorization = `Bearer ${response.accessToken}`
        return API(originalRequest)
      } catch (refreshError) {
        // Refresh failed, logout
        localStorage.clear()
        window.location.href = '/login'
        return Promise.reject(refreshError)
      }
    }

    return Promise.reject(error)
  }
)
```

---

## 🎯 **Best Practices**

### **1. Loading States**
```typescript
const [loading, setLoading] = useState(false)

const handleAction = async () => {
  setLoading(true)
  try {
    await SomeService.someAction()
  } finally {
    setLoading(false) // Always reset loading
  }
}
```

### **2. Error Handling**
```typescript
try {
  await SomeService.someAction()
} catch (err: any) {
  // Always provide user-friendly message
  setError(err.message || 'Đã xảy ra lỗi')
}
```

### **3. Optimistic Updates**
```typescript
// Update UI immediately
setStudents(students.filter(s => s.id !== studentId))

// Then call API
try {
  await ClassService.removeStudentFromClass(classId, studentId)
} catch (err) {
  // Revert on error
  setStudents(originalStudents)
  alert('Xóa thất bại')
}
```

### **4. Parallel Requests**
```typescript
// Use Promise.all for parallel requests
const [profile, classes, students] = await Promise.all([
  TeacherService.getMyProfile(),
  ClassService.getMyClasses(),
  StudentService.getAllStudents()
])
```

### **5. Type Safety**
```typescript
// Always use TypeScript types
import type { StudentResponse } from '~/types'

const student: StudentResponse = await StudentService.getMyProfile()
```
