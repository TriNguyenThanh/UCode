import { useEffect, useCallback, useRef } from 'react'
import { incrementTabSwitch, logActivity, logActivitiesBatch, type ActivityLogData } from '~/services/examMonitoringService'

interface UseExamMonitoringOptions {
  assignmentId: string
  isExamination: boolean
  enabled: boolean
}

/**
 * Hook to monitor comprehensive exam activities including:
 * - Tab switches and focus changes
 * - Mouse and keyboard activity
 * - Copy/paste events
 * - Idle detection
 */
export function useExamMonitoring({ assignmentId, isExamination, enabled }: UseExamMonitoringOptions) {
  const hasStartedRef = useRef(false)
  const tabSwitchTimeoutRef = useRef<NodeJS.Timeout | null>(null)
  const idleTimeoutRef = useRef<NodeJS.Timeout | null>(null)
  const activityBufferRef = useRef<ActivityLogData[]>([])
  const flushIntervalRef = useRef<NodeJS.Timeout | null>(null)
  const lastActivityTimeRef = useRef<number>(Date.now())
  const isIdleRef = useRef(false)

  // Flush activity buffer to server
  const flushActivityBuffer = useCallback(() => {
    if (activityBufferRef.current.length > 0) {
      // DISABLED: Activity logging is too heavy for production
      // logActivitiesBatch(assignmentId, activityBufferRef.current)
      activityBufferRef.current = []
    }
  }, [assignmentId])

  // Add activity to buffer
  const addActivity = useCallback((activityType: ActivityLogData['activityType'], metadata?: Record<string, any>, suspicionLevel = 0) => {
    if (!enabled || !isExamination || !hasStartedRef.current) return

    const activity: ActivityLogData = {
      activityType,
      timestamp: new Date().toISOString(),
      metadata,
      suspicionLevel
    }

    activityBufferRef.current.push(activity)

    // Flush if buffer gets too large (>20 events)
    if (activityBufferRef.current.length >= 20) {
      flushActivityBuffer()
    }
  }, [enabled, isExamination, flushActivityBuffer])

  // Handle visibility change (tab switch)
  const handleVisibilityChange = useCallback(() => {
    if (!enabled || !isExamination || !hasStartedRef.current) return

    if (document.hidden) {
      // Student switched tabs or minimized window
      tabSwitchTimeoutRef.current = setTimeout(() => {
        incrementTabSwitch(assignmentId)
        addActivity('TAB_SWITCH', { hidden: true }, 50)
        addActivity('FOCUS_LOST')
      }, 500)
    } else {
      // Student came back
      if (tabSwitchTimeoutRef.current) {
        clearTimeout(tabSwitchTimeoutRef.current)
        tabSwitchTimeoutRef.current = null
      }
      addActivity('FOCUS_GAINED')
    }
  }, [assignmentId, isExamination, enabled, addActivity])

  // Handle window blur
  const handleBlur = useCallback(() => {
    if (!enabled || !isExamination || !hasStartedRef.current) return

    tabSwitchTimeoutRef.current = setTimeout(() => {
      incrementTabSwitch(assignmentId)
      addActivity('TAB_SWITCH', { type: 'blur' }, 50)
      addActivity('FOCUS_LOST')
    }, 1000)
  }, [assignmentId, isExamination, enabled, addActivity])

  // Handle window focus
  const handleFocus = useCallback(() => {
    if (!enabled || !isExamination) return

    if (tabSwitchTimeoutRef.current) {
      clearTimeout(tabSwitchTimeoutRef.current)
      tabSwitchTimeoutRef.current = null
    }
    
    if (hasStartedRef.current) {
      addActivity('FOCUS_GAINED')
    }
  }, [isExamination, enabled, addActivity])

  // Handle mouse movement
  const handleMouseMove = useCallback(() => {
    if (!enabled || !isExamination || !hasStartedRef.current) return

    const now = Date.now()
    const timeSinceLastActivity = now - lastActivityTimeRef.current
    
    // Only log if it's been more than 5 seconds since last activity
    if (timeSinceLastActivity > 5000) {
      addActivity('MOUSE_ACTIVE')
      lastActivityTimeRef.current = now
      
      // If was idle, mark as no longer idle
      if (isIdleRef.current) {
        isIdleRef.current = false
        addActivity('IDLE_END', { idleDuration: timeSinceLastActivity })
      }
    }

    // Reset idle timeout
    if (idleTimeoutRef.current) {
      clearTimeout(idleTimeoutRef.current)
    }
    idleTimeoutRef.current = setTimeout(() => {
      if (!isIdleRef.current) {
        isIdleRef.current = true
        addActivity('IDLE_START', { startTime: new Date().toISOString() }, 30)
      }
    }, 60000) // 1 minute of inactivity = idle
  }, [enabled, isExamination, addActivity])

  // Handle keyboard activity
  const handleKeyDown = useCallback(() => {
    if (!enabled || !isExamination || !hasStartedRef.current) return

    const now = Date.now()
    const timeSinceLastActivity = now - lastActivityTimeRef.current
    
    // Only log if it's been more than 5 seconds since last activity
    if (timeSinceLastActivity > 5000) {
      addActivity('KEYBOARD_ACTIVE')
      lastActivityTimeRef.current = now
      
      // If was idle, mark as no longer idle
      if (isIdleRef.current) {
        isIdleRef.current = false
        addActivity('IDLE_END', { idleDuration: timeSinceLastActivity })
      }
    }

    // Reset idle timeout
    if (idleTimeoutRef.current) {
      clearTimeout(idleTimeoutRef.current)
    }
    idleTimeoutRef.current = setTimeout(() => {
      if (!isIdleRef.current) {
        isIdleRef.current = true
        addActivity('IDLE_START', { startTime: new Date().toISOString() }, 30)
      }
    }, 60000)
  }, [enabled, isExamination, addActivity])

  // Handle paste events (detect external code paste)
  const handlePaste = useCallback((e: ClipboardEvent) => {
    if (!enabled || !isExamination || !hasStartedRef.current) return

    const pastedText = e.clipboardData?.getData('text') || ''
    const textLength = pastedText.length
    
    // Suspicious if pasting a large amount of code
    const suspicionLevel = textLength > 100 ? 70 : textLength > 50 ? 40 : 20
    
    addActivity('COPY_PASTE', {
      textLength,
      timestamp: new Date().toISOString(),
      preview: pastedText.substring(0, 100) // First 100 chars for review
    }, suspicionLevel)
  }, [enabled, isExamination, addActivity])

  // Start monitoring
  const startMonitoring = useCallback(() => {
    hasStartedRef.current = true
    lastActivityTimeRef.current = Date.now()
    
    // Start periodic flush (every 30 seconds)
    flushIntervalRef.current = setInterval(() => {
      flushActivityBuffer()
    }, 30000)
  }, [flushActivityBuffer])

  // Stop monitoring
  const stopMonitoring = useCallback(() => {
    hasStartedRef.current = false
    
    // Flush remaining activities
    flushActivityBuffer()
    
    // Clear all timeouts
    if (tabSwitchTimeoutRef.current) {
      clearTimeout(tabSwitchTimeoutRef.current)
      tabSwitchTimeoutRef.current = null
    }
    if (idleTimeoutRef.current) {
      clearTimeout(idleTimeoutRef.current)
      idleTimeoutRef.current = null
    }
    if (flushIntervalRef.current) {
      clearInterval(flushIntervalRef.current)
      flushIntervalRef.current = null
    }
  }, [flushActivityBuffer])

  // Setup event listeners
  useEffect(() => {
    if (!enabled || !isExamination) return

    document.addEventListener('visibilitychange', handleVisibilityChange)
    window.addEventListener('blur', handleBlur)
    window.addEventListener('focus', handleFocus)
    document.addEventListener('mousemove', handleMouseMove)
    document.addEventListener('keydown', handleKeyDown)
    document.addEventListener('paste', handlePaste as EventListener)

    return () => {
      document.removeEventListener('visibilitychange', handleVisibilityChange)
      window.removeEventListener('blur', handleBlur)
      window.removeEventListener('focus', handleFocus)
      document.removeEventListener('mousemove', handleMouseMove)
      document.removeEventListener('keydown', handleKeyDown)
      document.removeEventListener('paste', handlePaste as EventListener)
      
      // Cleanup on unmount
      if (tabSwitchTimeoutRef.current) {
        clearTimeout(tabSwitchTimeoutRef.current)
      }
      if (idleTimeoutRef.current) {
        clearTimeout(idleTimeoutRef.current)
      }
      if (flushIntervalRef.current) {
        clearInterval(flushIntervalRef.current)
      }
      
      // Final flush
      flushActivityBuffer()
    }
  }, [enabled, isExamination, handleVisibilityChange, handleBlur, handleFocus, handleMouseMove, handleKeyDown, handlePaste, flushActivityBuffer])

  return {
    startMonitoring,
    stopMonitoring
  }
}
