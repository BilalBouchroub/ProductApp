import { RotateCcw, SlidersHorizontal } from 'lucide-react'
import type { ReactNode } from 'react'
import { Button } from '../ui/Button'

interface FilterBarProps { children: ReactNode; onReset?: () => void; activeCount?: number }
export function FilterBar({ children, onReset, activeCount = 0 }: FilterBarProps) {
  return <div className="flex flex-col gap-3 rounded-2xl border border-slate-200 bg-white p-3 sm:flex-row sm:items-center"><div className="flex items-center gap-2 text-sm font-semibold text-slate-700"><SlidersHorizontal className="size-4" />Filtres{activeCount > 0 && <span className="rounded-full bg-blue-600 px-1.5 py-0.5 text-[10px] text-white">{activeCount}</span>}</div><div className="flex min-w-0 flex-1 flex-wrap gap-2">{children}</div>{onReset && <Button variant="ghost" size="sm" onClick={onReset}><RotateCcw className="size-4" />Réinitialiser</Button>}</div>
}
