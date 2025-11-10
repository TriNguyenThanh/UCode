import React from 'react'
import {
  Box,
  Typography,
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
  Button,
  Divider,
  CircularProgress,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
} from '@mui/material'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'
import VisibilityIcon from '@mui/icons-material/Visibility'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import MemoryIcon from '@mui/icons-material/Memory'
import CodeIcon from '@mui/icons-material/Code'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism'
import type { Submission, TestcaseStatus } from '~/types'
import { getSubmission } from '~/services/submissionService'
import { TestCaseResultDialog } from './TestCaseResultDialog'

interface SubmissionHistoryProps {
  submissions: Submission[]
}

interface SubmissionDetail extends Submission {
  sourceCode?: string
}

interface TestCaseResult {
  index: number
  status: TestcaseStatus
  input?: string
  expectedOutput?: string
  actualOutput?: string
  timeCost?: number
  memoryCost?: number
}

// Parse compareResult JSON to get test case details
function parseCompareResult(compareResult?: string): TestCaseResult[] {
  if (!compareResult) return []
  
  try {
    const result = JSON.parse(compareResult)
    
    // Check if result is an array of test cases
    if (Array.isArray(result)) {
      return result.map((tc, index) => ({
        index: index + 1,
        status: tc.status || tc.Status || 'Skipped',
        input: tc.input || tc.Input,
        expectedOutput: tc.expectedOutput || tc.ExpectedOutput,
        actualOutput: tc.actualOutput || tc.ActualOutput,
        timeCost: tc.timeCost || tc.TimeCost,
        memoryCost: tc.memoryCost || tc.MemoryCost,
      }))
    }
    
    // If result has testCases property
    if (result.testCases && Array.isArray(result.testCases)) {
      return result.testCases.map((tc: any, index: number) => ({
        index: index + 1,
        status: tc.status || tc.Status || 'Skipped',
        input: tc.input || tc.Input,
        expectedOutput: tc.expectedOutput || tc.ExpectedOutput,
        actualOutput: tc.actualOutput || tc.ActualOutput,
        timeCost: tc.timeCost || tc.TimeCost,
        memoryCost: tc.memoryCost || tc.MemoryCost,
      }))
    }
    
    return []
  } catch (error) {
    console.error('Failed to parse compareResult:', error)
    return []
  }
}

// Get language for syntax highlighting
function getLanguageForHighlight(language: string): string {
  const langMap: Record<string, string> = {
    'c': 'c',
    'cpp': 'cpp',
    'c++': 'cpp',
    'java': 'java',
    'python': 'python',
    'python3': 'python',
    'javascript': 'javascript',
    'typescript': 'typescript',
    'csharp': 'csharp',
    'c#': 'csharp',
    'go': 'go',
    'rust': 'rust',
    'php': 'php',
    'ruby': 'ruby',
    'swift': 'swift',
    'kotlin': 'kotlin',
  }
  
  return langMap[language.toLowerCase()] || 'text'
}

// Get status color
function getStatusColor(status: TestcaseStatus): 'success' | 'error' | 'warning' | 'default' {
  switch (status) {
    case 'Passed':
      return 'success'
    case 'WrongAnswer':
    case 'RuntimeError':
    case 'CompilationError':
      return 'error'
    case 'TimeLimitExceeded':
    case 'MemoryLimitExceeded':
      return 'warning'
    default:
      return 'default'
  }
}

// Get status label
function getStatusLabel(status: TestcaseStatus): string {
  const labels: Record<TestcaseStatus, string> = {
    'Passed': '✓ Passed',
    'WrongAnswer': '✗ Wrong Answer',
    'TimeLimitExceeded': '⏱ Time Limit',
    'MemoryLimitExceeded': '💾 Memory Limit',
    'RuntimeError': '⚠ Runtime Error',
    'InternalError': '❌ Internal Error',
    'CompilationError': '🔧 Compilation Error',
    'Skipped': '⊘ Skipped',
  }
  
  return labels[status] || status
}

