import { Info, ShieldCheck } from 'lucide-react'
import { PageHeader } from '../../../components/common/PageHeader'
import { RoleCard } from '../components/roles/RoleCard'
import { mockRoles } from '../mocks/roles.mock'

export function RolesPage() {
  return <div className="space-y-7"><PageHeader eyebrow="Administration" title="Rôles et permissions" description="Consultez les responsabilités et les autorisations associées à chaque espace professionnel." /><div className="flex items-start gap-3 rounded-2xl border border-blue-100 bg-blue-50 p-4 text-sm leading-6 text-blue-800"><Info className="mt-0.5 size-5 shrink-0" /><div><p className="font-semibold">Configuration en lecture seule</p><p>La gestion dynamique des permissions sera activée lorsqu’elle sera disponible dans le backend. Aucun droit réel n’est modifié ici.</p></div></div><section aria-label="Rôles disponibles" className="grid gap-6 xl:grid-cols-3">{mockRoles.map((role) => <RoleCard key={role.role} definition={role} />)}</section><div className="surface-card flex flex-col items-center p-8 text-center"><span className="rounded-2xl bg-slate-100 p-3 text-slate-600"><ShieldCheck className="size-7" /></span><h2 className="mt-4 font-semibold text-slate-950">Principe du moindre privilège</h2><p className="mt-2 max-w-2xl text-sm leading-6 text-slate-500">Chaque utilisateur doit disposer uniquement des autorisations nécessaires à sa fonction. Les changements de rôle sont tracés dans les logs de la plateforme.</p></div></div>
}
