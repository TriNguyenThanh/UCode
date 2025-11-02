import { useState } from 'react'
import { redirect, useNavigate, useLoaderData, Form, useActionData } from 'react-router'
import type { Route } from './+types/teacher.problem.$id.edit'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import { mockProblems } from '~/data/mock'
import {
  Box,
  Container,
  Typography,
  TextField,
  Button,
  Paper,
  Chip,
  IconButton,
  InputAdornment,
  Tabs,
  Tab,
  Alert,
  Divider,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material'
import SaveIcon from '@mui/icons-material/Save'
import CancelIcon from '@mui/icons-material/Cancel'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import NotificationsActiveIcon from '@mui/icons-material/NotificationsActive'

interface TabPanelProps {
  children?: React.ReactNode
  index: number
  value: number
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <div role="tabpanel" hidden={value !== index}>
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  )
}

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }

  const problem = mockProblems.find((p) => p.id === params.id)
  if (!problem) {
    throw new Response('Problem không tồn tại', { status: 404 })
  }

  return { user, problem }
}

export async function clientAction({ request, params }: Route.ClientActionArgs) {
  const formData = await request.formData()
  const title = formData.get('title') as string
  
  if (!title) {
    return { error: 'Vui lòng điền tên bài toán' }
  }

  // Update problem (mock - in real app, this would call API)
  console.log('Updating problem:', params.id, { title })
  
  // Redirect back to problem detail
  return redirect(`/problem/${params.id}`)
}

export default function EditProblem() {
  const { problem } = useLoaderData<typeof clientLoader>()
  const navigate = useNavigate()
  const actionData = useActionData<typeof clientAction>()
  const [tabValue, setTabValue] = useState(0)
  const [tags, setTags] = useState<string[]>(problem.tags || [])
  const [tagInput, setTagInput] = useState('')
  const [difficulty, setDifficulty] = useState<'Easy' | 'Medium' | 'Hard'>(problem.difficulty || 'Medium')

  const handleAddTag = () => {
    if (tagInput.trim() && !tags.includes(tagInput.trim())) {
      setTags([...tags, tagInput.trim()])
      setTagInput('')
    }
  }

  const handleDeleteTag = (tagToDelete: string) => {
    setTags(tags.filter((tag) => tag !== tagToDelete))
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />
      
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'secondary.main', mb: 1 }}>
            Chỉnh sửa bài toán
          </Typography>
          <Typography variant="body1" color="text.secondary">
            ID: {problem.id} - {problem.title}
          </Typography>
        </Box>

        {actionData?.error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {actionData.error}
          </Alert>
        )}

        <Form method="post">
          <Paper sx={{ mb: 3 }}>
            {/* Tabs */}
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <Tabs
                value={tabValue}
                onChange={(_, newValue) => setTabValue(newValue)}
                sx={{
                  px: 2,
                  '& .MuiTab-root': { color: 'text.secondary', textTransform: 'none', fontSize: '1rem' },
                  '& .Mui-selected': { color: 'secondary.main', fontWeight: 'bold' },
                  '& .MuiTabs-indicator': { bgcolor: 'primary.main', height: 3 },
                }}
              >
                <Tab label="Problem Statement" />
                <Tab label="Input & Output Format" />
                <Tab label="Constraints & Scoring" />
                <Tab label="Notifications" />
              </Tabs>
            </Box>

            {/* Tab 1: Problem Statement */}
            <TabPanel value={tabValue} index={0}>
              <Box sx={{ px: 3 }}>
                <TextField
                  fullWidth
                  required
                  name="title"
                  label="Tên bài toán"
                  defaultValue={problem.title}
                  placeholder="VD: Two Sum, Binary Search, Merge Sort..."
                  sx={{ mb: 3 }}
                />

                <FormControl fullWidth sx={{ mb: 3 }}>
                  <InputLabel>Độ khó</InputLabel>
                  <Select
                    value={difficulty}
                    onChange={(e) => setDifficulty(e.target.value as 'Easy' | 'Medium' | 'Hard')}
                    label="Độ khó"
                    name="difficulty"
                  >
                    <MenuItem value="Easy">Easy</MenuItem>
                    <MenuItem value="Medium">Medium</MenuItem>
                    <MenuItem value="Hard">Hard</MenuItem>
                  </Select>
                </FormControl>

                <TextField
                  fullWidth
                  name="description"
                  label="Mô tả bài toán"
                  defaultValue={problem.description}
                  placeholder="Mô tả chi tiết về bài toán, yêu cầu, ví dụ..."
                  multiline
                  rows={12}
                  sx={{ mb: 3 }}
                  helperText="Hỗ trợ Markdown. Sử dụng backticks ` ` để highlight code."
                />

                <TextField
                  fullWidth
                  name="sampleInput"
                  label="Sample Input"
                  defaultValue="[2,7,11,15], target = 9"
                  placeholder="VD: [2,7,11,15], target = 9"
                  multiline
                  rows={3}
                  sx={{ mb: 2 }}
                />

                <TextField
                  fullWidth
                  name="sampleOutput"
                  label="Sample Output"
                  defaultValue="[0,1]"
                  placeholder="VD: [0,1]"
                  multiline
                  rows={3}
                  sx={{ mb: 2 }}
                />

                <TextField
                  fullWidth
                  name="explanation"
                  label="Explanation (Optional)"
                  placeholder="Giải thích sample..."
                  multiline
                  rows={3}
                />
              </Box>
            </TabPanel>

            {/* Tab 2: Input & Output Format */}
            <TabPanel value={tabValue} index={1}>
              <Box sx={{ px: 3 }}>
                <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold', color: 'secondary.main' }}>
                  Input Format
                </Typography>
                <TextField
                  fullWidth
                  name="inputFormat"
                  placeholder="Mô tả format input chi tiết...
