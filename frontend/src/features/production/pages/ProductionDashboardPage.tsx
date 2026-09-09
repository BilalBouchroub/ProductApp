import {
  Activity,
  AlertTriangle,
  ArrowRight,
  Beaker,
  Boxes,
  CheckCircle2,
  CircleDollarSign,
  Clock3,
  FlaskConical,
  PackageCheck,
  PackageOpen,
  Plus,
  RefreshCw,
  Settings2,
  Sparkles,
  TrendingUp,
  type LucideIcon,
} from 'lucide-react'
import { Link } from 'react-router-dom'
import { ErrorState } from '../../../components/ui/ErrorState'
import { cn } from '../../../utils/cn'
import { ProductionDashboardCharts } from '../components/dashboard/ProductionDashboardCharts'
import { ExperimentCard } from '../components/experiments/ExperimentCard'
import { ProductCard } from '../components/products/ProductCard'
import { useProductionDashboard } from '../hooks/useProductionDashboard'
import { minutesToLabel } from '../utils/productionCalculations'
import { formatCurrency } from '../utils/productionFormatters'

const kpiTones = {
  blue: { icon: 'bg-blue-50 text-blue-600', glow: 'from-blue-500/15', bar: 'bg-blue-500' },
  violet: { icon: 'bg-violet-50 text-violet-600', glow: 'from-violet-500/15', bar: 'bg-violet-500' },
  emerald: { icon: 'bg-emerald-50 text-emerald-600', glow: 'from-emerald-500/15', bar: 'bg-emerald-500' },
  amber: { icon: 'bg-amber-50 text-amber-600', glow: 'from-amber-500/15', bar: 'bg-amber-500' },
} as const

type KpiTone = keyof typeof kpiTones

function KpiCard({
  label,
  value,
  detail,
  icon: Icon,
  tone,
  progress,
  delay,
}: {
  label: string
  value: string
  detail: string
  icon: LucideIcon
  tone: KpiTone
  progress?: number
  delay: number
}) {
  const colors = kpiTones[tone]
  return <article className="dashboard-enter group relative overflow-hidden rounded-2xl border border-slate-200 bg-white p-5 shadow-sm transition duration-300 hover:-translate-y-1 hover:border-slate-300 hover:shadow-lg" style={{ animationDelay: `${delay}ms` }}>
    <div className={cn('pointer-events-none absolute inset-x-0 top-0 h-20 bg-gradient-to-b to-transparent opacity-70', colors.glow)} />
    <div className="relative flex items-start justify-between gap-3"><div><p className="text-sm font-medium text-slate-500">{label}</p><p className="mt-2 text-2xl font-bold tracking-tight text-slate-950">{value}</p></div><span className={cn('grid size-11 shrink-0 place-items-center rounded-2xl transition duration-300 group-hover:scale-110', colors.icon)}><Icon className="size-5" /></span></div>
    <p className="relative mt-4 text-xs font-medium text-slate-500">{detail}</p>
    {progress !== undefined && <div className="relative mt-3 h-1.5 overflow-hidden rounded-full bg-slate-100"><span className={cn('block h-full rounded-full transition-all duration-700', colors.bar)} style={{ width: `${Math.min(100, Math.max(0, progress))}%` }} /></div>}
  </article>
}

function DashboardSkeleton() {
  return <div className="space-y-6" role="status" aria-label="Chargement du tableau de bord">
    <div className="h-64 animate-pulse rounded-3xl bg-slate-200" />
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{Array.from({ length: 8 }, (_, index) => <div key={index} className="h-36 animate-pulse rounded-2xl bg-slate-200" />)}</div>
    <div className="grid gap-5 xl:grid-cols-2"><div className="h-96 animate-pulse rounded-2xl bg-slate-200" /><div className="h-96 animate-pulse rounded-2xl bg-slate-200" /></div>
  </div>
}

