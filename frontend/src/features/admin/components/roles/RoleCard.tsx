import { Factory, ShieldCheck, ShoppingBag } from 'lucide-react'
import type { RoleDefinition } from '../../types/admin.types'
import { PermissionList } from './PermissionList'
import { UserRoleBadge } from '../users/UserRoleBadge'

const roleStyles = {
  Administrator: { icon: ShieldCheck, gradient: 'from-violet-600 to-indigo-700', background: 'bg-violet-50', text: 'text-violet-700' },
  ProductionManager: { icon: Factory, gradient: 'from-blue-600 to-cyan-700', background: 'bg-blue-50', text: 'text-blue-700' },
  CommercialManager: { icon: ShoppingBag, gradient: 'from-emerald-600 to-teal-700', background: 'bg-emerald-50', text: 'text-emerald-700' },
} as const

export function RoleCard({ definition }: { definition: RoleDefinition }) {
  const style = roleStyles[definition.role]
  const Icon = style.icon
  return <article className="surface-card overflow-hidden"><div className={`h-2 bg-gradient-to-r ${style.gradient}`} /><div className="p-5 sm:p-6"><div className="flex items-start justify-between gap-4"><span className={`rounded-2xl p-3 ${style.background} ${style.text}`}><Icon className="size-6" /></span><span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">{definition.userCount} utilisateurs</span></div><div className="mt-5"><UserRoleBadge role={definition.role} /><h2 className="mt-3 text-lg font-bold text-slate-950">{definition.title}</h2><p className="mt-2 min-h-12 text-sm leading-6 text-slate-500">{definition.description}</p></div><div className="my-5 h-px bg-slate-200" /><h3 className="mb-4 text-xs font-bold tracking-wider text-slate-500 uppercase">Permissions accordées</h3><PermissionList permissions={definition.permissions} /></div></article>
}
