import React from 'react'
import {
  Box,
  Typography,
  Button,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  CircularProgress,
  Divider,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import SaveIcon from '@mui/icons-material/Save'
import type { Tag } from '~/types'
import {
  addTagsToProblem,
  removeTagFromProblem,
} from '~/services/problemService'
import {
  getAllTags,
} from '~/services/tagService'

interface TagManagementProps {
  problemId: string
  initialTags: string[]
  onSnackbar: (message: string, severity: 'success' | 'error') => void
}

export function TagManagement({ problemId, initialTags, onSnackbar }: TagManagementProps) {
  const [tagDialogOpen, setTagDialogOpen] = React.useState(false)
  const [availableTags, setAvailableTags] = React.useState<Tag[]>([])
  const [currentTags, setCurrentTags] = React.useState<string[]>(initialTags)
  const [selectedTagIds, setSelectedTagIds] = React.useState<string[]>([])
  const [loadingTags, setLoadingTags] = React.useState(false)
  const [savingTags, setSavingTags] = React.useState(false)

  React.useEffect(() => {
    setCurrentTags(initialTags)
  }, [initialTags])

  React.useEffect(() => {
    if (tagDialogOpen) {
      setLoadingTags(true)
      getAllTags()
        .then(tags => {
          setAvailableTags(tags)
          const selectedIds = tags
            .filter(tag => currentTags.includes(tag.name))
            .map(tag => tag.tagId)
          setSelectedTagIds(selectedIds)
        })
        .catch(error => {
          console.error('Failed to load tags:', error)
          onSnackbar('Không thể tải danh sách tags', 'error')
        })
        .finally(() => {
          setLoadingTags(false)
        })
    }
  }, [tagDialogOpen, currentTags, onSnackbar])

  const handleToggleTag = (tagId: string) => {
    setSelectedTagIds(prev => {
      if (prev.includes(tagId)) {
        return prev.filter(id => id !== tagId)
      } else {
        return [...prev, tagId]
      }
    })
  }

  const handleSaveTags = async () => {
    setSavingTags(true)
    try {
      const currentTagIds = availableTags
        .filter(tag => currentTags.includes(tag.name))
        .map(tag => tag.tagId)

      const tagsToAdd = selectedTagIds.filter(id => !currentTagIds.includes(id))
      const tagsToRemove = currentTagIds.filter(id => !selectedTagIds.includes(id))

      for (const tagId of tagsToRemove) {
        await removeTagFromProblem(problemId, tagId)
      }

      if (tagsToAdd.length > 0) {
        await addTagsToProblem(problemId, tagsToAdd)
      }

      const newTagNames = availableTags
        .filter(tag => selectedTagIds.includes(tag.tagId))
        .map(tag => tag.name)
      setCurrentTags(newTagNames)

      onSnackbar('Cập nhật tags thành công!', 'success')
      setTagDialogOpen(false)
    } catch (error: any) {
      onSnackbar(error.message || 'Không thể cập nhật tags', 'error')
    } finally {
      setSavingTags(false)
    }
  }

  const handleRemoveTagFromChip = async (tagName: string) => {
    try {
      const tag = availableTags.find(t => t.name === tagName)
      if (!tag) {
        const tags = await getAllTags()
        setAvailableTags(tags)
        const foundTag = tags.find(t => t.name === tagName)
        if (foundTag) {
          await removeTagFromProblem(problemId, foundTag.tagId)
          setCurrentTags(prev => prev.filter(t => t !== tagName))
          onSnackbar(`Đã xóa tag "${tagName}"`, 'success')
        }
      } else {
        await removeTagFromProblem(problemId, tag.tagId)
        setCurrentTags(prev => prev.filter(t => t !== tagName))
        onSnackbar(`Đã xóa tag "${tagName}"`, 'success')
      }
    } catch (error: any) {
      onSnackbar(error.message || 'Không thể xóa tag', 'error')
    }
  }

  return (
    <>
      <Box sx={{ px: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, color: '#1d1d1f' }}>
            Tags ({currentTags.length})
          </Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setTagDialogOpen(true)}
            sx={{
              bgcolor: '#FACB01',
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': {
                bgcolor: '#0051D5',
              },
            }}
          >
            Quản lý Tags
          </Button>
        </Box>

        {currentTags.length === 0 ? (
          <Alert severity="info">
            Chưa có tag nào được gán cho bài toán này. Nhấn "Quản lý Tags" để thêm tags.
          </Alert>
        ) : (
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            {currentTags.map((tagName) => (
              <Chip
                key={tagName}
                label={tagName}
                onDelete={() => handleRemoveTagFromChip(tagName)}
                sx={{
                  bgcolor: '#FACB01',
                  color: '#ffffff',
                  fontWeight: 600,
                  '& .MuiChip-deleteIcon': {
                    color: 'rgba(255, 255, 255, 0.7)',
                    '&:hover': {
                      color: '#ffffff',
                    },
                  },
                }}
              />
            ))}
          </Box>
        )}
      </Box>

      {/* Tag Management Dialog */}
      <Dialog 
        open={tagDialogOpen} 
        onClose={() => setTagDialogOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            Quản lý Tags
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Chọn các tag phù hợp cho bài toán này
          </Typography>
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            {loadingTags ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                <CircularProgress />
              </Box>
            ) : (
              <>
                <Typography variant="subtitle2" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
                  Tags đã chọn ({selectedTagIds.length})
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 3, minHeight: 40 }}>
                  {selectedTagIds.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">
                      Chưa có tag nào được chọn
                    </Typography>
                  ) : (
                    selectedTagIds.map((tagId) => {
                      const tag = availableTags.find(t => t.tagId === tagId)
                      return tag ? (
                        <Chip
                          key={tagId}
                          label={tag.name}
                          sx={{
                            bgcolor: '#34C759',
                            color: '#ffffff',
                            fontWeight: 600,
                          }}
                        />
                      ) : null
                    })
                  )}
                </Box>

                <Divider sx={{ my: 2 }} />

                <Typography variant="subtitle2" sx={{ mb: 2, fontWeight: 600, color: '#1d1d1f' }}>
                  Tất cả các tags ({availableTags.length})
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', maxHeight: 400, overflowY: 'auto', p: 1 }}>
                  {availableTags.map((tag) => {
                    const isSelected = selectedTagIds.includes(tag.tagId)
                    return (
                      <Chip
                        key={tag.tagId}
                        label={tag.name}
                        onClick={() => handleToggleTag(tag.tagId)}
                        sx={{
                          bgcolor: isSelected ? '#FACB01' : '#f5f5f7',
                          color: isSelected ? '#ffffff' : '#1d1d1f',
                          fontWeight: isSelected ? 600 : 400,
                          border: isSelected ? 'none' : '1px solid #d2d2d7',
                          cursor: 'pointer',
                          '&:hover': {
                            bgcolor: isSelected ? '#0051D5' : '#e8e8ed',
                          },
                        }}
                      />
                    )
                  })}
                </Box>
              </>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={() => setTagDialogOpen(false)}
            sx={{ textTransform: 'none', fontWeight: 600 }}
          >
            Hủy
          </Button>
          <Button 
            variant="contained"
            onClick={handleSaveTags}
            disabled={savingTags}
            startIcon={savingTags ? <CircularProgress size={20} /> : <SaveIcon />}
            sx={{
              bgcolor: '#FACB01',
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': {
                bgcolor: '#0051D5',
              },
            }}
          >
            {savingTags ? 'Đang lưu...' : 'Lưu'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
