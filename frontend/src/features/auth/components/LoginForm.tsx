import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowRight, Eye, EyeOff, LockKeyhole, Mail } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { Button } from '../../../components/ui/Button'
import { Input } from '../../../components/ui/Input'
import { useAuth } from '../../../hooks/useAuth'
import { getRoleHome } from '../../../utils/roleRedirect'
import { loginSchema, type LoginFormValues } from '../schemas/loginSchema'
import { toast } from 'sonner'

export function LoginForm() {
  const [showPassword, setShowPassword] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema), defaultValues: { email: 'admin@productapp.local', password: 'AdminDemo!2026' },
  })
  const submit = handleSubmit(async ({ email, password }) => { try {
    const session = await login({ email, password }); navigate(getRoleHome(session.user.role), { replace: true })
  } catch (error) { toast.error(error instanceof Error ? error.message : 'Connexion impossible.') } })
  return <form onSubmit={submit} noValidate className="space-y-5">
    <div className="relative"><Mail className="pointer-events-none absolute top-[2.55rem] left-3.5 z-10 size-4 text-slate-400" /><Input label="Adresse e-mail" type="email" autoComplete="email" placeholder="prenom.nom@entreprise.com" className="pl-10" error={errors.email?.message} {...register('email')} /></div>
    <div className="relative"><LockKeyhole className="pointer-events-none absolute top-[2.55rem] left-3.5 z-10 size-4 text-slate-400" /><Input label="Mot de passe" type={showPassword ? 'text' : 'password'} autoComplete="current-password" className="pr-11 pl-10" error={errors.password?.message} {...register('password')} /><button type="button" onClick={() => setShowPassword(value => !value)} className="absolute top-[2.1rem] right-2 rounded-lg p-2 text-slate-400 hover:bg-slate-100" aria-label={showPassword ? 'Masquer le mot de passe' : 'Afficher le mot de passe'}>{showPassword ? <EyeOff className="size-4" /> : <Eye className="size-4" />}</button></div>
    <div className="rounded-xl border border-blue-100 bg-blue-50 px-4 py-3 text-xs leading-5 text-blue-800">L’espace professionnel est déterminé automatiquement par le rôle associé à votre compte.</div>
    <Button type="submit" size="lg" loading={isSubmitting} className="w-full">Se connecter<ArrowRight className="size-4" /></Button>
  </form>
}
