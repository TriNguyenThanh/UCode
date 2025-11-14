// import * as React from 'react'
// import { useLoaderData, redirect, useNavigate } from 'react-router'
// import type { Route } from './+types/admin.classes.$classId'
// import { auth } from '~/auth'
// import { Navigation } from '~/components/Navigation'
// import {
//   getClassDetailForAdmin,
//   archiveClassAdmin,
//   unarchiveClassAdmin,
//   deleteClassByAdmin,
//   type AdminClassDetail,
// } from '~/services/adminService'
// import {
//   Container,
//   Typography,
//   Box,
//   Card,
//   CardContent,
//   Button,
//   Chip,
//   IconButton,
//   Menu,
//   MenuItem,
//   Dialog,
//   DialogTitle,
//   DialogContent,
//   DialogActions,
//   CircularProgress,
//   Alert,
//   Snackbar,
//   TextField,
//   Breadcrumbs,
//   Link as MuiLink,
//   Divider,
//   Grid,
// } from '@mui/material'
// import { Link } from 'react-router'
// import ArrowBackIcon from '@mui/icons-material/ArrowBack'
// import MoreVertIcon from '@mui/icons-material/MoreVert'
// import EditIcon from '@mui/icons-material/Edit'
// import DeleteIcon from '@mui/icons-material/Delete'
// import ArchiveIcon from '@mui/icons-material/Archive'
// import UnarchiveIcon from '@mui/icons-material/Unarchive'
// import PeopleIcon from '@mui/icons-material/People'
// import AssignmentIcon from '@mui/icons-material/Assignment'
// import DescriptionIcon from '@mui/icons-material/Description'
// import PersonIcon from '@mui/icons-material/Person'
// import EmailIcon from '@mui/icons-material/Email'
// import CalendarTodayIcon from '@mui/icons-material/CalendarToday'
// import CheckCircleIcon from '@mui/icons-material/CheckCircle'
// import CancelIcon from '@mui/icons-material/Cancel'

// export const meta: Route.MetaFunction = () => [
//   { title: 'Chi tiết lớp học | Admin | UCode' },
// ]

// export async function clientLoader({ params }: Route.ClientLoaderArgs) {
//   const user = auth.getUser()
//   if (!user) throw redirect('/login')
//   if (user.role !== 'admin') throw redirect('/home')

//   return { user, classId: params.classId }
// }

// export default function AdminClassDetail() {
//   const { classId } = useLoaderData<typeof clientLoader>()
//   const navigate = useNavigate()

//   // State
//   const [classDetail, setClassDetail] = React.useState<AdminClassDetail | null>(null)
//   const [loading, setLoading] = React.useState(true)
//   const [error, setError] = React.useState<string | null>(null)
  
//   // Menu & Dialog
//   const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null)
//   const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false)
//   const [archiveDialogOpen, setArchiveDialogOpen] = React.useState(false)
//   const [archiveReason, setArchiveReason] = React.useState('')
//   const [snackbar, setSnackbar] = React.useState({ open: false, message: '', severity: 'success' as 'success' | 'error' })

//   // Load class detail
//   const loadClassDetail = React.useCallback(async () => {
//     try {
//       setLoading(true)
//       setError(null)
//       const data = await getClassDetailForAdmin(classId)
//       setClassDetail(data)
//     } catch (err: any) {
//       setError(err.message || 'Failed to load class detail')
//     } finally {
//       setLoading(false)
//     }
//   }, [classId])

//   React.useEffect(() => {
//     loadClassDetail()
//   }, [loadClassDetail])

//   const handleArchive = async () => {
//     if (!classDetail) return
    
//     try {
//       await archiveClassAdmin(classDetail.classId, archiveReason)
//       setSnackbar({ open: true, message: 'Lớp học đã được lưu trữ thành công', severity: 'success' })
//       loadClassDetail()
//     } catch (err: any) {
//       setSnackbar({ open: true, message: err.message || 'Lưu trữ lớp học thất bại', severity: 'error' })
//     } finally {
//       setArchiveDialogOpen(false)
//       setArchiveReason('')
//       setAnchorEl(null)
//     }
//   }

//   const handleUnarchive = async () => {
//     if (!classDetail) return
    
//     try {
//       await unarchiveClassAdmin(classDetail.classId)
//       setSnackbar({ open: true, message: 'Khôi phục lớp học thành công', severity: 'success' })
//       loadClassDetail()
//     } catch (err: any) {
//       setSnackbar({ open: true, message: err.message || 'Khôi phục lớp học thất bại', severity: 'error' })
//     } finally {
//       setAnchorEl(null)
//     }
//   }

