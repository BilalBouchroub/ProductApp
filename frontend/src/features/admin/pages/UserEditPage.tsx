import { ArrowLeft } from 'lucide-react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { toast } from 'sonner'
import { PageHeader } from '../../../components/common/PageHeader'
import { ErrorState } from '../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { UserForm } from '../components/users/UserForm'
import { useAdminUser, useAdminUserMutations } from '../hooks/useAdminUsers'
import type { UserFormValues } from '../schemas/userSchema'

export function UserEditPage() {
  const { id = '' } = useParams()
  const navigate = useNavigate()
  const userQuery = useAdminUser(id)
  const { updateUser } = useAdminUserMutations()
  if (userQuery.isLoading) return <div className="surface-card p-7"><LoadingSkeleton lines={8} /></div>
  if (userQuery.isError || !userQuery.data) return <div className="surface-card"><ErrorState title="Utilisateur introuvable" description="Ce compte n’existe plus dans les données de démonstration." /></div>
  const submit = async (values: UserFormValues) => { try { const user = await updateUser.mutateAsync({ id, input: values }); toast.success('Les informations ont été enregistrées.'); navigate(`/admin/users/${user.id}`, { replace: true }) } catch (error) { toast.error(error instanceof Error ? error.message : 'Impossible de modifier cet utilisateur.') } }
  return <div className="mx-auto max-w-4xl space-y-6"><PageHeader eyebrow="Utilisateurs" title={`Modifier ${userQuery.data.fullName}`} description="Mettez à jour les coordonnées, le rôle ou le statut du compte." actions={<Link className="inline-flex h-10 items-center justify-center gap-2 rounded-xl border border-slate-300 bg-white px-4 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100" to={`/admin/users/${id}`}><ArrowLeft className="size-4" />Annuler</Link>} /><UserForm user={userQuery.data} onSubmit={submit} loading={updateUser.isPending} cancelTo={`/admin/users/${id}`} /></div>
}
