import * as React from 'react'
import { useLoaderData, redirect } from 'react-router'
import type { Route } from './+types/student.assignment.$assignmentId.problem.$problemId'
import { auth } from '~/auth'
import { ProblemSolver } from '~/components/ProblemSolver'
import { getProblemForStudent } from '~/services/problemService'
import { getSubmissionsByProblem } from '~/services/submissionService'
import { getAssignment } from '~/services/assignmentService'
import type { Submission, Assignment } from '~/types'
import { useExamMonitoring } from '~/utils/useExamMonitoring'

export const meta: Route.MetaFunction = () => [
  { title: 'Làm bài tập | UCode' },
  { name: 'description', content: 'Coding interface để giải bài tập.' }
]

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  
  // Only students can access this page
  if (user.role !== 'student') {
    throw redirect('/home')
  }

  if (!params.assignmentId || !params.problemId) {
    throw new Response('Assignment ID and Problem ID are required', { status: 400 })
  }

  try {
    // Fetch problem for student
    const problem = await getProblemForStudent(params.problemId)
    
    // Fetch assignment to check if it's an examination
    const assignment = await getAssignment(params.assignmentId)
    
    // Fetch user's submission history for this problem
    let submissions: Submission[] = []
    try {
      submissions = await getSubmissionsByProblem(params.problemId, 1, 10)
    } catch (error) {
      console.error('Failed to fetch submissions:', error)
      // Continue even if submissions fail
    }

    return { problem, submissions, assignmentId: params.assignmentId, assignment }
  } catch (error: any) {
    console.error('Failed to load problem:', error)
    throw new Response(error.message || 'Problem not found', { status: 404 })
  }
}

export default function StudentProblemSolver() {
  const { problem, submissions, assignmentId, assignment } = useLoaderData<typeof clientLoader>()
  
  // Exam monitoring for EXAMINATION type assignments
  const isExamination = assignment.assignmentType === 'EXAMINATION'
  
  const { startMonitoring } = useExamMonitoring({
    assignmentId,
    isExamination,
    enabled: true // Always enabled for students in problem solver
  })

  // Start monitoring when component mounts
  React.useEffect(() => {
    if (isExamination) {
      startMonitoring()
    }
  }, [isExamination, startMonitoring])
  
  return (
    <ProblemSolver 
      problem={problem} 
      initialSubmissions={submissions}
      backUrl={`/assignment/${assignmentId}`}
      assignmentId={assignmentId}
    />
  )
}
