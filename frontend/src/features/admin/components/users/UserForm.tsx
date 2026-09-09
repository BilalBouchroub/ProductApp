import { zodResolver } from '@hookform/resolvers/zod'
import { Save } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { Button } from '../../../../components/ui/Button'
import { Input } from '../../../../components/ui/Input'
import { Select } from '../../../../components/ui/Select'
import type { AdminUser } from '../../types/admin.types'
import { createUserSchema, userSchema, type UserFormValues } from '../../schemas/userSchema'

const roleOptions = [
  { value: 'Administrator', label: 'Administrateur' },
  { value: 'ProductionManager', label: 'Responsable Production' },
  { value: 'CommercialManager', label: 'Responsable Commercial / Marketing' },
] as const

const statusOptions = [
  { value: 'Active', label: 'Actif' },
  { value: 'Inactive', label: 'Inactif' },
  { value: 'Suspended', label: 'Suspendu' },
] as const

interface UserFormProps {
  user?: AdminUser
  onSubmit: (values: UserFormValues) => Promise<void>
  loading: boolean
  cancelTo?: string
}

export function UserForm({ user, onSubmit, loading, cancelTo = '/admin/users' }: UserFormProps) {
  const isCreating = !user
  const { register, handleSubmit, formState: { errors } } = useForm<UserFormValues>({
    resolver: zodResolver(isCreating ? createUserSchema : userSchema),
    defaultValues: user
      ? {
          firstName: user.firstName,
          lastName: user.lastName,
          email: user.email,
          phoneNumber: user.phoneNumber,
          role: user.role,
          status: user.status,
        }
      : {
          firstName: '',
          lastName: '',
          email: '',
          phoneNumber: '',
          role: 'ProductionManager',
          status: 'Active',
          temporaryPassword: '',
          confirmTemporaryPassword: '',
        },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="surface-card p-5 sm:p-7">
      <div className="grid gap-5 sm:grid-cols-2">
        <Input label="Prénom" autoComplete="given-name" error={errors.firstName?.message} {...register('firstName')} />
        <Input label="Nom" autoComplete="family-name" error={errors.lastName?.message} {...register('lastName')} />
        <Input label="Adresse e-mail" type="email" autoComplete="email" error={errors.email?.message} {...register('email')} />
        <Input label="Téléphone" type="tel" autoComplete="tel" placeholder="+212 6 00 00 00 00" error={errors.phoneNumber?.message} {...register('phoneNumber')} />
        <Select label="Rôle" options={roleOptions} error={errors.role?.message} {...register('role')} />
        <Select label="Statut" options={statusOptions} error={errors.status?.message} {...register('status')} />
        {isCreating && (
          <>
            <Input
              label="Mot de passe temporaire"
              type="password"
              autoComplete="new-password"
              hint="12 caractères minimum avec majuscule, minuscule, chiffre et caractère spécial."
              error={errors.temporaryPassword?.message}
              {...register('temporaryPassword')}
            />
            <Input
              label="Confirmer le mot de passe"
              type="password"
              autoComplete="new-password"
              error={errors.confirmTemporaryPassword?.message}
              {...register('confirmTemporaryPassword')}
            />
          </>
        )}
      </div>

      {isCreating && (
        <div className="mt-6 rounded-xl border border-amber-200 bg-amber-50 p-4 text-xs leading-5 text-amber-900">
          <strong>Invitation :</strong> après la création, le backend enverra automatiquement
          les informations de connexion à cette adresse e-mail via le serveur SMTP configuré.
          Le mot de passe sera uniquement conservé sous forme de hash dans SQL Server.
        </div>
      )}

      <div className="mt-7 flex flex-col-reverse justify-end gap-3 border-t border-slate-200 pt-5 sm:flex-row">
        <Link to={cancelTo} className="inline-flex h-10 w-full items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100 sm:w-auto">Annuler</Link>
        <Button type="submit" loading={loading} className="w-full sm:w-auto">
          <Save className="size-4" />
          {user ? 'Enregistrer les modifications' : 'Créer l’utilisateur'}
        </Button>
      </div>
    </form>
  )
}
