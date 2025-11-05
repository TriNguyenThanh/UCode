// ============================================
// USER & AUTHENTICATION
// ============================================

export interface User {
  id: string
  email: string
  name: string
  studentId?: string
  teacherId?: string
  role: 'student' | 'teacher' | 'admin'
}

// ============================================
// CLASS
// ============================================

export interface Class {
  id: string
  name: string
  code: string
  teacherName: string
  semester: string
  description?: string
  coverImage?: string
  studentCount: number
}

// ============================================
// PROBLEM
// ============================================

export type Difficulty = 'EASY' | 'MEDIUM' | 'HARD'
export type Visibility = 'PUBLIC' | 'PRIVATE'
export type ProblemStatus = 'DRAFT' | 'PUBLISHED' | 'ARCHIVED'
export type IoMode = 'STDIO' | 'FILE'

export interface Problem {
  problemId: string
  code: string
  slug: string
  title: string
  difficulty: Difficulty
  ownerId: string
  visibility: Visibility
  status: ProblemStatus
  timeLimitMs: number
  memoryLimitKb: number
  sourceLimitKb: number
  stackLimitKb: number
  ioMode: IoMode
  statement?: string
  solution?: string
  inputFormat?: string
  outputFormat?: string
  constraints?: string
  validatorRef?: string
  changelog?: string
  isLocked: boolean
  createdAt: string
  updatedAt: string
  tagNames: string[]
  problemLanguages: ProblemLanguage[]
  problemAssets: ProblemAsset[]
}

export interface ProblemLanguage {
  problemId: string
  languageId: string
  languageCode?: string
  languageDisplayName?: string
  timeFactor?: number
  memoryKb?: number
  head?: string
  body?: string
  tail?: string
  isAllowed: boolean
}

export type AssetType = 'STATEMENT' | 'SOLUTION' | 'EDITORIAL' | 'TUTORIAL' | 'HINT' | 'TESTCASE'
export type ContentFormat = 'MARKDOWN' | 'HTML' | 'PDF' | 'TEXT'

export interface ProblemAsset {
  problemAssetId: string
  problemId: string
  type: AssetType
  objectRef: string
  checksum?: string
  title?: string
  format: ContentFormat
  orderIndex: number
  isActive: boolean
  createdAt: string
}

// ============================================
// DATASET & TEST CASE
// ============================================

export type DatasetKind = 'SAMPLE' | 'HIDDEN' | 'CUSTOM'

export interface TestCase {
  testCaseId?: string
  datasetId?: string
  input: string
  expectedOutput: string
  orderIndex: number
}

export interface Dataset {
  datasetId?: string
  problemId: string
  name: string
  kind: DatasetKind
  testCases: TestCase[]
}

// ============================================
// ASSIGNMENT
// ============================================

export type AssignmentType = 'HOMEWORK' | 'EXAM' | 'PRACTICE' | 'CONTEST'
export type AssignmentStatus = 'DRAFT' | 'SCHEDULED' | 'ACTIVE' | 'ENDED' | 'GRADED'

export interface Assignment {
  assignmentId: string
  assignmentType: AssignmentType
  status: AssignmentStatus
  classId: string
  title: string
  description?: string
  startTime?: string
  endTime?: string
  assignedBy: string
  createdAt: string
  assignedAt?: string
  totalPoints?: number
  totalProblems?: number
  allowLateSubmission: boolean
  problems?: AssignmentProblemDetail[]
  statistics?: AssignmentStatistics
}

export interface AssignmentProblemDetail {
  problemId: string
  code: string
  title: string
  difficulty: Difficulty
  points: number
  orderIndex: number
}

export interface AssignmentStatistics {
  totalStudents: number
  notStarted: number
  inProgress: number
  submitted: number
  graded: number
  averageScore: number
  completionRate: number
}

// ============================================
// ASSIGNMENT USER (Student's assignment status)
// ============================================

export type AssignmentUserStatus = 'NOT_STARTED' | 'IN_PROGRESS' | 'SUBMITTED' | 'GRADED'

export interface AssignmentUser {
  assignmentUserId?: string
  assignmentId: string
  userId: string
  status: AssignmentUserStatus
  assignedAt: string
  startedAt?: string
  score?: number
  maxScore?: number
}

// ============================================
// SUBMISSION
// ============================================

export type SubmissionStatus = 
  | 'Pending' 
  | 'Judging' 
  | 'Accepted' 
  | 'WrongAnswer' 
  | 'TimeLimitExceeded' 
  | 'MemoryLimitExceeded' 
  | 'RuntimeError' 
  | 'CompilationError' 
  | 'SystemError'

export interface Submission {
  submissionId: string
  problemId: string
  userId: string
  sourceCodeRef: string
  language: string
  status: SubmissionStatus
  compareResult?: string
  errorCode?: string
  errorMessage?: string
  totalTime: number
  totalMemory: number
  resultFileRef?: string
  submittedAt: string
}

export interface BestSubmission {
  submissionId?: string
  assignmentUserId?: string
  problemId: string
  userId?: string
  status: SubmissionStatus
  startedAt?: string
  submittedAt?: string
  score?: number
  maxScore: number
  solutionCode?: string
  teacherFeedback?: string
  attemptCount: number
  totalTestCases?: number
  passedTestCases?: number
  executionTime?: number
  memoryUsed?: number
}

// ============================================
// LANGUAGE
// ============================================

export interface Language {
  languageId: string
  code: string
  displayName: string
  defaultTimeFactor: number
  defaultMemoryKb: number
  defaultHead: string
  defaultBody: string
  defaultTail: string
  isEnabled: boolean
  displayOrder: number
}

// ============================================
// TAG
// ============================================

export type TagCategory = 'ALGORITHM' | 'DATA_STRUCTURE' | 'DIFFICULTY' | 'TOPIC' | 'OTHER'

export interface Tag {
  tagId: string
  name: string
  category: TagCategory
  color?: string
  problemCount?: number
}

// ============================================
// PRACTICE CATEGORY
// ============================================

export interface PracticeCategory {
  id: string
  name: string
  description: string
  problemCount: number
  icon?: string
}

// ============================================
// API RESPONSE WRAPPERS
// ============================================

export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
  timestamp?: string
}

export interface PagedResponse<T> {
  data: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface ErrorResponse {
  error: string
  message: string
  timestamp?: string
  path?: string
  errors?: { [key: string]: string[] }
}