VD:
- Dòng đầu tiên chứa số nguyên n (1 ≤ n ≤ 10^5)
- Dòng thứ hai chứa n số nguyên a[i] (-10^9 ≤ a[i] ≤ 10^9)"
                  multiline
                  rows={8}
                  sx={{ mb: 4 }}
                />

                <Divider sx={{ my: 3 }} />

                <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold', color: 'secondary.main' }}>
                  Output Format
                </Typography>
                <TextField
                  fullWidth
                  name="outputFormat"
                  placeholder="Mô tả format output chi tiết...
VD:
- In ra một số nguyên duy nhất là kết quả
- Nếu không tồn tại đáp án, in -1"
                  multiline
                  rows={6}
                />
              </Box>
            </TabPanel>

            {/* Tab 3: Constraints & Scoring */}
            <TabPanel value={tabValue} index={2}>
              <Box sx={{ px: 3 }}>
                <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold', color: 'secondary.main' }}>
                  Constraints
                </Typography>
                <TextField
                  fullWidth
                  name="constraints"
                  placeholder="Ràng buộc của bài toán...
VD:
• 1 ≤ n ≤ 10^5
• -10^9 ≤ a[i] ≤ 10^9
• 1 ≤ target ≤ 10^9
• Đảm bảo luôn có đáp án"
                  multiline
                  rows={8}
                  sx={{ mb: 4 }}
                />

                <Divider sx={{ my: 3 }} />

                <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold', color: 'secondary.main' }}>
                  Time & Memory Limits
                </Typography>
                <Box sx={{ display: 'flex', gap: 2, mb: 4 }}>
                  <TextField
                    fullWidth
                    name="timeLimit"
                    label="Time Limit (seconds)"
                    type="number"
                    defaultValue={1}
                    InputProps={{
                      inputProps: { min: 0.5, max: 10, step: 0.5 },
                      endAdornment: <InputAdornment position="end">s</InputAdornment>,
                    }}
                  />
                  <TextField
                    fullWidth
                    name="memoryLimit"
                    label="Memory Limit (MB)"
                    type="number"
                    defaultValue={128}
                    InputProps={{
                      inputProps: { min: 64, max: 512, step: 64 },
                      endAdornment: <InputAdornment position="end">MB</InputAdornment>,
                    }}
                  />
                </Box>

                <Divider sx={{ my: 3 }} />

                <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold', color: 'secondary.main' }}>
                  Tags
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap' }}>
                  {tags.map((tag) => (
                    <Chip
                      key={tag}
                      label={tag}
                      onDelete={() => handleDeleteTag(tag)}
                      color="primary"
                      sx={{ bgcolor: 'primary.main', color: 'secondary.main' }}
                    />
                  ))}
                </Box>
                <TextField
                  fullWidth
                  value={tagInput}
                  onChange={(e) => setTagInput(e.target.value)}
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault()
                      handleAddTag()
                    }
                  }}
                  placeholder="Thêm tag (Array, Hash Table, Binary Search...)"
                  InputProps={{
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton onClick={handleAddTag} edge="end">
                          <AddIcon />
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                />
                <input type="hidden" name="tags" value={tags.join(',')} />
              </Box>
            </TabPanel>

            {/* Tab 4: Notifications */}
            <TabPanel value={tabValue} index={3}>
              <Box sx={{ px: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
                  <NotificationsActiveIcon sx={{ color: 'primary.main', fontSize: 28 }} />
                  <Typography variant="h6" sx={{ fontWeight: 'bold', color: 'secondary.main' }}>
                    Notifications & Hints
                  </Typography>
                </Box>

                <Alert severity="info" sx={{ mb: 3 }}>
                  Thông báo sẽ hiển thị cho sinh viên khi họ làm bài toán này. Có thể dùng để:
                  <ul style={{ marginTop: 8, marginBottom: 0 }}>
                    <li>Cảnh báo về test cases đặc biệt</li>
                    <li>Gợi ý cách tiếp cận</li>
                    <li>Lưu ý về độ phức tạp</li>
                  </ul>
                </Alert>

                <TextField
                  fullWidth
                  name="notification1"
                  label="Notification 1"
                  placeholder="VD: Lưu ý xử lý trường hợp mảng rỗng hoặc chỉ có 1 phần tử"
                  multiline
                  rows={2}
                  sx={{ mb: 2 }}
                />

                <TextField
                  fullWidth
                  name="notification2"
                  label="Notification 2"
                  placeholder="VD: Có thể sử dụng Hash Table để giảm độ phức tạp xuống O(n)"
                  multiline
                  rows={2}
                  sx={{ mb: 2 }}
                />

                <TextField
                  fullWidth
                  name="notification3"
                  label="Notification 3 (Optional)"
                  placeholder="VD: Thử suy nghĩ về cách tối ưu bộ nhớ..."
                  multiline
                  rows={2}
                  sx={{ mb: 3 }}
                />

                <Divider sx={{ my: 3 }} />

                <Typography variant="h6" sx={{ mb: 2, fontWeight: 'bold', color: 'secondary.main' }}>
                  Editorial / Solution (Optional)
                </Typography>
                <TextField
                  fullWidth
                  name="editorial"
                  placeholder="Hướng dẫn giải chi tiết, giải thích thuật toán, code mẫu..."
                  multiline
                  rows={10}
                  helperText="Chỉ hiển thị sau khi sinh viên submit hoặc sau deadline"
                />
              </Box>
            </TabPanel>
          </Paper>

          {/* Actions */}
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'space-between', alignItems: 'center' }}>
            <Button
              variant="outlined"
              startIcon={<CancelIcon />}
              onClick={() => navigate(-1)}
              sx={{
                borderColor: 'text.secondary',
                color: 'text.secondary',
              }}
            >
              Hủy
            </Button>
            
            <Box sx={{ display: 'flex', gap: 2 }}>
              {tabValue < 3 && (
                <Button
                  variant="outlined"
                  onClick={() => setTabValue(tabValue + 1)}
                  sx={{
                    borderColor: 'secondary.main',
                    color: 'secondary.main',
                  }}
                >
                  Tiếp theo
                </Button>
              )}
              <Button
                type="submit"
                variant="contained"
                startIcon={<SaveIcon />}
                sx={{
                  bgcolor: 'secondary.main',
                  color: 'primary.main',
                  px: 4,
                  '&:hover': {
                    bgcolor: 'primary.main',
                    color: 'secondary.main',
                  },
                }}
              >
                Lưu thay đổi
              </Button>
            </Box>
          </Box>
        </Form>
      </Container>
    </Box>
  )
}
