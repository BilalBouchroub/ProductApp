import { ArrowLeft, Pencil } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { PageHeader } from '../../../components/common/PageHeader'
import { ErrorState } from '../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { UserProfileCard } from '../components/users/UserProfileCard'
import { useAdminUser } from '../hooks/useAdminUsers'

const secondaryLink = 'inline-flex h-10 items-center justify-center gap-2 rounded-xl border border-slate-300 bg-white px-4 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100'
const primaryLink = 'inline-flex h-10 items-center justify-center gap-2 rounded-xl bg-blue-600 px-4 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100'

export function UserDetailsPage() {
  const { id = '' } = useParams()
  const userQuery = useAdminUser(id)

  if (userQuery.isLoading) {
    return <div className="surface-card p-7"><LoadingSkeleton lines={8} /></div>
  }

  if (userQuery.isError || !userQuery.data) {
    return <div className="surface-card"><ErrorState title="Utilisateur introuvable" description="Ce compte n’existe plus ou n’est pas accessible." onRetry={() => userQuery.refetch()} /></div>
  }

  return <div className="mx-auto max-w-5xl space-y-6">
    <PageHeader
      eyebrow="Utilisateurs"
      title="Détails du compte"
      description="Consultez l’identité, les autorisations et l’activité du compte. Les actions administratives sont centralisées dans la liste."
      actions={<>
        <Link to="/admin/users" className={secondaryLink}><ArrowLeft className="size-4" />Liste</Link>
        <Link to={`/admin/users/${id}/edit`} className={primaryLink}><Pencil className="size-4" />Modifier</Link>
      </>}
    />
    <UserProfileCard user={userQuery.data} />
  </div>
}
