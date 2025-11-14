import { API } from '../api';

export interface AdminUser {
  id: string;
  username: string;
  email: string;
  fullName: string;
  role: 'Admin' | 'Teacher' | 'Student';
  status: 'Active' | 'Inactive' | 'Pending';
  createdAt: string;
  updatedAt: string;
}

export interface AdminUserFilters {
  pageNumber?: number;
  pageSize?: number;
  role?: 'Admin' | 'Teacher' | 'Student';
  status?: 'Active' | 'Inactive' | 'Pending';
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Admin User Management
export const getAllUsersAdmin = async (
  filters: AdminUserFilters = {}
): Promise<PagedResponse<AdminUser>> => {
  const params = new URLSearchParams();
  
  if (filters.pageNumber) params.append('pageNumber', filters.pageNumber.toString());
  if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
  if (filters.role) params.append('role', filters.role);
  if (filters.status) params.append('status', filters.status);

  const response = await API.get(`/api/v1/users?${params.toString()}`);
  return response.data.data;
};

export const updateUserStatus = async (
  userId: string,
  status: 'Active' | 'Inactive' | 'Pending'
): Promise<void> => {
  await API.patch('/api/v1/users/update-status', { userId, status });
};

export const updateUserRole = async (
  userId: string,
  role: 'Admin' | 'Teacher' | 'Student'
): Promise<void> => {
  await API.put(`/api/v1/users/${userId}/role`, { role });
};

export const deleteUserAdmin = async (userId: string): Promise<void> => {
  await API.delete(`/api/v1/users/${userId}`);
};

// Dashboard Statistics
export interface DashboardStats {
  totalUsers: number;
  totalStudents: number;
  totalTeachers: number;
  totalClasses: number;
  totalAssignments: number;
  activeUsers: number;
  pendingUsers: number;
}

export const getDashboardStats = async (): Promise<DashboardStats> => {
  const response = await API.get('/api/v1/admin/dashboard/stats');
  return response.data.data;
};

// Admin Class Management
export interface AdminClass {
  id: string;
  name: string;
  description: string;
  teacherId: string;
  teacherName: string;
  studentCount: number;
  status: 'Active' | 'Archived';
  createdAt: string;
  updatedAt: string;
}

export const getAllClassesAdmin = async (
  pageNumber = 1,
  pageSize = 10
): Promise<PagedResponse<AdminClass>> => {
  const response = await API.get(
    `/api/v1/classes/all?pageNumber=${pageNumber}&pageSize=${pageSize}`
  );
  return response.data.data;
};

export const archiveClass = async (classId: string): Promise<void> => {
  await API.patch(`/api/v1/classes/${classId}/archive`);
};

export const unarchiveClass = async (classId: string): Promise<void> => {
  await API.patch(`/api/v1/classes/${classId}/unarchive`);
};

// Admin Class Management - NEW ENDPOINTS
export interface AdminClassDetail {
  classId: string;
  name: string;
  description: string;
  classCode: string;
  teacherId: string;
  teacherName: string;
  teacherEmail: string;
  studentCount: number;
  activeStudentCount: number;
  assignmentCount: number;
  submissionCount: number;
  isActive: boolean;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
  archivedAt?: string;
}

export interface ClassStatistics {
  totalClasses: number;
  activeClasses: number;
  archivedClasses: number;
  totalStudentsEnrolled: number;
  totalTeachers: number;
  averageStudentsPerClass: number;
  mostPopularClass?: {
    classId: string;
    className: string;
    studentCount: number;
  };
  topTeachers?: Array<{
    teacherId: string;
    teacherName: string;
    classCount: number;
  }>;
}

// Get all classes for admin with detailed info
export const getAllClassesForAdmin = async (
  pageNumber = 1,
  pageSize = 10,
  filters?: {
    teacherId?: string;
    isActive?: boolean;
    isArchived?: boolean;
    searchTerm?: string;
  }
): Promise<PagedResponse<AdminClassDetail>> => {
  const params = new URLSearchParams();
  params.append('pageNumber', pageNumber.toString());
  params.append('pageSize', pageSize.toString());
  
  if (filters?.teacherId) params.append('teacherId', filters.teacherId);
  if (filters?.isActive !== undefined) params.append('isActive', filters.isActive.toString());
  if (filters?.isArchived !== undefined) params.append('isArchived', filters.isArchived.toString());
  if (filters?.searchTerm) params.append('searchTerm', filters.searchTerm);

  const response = await API.get(`/api/v1/admin/classes?${params.toString()}`);
  return response.data.data;
};

// Get class detail for admin
export const getClassDetailForAdmin = async (classId: string): Promise<AdminClassDetail> => {
  const response = await API.get(`/api/v1/admin/classes/${classId}`);
  return response.data.data;
};

// Archive class
export const archiveClassAdmin = async (
  classId: string,
  reason?: string
): Promise<void> => {
  await API.patch(`/api/v1/admin/classes/${classId}/archive`, { classId, reason });
};

// Unarchive class
export const unarchiveClassAdmin = async (classId: string): Promise<void> => {
  await API.patch(`/api/v1/admin/classes/${classId}/unarchive`);
};

// Update class by admin
export const updateClassByAdmin = async (
  classId: string,
  data: {
    name: string;
    description: string;
    teacherId: string;
    isActive: boolean;
  }
): Promise<void> => {
  await API.put(`/api/v1/admin/classes/${classId}`, {
    classId,
    ...data,
  });
};

// Delete class permanently (admin only)
export const deleteClassByAdmin = async (classId: string): Promise<void> => {
  await API.delete(`/api/v1/admin/classes/${classId}`);
};

// Get class statistics
export const getClassStatistics = async (): Promise<ClassStatistics> => {
  const response = await API.get('/api/v1/admin/classes/statistics');
  return response.data.data;
};

// Bulk actions
export interface BulkActionRequest {
  action: 'archive' | 'unarchive' | 'delete';
  classIds: string[];
  reason?: string;
}

export const bulkActionClasses = async (request: BulkActionRequest): Promise<any> => {
  const response = await API.post('/api/v1/admin/classes/bulk-action', request);
  return response.data.data;
};

// Get students in a class
export const getClassStudents = async (
  classId: string,
  pageNumber = 1,
  pageSize = 20,
  searchTerm?: string
): Promise<PagedResponse<any>> => {
  const params = new URLSearchParams();
  params.append('pageNumber', pageNumber.toString());
  params.append('pageSize', pageSize.toString());
  if (searchTerm) params.append('searchTerm', searchTerm);

  const response = await API.get(`/api/v1/admin/classes/${classId}/students?${params.toString()}`);
  return response.data.data;
};
