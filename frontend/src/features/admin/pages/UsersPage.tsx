import { UserPlus, Users } from 'lucide-react'
import { useEffect, useMemo, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { toast } from 'sonner'
import { PageHeader } from '../../../components/common/PageHeader'
import { Button } from '../../../components/ui/Button'
import { ErrorState } from '../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { UserFilters, type UserFilterState } from '../components/users/UserFilters'
import { UserTable } from '../components/users/UserTable'
import { useAdminUserMutations, useAdminUsers } from '../hooks/useAdminUsers'
import type { AdminUser } from '../types/admin.types'
import type { UserRole } from '../../../types/auth'

type Feedback = { tone: 'success' | 'error'; text: string }

function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'Une erreur inattendue est survenue.'
}

export function UsersPage() {
  const usersQuery = useAdminUsers()
  const mutations = useAdminUserMutations()
  const [filters, setFilters] = useState<UserFilterState>({ search: '', role: '', status: '' })
  const [deleteTarget, setDeleteTarget] = useState<AdminUser | null>(null)
  const [disableTarget, setDisableTarget] = useState<AdminUser | null>(null)
  const [roleTarget, setRoleTarget] = useState<AdminUser | null>(null)
  const [selectedRole, setSelectedRole] = useState<UserRole>('ProductionManager')
  const [feedback, setFeedback] = useState<Feedback | null>(null)
  const actionPanelRef = useRef<HTMLElement>(null)

  useEffect(() => {
    if (!deleteTarget && !disableTarget && !roleTarget) return
    const frame = requestAnimationFrame(() => actionPanelRef.current?.scrollIntoView?.({ behavior: 'smooth', block: 'center' }))
    return () => cancelAnimationFrame(frame)
  }, [deleteTarget, disableTarget, roleTarget])

  const users = useMemo(() => (usersQuery.data ?? []).filter((user) => {
    const term = filters.search.trim().toLowerCase()
    return (!term || (user.fullName + ' ' + user.email).toLowerCase().includes(term))
      && (!filters.role || user.role === filters.role)
      && (!filters.status || user.status === filters.status)
  }), [filters, usersQuery.data])

  const success = (text: string) => {
    setFeedback({ tone: 'success', text })
    toast.success(text)
  }
  const failure = (error: unknown) => {
    const text = errorMessage(error)
    setFeedback({ tone: 'error', text })
    toast.error(text)
  }

  const activate = async (user: AdminUser) => {
    setFeedback(null)
    try {
      await mutations.setStatus.mutateAsync({ id: user.id, status: 'Active' })
      success(user.fullName + ' a été activé.')
    } catch (error) {
      failure(error)
    }
  }

  const resetPassword = async (user: AdminUser) => {
    setFeedback(null)
    try {
      await mutations.resetPassword.mutateAsync(user.id)
      success('La demande de réinitialisation a été envoyée pour ' + user.fullName + '.')
    } catch (error) {
      failure(error)
    }
  }

  const confirmDelete = async () => {
    if (!deleteTarget) return
    setFeedback(null)
    try {
      const name = deleteTarget.fullName
      await mutations.deleteUser.mutateAsync(deleteTarget.id)
      setDeleteTarget(null)
      success(name + ' a été supprimé.')
    } catch (error) {
      setDeleteTarget(null)
      failure(error)
    }
  }

  const confirmDisable = async () => {
    if (!disableTarget) return
    setFeedback(null)
    try {
      const name = disableTarget.fullName
      await mutations.setStatus.mutateAsync({ id: disableTarget.id, status: 'Inactive' })
      setDisableTarget(null)
      success(name + ' a été désactivé.')
    } catch (error) {
      setDisableTarget(null)
      failure(error)
    }
  }

  const confirmRole = async () => {
    if (!roleTarget) return
    setFeedback(null)
    try {
      const name = roleTarget.fullName
      await mutations.changeRole.mutateAsync({ id: roleTarget.id, role: selectedRole })
      setRoleTarget(null)
      success('Le rôle de ' + name + ' a été modifié.')
    } catch (error) {
      setRoleTarget(null)
      failure(error)
    }
  }

  const openRole = (user: AdminUser) => {
    setSelectedRole(user.role)
    setRoleTarget(user)
  }

  const pendingAction = mutations.resetPassword.isPending && mutations.resetPassword.variables
    ? { id: mutations.resetPassword.variables, action: 'password' as const }
    : mutations.setStatus.isPending && mutations.setStatus.variables?.status === 'Active'
      ? { id: mutations.setStatus.variables.id, action: 'status' as const }
      : null

  return <div className="space-y-6">
    <PageHeader
      eyebrow="Administration"
      title="Gestion des utilisateurs"
      description="Gérez les accès, les rôles et le cycle de vie des comptes de la plateforme."
      actions={<Link to="/admin/users/new" className="inline-flex h-10 items-center justify-center gap-2 rounded-xl bg-blue-600 px-4 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100"><UserPlus className="size-4" />Ajouter un utilisateur</Link>}
    />

    {feedback && <div
      role={feedback.tone === 'error' ? 'alert' : 'status'}
      className={feedback.tone === 'error'
        ? 'rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800'
        : 'rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-800'}
    >
      {feedback.text}
    </div>}

    <div className="grid gap-4 sm:grid-cols-3">
      <div className="surface-card p-4">
        <p className="text-xs font-semibold text-slate-500 uppercase">Utilisateurs affichés</p>
        <p className="mt-2 text-2xl font-bold text-slate-950">{users.length}</p>
      </div>
      <div className="surface-card p-4">
        <p className="text-xs font-semibold text-slate-500 uppercase">Comptes actifs</p>
        <p className="mt-2 text-2xl font-bold text-emerald-600">
          {(usersQuery.data ?? []).filter((user) => user.status === 'Active').length}
        </p>
      </div>
      <div className="surface-card p-4">
        <p className="text-xs font-semibold text-slate-500 uppercase">Rôles disponibles</p>
        <p className="mt-2 flex items-center gap-2 text-2xl font-bold text-slate-950">
          <Users className="size-5 text-blue-600" />3
        </p>
      </div>
    </div>

    <UserFilters filters={filters} onChange={setFilters} />

    {(deleteTarget || disableTarget || roleTarget) && <section ref={actionPanelRef} role="dialog" aria-label="Confirmation de l’action" className="rounded-2xl border-2 border-blue-400 bg-blue-50 p-5 shadow-lg">
      <h2 className="font-semibold text-slate-950">
        {deleteTarget ? 'Confirmer la suppression' : disableTarget ? 'Confirmer la désactivation' : 'Changer le rôle'}
      </h2>
      <p className="mt-1 text-sm text-slate-600">
        {deleteTarget
          ? `Supprimer le compte de ${deleteTarget.fullName} ? Les sessions seront révoquées.`
          : disableTarget
            ? `Désactiver le compte de ${disableTarget.fullName} ?`
            : `Sélectionnez le nouveau rôle de ${roleTarget?.fullName ?? ''}.`}
      </p>
      {roleTarget && <select
        value={selectedRole}
        onChange={(event) => setSelectedRole(event.target.value as UserRole)}
        className="mt-4 h-10 w-full max-w-md rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:ring-4 focus:ring-blue-100"
      >
        <option value="Administrator">Administrateur</option>
        <option value="ProductionManager">Responsable Production</option>
        <option value="CommercialManager">Responsable Commercial / Marketing</option>
      </select>}
      <div className="mt-4 flex flex-wrap gap-3">
        <Button variant="secondary" onClick={() => { setDeleteTarget(null); setDisableTarget(null); setRoleTarget(null) }}>Annuler</Button>
        {deleteTarget && <Button variant="danger" loading={mutations.deleteUser.isPending} onClick={() => void confirmDelete()}>Supprimer maintenant</Button>}
        {disableTarget && <Button variant="danger" loading={mutations.setStatus.isPending} onClick={() => void confirmDisable()}>Désactiver maintenant</Button>}
        {roleTarget && <Button loading={mutations.changeRole.isPending} disabled={selectedRole === roleTarget.role} onClick={() => void confirmRole()}>Enregistrer le rôle</Button>}
      </div>
    </section>}

    {usersQuery.isLoading
      ? <div className="surface-card p-6"><LoadingSkeleton lines={8} /></div>
      : usersQuery.isError
        ? <div className="surface-card"><ErrorState description="Impossible de charger les utilisateurs depuis l’API." onRetry={() => usersQuery.refetch()} /></div>
        : <UserTable
            users={users}
            pendingAction={pendingAction}
            onDelete={setDeleteTarget}
            onToggleStatus={(user) => user.status === 'Active' ? setDisableTarget(user) : void activate(user)}
            onChangeRole={openRole}
            onResetPassword={(user) => void resetPassword(user)}
          />}

  </div>
}
