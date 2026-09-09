import type { UserRole } from '../../types/auth'
import { cn } from '../../utils/cn'

const labels: Record<UserRole, string> = { Administrator: 'Administrateur', ProductionManager: 'Responsable Production', CommercialManager: 'Responsable Commercial' }
const colors: Record<UserRole, string> = { Administrator: 'bg-violet-50 text-violet-700 ring-violet-200', ProductionManager: 'bg-blue-50 text-blue-700 ring-blue-200', CommercialManager: 'bg-emerald-50 text-emerald-700 ring-emerald-200' }

export function RoleBadge({ role, compact = false }: { role: UserRole; compact?: boolean }) {
  return <span className={cn('inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold ring-1 ring-inset', colors[role])}>{compact ? labels[role].replace('Responsable ', '') : labels[role]}</span>
}
