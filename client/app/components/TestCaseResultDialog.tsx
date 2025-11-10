import React from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Paper,
  Chip,
  IconButton,
  Divider,
  Grid,
} from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import MemoryIcon from '@mui/icons-material/Memory'
import ErrorIcon from '@mui/icons-material/Error'
import BuildIcon from '@mui/icons-material/Build'
import SkipNextIcon from '@mui/icons-material/SkipNext'
import WarningIcon from '@mui/icons-material/Warning' 

interface TestCaseResultDialogProps {
  open: boolean
  onClose: () => void
  compareResult: string
  passedTestcase: number
  totalTestcase: number
  submissionId: string
}

// Enum mapping từ C# backend
enum TestcaseStatus {
  Passed = 0,
  TimeLimitExceeded = 1,
  MemoryLimitExceeded = 2,
  RuntimeError = 3,
  InternalError = 4,
  WrongAnswer = 5,
  CompilationError = 6,
  Skipped = 7,
}

interface TestCaseInfo {
  index: number
  status: TestcaseStatus
  statusText: string
  icon: React.ReactNode
  color: 'success' | 'error' | 'warning' | 'info' | 'default'
}

// Parse compareResult string to array of test case statuses
function parseCompareResult(compareResult: string): TestCaseInfo[] {
  if (!compareResult) return []

  return compareResult.split('').map((char, index) => {
    const status = parseInt(char, 10) as TestcaseStatus
    return {
      index: index + 1,
      status,
      ...getStatusInfo(status),
    }
  })
}

// Get status display info
function getStatusInfo(status: TestcaseStatus): {
  statusText: string
  icon: React.ReactNode
  color: 'success' | 'error' | 'warning' | 'info' | 'default'
} {
  switch (status) {
    case TestcaseStatus.Passed:
      return {
        statusText: 'Passed',
        icon: <CheckCircleIcon fontSize="small" />,
        color: 'success',
      }
    case TestcaseStatus.TimeLimitExceeded:
      return {
        statusText: 'Time Limit Exceeded',
        icon: <AccessTimeIcon fontSize="small" />,
        color: 'warning',
      }
    case TestcaseStatus.MemoryLimitExceeded:
      return {
        statusText: 'Memory Limit Exceeded',
        icon: <MemoryIcon fontSize="small" />,
        color: 'warning',
      }
    case TestcaseStatus.RuntimeError:
      return {
        statusText: 'Runtime Error',
        icon: <ErrorIcon fontSize="small" />,
        color: 'error',
      }
    case TestcaseStatus.InternalError:
      return {
        statusText: 'Internal Error',
        icon: <WarningIcon fontSize="small" />,
        color: 'error',
      }
    case TestcaseStatus.WrongAnswer:
      return {
        statusText: 'Wrong Answer',
        icon: <CancelIcon fontSize="small" />,
        color: 'error',
      }
    case TestcaseStatus.CompilationError:
      return {
        statusText: 'Compilation Error',
        icon: <BuildIcon fontSize="small" />,
        color: 'error',
      }
    case TestcaseStatus.Skipped:
      return {
        statusText: 'Skipped',
        icon: <SkipNextIcon fontSize="small" />,
        color: 'default',
      }
    default:
      return {
        statusText: 'Unknown',
        icon: <ErrorIcon fontSize="small" />,
        color: 'default',
      }
  }
}

// Helper function to get status colors
function getStatusColor(status: TestcaseStatus) {
  if (status === TestcaseStatus.Passed) {
    return { border: 'success.main', bg: 'success.50' }
  }
  if (status === TestcaseStatus.TimeLimitExceeded || status === TestcaseStatus.MemoryLimitExceeded) {
    return { border: 'warning.main', bg: 'warning.50' }
  }
  return { border: 'error.main', bg: 'error.50' }
}

// Calculate statistics
function calculateStats(testCases: TestCaseInfo[]) {
  const stats = {
    passed: 0,
    timeLimitExceeded: 0,
    memoryLimitExceeded: 0,
    runtimeError: 0,
    wrongAnswer: 0,
    compilationError: 0,
    internalError: 0,
    skipped: 0,
  }

  testCases.forEach((tc) => {
    switch (tc.status) {
      case TestcaseStatus.Passed:
        stats.passed++
        break
      case TestcaseStatus.TimeLimitExceeded:
        stats.timeLimitExceeded++
        break
      case TestcaseStatus.MemoryLimitExceeded:
        stats.memoryLimitExceeded++
        break
      case TestcaseStatus.RuntimeError:
        stats.runtimeError++
        break
      case TestcaseStatus.WrongAnswer:
        stats.wrongAnswer++
        break
      case TestcaseStatus.CompilationError:
        stats.compilationError++
        break
      case TestcaseStatus.InternalError:
        stats.internalError++
        break
      case TestcaseStatus.Skipped:
        stats.skipped++
        break
    }
  })

  return stats
}

