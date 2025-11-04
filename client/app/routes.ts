import { type RouteConfig, index, route } from '@react-router/dev/routes'

export default [
  index('routes/_index.tsx'),
  route('home', 'routes/home.tsx'),
  route('login', 'routes/login.tsx'),
  route('logout', 'routes/logout.ts'),
  route('class/:id', 'routes/class.$id.tsx'),
  route('assignment/:id', 'routes/assignment.$id.tsx'),
  route('problem/:id', 'routes/problem.$id.tsx'),
  route('practice', 'routes/practice.tsx'),
  route('practice/:categoryId', 'routes/practice.$categoryId.tsx'),
  route('profile', 'routes/profile.tsx'),
  route('settings', 'routes/settings.tsx'),
  // Teacher routes
  route('teacher/home', 'routes/teacher.home.tsx'),
  route('teacher/class/create', 'routes/teacher.class.create.tsx'),
  route('teacher/class/:id', 'routes/teacher.class.$id.tsx'),
  route('teacher/class/:classId/students', 'routes/teacher.class.$classId.students.tsx'),
  route('teacher/class/:classId/create-assignment', 'routes/teacher.class.$classId.create-assignment.tsx'),
  route('teacher/assignment/:id', 'routes/teacher.assignment.$id.tsx'),
  route('teacher/assignment/:id/edit', 'routes/teacher.assignment.$id.edit.tsx'),
  route('teacher/grading/:assignmentId', 'routes/teacher.grading.$assignmentId.tsx'),
  route('teacher/problem/create', 'routes/teacher.problem.create.tsx'),
  route('teacher/problem/:id/edit', 'routes/teacher.problem.$id.edit.tsx'),
  // Admin routes
  route('admin/home', 'routes/admin.home.tsx'),
  route('admin/users', 'routes/admin.users.tsx'),
  route('admin/classes', 'routes/admin.classes.tsx'),
  route('admin/settings', 'routes/admin.settings.tsx'),
  route('admin/logs', 'routes/admin.logs.tsx'),
] satisfies RouteConfig
