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

