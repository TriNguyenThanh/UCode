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
