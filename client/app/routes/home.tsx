import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/home'
import { auth } from '~/auth'
import { API } from '~/api'
import { Navigation } from '~/components/Navigation'
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  CardActionArea,
  Chip,
  Paper,
  Divider,
  IconButton,
  Stack,
} from '@mui/material'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import AssignmentIcon from '@mui/icons-material/Assignment'
import CodeIcon from '@mui/icons-material/Code'
import ClassIcon from '@mui/icons-material/Class'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import { mockPracticeCategories } from '~/data/mock'
import type { ApiResponse, PagedResponse, Class, Assignment } from '~/types'
import { getStudentAssignments, getMyAssignments } from '~/services/assignmentService'

export const meta: Route.MetaFunction = () => [
  { title: 'Trang chủ | UCode' },
  { name: 'description', content: 'Trang chủ học lập trình của sinh viên UTC2.' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')
  
  try {
    // Lấy classes từ API
    const classesResponse = await API.get<ApiResponse<PagedResponse<Class>>>('/api/v1/classes')
    const classesData = classesResponse.data.data?.items || []
    const classes = classesData.map((cls: Class) => ({
      id: cls.classId,
      name: cls.className,
      code: cls.classCode,
      teacherName: cls.teacherName,
      semester: cls.semester,
      description: cls.description,
      studentCount: cls.studentCount,
    }))
    
    // Lấy assignments từ API dựa vào role
    let allAssignments: Assignment[] = []
    try {
      if (user.role === 'student') {
        allAssignments = await getStudentAssignments()
      } else if (user.role === 'teacher') {
        allAssignments = await getMyAssignments()
      }
    } catch (error) {
      console.error('Error loading assignments:', error)
      allAssignments = []
    }
    
    // Filter assignments due within 7 days
    const now = new Date()
    const sevenDaysLater = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000)
    const upcomingAssignments = allAssignments.filter(
      (assignment: Assignment) => {
        if (!assignment.endTime) return false
        const dueDate = new Date(assignment.endTime)
        return dueDate <= sevenDaysLater && dueDate > now
      }
    )
    
    return {
      user,
      classes,
      upcomingAssignments,
      practiceCategories: mockPracticeCategories,
    }
  } catch (error) {
    console.error('Error loading home data:', error)
    // Fallback to empty data nếu API fail
    return {
      user,
      classes: [],
      upcomingAssignments: [],
      practiceCategories: mockPracticeCategories,
    }
  }
}

