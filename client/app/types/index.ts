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
// CLASS TYPES
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
  className: string
  classCode: string
  teacherId: string
  semester: string
  description?: string
  coverImage?: string
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

// ============================================
// COMMON TYPES
// ============================================

export interface PagedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalPages: number
  totalCount: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface ApiResponse<T> {
  success: boolean
  message: string
  data?: T
  errors?: string[]
}

// ============================================
// PROBLEM TYPES (Cho Assignment Service - tương lai)
// ============================================

export interface Problem {
  problemId: string
  title: string
  difficulty: 'Easy' | 'Medium' | 'Hard'
  description: string
  category: string
  tags: string[]
  timeLimit: number // seconds
  memoryLimit: number // MB
  testCases: TestCase[]
  sampleInput?: string
  sampleOutput?: string
  createdBy?: string // teacherId
  createdAt?: string
}

export interface TestCase {
  testCaseId: string
  input: string
  expectedOutput: string
  isHidden: boolean
  points?: number
}

export interface CreateProblemRequest {
  title: string
  difficulty: 'Easy' | 'Medium' | 'Hard'
  description: string
  category: string
  tags: string[]
  timeLimit: number
  memoryLimit: number
  testCases: Omit<TestCase, 'testCaseId'>[]
  sampleInput?: string
  sampleOutput?: string
}

// ============================================
// ASSIGNMENT TYPES (Cho Assignment Service - tương lai)
// ============================================

export interface Assignment {
  assignmentId: string
  classId: string
  className: string
  title: string
  description: string
  dueDate: string // ISO date string
  startDate: string
  problems: Problem[]
  totalPoints: number
  status: 'upcoming' | 'active' | 'completed' | 'overdue'
  createdBy?: string // teacherId
  createdAt?: string
}

export interface CreateAssignmentRequest {
  classId: string
  title: string
  description: string
  dueDate: string
  startDate: string
  problemIds: string[]
  totalPoints: number
}

// ============================================
// SUBMISSION TYPES (Cho Grading Service - tương lai)
// ============================================

export interface Submission {
  submissionId: string
  problemId: string
  userId: string
  assignmentId?: string
  code: string
  language: string
  status: 'Pending' | 'Accepted' | 'Wrong Answer' | 'Time Limit Exceeded' | 'Runtime Error' | 'Compilation Error'
  score: number
  executionTime?: number // ms
  memoryUsed?: number // MB
  testResults?: TestResult[]
  submittedAt: string
}

export interface TestResult {
  testCaseId: string
  passed: boolean
  executionTime: number
  memoryUsed: number
  output?: string
  error?: string
}

export interface SubmitCodeRequest {
  problemId: string
  assignmentId?: string
  code: string
  language: string
}

// ============================================
// PRACTICE TYPES
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
export type SubmissionStatus = 'Pending' | 'Accepted' | 'Wrong Answer' | 'Time Limit Exceeded' | 'Runtime Error' | 'Compilation Error'

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
