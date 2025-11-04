import { useState } from 'react'
import { redirect, useNavigate, Form, useActionData } from 'react-router'
import type { Route } from './+types/teacher.problem.create'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
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

export async function clientLoader({ }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user || user.role !== 'teacher') {
    throw redirect('/home')
  }
  return { user }
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const formData = await request.formData()
  const title = formData.get('title') as string

  if (!title) {
    return { error: 'Vui lòng điền tên bài toán' }
  }

  // Create problem (mock - in real app, this would call API)
  const newProblemId = `problem-${Date.now()}`

  // Redirect to problem detail or problem list
  return redirect(`/teacher/problem/${newProblemId}`)
}

export default function CreateProblem() {
  const navigate = useNavigate()
  const actionData = useActionData<typeof clientAction>()
  const [tabValue, setTabValue] = useState(0)
  const [tags, setTags] = useState<string[]>([])
  const [tagInput, setTagInput] = useState('')
  const [difficulty, setDifficulty] = useState<'Easy' | 'Medium' | 'Hard'>('Medium')

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
    <Box sx={{ minHeight: '100vh', bgcolor: '#f5f5f7' }}>
      <Navigation />

      <Container maxWidth="lg" sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" sx={{ fontWeight: 700, color: '#1d1d1f', mb: 1 }}>
            Tạo bài toán mới
          </Typography>
          <Typography variant="body1" sx={{ color: '#86868b' }}>
            Tạo bài toán để thêm vào assignments hoặc cho sinh viên luyện tập
          </Typography>
        </Box>

        {actionData?.error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {actionData.error}
          </Alert>
        )}

        <Form method="post">
          <Paper elevation={0} sx={{ mb: 3, bgcolor: '#ffffff', border: '1px solid #d2d2d7', borderRadius: 2 }}>
            {/* Tabs */}
            <Box sx={{ borderBottom: 1, borderColor: '#d2d2d7' }}>
              <Tabs
                value={tabValue}
                onChange={(_, newValue) => setTabValue(newValue)}
                sx={{
                  px: 2,
                  '& .MuiTab-root': { color: '#86868b', textTransform: 'none', fontSize: '1rem', fontWeight: 500 },
                  '& .Mui-selected': { color: '#007AFF', fontWeight: 600 },
                  '& .MuiTabs-indicator': { bgcolor: '#007AFF', height: 3 },
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
                  placeholder="VD: [2,7,11,15], target = 9"
                  multiline
                  rows={3}
                  sx={{ mb: 2 }}
                />

                <TextField
                  fullWidth
                  name="sampleOutput"
                  label="Sample Output"
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
                <Typography variant="h6" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
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

                <Typography variant="h6" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
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
                <Typography variant="h6" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
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

                <Typography variant="h6" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
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

                <Typography variant="h6" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
                  Tags
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap' }}>
                  {tags.map((tag) => (
                    <Chip
                      key={tag}
                      label={tag}
                      onDelete={() => handleDeleteTag(tag)}
                      sx={{ bgcolor: '#007AFF', color: '#ffffff', fontWeight: 600 }}
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
                  <NotificationsActiveIcon sx={{ color: '#007AFF', fontSize: 28 }} />
                  <Typography variant="h6" sx={{ fontWeight: 600, color: '#1d1d1f' }}>
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

                <Typography variant="h6" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
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
                borderColor: '#d2d2d7',
                color: '#86868b',
                textTransform: 'none',
                fontWeight: 600,
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
                    borderColor: '#007AFF',
                    color: '#007AFF',
                    textTransform: 'none',
                    fontWeight: 600,
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
                  bgcolor: '#007AFF',
                  color: '#ffffff',
                  px: 4,
                  textTransform: 'none',
                  fontWeight: 600,
                  '&:hover': {
                    bgcolor: '#0051D5',
                  },
                }}
              >
                Tạo Problem
              </Button>
            </Box>
          </Box>
        </Form>
      </Container>
    </Box>
  )
}
