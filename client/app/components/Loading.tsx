import React from 'react'
import { Box, CircularProgress, Typography, Paper } from '@mui/material'
import CodeIcon from '@mui/icons-material/Code'

interface LoadingProps {
  message?: string
  fullScreen?: boolean
  size?: 'small' | 'medium' | 'large'
}

export function Loading({ 
  message = 'Đang tải...', 
  fullScreen = false,
  size = 'medium' 
}: LoadingProps) {
  const sizeMap = {
    small: 40,
    medium: 60,
    large: 80,
  }

  const content = (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 3,
        p: 4,
      }}
    >
      {/* Animated Logo */}
      <Box
        sx={{
          position: 'relative',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        {/* Spinning Circle */}
        <CircularProgress
          size={sizeMap[size]}
          thickness={4}
          sx={{
            color: 'primary.main',
            position: 'absolute',
          }}
        />
        
        {/* Center Icon */}
        <CodeIcon
          sx={{
            fontSize: sizeMap[size] * 0.5,
            color: 'primary.main',
            animation: 'pulse 2s ease-in-out infinite',
            '@keyframes pulse': {
              '0%, 100%': {
                opacity: 1,
                transform: 'scale(1)',
              },
              '50%': {
                opacity: 0.5,
                transform: 'scale(0.95)',
              },
            },
          }}
        />
      </Box>

      {/* Loading Message */}
      <Typography
        variant={size === 'small' ? 'body2' : 'h6'}
        sx={{
          color: 'text.secondary',
          fontWeight: 500,
          textAlign: 'center',
          animation: 'fadeInOut 2s ease-in-out infinite',
          '@keyframes fadeInOut': {
            '0%, 100%': {
              opacity: 1,
            },
            '50%': {
              opacity: 0.5,
            },
          },
        }}
      >
        {message}
      </Typography>

      {/* Animated Dots */}
      <Box
        sx={{
          display: 'flex',
          gap: 1,
        }}
      >
        {[0, 1, 2].map((i) => (
          <Box
            key={i}
            sx={{
              width: 8,
              height: 8,
              borderRadius: '50%',
              bgcolor: 'primary.main',
              animation: `bounce 1.4s ease-in-out ${i * 0.16}s infinite`,
              '@keyframes bounce': {
                '0%, 80%, 100%': {
                  transform: 'scale(0)',
                  opacity: 0.5,
                },
                '40%': {
                  transform: 'scale(1)',
                  opacity: 1,
                },
              },
            }}
          />
        ))}
      </Box>
    </Box>
  )

  if (fullScreen) {
    return (
      <Box
        sx={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          bgcolor: 'rgba(255, 255, 255, 0.9)',
          backdropFilter: 'blur(4px)',
          zIndex: 9999,
        }}
      >
        <Paper
          elevation={4}
          sx={{
            borderRadius: 2,
            overflow: 'hidden',
          }}
        >
          {content}
        </Paper>
      </Box>
    )
  }

  return content
}

// Loading Skeleton for specific use cases
export function LoadingSkeleton({ type = 'text' }: { type?: 'text' | 'card' | 'table' }) {
  const baseStyle = {
    bgcolor: 'grey.200',
    borderRadius: 1,
    animation: 'shimmer 1.5s ease-in-out infinite',
    '@keyframes shimmer': {
      '0%': {
        opacity: 1,
      },
      '50%': {
        opacity: 0.4,
      },
      '100%': {
        opacity: 1,
      },
    },
  }

  if (type === 'text') {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
        <Box sx={{ ...baseStyle, height: 20, width: '80%' }} />
        <Box sx={{ ...baseStyle, height: 20, width: '60%' }} />
        <Box sx={{ ...baseStyle, height: 20, width: '90%' }} />
      </Box>
    )
  }

  if (type === 'card') {
    return (
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Box sx={{ ...baseStyle, height: 200, mb: 2 }} />
        <Box sx={{ ...baseStyle, height: 20, width: '70%', mb: 1 }} />
        <Box sx={{ ...baseStyle, height: 16, width: '50%' }} />
      </Paper>
    )
  }

  if (type === 'table') {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
        {[...Array(5)].map((_, i) => (
          <Box
            key={i}
            sx={{
              ...baseStyle,
              height: 40,
              animationDelay: `${i * 0.1}s`,
            }}
          />
        ))}
      </Box>
    )
  }

  return null
}
