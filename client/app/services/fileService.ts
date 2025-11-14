import { API } from '../api'
import type { ApiResponse } from '~/types'
import { handleApiError } from './utils'

export interface FileUploadResponse {
  fileKey: string
  fileName: string
  fileUrl: string
  fileSizeBytes: number
  contentType: string
  category: string
  uploadedAt: string
}

export type FileCategory = 
  | 'AssignmentDocument' 
  | 'CodeSubmission' 
  | 'Image' 
  | 'Avatar' 
  | 'TestCase' 
  | 'Reference' 
  | 'Document'

/**
 * Upload file to server
 * @param file - File object to upload
 * @param category - File category (default: 'Image')
 * @returns Promise with upload response containing file URL
 */
export const uploadFile = async (
  file: File, 
  category: FileCategory = 'Image'
): Promise<FileUploadResponse> => {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('category', category)

  const response = await API.post<ApiResponse<FileUploadResponse>>(
    '/api/files/upload',
    formData,
    {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }
  )

  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Failed to upload file')
  }

  return response.data.data
}

/**
 * Delete file from server
 * @param fileKey - The file key/path to delete
 * @returns Promise with boolean result
 */
export const deleteFile = async (fileKey: string): Promise<boolean> => {
  const response = await API.delete<ApiResponse<boolean>>(`/api/files/${fileKey}`)
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to delete file')
  }

  return response.data.data || false
}

/**
 * Get presigned URL for file
 * @param fileKey - The file key/path
 * @param expirationMinutes - URL expiration time in minutes (default: 60)
 * @returns Promise with presigned URL
 */
export const getPresignedUrl = async (
  fileKey: string, 
  expirationMinutes: number = 60
): Promise<string> => {
  const response = await API.post<ApiResponse<string>>('/api/files/presigned-url', {
    key: fileKey,
    expirationMinutes,
  })

  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Failed to get presigned URL')
  }

  return response.data.data
}

/**
 * Check if file exists
 * @param fileKey - The file key/path
 * @returns Promise with boolean result
 */
export const fileExists = async (fileKey: string): Promise<boolean> => {
  const response = await API.get<ApiResponse<boolean>>(`/api/files/exists/${fileKey}`)
  
  if (!response.data.success) {
    throw new Error(response.data.message || 'Failed to check file existence')
  }

  return response.data.data || false
}
