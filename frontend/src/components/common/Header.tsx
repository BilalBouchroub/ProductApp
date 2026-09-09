import { Menu, Sparkles } from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { Breadcrumbs } from './Breadcrumbs'
import { NotificationBell } from './NotificationBell'
import { UserMenu } from './UserMenu'

export function Header({ onMenuClick }: { onMenuClick: () => void }) {
  const { pathname } = useLocation()
  const productMatch = pathname.match(/^\/(?:production|commercial)\/products\/([0-9a-f-]{36})(?:\/|$)/i)
  const experimentMatch = pathname.match(/^\/production\/experiments\/([0-9a-f-]{36})(?:\/|$)/i)
  const smartProductPath = productMatch ? `/ai?productId=${productMatch[1]}` : experimentMatch ? `/ai?experimentId=${experimentMatch[1]}` : '/ai'
  return <header className="sticky top-0 z-20 flex h-[4.5rem] items-center justify-between border-b border-slate-200/80 bg-white/90 px-4 backdrop-blur-xl sm:px-6 lg:px-8"><div className="flex items-center gap-3"><button type="button" onClick={onMenuClick} className="grid size-10 place-items-center rounded-xl text-slate-600 hover:bg-slate-100 lg:hidden" aria-label="Ouvrir la navigation"><Menu className="size-5" /></button><Breadcrumbs /></div><div className="flex items-center gap-1 sm:gap-2"><Link to={smartProductPath} className="flex h-9 items-center gap-2 rounded-xl bg-gradient-to-r from-violet-600 to-blue-600 px-3 text-xs font-bold text-white shadow-sm transition hover:shadow-md" title="Ouvrir SMART PRODUCT"><Sparkles className="size-3.5" /><span className="hidden sm:inline">SMART PRODUCT</span></Link><NotificationBell /><span className="mx-1 h-7 w-px bg-slate-200" /><UserMenu /></div></header>
}
