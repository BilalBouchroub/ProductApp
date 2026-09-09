import { useMemo } from 'react'
import type { ColumnDef } from '@tanstack/react-table'
import { DataTable } from '../../../../components/common/DataTable'
import { UserRoleBadge } from '../users/UserRoleBadge'
import { LogLevelBadge } from '../logs/LogLevelBadge'
import type { PlatformLog } from '../../types/admin.types'
import { formatAdminDateTime } from '../../utils/adminFormatters'

export function RecentActivityTable({ logs }: { logs: readonly PlatformLog[] }) {
  const columns = useMemo<ColumnDef<PlatformLog>[]>(() => [
    { accessorKey: 'userName', header: 'Utilisateur', cell: ({ row }) => <div><p className="font-semibold text-slate-800">{row.original.userName}</p><div className="mt-1"><UserRoleBadge role={row.original.userRole} compact /></div></div> },
    { accessorKey: 'action', header: 'Action', cell: ({ row }) => <span className="font-mono text-xs text-slate-700">{row.original.action}</span> },
    { accessorKey: 'module', header: 'Module' },
    { accessorKey: 'timestamp', header: 'Date', cell: ({ row }) => formatAdminDateTime(row.original.timestamp) },
    { accessorKey: 'level', header: 'Niveau', cell: ({ row }) => <LogLevelBadge level={row.original.level} /> },
  ], [])
  return <DataTable data={logs} columns={columns} emptyTitle="Aucune activité récente" />
}
