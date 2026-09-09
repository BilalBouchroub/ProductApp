import { AlertTriangle, BarChart3, CircleDollarSign, ClipboardCheck, Gauge, PackageCheck, RefreshCw, Sparkles, TrendingUp } from 'lucide-react'
import { Link } from 'react-router-dom'
import { PageHeader } from '../../../components/common/PageHeader'
import { StatCard } from '../../../components/common/StatCard'
import { Button } from '../../../components/ui/Button'
import { EmptyState } from '../../../components/ui/EmptyState'
import { ErrorState } from '../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { formatRelativeDate } from '../../../utils/formatters'
import { CommercialDashboardCharts } from '../components/dashboard/CommercialDashboardCharts'
import { RecommendationBadge } from '../components/results/RecommendationBadge'
import { useCommercialDashboard } from '../hooks/useCommercialDashboard'
import type { MarketStudy } from '../types/commercial.types'

export function CommercialDashboardPage() {
  const dashboard = useCommercialDashboard()
  if (dashboard.isLoading) return <LoadingSkeleton lines={10}/>
  if (dashboard.isError || !dashboard.data) {
    const reason = dashboard.error instanceof Error ? dashboard.error.message : 'Le service commercial ne répond pas.'
    return <ErrorState title={'La vue d’ensemble est indisponible'} description={reason} onRetry={() => dashboard.refetch()}/>
  }

  const data = dashboard.data
  const metrics = data.metrics
  const cards = [
    ['En attente', metrics.waiting, 'Versions sans étude', ClipboardCheck, 'blue'],
    ['Études en cours', metrics.inProgress, 'Brouillons et études actives', BarChart3, 'violet'],
    ['Produits étudiés', metrics.studied, 'Dernières décisions validées', PackageCheck, 'blue'],
    ['Viables', metrics.viable, 'Décision favorable', Sparkles, 'emerald'],
    ['À optimiser', metrics.toOptimize, 'Potentiel à consolider', TrendingUp, 'amber'],
    ['Non viables', metrics.notViable, 'Décision défavorable', AlertTriangle, 'amber'],
    ['Score moyen', metrics.studied > 0 ? metrics.averageScore.toFixed(1) + '/100' : '—', 'Décisions validées', Gauge, 'blue'],
    ['Marge moyenne', metrics.studied > 0 ? metrics.averageMargin.toFixed(1) + '%' : '—', 'Décisions validées', CircleDollarSign, 'emerald'],
  ] as const

  return <div className={'space-y-8'}>
    <PageHeader eyebrow={'Responsable Commercial / Marketing'} title={'Intelligence marché et viabilité'}
      description={'Vue consolidée à partir des produits, processus, expériences et études réellement enregistrés sur la plateforme.'}
      actions={<><Button variant={'secondary'} onClick={() => dashboard.refetch()} loading={dashboard.isFetching}><RefreshCw className={'size-4'}/>Actualiser</Button><Link to={'/commercial/comparison'}><Button variant={'secondary'}>Comparer</Button></Link><Link to={'/commercial/products'}><Button>Lancer une étude</Button></Link></>}/>
    <section className={'grid gap-4 sm:grid-cols-2 xl:grid-cols-4'}>{cards.map(([label,value,detail,icon,accent]) => <StatCard key={label} label={label} value={String(value)} detail={detail} icon={icon} accent={accent}/>)}</section>
    <section className={'grid gap-4 lg:grid-cols-2'}><Highlight title={'Meilleur produit'} study={metrics.best}/><Highlight title={'Produit présentant le plus de risques'} study={metrics.riskiest}/></section>
    <CommercialDashboardCharts products={data.products} studies={data.studies} trends={data.trends}/>
    <section className={'surface-card p-5'}><h2 className={'font-bold'}>Dernières études</h2>{data.studies.length > 0 ? <div className={'mt-4 grid gap-3 md:grid-cols-2'}>{data.studies.slice(0,4).map(study => <Link key={study.id} to={'/commercial/studies/' + study.id} className={'rounded-xl border border-slate-200 p-4 hover:border-blue-300'}><div className={'flex justify-between gap-3'}><strong>{study.studyName}</strong>{study.feasibility && <RecommendationBadge recommendation={study.feasibility.recommendation}/>}</div><p className={'mt-2 text-xs text-slate-400'}>{study.targetMarket} · {study.feasibility ? 'score ' + study.feasibility.globalScore : 'calcul non validé'}</p></Link>)}</div> : <EmptyState title={'Aucune étude enregistrée'} description={'Les études créées sur la plateforme apparaîtront ici.'}/>}</section>
    <p className={'pb-2 text-center text-xs text-slate-400'}>Données actualisées {formatRelativeDate(data.updatedAt)}</p>
  </div>
}

function Highlight({ title, study }: { title: string; study: MarketStudy | null }) {
  return <article className={'surface-card p-5'}><p className={'text-xs font-semibold uppercase tracking-wider text-slate-400'}>{title}</p><h2 className={'mt-2 font-bold text-slate-950'}>{study?.studyName ?? 'Données insuffisantes'}</h2>{study?.feasibility && <div className={'mt-3 flex items-center justify-between'}><RecommendationBadge recommendation={study.feasibility.recommendation}/><strong>{study.feasibility.globalScore}/100</strong></div>}</article>
}