//   const handleDelete = async () => {
//     if (!classDetail) return
    
//     try {
//       await deleteClassByAdmin(classDetail.classId)
//       setSnackbar({ open: true, message: 'Xóa lớp học thành công', severity: 'success' })
//       setTimeout(() => navigate('/admin/classes'), 1500)
//     } catch (err: any) {
//       setSnackbar({ open: true, message: err.message || 'Xóa lớp học thất bại', severity: 'error' })
//     } finally {
//       setDeleteDialogOpen(false)
//       setAnchorEl(null)
//     }
//   }

//   const handleMenuClose = () => {
//     setAnchorEl(null)
//   }

//   if (loading) {
//     return (
//       <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
//         <Navigation />
//         <Container maxWidth='xl' sx={{ py: 4 }}>
//           <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', py: 8 }}>
//             <CircularProgress />
//           </Box>
//         </Container>
//       </Box>
//     )
//   }

//   if (error || !classDetail) {
//     return (
//       <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
//         <Navigation />
//         <Container maxWidth='xl' sx={{ py: 4 }}>
//           <Alert severity='error'>
//             {error || 'Không tìm thấy lớp học'}
//           </Alert>
//           <Button
//             component={Link}
//             to='/admin/classes'
//             startIcon={<ArrowBackIcon />}
//             sx={{ mt: 2 }}
//           >
//             Quay lại danh sách
//           </Button>
//         </Container>
//       </Box>
//     )
//   }

//   return (
//     <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
//       <Navigation />

//       <Container maxWidth='xl' sx={{ py: 4 }}>
//         {/* Breadcrumbs */}
//         <Breadcrumbs sx={{ mb: 3 }}>
//           <MuiLink component={Link} to='/admin' underline='hover' color='inherit'>
//             Admin
//           </MuiLink>
//           <MuiLink component={Link} to='/admin/classes' underline='hover' color='inherit'>
//             Quản lý lớp học
//           </MuiLink>
//           <Typography color='text.primary'>{classDetail.name}</Typography>
//         </Breadcrumbs>

//         {/* Header */}
//         <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 4 }}>
//           <Box>
//             <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
//               <Typography variant='h3' sx={{ fontWeight: 700, color: '#1d1d1f' }}>
//                 {classDetail.name}
//               </Typography>
//               <Chip
//                 label={classDetail.classCode}
//                 sx={{
//                   bgcolor: '#007AFF',
//                   color: 'white',
//                   fontWeight: 600,
//                   fontSize: '0.875rem',
//                 }}
//               />
//               {classDetail.isArchived && (
//                 <Chip
//                   icon={<ArchiveIcon />}
//                   label='Đã lưu trữ'
//                   sx={{
//                     bgcolor: '#FF9500',
//                     color: 'white',
//                     fontWeight: 600,
//                   }}
//                 />
//               )}
//               {!classDetail.isActive && (
//                 <Chip
//                   icon={<CancelIcon />}
//                   label='Không hoạt động'
//                   color='error'
//                   sx={{ fontWeight: 600 }}
//                 />
//               )}
//             </Box>
//             <Typography variant='body1' sx={{ color: '#6e6e73', fontSize: '1rem' }}>
//               {classDetail.description}
//             </Typography>
//           </Box>

//           <Box sx={{ display: 'flex', gap: 1 }}>
//             <Button
//               component={Link}
//               to='/admin/classes'
//               variant='outlined'
//               startIcon={<ArrowBackIcon />}
//               sx={{
//                 borderRadius: 2,
//                 borderColor: '#d2d2d7',
//                 color: '#1d1d1f',
//                 textTransform: 'none',
//                 fontWeight: 600,
//               }}
//             >
//               Quay lại
//             </Button>
//             <IconButton
//               onClick={(e) => setAnchorEl(e.currentTarget)}
//               sx={{
//                 border: '1px solid #d2d2d7',
//                 borderRadius: 2,
//               }}
//             >
//               <MoreVertIcon />
//             </IconButton>
//           </Box>
//         </Box>

