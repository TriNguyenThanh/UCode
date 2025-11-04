export type User = { email: string; token: string; role: 'student' | 'teacher' | 'admin' } | null

export const auth = {
  async login(email: string, password: string): Promise<NonNullable<User>> {
    await new Promise((r) => setTimeout(r, 400))
    
    // Admin account
    if (email === 'admin@example.com' && password === '123456') {
      const token = 'demo-token-admin-xyz'
      const role = 'admin'
      localStorage.setItem('token', token)
      localStorage.setItem('email', email)
      localStorage.setItem('role', role)
      return { token, email, role }
    }
    
    // Student account
    if (email === 'student@example.com' && password === '123456') {
      const token = 'demo-token-abc123'
      const role = 'student'
      localStorage.setItem('token', token)
      localStorage.setItem('email', email)
      localStorage.setItem('role', role)
      return { token, email, role }
    }
    
    // Teacher account
    if (email === 'teacher@example.com' && password === '123456') {
      const token = 'demo-token-teacher-xyz'
      const role = 'teacher'
      localStorage.setItem('token', token)
      localStorage.setItem('email', email)
      localStorage.setItem('role', role)
      return { token, email, role }
    }
    
    const err = new Error('Invalid credentials')
    // @ts-expect-error attach status for convenience
    err.status = 401
    throw err
  },
  logout(): void {
    localStorage.removeItem('token')
    localStorage.removeItem('email')
    localStorage.removeItem('role')
  },
  getUser(): User {
    const token = localStorage.getItem('token')
    const email = localStorage.getItem('email')
    const role = localStorage.getItem('role') as 'student' | 'teacher' | 'admin' | null
    if (!token) return null
    return { email: email ?? '', token, role: role ?? 'student' }
  }
}