export function TestCaseResultDialog({
  open,
  onClose,
  compareResult,
  passedTestcase,
  totalTestcase,
  submissionId,
}: TestCaseResultDialogProps) {
  const testCases = React.useMemo(() => parseCompareResult(compareResult), [compareResult])
  const stats = React.useMemo(() => calculateStats(testCases), [testCases])

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box>
            <Typography variant="h6" sx={{ fontWeight: 600 }}>
              Chi tiết Test Cases
            </Typography>
            <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'monospace' }}>
              Submission: {submissionId.slice(0, 8)}...
            </Typography>
          </Box>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        {/* Summary Statistics */}
        <Paper variant="outlined" sx={{ p: 2, mb: 3, bgcolor: '#f5f5f7' }}>
          <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2 }}>
            Tổng quan
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
            <Chip
              icon={<CheckCircleIcon />}
              label={`Passed: ${passedTestcase}/${totalTestcase}`}
              color={passedTestcase === totalTestcase ? 'success' : 'default'}
              sx={{ fontWeight: 600 }}
            />
            {stats.wrongAnswer > 0 && (
              <Chip
                icon={<CancelIcon />}
                label={`Wrong Answer: ${stats.wrongAnswer}`}
                color="error"
                size="small"
              />
            )}
            {stats.timeLimitExceeded > 0 && (
              <Chip
                icon={<AccessTimeIcon />}
                label={`TLE: ${stats.timeLimitExceeded}`}
                color="warning"
                size="small"
              />
            )}
            {stats.memoryLimitExceeded > 0 && (
              <Chip
                icon={<MemoryIcon />}
                label={`MLE: ${stats.memoryLimitExceeded}`}
                color="warning"
                size="small"
              />
            )}
            {stats.runtimeError > 0 && (
              <Chip
                icon={<ErrorIcon />}
                label={`Runtime Error: ${stats.runtimeError}`}
                color="error"
                size="small"
              />
            )}
            {stats.compilationError > 0 && (
              <Chip
                icon={<BuildIcon />}
                label={`Compilation Error: ${stats.compilationError}`}
                color="error"
                size="small"
              />
            )}
            {stats.internalError > 0 && (
              <Chip
                icon={<WarningIcon />}
                label={`Internal Error: ${stats.internalError}`}
                color="error"
                size="small"
              />
            )}
            {stats.skipped > 0 && (
              <Chip
                icon={<SkipNextIcon />}
                label={`Skipped: ${stats.skipped}`}
                color="default"
                size="small"
              />
            )}
          </Box>
        </Paper>

        {/* Test Cases Grid */}
        <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 2 }}>
          Chi tiết từng test case
        </Typography>
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: {
              xs: '1fr',
              sm: 'repeat(2, 1fr)',
              md: 'repeat(3, 1fr)',
            },
            gap: 1.5,
          }}
        >
          {testCases.map((tc) => {
            const colors = getStatusColor(tc.status)
            
            return (
              <Box key={tc.index}>
                <Paper
                  variant="outlined"
                  sx={{
                    p: 1.5,
                    borderLeft: 4,
                    borderLeftColor: colors.border,
                    bgcolor: colors.bg,
                    transition: 'all 0.2s',
                    '&:hover': {
                      boxShadow: 2,
                      transform: 'translateY(-2px)',
                    },
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
                    <Typography variant="body2" sx={{ fontWeight: 600, fontFamily: 'monospace' }}>
                      Test #{tc.index}
                    </Typography>
                    {tc.icon}
                  </Box>
                  <Chip
                    label={tc.statusText}
                    size="small"
                    color={tc.color}
                    sx={{
                      width: '100%',
                      fontSize: '0.7rem',
                      height: 'auto',
                      py: 0.5,
                      '& .MuiChip-label': {
                        whiteSpace: 'normal',
                        textAlign: 'center',
                      },
                    }}
                  />
                </Paper>
              </Box>
            )
          })}
        </Box>

        {/* Empty State */}
        {testCases.length === 0 && (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <Typography variant="body2" color="text.secondary">
              Không có dữ liệu test case
            </Typography>
          </Box>
        )}
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} variant="contained">
          Đóng
        </Button>
      </DialogActions>
    </Dialog>
  )
}
