import React, { useEffect, useRef, useState, useCallback } from 'react';
import { Box, Typography, CircularProgress, Button, Paper } from '@mui/material';
import type { Config, Human as HumanType, FaceResult } from '@vladmandic/human';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import FaceIcon from '@mui/icons-material/Face';

// Dynamic import to avoid SSR issues - only import Human class
let Human: any;

if (typeof window !== 'undefined') {
  import('@vladmandic/human').then((module) => {
    Human = module.Human;
  });
}

interface FaceVerificationProps {
  onVerified: (image: string) => void;
  onCancel: () => void;
}

type ChallengeType = 'center' | 'blink' | 'mouth' | 'left' | 'right' | 'up';

interface Challenge {
  id: ChallengeType;
  label: string;
  check: (face: FaceResult) => boolean;
}

const humanConfig: Partial<Config> = {
  modelBasePath: 'https://cdn.jsdelivr.net/npm/@vladmandic/human/models/',
  face: { 
    enabled: true, 
    detector: { rotation: true }, 
    mesh: { enabled: true } 
  },
  body: { enabled: false },
  hand: { enabled: false },
  gesture: { enabled: true }
};

export default function FaceVerification({ onVerified, onCancel }: FaceVerificationProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const humanRef = useRef<HumanType | null>(null);
  const isLockedRef = useRef(false);
  const currentStepRef = useRef(0);
  const challengesRef = useRef<Challenge[]>([]);
  
  const [status, setStatus] = useState<'initializing' | 'ready' | 'verifying' | 'success' | 'failed'>('initializing');
  const [message, setMessage] = useState<string>('Đang khởi tạo hệ thống nhận diện...');
  const [progress, setProgress] = useState(0);

  // Define all 6 challenges using face.html algorithm (check function receives face object directly)
  const allChallenges: Challenge[] = [
    {
      id: 'center',
      label: 'Nhìn thẳng vào camera',
      check: (f) => Math.abs(f.rotation?.angle?.yaw ?? 1) < 0.15 && Math.abs(f.rotation?.angle?.pitch ?? 1) < 0.15
    },
    {
      id: 'left',
      label: 'Quay sang TRÁI màn hình',
      check: (f) => (f.rotation?.angle?.yaw ?? 0) < -0.35
    },
    {
      id: 'right',
      label: 'Quay sang PHẢI màn hình',
      check: (f) => (f.rotation?.angle?.yaw ?? 0) > 0.35
    },
    {
      id: 'up',
      label: 'Ngước mặt lên TRÊN',
      check: (f) => (f.rotation?.angle?.pitch ?? 0) < -0.25
    },
    {
      id: 'blink',
      label: 'Vui lòng chớp mắt',
      check: (f) => {
        // Use mesh points to detect eye closure
        // Points 159, 145 are upper/lower eyelid for left eye
        // Points 386, 374 are upper/lower eyelid for right eye
        if (!f.mesh || f.mesh.length < 400) return false;
        
        const leftUpper = f.mesh[159]?.[1] ?? 0;
        const leftLower = f.mesh[145]?.[1] ?? 0;
        const rightUpper = f.mesh[386]?.[1] ?? 0;
        const rightLower = f.mesh[374]?.[1] ?? 0;
        
        const leftEyeOpen = Math.abs(leftLower - leftUpper);
        const rightEyeOpen = Math.abs(rightLower - rightUpper);
        
        // Eyes closed when distance is small (< 5 pixels typically)
        return leftEyeOpen < 8 || rightEyeOpen < 8;
      }
    },
    {
      id: 'mouth',
      label: 'Vui lòng há miệng',
      check: (f) => {
        // Use mesh points to detect mouth opening
        // Points 13, 14 are upper/lower lip center
        if (!f.mesh || f.mesh.length < 400) return false;
        
        const upperLip = f.mesh[13]?.[1] ?? 0;
        const lowerLip = f.mesh[14]?.[1] ?? 0;
        const mouthOpen = Math.abs(lowerLip - upperLip);
        
        // Mouth open when distance is large (> 15 pixels)
        return mouthOpen > 15;
      }
    }
  ];

  const generateChallenges = useCallback(() => {
    // Random select 4 challenges from all 6 available
    const shuffled = [...allChallenges].sort(() => 0.5 - Math.random());
    const selected = shuffled.slice(0, 4);
    challengesRef.current = selected;
    currentStepRef.current = 0;
    setProgress(0);
  }, []);

  useEffect(() => {
    generateChallenges();
    
    const initHuman = async () => {
      try {
        const human = new Human(humanConfig);
        await human.load();
        await human.warmup();
        humanRef.current = human;
        setStatus('ready');
        setMessage('Sẵn sàng. Nhấn Bắt đầu để xác thực.');
      } catch (error) {
        console.error('Failed to init Human:', error);
        setStatus('failed');
        setMessage('Không thể khởi tạo hệ thống nhận diện khuôn mặt.');
      }
    };

    initHuman();
  }, [generateChallenges]);

  const startVerification = async () => {
    if (!humanRef.current || !videoRef.current) return;

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' } });
      videoRef.current.srcObject = stream;
      await videoRef.current.play();
      
      setStatus('verifying');
      setMessage(challengesRef.current[0]?.label || 'Bắt đầu...');
      detect();
    } catch (error) {
      console.error('Camera access denied:', error);
      setStatus('failed');
      setMessage('Không thể truy cập camera. Vui lòng cấp quyền.');
    }
  };

  // Detection loop following face.html pattern exactly
  const detect = async () => {
    const human = humanRef.current;
    const video = videoRef.current;
    const challenges = challengesRef.current;
    
    if (!human || !video) return;
    if (currentStepRef.current >= challenges.length) return;

    const result = await human.detect(video);
    
    // Check if face detected and current challenge passes
    if (result.face?.[0] && challenges[currentStepRef.current]?.check(result.face[0])) {
      goToNextStep();
    }
    
    requestAnimationFrame(detect);
  };

  const goToNextStep = () => {
    if (isLockedRef.current) return;
    isLockedRef.current = true;
    
    const challenges = challengesRef.current;
    
    setTimeout(() => {
      currentStepRef.current++;
      const newProgress = (currentStepRef.current / challenges.length) * 100;
      setProgress(newProgress);
      
      if (currentStepRef.current < challenges.length) {
        setMessage(challenges[currentStepRef.current].label);
        isLockedRef.current = false;
      } else {
        // All challenges completed
        finishVerification();
      }
    }, 800);
  };

  const finishVerification = () => {
    setStatus('success');
    setMessage('✅ Xác thực thành công!');
    
    const video = videoRef.current;
    if (!video) return;
    
    // Capture image
    const captureCanvas = document.createElement('canvas');
    captureCanvas.width = video.videoWidth;
    captureCanvas.height = video.videoHeight;
    const ctx = captureCanvas.getContext('2d');
    if (ctx) {
      ctx.drawImage(video, 0, 0);
      const imageBase64 = captureCanvas.toDataURL('image/jpeg', 0.8);
      
      // Stop camera
      const stream = video.srcObject as MediaStream;
      stream?.getTracks().forEach(track => track.stop());
      
      setTimeout(() => onVerified(imageBase64), 1000);
    }
  };

  return (
    <Paper elevation={3} sx={{ p: 3, maxWidth: 600, mx: 'auto', textAlign: 'center' }}>
      <Typography variant="h5" gutterBottom>
        Xác thực khuôn mặt
      </Typography>

      <Box sx={{ position: 'relative', width: '100%', height: 400, bgcolor: '#000', mb: 2, borderRadius: 2, overflow: 'hidden' }}>
        <video
          ref={videoRef}
          style={{ 
            width: '100%', 
            height: '100%', 
            objectFit: 'cover',
            transform: 'scaleX(-1)' // Mirror effect
          }}
          playsInline
          muted
        />
        <canvas
          ref={canvasRef}
          style={{ 
            position: 'absolute', 
            top: 0, 
            left: 0, 
            width: '100%', 
            height: '100%',
            transform: 'scaleX(-1)' // Mirror effect
          }}
        />
        
        {status === 'initializing' && (
          <Box sx={{ position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'rgba(0,0,0,0.7)' }}>
            <CircularProgress color="primary" />
          </Box>
        )}

        {status === 'success' && (
          <Box sx={{ position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'rgba(0,0,0,0.7)' }}>
            <CheckCircleIcon sx={{ fontSize: 80, color: 'success.main' }} />
          </Box>
        )}
      </Box>

      <Typography variant="h6" color={status === 'failed' ? 'error' : 'primary'} gutterBottom>
        {message}
      </Typography>

      {status === 'verifying' && (
        <Box sx={{ width: '100%', height: 10, bgcolor: '#e0e0e0', borderRadius: 5, mb: 2, overflow: 'hidden' }}>
          <Box sx={{ width: `${progress}%`, height: '100%', bgcolor: 'primary.main', transition: 'width 0.3s ease' }} />
        </Box>
      )}

      <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2 }}>
        {status === 'ready' && (
          <Button variant="contained" color="primary" startIcon={<FaceIcon />} onClick={startVerification}>
            Bắt đầu xác thực
          </Button>
        )}
        
        {status === 'failed' && (
          <Button variant="contained" color="primary" onClick={() => window.location.reload()}>
            Thử lại
          </Button>
        )}

        <Button variant="outlined" color="secondary" onClick={onCancel}>
          Hủy bỏ
        </Button>
      </Box>
    </Paper>
  );
}
