import { FilterBar } from '../../../../components/common/FilterBar'
import { SearchInput } from '../../../../components/common/SearchInput'

export interface UserFilterState { search: string; role: string; status: string }

interface UserFiltersProps { filters: UserFilterState; onChange: (filters: UserFilterState) => void }
export function UserFilters({ filters, onChange }: UserFiltersProps) {
  const activeCount = Number(Boolean(filters.search)) + Number(Boolean(filters.role)) + Number(Boolean(filters.status))
  return <FilterBar activeCount={activeCount} onReset={() => onChange({ search: '', role: '', status: '' })}><div className="min-w-[min(100%,18rem)] flex-1"><SearchInput value={filters.search} onChange={(search) => onChange({ ...filters, search })} placeholder="Nom ou adresse e-mail…" label="Rechercher un utilisateur" /></div><label className="sr-only" htmlFor="user-role-filter">Filtrer par rôle</label><select id="user-role-filter" value={filters.role} onChange={(event) => onChange({ ...filters, role: event.target.value })} className="field-control w-full sm:w-52"><option value="">Tous les rôles</option><option value="Administrator">Administrateur</option><option value="ProductionManager">Responsable Production</option><option value="CommercialManager">Responsable Commercial</option></select><label className="sr-only" htmlFor="user-status-filter">Filtrer par statut</label><select id="user-status-filter" value={filters.status} onChange={(event) => onChange({ ...filters, status: event.target.value })} className="field-control w-full sm:w-44"><option value="">Tous les statuts</option><option value="Active">Actifs</option><option value="Inactive">Inactifs</option><option value="Suspended">Suspendus</option></select></FilterBar>
}
