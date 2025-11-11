import { useLoaderData, redirect } from 'react-router'
import type { Route } from './+types/student.assignment.$assignmentId.problem.$problemId'
import { auth } from '~/auth'
import { ProblemSolver } from '~/components/ProblemSolver'
import { getProblemForStudent } from '~/services/problemService'
import { getSubmissionsByProblem } from '~/services/submissionService'
import type { Submission } from '~/types'

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
    
    // Fetch user's submission history for this problem
    let submissions: Submission[] = []
    try {
      submissions = await getSubmissionsByProblem(params.problemId, 1, 10)
    } catch (error) {
      console.error('Failed to fetch submissions:', error)
      // Continue even if submissions fail
    }

    return { problem, submissions, assignmentId: params.assignmentId }
  } catch (error: any) {
    console.error('Failed to load problem:', error)
    throw new Response(error.message || 'Problem not found', { status: 404 })
  }
}

export default function StudentProblemSolver() {
  const { problem, submissions, assignmentId } = useLoaderData<typeof clientLoader>()
  
  return (
    <ProblemSolver 
      problem={problem} 
      initialSubmissions={submissions}
      backUrl={`/assignment/${assignmentId}`}
      assignmentId={assignmentId}
    />
  )
}
