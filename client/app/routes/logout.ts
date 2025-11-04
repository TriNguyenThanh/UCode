import { redirect } from 'react-router'
import type { Route } from './+types/logout'
import { auth } from '~/auth'

export async function clientAction({}: Route.ClientActionArgs) {
  auth.logout()
  return redirect('/login')
}
;({}: Route.ActionArgs) => {
  auth.logout()
  return redirect('/login')
}
