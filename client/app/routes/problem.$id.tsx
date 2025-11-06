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
  CircularProgress,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import SendIcon from '@mui/icons-material/Send'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import MemoryIcon from '@mui/icons-material/Memory'
import RestartAltIcon from '@mui/icons-material/RestartAlt'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CancelIcon from '@mui/icons-material/Cancel'
import { CodeEditor } from '~/components/CodeEditor'
import { getProblem, getProblemForStudent } from '~/services/problemService'
import { runCode, submitCode, getSubmissionsByProblem } from '~/services/submissionService'
import { getAllLanguages } from '~/services/languageService'
import type { Problem, Language, Submission } from '~/types'

// Function to get code template based on language code
function getCodeTemplate(languageCode: string, problemLanguages?: Problem['problemLanguages']): string {
  // Kiểm tra xem problem có template riêng cho ngôn ngữ này không
  const problemLanguage = problemLanguages?.find(pl => pl.languageCode === languageCode)
  
  if (problemLanguage) {
    // Nối head + body + tail nếu có
    const parts = []
    if (problemLanguage.head) parts.push(problemLanguage.head)
    if (problemLanguage.body) parts.push(problemLanguage.body)
    if (problemLanguage.tail) parts.push(problemLanguage.tail)
    
    if (parts.length > 0) {
      return parts.join('\n\n')
    }
  }

  // Default templates nếu không có template từ problem
  const templates: Record<string, string> = {
    cpp: `#include <iostream>
using namespace std;

int main() {
    // Your code here
    return 0;
}`,
    java: `public class Solution {
    public static void main(String[] args) {
        // Your code here
    }
}`,
    python: `# Your code here
def solution():
    pass

if __name__ == "__main__":
    solution()`,
    javascript: `// Your code here
function solution() {
    
}

solution();`,
    typescript: `// Your code here
function solution(): void {
    
}

solution();`,
    c: `#include <stdio.h>

int main() {
    // Your code here
    return 0;
}`,
    csharp: `using System;

class Program {
    static void Main() {
        // Your code here
    }
}`,
    go: `package main

import "fmt"

func main() {
    // Your code here
}`,
    rust: `fn main() {
    // Your code here
}`,
  }
  return templates[languageCode] || '// Your code here'
}

