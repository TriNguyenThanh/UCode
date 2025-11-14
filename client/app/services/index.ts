// ============================================
// USER SERVICE APIs
// ============================================

// NOTE: Auth operations are handled in ~/auth.ts
// Use: import { auth } from '~/auth'

// Student Service - Student operations
export * as StudentService from './studentService'

// Teacher Service - Teacher operations
export * as TeacherService from './teacherService'

// Class Service - Class management
export * as ClassService from './classService'

// User Service - User management (Admin only)
export * as UserService from './userService'

// ============================================
// OTHER SERVICES (Assignment, Problem, etc.)
// ============================================

// Export all services
export * from './assignmentService'
export * from './classService'
export * from './datasetService'
export * from './languageService'
export * from './submissionService'

// Export problemService with specific exports to avoid conflicts
export {
  createProblem,
  getProblem,
  getProblemForStudent,
  getMyProblems,
  deleteProblem,
  updateProblem,
  getPublicProblems,
  searchProblems,
  getProblemAssets,
  addProblemAsset,
  updateProblemAsset,
  deleteProblemAsset,
  addTagsToProblem,
  removeTagFromProblem,
  getAvailableLanguagesForProblem,
  addOrUpdateProblemLanguages,
  deleteProblemLanguage,
  type CreateProblemRequest,
  type UpdateProblemRequest,
  type CreateProblemAssetRequest,
  type UpdateProblemAssetRequest,
  type ProblemLanguageRequest,
  type SearchProblemsParams,
} from './problemService'

// Rename export to avoid conflict with tagService
export { getProblemsByTag as getProblemsByTagName } from './problemService'

// Export tagService
export * from './tagService'

// Export utils
export * from './utils'

