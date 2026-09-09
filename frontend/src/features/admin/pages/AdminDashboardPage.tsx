import { FileClock, UserPlus } from 'lucide-react'
import { Link } from 'react-router-dom'
import { PageHeader } from '../../../components/common/PageHeader'
import { ErrorState } from '../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { AdminDashboardCharts } from '../components/dashboard/AdminDashboardCharts'
import { AdminMetricGrid } from '../components/dashboard/AdminMetricGrid'
import { RecentActivityTable } from '../components/dashboard/RecentActivityTable'
import { useAdminDashboard } from '../hooks/useAdminDashboard'

export function AdminDashboardPage() {
  const dashboard = useAdminDashboard()
  if (dashboard.isLoading) return <div className="space-y-6"><LoadingSkeleton lines={3} /><div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{Array.from({ length: 8 }, (_, index) => <div key={index} className="surface-card h-36 animate-pulse bg-slate-100" />)}</div></div>
  if (dashboard.isError || !dashboard.data) return <div className="surface-card"><ErrorState description="Impossible de charger le tableau de bord administrateur." onRetry={() => dashboard.refetch()} /></div>
  return <div className="space-y-8"><PageHeader eyebrow="Espace Administrateur" title="Vue d’ensemble de la plateforme" description="Supervisez les utilisateurs, le portefeuille produit, les analyses commerciales et la traçabilité depuis un point central." actions={<><Link to="/admin/logs" className="inline-flex h-10 items-center justify-center gap-2 rounded-xl border border-slate-300 bg-white px-4 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100"><FileClock className="size-4" />Consulter les logs</Link><Link to="/admin/users/new" className="inline-flex h-10 items-center justify-center gap-2 rounded-xl bg-blue-600 px-4 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100"><UserPlus className="size-4" />Ajouter un utilisateur</Link></>} /><AdminMetricGrid metrics={dashboard.data.metrics} /><AdminDashboardCharts data={dashboard.data} /><section className="space-y-4"><div><h2 className="text-lg font-semibold text-slate-950">Activité récente</h2><p className="mt-1 text-sm text-slate-500">Dernières actions et alertes enregistrées sur la plateforme.</p></div><RecentActivityTable logs={dashboard.data.recentLogs} /></section></div>
}
