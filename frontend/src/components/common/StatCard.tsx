import { ArrowDownRight, ArrowUpRight, type LucideIcon } from 'lucide-react'
import { cn } from '../../utils/cn'

interface StatCardProps { label: string; value: string; detail: string; trend?: 'up' | 'down' | 'neutral'; icon: LucideIcon; accent?: 'blue' | 'emerald' | 'amber' | 'violet' }
const accents = { blue: 'bg-blue-50 text-blue-600', emerald: 'bg-emerald-50 text-emerald-600', amber: 'bg-amber-50 text-amber-600', violet: 'bg-violet-50 text-violet-600' }
export function StatCard({ label, value, detail, trend = 'neutral', icon: Icon, accent = 'blue' }: StatCardProps) {
  return <article className="surface-card p-5"><div className="flex items-start justify-between"><div><p className="text-sm font-medium text-slate-500">{label}</p><p className="mt-2 text-2xl font-bold tracking-tight text-slate-950">{value}</p></div><span className={cn('rounded-xl p-2.5', accents[accent])}><Icon className="size-5" /></span></div><p className={cn('mt-4 flex items-center gap-1 text-xs font-medium', trend === 'up' && 'text-emerald-600', trend === 'down' && 'text-red-600', trend === 'neutral' && 'text-slate-500')}>{trend === 'up' && <ArrowUpRight className="size-3.5" />}{trend === 'down' && <ArrowDownRight className="size-3.5" />}{detail}</p></article>
}
