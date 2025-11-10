export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
  timestamp?: string
}

export interface PagedResponse<T> {
  data: T[] // Frontend standard format
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

// Backend actual response format (for students endpoint)
export interface BackendPagedResponse<T> {
  items: T[] // Backend uses 'items' not 'data'
  pageNumber: number // Backend uses 'pageNumber' not 'page'
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface ErrorResponse {
  error: string
  message: string
  timestamp?: string
  path?: string
  errors?: { [key: string]: string[] }
}


// ============================================
// AUTH & USER TYPES
// ============================================

export interface User {
  userId: string // Map từ backend UserId (Guid)
  username: string
  email: string
  fullName: string // Map từ backend FullName
  role: 'Student' | 'Teacher' | 'Admin' // Map từ UserRole enum
  status?: 'Active' | 'Inactive' | 'Banned' // Map từ UserStatus enum
  
  // Student specific
  studentCode?: string // Map từ backend StudentCode
  major?: string
  enrollmentYear?: number
  classYear?: number
  
  // Teacher specific
  teacherCode?: string // Map từ backend TeacherCode
  department?: string
  title?: string
  
  // Common
  phone?: string
  address?: string
  dateOfBirth?: string // ISO date string
  avatarUrl?: string
  createdAt?: string
  updatedAt?: string
}

export interface LoginRequest {
  emailOrUsername: string // Backend expects EmailOrUsername
  password: string
  rememberMe?: boolean
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  user: User
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  email: string
  token: string
  newPassword: string
}

// ============================================
// CLASS
// ============================================

export interface Class {
  classId: string // Backend: ClassId (Guid)
  className: string // Backend: ClassName
  classCode: string // Backend: ClassCode
  teacherId: string
  teacherName: string
  semester: string
  description?: string
  coverImage?: string
  studentCount: number
  createdAt: string
  updatedAt?: string
}

export interface CreateClassRequest {
  name: string // Backend expects "Name"
  classCode?: string // Backend expects "ClassCode" (optional)
  teacherId: string // Backend expects "TeacherId"
  description?: string // Backend expects "Description"
}

export interface UpdateClassRequest {
  className?: string
  description?: string
  coverImage?: string
}

export interface GetClassesRequest {
  pageNumber?: number
  pageSize?: number
  teacherId?: string
  semester?: string
  search?: string
}

export interface ClassWithStudents extends Class {
  students: User[]
}

// ============================================
// STUDENT TYPES
// ============================================

export interface CreateStudentRequest {
  studentCode: string
  username: string
  email: string
  password: string
  fullName: string
  phone?: string
  dateOfBirth?: string
  address?: string
  major?: string
  enrollmentYear?: number
  classYear?: number
}

export interface UpdateStudentRequest {
  fullName?: string
  email?: string
  phone?: string
  address?: string
  major?: string
  classYear?: number
}

export interface StudentResponse extends User {
  studentCode: string
  major?: string
  enrollmentYear?: number
  classYear?: number
}

// ============================================
// TEACHER TYPES
// ============================================

export interface CreateTeacherRequest {
  teacherCode: string
  username: string
  email: string
  password: string
  fullName: string
  phone?: string
  department?: string
  title?: string
}

export interface UpdateTeacherRequest {
  fullName?: string
  email?: string
  phone?: string
  department?: string
  title?: string
}

export interface TeacherResponse extends User {
  teacherCode: string
  department?: string
  title?: string
  classes?: Class[]
}

// ============================================
// ADMIN TYPES
// ============================================

export interface CreateAdminRequest {
  username: string
  email: string
  password: string
  fullName: string
  phone?: string
}

export interface UpdateUserByAdminRequest {
  email?: string
  fullName?: string
  phone?: string
  major?: string
  classYear?: number
  department?: string
  title?: string
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
  datasetSample?: Dataset
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
  createdBy?: string 
}

/// =====================
/// TESTCASE STATUS
/// =====================
export type TestcaseStatus =
        'Passed' |
        'TimeLimitExceeded' |
        'MemoryLimitExceeded' |
        'RuntimeError' |
        'InternalError' |
        'WrongAnswer' |
        'CompilationError' |
        'Skipped'

// ============================================
// DATASET & TEST CASE
// ============================================

export type DatasetKind = 'SAMPLE' | 'PUBLIC' | 'PRIVATE' | 'OFFICIAL'

export interface TestCase {
  testCaseId?: string
  datasetId?: string
  inputRef: string
  outputRef: string
  indexNo: number
  score?: number
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
  // Extended fields (may need to fetch separately from user service)
  user?: {
    userId: string
    fullName: string
    studentCode?: string
    email: string
  }
}

// ============================================
// SUBMISSION
// ============================================

export type SubmissionStatus =
  | 'Pending'
  | 'Running'
  | 'Passed'
  | 'Failed'
  | 'CompilationError'
  | 'RuntimeError'
  | 'TimeLimitExceeded'
  | 'MemoryLimitExceeded'

export interface SubmissionRequest {
  problemId: string
  assignmentUserId?: string
  sourceCode: string
  language: string
}

export interface CreateSubmissionResponse {
  submissionId: string
  status: SubmissionStatus
}

export interface Submission {
  submissionId: string
  userId: string
  problemId: string
  assignmentUserId?: string
  sourceCodeRef: string
  language: string
  status: SubmissionStatus
  compareResult?: string
  errorCode?: string
  errorMessage?: string
  totalTestcase: number
  passedTestcase: number
  totalTime: number
  totalMemory: number
  submittedAt: string
  resultFileRef?: string
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
  categoryId: string
  name: string
  description: string
  problemCount: number
  icon?: string
}

// ============================================
// UTILITY TYPES
// ============================================

export type UserRole = 'Student' | 'Teacher' | 'Admin'
export type UserStatus = 'Active' | 'Inactive' | 'Banned'
export type ProblemDifficulty = 'Easy' | 'Medium' | 'Hard'

// ============================================
// TYPE GUARDS
// ============================================

export function isStudent(user: User): user is User & Required<Pick<User, 'studentCode'>> {
  return user.role === 'Student' && !!user.studentCode
}

export function isTeacher(user: User): user is User & Required<Pick<User, 'teacherCode'>> {
  return user.role === 'Teacher' && !!user.teacherCode
}

export function isAdmin(user: User): boolean {
  return user.role === 'Admin'
}
