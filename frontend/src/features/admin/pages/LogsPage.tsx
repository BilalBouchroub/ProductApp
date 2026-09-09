import { Download } from 'lucide-react'
import { useMemo, useState } from 'react'
import { toast } from 'sonner'
import { PageHeader } from '../../../components/common/PageHeader'
import { Button } from '../../../components/ui/Button'
import { ErrorState } from '../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { LogDetailsDrawer } from '../components/logs/LogDetailsDrawer'
import { LogFilters, type LogFilterState } from '../components/logs/LogFilters'
import { LogsSummary } from '../components/logs/LogsSummary'
import { LogTable } from '../components/logs/LogTable'
import { useAdminLogs } from '../hooks/useAdminLogs'
import { useAdminUsers } from '../hooks/useAdminUsers'
import { LOG_MODULES, type PlatformLog } from '../types/admin.types'
import { exportLogsToCsv } from '../utils/exportLogsToCsv'

const initialFilters: LogFilterState = { search: '', userId: '', role: '', module: '', action: '', level: '', dateFrom: '', dateTo: '' }
export function LogsPage() {
  const logsQuery = useAdminLogs()
  const usersQuery = useAdminUsers()
  const [filters, setFilters] = useState(initialFilters)
  const [selectedLog, setSelectedLog] = useState<PlatformLog | null>(null)
  const actions = useMemo(() => [...new Set((logsQuery.data ?? []).map((log) => log.action))].sort(), [logsQuery.data])
  const logs = useMemo(() => (logsQuery.data ?? []).filter((log) => {
    const term = filters.search.trim().toLowerCase()
    const timestamp = new Date(log.timestamp).getTime()
    const from = filters.dateFrom ? new Date(`${filters.dateFrom}T00:00:00`).getTime() : null
    const to = filters.dateTo ? new Date(`${filters.dateTo}T23:59:59`).getTime() : null
    return (!term || `${log.userName} ${log.action} ${log.description} ${log.ipAddress}`.toLowerCase().includes(term)) && (!filters.userId || log.userId === filters.userId) && (!filters.role || log.userRole === filters.role) && (!filters.module || log.module === filters.module) && (!filters.action || log.action === filters.action) && (!filters.level || log.level === filters.level) && (from === null || timestamp >= from) && (to === null || timestamp <= to)
  }), [filters, logsQuery.data])
  const handleExport = () => { exportLogsToCsv(logs); toast.success(`${logs.length} logs exportés localement au format CSV.`) }
  return <div className="space-y-6"><PageHeader eyebrow="Administration" title="Logs de la plateforme" description="Analysez les événements, les actions utilisateur et les alertes techniques." actions={<Button variant="secondary" onClick={handleExport} disabled={logs.length === 0}><Download className="size-4" />Exporter CSV</Button>} />{logsQuery.isLoading ? <div className="surface-card p-6"><LoadingSkeleton lines={8} /></div> : logsQuery.isError ? <div className="surface-card"><ErrorState description="Impossible de charger les logs." onRetry={() => logsQuery.refetch()} /></div> : <><LogsSummary logs={logsQuery.data ?? []} /><LogFilters filters={filters} onChange={setFilters} users={usersQuery.data ?? []} actions={actions} modules={LOG_MODULES} /><div className="flex items-center justify-between"><p className="text-sm text-slate-500"><strong className="text-slate-800">{logs.length}</strong> événements trouvés</p><p className="text-xs text-slate-400">Journal d’audit de la plateforme</p></div><LogTable logs={logs} onOpen={setSelectedLog} /></>}<LogDetailsDrawer log={selectedLog} onClose={() => setSelectedLog(null)} /></div>
}
