import { Outlet } from 'react-router-dom'
import { Header } from '../components/common/Header'
import { MobileNavigation } from '../components/common/MobileNavigation'
import { Sidebar } from '../components/common/Sidebar'
import { Drawer } from '../components/ui/Drawer'
import { useAuth } from '../hooks/useAuth'
import { useDisclosure } from '../hooks/useDisclosure'

export function AppLayout() {
  const { session } = useAuth()
  const menu = useDisclosure()
  if (!session) return null
  return <div className="min-h-screen bg-slate-50"><aside className="fixed inset-y-0 left-0 z-30 hidden w-[17rem] lg:block"><Sidebar role={session.user.role} /></aside><div className="lg:pl-[17rem]"><Header onMenuClick={menu.open} /><main id="main-content" className="mx-auto min-h-[calc(100vh-4.5rem)] max-w-[1600px] px-4 py-6 pb-24 sm:px-6 sm:py-8 lg:px-8 lg:pb-8"><Outlet /></main></div><MobileNavigation role={session.user.role} onMenuClick={menu.open} /><Drawer open={menu.isOpen} onClose={menu.close} title="Navigation" side="left"><Sidebar role={session.user.role} onNavigate={menu.close} /></Drawer></div>
}
