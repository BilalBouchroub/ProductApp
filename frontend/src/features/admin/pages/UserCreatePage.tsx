import { ArrowLeft } from 'lucide-react'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { PageHeader } from '../../../components/common/PageHeader'
import { UserForm } from '../components/users/UserForm'
import { useAdminUserMutations } from '../hooks/useAdminUsers'
import type { UserFormValues } from '../schemas/userSchema'

export function UserCreatePage() {
  const navigate = useNavigate()
  const { createUser } = useAdminUserMutations()
  const submit = async (values: UserFormValues) => { try { const user = await createUser.mutateAsync(values); toast.success(`${user.fullName} a été créé.`); navigate(`/admin/users/${user.id}`, { replace: true }) } catch (error) { toast.error(error instanceof Error ? error.message : 'Impossible de créer cet utilisateur.') } }
  return <div className="mx-auto max-w-4xl space-y-6"><PageHeader eyebrow="Utilisateurs" title="Ajouter un utilisateur" description="Créez un compte et attribuez-lui son espace professionnel." actions={<Link className="inline-flex h-10 items-center justify-center gap-2 rounded-xl border border-slate-300 bg-white px-4 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100" to="/admin/users"><ArrowLeft className="size-4" />Retour à la liste</Link>} /><UserForm onSubmit={submit} loading={createUser.isPending} /></div>
}
