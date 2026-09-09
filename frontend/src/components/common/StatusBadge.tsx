import type { StatusTone } from '../../types/status'
import { cn } from '../../utils/cn'

const colors: Record<StatusTone, string> = { neutral: 'bg-slate-100 text-slate-700', info: 'bg-blue-50 text-blue-700', success: 'bg-emerald-50 text-emerald-700', warning: 'bg-amber-50 text-amber-700', danger: 'bg-red-50 text-red-700' }
export function StatusBadge({ label, tone = 'neutral', dot = true }: { label: string; tone?: StatusTone; dot?: boolean }) {
  return <span className={cn('inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold', colors[tone])}>{dot && <span className="size-1.5 rounded-full bg-current" aria-hidden="true" />}{label}</span>
}
