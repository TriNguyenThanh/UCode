import { redirect } from 'react-router'
import type { Route } from './+types/_index'
import { auth } from '~/auth'

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (user) {
    // Redirect based on role
    if (user.role === 'teacher') {
      throw redirect('/teacher/home')
    }
    throw redirect('/home')
  }
  throw redirect('/login')
}

export default function Index() {
  return null
}
