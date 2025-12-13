import * as React from 'react'
import { useLoaderData, redirect, Link, useNavigate } from 'react-router'
import type { Route } from './+types/class.$id'
import { auth } from '~/auth'
import * as ClassService from '~/services/classService'
import { getStudentAssignmentsByClass } from '~/services/assignmentService'
import { Navigation } from '~/components/Navigation'
import type { Assignment, AssignmentStatus } from '~/types'
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  CardActionArea,
  Chip,
  Paper,
  IconButton,
  Divider,
  Avatar,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import AssignmentIcon from '@mui/icons-material/Assignment'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import PendingIcon from '@mui/icons-material/Pending'
import WarningIcon from '@mui/icons-material/Warning'

export const meta: Route.MetaFunction = ({ params }) => [
  { title: `Lớp học | UCode` },
  { name: 'description', content: 'Chi tiết lớp học và danh sách bài tập.' }
]

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')

  try {
    // Student chỉ cần thông tin cơ bản của class, không cần detail với danh sách sinh viên
    const classData = await ClassService.getClassById(params.id)
  
    const assignments = await getStudentAssignmentsByClass(params.id)

    return { user, classData, assignments }
  } catch (error) {
    console.error('Error loading class detail:', error)
    throw new Response('Lớp học không tồn tại', { status: 404 })
  }
}

