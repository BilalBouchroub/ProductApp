import { BarChart3, CalendarDays, CheckCircle2, Download, FileSpreadsheet, Filter, Printer, RefreshCw, Sparkles, TrendingUp, WalletCards } from 'lucide-react'
import { useMemo, useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { toast } from 'sonner'
import { PageHeader } from '../../../components/common/PageHeader'
import { StatusBadge } from '../../../components/common/StatusBadge'
import { Button } from '../../../components/ui/Button'
import { EmptyState } from '../../../components/ui/EmptyState'
import { ErrorState } from '../../../components/ui/ErrorState'
import { Input } from '../../../components/ui/Input'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { Select } from '../../../components/ui/Select'
import { cn } from '../../../utils/cn'
import { formatCurrency, formatProductionDate } from '../../production/utils/productionFormatters'
import { RecommendationBadge } from '../components/results/RecommendationBadge'
import { useCommercialProducts } from '../hooks/useCommercialProducts'
import { useMarketStudies } from '../hooks/useMarketStudies'
import { buildExecutiveReportHtml, buildPortfolioCsv, type CommercialReportRow } from '../services/commercialReportExport'
import type { CommercialRecommendation, MarketStudyStatus } from '../types/commercial.types'

interface ReportFilters { search: string; status: '' | MarketStudyStatus; recommendation: '' | CommercialRecommendation; from: string; to: string }
const emptyFilters: ReportFilters = { search: '', status: '', recommendation: '', from: '', to: '' }

export function CommercialReportsPage() {
  const studiesQuery = useMarketStudies()
  const productsQuery = useCommercialProducts()
  const [filters, setFilters] = useState<ReportFilters>(emptyFilters)
  const productNames = useMemo(() => new Map((productsQuery.data ?? []).map(view => [view.product.id, view.product.name])), [productsQuery.data])
  const rows = useMemo<CommercialReportRow[]>(() => (studiesQuery.data ?? []).filter(study => {
    const query = filters.search.trim().toLowerCase()
    const searchable = `${study.studyName} ${study.targetMarket} ${productNames.get(study.productId) ?? ''}`.toLowerCase()
    return (!query || searchable.includes(query)) && (!filters.status || study.status === filters.status)
      && (!filters.recommendation || study.feasibility?.recommendation === filters.recommendation)
      && (!filters.from || study.studyDate.slice(0, 10) >= filters.from) && (!filters.to || study.studyDate.slice(0, 10) <= filters.to)
  }).sort((left, right) => right.updatedAt.localeCompare(left.updatedAt)).map(study => ({ study, productName: productNames.get(study.productId) ?? study.studyName })), [studiesQuery.data, productNames, filters])

  if (studiesQuery.isLoading) return <LoadingSkeleton lines={10}/>
  if (studiesQuery.isError) {
    const reason = studiesQuery.error instanceof Error ? studiesQuery.error.message : 'Le service de reporting ne répond pas.'
    return <ErrorState title={'Rapports indisponibles'} description={reason} onRetry={() => studiesQuery.refetch()}/>
  }

  const validated = rows.filter(row => row.study.status === 'Validated' && row.study.feasibility)
  const viable = validated.filter(row => row.study.feasibility?.recommendation === 'Viable').length
  const averageScore = validated.length ? validated.reduce((sum, row) => sum + row.study.feasibility!.globalScore, 0) / validated.length : 0
  const averageMargin = validated.length ? validated.reduce((sum, row) => sum + row.study.pricing.marginRate, 0) / validated.length : 0
  const annualRevenue = validated.reduce((sum, row) => sum + row.study.forecast.annualRevenue, 0)
  const activeFilters = Object.values(filters).filter(Boolean).length

  const downloadCsv = () => {
    if (!rows.length) return toast.error('Aucune étude à exporter avec ces filtres.')
    downloadFile(`portefeuille-commercial-${new Date().toISOString().slice(0, 10)}.csv`, buildPortfolioCsv(rows), 'text/csv;charset=utf-8')
    toast.success(`${rows.length} étude(s) exportée(s) pour Excel.`)
  }
  const printReport = () => {
    if (!rows.length) return toast.error('Aucune étude à inclure dans le rapport.')
    const reportWindow = window.open('', '_blank', 'width=1200,height=800')
    if (!reportWindow) return toast.error('Autorisez les fenêtres contextuelles pour ouvrir le rapport.')
    reportWindow.opener = null
    reportWindow.document.write(buildExecutiveReportHtml(rows))
    reportWindow.document.close()
    reportWindow.focus()
    window.setTimeout(() => reportWindow.print(), 300)
    toast.success('Rapport prêt à imprimer ou enregistrer en PDF.')
  }

  return <div className={'space-y-7'}>
    <PageHeader eyebrow={'Reporting commercial'} title={'Centre de rapports et d’exports'}
      description={'Construisez vos livrables à partir des études réellement enregistrées, filtrez le périmètre et exportez uniquement les données utiles.'}
      actions={<Button variant={'secondary'} onClick={() => Promise.all([studiesQuery.refetch(), productsQuery.refetch()])} loading={studiesQuery.isFetching || productsQuery.isFetching}><RefreshCw className={'size-4'}/>Actualiser</Button>}/>

    <section className={'relative overflow-hidden rounded-3xl bg-gradient-to-br from-slate-950 via-blue-950 to-indigo-700 px-6 py-8 text-white shadow-xl shadow-blue-950/10 sm:px-8 lg:px-10'}>
      <div className={'absolute -right-20 -top-20 size-72 rounded-full bg-cyan-400/15 blur-3xl'} />
      <div className={'absolute -bottom-32 left-1/3 size-72 rounded-full bg-violet-500/20 blur-3xl'} />
      <div className={'relative grid items-center gap-8 lg:grid-cols-[1fr_auto]'}><div><div className={'mb-4 inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/10 px-3 py-1.5 text-xs font-semibold text-blue-100 backdrop-blur'}><Sparkles className={'size-3.5'}/>Données consolidées en temps réel</div><h2 className={'max-w-3xl text-2xl font-bold tracking-tight sm:text-3xl'}>Transformez le portefeuille commercial en document de décision</h2><p className={'mt-3 max-w-2xl text-sm leading-6 text-blue-100'}>Les exports respectent les filtres actifs et reprennent les scores, projections financières, recommandations, concurrents et risques de chaque étude.</p></div><div className={'flex flex-col gap-2 sm:flex-row lg:flex-col'}><Button className={'bg-white text-blue-700 hover:bg-blue-50'} size={'lg'} onClick={printReport} disabled={!rows.length}><Printer className={'size-4'}/>Imprimer / PDF</Button><Button className={'border-white/20 bg-white/10 text-white hover:bg-white/20'} size={'lg'} onClick={downloadCsv} disabled={!rows.length}><FileSpreadsheet className={'size-4'}/>Exporter vers Excel</Button></div></div>
    </section>

    <section aria-label={'Indicateurs du rapport'} className={'grid gap-4 sm:grid-cols-2 xl:grid-cols-5'}>
      <ReportKpi icon={<WalletCards className={'size-5'}/>} label={'Études incluses'} value={String(rows.length)} detail={`${validated.length} validée(s)`} tone={'blue'}/>
      <ReportKpi icon={<CheckCircle2 className={'size-5'}/>} label={'Produits viables'} value={String(viable)} detail={validated.length ? `${Math.round(viable / validated.length * 100)} % des décisions` : 'Aucune décision'} tone={'emerald'}/>
      <ReportKpi icon={<BarChart3 className={'size-5'}/>} label={'Score moyen'} value={validated.length ? `${averageScore.toFixed(1)}/100` : '—'} detail={'Décisions validées'} tone={'violet'}/>
      <ReportKpi icon={<TrendingUp className={'size-5'}/>} label={'Marge moyenne'} value={validated.length ? `${averageMargin.toFixed(1)} %` : '—'} detail={'Décisions validées'} tone={'amber'}/>
      <ReportKpi icon={<Download className={'size-5'}/>} label={'CA annuel prévu'} value={validated.length ? compactCurrency(annualRevenue) : '—'} detail={'Portefeuille filtré'} tone={'blue'}/>
    </section>

    <section className={'surface-card p-5 sm:p-6'}><div className={'flex flex-col justify-between gap-3 sm:flex-row sm:items-center'}><div className={'flex items-center gap-3'}><span className={'grid size-10 place-items-center rounded-xl bg-blue-50 text-blue-600'}><Filter className={'size-5'}/></span><div><h2 className={'font-bold text-slate-950'}>Périmètre du rapport</h2><p className={'text-xs text-slate-500'}>{activeFilters ? `${activeFilters} filtre(s) actif(s) · ${rows.length} résultat(s)` : 'Toutes les études sont incluses'}</p></div></div>{activeFilters > 0 && <Button variant={'ghost'} size={'sm'} onClick={() => setFilters(emptyFilters)}>Réinitialiser</Button>}</div>
      <div className={'mt-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-5'}><Input label={'Recherche'} value={filters.search} onChange={event => setFilters(current => ({ ...current, search: event.target.value }))} placeholder={'Produit, étude, marché…'}/><Select label={'Statut'} value={filters.status} onChange={event => setFilters(current => ({ ...current, status: event.target.value as ReportFilters['status'] }))} options={[{ label: 'Tous les statuts', value: '' }, { label: 'Brouillon', value: 'Draft' }, { label: 'En cours', value: 'InProgress' }, { label: 'Terminée', value: 'Completed' }, { label: 'Validée', value: 'Validated' }]}/><Select label={'Recommandation'} value={filters.recommendation} onChange={event => setFilters(current => ({ ...current, recommendation: event.target.value as ReportFilters['recommendation'] }))} options={[{ label: 'Toutes', value: '' }, { label: 'Viable', value: 'Viable' }, { label: 'À optimiser', value: 'ToOptimize' }, { label: 'Non viable', value: 'NotViable' }]}/><Input label={'Du'} type={'date'} value={filters.from} max={filters.to || undefined} onChange={event => setFilters(current => ({ ...current, from: event.target.value }))}/><Input label={'Au'} type={'date'} value={filters.to} min={filters.from || undefined} onChange={event => setFilters(current => ({ ...current, to: event.target.value }))}/></div>
    </section>

    <section className={'grid gap-5 xl:grid-cols-[1.15fr_.85fr]'}>
      <div className={'grid gap-4 sm:grid-cols-2'}><ExportCard icon={<Printer className={'size-6'}/>} title={'Rapport exécutif'} description={'Synthèse prête pour une réunion : indicateurs, recommandations et portefeuille détaillé.'} meta={'Format A4 paysage · impression ou PDF'} tone={'blue'} actionLabel={'Créer le rapport'} onClick={printReport} disabled={!rows.length}/><ExportCard icon={<FileSpreadsheet className={'size-6'}/>} title={'Base analytique Excel'} description={'24 colonnes exploitables : finance, marché, scores, risques et concurrence.'} meta={'CSV UTF-8 · compatible Excel'} tone={'emerald'} actionLabel={'Télécharger le fichier'} onClick={downloadCsv} disabled={!rows.length}/></div>
      <PortfolioQuality rows={rows}/>
    </section>

    <ReportTable rows={rows}/>
  </div>
}

function downloadFile(name: string, content: string, type: string) {
  const url = URL.createObjectURL(new Blob([content], { type }))
  const link = document.createElement('a')
  link.href = url
  link.download = name
  link.click()
  URL.revokeObjectURL(url)
}

const compactCurrency = (value: number) => new Intl.NumberFormat('fr-MA', { style: 'currency', currency: 'MAD', notation: 'compact', maximumFractionDigits: 1 }).format(value)

function ReportKpi({ icon, label, value, detail, tone }: { icon: ReactNode; label: string; value: string; detail: string; tone: 'blue' | 'emerald' | 'violet' | 'amber' }) {
  const colors = { blue: 'bg-blue-50 text-blue-600', emerald: 'bg-emerald-50 text-emerald-600', violet: 'bg-violet-50 text-violet-600', amber: 'bg-amber-50 text-amber-600' }
  return <article className={'surface-card p-4 transition duration-300 hover:-translate-y-1 hover:shadow-lg'}><div className={cn('grid size-10 place-items-center rounded-xl', colors[tone])}>{icon}</div><p className={'mt-4 text-xs font-semibold uppercase tracking-wider text-slate-400'}>{label}</p><p className={'mt-1 text-2xl font-bold tracking-tight text-slate-950'}>{value}</p><p className={'mt-1 text-xs text-slate-500'}>{detail}</p></article>
}

function ExportCard({ icon, title, description, meta, tone, actionLabel, onClick, disabled }: { icon: ReactNode; title: string; description: string; meta: string; tone: 'blue' | 'emerald'; actionLabel: string; onClick: () => void; disabled: boolean }) {
  return <article className={cn('relative overflow-hidden rounded-2xl border p-5 shadow-sm', tone === 'blue' ? 'border-blue-100 bg-gradient-to-br from-white to-blue-50' : 'border-emerald-100 bg-gradient-to-br from-white to-emerald-50')}><div className={cn('grid size-12 place-items-center rounded-2xl text-white shadow-lg', tone === 'blue' ? 'bg-blue-600 shadow-blue-200' : 'bg-emerald-600 shadow-emerald-200')}>{icon}</div><h2 className={'mt-5 font-bold text-slate-950'}>{title}</h2><p className={'mt-2 text-sm leading-6 text-slate-500'}>{description}</p><div className={'mt-5 flex items-center gap-2 text-xs text-slate-400'}><Download className={'size-3.5'}/>{meta}</div><Button className={'mt-5 w-full'} variant={tone === 'blue' ? 'primary' : 'secondary'} onClick={onClick} disabled={disabled}>{actionLabel}</Button></article>
}

function PortfolioQuality({ rows }: { rows: CommercialReportRow[] }) {
  const validated = rows.filter(row => row.study.status === 'Validated' && row.study.feasibility)
  const values = [
    { label: 'Viables', count: validated.filter(row => row.study.feasibility?.recommendation === 'Viable').length, color: 'bg-emerald-500' },
    { label: 'À optimiser', count: validated.filter(row => row.study.feasibility?.recommendation === 'ToOptimize').length, color: 'bg-amber-500' },
    { label: 'Non viables', count: validated.filter(row => row.study.feasibility?.recommendation === 'NotViable').length, color: 'bg-red-500' },
  ]
  const completion = rows.length ? Math.round(validated.length / rows.length * 100) : 0
  return <section className={'surface-card p-5 sm:p-6'}><div className={'flex items-center justify-between gap-3'}><div><h2 className={'font-bold text-slate-950'}>Qualité du portefeuille</h2><p className={'mt-1 text-xs text-slate-500'}>Couverture des décisions dans le rapport</p></div><span className={'grid size-14 place-items-center rounded-full border-4 border-blue-100 bg-blue-50 text-sm font-bold text-blue-700'}>{completion}%</span></div><div className={'mt-6 space-y-4'}>{values.map(item => { const percentage = validated.length ? item.count / validated.length * 100 : 0; return <div key={item.label}><div className={'mb-1.5 flex items-center justify-between text-xs'}><span className={'font-semibold text-slate-600'}>{item.label}</span><span className={'text-slate-400'}>{item.count}</span></div><div className={'h-2 overflow-hidden rounded-full bg-slate-100'}><div className={cn('h-full rounded-full transition-all', item.color)} style={{ width: `${percentage}%` }}/></div></div>})}</div><p className={'mt-5 rounded-xl bg-slate-50 p-3 text-xs leading-5 text-slate-500'}>{validated.length} décision(s) validée(s) sur {rows.length} étude(s) dans le périmètre actuel.</p></section>
}

function ReportTable({ rows }: { rows: CommercialReportRow[] }) {
  if (!rows.length) return <section className={'surface-card'}><EmptyState title={'Aucune étude dans ce périmètre'} description={'Modifiez ou réinitialisez les filtres pour alimenter le rapport.'}/></section>
  return <section className={'surface-card overflow-hidden'}><div className={'flex items-center justify-between gap-3 border-b border-slate-200 px-5 py-5 sm:px-6'}><div className={'flex items-center gap-3'}><span className={'grid size-10 place-items-center rounded-xl bg-violet-50 text-violet-600'}><CalendarDays className={'size-5'}/></span><div><h2 className={'font-bold text-slate-950'}>Aperçu du rapport</h2><p className={'text-xs text-slate-500'}>{rows.length} ligne(s) seront exportée(s)</p></div></div></div><div className={'overflow-x-auto'}><table className={'w-full min-w-[900px] text-left text-sm'}><thead className={'bg-slate-50 text-xs uppercase tracking-wider text-slate-400'}><tr><th className={'px-5 py-4'}>Produit / étude</th><th className={'px-5 py-4'}>Date</th><th className={'px-5 py-4'}>Statut</th><th className={'px-5 py-4'}>Décision</th><th className={'px-5 py-4 text-right'}>Marge</th><th className={'px-5 py-4 text-right'}>CA annuel prévu</th><th className={'px-5 py-4 text-right'}>Action</th></tr></thead><tbody className={'divide-y divide-slate-100'}>{rows.map(({ study, productName }) => <tr key={study.id} className={'transition hover:bg-slate-50/70'}><td className={'px-5 py-4'}><p className={'font-semibold text-slate-900'}>{productName}</p><p className={'mt-0.5 max-w-64 truncate text-xs text-slate-400'}>{study.studyName} · v{study.productVersion}</p></td><td className={'px-5 py-4 text-slate-600'}>{formatProductionDate(study.studyDate)}</td><td className={'px-5 py-4'}><StudyStatus status={study.status}/></td><td className={'px-5 py-4'}>{study.feasibility ? <div className={'flex items-center gap-2'}><RecommendationBadge recommendation={study.feasibility.recommendation}/><strong className={'text-slate-700'}>{study.feasibility.globalScore}/100</strong></div> : <span className={'text-xs text-slate-400'}>Calcul non validé</span>}</td><td className={'px-5 py-4 text-right font-semibold text-slate-700'}>{study.feasibility ? `${study.pricing.marginRate.toFixed(1)} %` : '—'}</td><td className={'px-5 py-4 text-right font-semibold text-slate-700'}>{study.feasibility ? formatCurrency(study.forecast.annualRevenue) : '—'}</td><td className={'px-5 py-4 text-right'}><Link to={'/commercial/studies/' + study.id}><Button size={'sm'} variant={'ghost'}>Consulter</Button></Link></td></tr>)}</tbody></table></div></section>
}

function StudyStatus({ status }: { status: MarketStudyStatus }) {
  const values: Record<MarketStudyStatus, { label: string; tone: 'neutral' | 'info' | 'success' | 'warning' }> = {
    Draft: { label: 'Brouillon', tone: 'neutral' }, InProgress: { label: 'En cours', tone: 'info' },
    Completed: { label: 'Terminée', tone: 'warning' }, Validated: { label: 'Validée', tone: 'success' },
  }
  return <StatusBadge {...values[status]}/>
}
