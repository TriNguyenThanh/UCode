export type User = { email: string; token: string } | null

export const auth = {
  async login(email: string, password: string): Promise<NonNullable<User>> {
    await new Promise((r) => setTimeout(r, 400))
    if (email === 'admin@example.com' && password === '123456') {
      const token = 'demo-token-abc123'
      localStorage.setItem('token', token)
      localStorage.setItem('email', email)
      return { token, email }
    }
    const err = new Error('Invalid credentials')
    // @ts-expect-error attach status for convenience
    err.status = 401
    throw err
  },
  logout(): void {
    localStorage.removeItem('token')
    localStorage.removeItem('email')
  },
  getUser(): User {
    const token = localStorage.getItem('token')
    const email = localStorage.getItem('email')
    if (!token) return null
    return { email: email ?? '', token }
  }
}