export default function ClassDetail() {
  const { classData, assignments, user } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  
  const [examDialogOpen, setExamDialogOpen] = React.useState(false)
  const [selectedAssignment, setSelectedAssignment] = React.useState<Assignment | null>(null)

  const handleAssignmentClick = (assignment: Assignment, event: React.MouseEvent) => {
    // Only show dialog for students accessing EXAMINATION type assignments
    if (user.role === 'student' && assignment.assignmentType === 'EXAMINATION') {
      event.preventDefault()
      setSelectedAssignment(assignment)
      setExamDialogOpen(true)
    }
    // For other types or roles, let the default Link behavior work
  }

  const handleExamConfirm = () => {
    if (selectedAssignment) {
      setExamDialogOpen(false)
      navigate(`/assignment/${selectedAssignment.assignmentId}`)
    }
  }

  const handleExamCancel = () => {
    setExamDialogOpen(false)
    setSelectedAssignment(null)
  }

  const getDaysUntilDue = (endTime?: string) => {
    if (!endTime) return null
    const now = new Date()
    const dueDate = new Date(endTime)
    const diff = dueDate.getTime() - now.getTime()
    return Math.ceil(diff / (1000 * 60 * 60 * 24))
  }

  const getStatusInfo = (assignment: Assignment) => {
    // Status based on assignment.status (AssignmentStatus type)
    const status: AssignmentStatus = assignment.status
    
    switch (status) {
      case 'DRAFT':
        return { 
          label: 'Bản nháp', 
          color: 'default' as const, 
          icon: <PendingIcon /> 
        }
        
      case 'PUBLISHED': {
        const daysLeft = getDaysUntilDue(assignment.endTime)
        if (daysLeft === null) {
          return { 
            label: 'Đang diễn ra', 
            color: 'success' as const, 
            icon: <CheckCircleIcon /> 
          }
        }
        if (daysLeft < 0) {
          return { 
            label: 'Quá hạn', 
            color: 'error' as const, 
            icon: <PendingIcon /> 
          }
        }
        if (daysLeft === 0) {
          return { 
            label: 'Hết hạn hôm nay', 
            color: 'error' as const, 
            icon: <AccessTimeIcon /> 
          }
        }
        if (daysLeft === 1) {
          return { 
            label: 'Còn 1 ngày', 
            color: 'error' as const, 
            icon: <AccessTimeIcon /> 
          }
        }
        if (daysLeft <= 3) {
          return { 
            label: `Còn ${daysLeft} ngày`, 
            color: 'error' as const, 
            icon: <AccessTimeIcon /> 
          }
        }
        if (daysLeft <= 7) {
          return { 
            label: `Còn ${daysLeft} ngày`, 
            color: 'warning' as const, 
            icon: <AccessTimeIcon /> 
          }
        }
        return { 
          label: `Còn ${daysLeft} ngày`, 
          color: 'info' as const, 
          icon: <AccessTimeIcon /> 
        }
      }
      
      case 'CLOSED':
        return { 
          label: 'Đã đóng', 
          color: 'error' as const, 
          icon: <PendingIcon /> 
        }
    }
  }

  const getAssignmentTypeLabel = (type: string) => {
    const typeMap: Record<string, string> = {
      HOMEWORK: 'Bài tập',
      EXAMINATION: 'Kiểm tra',
      PRACTICE: 'Luyện tập',
    }
    return typeMap[type] || type
  }

  // Show loading screen while navigation is in progress
  // Now handled by root.tsx global loading

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Back Button */}
        <IconButton component={Link} to='/home' sx={{ mb: 2 }}>
          <ArrowBackIcon />
        </IconButton>

        {/* Class Header */}
        <Paper
          sx={{
            mb: 4,
            overflow: 'hidden',
            background: 'linear-gradient(135deg, #00275e 0%, #003d8f 100%)'
          }}
        >
          <Box sx={{ p: 4 }}>
            <Chip
              label={classData.classCode}
              sx={{
                mb: 2,
                bgcolor: 'primary.main',
                color: 'secondary.main',
                fontWeight: 700,
                fontSize: '0.9rem'
              }}
            />
            <Typography variant='h3' sx={{ fontWeight: 700, color: 'white', mb: 2 }}>
              {classData.className}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main' }}>
                  {classData.teacherName.charAt(0)}
                </Avatar>
                <Typography variant='body1' sx={{ color: 'primary.main', fontWeight: 500 }}>
                  {classData.teacherName}
                </Typography>
              </Box>
              <Chip
                label={classData.semester}
                sx={{ bgcolor: 'rgba(250, 203, 1, 0.1)', color: 'primary.main', borderColor: 'primary.main' }}
                variant='outlined'
              />
            </Box>
            {classData.description && (
              <Typography variant='body1' sx={{ mt: 2, color: 'rgba(255,255,255,0.8)' }}>
                {classData.description}
              </Typography>
            )}
          </Box>
        </Paper>

        {/* Assignment Summary */}
        {assignments.length > 0 && (
          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 2, mb: 4 }}>
            <Card elevation={0} sx={{ border: '2px solid', borderColor: 'divider' }}>
              <CardContent>
                <Typography variant='body2' color='text.secondary' sx={{ mb: 1 }}>
                  Tổng số bài tập
                </Typography>
                <Typography variant='h4' sx={{ fontWeight: 700, color: 'primary.main' }}>
                  {assignments.length}
                </Typography>
              </CardContent>
            </Card>

            <Card elevation={0} sx={{ border: '2px solid', borderColor: 'divider' }}>
              <CardContent>
                <Typography variant='body2' color='text.secondary' sx={{ mb: 1 }}>
                  Tổng điểm tối đa
                </Typography>
                <Typography variant='h4' sx={{ fontWeight: 700, color: 'success.main' }}>
                  {assignments.reduce((sum, a) => sum + (a.totalPoints ?? 0), 0)}
                </Typography>
              </CardContent>
            </Card>

            <Card elevation={0} sx={{ border: '2px solid', borderColor: 'divider' }}>
              <CardContent>
                <Typography variant='body2' color='text.secondary' sx={{ mb: 1 }}>
                  Đang diễn ra
                </Typography>
                <Typography variant='h4' sx={{ fontWeight: 700, color: 'info.main' }}>
                  {assignments.filter((a) => a.status === 'PUBLISHED').length}
                </Typography>
              </CardContent>
            </Card>
          </Box>
        )}

        {/* Assignments List */}
        <Box>
          <Typography variant='h5' sx={{ fontWeight: 600, mb: 3, display: 'flex', alignItems: 'center', gap: 1 }}>
            <AssignmentIcon sx={{ color: 'primary.main' }} />
            Danh sách bài tập ({assignments.length})
          </Typography>

          {assignments.length === 0 ? (
            <Paper sx={{ p: 4, textAlign: 'center' }}>
              <Typography color='text.secondary'>Chưa có bài tập nào được giao 📝</Typography>
            </Paper>
          ) : (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {assignments.map((assignment) => {
                const statusInfo = getStatusInfo(assignment)
                return (
                  <Card
                    key={assignment.assignmentId}
                    elevation={0}
                    sx={{
                      border: '2px solid',
                      borderColor: 'divider',
                      transition: 'all 0.2s',
                      '&:hover': {
                        borderColor: 'primary.main',
                        transform: 'translateY(-2px)',
                        boxShadow: 3
                      }
                    }}
                  >
                    <CardActionArea 
                      component={Link} 
                      to={`/assignment/${assignment.assignmentId}`}
                      onClick={(e) => handleAssignmentClick(assignment, e)}
                    >
                      <CardContent sx={{ p: 3 }}>
                        <Box sx={{ display: 'flex', gap: 3 }}>
                          {/* Icon */}
                          <Box
                            sx={{
                              width: 60,
                              height: 60,
                              borderRadius: 2,
                              bgcolor: 'secondary.main',
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              flexShrink: 0
                            }}
                          >
                            <AssignmentIcon sx={{ color: 'primary.main', fontSize: 32 }} />
                          </Box>

                          {/* Content */}
                          <Box sx={{ flexGrow: 1 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                              <Typography variant='h6' sx={{ fontWeight: 600 }}>
                                {assignment.title}
                              </Typography>
                              <Chip
                                label={getAssignmentTypeLabel(assignment.assignmentType)}
                                size='small'
                                color='primary'
                                sx={{ height: 20, fontSize: '0.7rem' }}
                              />
                            </Box>
                            
                            {assignment.description && (
                              <Typography variant='body2' color='text.secondary' sx={{ mb: 2 }}>
                                {assignment.description}
                              </Typography>
                            )}

                            {/* Stats */}
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
                              <Chip
                                icon={statusInfo.icon}
                                label={statusInfo.label}
                                size='small'
                                color={statusInfo.color}
                              />
                              
                              
                              {/* Total Points */}
                              <Chip 
                                label={`${assignment.totalPoints ?? 0} điểm`} 
                                size='small' 
                                variant='outlined'
                                color='primary'
                                sx={{ fontWeight: 600 }}
                              />

                              {/* Start Time */}
                              {assignment.startTime && (
                                <Chip
                                  label={`Bắt đầu: ${new Date(assignment.startTime).toLocaleDateString('vi-VN', {
                                    day: '2-digit',
                                    month: '2-digit',
                                    year: 'numeric'
                                  })}`}
                                  size='small'
                                  variant='outlined'
                                />
                              )}
                              
                              {/* End Time */}
                              {assignment.endTime && (
                                <Chip
                                  label={`Hạn: ${new Date(assignment.endTime).toLocaleDateString('vi-VN', {
                                    day: '2-digit',
                                    month: '2-digit',
                                    year: 'numeric'
                                  })}`}
                                  size='small'
                                  variant='outlined'
                                  color='error'
                                />
                              )}
                              
                              {/* Late Submission Indicator */}
                              {assignment.allowLateSubmission && (
                                <Chip
                                  label='Cho phép nộp trễ'
                                  size='small'
                                  variant='outlined'
                                  color='warning'
                                />
                              )}
                            </Box>

                            {/* Statistics (if available) */}
                            {assignment.statistics && (
                              <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid', borderColor: 'divider' }}>
                                <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
                                  <Box>
                                    <Typography variant='caption' color='text.secondary'>
                                      Đã nộp
                                    </Typography>
                                    <Typography variant='body2' sx={{ fontWeight: 600 }}>
                                      {assignment.statistics.submitted}/{assignment.statistics.totalStudents}
                                    </Typography>
                                  </Box>
                                  <Box>
                                    <Typography variant='caption' color='text.secondary'>
                                      Điểm TB
                                    </Typography>
                                    <Typography variant='body2' sx={{ fontWeight: 600 }}>
                                      {assignment.statistics.averageScore.toFixed(1)}
                                    </Typography>
                                  </Box>
                                  <Box>
                                    <Typography variant='caption' color='text.secondary'>
                                      Hoàn thành
                                    </Typography>
                                    <Typography variant='body2' sx={{ fontWeight: 600 }}>
                                      {assignment.statistics.completionRate.toFixed(0)}%
                                    </Typography>
                                  </Box>
                                </Box>
                              </Box>
                            )}
                          </Box>
                        </Box>
                      </CardContent>
                    </CardActionArea>
                  </Card>
                )
              })}
            </Box>
          )}
        </Box>
      </Container>

      {/* Examination Warning Dialog */}
      <Dialog
        open={examDialogOpen}
        onClose={handleExamCancel}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WarningIcon color="warning" />
          <Typography variant="h6">Bài kiểm tra - Lưu ý quan trọng</Typography>
        </DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ mb: 2 }}>
            <strong>Bài kiểm tra sẽ kiểm soát hành vi của bạn trong quá trình làm bài:</strong>
          </DialogContentText>
          <DialogContentText component="div" sx={{ mb: 2 }}>
            <Box component="ul" sx={{ pl: 2 }}>
              <li>Hệ thống sẽ ghi lại số lần bạn chuyển tab hoặc rời khỏi màn hình làm bài</li>
              <li>Mọi hoạt động bất thường sẽ được báo cáo cho giáo viên</li>
              <li>Việc chuyển tab nhiều lần có thể ảnh hưởng đến kết quả của bạn</li>
            </Box>
          </DialogContentText>
          <DialogContentText>
            Bạn có chắc chắn muốn bắt đầu làm bài kiểm tra này không?
          </DialogContentText>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={handleExamCancel} color="inherit">
            Hủy
          </Button>
          <Button onClick={handleExamConfirm} variant="contained" color="primary" autoFocus>
            Xác nhận và bắt đầu
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
