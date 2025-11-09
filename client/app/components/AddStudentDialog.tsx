import { useState } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Tabs,
  Tab,
  Box,
} from '@mui/material'
import VisualSelectTab from './AddStudentDialog/VisualSelectTab'
import ImportExcelTab from './AddStudentDialog/ImportExcelTab'

interface AddStudentDialogProps {
  open: boolean
  classId: string
  onClose: () => void
  onSuccess: () => void
}

export default function AddStudentDialog({
  open,
  classId,
  onClose,
  onSuccess,
}: AddStudentDialogProps) {
  const [activeTab, setActiveTab] = useState(0)

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue)
  }

  const handleSuccess = () => {
    onSuccess()
    onClose()
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>Thêm sinh viên vào lớp</DialogTitle>
      <DialogContent dividers>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
          <Tabs value={activeTab} onChange={handleTabChange} aria-label="add students tabs">
            <Tab label="Chọn từ danh sách" />
            <Tab label="Import Excel" />
          </Tabs>
        </Box>

        {activeTab === 0 && (
          <VisualSelectTab classId={classId} onSuccess={handleSuccess} />
        )}
        
        {activeTab === 1 && (
          <ImportExcelTab classId={classId} onSuccess={handleSuccess} />
        )}
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose}>Đóng</Button>
      </DialogActions>
    </Dialog>
  )
}
