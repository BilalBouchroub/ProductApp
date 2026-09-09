import { Navigate } from 'react-router-dom'
import { useAuth } from '../../../hooks/useAuth'
import { getRoleHome } from '../../../utils/roleRedirect'
import { LoginForm } from '../components/LoginForm'

export function LoginPage() {
  const { session } = useAuth()
  if (session) return <Navigate to={getRoleHome(session.user.role)} replace />
  return <div><p className="text-sm font-semibold text-blue-600">Bienvenue</p><h1 className="mt-2 text-3xl font-bold tracking-tight text-slate-950">Connectez-vous à votre espace</h1><p className="mt-3 text-sm leading-6 text-slate-500">Accédez à une expérience adaptée à votre responsabilité dans l’entreprise.</p><div className="mt-8"><LoginForm /></div><p className="mt-8 text-center text-xs text-slate-400">Accès sécurisé · Session de démonstration uniquement</p></div>
}