//         {/* Stats Cards */}
//         <Grid container spacing={2} sx={{ mb: 4 }}>
//           {[
//             {
//               icon: PeopleIcon,
//               label: 'Tổng sinh viên',
//               value: classDetail.studentCount,
//               color: '#007AFF',
//             },
//             {
//               icon: CheckCircleIcon,
//               label: 'Sinh viên hoạt động',
//               value: classDetail.activeStudentCount,
//               color: '#34C759',
//             },
//             {
//               icon: AssignmentIcon,
//               label: 'Tổng bài tập',
//               value: classDetail.assignmentCount,
//               color: '#FF9500',
//             },
//             {
//               icon: DescriptionIcon,
//               label: 'Tổng bài nộp',
//               value: classDetail.submissionCount,
//               color: '#AF52DE',
//             },
//           ].map((stat, index) => (
//             <Grid item xs={12} sm={6} md={3} key={index}>
//               <Card
//                 elevation={0}
//                 sx={{
//                   borderRadius: 3,
//                   bgcolor: 'white',
//                   border: '1px solid #d2d2d7',
//                 }}
//               >
//                 <CardContent>
//                   <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
//                     <stat.icon sx={{ color: stat.color, fontSize: 24, mr: 1 }} />
//                     <Typography variant='body2' sx={{ fontWeight: 600, color: '#86868b' }}>
//                       {stat.label}
//                     </Typography>
//                   </Box>
//                   <Typography variant='h3' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
//                     {stat.value}
//                   </Typography>
//                 </CardContent>
//               </Card>
//             </Grid>
//           ))}
//         </Grid>

//         {/* Class Information */}
//         <Card
//           elevation={0}
//           sx={{
//             borderRadius: 3,
//             bgcolor: 'white',
//             border: '1px solid #d2d2d7',
//             mb: 3,
//           }}
//         >
//           <CardContent sx={{ p: 4 }}>
//             <Typography variant='h5' sx={{ fontWeight: 700, mb: 3, color: '#1d1d1f' }}>
//               Thông tin lớp học
//             </Typography>

//             <Grid container spacing={3}>
//               <Grid item xs={12} md={6}>
//                 <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
//                   <PersonIcon sx={{ color: '#86868b', mr: 2 }} />
//                   <Box>
//                     <Typography variant='body2' sx={{ color: '#86868b', fontSize: '0.875rem' }}>
//                       Giảng viên
//                     </Typography>
//                     <Typography variant='body1' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
//                       {classDetail.teacherName}
//                     </Typography>
//                   </Box>
//                 </Box>

//                 <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
//                   <EmailIcon sx={{ color: '#86868b', mr: 2 }} />
//                   <Box>
//                     <Typography variant='body2' sx={{ color: '#86868b', fontSize: '0.875rem' }}>
//                       Email giảng viên
//                     </Typography>
//                     <Typography variant='body1' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
//                       {classDetail.teacherEmail}
//                     </Typography>
//                   </Box>
//                 </Box>
//               </Grid>

//               <Grid item xs={12} md={6}>
//                 <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
//                   <CalendarTodayIcon sx={{ color: '#86868b', mr: 2 }} />
//                   <Box>
//                     <Typography variant='body2' sx={{ color: '#86868b', fontSize: '0.875rem' }}>
//                       Ngày tạo
//                     </Typography>
//                     <Typography variant='body1' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
//                       {new Date(classDetail.createdAt).toLocaleDateString('vi-VN')}
//                     </Typography>
//                   </Box>
//                 </Box>

//                 <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
//                   <CalendarTodayIcon sx={{ color: '#86868b', mr: 2 }} />
//                   <Box>
//                     <Typography variant='body2' sx={{ color: '#86868b', fontSize: '0.875rem' }}>
//                       Cập nhật lần cuối
//                     </Typography>
//                     <Typography variant='body1' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
//                       {new Date(classDetail.updatedAt).toLocaleDateString('vi-VN')}
//                     </Typography>
//                   </Box>
//                 </Box>

//                 {classDetail.archivedAt && (
//                   <Box sx={{ display: 'flex', alignItems: 'center' }}>
//                     <ArchiveIcon sx={{ color: '#FF9500', mr: 2 }} />
//                     <Box>
//                       <Typography variant='body2' sx={{ color: '#86868b', fontSize: '0.875rem' }}>
//                         Ngày lưu trữ
//                       </Typography>
//                       <Typography variant='body1' sx={{ fontWeight: 600, color: '#1d1d1f' }}>
//                         {new Date(classDetail.archivedAt).toLocaleDateString('vi-VN')}
//                       </Typography>
//                     </Box>
//                   </Box>
//                 )}
//               </Grid>
//             </Grid>
//           </CardContent>
//         </Card>

