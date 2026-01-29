import { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  Alert,
  CircularProgress,
} from '@mui/material';
import FaceIcon from '@mui/icons-material/Face';
import { auth } from '~/auth';

interface FaceAuthGuardProps {
  children: React.ReactNode;
}

/**
 * Component bảo vệ toàn bộ app - chỉ cho phép sinh viên đã xác thực khuôn mặt sử dụng
 * 
 * Logic:
 * - Nếu chưa đăng nhập → không block (để redirect logic khác xử lý)
 * - Nếu là Teacher/Admin → không block (không yêu cầu face auth)
 * - Nếu là Student và chưa xác thực khuôn mặt → block toàn bộ, chỉ cho phép vào trang đăng ký
 * - Nếu đang ở trang đăng ký → không block
 */
export default function FaceAuthGuard({ children }: FaceAuthGuardProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const [isChecking, setIsChecking] = useState(true);
  const [shouldBlock, setShouldBlock] = useState(false);
  const [user, setUser] = useState(auth.getUser());

  useEffect(() => {
    const checkFaceAuth = async () => {
      setIsChecking(true);

      try {
        const currentUser = auth.getUser();
        setUser(currentUser);

        // Không block nếu chưa đăng nhập
        if (!currentUser) {
          setShouldBlock(false);
          setIsChecking(false);
          return;
        }

        // Không block Teacher và Admin
        if (currentUser.role === 'teacher' || currentUser.role === 'admin') {
          setShouldBlock(false);
          setIsChecking(false);
          return;
        }

        // Không block nếu đang ở trang đăng ký khuôn mặt hoặc login
        const allowedPaths = ['/face-registration', '/login', '/logout'];
        if (allowedPaths.some(path => location.pathname === path)) {
          setShouldBlock(false);
          setIsChecking(false);
          return;
        }

        // Kiểm tra Student đã xác thực khuôn mặt chưa
        if (currentUser.role === 'student') {
          // Fetch fresh user data to get latest isFaceAuth status
          const freshUser = await auth.getUserProfile();
          
          if (!freshUser?.isFaceAuth) {
            // Chưa xác thực → block
            setShouldBlock(true);
          } else {
            // Đã xác thực → cho phép
            setShouldBlock(false);
          }
        } else {
          setShouldBlock(false);
        }
      } catch (error) {
        console.error('Error checking face auth:', error);
        // Nếu có lỗi, không block để tránh khóa user
        setShouldBlock(false);
      } finally {
        setIsChecking(false);
      }
    };

    checkFaceAuth();
  }, [location.pathname]);

  const handleGoToRegistration = () => {
    navigate('/face-registration');
  };

  // Đang kiểm tra
  if (isChecking) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          height: '100vh',
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  // Block nếu chưa xác thực
  if (shouldBlock) {
    return (
      <>
        {children}
        <Dialog
          open={true}
          maxWidth="sm"
          fullWidth
          disableEscapeKeyDown
          onClose={(_, reason) => {
            if (reason === 'backdropClick' || reason === 'escapeKeyDown') {
              return;
            }
          }}
        >
          <DialogTitle sx={{ textAlign: 'center', pt: 3 }}>
            <FaceIcon sx={{ fontSize: 60, color: 'primary.main', mb: 1 }} />
            <Typography variant="h5">Yêu cầu xác thực khuôn mặt</Typography>
          </DialogTitle>
          <DialogContent>
            <Alert severity="warning" sx={{ mb: 2 }}>
              Bạn cần đăng ký xác thực khuôn mặt để sử dụng hệ thống.
            </Alert>
            <Typography variant="body1" gutterBottom>
              Để đảm bảo bảo mật và hỗ trợ điểm danh tự động, tất cả sinh viên cần đăng ký xác thực khuôn mặt trước khi sử dụng các tính năng của hệ thống.
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
              Quá trình đăng ký chỉ mất vài phút và chỉ cần thực hiện một lần duy nhất.
            </Typography>
          </DialogContent>
          <DialogActions sx={{ justifyContent: 'center', pb: 3 }}>
            <Button
              variant="contained"
              color="primary"
              size="large"
              startIcon={<FaceIcon />}
              onClick={handleGoToRegistration}
            >
              Đăng ký ngay
            </Button>
          </DialogActions>
        </Dialog>
      </>
    );
  }

  // Cho phép truy cập
  return <>{children}</>;
}
