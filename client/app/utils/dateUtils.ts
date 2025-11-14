/**
 * Format ISO datetime string to Asia/Ho_Chi_Minh timezone
 * @param isoString - ISO datetime string from backend (e.g., "2024-11-11T10:30:00Z")
 * @param format - 'short' | 'long' | 'time' (default: 'short')
 * @returns Formatted datetime string in Asia/Ho_Chi_Minh timezone
 */
export function formatDateTime(
  isoString?: string,
  format: 'short' | 'long' | 'time' = 'short'
): string {
  if (!isoString) return ''

  try {
    const date = new Date(isoString)

    // Check if date is valid
    if (isNaN(date.getTime())) {
      return isoString
    }

    // ✅ Use Asia/Ho_Chi_Minh timezone
    const options: Intl.DateTimeFormatOptions = {
      timeZone: 'Asia/Ho_Chi_Minh',  // ✅ Fixed timezone
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false,
    }

    switch (format) {
      case 'short':
        return date.toLocaleString('vi-VN', options)
      
      case 'long':
        return date.toLocaleString('vi-VN', {
          ...options,
          weekday: 'long',
          year: 'numeric',
          month: 'long',
          day: 'numeric',
        })
      
      case 'time':
        return date.toLocaleTimeString('vi-VN', {
          timeZone: 'Asia/Ho_Chi_Minh',  // ✅ Fixed timezone
          hour: '2-digit',
          minute: '2-digit',
          second: '2-digit',
        })

      default:
        return date.toLocaleString('vi-VN', options)
    }
  } catch (error) {
    console.error('Error formatting date:', error)
    return isoString
  }
}

/**
 * Get relative time (e.g., "2 hours ago", "in 3 days")
 * Based on Asia/Ho_Chi_Minh timezone
 */
export function getRelativeTime(isoString?: string): string {
  if (!isoString) return ''

  try {
    const date = new Date(isoString)
    // Get current time in Asia/Ho_Chi_Minh timezone
    const now = new Date()
    const diff = now.getTime() - date.getTime()

    const seconds = Math.floor(diff / 1000)
    const minutes = Math.floor(seconds / 60)
    const hours = Math.floor(minutes / 60)
    const days = Math.floor(hours / 24)

    if (seconds < 60) return 'Vừa xong'
    if (minutes < 60) return `${minutes} phút trước`
    if (hours < 24) return `${hours} giờ trước`
    if (days < 7) return `${days} ngày trước`
    if (days < 30) return `${Math.floor(days / 7)} tuần trước`
    if (days < 365) return `${Math.floor(days / 30)} tháng trước`
    
    return `${Math.floor(days / 365)} năm trước`
  } catch (error) {
    console.error('Error calculating relative time:', error)
    return ''
  }
}

/**
 * Get days until date (in Asia/Ho_Chi_Minh timezone)
 */
export function getDaysUntil(isoString?: string): number | null {
  if (!isoString) return null

  try {
    const date = new Date(isoString)
    const now = new Date()
    const diff = date.getTime() - now.getTime()
    return Math.ceil(diff / (1000 * 60 * 60 * 24))
  } catch (error) {
    console.error('Error calculating days until:', error)
    return null
  }
}