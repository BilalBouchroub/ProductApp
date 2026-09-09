import type { ReactNode } from 'react'

interface ChartCardProps { title: string; description?: string; children: ReactNode; action?: ReactNode }
export function ChartCard({ title, description, children, action }: ChartCardProps) {
  return <section className="surface-card min-w-0 p-5 sm:p-6"><div className="mb-6 flex items-start justify-between gap-3"><div><h2 className="font-semibold text-slate-950">{title}</h2>{description && <p className="mt-1 text-xs text-slate-500">{description}</p>}</div>{action}</div><div className="h-72 min-w-0">{children}</div></section>
}
