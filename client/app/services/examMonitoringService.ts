import { API } from '~/api'

export type ActivityType = 
  | 'TAB_SWITCH'
  | 'COPY_PASTE' 
  | 'IDLE_START'
  | 'IDLE_END'
  | 'MOUSE_ACTIVE'
  | 'KEYBOARD_ACTIVE'
  | 'FOCUS_LOST'
  | 'FOCUS_GAINED'

export interface ActivityLogData {
  activityType: ActivityType
  timestamp: string // ISO string
  metadata?: Record<string, any>
  suspicionLevel?: number // 0-100
}

/**
 * Increment tab switch count for an assignment
 */
export async function incrementTabSwitch(assignmentId: string): Promise<void> {
  try {
    await API.post(`/api/v1/assignments/${assignmentId}/student/increment-tab-switch`)
  } catch (error) {
    console.error('Failed to increment tab switch count:', error)
    // Don't throw - we don't want to block the user
  }
}

/**
 * Increment AI detection count for an assignment
 */
export async function incrementAIDetection(assignmentId: string): Promise<void> {
  try {
    await API.post(`/api/v1/assignments/${assignmentId}/student/increment-ai-detection`)
  } catch (error) {
    console.error('Failed to increment AI detection count:', error)
    // Don't throw - we don't want to block the user
  }
}

/**
 * Log detailed activity for exam monitoring
 */
export async function logActivity(assignmentId: string, activityData: ActivityLogData): Promise<void> {
  try {
    // Convert metadata object to JSON string and use correct casing for backend
    const payload = {
      ActivityType: activityData.activityType,
      Timestamp: activityData.timestamp,
      Metadata: activityData.metadata ? JSON.stringify(activityData.metadata) : null,
      SuspicionLevel: activityData.suspicionLevel || 0
    }
    
    await API.post(`/api/v1/assignments/${assignmentId}/student/log-activity`, payload)
  } catch (error) {
    console.error('Failed to log activity:', error)
    // Don't throw - we don't want to block the user
  }
}

/**
 * Batch log multiple activities (more efficient)
 */
export async function logActivitiesBatch(assignmentId: string, activities: ActivityLogData[]): Promise<void> {
  try {
    // Convert metadata objects to JSON strings and use correct casing for backend
    const payload = {
      Activities: activities.map(activity => ({
        ActivityType: activity.activityType,
        Timestamp: activity.timestamp,
        Metadata: activity.metadata ? JSON.stringify(activity.metadata) : null,
        SuspicionLevel: activity.suspicionLevel || 0
      }))
    }
    
    await API.post(`/api/v1/assignments/${assignmentId}/student/log-activities-batch`, payload)
  } catch (error) {
    console.error('Failed to log activities batch:', error)
    // Don't throw - we don't want to block the user
  }
}
