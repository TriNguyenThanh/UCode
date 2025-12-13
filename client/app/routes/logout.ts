import { redirect } from 'react-router'
import type { Route } from './+types/logout'
import { auth } from '~/auth'

export async function clientLoader({}: Route.ClientLoaderArgs) {
  await auth.logout()
  throw redirect('/login')
}

export async function clientAction({}: Route.ClientActionArgs) {
  await auth.logout()
  throw redirect('/login')
}
