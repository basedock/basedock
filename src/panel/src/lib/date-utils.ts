/**
 * Format a date as a relative time string
 * Examples: "just now", "2 minutes ago", "3 hours ago", "2 days ago"
 */
export function formatRelativeTime(date: string | Date | null | undefined): string {
  if (!date) return 'Never'

  const now = new Date()
  const then = typeof date === 'string' ? new Date(date) : date
  const diffMs = now.getTime() - then.getTime()
  const diffSecs = Math.floor(diffMs / 1000)
  const diffMins = Math.floor(diffSecs / 60)
  const diffHours = Math.floor(diffMins / 60)
  const diffDays = Math.floor(diffHours / 24)

  if (diffSecs < 10) return 'just now'
  if (diffSecs < 60) return `${diffSecs} seconds ago`
  if (diffMins === 1) return '1 minute ago'
  if (diffMins < 60) return `${diffMins} minutes ago`
  if (diffHours === 1) return '1 hour ago'
  if (diffHours < 24) return `${diffHours} hours ago`
  if (diffDays === 1) return '1 day ago'
  if (diffDays < 30) return `${diffDays} days ago`

  // For dates older than 30 days, show absolute date
  return formatAbsoluteDate(date)
}

/**
 * Format a date as an absolute date string
 * Example: "Dec 11, 2025 at 3:45 PM"
 */
export function formatAbsoluteDate(date: string | Date | null | undefined): string {
  if (!date) return 'Never'

  const d = typeof date === 'string' ? new Date(date) : date

  return d.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  })
}

/**
 * Calculate uptime from a deployment date
 * Returns a human-readable uptime string
 */
export function calculateUptime(deployedAt: string | Date | null | undefined): string {
  if (!deployedAt) return 'Not deployed'

  const now = new Date()
  const deployed = typeof deployedAt === 'string' ? new Date(deployedAt) : deployedAt
  const diffMs = now.getTime() - deployed.getTime()

  // If deployed in the future or very recent, return "Just deployed"
  if (diffMs < 0 || diffMs < 60000) return 'Just deployed'

  const diffSecs = Math.floor(diffMs / 1000)
  const diffMins = Math.floor(diffSecs / 60)
  const diffHours = Math.floor(diffMins / 60)
  const diffDays = Math.floor(diffHours / 24)

  if (diffDays > 0) {
    const hours = diffHours % 24
    if (diffDays === 1 && hours === 0) return '1 day'
    if (hours === 0) return `${diffDays} days`
    return `${diffDays}d ${hours}h`
  }

  if (diffHours > 0) {
    const mins = diffMins % 60
    if (diffHours === 1 && mins === 0) return '1 hour'
    if (mins === 0) return `${diffHours} hours`
    return `${diffHours}h ${mins}m`
  }

  return `${diffMins} minutes`
}
