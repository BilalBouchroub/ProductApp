import type { ReactNode } from 'react'
interface PageHeaderProps { eyebrow?: string; title: string; description?: string; actions?: ReactNode }
export function PageHeader({ eyebrow, title, description, actions }: PageHeaderProps) {
  return <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end"><div>{eyebrow && <p className="mb-1 text-xs font-semibold tracking-widest text-blue-600 uppercase">{eyebrow}</p>}<h1 className="text-2xl font-bold tracking-tight text-slate-950 sm:text-3xl">{title}</h1>{description && <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-500">{description}</p>}</div>{actions && <div className="flex shrink-0 flex-wrap gap-2">{actions}</div>}</div>
}
