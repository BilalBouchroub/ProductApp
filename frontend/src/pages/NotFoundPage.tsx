import { ArrowLeft, SearchX } from 'lucide-react'
import { Link } from 'react-router-dom'
import { Button } from '../components/ui/Button'
import { useAuth } from '../hooks/useAuth'
import { getRoleHome } from '../utils/roleRedirect'

export function NotFoundPage() {
  const { session } = useAuth()
  const home = session ? getRoleHome(session.user.role) : '/login'
  return <main className="grid min-h-screen place-items-center bg-slate-50 p-6"><div className="max-w-md text-center"><SearchX className="mx-auto size-14 text-slate-300" /><p className="mt-6 text-sm font-bold tracking-widest text-blue-600 uppercase">Erreur 404</p><h1 className="mt-2 text-3xl font-bold text-slate-950">Cette page reste introuvable.</h1><p className="mt-4 text-sm leading-6 text-slate-500">L’adresse est incorrecte ou la page n’est pas encore disponible dans ce socle.</p><Link to={home} className="mt-7 inline-block"><Button><ArrowLeft className="size-4" />Retour à l’accueil</Button></Link></div></main>
}
