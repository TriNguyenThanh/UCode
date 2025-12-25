import { useState, useEffect } from 'react'
import { redirect, useLoaderData, Link, useRevalidator } from 'react-router'
import type { Route } from './+types/teacher.class.$id'
import { auth } from '~/auth'
import * as ClassService from '~/services/classService'
import type { Class, Assignment, AttendanceSession } from '~/types/index'
import { Navigation } from '~/components/Navigation'
import { AttendanceQRDialog } from '~/components/AttendanceQRDialog'
import { getClassById } from '~/services/classService'
import { getAssignmentsByClass, deleteAssignment } from '~/services/assignmentService'
import { getAttendanceSessions } from '~/services/attendanceService'
import {
  Box,
  Container,
  Typography,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Tabs,
  Tab,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import PeopleIcon from '@mui/icons-material/People'
import QrCodeIcon from '@mui/icons-material/QrCode2'
import VisibilityIcon from '@mui/icons-material/Visibility'

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  try {
    // Fetch class data
    const classData = await ClassService.getClassDetail(params.id)
    
    // Try to fetch assignments, but don't fail if assignment service is unavailable
    let assignments: any[] = []
    try {
      assignments = await getAssignmentsByClass(params.id)
    } catch (assignmentError) {
      console.warn('Assignment service unavailable:', assignmentError)
    }

    // Try to fetch attendance sessions
    let attendanceSessions: AttendanceSession[] = []
    try {
      attendanceSessions = await getAttendanceSessions(params.id)
    } catch (attendanceError) {
      console.warn('Attendance service unavailable:', attendanceError)
    }

    return { user, classData, assignments, attendanceSessions }
  } catch (error) {
    console.error('Failed to load class data:', error)
    throw new Response('Không thể tải thông tin lớp học', { status: 500 })
  }
}

