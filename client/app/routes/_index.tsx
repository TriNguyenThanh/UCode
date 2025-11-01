import * as React from 'react'
import { Link } from 'react-router'
import { auth } from '~/auth'
import type { Route } from './+types/_index'

export const meta: Route.MetaFunction = () => [
  { title: 'Home | UCode' },
  { name: 'description', content: 'Overview of your account.' },
];

export default function Index() {
  const user = auth.getUser()
  return (
    <div className='grid gap-2'>
      <h1 className='text-2xl font-semibold'>Home</h1>
      {user ? (
        <p>
          Welcome back, <strong>{user.email}</strong>. Go to your{' '}
          <Link to='/home' className='underline'>
            Home
          </Link>
          .
        </p>
      ) : (
        <p>
          You are not logged in. Please{' '}
          <Link to='/login' className='underline'>
            login
          </Link>{' '}
          to continue.
        </p>
      )}
    </div>
  )
}
