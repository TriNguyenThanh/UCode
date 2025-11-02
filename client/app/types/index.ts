export interface User {
  id: string
  email: string
  name: string
  studentId?: string
  teacherId?: string
  role: 'student' | 'teacher' | 'admin'
}

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

export interface Problem {
  id: string
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
}

export interface TestCase {
  id: string
  input: string
  expectedOutput: string
  isHidden: boolean
}

export interface Assignment {
  id: string
  classId: string
  className: string
  title: string
  description: string
  dueDate: Date
  startDate: Date
  problems: string[] // Array of problem IDs
  totalPoints: number
  status: 'upcoming' | 'active' | 'completed' | 'overdue'
}

export interface Submission {
  id: string
  problemId: string
  userId: string
  code: string
  language: string
  status: 'Accepted' | 'Wrong Answer' | 'Time Limit Exceeded' | 'Runtime Error' | 'Compilation Error'
  score: number
  submittedAt: Date
}

export interface PracticeCategory {
  id: string
  name: string
  description: string
  problemCount: number
  icon?: string
}
