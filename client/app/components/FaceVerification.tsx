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
  const noFaceCountRef = useRef(0);
  const animationFrameRef = useRef<number | null>(null);
  const faceAlignedRef = useRef(false);
  
  const [status, setStatus] = useState<'initializing' | 'ready' | 'verifying' | 'success' | 'failed'>('initializing');
  const [message, setMessage] = useState<string>('Đang khởi tạo hệ thống nhận diện...');
  const [progress, setProgress] = useState(0);
  const [faceAligned, setFaceAligned] = useState(false);

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
    // Random select 4 challenges from 5 available (exclude 'center')
    // Then add 'center' as the final step to capture the best image
    const availableChallenges = allChallenges.filter(c => c.id !== 'center');
    const shuffled = [...availableChallenges].sort(() => 0.5 - Math.random());
    const selected = shuffled.slice(0, 4);
    
    // Add 'center' as the final mandatory step for image capture
    const centerChallenge = allChallenges.find(c => c.id === 'center')!;
    challengesRef.current = [...selected, centerChallenge];
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
    if (!humanRef.current || !videoRef.current || !canvasRef.current) return;

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ 
        video: { facingMode: 'user', width: { ideal: 640 }, height: { ideal: 480 } } 
      });
      videoRef.current.srcObject = stream;
      
      // Wait for video to be ready
      await new Promise<void>((resolve) => {
        videoRef.current!.onloadedmetadata = () => {
          // Set canvas size to match video
          canvasRef.current!.width = videoRef.current!.videoWidth;
          canvasRef.current!.height = videoRef.current!.videoHeight;
          resolve();
        };
      });
      
      await videoRef.current.play();
      
      setStatus('verifying');
      setMessage('Đặt khuôn mặt vào trong khung oval');
      setFaceAligned(false);
      faceAlignedRef.current = false;
      noFaceCountRef.current = 0;
      currentStepRef.current = 0;
      isLockedRef.current = false;
      
      // Draw initial oval guide
      drawOvalGuide(canvasRef.current, false);
      
      detect();
    } catch (error) {
      console.error('Camera access denied:', error);
      setStatus('failed');
      setMessage('Không thể truy cập camera. Vui lòng cấp quyền.');
    }
  };

  // Draw oval guide on canvas
  const drawOvalGuide = (canvas: HTMLCanvasElement, isAligned: boolean) => {
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Draw semi-transparent overlay
    ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Calculate oval dimensions (centered, portrait orientation)
    const centerX = canvas.width / 2;
    const centerY = canvas.height / 2;
    const radiusX = canvas.width * 0.35; // 70% of width
    const radiusY = canvas.height * 0.45; // 90% of height

    // Clear the oval area
    ctx.save();
    ctx.beginPath();
    ctx.ellipse(centerX, centerY, radiusX, radiusY, 0, 0, 2 * Math.PI);
    ctx.clip();
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.restore();

    // Draw oval border
    ctx.beginPath();
    ctx.ellipse(centerX, centerY, radiusX, radiusY, 0, 0, 2 * Math.PI);
    ctx.strokeStyle = isAligned ? '#4caf50' : '#2196f3';
    ctx.lineWidth = 4;
    ctx.stroke();
  };

  // Check if face is within oval bounds (for size/position only, not rotation)
  const checkFaceInBounds = (face: FaceResult, canvas: HTMLCanvasElement): { inBounds: boolean; hint: string } => {
    if (!face.box || canvas.width === 0 || canvas.height === 0) {
      return { inBounds: false, hint: 'Không tìm thấy khuôn mặt' };
    }

    const centerX = canvas.width / 2;
    const centerY = canvas.height / 2;
    const radiusX = canvas.width * 0.35;
    const radiusY = canvas.height * 0.45;

    // Get face bounding box - Human.js returns [x, y, width, height] in pixels
    const faceX = face.box[0] + face.box[2] / 2;
    const faceY = face.box[1] + face.box[3] / 2;
    const faceWidth = face.box[2];
    const faceHeight = face.box[3];

    // Debug: log actual values
    // console.log('Face:', { faceWidth, faceHeight, canvasW: canvas.width, canvasH: canvas.height });

    // Check position - face center must be close to oval center
    const distanceX = faceX - centerX;
    const distanceY = faceY - centerY;
    
    if (Math.abs(distanceX) > radiusX * 0.35) {
      return { inBounds: false, hint: distanceX > 0 ? 'Di chuyển sang trái' : 'Di chuyển sang phải' };
    }
    if (Math.abs(distanceY) > radiusY * 0.35) {
      return { inBounds: false, hint: distanceY > 0 ? 'Di chuyển lên trên' : 'Di chuyển xuống dưới' };
    }

    // Check size - STRICT: face must fill at least 50% of oval width/height
    // Oval width = radiusX * 2, height = radiusY * 2
    const ovalWidth = radiusX * 2;
    const ovalHeight = radiusY * 2;
    
    const fillRatioX = faceWidth / ovalWidth;
    const fillRatioY = faceHeight / ovalHeight;

    // Face must fill at least 50% of oval (Binance style)
    if (fillRatioX < 0.5 || fillRatioY < 0.5) {
      return { inBounds: false, hint: 'Tiến lại gần camera hơn' };
    }
    // Face should not be larger than oval
    if (fillRatioX > 0.95 || fillRatioY > 0.95) {
      return { inBounds: false, hint: 'Lùi ra xa camera hơn' };
    }

    return { inBounds: true, hint: '' };
  };

  // Check if face fits within oval guide - Binance style (strict) - for initial alignment
  const checkFaceAlignment = (face: FaceResult, canvas: HTMLCanvasElement): { aligned: boolean; hint: string } => {
    // First check bounds with STRICTER size requirement for initial alignment
    if (!face.box || canvas.width === 0 || canvas.height === 0) {
      return { aligned: false, hint: 'Không tìm thấy khuôn mặt' };
    }

    const centerX = canvas.width / 2;
    const centerY = canvas.height / 2;
    const radiusX = canvas.width * 0.35;
    const radiusY = canvas.height * 0.45;

    const faceX = face.box[0] + face.box[2] / 2;
    const faceY = face.box[1] + face.box[3] / 2;
    const faceWidth = face.box[2];
    const faceHeight = face.box[3];

    // Check position - centering (more lenient)
    const distanceX = faceX - centerX;
    const distanceY = faceY - centerY;
    
    if (Math.abs(distanceX) > radiusX * 0.35) {
      return { aligned: false, hint: distanceX > 0 ? 'Di chuyển sang phải' : 'Di chuyển sang trái' };
    }
    if (Math.abs(distanceY) > radiusY * 0.35) {
      return { aligned: false, hint: distanceY > 0 ? 'Di chuyển lên trên' : 'Di chuyển xuống dưới' };
    }

    // Size check - face must fill 60-95% of oval
    const ovalWidth = radiusX * 2;
    const ovalHeight = radiusY * 2;
    const fillRatioX = faceWidth / ovalWidth;
    const fillRatioY = faceHeight / ovalHeight;

    if (fillRatioX < 0.6 || fillRatioY < 0.6) {
      return { aligned: false, hint: 'Tiến lại gần camera hơn' };
    }
    if (fillRatioX > 0.95 || fillRatioY > 0.95) {
      return { aligned: false, hint: 'Lùi ra xa camera hơn' };
    }

    // Check rotation (more lenient)
    const yaw = face.rotation?.angle?.yaw ?? 1;
    const pitch = face.rotation?.angle?.pitch ?? 1;
    
    if (Math.abs(yaw) > 0.2) {
      return { aligned: false, hint: yaw > 0 ? 'Quay mặt sang trái một chút' : 'Quay mặt sang phải một chút' };
    }
    if (Math.abs(pitch) > 0.2) {
      return { aligned: false, hint: pitch > 0 ? 'Hạ cằm xuống một chút' : 'Ngẩng mặt lên một chút' };
    }

    return { aligned: true, hint: '' };
  };

  // Detection loop following face.html pattern exactly
  const detect = async () => {
    const human = humanRef.current;
    const video = videoRef.current;
    const canvas = canvasRef.current;
    const challenges = challengesRef.current;
    
    if (!human || !video || !canvas || canvas.width === 0) return;

    const result = await human.detect(video);
    
    // Check if face is detected
    if (result.face?.[0]) {
      noFaceCountRef.current = 0;
      
      const face = result.face[0];
      
      if (!faceAlignedRef.current) {
        // Initial alignment phase - check full alignment (position + size + rotation)
        const { aligned, hint } = checkFaceAlignment(face, canvas);
        drawOvalGuide(canvas, aligned);
        
        if (aligned) {
          faceAlignedRef.current = true;
          setFaceAligned(true);
          setMessage(challenges[0]?.label || 'Bắt đầu...');
        } else {
          setMessage(hint || 'Đặt khuôn mặt vào trong khung oval');
        }
      } else {
        // Challenge phase - only check bounds (position + size), allow rotation for challenges
        const { inBounds, hint } = checkFaceInBounds(face, canvas);
        drawOvalGuide(canvas, inBounds);
        
        if (!inBounds) {
          // Face moved out of bounds - reset progress!
          currentStepRef.current = 0;
          setProgress(0);
          faceAlignedRef.current = false;
          setFaceAligned(false);
          setMessage(hint || 'Đặt khuôn mặt vào trong khung oval');
          isLockedRef.current = false;
        } else {
          // Face still in bounds, check challenges
          if (currentStepRef.current >= challenges.length) return;

          // Check if current challenge passes
          if (challenges[currentStepRef.current]?.check(face)) {
            goToNextStep();
          }
        }
      }
    } else {
      // No face detected
      noFaceCountRef.current++;
      
      // Draw empty oval guide
      if (canvas) {
        drawOvalGuide(canvas, false);
      }

      // If no face for ~2 seconds (60 frames at 30fps), reset to beginning
      if (noFaceCountRef.current > 60) {
        currentStepRef.current = 0;
        setProgress(0);
        faceAlignedRef.current = false;
        setFaceAligned(false);
        setMessage('Đặt khuôn mặt vào trong khung oval');
        noFaceCountRef.current = 0;
        isLockedRef.current = false;
      }
    }
    
    animationFrameRef.current = requestAnimationFrame(detect);
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
    
    // Stop animation frame
    if (animationFrameRef.current) {
      cancelAnimationFrame(animationFrameRef.current);
    }
    
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

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (animationFrameRef.current) {
        cancelAnimationFrame(animationFrameRef.current);
      }
      const video = videoRef.current;
      if (video?.srcObject) {
        const stream = video.srcObject as MediaStream;
        stream?.getTracks().forEach(track => track.stop());
      }
    };
  }, []);

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