export default function TeacherClassDetail() {
  const { classData, assignments, attendanceSessions } = useLoaderData<typeof clientLoader>()
  const revalidator = useRevalidator()
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [assignmentToDelete, setAssignmentToDelete] = useState<string | null>(null)
  const [archivedDialogOpen, setArchivedDialogOpen] = useState(false)
  const [currentTab, setCurrentTab] = useState(0)
  const [qrDialogOpen, setQrDialogOpen] = useState(false)
  const [selectedSession, setSelectedSession] = useState<AttendanceSession | null>(null)

  // Hiển thị dialog nếu lớp bị archive
  useEffect(() => {
    if (classData.isArchived) {
      setArchivedDialogOpen(true)
    }
  }, [classData.isArchived])

  const handleDeleteAssignment = async () => {
    if (!assignmentToDelete) return
    
    try {
      await deleteAssignment(assignmentToDelete)
      setDeleteDialogOpen(false)
      setAssignmentToDelete(null)
      revalidator.revalidate()
    } catch (error) {
      console.error('Failed to delete assignment:', error)
      alert('Không thể xóa bài tập')
    }
  }

  const openDeleteDialog = (assignmentId: string) => {
    setAssignmentToDelete(assignmentId)
    setDeleteDialogOpen(true)
  }

  const openQRDialog = (session: AttendanceSession) => {
    setSelectedSession(session)
    setQrDialogOpen(true)
  }

  const closeQRDialog = () => {
    setQrDialogOpen(false)
    setSelectedSession(null)
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
          <Box
            sx={{
              width: 80,
              height: 80,
              borderRadius: 2,
              bgcolor: 'secondary.main',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <Typography variant="h4" sx={{ color: 'primary.main', fontWeight: 'bold' }}>
              {classData.classCode.charAt(0)}
            </Typography>
          </Box>
          <Box sx={{ flex: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 0.5 }}>
              <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
                {classData.className}
              </Typography>
              {classData.isArchived && (
                <Chip 
                  label="Ngừng hoạt động" 
                  color="warning" 
                  size="small"
                  sx={{ fontWeight: 600 }}
                />
              )}
            </Box>
            <Typography variant="body1" color="text.secondary">
              Mã lớp: {classData.classCode} • Học kỳ: {classData.semester}
            </Typography>
          </Box>
          <Button
            variant="outlined"
            startIcon={<PeopleIcon />}
            component={Link}
            to={`/teacher/class/${classData.classId}/students`}
            disabled={classData.isArchived}
            sx={{
              borderColor: 'secondary.main',
              color: 'secondary.main',
              '&:hover': {
                borderColor: 'primary.main',
                bgcolor: 'primary.main',
                color: 'white',
              },
              '&.Mui-disabled': {
                borderColor: 'grey.400',
                color: 'grey.400',
              },
            }}
          >
            Quản lý sinh viên ({classData.studentCount})
          </Button>
        </Box>
        <Typography variant="body1" color="text.secondary">
          {classData.description || 'Không có mô tả'}
        </Typography>
      </Box>

      {/* Tabs for Assignments and Attendance */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs value={currentTab} onChange={(e, newValue) => setCurrentTab(newValue)}>
          <Tab label="Bài tập" />
          <Tab label="Điểm danh" />
        </Tabs>
      </Box>

      {/* Assignments Tab */}
      {currentTab === 0 && (
        <>
          <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h5" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
              Danh sách bài tập
            </Typography>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              component={Link}
              to={`/teacher/class/${classData.classId}/create-assignment`}
              disabled={classData.isArchived}
              sx={{
                bgcolor: 'secondary.main',
                color: 'primary.main',
                '&:hover': {
                  bgcolor: 'primary.main',
                  color: 'secondary.main',
                },
                '&.Mui-disabled': {
                  bgcolor: 'grey.300',
                  color: 'grey.500',
                },
              }}
            >
              Tạo bài tập mới
            </Button>
          </Box>

      <TableContainer component={Paper} sx={{ boxShadow: 3 }}>
        <Table>
          <TableHead sx={{ bgcolor: 'secondary.main' }}>
            <TableRow>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Tên bài tập</TableCell>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Hạn nộp</TableCell>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Số bài</TableCell>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Trạng thái</TableCell>
              <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }} align="right">
                Thao tác
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {assignments.map((assignment) => {
              const submittedCount = Math.floor(Math.random() * classData.studentCount)
              const endTime = assignment.endTime ? new Date(assignment.endTime) : null
              const daysUntilDue = endTime
                ? Math.ceil((endTime.getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24))
                : null
              const isOverdue = daysUntilDue !== null && daysUntilDue < 0
              const isActive = assignment.status === 'PUBLISHED'

              return (
                <TableRow key={assignment.assignmentId} hover>
                  <TableCell>
                    <Link
                      to={`/teacher/assignment/${assignment.assignmentId}`}
                      style={{ textDecoration: 'none', color: 'inherit', fontWeight: 500 }}
                    >
                      {assignment.title}
                    </Link>
                  </TableCell>
                  <TableCell>
                    {endTime ? (
                      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                        <Typography variant="body2">
                          {endTime.toLocaleDateString('vi-VN')}
                        </Typography>
                        {isOverdue ? (
                          <Typography variant="caption" color="error">
                            Quá hạn {Math.abs(daysUntilDue!)} ngày
                          </Typography>
                        ) : daysUntilDue !== null ? (
                          <Typography variant="caption" color="text.secondary">
                            Còn {daysUntilDue} ngày
                          </Typography>
                        ) : null}
                      </Box>
                    ) : (
                      <Typography variant="body2" color="text.secondary">
                        Không giới hạn
                      </Typography>
                    )}
                  </TableCell>
                  <TableCell>
                    <Chip label={`${assignment.problems?.length || 0} bài`} size="small" />
                  </TableCell>
                  <TableCell>
                    {assignment.status === 'PUBLISHED' ? (
                      <Chip label="Đã giao" color="success" size="small" />
                    ) : assignment.status === 'CLOSED' ? (
                      <Chip label="Đã đóng" color="error" size="small" />
                    ) : (
                      <Chip label="Nháp" color="default" size="small" />
                    )}
                  </TableCell>
                  <TableCell align="right">
                    <IconButton
                      size="small"
                      component={Link}
                      to={`/teacher/assignment/${assignment.assignmentId}`}
                      disabled={classData.isArchived}
                      sx={{ 
                        color: 'secondary.main',
                        '&.Mui-disabled': { color: 'grey.400' }
                      }}
                    >
                      <EditIcon fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={() => openDeleteDialog(assignment.assignmentId)}
                      disabled={classData.isArchived}
                      sx={{ 
                        color: 'text.secondary', 
                        '&:hover': { color: 'error.main' },
                        '&.Mui-disabled': { color: 'grey.400' }
                      }}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      </TableContainer>

      {assignments.length === 0 && (
        <Paper sx={{ p: 6, textAlign: 'center', bgcolor: 'grey.50' }}>
          <Typography variant="h6" color="text.secondary" gutterBottom>
            Chưa có bài tập nào
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Tạo bài tập đầu tiên cho lớp học này
          </Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            component={Link}
            to={`/teacher/class/${classData.classId}/create-assignment`}
            disabled={classData.isArchived}
            sx={{
              bgcolor: 'secondary.main',
              color: 'primary.main',
              '&:hover': {
                bgcolor: 'primary.main',
                color: 'secondary.main',
              },
              '&.Mui-disabled': {
                bgcolor: 'grey.300',
                color: 'grey.500',
              },
            }}
          >
            Tạo bài tập
          </Button>
        </Paper>
      )}
        </>
      )}

      {/* Attendance Tab */}
      {currentTab === 1 && (
        <>
          <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h5" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
              Danh sách phiên điểm danh
            </Typography>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              component={Link}
              to={`/teacher/class/${classData.classId}/create-attendance`}
              disabled={classData.isArchived}
              sx={{
                bgcolor: 'secondary.main',
                color: 'primary.main',
                '&:hover': {
                  bgcolor: 'primary.main',
                  color: 'secondary.main',
                },
                '&.Mui-disabled': {
                  bgcolor: 'grey.300',
                  color: 'grey.500',
                },
              }}
            >
              Tạo phiên điểm danh
            </Button>
          </Box>

          <TableContainer component={Paper} sx={{ boxShadow: 3 }}>
            <Table>
              <TableHead sx={{ bgcolor: 'secondary.main' }}>
                <TableRow>
                  <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Tiêu đề</TableCell>
                  <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Mã phiên</TableCell>
                  <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Thời gian</TableCell>
                  <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }}>Trạng thái</TableCell>
                  <TableCell sx={{ color: 'primary.main', fontWeight: 'bold' }} align="right">
                    Thao tác
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {attendanceSessions.map((session) => {
                  const startTime = new Date(session.startTime)
                  const endTime = new Date(session.endTime)
                  const now = new Date()
                  const isOngoing = now >= startTime && now <= endTime
                  const isPast = now > endTime
                  const isFuture = now < startTime

                  return (
                    <TableRow key={session.id} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight={500}>
                          {session.title}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" fontFamily="monospace">
                          {session.sessionCode}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                          <Typography variant="body2">
                            {startTime.toLocaleDateString('vi-VN')} {startTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            đến {endTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        {!session.isActive ? (
                          <Chip label="Tạm dừng" color="default" size="small" />
                        ) : isOngoing ? (
                          <Chip label="Đang diễn ra" color="success" size="small" />
                        ) : isPast ? (
                          <Chip label="Đã kết thúc" color="error" size="small" />
                        ) : (
                          <Chip label="Sắp diễn ra" color="info" size="small" />
                        )}
                      </TableCell>
                      <TableCell align="right">
                        <IconButton
                          size="small"
                          component={Link}
                          to={`/teacher/class/${classData.classId}/attendance/${session.id}`}
                          sx={{ 
                            color: 'secondary.main',
                          }}
                          title="Xem chi tiết"
                        >
                          <VisibilityIcon fontSize="small" />
                        </IconButton>
                        <IconButton
                          size="small"
                          onClick={() => openQRDialog(session)}
                          disabled={classData.isArchived || !session.isActive}
                          sx={{ 
                            color: 'secondary.main',
                            '&.Mui-disabled': { color: 'grey.400' }
                          }}
                          title="Xem QR Code"
                        >
                          <QrCodeIcon fontSize="small" />
                        </IconButton>
                        <IconButton
                          size="small"
                          component={Link}
                          to={`/teacher/class/${classData.classId}/attendance/${session.id}/edit`}
                          disabled={classData.isArchived}
                          sx={{ 
                            color: 'secondary.main',
                            '&.Mui-disabled': { color: 'grey.400' }
                          }}
                          title="Chỉnh sửa cấu hình"
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>
          </TableContainer>

          {attendanceSessions.length === 0 && (
            <Paper sx={{ p: 6, textAlign: 'center', bgcolor: 'grey.50' }}>
              <Typography variant="h6" color="text.secondary" gutterBottom>
                Chưa có phiên điểm danh nào
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Tạo phiên điểm danh đầu tiên cho lớp học này
              </Typography>
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                component={Link}
                to={`/teacher/class/${classData.classId}/create-attendance`}
                disabled={classData.isArchived}
                sx={{
                  bgcolor: 'secondary.main',
                  color: 'primary.main',
                  '&:hover': {
                    bgcolor: 'primary.main',
                    color: 'secondary.main',
                  },
                  '&.Mui-disabled': {
                    bgcolor: 'grey.300',
                    color: 'grey.500',
                  },
                }}
              >
                Tạo phiên điểm danh
              </Button>
            </Paper>
          )}
        </>
      )}

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle sx={{ bgcolor: 'error.main', color: 'white' }}>
          Xác nhận xóa bài tập
        </DialogTitle>
        <DialogContent sx={{ mt: 2 }}>
          <Typography>
            Bạn có chắc chắn muốn xóa bài tập này? 
            Tất cả dữ liệu liên quan sẽ bị xóa và không thể khôi phục.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Hủy</Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleDeleteAssignment}
          >
            Xóa bài tập
          </Button>
        </DialogActions>
      </Dialog>

      {/* Archived Class Dialog */}
      <Dialog 
        open={archivedDialogOpen} 
        onClose={() => setArchivedDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ bgcolor: 'warning.main', color: 'white', display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box component="span" sx={{ fontSize: 24 }}>⚠️</Box>
          Lớp học đã ngừng hoạt động
        </DialogTitle>
        <DialogContent sx={{ mt: 2 }}>
          <Typography variant="body1" sx={{ mb: 2, fontWeight: 600 }}>
            Lớp học <strong>{classData.className}</strong> đã bị ngừng hoạt động.
          </Typography>
          {classData.archiveReason && (
            <Box sx={{ p: 2, bgcolor: 'grey.100', borderRadius: 1, borderLeft: '4px solid', borderColor: 'warning.main' }}>
              <Typography variant="body2" sx={{ fontWeight: 600, mb: 1, color: 'text.secondary' }}>
                Lý do:
              </Typography>
              <Typography variant="body1">
                {classData.archiveReason}
              </Typography>
            </Box>
          )}
          {classData.archivedAt && (
            <Typography variant="body2" sx={{ mt: 2, color: 'text.secondary' }}>
              Ngày ngừng: {new Date(classData.archivedAt).toLocaleDateString('vi-VN')}
            </Typography>
          )}
          <Typography variant="body2" sx={{ mt: 2, color: 'text.secondary', fontStyle: 'italic' }}>
            Bạn vẫn có thể xem thông tin lớp học nhưng không thể thực hiện các thao tác khác. 
            Vui lòng liên hệ quản trị viên nếu cần hỗ trợ.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button 
            variant="contained"
            onClick={() => setArchivedDialogOpen(false)}
            sx={{
              bgcolor: 'secondary.main',
              color: 'primary.main',
              '&:hover': { bgcolor: 'primary.main', color: 'white' }
            }}
          >
            Đã hiểu
          </Button>
        </DialogActions>
      </Dialog>

      {/* QR Code Dialog */}
      <AttendanceQRDialog
        open={qrDialogOpen}
        session={selectedSession}
        onClose={closeQRDialog}
      />
      </Container>
    </Box>
  )
}
