import type { PlatformLog } from '../types/admin.types'

function escapeCsv(value: string | null): string {
  return `"${(value ?? '').replaceAll('"', '""')}"`
}

export function exportLogsToCsv(logs: readonly PlatformLog[]): void {
  const header = ['Date', 'Utilisateur', 'Rôle', 'Action', 'Module', 'Niveau', 'Description', 'Adresse IP', 'Type entité', 'ID entité']
  const rows = logs.map((log) => [log.timestamp, log.userName, log.userRole, log.action, log.module, log.level, log.description, log.ipAddress, log.entityType, log.entityId].map(escapeCsv).join(','))
  const blob = new Blob([`\uFEFF${header.map(escapeCsv).join(',')}\n${rows.join('\n')}`], { type: 'text/csv;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `productapp-logs-${new Date().toISOString().slice(0, 10)}.csv`
  link.click()
  URL.revokeObjectURL(url)
}