export const meta: Route.MetaFunction = () => [
  { title: 'Giải bài tập | UCode' },
  { name: 'description', content: 'Coding interface để giải bài tập.' },
]

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')

  if (!params.id) throw new Response('Not Found', { status: 404 })

  try {
    // Fetch problem based on user role
    let problem: Problem
    if (user.role === 'student') {
      problem = await getProblemForStudent(params.id)
    } else {
      problem = await getProblem(params.id)
    }

    // Fetch available languages
    const languages = await getAllLanguages(false) // Only enabled languages

    // Fetch user's submission history for this problem
    let submissions: Submission[] = []
    try {
      submissions = await getSubmissionsByProblem(params.id, 1, 10)
    } catch (error) {
      console.error('Failed to fetch submissions:', error)
      // Continue even if submissions fail
    }

    return { user, problem, languages, submissions }
  } catch (error: any) {
    console.error('Failed to load problem:', error)
    throw new Response(error.message || 'Problem not found', { status: 404 })
  }
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
  const { problem, languages, submissions } = useLoaderData<typeof clientLoader>()
  const [tabValue, setTabValue] = React.useState(0)
  
  // Find first available language or default to cpp
  const defaultLanguage = languages.length > 0 ? languages[0] : null
  const [selectedLanguage, setSelectedLanguage] = React.useState<Language | null>(defaultLanguage)
  const [code, setCode] = React.useState('')
  const [output, setOutput] = React.useState('')
  const [isRunning, setIsRunning] = React.useState(false)
  const [isSubmitting, setIsSubmitting] = React.useState(false)

  // Initialize code template when language or problem changes
  React.useEffect(() => {
    if (selectedLanguage) {
      setCode(getCodeTemplate(selectedLanguage.code, problem.problemLanguages))
    }
  }, [selectedLanguage, problem])

  // Handle language change
  const handleLanguageChange = (languageId: string) => {
    const lang = languages.find(l => l.languageId === languageId)
    if (lang) {
      setSelectedLanguage(lang)
      setCode(getCodeTemplate(lang.code, problem.problemLanguages))
      setOutput('')
    }
  }

  // Handle reset code
  const handleResetCode = () => {
    if (selectedLanguage) {
      setCode(getCodeTemplate(selectedLanguage.code, problem.problemLanguages))
      setOutput('')
    }
  }

  // Handle run code (test without submitting)
  const handleRunCode = async () => {
    if (!selectedLanguage) {
      setOutput('❌ Vui lòng chọn ngôn ngữ lập trình')
      return
    }

    if (!code.trim()) {
      setOutput('❌ Vui lòng nhập code')
      return
    }

    setIsRunning(true)
    setOutput('⏳ Đang biên dịch và chạy code...\n')
    
    try {
      const result = await runCode({
        problemId: problem.problemId,
        languageId: selectedLanguage.languageId,
        sourceCode: code,
      })

      setOutput(`✅ Đã gửi code để chạy thử!\n\nSubmission ID: ${result.submissionId}\nStatus: ${result.status}\n\nĐang xử lý...`)
      
      // Có thể polling để lấy kết quả
      // TODO: Implement polling getSubmission(result.submissionId) để lấy kết quả chi tiết
      
    } catch (error: any) {
      setOutput(`❌ Lỗi: ${error.message || 'Không thể chạy code'}`)
    } finally {
      setIsRunning(false)
    }
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

    setIsSubmitting(true)
    setOutput('📤 Đang nộp bài...\n')
    
    try {
      const result = await submitCode({
        problemId: problem.problemId,
        languageId: selectedLanguage.languageId,
        sourceCode: code,
      })

      setOutput(`🎉 Đã nộp bài thành công!\n\nSubmission ID: ${result.submissionId}\nStatus: ${result.status}\nThời gian nộp: ${new Date(result.submittedAt).toLocaleString('vi-VN')}\n\nĐang chấm điểm...`)
      
      // Reload submissions
      window.location.reload()
      
    } catch (error: any) {
      setOutput(`❌ Lỗi: ${error.message || 'Không thể nộp bài'}`)
    } finally {
      setIsSubmitting(false)
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
                {problem.statement || 'Chưa có mô tả'}
              </Typography>

              {/* Input/Output Format */}
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

              {/* Constraints */}
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

              {/* Tags */}
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
              <Typography variant='h6' sx={{ fontWeight: 600, mb: 2 }}>
                Lịch sử nộp bài
              </Typography>
              {submissions.length > 0 ? (
                <TableContainer component={Paper} variant="outlined">
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Thời gian</TableCell>
                        <TableCell>Ngôn ngữ</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell align="right">Time (ms)</TableCell>
                        <TableCell align="right">Memory (KB)</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {submissions.map((sub) => (
                        <TableRow key={sub.submissionId}>
                          <TableCell>{new Date(sub.submittedAt).toLocaleString('vi-VN')}</TableCell>
                          <TableCell>{sub.language}</TableCell>
                          <TableCell>
                            <Chip 
                              label={sub.status} 
                              size="small"
                              color={sub.status === 'Accepted' ? 'success' : 'error'}
                              icon={sub.status === 'Accepted' ? <CheckCircleIcon /> : <CancelIcon />}
                            />
                          </TableCell>
                          <TableCell align="right">{sub.totalTime}</TableCell>
                          <TableCell align="right">{sub.totalMemory}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              ) : (
                <Typography variant='body2' color='text.secondary'>
                  Chưa có lần nộp bài nào.
                </Typography>
              )}
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
            <FormControl size='small' sx={{ minWidth: 180 }}>
              <Select
                value={selectedLanguage?.languageId || ''}
                onChange={(e) => handleLanguageChange(e.target.value)}
                sx={{
                  color: 'white',
                  '.MuiOutlinedInput-notchedOutline': { borderColor: 'primary.main' },
                  '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'primary.main' },
                  '& .MuiSvgIcon-root': { color: 'primary.main' },
                }}
              >
                {languages.map((lang) => (
                  <MenuItem key={lang.languageId} value={lang.languageId}>
                    {lang.displayName}
                  </MenuItem>
                ))}
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
              language={selectedLanguage?.code || 'cpp'} 
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
                {output || 'Nhấn "Chạy thử" để kiểm tra code hoặc "Nộp bài" để submit...'}
              </Typography>
            </Box>
          </Paper>
        </Box>
      </Box>
    </Box>
  )
}
