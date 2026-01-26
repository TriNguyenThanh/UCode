import { useState } from 'react'
import { QRCodeSVG } from 'qrcode.react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  TextField,
  InputAdornment,
  IconButton,
} from '@mui/material'
import QrCodeIcon from '@mui/icons-material/QrCode2'
import ContentCopyIcon from '@mui/icons-material/ContentCopy'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import type { AttendanceSession } from '~/types'

interface AttendanceQRDialogProps {
  open: boolean
  session: AttendanceSession | null
  onClose: () => void
}

export function AttendanceQRDialog({ open, session, onClose }: AttendanceQRDialogProps) {
  const [copied, setCopied] = useState(false)

  const handleCopyUrl = async () => {
    if (!session) return
    const attendanceUrl = `${window.location.origin}/attendance/${session.sessionCode}`
    try {
      await navigator.clipboard.writeText(attendanceUrl)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch (err) {
      console.error('Failed to copy:', err)
    }
  }

  const handleClose = () => {
    setCopied(false)
    onClose()
  }

  if (!session) return null

  const attendanceUrl = `${window.location.origin}/attendance/${session.sessionCode}`

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle
        sx={{
          bgcolor: 'secondary.main',
          color: 'primary.main',
          display: 'flex',
          alignItems: 'center',
          gap: 1,
        }}
      >
        <QrCodeIcon />
        QR Code Điểm Danh
      </DialogTitle>
      <DialogContent sx={{ mt: 2 }}>
        <Typography variant="h6" align="center" gutterBottom>
          {session.title}
        </Typography>
        <Typography variant="body2" color="text.secondary" align="center" gutterBottom>
          Mã phiên: <strong>{session.sessionCode}</strong>
        </Typography>

        {/* QR Code */}
        <Box sx={{ display: 'flex', justifyContent: 'center', my: 3 }}>
          <QRCodeSVG
            value={attendanceUrl}
            size={400}
            level="H"
            includeMargin={true}
            imageSettings={{
              src: '/logo.png',
              x: undefined,
              y: undefined,
              height: 60,
              width: 60,
              excavate: true,
            }}
          />
        </Box>

        {/* URL Section */}
        <Typography variant="body2" fontWeight="bold" gutterBottom>
          Link điểm danh:
        </Typography>
        <TextField
          fullWidth
          value={attendanceUrl}
          InputProps={{
            readOnly: true,
            endAdornment: (
              <InputAdornment position="end">
                <IconButton onClick={handleCopyUrl} edge="end">
                  {copied ? <CheckCircleIcon color="success" /> : <ContentCopyIcon />}
                </IconButton>
              </InputAdornment>
            ),
          }}
          size="small"
        />
        {copied && (
          <Typography variant="caption" color="success.main" sx={{ mt: 1, display: 'block' }}>
            Đã sao chép vào clipboard!
          </Typography>
        )}

        {/* Session Info */}
        <Box sx={{ mt: 3, p: 2, bgcolor: 'grey.50', borderRadius: 1 }}>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            <strong>Thời gian bắt đầu:</strong>{' '}
            {new Date(session.startTime).toLocaleString('vi-VN')}
          </Typography>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            <strong>Thời gian kết thúc:</strong>{' '}
            {new Date(session.endTime).toLocaleString('vi-VN')}
          </Typography>
          {session.requireIpCheck && (
            <Typography variant="body2" color="text.secondary" gutterBottom>
              <strong>Kiểm tra IP:</strong> Có ({session.allowedIpSubnet})
            </Typography>
          )}
          {session.requireGpsCheck && (
            <Typography variant="body2" color="text.secondary" gutterBottom>
              <strong>Kiểm tra GPS:</strong> Có (Bán kính: {session.allowedRadiusMeters}m)
            </Typography>
          )}
          {session.requireFaceCheck && (
            <Typography variant="body2" color="text.secondary">
              <strong>Kiểm tra khuôn mặt:</strong> Có
            </Typography>
          )}
        </Box>
      </DialogContent>
      <DialogActions>
        <Button
          variant="contained"
          onClick={handleClose}
          sx={{
            bgcolor: 'secondary.main',
            color: 'primary.main',
            '&:hover': { bgcolor: 'primary.main', color: 'white' },
          }}
        >
          Đóng
        </Button>
      </DialogActions>
    </Dialog>
  )
}
