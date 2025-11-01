import * as React from 'react'
import { useLoaderData, redirect } from 'react-router'
import type { Route } from './+types/home'
import { auth } from '~/auth'

export const meta: Route.MetaFunction = () => [
  { title: 'Home | UCode' },
  { name: 'description', content: 'Overview of your account.' },
];

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  return user
}

export default function Home() {
  const user = useLoaderData<typeof clientLoader>()
  return (
    <div className='grid gap-2'>
      <h1 className='text-2xl font-semibold'>Home</h1>
      <p>
        Authenticated as <strong>{user.email}</strong>.
      </p>
      <p className='text-sm text-gray-600'>
        Protected by a loader that redirects to <code>/login</code> if there is no token.
      </p>
    </div>
  )
}
