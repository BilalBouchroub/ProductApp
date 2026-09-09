import { Inbox } from 'lucide-react'
import type { ReactNode } from 'react'

interface EmptyStateProps { title?: string; description?: string; action?: ReactNode }
export function EmptyState({ title = 'Aucune donnée', description = 'Les informations apparaîtront ici lorsqu’elles seront disponibles.', action }: EmptyStateProps) {
  return <div className="flex flex-col items-center px-6 py-12 text-center"><span className="mb-4 rounded-2xl bg-slate-100 p-3 text-slate-500"><Inbox className="size-6" /></span><h3 className="font-semibold text-slate-900">{title}</h3><p className="mt-1 max-w-sm text-sm text-slate-500">{description}</p>{action && <div className="mt-5">{action}</div>}</div>
}
