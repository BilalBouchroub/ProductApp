import { Ban, Eye, KeyRound, Pencil, ShieldCheck, Trash2, UserCheck } from 'lucide-react'
import type { AdminUser } from '../../types/admin.types'
import { formatAdminDate } from '../../utils/adminFormatters'
import { UserRoleBadge } from './UserRoleBadge'
import { UserStatusBadge } from './UserStatusBadge'

type UserAction = 'status' | 'role' | 'password' | 'delete'

interface UserTableProps {
  users: readonly AdminUser[]
  pendingAction?: { id: string; action: UserAction } | null
  onDelete: (user: AdminUser) => void
  onToggleStatus: (user: AdminUser) => void
  onChangeRole: (user: AdminUser) => void
  onResetPassword: (user: AdminUser) => void
}

const linkClass = 'inline-flex h-9 items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100'
const actionClass = 'inline-flex h-9 items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100 disabled:cursor-wait disabled:opacity-50'
const dangerClass = 'inline-flex h-9 items-center justify-center gap-2 rounded-lg border border-red-200 bg-white px-3 text-sm font-semibold text-red-700 transition hover:bg-red-50 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-red-100 disabled:cursor-wait disabled:opacity-50'

export function UserTable({
  users,
  pendingAction = null,
  onDelete,
  onToggleStatus,
  onChangeRole,
  onResetPassword,
}: UserTableProps) {
  if (users.length === 0) {
    return <div className="rounded-2xl border border-slate-200 bg-white p-8 text-center text-sm text-slate-500">Aucun utilisateur correspondant</div>
  }

  const pending = (user: AdminUser, action: UserAction) =>
    pendingAction?.id === user.id && pendingAction.action === action

  return <section aria-label="Liste des utilisateurs" className="space-y-3">
    {users.map((user) => <article key={user.id} className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
        <div className="flex min-w-0 items-center gap-3">
          <span className="grid size-11 shrink-0 place-items-center rounded-xl bg-slate-900 text-sm font-bold text-white">
            {user.firstName.charAt(0)}{user.lastName.charAt(0)}
          </span>
          <div className="min-w-0">
            <p className="truncate font-semibold text-slate-950">{user.fullName}</p>
            <p className="truncate text-sm text-slate-500">{user.email}</p>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2 text-xs text-slate-500">
          <UserRoleBadge role={user.role} compact />
          <UserStatusBadge status={user.status} />
          <span>Créé le {formatAdminDate(user.createdAt)}</span>
          <span>Dernière connexion : {formatAdminDate(user.lastLoginAt)}</span>
        </div>
      </div>

      <div className="mt-4 flex flex-wrap gap-2 border-t border-slate-100 pt-4" aria-label={`Actions pour ${user.fullName}`}>
        <a href={`/admin/users/${user.id}`} className={linkClass} aria-label={`Consulter ${user.fullName}`}><Eye className="size-4" />Voir</a>
        <a href={`/admin/users/${user.id}/edit`} className={linkClass} aria-label={`Modifier ${user.fullName}`}><Pencil className="size-4" />Modifier</a>
        <button type="button" className={actionClass} disabled={pending(user, 'status')} onClick={() => onToggleStatus(user)} aria-label={`${user.status === 'Active' ? 'Désactiver' : 'Activer'} ${user.fullName}`}>
          {user.status === 'Active' ? <Ban className="size-4" /> : <UserCheck className="size-4" />}
          {user.status === 'Active' ? 'Désactiver' : 'Activer'}
        </button>
        <button type="button" className={actionClass} disabled={pending(user, 'role')} onClick={() => onChangeRole(user)} aria-label={`Changer le rôle de ${user.fullName}`}><ShieldCheck className="size-4" />Rôle</button>
        <button type="button" className={actionClass} disabled={pending(user, 'password')} onClick={() => onResetPassword(user)} aria-label={`Réinitialiser le mot de passe de ${user.fullName}`}><KeyRound className="size-4" />Mot de passe</button>
        <button type="button" className={dangerClass} disabled={pending(user, 'delete')} onClick={() => onDelete(user)} aria-label={`Supprimer ${user.fullName}`}><Trash2 className="size-4" />Supprimer</button>
      </div>
    </article>)}
  </section>
}