export function SubmissionHistory({ submissions }: SubmissionHistoryProps) {
  const [detailDialogOpen, setDetailDialogOpen] = React.useState(false)
  const [selectedSubmission, setSelectedSubmission] = React.useState<SubmissionDetail | null>(null)
  const [loadingDetail, setLoadingDetail] = React.useState(false)
  const [testCaseDialogOpen, setTestCaseDialogOpen] = React.useState(false)
  const [selectedSubmissionForTestCases, setSelectedSubmissionForTestCases] = React.useState<Submission | null>(null)

  const handleViewDetail = async (submission: Submission) => {
    setDetailDialogOpen(true)
    setSelectedSubmission(submission)
    setLoadingDetail(true)

    try {
      // If sourceCode already exists in submission, use it directly
      if (submission.sourceCodeRef) {
        setSelectedSubmission({
          ...submission,
          sourceCode: submission.sourceCodeRef,
        })
        setLoadingDetail(false)
        return
      }

      // Otherwise, fetch full submission details from API
      const fullSubmission = await getSubmission(submission.submissionId)
      
      setSelectedSubmission({
        ...submission,
        ...fullSubmission,
        sourceCode: fullSubmission.sourceCodeRef || '// Source code not available',
      })
    } catch (error) {
      console.error('Failed to fetch source code:', error)
      setSelectedSubmission({
        ...submission,
        sourceCode: '// Failed to load source code',
      })
    } finally {
      setLoadingDetail(false)
    }
  }

  const handleCloseDetail = () => {
    setDetailDialogOpen(false)
    setSelectedSubmission(null)
  }

  const handleOpenTestCaseDialog = (submission: Submission) => {
    setSelectedSubmissionForTestCases(submission)
    setTestCaseDialogOpen(true)
  }

  const handleCloseTestCaseDialog = () => {
    setTestCaseDialogOpen(false)
    setSelectedSubmissionForTestCases(null)
  }

  const testCaseResults = React.useMemo(() => {
    if (!selectedSubmission?.compareResult) return []
    return parseCompareResult(selectedSubmission.compareResult)
  }, [selectedSubmission?.compareResult])

  return (
    <>
      <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
        Lịch sử nộp bài
      </Typography>
      
      {submissions.length > 0 ? (
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Thời gian</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Ngôn ngữ</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                <TableCell sx={{ fontWeight: 600 }} align="center">Điểm</TableCell>
                <TableCell sx={{ fontWeight: 600 }} align="right">Time (ms)</TableCell>
                <TableCell sx={{ fontWeight: 600 }} align="right">Memory (KB)</TableCell>
                <TableCell sx={{ fontWeight: 600 }} align="center">Chi tiết</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {submissions.map((sub) => (
                <TableRow key={sub.submissionId} hover>
                  <TableCell>
                    {new Date(sub.submittedAt).toLocaleString('vi-VN')}
                  </TableCell>
                  <TableCell>
                    <Chip label={sub.language} size="small" variant="outlined" />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={sub.status}
                      size="small"
                      color={sub.status === 'Passed' ? 'success' : 'error'}
                      icon={sub.status === 'Passed' ? <CheckCircleIcon /> : <CancelIcon />}
                    />
                  </TableCell>
                  <TableCell align="center">
                    <Typography
                      variant="body2"
                      onClick={() => handleOpenTestCaseDialog(sub)}
                      sx={{
                        fontWeight: 600,
                        color: sub.passedTestcase === sub.totalTestcase ? 'success.main' : 'error.main',
                        cursor: 'pointer',
                        '&:hover': {
                          textDecoration: 'underline',
                          opacity: 0.8,
                        },
                      }}
                    >
                      {sub.passedTestcase}/{sub.totalTestcase}
                    </Typography>
                  </TableCell>
                  <TableCell align="right">{sub.totalTime}</TableCell>
                  <TableCell align="right">{sub.totalMemory}</TableCell>
                  <TableCell align="center">
                    <IconButton
                      size="small"
                      color="primary"
                      onClick={() => handleViewDetail(sub)}
                    >
                      <VisibilityIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        <Alert severity="info">Chưa có lần nộp bài nào.</Alert>
      )}

      {/* Detail Dialog */}
      <Dialog
        open={detailDialogOpen}
        onClose={handleCloseDetail}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6" sx={{ fontWeight: 600 }}>
              Chi tiết bài nộp
            </Typography>
            {selectedSubmission && (
              <Chip
                label={selectedSubmission.status}
                color={selectedSubmission.status === 'Passed' ? 'success' : 'error'}
                icon={selectedSubmission.status === 'Passed' ? <CheckCircleIcon /> : <CancelIcon />}
              />
            )}
          </Box>
        </DialogTitle>
        
        <DialogContent>
          {selectedSubmission && (
            <Box>
              {/* Submission Info */}
              <Paper variant="outlined" sx={{ p: 2, mb: 3, bgcolor: '#f5f5f7' }}>
                <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 2 }}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      Submission ID
                    </Typography>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                      {selectedSubmission.submissionId}
                    </Typography>
                  </Box>
                  
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      Thời gian nộp
                    </Typography>
                    <Typography variant="body2">
                      {new Date(selectedSubmission.submittedAt).toLocaleString('vi-VN')}
                    </Typography>
                  </Box>
                  
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      Ngôn ngữ
                    </Typography>
                    <Typography variant="body2">
                      <Chip label={selectedSubmission.language} size="small" />
                    </Typography>
                  </Box>
                  
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      Điểm
                    </Typography>
                    <Typography 
                      variant="body2" 
                      sx={{ 
                        fontWeight: 600,
                        color: selectedSubmission.passedTestcase === selectedSubmission.totalTestcase 
                          ? 'success.main' 
                          : 'error.main'
                      }}
                    >
                      {selectedSubmission.passedTestcase}/{selectedSubmission.totalTestcase} test cases
                    </Typography>
                  </Box>
                  
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      <AccessTimeIcon sx={{ fontSize: 14, verticalAlign: 'middle', mr: 0.5 }} />
                      Thời gian
                    </Typography>
                    <Typography variant="body2">{selectedSubmission.totalTime} ms</Typography>
                  </Box>
                  
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      <MemoryIcon sx={{ fontSize: 14, verticalAlign: 'middle', mr: 0.5 }} />
                      Bộ nhớ
                    </Typography>
                    <Typography variant="body2">{selectedSubmission.totalMemory} KB</Typography>
                  </Box>
                </Box>

                {selectedSubmission.errorMessage && (
                  <Alert severity="error" sx={{ mt: 2 }}>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      Error:
                    </Typography>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace', whiteSpace: 'pre-wrap' }}>
                      {selectedSubmission.errorMessage}
                    </Typography>
                  </Alert>
                )}
              </Paper>

              {/* Source Code */}
              <Box sx={{ mb: 3 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1, display: 'flex', alignItems: 'center' }}>
                  <CodeIcon sx={{ mr: 1 }} />
                  Source Code
                </Typography>
                
                {loadingDetail ? (
                  <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
                    <CircularProgress />
                  </Box>
                ) : (
                  <Paper variant="outlined" sx={{ overflow: 'hidden' }}>
                    <SyntaxHighlighter
                      language={getLanguageForHighlight(selectedSubmission.language)}
                      style={vscDarkPlus as any}
                      showLineNumbers
                      customStyle={{
                        margin: 0,
                        maxHeight: '400px',
                        fontSize: '0.875rem',
                      }}
                    >
                      {selectedSubmission.sourceCode || '// Loading...'}
                    </SyntaxHighlighter>
                  </Paper>
                )}
              </Box>

              {/* Test Cases Results */}
              {testCaseResults.length > 0 && (
                <Box>
                  <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
                    Kết quả Test Cases
                  </Typography>
                  
                  {testCaseResults.map((tc) => (
                    <Accordion key={tc.index} variant="outlined" sx={{ mb: 1 }}>
                      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, width: '100%' }}>
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>
                            Test Case #{tc.index}
                          </Typography>
                          <Chip
                            label={getStatusLabel(tc.status)}
                            size="small"
                            color={getStatusColor(tc.status)}
                          />
                          {tc.timeCost !== undefined && (
                            <Typography variant="caption" color="text.secondary">
                              {tc.timeCost}ms
                            </Typography>
                          )}
                          {tc.memoryCost !== undefined && (
                            <Typography variant="caption" color="text.secondary">
                              {tc.memoryCost}KB
                            </Typography>
                          )}
                        </Box>
                      </AccordionSummary>
                      <AccordionDetails>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                          {tc.input && (
                            <Box>
                              <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
                                Input:
                              </Typography>
                              <Paper variant="outlined" sx={{ p: 1, bgcolor: '#f5f5f7', mt: 0.5 }}>
                                <Typography
                                  variant="body2"
                                  component="pre"
                                  sx={{
                                    fontFamily: 'monospace',
                                    fontSize: '0.75rem',
                                    whiteSpace: 'pre-wrap',
                                    wordBreak: 'break-all',
                                    m: 0,
                                  }}
                                >
                                  {tc.input}
                                </Typography>
                              </Paper>
                            </Box>
                          )}
                          
                          {tc.expectedOutput && (
                            <Box>
                              <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
                                Expected Output:
                              </Typography>
                              <Paper variant="outlined" sx={{ p: 1, bgcolor: '#f5f5f7', mt: 0.5 }}>
                                <Typography
                                  variant="body2"
                                  component="pre"
                                  sx={{
                                    fontFamily: 'monospace',
                                    fontSize: '0.75rem',
                                    whiteSpace: 'pre-wrap',
                                    wordBreak: 'break-all',
                                    m: 0,
                                  }}
                                >
                                  {tc.expectedOutput}
                                </Typography>
                              </Paper>
                            </Box>
                          )}
                          
                          {tc.actualOutput && (
                            <Box>
                              <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
                                Your Output:
                              </Typography>
                              <Paper
                                variant="outlined"
                                sx={{
                                  p: 1,
                                  bgcolor: tc.status === 'Passed' ? '#e8f5e9' : '#ffebee',
                                  mt: 0.5,
                                }}
                              >
                                <Typography
                                  variant="body2"
                                  component="pre"
                                  sx={{
                                    fontFamily: 'monospace',
                                    fontSize: '0.75rem',
                                    whiteSpace: 'pre-wrap',
                                    wordBreak: 'break-all',
                                    m: 0,
                                  }}
                                >
                                  {tc.actualOutput}
                                </Typography>
                              </Paper>
                            </Box>
                          )}
                        </Box>
                      </AccordionDetails>
                    </Accordion>
                  ))}
                </Box>
              )}
            </Box>
          )}
        </DialogContent>
        
        <DialogActions>
          <Button onClick={handleCloseDetail}>Đóng</Button>
        </DialogActions>
      </Dialog>

      {/* Test Case Result Dialog */}
      {selectedSubmissionForTestCases && (
        <TestCaseResultDialog
          open={testCaseDialogOpen}
          onClose={handleCloseTestCaseDialog}
          compareResult={selectedSubmissionForTestCases.compareResult || ''}
          passedTestcase={selectedSubmissionForTestCases.passedTestcase}
          totalTestcase={selectedSubmissionForTestCases.totalTestcase}
          submissionId={selectedSubmissionForTestCases.submissionId}
        />
      )}
    </>
  )
}

