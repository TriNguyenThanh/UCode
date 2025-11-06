import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/problem.$id'
import { auth } from '~/auth'
import {
  Box,
  Typography,
  Paper,
  Chip,
  Button,
  IconButton,
  Tabs,
  Tab,
  Divider,
  Select,
  MenuItem,
  FormControl,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import SendIcon from '@mui/icons-material/Send'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import MemoryIcon from '@mui/icons-material/Memory'
import RestartAltIcon from '@mui/icons-material/RestartAlt'
import { mockProblems } from '~/data/mock'
import { CodeEditor } from '~/components/CodeEditor'
import { getCodeTemplate } from '~/utils/codeTemplates'
import { submitCode, runCode, pollSubmissionResult } from '~/services/submissionService'
import type { SubmissionRequest } from '~/types'

export const meta: Route.MetaFunction = () => [
  { title: 'Giải bài tập | UCode' },
  { name: 'description', content: 'Coding interface để giải bài tập.' },
]

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')

  const problem = mockProblems.find((p) => p.id === params.id)
  if (!problem) throw new Response('Not Found', { status: 404 })

  return { user, problem }
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

export default function ProblemDetail() {
  const { problem } = useLoaderData<typeof clientLoader>()
  const [tabValue, setTabValue] = React.useState(0)
  const [language, setLanguage] = React.useState('cpp')
  const [code, setCode] = React.useState(getCodeTemplate('cpp'))
  const [output, setOutput] = React.useState('')
  const [isRunning, setIsRunning] = React.useState(false)

  // Handle language change
  const handleLanguageChange = (newLanguage: string) => {
    setLanguage(newLanguage)
    setCode(getCodeTemplate(newLanguage))
    setOutput('')
  }

  // Handle reset code
  const handleResetCode = () => {
    setCode(getCodeTemplate(language))
    setOutput('')
  }

  // Handle run code
  const handleRunCode = async () => {
    setIsRunning(true)
    setOutput('⏳ Compiling and running code...\n')

    try {
      const request: SubmissionRequest = {
        problemId: problem.id,
        sourceCode: code,
        language: language,
      }

      // Call run-code API (test with sample cases only)
      const response = await runCode(request)
      setOutput(`⏳ Running test cases... (Submission ID: ${response.submissionId})\n`)

      // Poll for result
      const result = await pollSubmissionResult(response.submissionId)

      // Format output
      let outputText = ''
      if (result.status === 'Passed') {
        outputText = `✅ All test cases passed!\n\n`
        outputText += `📊 Test cases passed: ${result.passedTestcase}/${result.totalTestcase}\n`
        outputText += `⏱️  Execution Time: ${(result.totalTime / 1000).toFixed(2)}s\n`
        outputText += `💾 Memory Used: ${(result.totalMemory / 1024).toFixed(2)} MB\n`
      } else if (result.status === 'Failed') {
        outputText = `❌ Some test cases failed\n\n`
        outputText += `📊 Test cases passed: ${result.passedTestcase}/${result.totalTestcase}\n`
        outputText += `⏱️  Execution Time: ${(result.totalTime / 1000).toFixed(2)}s\n`
        outputText += `💾 Memory Used: ${(result.totalMemory / 1024).toFixed(2)} MB\n`
        if (result.compareResult) {
          outputText += `\n${result.compareResult}`
        }
      } else if (result.status === 'CompilationError') {
        outputText = `❌ Compilation Error\n\n${result.errorMessage || 'Unknown compilation error'}`
      } else if (result.status === 'RuntimeError') {
        outputText = `❌ Runtime Error\n\n${result.errorMessage || 'Unknown runtime error'}`
      } else if (result.status === 'TimeLimitExceeded') {
        outputText = `⏰ Time Limit Exceeded\n\nYour code took too long to execute.`
      } else if (result.status === 'MemoryLimitExceeded') {
        outputText = `💾 Memory Limit Exceeded\n\nYour code used too much memory.`
      }

      setOutput(outputText)
    } catch (error) {
      setOutput(`❌ Error: ${error instanceof Error ? error.message : 'Unknown error occurred'}\n\nPlease try again.`)
    } finally {
      setIsRunning(false)
    }
  }

  // Handle submit code
  const handleSubmitCode = async () => {
    setIsRunning(true)
    setOutput('📤 Submitting code to judge...\n')

    try {
      const request: SubmissionRequest = {
        problemId: problem.id,
        sourceCode: code,
        language: language,
      }

      // Call submit-code API (official judging)
      const response = await submitCode(request)
      setOutput(`⏳ Judging in progress... (Submission ID: ${response.submissionId})\n`)

      // Poll for result
      const result = await pollSubmissionResult(response.submissionId, 120, 1000) // 2 minutes timeout

      // Format output
      let outputText = ''
      if (result.status === 'Passed') {
        outputText = `🎉 Submission successful!\n\n`
        outputText += `✅ Status: Accepted\n`
        outputText += `📊 Test cases passed: ${result.passedTestcase}/${result.totalTestcase}\n`
        outputText += `⏱️  Execution Time: ${(result.totalTime / 1000).toFixed(2)}s\n`
        outputText += `💾 Memory Used: ${(result.totalMemory / 1024).toFixed(2)} MB\n\n`
        outputText += `🏆 Congratulations! Your solution is correct!`
      } else if (result.status === 'Failed') {
        outputText = `❌ Submission failed\n\n`
        outputText += `📊 Test cases passed: ${result.passedTestcase}/${result.totalTestcase}\n`
        outputText += `⏱️  Execution Time: ${(result.totalTime / 1000).toFixed(2)}s\n`
        outputText += `💾 Memory Used: ${(result.totalMemory / 1024).toFixed(2)} MB\n`
        if (result.compareResult) {
          outputText += `\n${result.compareResult}`
        }
      } else if (result.status === 'CompilationError') {
        outputText = `❌ Compilation Error\n\n${result.errorMessage || 'Unknown compilation error'}`
      } else if (result.status === 'RuntimeError') {
        outputText = `❌ Runtime Error\n\n${result.errorMessage || 'Unknown runtime error'}`
      } else if (result.status === 'TimeLimitExceeded') {
        outputText = `⏰ Time Limit Exceeded\n\nYour code took too long to execute.`
      } else if (result.status === 'MemoryLimitExceeded') {
        outputText = `💾 Memory Limit Exceeded\n\nYour code used too much memory.`
      }

      setOutput(outputText)
    } catch (error) {
      setOutput(`❌ Error: ${error instanceof Error ? error.message : 'Unknown error occurred'}\n\nPlease try again.`)
    } finally {
      setIsRunning(false)
    }
  }

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy':
        return 'success'
      case 'Medium':
        return 'warning'
      case 'Hard':
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
          color: 'white',
        }}
      >
        <Box sx={{ px: 3, py: 2, display: 'flex', alignItems: 'center', gap: 2 }}>
          <IconButton component={Link} to='/home' sx={{ color: 'primary.main' }}>
            <ArrowBackIcon />
          </IconButton>
          <Typography variant='h6' sx={{ fontWeight: 600, flexGrow: 1, color: 'primary.main' }}>
            {problem.title}
          </Typography>
          <Chip label={problem.difficulty} size='small' color={getDifficultyColor(problem.difficulty) as any} />
        </Box>
      </Paper>

      {/* Main Content */}
      <Box sx={{ display: 'flex', flexGrow: 1, overflow: 'hidden' }}>
        {/* Left Panel - Problem Description */}
        <Box
          sx={{
            width: '40%',
            borderRight: '1px solid',
            borderColor: 'divider',
            display: 'flex',
            flexDirection: 'column',
            bgcolor: 'white',
          }}
        >
          <Tabs value={tabValue} onChange={handleTabChange} sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tab label='Đề bài' />
            <Tab label='Hướng dẫn' />
            <Tab label='Nộp bài' />
          </Tabs>

          <Box sx={{ flexGrow: 1, overflow: 'auto' }}>
            <TabPanel value={tabValue} index={0}>
              {/* Problem Description */}
              <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                Mô tả
              </Typography>
              <Typography variant='body1' sx={{ mb: 3, whiteSpace: 'pre-line' }}>
                {problem.statement}
              </Typography>

              {/* Constraints */}
              <Box sx={{ mb: 3 }}>
                <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                  Ràng buộc
                </Typography>
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                  <Chip
                    icon={<AccessTimeIcon />}
                    label={`Time Limit: ${problem.timeLimitMs}s`}
                    variant='outlined'
                    color='primary'
                  />
                  <Chip
                    icon={<MemoryIcon />}
                    label={`Memory: ${problem.memoryLimitKb}MB`}
                    variant='outlined'
                    color='primary'
                  />
                </Box>
              </Box>

              {/* Sample Input/Output */}
              {problem.sampleInput && (
                <Box sx={{ mb: 3 }}>
                  <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                    Ví dụ
                  </Typography>
                  <Paper sx={{ p: 2, bgcolor: 'grey.50', mb: 2 }}>
                    <Typography variant='body2' sx={{ fontWeight: 600, mb: 1 }}>
                      Input:
                    </Typography>
                    <Typography variant='body2' component='pre' sx={{ fontFamily: 'monospace' }}>
                      {problem.sampleInput}
                    </Typography>
                  </Paper>
                  {problem.sampleOutput && (
                    <Paper sx={{ p: 2, bgcolor: 'grey.50' }}>
                      <Typography variant='body2' sx={{ fontWeight: 600, mb: 1 }}>
                        Output:
                      </Typography>
                      <Typography variant='body2' component='pre' sx={{ fontFamily: 'monospace' }}>
                        {problem.sampleOutput}
                      </Typography>
                    </Paper>
                  )}
                </Box>
              )}

              {/* Tags */}
              <Box>
                <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                  Tags
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                  <Chip label={problem.category} size='small' color='primary' />
                  {problem.tags.map((tag) => (
                    <Chip key={tag} label={tag} size='small' variant='outlined' />
                  ))}
                </Box>
              </Box>
            </TabPanel>

            <TabPanel value={tabValue} index={1}>
              <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                Hướng dẫn giải
              </Typography>
              <Typography variant='body2' color='text.secondary'>
                Nội dung hướng dẫn sẽ được cập nhật sau...
              </Typography>
            </TabPanel>

            <TabPanel value={tabValue} index={2}>
              <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                Lịch sử nộp bài
              </Typography>
              <Typography variant='body2' color='text.secondary'>
                Chưa có lần nộp bài nào.
              </Typography>
            </TabPanel>
          </Box>
        </Box>

        {/* Right Panel - Code Editor */}
        <Box sx={{ width: '60%', display: 'flex', flexDirection: 'column', bgcolor: '#1e1e1e' }}>
          {/* Editor Toolbar */}
          <Box
            sx={{
              px: 2,
              py: 1.5,
              display: 'flex',
              alignItems: 'center',
              gap: 2,
              bgcolor: '#2d2d2d',
              borderBottom: '1px solid #3d3d3d',
            }}
          >
            <FormControl size='small' sx={{ minWidth: 150 }}>
              <Select
                value={language}
                onChange={(e) => handleLanguageChange(e.target.value)}
                sx={{
                  color: 'white',
                  '.MuiOutlinedInput-notchedOutline': { borderColor: 'primary.main' },
                  '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'primary.main' },
                  '& .MuiSvgIcon-root': { color: 'primary.main' },
                }}
              >
                <MenuItem value='cpp'>C++</MenuItem>
                <MenuItem value='java'>Java</MenuItem>
                <MenuItem value='python'>Python</MenuItem>
                <MenuItem value='javascript'>JavaScript</MenuItem>
                <MenuItem value='typescript'>TypeScript</MenuItem>
                <MenuItem value='c'>C</MenuItem>
                <MenuItem value='csharp'>C#</MenuItem>
                <MenuItem value='go'>Go</MenuItem>
                <MenuItem value='rust'>Rust</MenuItem>
              </Select>
            </FormControl>

            <Button
              startIcon={<RestartAltIcon />}
              size='small'
              onClick={handleResetCode}
              sx={{ color: '#86868b' }}
            >
              Reset Code
            </Button>

            <Box sx={{ flexGrow: 1 }} />

            <Button
              startIcon={<PlayArrowIcon />}
              variant='outlined'
              sx={{ color: 'primary.main', borderColor: 'primary.main' }}
              onClick={handleRunCode}
              disabled={isRunning}
            >
              {isRunning ? 'Đang chạy...' : 'Chạy thử'}
            </Button>
            <Button
              startIcon={<SendIcon />}
              variant='contained'
              sx={{ bgcolor: 'primary.main', color: 'secondary.main', fontWeight: 600 }}
              onClick={handleSubmitCode}
              disabled={isRunning}
            >
              Nộp bài
            </Button>
          </Box>

          {/* Code Editor Area */}
          <Box sx={{ flexGrow: 1, overflow: 'hidden' }}>
            <CodeEditor value={code} onChange={(value) => setCode(value || '')} language={language} />
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
            }}
          >
            <Box sx={{ p: 2 }}>
              <Typography
                variant='body2'
                sx={{
                  fontFamily: 'monospace',
                  color: '#d4d4d4',
                  whiteSpace: 'pre-wrap',
                  wordBreak: 'break-word',
                }}
              >
                {output || 'Output console...\n\nNhấn "Chạy thử" để test code hoặc "Nộp bài" để submit.'}
              </Typography>
            </Box>
          </Paper>
        </Box>
      </Box>
    </Box>
  )
}
