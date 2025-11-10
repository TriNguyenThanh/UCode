import React, { useState, useEffect, useMemo } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Checkbox,
  Box,
  Typography,
  Chip,
  IconButton,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  Tooltip,
  Alert,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import CloseIcon from '@mui/icons-material/Close'
import EditIcon from '@mui/icons-material/Edit'
import CheckIcon from '@mui/icons-material/Check'
import type { Problem, Difficulty } from '~/types'
import { searchProblems } from '~/services'

interface ProblemWithPoints extends Problem {
  points: number
  orderIndex: number
  isSelected: boolean
  isEditing: boolean
}

interface AddProblemDialogProps {
  open: boolean
  onClose: () => void
  existingProblems: { problemId: string; points: number; orderIndex: number }[]
  onSave: (problems: { problemId: string; points: number; orderIndex: number }[]) => Promise<void>
}

export function AddProblemDialog({ open, onClose, existingProblems, onSave }: AddProblemDialogProps) {
  const [searchQuery, setSearchQuery] = useState('')
  const [difficultyFilter, setDifficultyFilter] = useState<Difficulty | 'all'>('all')
  const [allProblems, setAllProblems] = useState<ProblemWithPoints[]>([])
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [hasChanges, setHasChanges] = useState(false)
  const [editingPoints, setEditingPoints] = useState<{ [key: string]: number }>({})

  // Load all problems from API
  useEffect(() => {
    if (open) {
      loadProblems()
    }
  }, [open])

  const loadProblems = async () => {
    setLoading(true)
    try {
      // Fetch all problems from API
      const response = await searchProblems({
        page: 1,
        pageSize: 100, // Large number to get all problems
      })
      
      const mockProblems: Problem[] = response.data || []

      const problemsWithSelection = mockProblems.map((problem) => {
        const existing = existingProblems.find((p) => p.problemId === problem.problemId)
        return {
          ...problem,
          points: existing?.points || 100, // Default 100 points
          orderIndex: existing?.orderIndex || 0,
          isSelected: !!existing,
          isEditing: false,
        }
      })

      setAllProblems(problemsWithSelection)
    } catch (error) {
      console.error('Failed to load problems:', error)
    } finally {
      setLoading(false)
    }
  }

  // Filter problems
  const filteredProblems = useMemo(() => {
    return allProblems.filter((problem) => {
      const matchesSearch =
        problem.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
        problem.code.toLowerCase().includes(searchQuery.toLowerCase()) ||
        problem.problemId.toLowerCase().includes(searchQuery.toLowerCase())

      const matchesDifficulty =
        difficultyFilter === 'all' || problem.difficulty === difficultyFilter

      return matchesSearch && matchesDifficulty
    })
  }, [allProblems, searchQuery, difficultyFilter])

  // Handle checkbox toggle
  const handleToggleSelect = (problemId: string) => {
    setAllProblems((prev) =>
      prev.map((p) =>
        p.problemId === problemId ? { ...p, isSelected: !p.isSelected } : p
      )
    )
    setHasChanges(true)
  }

  // Handle edit points
  const handleStartEdit = (problemId: string, currentPoints: number) => {
    setEditingPoints({ ...editingPoints, [problemId]: currentPoints })
    setAllProblems((prev) =>
      prev.map((p) => (p.problemId === problemId ? { ...p, isEditing: true } : p))
    )
  }

  const handleSavePoints = (problemId: string) => {
    const newPoints = editingPoints[problemId]
    if (newPoints !== undefined && newPoints > 0) {
      setAllProblems((prev) =>
        prev.map((p) =>
          p.problemId === problemId
            ? { ...p, points: newPoints, isEditing: false }
            : p
        )
      )
      setHasChanges(true)
    }
  }

  const handleCancelEdit = (problemId: string) => {
    setAllProblems((prev) =>
      prev.map((p) => (p.problemId === problemId ? { ...p, isEditing: false } : p))
    )
    const newEditingPoints = { ...editingPoints }
    delete newEditingPoints[problemId]
    setEditingPoints(newEditingPoints)
  }

  // Handle save
  const handleSave = async () => {
    setSaving(true)
    try {
      const selectedProblems = allProblems
        .filter((p) => p.isSelected)
        .map((p, index) => ({
          problemId: p.problemId,
          points: p.points,
          orderIndex: index + 1,
        }))

      await onSave(selectedProblems)
      setHasChanges(false)
      onClose()
    } catch (error) {
      console.error('Failed to save problems:', error)
      alert('Không thể lưu thay đổi. Vui lòng thử lại.')
    } finally {
      setSaving(false)
    }
  }

  // Get difficulty color
  const getDifficultyColor = (difficulty: Difficulty) => {
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

  const selectedCount = allProblems.filter((p) => p.isSelected).length

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle sx={{ bgcolor: 'secondary.main', color: 'primary.main', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h6" component="span">
            Thêm bài vào assignment
          </Typography>
          {selectedCount > 0 && (
            <Chip
              label={`${selectedCount} bài được chọn`}
              size="small"
              sx={{ ml: 2, bgcolor: 'primary.main', color: 'secondary.main' }}
            />
          )}
        </Box>
        <IconButton onClick={onClose} size="small" sx={{ color: 'primary.main' }}>
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent sx={{ mt: 2 }}>
        {/* Filters */}
        <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
          <TextField
            fullWidth
            placeholder="Tìm kiếm theo tên, code, hoặc ID..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            }}
          />
          <FormControl sx={{ minWidth: 150 }}>
            <InputLabel>Độ khó</InputLabel>
            <Select
              value={difficultyFilter}
              label="Độ khó"
              onChange={(e) => setDifficultyFilter(e.target.value as any)}
            >
              <MenuItem value="all">Tất cả</MenuItem>
              <MenuItem value="EASY">Easy</MenuItem>
              <MenuItem value="MEDIUM">Medium</MenuItem>
              <MenuItem value="HARD">Hard</MenuItem>
            </Select>
          </FormControl>
        </Box>

        {/* Info Alert */}
        <Alert severity="info" sx={{ mb: 2 }}>
          Chọn các bài cần thêm vào assignment. Mặc định điểm tối đa là 100 điểm, bạn có thể chỉnh sửa sau.
        </Alert>

        {/* Problems Table */}
        <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 500 }}>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                <TableCell padding="checkbox" sx={{ bgcolor: 'grey.100' }}>
                  <Checkbox
                    indeterminate={selectedCount > 0 && selectedCount < filteredProblems.length}
                    checked={filteredProblems.length > 0 && selectedCount === filteredProblems.length}
                    onChange={(e) => {
                      const isChecked = e.target.checked
                      setAllProblems((prev) =>
                        prev.map((p) =>
                          filteredProblems.some((fp) => fp.problemId === p.problemId)
                            ? { ...p, isSelected: isChecked }
                            : p
                        )
                      )
                      setHasChanges(true)
                    }}
                  />
                </TableCell>
                <TableCell sx={{ bgcolor: 'grey.100', fontWeight: 600 }}>Code</TableCell>
                <TableCell sx={{ bgcolor: 'grey.100', fontWeight: 600 }}>Tên bài</TableCell>
                <TableCell sx={{ bgcolor: 'grey.100', fontWeight: 600 }}>Độ khó</TableCell>
                <TableCell sx={{ bgcolor: 'grey.100', fontWeight: 600 }}>Tags</TableCell>
                <TableCell sx={{ bgcolor: 'grey.100', fontWeight: 600, textAlign: 'right' }}>
                  Điểm
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                    Đang tải danh sách bài...
                  </TableCell>
                </TableRow>
              ) : filteredProblems.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                    Không tìm thấy bài nào
                  </TableCell>
                </TableRow>
              ) : (
                filteredProblems.map((problem) => (
                  <TableRow
                    key={problem.problemId}
                    hover
                    sx={{
                      bgcolor: problem.isSelected ? 'action.selected' : 'inherit',
                      '&:hover': { bgcolor: problem.isSelected ? 'action.selected' : 'action.hover' },
                    }}
                  >
                    <TableCell padding="checkbox">
                      <Checkbox
                        checked={problem.isSelected}
                        onChange={() => handleToggleSelect(problem.problemId)}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                        {problem.code}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ fontWeight: 500 }}>
                        {problem.title}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={problem.difficulty}
                        size="small"
                        color={getDifficultyColor(problem.difficulty)}
                      />
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                        {problem.tagNames.slice(0, 2).map((tag) => (
                          <Chip
                            key={tag}
                            label={tag}
                            size="small"
                            variant="outlined"
                            sx={{ fontSize: '0.7rem', height: 20 }}
                          />
                        ))}
                        {problem.tagNames.length > 2 && (
                          <Tooltip title={problem.tagNames.slice(2).join(', ')}>
                            <Chip
                              label={`+${problem.tagNames.length - 2}`}
                              size="small"
                              variant="outlined"
                              sx={{ fontSize: '0.7rem', height: 20 }}
                            />
                          </Tooltip>
                        )}
                      </Box>
                    </TableCell>
                    <TableCell align="right">
                      {problem.isEditing ? (
                        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', gap: 1 }}>
                          <TextField
                            type="number"
                            size="small"
                            value={editingPoints[problem.problemId] ?? problem.points}
                            onChange={(e) =>
                              setEditingPoints({
                                ...editingPoints,
                                [problem.problemId]: parseInt(e.target.value) || 0,
                              })
                            }
                            inputProps={{ min: 1, max: 100 }}
                            sx={{ width: 80 }}
                          />
                          <IconButton
                            size="small"
                            color="success"
                            onClick={() => handleSavePoints(problem.problemId)}
                          >
                            <CheckIcon fontSize="small" />
                          </IconButton>
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => handleCancelEdit(problem.problemId)}
                          >
                            <CloseIcon fontSize="small" />
                          </IconButton>
                        </Box>
                      ) : (
                        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', gap: 1 }}>
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>
                            {problem.points} điểm
                          </Typography>
                          <IconButton
                            size="small"
                            onClick={() => handleStartEdit(problem.problemId, problem.points)}
                            disabled={!problem.isSelected}
                          >
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Box>
                      )}
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </DialogContent>

      <DialogActions sx={{ p: 2, borderTop: '1px solid', borderColor: 'divider' }}>
        <Button onClick={onClose} disabled={saving}>
          Hủy
        </Button>
        <Button
          variant="contained"
          onClick={handleSave}
          disabled={!hasChanges || saving}
          sx={{
            bgcolor: 'secondary.main',
            color: 'primary.main',
            '&:hover': {
              bgcolor: 'primary.main',
              color: 'secondary.main',
            },
            '&:disabled': {
              bgcolor: 'grey.300',
              color: 'grey.500',
            },
          }}
        >
          {saving ? 'Đang lưu...' : 'Lưu thay đổi'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
