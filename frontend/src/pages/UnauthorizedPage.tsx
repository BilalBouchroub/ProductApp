import { ArrowLeft, ShieldX } from 'lucide-react'
import { Link } from 'react-router-dom'
import { Button } from '../components/ui/Button'
import { useAuth } from '../hooks/useAuth'
import { getRoleHome } from '../utils/roleRedirect'

export function UnauthorizedPage() {
  const { session } = useAuth()
  const home = session ? getRoleHome(session.user.role) : '/login'
  return <main className="grid min-h-screen place-items-center bg-slate-50 p-6"><div className="max-w-md text-center"><span className="mx-auto grid size-16 place-items-center rounded-2xl bg-red-50 text-red-600"><ShieldX className="size-8" /></span><p className="mt-6 text-sm font-bold tracking-widest text-red-600 uppercase">Accès refusé</p><h1 className="mt-2 text-3xl font-bold text-slate-950">Vous n’avez pas accès à cet espace.</h1><p className="mt-4 text-sm leading-6 text-slate-500">Votre rôle actuel ne possède pas les autorisations nécessaires pour consulter cette page.</p><Link to={home} className="mt-7 inline-block"><Button><ArrowLeft className="size-4" />Retour à mon espace</Button></Link></div></main>
}
