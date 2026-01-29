import { useState, lazy, Suspense } from 'react';
import { useNavigate } from 'react-router';
import {
  Box,
  Container,
  Paper,
  Typography,
  Alert,
  Button,
  CircularProgress,
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import { API } from '~/api';
import { auth } from '~/auth';

// Lazy load FaceVerification to avoid SSR issues with @vladmandic/human
const FaceVerification = lazy(() => import('~/components/FaceVerification'));

interface FaceRegisterResponse {
  userId: string;
  confidence: number;
  imageUrl: string;
  registeredAt: string;
}

export default function FaceRegistrationPage() {
  const navigate = useNavigate();
  const [isVerifying, setIsVerifying] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const user = auth.getUser();

  const handleFaceVerified = async (imageBase64: string) => {
    setIsLoading(true);
    setError(null);

    try {
      // Remove data URL prefix if exists
      const base64Image = imageBase64.replace(/^data:image\/\w+;base64,/, '');

      const response = await API.post<{
        success: boolean;
        data: FaceRegisterResponse | null;
        message: string;
        errors: string[] | null;
      }>('api/v1/face-auth/register', {
        image: base64Image,
      });

      if (response.data.success) {
        setSuccess(true);
        
        // Update localStorage
        localStorage.setItem('isFaceAuth', 'true');
        
        // Wait a bit then redirect
        setTimeout(() => {
          navigate('/student/home');
        }, 2000);
      } else {
        setError(response.data.message || 'Đăng ký khuôn mặt thất bại');
        setIsVerifying(false);
      }
    } catch (err: any) {
      console.error('Face registration error:', err);
      const message = err.response?.data?.message || err.message || 'Đã xảy ra lỗi khi đăng ký khuôn mặt';
      setError(message);
      setIsVerifying(false);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancel = () => {
    setIsVerifying(false);
  };

  const handleStartVerification = () => {
    setError(null);
    setIsVerifying(true);
  };

  if (success) {
    return (
      <Container maxWidth="md" sx={{ mt: 8 }}>
        <Paper elevation={3} sx={{ p: 4, textAlign: 'center' }}>
          <CheckCircleIcon sx={{ fontSize: 80, color: 'success.main', mb: 2 }} />
          <Typography variant="h4" gutterBottom>
            Đăng ký thành công!
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Khuôn mặt của bạn đã được đăng ký thành công. Đang chuyển hướng...
          </Typography>
        </Paper>
      </Container>
    );
  }

  if (isVerifying) {
    return (
      <Container maxWidth="md" sx={{ mt: 4 }}>
        <Suspense fallback={
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '400px' }}>
            <CircularProgress />
          </Box>
        }>
          <FaceVerification onVerified={handleFaceVerified} onCancel={handleCancel} />
        </Suspense>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ mt: 8 }}>
      <Paper elevation={3} sx={{ p: 4 }}>
        <Typography variant="h4" gutterBottom align="center">
          Đăng ký xác thực khuôn mặt
        </Typography>

        <Alert severity="info" sx={{ mb: 3 }}>
          Để sử dụng hệ thống, bạn cần đăng ký xác thực khuôn mặt. Điều này giúp bảo mật tài khoản và hỗ trợ điểm danh tự động.
        </Alert>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        <Box sx={{ mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Hướng dẫn:
          </Typography>
          <Typography variant="body2" component="div" sx={{ pl: 2 }}>
            <ol>
              <li>Nhấn nút "Bắt đầu đăng ký" bên dưới</li>
              <li>Cho phép truy cập camera khi được yêu cầu</li>
              <li>Làm theo hướng dẫn trên màn hình (chớp mắt, há miệng, quay đầu...)</li>
              <li>Hoàn thành tất cả các bước xác thực</li>
              <li>Hệ thống sẽ tự động lưu và kích hoạt tài khoản của bạn</li>
            </ol>
          </Typography>
        </Box>

        <Box sx={{ mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Lưu ý quan trọng:
          </Typography>
          <Typography variant="body2" component="div" sx={{ pl: 2 }}>
            <ul>
              <li>Đảm bảo ánh sáng đủ và khuôn mặt rõ ràng</li>
              <li>Không đeo khẩu trang hoặc kính râm</li>
              <li>Nhìn thẳng vào camera</li>
              <li>Mỗi sinh viên chỉ được đăng ký 1 lần</li>
              <li>Không được sử dụng ảnh của người khác</li>
            </ul>
          </Typography>
        </Box>

        <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2 }}>
          <Button
            variant="contained"
            color="primary"
            size="large"
            onClick={handleStartVerification}
            disabled={isLoading}
          >
            Bắt đầu đăng ký
          </Button>
        </Box>
      </Paper>
    </Container>
  );
}