function EmptySection({ icon: Icon, title, description, to, action }: { icon: LucideIcon; title: string; description: string; to: string; action: string }) {
  return <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center"><span className="mx-auto grid size-12 place-items-center rounded-2xl bg-white text-slate-400 shadow-sm"><Icon className="size-6" /></span><h3 className="mt-4 font-semibold text-slate-800">{title}</h3><p className="mx-auto mt-1 max-w-sm text-sm text-slate-500">{description}</p><Link to={to} className="mt-4 inline-flex items-center gap-1 text-sm font-semibold text-blue-600 hover:text-blue-700">{action}<ArrowRight className="size-4" /></Link></div>
}

export function ProductionDashboardPage() {
  const dashboard = useProductionDashboard()
  if (dashboard.isLoading) return <DashboardSkeleton />
  if (dashboard.isError || !dashboard.data) {
    const reason = dashboard.error instanceof Error ? dashboard.error.message : 'Le service de production ne répond pas.'
    return <div className="surface-card"><ErrorState title="La vue d’ensemble est indisponible" description={`${reason} Vérifiez que l’API est démarrée, puis réessayez.`} onRetry={() => dashboard.refetch()} /></div>
  }

  const data = dashboard.data
  const metrics = data.metrics
  const completedResults = metrics.successes + metrics.partialSuccesses + metrics.failures
  const successRate = completedResults > 0 ? ((metrics.successes + metrics.partialSuccesses * 0.5) / completedResults) * 100 : 0
  const productTotal = Math.max(metrics.totalProducts, 1)
  const kpis = [
    { label: 'Produits suivis', value: String(metrics.totalProducts), detail: `${metrics.drafts} brouillon${metrics.drafts > 1 ? 's' : ''} à compléter`, icon: Boxes, tone: 'blue' as const, progress: 100 },
    { label: 'Expériences', value: String(metrics.totalExperiments), detail: `${metrics.experimenting} produit${metrics.experimenting > 1 ? 's' : ''} en expérimentation`, icon: Beaker, tone: 'violet' as const, progress: metrics.totalProducts ? metrics.experimenting / productTotal * 100 : 0 },
    { label: 'Taux de réussite', value: `${successRate.toFixed(0)} %`, detail: `${metrics.successes} réussite${metrics.successes > 1 ? 's' : ''} confirmée${metrics.successes > 1 ? 's' : ''}`, icon: TrendingUp, tone: 'emerald' as const, progress: successRate },
    { label: 'Prêts pour étude', value: String(metrics.ready), detail: 'Transmission vers le Commercial', icon: PackageCheck, tone: 'emerald' as const, progress: metrics.ready / productTotal * 100 },
    { label: 'Coût moyen', value: formatCurrency(metrics.averageCost), detail: 'Par gamme de production configurée', icon: CircleDollarSign, tone: 'violet' as const },
    { label: 'Durée moyenne', value: minutesToLabel(Math.round(metrics.averageDuration)), detail: 'Temps planifié par chaîne', icon: Clock3, tone: 'blue' as const },
    { label: 'Perte moyenne', value: `${metrics.averageWasteRate.toFixed(1)} %`, detail: 'Sur les expériences terminées', icon: Activity, tone: metrics.averageWasteRate > 5 ? 'amber' as const : 'emerald' as const, progress: metrics.averageWasteRate },
    { label: 'Alertes ressources', value: String(metrics.lowStockResources), detail: 'Stocks faibles ou indisponibles', icon: AlertTriangle, tone: 'amber' as const, progress: Math.min(metrics.lowStockResources * 12.5, 100) },
  ]
  const workflow = [
    { label: 'Brouillons', value: metrics.drafts, icon: PackageOpen, color: 'bg-slate-400' },
    { label: 'Configuration', value: metrics.configuring, icon: Settings2, color: 'bg-blue-500' },
    { label: 'Expérimentation', value: metrics.experimenting, icon: FlaskConical, color: 'bg-violet-500' },
    { label: 'Prêts pour étude', value: metrics.ready, icon: CheckCircle2, color: 'bg-emerald-500' },
  ]

  return <div className="space-y-8">
    <section className="dashboard-enter relative overflow-hidden rounded-3xl bg-gradient-to-br from-slate-950 via-blue-950 to-blue-700 px-6 py-8 text-white shadow-xl shadow-blue-950/10 sm:px-8 lg:px-10 lg:py-10">
      <div className="pointer-events-none absolute -right-16 -top-24 size-72 rounded-full bg-cyan-400/20 blur-3xl" /><div className="pointer-events-none absolute -bottom-32 left-1/3 size-72 rounded-full bg-violet-500/20 blur-3xl" />
      <div className="relative grid items-end gap-8 lg:grid-cols-[1fr_auto]">
        <div><p className="flex items-center gap-2 text-xs font-semibold uppercase tracking-[.2em] text-blue-200"><Sparkles className="size-4" />Centre de pilotage Production</p><h1 className="mt-3 max-w-3xl text-3xl font-bold tracking-tight sm:text-4xl">Une vue claire de toute votre activité industrielle.</h1><p className="mt-3 max-w-2xl text-sm leading-6 text-blue-100/85 sm:text-base">Coûts, délais, ressources et expériences sont synchronisés avec les données réelles de ProductApp.</p>
          <div className="mt-6 flex flex-wrap gap-3"><Link to="/production/experiments/new" className="inline-flex h-11 items-center gap-2 rounded-xl bg-white px-4 text-sm font-semibold text-blue-800 shadow-lg shadow-blue-950/20 transition hover:-translate-y-0.5 hover:bg-blue-50"><Beaker className="size-4" />Nouvelle expérience</Link><Link to="/production/products/new" className="inline-flex h-11 items-center gap-2 rounded-xl border border-white/25 bg-white/10 px-4 text-sm font-semibold text-white backdrop-blur transition hover:-translate-y-0.5 hover:bg-white/20"><Plus className="size-4" />Nouveau produit</Link></div>
        </div>
        <div className="flex items-center gap-4 rounded-2xl border border-white/15 bg-white/10 p-4 backdrop-blur-md"><div className="grid size-16 place-items-center rounded-full border-4 border-emerald-300/70 bg-emerald-400/10 text-lg font-bold">{successRate.toFixed(0)}%</div><div><p className="text-xs uppercase tracking-wider text-blue-200">Performance essais</p><p className="mt-1 font-semibold">{completedResults > 0 ? 'Résultats consolidés' : 'En attente de résultats'}</p><button type="button" onClick={() => dashboard.refetch()} className="mt-2 inline-flex items-center gap-1 text-xs font-medium text-blue-200 transition hover:text-white"><RefreshCw className={cn('size-3.5', dashboard.isFetching && 'animate-spin')} />Actualiser</button></div></div>
      </div>
    </section>

    {data.warnings.length > 0 && <div role="status" className="flex items-start gap-3 rounded-2xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-900"><AlertTriangle className="mt-0.5 size-5 shrink-0 text-amber-600" /><div><p className="font-semibold">Certaines données sont partielles</p>{data.warnings.map(warning => <p key={warning} className="mt-1 text-amber-800">{warning}</p>)}</div></div>}

    <section aria-label="Statistiques clés" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{kpis.map((kpi, index) => <KpiCard key={kpi.label} {...kpi} delay={index * 45} />)}</section>

    <section className="grid gap-5 xl:grid-cols-[1.35fr_.65fr]">
      <article className="surface-card p-5 sm:p-6"><div className="flex items-start justify-between gap-3"><div><h2 className="text-lg font-bold text-slate-950">Flux des produits</h2><p className="mt-1 text-sm text-slate-500">Répartition dans les principales étapes du cycle.</p></div><Activity className="size-5 text-blue-500" /></div><div className="mt-6 space-y-5">{workflow.map(item => { const Icon = item.icon; const percent = item.value / productTotal * 100; return <div key={item.label}><div className="mb-2 flex items-center justify-between text-sm"><span className="flex items-center gap-2 font-medium text-slate-700"><Icon className="size-4 text-slate-400" />{item.label}</span><strong className="text-slate-950">{item.value}</strong></div><div className="h-2.5 overflow-hidden rounded-full bg-slate-100"><span className={cn('block h-full rounded-full transition-all duration-700', item.color)} style={{ width: `${percent}%` }} /></div></div>})}</div></article>
      <article className="overflow-hidden rounded-2xl border border-slate-200 bg-slate-950 p-5 text-white shadow-sm sm:p-6"><div className="flex items-start justify-between"><div><p className="text-xs font-semibold uppercase tracking-wider text-amber-300">À traiter</p><h2 className="mt-1 text-lg font-bold">Priorités opérationnelles</h2></div><AlertTriangle className="size-5 text-amber-300" /></div><div className="mt-6 space-y-3"><div className="rounded-xl bg-white/8 p-4"><p className="text-2xl font-bold">{data.optimizationRequests.filter(request => request.status !== 'Resolved' && request.status !== 'Rejected').length}</p><p className="mt-1 text-xs text-slate-300">Demandes d’optimisation ouvertes</p></div><div className="rounded-xl bg-white/8 p-4"><p className="text-2xl font-bold">{metrics.lowStockResources}</p><p className="mt-1 text-xs text-slate-300">Ressources nécessitant une attention</p></div></div><Link to="/production/optimization-requests" className="mt-5 inline-flex items-center gap-1 text-sm font-semibold text-amber-300 transition hover:text-amber-200">Voir les demandes<ArrowRight className="size-4" /></Link></article>
    </section>

    <ProductionDashboardCharts data={data} />

    <section><div className="flex items-end justify-between gap-4"><div><h2 className="text-xl font-bold text-slate-950">Produits récemment modifiés</h2><p className="mt-1 text-sm text-slate-500">Accédez rapidement aux dernières gammes mises à jour.</p></div><Link to="/production/products" className="hidden items-center gap-1 text-sm font-semibold text-blue-600 sm:inline-flex">Tous les produits<ArrowRight className="size-4" /></Link></div><div className="mt-4 grid gap-4 md:grid-cols-2 xl:grid-cols-3">{data.recentProducts.length > 0 ? data.recentProducts.slice(0, 3).map(product => <ProductCard key={product.id} product={product} />) : <div className="md:col-span-2 xl:col-span-3"><EmptySection icon={Boxes} title="Aucun produit" description="Créez votre premier produit pour alimenter les statistiques de production." to="/production/products/new" action="Créer un produit" /></div>}</div></section>

    <section><div className="flex items-end justify-between gap-4"><div><h2 className="text-xl font-bold text-slate-950">Expériences récentes</h2><p className="mt-1 text-sm text-slate-500">Suivez les derniers essais et leurs écarts.</p></div><Link to="/production/experiments" className="hidden items-center gap-1 text-sm font-semibold text-blue-600 sm:inline-flex">Toutes les expériences<ArrowRight className="size-4" /></Link></div><div className="mt-4 grid gap-4 lg:grid-cols-2">{data.recentExperiments.length > 0 ? data.recentExperiments.slice(0, 2).map(experiment => <ExperimentCard key={experiment.id} experiment={experiment} />) : <div className="lg:col-span-2"><EmptySection icon={Beaker} title="Aucune expérience" description="Lancez un essai pour comparer les coûts, durées et rendements réels." to="/production/experiments/new" action="Créer une expérience" /></div>}</div></section>

    <p className="pb-2 text-center text-xs text-slate-400">Données actualisées à {new Intl.DateTimeFormat('fr-MA', { hour: '2-digit', minute: '2-digit' }).format(new Date(data.updatedAt))}</p>
  </div>
}