export default function Home() {
  const { user, classes, upcomingAssignments, practiceCategories } = useLoaderData<typeof clientLoader>()

  const getDaysUntilDue = (endTime?: string) => {
    if (!endTime) return null
    const now = new Date()
    const dueDate = new Date(endTime)
    const diff = dueDate.getTime() - now.getTime()
    return Math.ceil(diff / (1000 * 60 * 60 * 24))
  }

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy':
        return 'success'
      case 'Medium':
        return 'warning'
      case 'Hard':
        return 'error'
      default:
        return 'default'
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      
      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Welcome Section */}
        <Box sx={{ mb: 4 }}>
          <Typography variant='h4' sx={{ fontWeight: 700, mb: 1 }}>
            Xin chào, {user.email.split('@')[0]}! 👋
          </Typography>
          <Typography variant='body1' color='text.secondary'>
            Hôm nay bạn muốn học gì?
          </Typography>
        </Box>

        {/* Lớp học của tôi */}
        <Box sx={{ mb: 5 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <ClassIcon sx={{ mr: 1, color: 'primary.main' }} />
            <Typography variant='h5' sx={{ fontWeight: 600, flexGrow: 1 }}>
              Lớp học của tôi
            </Typography>
            <IconButton component={Link} to='/classes' size='small'>
              <ArrowForwardIcon />
            </IconButton>
          </Box>
          
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr' }, gap: 3 }}>
            {classes.map((classItem: any) => (
              <Card elevation={0} sx={{ border: '1px solid', borderColor: 'divider', height: '100%' }} key={classItem.id}>
                <CardActionArea component={Link} to={`/class/${classItem.id}`}>
                  <Box
                    sx={{
                      height: 120,
                      bgcolor: 'primary.main',
                      backgroundImage: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    <Typography variant='h6' sx={{ color: 'white', fontWeight: 700, px: 2, textAlign: 'center' }}>
                      {classItem.code}
                    </Typography>
                  </Box>
                  <CardContent>
                    <Typography variant='h6' sx={{ fontWeight: 600, mb: 1 }}>
                      {classItem.name}
                    </Typography>
                    <Typography variant='body2' color='text.secondary' sx={{ mb: 1 }}>
                      {classItem.teacherName}
                    </Typography>
                    <Chip label={classItem.semester} size='small' />
                  </CardContent>
                </CardActionArea>
              </Card> 
            ))}
          </Box>
        </Box>

        <Divider sx={{ my: 4 }} />

        {/* Việc cần làm (7 ngày tới) */}
        <Box sx={{ mb: 5 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <AccessTimeIcon sx={{ mr: 1, color: 'error.main' }} />
            <Typography variant='h5' sx={{ fontWeight: 600 }}>
              Việc cần làm
            </Typography>
            <Chip label={`${upcomingAssignments.length} bài tập`} size='small' sx={{ ml: 2 }} color='error' />
          </Box>
          
          {upcomingAssignments.length === 0 ? (
            <Paper sx={{ p: 3, textAlign: 'center' }}>
              <Typography color='text.secondary'>
                Bạn không có bài tập nào sắp đến hạn trong 7 ngày tới 🎉
              </Typography>
            </Paper>
          ) : (
            <Stack spacing={2}>
              {upcomingAssignments.map((assignment: Assignment) => {
                const daysLeft = getDaysUntilDue(assignment.endTime)
                if (daysLeft === null) return null
                
                return (
                  <Card elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }} key={assignment.assignmentId}>
                    <CardActionArea component={Link} to={`/assignment/${assignment.assignmentId}`}>
                      <CardContent>
                        <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
                          <Box
                            sx={{
                              width: 48,
                              height: 48,
                              borderRadius: 2,
                              bgcolor: 'error.light',
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              flexShrink: 0,
                            }}
                          >
                            <AssignmentIcon sx={{ color: 'error.main' }} />
                          </Box>
                          <Box sx={{ flexGrow: 1 }}>
                            <Typography variant='h6' sx={{ fontWeight: 600, mb: 0.5 }}>
                              {assignment.title}
                            </Typography>
                            <Typography variant='body2' color='text.secondary' sx={{ mb: 1 }}>
                              {assignment.assignmentType}
                            </Typography>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
                              <Chip
                                icon={<AccessTimeIcon />}
                                label={`Còn ${daysLeft} ngày`}
                                size='small'
                                color={daysLeft <= 2 ? 'error' : 'warning'}
                              />
                              <Chip label={`${assignment.totalProblems || 0} bài`} size='small' variant='outlined' />
                              {assignment.totalPoints && (
                                <Chip label={`${assignment.totalPoints} điểm`} size='small' variant='outlined' />
                              )}
                            </Box>
                          </Box>
                        </Box>
                      </CardContent>
                    </CardActionArea>
                  </Card>
                )
              })}
            </Stack>
          )}
        </Box>

        <Divider sx={{ my: 4 }} />

        {/* Luyện tập */}
        <Box sx={{ mb: 5 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <CodeIcon sx={{ mr: 1, color: 'success.main' }} />
            <Typography variant='h5' sx={{ fontWeight: 600, flexGrow: 1 }}>
              Luyện tập
            </Typography>
            <IconButton component={Link} to='/practice' size='small'>
              <ArrowForwardIcon />
            </IconButton>
          </Box>
          
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr', lg: '1fr 1fr 1fr 1fr' }, gap: 2 }}>
            {practiceCategories.map((category: any) => (
              <Card elevation={0} sx={{ border: '1px solid', borderColor: 'divider', height: '100%' }} key={category.id}>
                <CardActionArea component={Link} to={`/practice/${category.id}`}>
                  <CardContent>
                    <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                      <Typography variant='h4' sx={{ mr: 1 }}>
                        {category.icon}
                      </Typography>
                      <Typography variant='h6' sx={{ fontWeight: 600 }}>
                        {category.name}
                      </Typography>
                    </Box>
                    <Typography variant='body2' color='text.secondary' sx={{ mb: 2 }}>
                      {category.description}
                    </Typography>
                    <Chip label={`${category.problemCount} bài`} size='small' color='success' variant='outlined' />
                  </CardContent>
                </CardActionArea>
              </Card>
            ))}
          </Box>
        </Box>
      </Container>
    </Box>
  )
}
