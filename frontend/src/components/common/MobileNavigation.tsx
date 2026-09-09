import { Bell, Gauge, Menu } from 'lucide-react'
import { NavLink } from 'react-router-dom'
import type { UserRole } from '../../types/auth'
import { getRoleHome } from '../../utils/roleRedirect'

export function MobileNavigation({ role, onMenuClick }: { role: UserRole; onMenuClick: () => void }) {
  return <nav className="fixed inset-x-0 bottom-0 z-20 grid grid-cols-3 border-t border-slate-200 bg-white/95 px-4 pb-[max(.5rem,env(safe-area-inset-bottom))] backdrop-blur lg:hidden" aria-label="Navigation mobile"><NavLink to={getRoleHome(role)} className="flex flex-col items-center gap-1 py-2 text-[11px] font-medium text-blue-600"><Gauge className="size-5" />Accueil</NavLink><button type="button" className="flex flex-col items-center gap-1 py-2 text-[11px] font-medium text-slate-500" aria-label="Notifications"><Bell className="size-5" />Alertes</button><button type="button" onClick={onMenuClick} className="flex flex-col items-center gap-1 py-2 text-[11px] font-medium text-slate-500"><Menu className="size-5" />Menu</button></nav>
}
