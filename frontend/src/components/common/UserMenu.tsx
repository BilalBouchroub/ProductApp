import { LogOut, RotateCcw, Settings, UserRound } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { useApiMocks } from '../../api/apiMode'
import { useAuth } from '../../hooks/useAuth'
import { ConfirmDialog } from '../ui/ConfirmDialog'
import { RoleBadge } from './RoleBadge'

export function UserMenu() {
  const [open, setOpen] = useState(false)
  const [resetOpen, setResetOpen] = useState(false)
  const { session, logout } = useAuth()
  const navigate = useNavigate()

  if (!session) return null

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  const resetDemo = async () => {
    const { sharedMockRepository } = await import('../../mocks/shared/sharedMock.repository')
    sharedMockRepository.reset()
    setResetOpen(false)
    setOpen(false)
    toast.success('Données de démonstration réinitialisées.')
  }

  return <div className="relative">
    <button type="button" onClick={() => setOpen((value) => !value)} className="flex items-center gap-2 rounded-xl p-1.5 pr-2 hover:bg-slate-100" aria-label="Menu utilisateur" aria-expanded={open}>
      <span className="grid size-8 place-items-center rounded-lg bg-slate-900 text-xs font-bold text-white">{session.user.initials}</span>
      <span className="hidden max-w-32 truncate text-sm font-semibold text-slate-700 lg:block">{session.user.name}</span>
    </button>
    {open && <div className="absolute top-12 right-0 z-30 w-64 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-xl">
      <div className="border-b border-slate-100 p-4">
        <p className="truncate text-sm font-semibold text-slate-900">{session.user.name}</p>
        <p className="mt-0.5 truncate text-xs text-slate-500">{session.user.email}</p>
        <div className="mt-2"><RoleBadge role={session.user.role} compact /></div>
      </div>
      <div className="p-2">
        <button type="button" disabled className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left text-sm text-slate-400"><UserRound className="size-4" />Mon profil</button>
        <button type="button" disabled className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left text-sm text-slate-400"><Settings className="size-4" />Préférences</button>
        {useApiMocks && <button type="button" onClick={() => setResetOpen(true)} className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left text-sm text-amber-700 hover:bg-amber-50"><RotateCcw className="size-4" />Réinitialiser la démo</button>}
        <button type="button" onClick={handleLogout} className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left text-sm font-medium text-red-600 hover:bg-red-50"><LogOut className="size-4" />Se déconnecter</button>
      </div>
    </div>}
    {useApiMocks && <ConfirmDialog open={resetOpen} onClose={() => setResetOpen(false)} onConfirm={() => { void resetDemo() }} title="Réinitialiser les données" description="Toutes les modifications locales seront remplacées par le scénario de démonstration initial." confirmLabel="Réinitialiser" />}
  </div>
}