//         {/* Context Menu */}
//         <Menu
//           anchorEl={anchorEl}
//           open={Boolean(anchorEl)}
//           onClose={handleMenuClose}
//           PaperProps={{
//             sx: {
//               borderRadius: 2,
//               boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
//             },
//           }}
//         >
//           <MenuItem onClick={handleMenuClose}>
//             <EditIcon sx={{ mr: 1, fontSize: 20 }} />
//             Chỉnh sửa
//           </MenuItem>
//           {classDetail.isArchived ? (
//             <MenuItem
//               onClick={() => {
//                 handleMenuClose()
//                 handleUnarchive()
//               }}
//               sx={{ color: 'info.main' }}
//             >
//               <UnarchiveIcon sx={{ mr: 1, fontSize: 20 }} />
//               Khôi phục
//             </MenuItem>
//           ) : (
//             <MenuItem
//               onClick={() => {
//                 handleMenuClose()
//                 setArchiveDialogOpen(true)
//               }}
//               sx={{ color: 'warning.main' }}
//             >
//               <ArchiveIcon sx={{ mr: 1, fontSize: 20 }} />
//               Lưu trữ
//             </MenuItem>
//           )}
//           <MenuItem
//             onClick={() => {
//               handleMenuClose()
//               setDeleteDialogOpen(true)
//             }}
//             sx={{ color: 'error.main' }}
//           >
//             <DeleteIcon sx={{ mr: 1, fontSize: 20 }} />
//             Xóa vĩnh viễn
//           </MenuItem>
//         </Menu>

//         {/* Archive Dialog */}
//         <Dialog
//           open={archiveDialogOpen}
//           onClose={() => {
//             setArchiveDialogOpen(false)
//             setArchiveReason('')
//           }}
//           maxWidth='sm'
//           fullWidth
//         >
//           <DialogTitle>Lưu trữ lớp học</DialogTitle>
//           <DialogContent>
//             <Typography variant='body2' sx={{ mb: 2, color: '#86868b' }}>
//               Bạn có chắc chắn muốn lưu trữ lớp học <strong>{classDetail.name}</strong>?
//               Lớp học sẽ không hiển thị cho giảng viên và sinh viên.
//             </Typography>
//             <TextField
//               fullWidth
//               multiline
//               rows={3}
//               label='Lý do lưu trữ (không bắt buộc)'
//               placeholder='VD: Kết thúc học kỳ, chuyển giảng viên...'
//               value={archiveReason}
//               onChange={(e) => setArchiveReason(e.target.value)}
//               sx={{ mt: 2 }}
//             />
//           </DialogContent>
//           <DialogActions sx={{ px: 3, pb: 2 }}>
//             <Button
//               onClick={() => {
//                 setArchiveDialogOpen(false)
//                 setArchiveReason('')
//               }}
//               sx={{ textTransform: 'none' }}
//             >
//               Hủy
//             </Button>
//             <Button
//               onClick={handleArchive}
//               variant='contained'
//               startIcon={<ArchiveIcon />}
//               sx={{
//                 textTransform: 'none',
//                 bgcolor: '#FF9500',
//                 '&:hover': { bgcolor: '#CC7700' },
//               }}
//             >
//               Lưu trữ
//             </Button>
//           </DialogActions>
//         </Dialog>

//         {/* Delete Dialog */}
//         <Dialog
//           open={deleteDialogOpen}
//           onClose={() => setDeleteDialogOpen(false)}
//           maxWidth='xs'
//         >
//           <DialogTitle>Xóa lớp học vĩnh viễn</DialogTitle>
//           <DialogContent>
//             <Typography variant='body2' sx={{ color: '#86868b' }}>
//               Bạn có chắc chắn muốn xóa vĩnh viễn lớp học <strong>{classDetail.name}</strong>?
//               Hành động này không thể hoàn tác.
//             </Typography>
//           </DialogContent>
//           <DialogActions sx={{ px: 3, pb: 2 }}>
//             <Button onClick={() => setDeleteDialogOpen(false)} sx={{ textTransform: 'none' }}>
//               Hủy
//             </Button>
//             <Button
//               onClick={handleDelete}
//               variant='contained'
//               color='error'
//               startIcon={<DeleteIcon />}
//               sx={{ textTransform: 'none' }}
//             >
//               Xóa vĩnh viễn
//             </Button>
//           </DialogActions>
//         </Dialog>

//         {/* Snackbar */}
//         <Snackbar
//           open={snackbar.open}
//           autoHideDuration={4000}
//           onClose={() => setSnackbar({ ...snackbar, open: false })}
//           anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
//         >
//           <Alert
//             onClose={() => setSnackbar({ ...snackbar, open: false })}
//             severity={snackbar.severity}
//             sx={{ width: '100%' }}
//           >
//             {snackbar.message}
//           </Alert>
//         </Snackbar>
//       </Container>
//     </Box>
//   )
// }
