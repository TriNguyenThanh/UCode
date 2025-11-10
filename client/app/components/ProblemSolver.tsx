import * as React from 'react'
import { Link } from 'react-router'
import {
  Box,
  Typography,
  Paper,
  Chip,
  Button,
  IconButton,
  Tabs,
  Tab,
  Select,
  MenuItem,
  FormControl,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import SendIcon from '@mui/icons-material/Send'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import MemoryIcon from '@mui/icons-material/Memory'
import RestartAltIcon from '@mui/icons-material/RestartAlt'
import { CodeEditor } from './CodeEditor'
import { SubmissionHistory } from './SubmissionHistory'
import { Loading } from './Loading'
import { runCode, submitCode, getSubmissionsByProblem, getSubmission } from '~/services/submissionService'
import type { Problem, Submission } from '~/types'

interface ProblemSolverProps {
  problem: Problem
  initialSubmissions?: Submission[]
  backUrl: string
  assignmentId?: string | null
}

interface TabPanelProps {
  children?: React.ReactNode
  index: number
  value: number
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props
  return (
    <div role='tabpanel' hidden={value !== index} {...other}>
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  )
}

function getCodeTemplate(languageCode: string, problemLanguages?: Problem['problemLanguages']): string {
  const problemLanguage = problemLanguages?.find((pl) => pl.languageCode === languageCode)

  if (problemLanguage) {
    const parts = []
    if (problemLanguage.head) parts.push(problemLanguage.head)
    if (problemLanguage.body) parts.push(problemLanguage.body)
    if (problemLanguage.tail) parts.push(problemLanguage.tail)

    if (parts.length > 0) {
      return parts.join('\n\n')
    }
  }
  return '// Your code here'
}

export function ProblemSolver({ problem, initialSubmissions = [], backUrl, assignmentId = null }: ProblemSolverProps) {
  const [tabValue, setTabValue] = React.useState(0)
  
  // Panel resizing
  const [leftPanelWidth, setLeftPanelWidth] = React.useState(50)
  const [isDragging, setIsDragging] = React.useState(false)
  const containerRef = React.useRef<HTMLDivElement>(null)

  const availableLanguages = problem.problemLanguages || []
  
  const defaultLanguage = availableLanguages.length > 0 ? availableLanguages[0] : null
  const [selectedLanguage, setSelectedLanguage] = React.useState<typeof defaultLanguage>(defaultLanguage)
  const [code, setCode] = React.useState('')
  const [output, setOutput] = React.useState('')
  const [isRunning, setIsRunning] = React.useState(false)
  const [isSubmitting, setIsSubmitting] = React.useState(false)
  const [submissions, setSubmissions] = React.useState<Submission[]>(initialSubmissions)
  const [hasRunSuccessfully, setHasRunSuccessfully] = React.useState(false)
  const [lastRunCode, setLastRunCode] = React.useState('')
  const [isPolling, setIsPolling] = React.useState(false)

  // Handle panel resizing
  const handleMouseDown = (e: React.MouseEvent) => {
    e.preventDefault()
    setIsDragging(true)
  }

  React.useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (!isDragging || !containerRef.current) return
      
      const containerRect = containerRef.current.getBoundingClientRect()
      const newLeftWidth = ((e.clientX - containerRect.left) / containerRect.width) * 100
      
      const clampedWidth = Math.min(Math.max(newLeftWidth, 20), 80)
      setLeftPanelWidth(clampedWidth)
    }

    const handleMouseUp = () => {
      setIsDragging(false)
    }

    if (isDragging) {
      document.addEventListener('mousemove', handleMouseMove)
      document.addEventListener('mouseup', handleMouseUp)
    }

    return () => {
      document.removeEventListener('mousemove', handleMouseMove)
      document.removeEventListener('mouseup', handleMouseUp)
    }
  }, [isDragging])

  // Initialize code template
  React.useEffect(() => {
    if (selectedLanguage && selectedLanguage.languageCode) {
      setCode(getCodeTemplate(selectedLanguage.languageCode, problem.problemLanguages))
    }
  }, [selectedLanguage, problem])

  // Handle language change
  const handleLanguageChange = (languageId: string) => {
    const lang = availableLanguages.find((l) => l.languageId === languageId)
    if (lang && lang.languageCode) {
      setSelectedLanguage(lang)
      setCode(getCodeTemplate(lang.languageCode, problem.problemLanguages))
      setOutput('')
      setHasRunSuccessfully(false) 
      setLastRunCode('')
    }
  }

  // Handle reset code
  const handleResetCode = () => {
    if (selectedLanguage && selectedLanguage.languageCode) {
      setCode(getCodeTemplate(selectedLanguage.languageCode, problem.problemLanguages))
      setOutput('')
      setHasRunSuccessfully(false) 
      setLastRunCode('')
    }
  }

  // Refresh submissions
  const refreshSubmissions = async () => {
    try {
      const newSubmissions = await getSubmissionsByProblem(problem.problemId, 1, 10)
      setSubmissions(newSubmissions)
      setTabValue(2)
    } catch (error) {
      console.error('Failed to refresh submissions:', error)
    }
  }

  // Get status text
  const getStatusText = (statusCode: string): { text: string; emoji: string } => {
    switch (statusCode) {
      case '0': return { text: 'Passed', emoji: '✅' }
      case '1': return { text: 'Time Limit Exceeded', emoji: '⏰' }
      case '2': return { text: 'Memory Limit Exceeded', emoji: '💾' }
      case '3': return { text: 'Runtime Error', emoji: '💥' }
      case '4': return { text: 'Internal Error', emoji: '⚠️' }
      case '5': return { text: 'Wrong Answer', emoji: '❌' }
      case '6': return { text: 'Compilation Error', emoji: '🔧' }
      case '7': return { text: 'Skipped', emoji: '⏭️' }
      default: return { text: 'Unknown', emoji: '❓' }
    }
  }

  // Parse test case results
  const parseTestCaseResults = (compareResult: string): string => {
    if (!compareResult) return ''
    
    let testCaseDetails = '\n\n📋 Chi tiết từng test case:\n'
    testCaseDetails += '─'.repeat(40) + '\n'
    
    for (let i = 0; i < compareResult.length; i++) {
      const statusCode = compareResult[i]
      const { text, emoji } = getStatusText(statusCode)
      testCaseDetails += `Test case #${i + 1}: ${emoji} ${text}\n`
    }
    
    return testCaseDetails
  }

  // Polling for submission result
  const pollSubmissionResult = async (submissionId: string, sourceCode: string, isSubmit: boolean = false) => {
    const maxAttempts = 30
    let attempts = 0
    
    setIsPolling(true)
    
    const poll = async (): Promise<void> => {
      try {
        const submission = await getSubmission(submissionId)
        
        const processingStatuses: string[] = ['Pending', 'Running']
        if (processingStatuses.includes(submission.status)) {
          attempts++
          
          if (attempts >= maxAttempts) {
            setOutput(prev => prev + '\n\n⏱️ Timeout: Quá trình chấm điểm mất nhiều thời gian. Vui lòng kiểm tra lại sau.')
            return
          }
          
          setOutput(prev => {
            const lines = prev.split('\n')
            return lines.slice(0, -1).join('\n') + `\nĐang xử lý... (${attempts}s)`
          })
          
          setTimeout(() => poll(), 2000)
        } else {
          let resultText = isSubmit ? '🎉 Kết quả nộp bài:\n\n' : '✅ Kết quả chạy thử:\n\n'
          resultText += `Submission ID: ${submission.submissionId}\n`
          resultText += `Status: ${submission.status}\n`
          resultText += `Thời gian: ${submission.totalTime}ms\n`
          resultText += `Bộ nhớ: ${submission.totalMemory}KB\n`
          
          if (submission.status === 'Passed') {
            resultText += `\n✅ ${submission.passedTestcase}/${submission.totalTestcase} test cases passed`
            if (!isSubmit) {
              setHasRunSuccessfully(true)
              setLastRunCode(sourceCode)
            }
          } else {
            resultText += `\n❌ ${submission.passedTestcase}/${submission.totalTestcase} test cases passed`
            if (submission.errorMessage) {
              resultText += `\n\nLỗi: ${submission.errorMessage}`
            }
            if (!isSubmit) {
              setHasRunSuccessfully(false)
              setLastRunCode('')
            }
          }
          
          if (submission.compareResult) {
            resultText += parseTestCaseResults(submission.compareResult)
          }
          
          setOutput(resultText)
          
          if (isSubmit) {
            await refreshSubmissions()
          }
          
          setIsPolling(false)
        }
      } catch (error: any) {
        setOutput(prev => prev + `\n\n❌ Lỗi khi lấy kết quả: ${error.message}`)
        setIsPolling(false)
      }
    }
    
    await poll()
  }

  // Handle run code
  const handleRunCode = async () => {
    if (!selectedLanguage) {
      setOutput('❌ Vui lòng chọn ngôn ngữ lập trình')
      return
    }

    if (!code.trim()) {
      setOutput('❌ Vui lòng nhập code')
      return
    }

    if (code !== lastRunCode) {
      setHasRunSuccessfully(false)
    }

    setIsRunning(true)
    setOutput('⏳ Đang biên dịch và chạy code...\n')

    try {
      const result = await runCode({
        problemId: problem.problemId,
        language: selectedLanguage.languageCode || 'cpp',
        sourceCode: code,
        assignmentId: assignmentId
      })

      setOutput(`✅ Đã gửi code để chạy thử!\n\nSubmission ID: ${result.submissionId}\nStatus: ${result.status}\n\nĐang xử lý... (0s)`)
      
      await pollSubmissionResult(result.submissionId, code, false)
      
    } catch (error: any) {
      setOutput(`❌ Lỗi: ${error.message || 'Không thể chạy code'}`)
      setHasRunSuccessfully(false)
      setLastRunCode('')
    } finally {
      setIsRunning(false)
    }
  }

  // Handle submit code
  const handleSubmitCode = async () => {
    if (!selectedLanguage) {
      setOutput('❌ Vui lòng chọn ngôn ngữ lập trình')
      return
    }

    if (!code.trim()) {
      setOutput('❌ Vui lòng nhập code')
      return
    }

    if (!hasRunSuccessfully) {
      setOutput('❌ Vui lòng chạy thử code thành công trước khi nộp bài!')
      return
    }

    if (code !== lastRunCode) {
      setOutput('⚠️ Code đã thay đổi sau lần chạy thử cuối!\n\nVui lòng chạy thử lại trước khi nộp bài.')
      return
    }

    setIsSubmitting(true)
    setOutput('📤 Đang nộp bài...\n')

    try {
      const result = await submitCode({
        problemId: problem.problemId,
        language: selectedLanguage.languageCode || 'cpp',
        sourceCode: code,
        assignmentId: assignmentId
      })

      setOutput(`🎉 Đã nộp bài thành công!\n\nSubmission ID: ${result.submissionId}\nStatus: ${result.status}\nThời gian nộp: ${new Date(result.submittedAt).toLocaleString('vi-VN')}\n\nĐang chấm điểm... (0s)`)
      
      await pollSubmissionResult(result.submissionId, code, true)
      
    } catch (error: any) {
      setOutput(`❌ Lỗi: ${error.message || 'Không thể nộp bài'}`)
    } finally {
      setIsSubmitting(false)
    }
  }

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'EASY':
        return 'success'
      case 'MEDIUM':
        return 'warning'
      case 'HARD':
        return 'error'
      default:
        return 'default'
    }
  }

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue)
  }

  return (
    <Box sx={{ height: '100vh', display: 'flex', flexDirection: 'column', bgcolor: 'grey.50' }}>
      {/* Header */}
      <Paper
        elevation={0}
        sx={{
          borderBottom: '2px solid',
          borderColor: 'primary.main',
          bgcolor: 'secondary.main',
          color: 'white'
        }}
      >
        <Box sx={{ px: 3, py: 2, display: 'flex', alignItems: 'center', gap: 2 }}>
          <IconButton component={Link} to={backUrl} sx={{ color: 'primary.main' }}>
            <ArrowBackIcon />
          </IconButton>
          <Typography variant='h6' sx={{ fontWeight: 600, flexGrow: 1, color: 'primary.main' }}>
            {problem.title}
          </Typography>
          <Chip label={problem.difficulty} size='small' color={getDifficultyColor(problem.difficulty) as any} />
        </Box>
      </Paper>

      {/* Main Content */}
      <Box 
        ref={containerRef}
        sx={{ 
          display: 'flex', 
          flexGrow: 1, 
          overflow: 'hidden',
          cursor: isDragging ? 'col-resize' : 'default',
          userSelect: isDragging ? 'none' : 'auto'
        }}
      >
        {/* Left Panel - Problem Description */}
        <Box
          sx={{
            width: `${leftPanelWidth}%`,
            borderRight: '1px solid',
            borderColor: 'divider',
            display: 'flex',
            flexDirection: 'column',
            bgcolor: 'white',
            minWidth: '300px'
          }}
        >
          <Tabs value={tabValue} onChange={handleTabChange} sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tab label='Đề bài' />
            <Tab label='Hướng dẫn' />
            <Tab label='Nộp bài' />
          </Tabs>

          <Box sx={{ flexGrow: 1, overflow: 'auto' }}>
            <TabPanel value={tabValue} index={0}>
              <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                Mô tả
              </Typography>
              <Typography variant='body1' sx={{ mb: 3, whiteSpace: 'pre-line' }}>
                {problem.statement || 'Chưa có đề bài chi tiết'}
              </Typography>

              {(problem.inputFormat || problem.outputFormat) && (
                <Box sx={{ mb: 3 }}>
                  {problem.inputFormat && (
                    <>
                      <Typography variant='h6' sx={{ fontWeight: 600, mb: 1 }}>
                        Định dạng Input
                      </Typography>
                      <Typography variant='body2' sx={{ mb: 2, whiteSpace: 'pre-line' }}>
                        {problem.inputFormat}
                      </Typography>
                    </>
                  )}
                  {problem.outputFormat && (
                    <>
                      <Typography variant='h6' sx={{ fontWeight: 600, mb: 1 }}>
                        Định dạng Output
                      </Typography>
                      <Typography variant='body2' sx={{ mb: 2, whiteSpace: 'pre-line' }}>
                        {problem.outputFormat}
                      </Typography>
                    </>
                  )}
                </Box>
              )}

              <Box sx={{ mb: 3 }}>
                <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                  Ràng buộc
                </Typography>
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mb: 2 }}>
                  <Chip
                    icon={<AccessTimeIcon />}
                    label={`Time Limit: ${problem.timeLimitMs}ms`}
                    variant='outlined'
                    color='primary'
                  />
                  <Chip
                    icon={<MemoryIcon />}
                    label={`Memory: ${problem.memoryLimitKb}KB`}
                    variant='outlined'
                    color='primary'
                  />
                </Box>
                {problem.constraints && (
                  <Typography variant='body2' sx={{ whiteSpace: 'pre-line' }}>
                    {problem.constraints}
                  </Typography>
                )}
              </Box>

              {problem.datasetSample && problem.datasetSample.testCases && problem.datasetSample.testCases.length > 0 && (
                <Box sx={{ mb: 3 }}>
                  <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                    Test case mẫu
                  </Typography>
                  <TableContainer component={Paper} variant='outlined'>
                    <Table size='small'>
                      <TableHead>
                        <TableRow>
                          <TableCell sx={{ fontWeight: 600 }}>Test case</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Input</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Output</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {problem.datasetSample.testCases.map((testCase: any, index: number) => (
                          <TableRow key={testCase.testCaseId || index}>
                            <TableCell>#{testCase.indexNo || index + 1}</TableCell>
                            <TableCell>
                              <Typography 
                                variant='body2' 
                                component='pre' 
                                sx={{ 
                                  fontFamily: 'monospace', 
                                  whiteSpace: 'pre-wrap',
                                  m: 0,
                                  p: 1,
                                  bgcolor: '#f5f5f5',
                                  borderRadius: 1
                                }}
                              >
                                {testCase.inputRef}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography 
                                variant='body2' 
                                component='pre' 
                                sx={{ 
                                  fontFamily: 'monospace', 
                                  whiteSpace: 'pre-wrap',
                                  m: 0,
                                  p: 1,
                                  bgcolor: '#f5f5f5',
                                  borderRadius: 1
                                }}
                              >
                                {testCase.outputRef}
                              </Typography>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </Box>
              )}

              {problem.tagNames && problem.tagNames.length > 0 && (
                <Box>
                  <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                    Tags
                  </Typography>
                  <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                    {problem.tagNames.map((tag) => (
                      <Chip key={tag} label={tag} size='small' variant='outlined' />
                    ))}
                  </Box>
                </Box>
              )}
            </TabPanel>

            <TabPanel value={tabValue} index={1}>
              <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                Hướng dẫn giải
              </Typography>
              {problem.solution ? (
                <Typography variant='body2' sx={{ whiteSpace: 'pre-line' }}>
                  {problem.solution}
                </Typography>
              ) : (
                <Typography variant='body2' color='text.secondary'>
                  Nội dung hướng dẫn sẽ được cập nhật sau...
                </Typography>
              )}
            </TabPanel>

            <TabPanel value={tabValue} index={2}>
              {submissions.length === 0 && tabValue === 2 ? (
                <Box sx={{ py: 8 }}>
                  <Loading message="Đang tải lịch sử nộp bài..." size="medium" />
                </Box>
              ) : (
                <SubmissionHistory submissions={submissions} />
              )}
            </TabPanel>
          </Box>
        </Box>

        {/* Resize Handle */}
        <Box
          sx={{
            width: '6px',
            cursor: 'col-resize',
            bgcolor: isDragging ? 'primary.main' : 'divider',
            '&:hover': {
              bgcolor: 'primary.main'
            },
            transition: 'background-color 0.2s ease',
            flexShrink: 0,
            position: 'relative'
          }}
          onMouseDown={handleMouseDown}
        >
          <Box
            sx={{
              position: 'absolute',
              top: '50%',
              left: '50%',
              transform: 'translate(-50%, -50%)',
              width: '2px',
              height: '20px',
              bgcolor: 'background.paper',
              borderRadius: '1px',
              opacity: 0.7
            }}
          />
        </Box>

        {/* Right Panel - Code Editor */}
        <Box sx={{ 
          width: `${100 - leftPanelWidth}%`, 
          display: 'flex', 
          flexDirection: 'column', 
          bgcolor: '#1e1e1e',
          minWidth: '300px'
        }}>
          {/* Editor Toolbar */}
          <Box
            sx={{
              px: 2,
              py: 1.5,
              display: 'flex',
              alignItems: 'center',
              gap: 2,
              bgcolor: '#2d2d2d',
              borderBottom: '1px solid #3d3d3d'
            }}
          >
            <FormControl size='small' sx={{ minWidth: 180 }}>
              <Select
                value={selectedLanguage?.languageId || ''}
                onChange={(e) => handleLanguageChange(e.target.value)}
                sx={{
                  color: 'white',
                  '.MuiOutlinedInput-notchedOutline': { borderColor: 'primary.main' },
                  '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'primary.main' },
                  '& .MuiSvgIcon-root': { color: 'primary.main' }
                }}
              >
                {availableLanguages.map((lang) => (
                  <MenuItem key={lang.languageId} value={lang.languageId}>
                    {lang.languageDisplayName}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <Button startIcon={<RestartAltIcon />} size='small' onClick={handleResetCode} sx={{ color: '#86868b' }}>
              Reset Code
            </Button>

            <Box sx={{ flexGrow: 1 }} />

            <Button
              startIcon={<PlayArrowIcon />}
              variant='outlined'
              sx={{ color: 'primary.main', borderColor: 'primary.main' }}
              onClick={handleRunCode}
              disabled={isRunning || isSubmitting}
            >
              {isRunning ? 'Đang chạy...' : 'Chạy thử'}
            </Button>
            <Button
              startIcon={<SendIcon />}
              variant='contained'
              sx={{ bgcolor: 'primary.main', color: 'secondary.main', fontWeight: 600 }}
              onClick={handleSubmitCode}
              disabled={isRunning || isSubmitting}
            >
              {isSubmitting ? 'Đang nộp...' : 'Nộp bài'}
            </Button>
          </Box>

          {/* Code Editor Area */}
          <Box sx={{ flexGrow: 1, overflow: 'hidden' }}>
            <CodeEditor
              value={code}
              onChange={(value) => setCode(value || '')}
              language={selectedLanguage?.languageCode || 'cpp'}
            />
          </Box>

          {/* Output Console */}
          <Paper
            sx={{
              height: '200px',
              borderTop: '2px solid',
              borderColor: 'primary.main',
              bgcolor: '#252526',
              borderRadius: 0,
              overflow: 'auto',
              position: 'relative'
            }}
          >
            {(isRunning || isSubmitting || isPolling) ? (
              <Box
                sx={{
                  position: 'absolute',
                  top: 0,
                  left: 0,
                  right: 0,
                  bottom: 0,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: 'rgba(37, 37, 38, 0.95)',
                  backdropFilter: 'blur(2px)',
                  zIndex: 1,
                }}
              >
                <Loading 
                  message={
                    isRunning ? 'Đang biên dịch và chạy code...' : 
                    isSubmitting ? 'Đang nộp bài...' : 
                    'Đang chấm điểm...'
                  }
                  size="medium"
                />
              </Box>
            ) : null}
            
            <Box sx={{ p: 2 }}>
              <Typography
                variant='body2'
                sx={{
                  fontFamily: 'monospace',
                  color: '#d4d4d4',
                  whiteSpace: 'pre-wrap',
                  wordBreak: 'break-word'
                }}
              >
                {output || 'Nhấn "Chạy thử" để kiểm tra code hoặc "Nộp bài" để submit...'}
              </Typography>
            </Box>
          </Paper>
        </Box>
      </Box>
    </Box>
  )
}
