import type { Submission, SubmissionRequest, CreateSubmissionResponse } from '~/types'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

/**
 * Submit code for judging (chấm điểm chính thức)
 */
export async function submitCode(request: SubmissionRequest): Promise<CreateSubmissionResponse> {
  const token = localStorage.getItem('token')
  
  const response = await fetch(`${API_BASE_URL}/api/v1/submissions/submit-code`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    const error = await response.json()
    throw new Error(error.message || 'Failed to submit code')
  }

  const result = await response.json()
  return result.data
}

/**
 * Run code for testing (chỉ chạy test với sample test cases)
 */
export async function runCode(request: SubmissionRequest): Promise<CreateSubmissionResponse> {
  const token = localStorage.getItem('token')
  
  const response = await fetch(`${API_BASE_URL}/api/v1/submissions/run-code`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    const error = await response.json()
    throw new Error(error.message || 'Failed to run code')
  }

  const result = await response.json()
  return result.data
}

/**
 * Get submission result by ID (polling để lấy kết quả)
 */
export async function getSubmission(submissionId: string): Promise<Submission> {
  const token = localStorage.getItem('token')
  
  const response = await fetch(`${API_BASE_URL}/api/v1/submissions/${submissionId}`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
    },
  })

  if (!response.ok) {
    const error = await response.json()
    throw new Error(error.message || 'Failed to get submission')
  }

  const result = await response.json()
  return result.data
}

/**
 * Poll submission result until it's completed
 * @param submissionId Submission ID to poll
 * @param maxAttempts Maximum number of polling attempts (default: 60)
 * @param interval Polling interval in milliseconds (default: 1000)
 */
export async function pollSubmissionResult(
  submissionId: string,
  maxAttempts: number = 60,
  interval: number = 1000
): Promise<Submission> {
  let attempts = 0

  return new Promise((resolve, reject) => {
    const poll = async () => {
      try {
        attempts++
        const submission = await getSubmission(submissionId)

        // Check if submission is completed
        if (
          submission.status === 'Passed' ||
          submission.status === 'Failed' ||
          submission.status === 'CompilationError' ||
          submission.status === 'RuntimeError' ||
          submission.status === 'TimeLimitExceeded' ||
          submission.status === 'MemoryLimitExceeded'
        ) {
          resolve(submission)
          return
        }

        // Continue polling if not completed
        if (attempts >= maxAttempts) {
          reject(new Error('Polling timeout: Submission is taking too long'))
          return
        }

        setTimeout(poll, interval)
      } catch (error) {
        reject(error)
      }
    }

    poll()
  })
}

/**
 * Get all user submissions with pagination
 */
export async function getUserSubmissions(
  pageNumber: number = 1,
  pageSize: number = 10
): Promise<Submission[]> {
  const token = localStorage.getItem('token')
  
  const response = await fetch(
    `${API_BASE_URL}/api/v1/submissions/user?pageNumber=${pageNumber}&pageSize=${pageSize}`,
    {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    }
  )

  if (!response.ok) {
    const error = await response.json()
    throw new Error(error.message || 'Failed to get user submissions')
  }

  const result = await response.json()
  return result.data
}

/**
 * Get all submissions for a specific problem
 */
export async function getProblemSubmissions(
  problemId: string,
  pageNumber: number = 1,
  pageSize: number = 10
): Promise<Submission[]> {
  const token = localStorage.getItem('token')
  
  const response = await fetch(
    `${API_BASE_URL}/api/v1/submissions/problem/${problemId}?pageNumber=${pageNumber}&pageSize=${pageSize}`,
    {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    }
  )

  if (!response.ok) {
    const error = await response.json()
    throw new Error(error.message || 'Failed to get problem submissions')
  }

  const result = await response.json()
  return result.data
}