import { ArrowLeftRight, BarChart3, Check, CircleDollarSign, Crown, RefreshCw, Scale, ShieldCheck, Sparkles, Target, TrendingUp } from 'lucide-react'
import { useMemo, useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { Legend, PolarAngleAxis, PolarGrid, Radar, RadarChart, ResponsiveContainer, Tooltip } from 'recharts'
import { ChartCard } from '../../../components/charts/ChartCard'
import { PageHeader } from '../../../components/common/PageHeader'
import { Button } from '../../../components/ui/Button'
import { EmptyState } from '../../../components/ui/EmptyState'
import { ErrorState } from '../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { Select } from '../../../components/ui/Select'
import { cn } from '../../../utils/cn'
import { RecommendationBadge } from '../components/results/RecommendationBadge'
import { useCommercialProducts } from '../hooks/useCommercialProducts'
import { useMarketStudies } from '../hooks/useMarketStudies'
import type { MarketStudy } from '../types/commercial.types'
import { formatCurrency } from '../../production/utils/productionFormatters'

type Side = 'left' | 'right'
type Winner = Side | 'tie' | 'neutral'

interface ComparisonMetric {
  label: string
  left: number
  right: number
  format: (value: number) => string
  preference: 'higher' | 'lower' | 'neutral'
}

const formatNumber = (value: number) => new Intl.NumberFormat('fr-MA', { maximumFractionDigits: 0 }).format(value)
const formatPercent = (value: number) => `${value.toFixed(1)} %`
const score = (value: number) => `${value.toFixed(0)}/100`

function metricWinner(metric: ComparisonMetric): Winner {
  if (metric.preference === 'neutral') return 'neutral'
  if (metric.left === metric.right) return 'tie'
  const leftWins = metric.preference === 'higher' ? metric.left > metric.right : metric.left < metric.right
  return leftWins ? 'left' : 'right'
}

export function CommercialComparisonPage() {
  const studiesQuery = useMarketStudies()
  const productsQuery = useCommercialProducts()
  const [leftId, setLeftId] = useState('')
  const [rightId, setRightId] = useState('')

  const studies = useMemo(() => (studiesQuery.data ?? [])
    .filter(study => study.status === 'Validated' && study.feasibility)
    .sort((left, right) => right.updatedAt.localeCompare(left.updatedAt)), [studiesQuery.data])

  if (studiesQuery.isLoading) return <LoadingSkeleton lines={10}/>
  if (studiesQuery.isError) {
    const reason = studiesQuery.error instanceof Error ? studiesQuery.error.message : 'Les études ne sont pas disponibles.'
    return <ErrorState title={'Comparateur indisponible'} description={reason} onRetry={() => studiesQuery.refetch()}/>
  }

  if (studies.length < 2) {
    return <div className={'space-y-6'}><PageHeader eyebrow={'Benchmark interne'} title={'Comparaison des études'} description={'Comparez les décisions commerciales validées sur une base identique.'}/><section className={'surface-card'}><EmptyState title={'Deux études validées sont nécessaires'} description={`La plateforme contient actuellement ${studies.length} étude validée. Validez au moins deux études pour activer le comparateur.`} action={<Link to={'/commercial/studies'}><Button>Voir les études</Button></Link>}/></section></div>
  }

  const normalizedLeftId = studies.some(study => study.id === leftId) ? leftId : studies[0]!.id
  const normalizedRightId = studies.some(study => study.id === rightId && study.id !== normalizedLeftId)
    ? rightId
    : studies.find(study => study.id !== normalizedLeftId)!.id
  const left = studies.find(study => study.id === normalizedLeftId)!
  const right = studies.find(study => study.id === normalizedRightId)!
  const productNames = new Map((productsQuery.data ?? []).map(view => [view.product.id, view.product.name]))
  const leftName = productNames.get(left.productId) ?? left.studyName
  const rightName = productNames.get(right.productId) ?? right.studyName
  const leftResult = left.feasibility!
  const rightResult = right.feasibility!
  const globalWinner: Winner = leftResult.globalScore === rightResult.globalScore ? 'tie' : leftResult.globalScore > rightResult.globalScore ? 'left' : 'right'
  const winnerStudy = globalWinner === 'left' ? left : globalWinner === 'right' ? right : null
  const scoreGap = Math.abs(leftResult.globalScore - rightResult.globalScore)

  const swap = () => {
    setLeftId(normalizedRightId)
    setRightId(normalizedLeftId)
  }

  const optionsFor = (excludedId: string) => studies
    .filter(study => study.id !== excludedId)
    .map(study => ({ label: `${productNames.get(study.productId) ?? study.studyName} · ${study.feasibility!.globalScore}/100`, value: study.id }))

  const radarData = [
    { criterion: 'Production', left: leftResult.productionScore, right: rightResult.productionScore },
    { criterion: 'Marché', left: leftResult.marketScore, right: rightResult.marketScore },
    { criterion: 'Finance', left: leftResult.financialScore, right: rightResult.financialScore },
    { criterion: 'Risques', left: leftResult.riskScore, right: rightResult.riskScore },
    { criterion: 'Global', left: leftResult.globalScore, right: rightResult.globalScore },
  ]
  const metrics: ComparisonMetric[] = [
    { label: 'Score global', left: leftResult.globalScore, right: rightResult.globalScore, format: score, preference: 'higher' },
    { label: 'Score production', left: leftResult.productionScore, right: rightResult.productionScore, format: score, preference: 'higher' },
    { label: 'Score marché', left: leftResult.marketScore, right: rightResult.marketScore, format: score, preference: 'higher' },
    { label: 'Score financier', left: leftResult.financialScore, right: rightResult.financialScore, format: score, preference: 'higher' },
    { label: 'Maîtrise des risques', left: leftResult.riskScore, right: rightResult.riskScore, format: score, preference: 'higher' },
    { label: 'Marge estimée', left: left.pricing.marginRate, right: right.pricing.marginRate, format: formatPercent, preference: 'higher' },
    { label: 'Marge unitaire', left: left.pricing.calculatedMargin, right: right.pricing.calculatedMargin, format: formatCurrency, preference: 'higher' },
    { label: 'CA annuel prévu', left: left.forecast.annualRevenue, right: right.forecast.annualRevenue, format: formatCurrency, preference: 'higher' },
    { label: 'Volume annuel prévu', left: left.forecast.annualSalesVolume, right: right.forecast.annualSalesVolume, format: value => `${formatNumber(value)} unités`, preference: 'higher' },
    { label: 'Taille du marché', left: left.estimatedMarketSize, right: right.estimatedMarketSize, format: formatCurrency, preference: 'higher' },
    { label: 'Coût de production', left: left.pricing.productionCost, right: right.pricing.productionCost, format: formatCurrency, preference: 'lower' },
    { label: 'Seuil de rentabilité', left: left.pricing.breakEvenUnits, right: right.pricing.breakEvenUnits, format: value => `${formatNumber(value)} unités`, preference: 'lower' },
  ]

  return <div className={'space-y-7'}>
    <PageHeader eyebrow={'Benchmark décisionnel'} title={'Comparateur de potentiel commercial'}
      description={'Mettez deux décisions validées face à face et identifiez le meilleur investissement à partir des données réelles de la plateforme.'}
      actions={<Button variant={'secondary'} onClick={() => Promise.all([studiesQuery.refetch(), productsQuery.refetch()])} loading={studiesQuery.isFetching || productsQuery.isFetching}><RefreshCw className={'size-4'}/>Actualiser</Button>}/>

    <section className={'relative overflow-hidden rounded-3xl bg-gradient-to-br from-slate-950 via-blue-950 to-blue-700 px-6 py-7 text-white shadow-xl shadow-blue-950/10 sm:px-8 lg:px-10'}>
      <div className={'absolute -right-24 -top-24 size-72 rounded-full bg-cyan-400/15 blur-3xl'} />
      <div className={'absolute -bottom-32 left-1/3 size-72 rounded-full bg-violet-500/15 blur-3xl'} />
      <div className={'relative grid items-center gap-8 lg:grid-cols-[1fr_auto]'}>
        <div><div className={'mb-4 inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/10 px-3 py-1.5 text-xs font-semibold text-blue-100 backdrop-blur'}><Sparkles className={'size-3.5'}/>Analyse comparative instantanée</div><h2 className={'max-w-3xl text-2xl font-bold tracking-tight sm:text-3xl'}>{winnerStudy ? `${productNames.get(winnerStudy.productId) ?? winnerStudy.studyName} prend l’avantage` : 'Les deux produits sont au même niveau'}</h2><p className={'mt-3 max-w-2xl text-sm leading-6 text-blue-100'}>{winnerStudy ? `Un écart de ${scoreGap.toFixed(0)} point${scoreGap > 1 ? 's' : ''} sur le score global. Vérifiez les critères financiers et opérationnels avant la décision finale.` : 'Les scores globaux sont identiques. La marge, les revenus et le niveau de risque permettront de départager les options.'}</p></div>
        <div className={'flex items-center gap-4 rounded-2xl border border-white/15 bg-white/10 p-4 backdrop-blur-md'}><div className={'grid size-16 place-items-center rounded-2xl bg-white text-2xl font-black text-blue-700'}>{scoreGap.toFixed(0)}</div><div><p className={'text-[11px] font-semibold uppercase tracking-[.18em] text-blue-200'}>Écart global</p><p className={'mt-1 font-semibold'}>{globalWinner === 'tie' ? 'Égalité' : 'point(s)'}</p></div></div>
      </div>
    </section>

    <section className={'surface-card relative grid gap-5 p-5 lg:grid-cols-[1fr_auto_1fr] lg:items-end lg:p-6'}>
      <div className={'rounded-2xl border border-blue-100 bg-blue-50/70 p-4'}><div className={'mb-3 flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-blue-700'}><span className={'grid size-7 place-items-center rounded-lg bg-blue-600 text-white'}>A</span>Premier scénario</div><Select label={'Produit / étude validée'} value={normalizedLeftId} onChange={event => setLeftId(event.target.value)} options={optionsFor(normalizedRightId)}/></div>
      <Button variant={'secondary'} size={'icon'} className={'mx-auto mb-2 rounded-full lg:rotate-0'} onClick={swap} aria-label={'Inverser les produits'} title={'Inverser les produits'}><ArrowLeftRight className={'size-4'}/></Button>
      <div className={'rounded-2xl border border-violet-100 bg-violet-50/70 p-4'}><div className={'mb-3 flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-violet-700'}><span className={'grid size-7 place-items-center rounded-lg bg-violet-600 text-white'}>B</span>Deuxième scénario</div><Select label={'Produit / étude validée'} value={normalizedRightId} onChange={event => setRightId(event.target.value)} options={optionsFor(normalizedLeftId)}/></div>
    </section>

    <section className={'grid gap-5 xl:grid-cols-2'}>
      <StudyCard side={'left'} study={left} productName={leftName} winner={globalWinner === 'left'}/>
      <StudyCard side={'right'} study={right} productName={rightName} winner={globalWinner === 'right'}/>
    </section>

    <section className={cn('overflow-hidden rounded-2xl border p-5 sm:p-6', globalWinner === 'tie' ? 'border-slate-200 bg-slate-50' : 'border-emerald-200 bg-gradient-to-r from-emerald-50 to-white')}>
      <div className={'flex flex-col justify-between gap-5 md:flex-row md:items-center'}><div className={'flex items-start gap-4'}><span className={cn('grid size-12 shrink-0 place-items-center rounded-2xl', globalWinner === 'tie' ? 'bg-slate-200 text-slate-700' : 'bg-emerald-500 text-white shadow-lg shadow-emerald-200')} >{globalWinner === 'tie' ? <Scale className={'size-6'}/> : <Crown className={'size-6'}/>}</span><div><p className={'text-xs font-bold uppercase tracking-[.18em] text-emerald-700'}>Verdict synthétique</p><h2 className={'mt-1 text-xl font-bold text-slate-950'}>{winnerStudy ? `${productNames.get(winnerStudy.productId) ?? winnerStudy.studyName} présente le meilleur score global` : 'Équilibre parfait sur le score global'}</h2><p className={'mt-2 max-w-3xl text-sm leading-6 text-slate-600'}>{winnerStudy ? `Cette option mène avec ${winnerStudy.feasibility!.globalScore}/100. Le verdict combine production, marché, finance et maîtrise des risques ; le tableau détaillé ci-dessous montre les critères qui expliquent l’écart.` : 'Le score global ne suffit pas à départager les deux options. Privilégiez la meilleure marge, le chiffre d’affaires projeté et le coût de production.'}</p></div></div>{winnerStudy && <Link to={'/commercial/studies/' + winnerStudy.id + '/result'}><Button>Ouvrir le résultat<TrendingUp className={'size-4'}/></Button></Link>}</div>
    </section>

    <section className={'grid gap-5 xl:grid-cols-[1.05fr_.95fr]'}>
      <ChartCard title={'Empreinte multicritère'} description={'Plus la surface est étendue, plus le profil est solide'}>
        <ResponsiveContainer><RadarChart data={radarData} outerRadius={'66%'}><PolarGrid stroke={'#cbd5e1'}/><PolarAngleAxis dataKey={'criterion'} tick={{ fill: '#475569', fontSize: 12 }}/><Radar name={leftName} dataKey={'left'} stroke={'#2563eb'} fill={'#2563eb'} fillOpacity={.22} strokeWidth={2}/><Radar name={rightName} dataKey={'right'} stroke={'#7c3aed'} fill={'#7c3aed'} fillOpacity={.18} strokeWidth={2}/><Tooltip/><Legend/></RadarChart></ResponsiveContainer>
      </ChartCard>
      <section className={'surface-card p-5 sm:p-6'}><div className={'flex items-center gap-3'}><span className={'grid size-10 place-items-center rounded-xl bg-blue-50 text-blue-600'}><Target className={'size-5'}/></span><div><h2 className={'font-bold text-slate-950'}>Avantages décisifs</h2><p className={'text-xs text-slate-500'}>Forces calculées pour chaque étude</p></div></div><div className={'mt-5 grid gap-4 sm:grid-cols-2'}><StrengthList label={leftName} tone={'blue'} values={leftResult.strengths}/><StrengthList label={rightName} tone={'violet'} values={rightResult.strengths}/></div></section>
    </section>

    <ComparisonTable leftName={leftName} rightName={rightName} metrics={metrics}/>
  </div>
}

function StudyCard({ side, study, productName, winner }: { side: Side; study: MarketStudy; productName: string; winner: boolean }) {
  const result = study.feasibility!
  const isLeft = side === 'left'
  return <article className={cn('relative overflow-hidden rounded-3xl border bg-white shadow-sm transition duration-300 hover:-translate-y-1 hover:shadow-xl', winner ? 'border-emerald-300 ring-4 ring-emerald-50' : 'border-slate-200')}>
    <div className={cn('relative overflow-hidden px-6 py-5 text-white', isLeft ? 'bg-gradient-to-br from-blue-700 via-blue-600 to-cyan-500' : 'bg-gradient-to-br from-violet-700 via-violet-600 to-fuchsia-500')}><div className={'absolute -right-8 -top-12 size-40 rounded-full bg-white/10'} /><div className={'relative flex items-start justify-between gap-4'}><div><div className={'flex items-center gap-2 text-xs font-bold uppercase tracking-[.18em] text-white/75'}><span className={'grid size-7 place-items-center rounded-lg bg-white/15'}>{isLeft ? 'A' : 'B'}</span>Scénario commercial</div><h2 className={'mt-4 text-xl font-bold'}>{productName}</h2><p className={'mt-1 text-sm text-white/75'}>{study.studyName} · version {study.productVersion}</p></div>{winner && <span className={'inline-flex items-center gap-1.5 rounded-full bg-white px-3 py-1.5 text-xs font-bold text-emerald-700 shadow-sm'}><Crown className={'size-3.5'}/>Leader</span>}</div></div>
    <div className={'p-5 sm:p-6'}><div className={'flex items-center justify-between gap-5'}><ScoreRing value={result.globalScore} tone={isLeft ? 'blue' : 'violet'}/><div className={'min-w-0 flex-1'}><p className={'text-xs font-semibold uppercase tracking-wider text-slate-400'}>Recommandation</p><div className={'mt-2'}><RecommendationBadge recommendation={result.recommendation}/></div><p className={'mt-3 text-sm leading-5 text-slate-500'}>{study.targetMarket} · {study.geographicArea}</p></div></div>
      <div className={'mt-6 grid grid-cols-3 gap-2'}><MiniMetric icon={<CircleDollarSign className={'size-4'}/>} label={'Marge'} value={formatPercent(study.pricing.marginRate)}/><MiniMetric icon={<TrendingUp className={'size-4'}/>} label={'CA annuel'} value={formatCompactCurrency(study.forecast.annualRevenue)}/><MiniMetric icon={<ShieldCheck className={'size-4'}/>} label={'Risques'} value={score(result.riskScore)}/></div>
      <div className={'mt-5 flex items-center justify-between border-t border-slate-100 pt-4'}><p className={'text-xs text-slate-400'}>{study.competitors.length} concurrent(s) · {study.risks.length} risque(s)</p><Link className={cn('text-sm font-semibold', isLeft ? 'text-blue-600 hover:text-blue-700' : 'text-violet-600 hover:text-violet-700')} to={'/commercial/studies/' + study.id + '/result'}>Voir l’analyse →</Link></div>
    </div>
  </article>
}

function ScoreRing({ value, tone }: { value: number; tone: 'blue' | 'violet' }) {
  const color = tone === 'blue' ? '#2563eb' : '#7c3aed'
  return <div className={'grid size-24 shrink-0 place-items-center rounded-full p-2'} style={{ background: `conic-gradient(${color} ${value * 3.6}deg, #e2e8f0 0deg)` }}><div className={'grid size-full place-items-center rounded-full bg-white text-center'}><div><strong className={'text-2xl text-slate-950'}>{value}</strong><span className={'block text-[10px] font-semibold text-slate-400'}>/ 100</span></div></div></div>
}

function MiniMetric({ icon, label, value }: { icon: ReactNode; label: string; value: string }) {
  return <div className={'min-w-0 rounded-xl bg-slate-50 p-3'}><span className={'text-slate-400'}>{icon}</span><p className={'mt-2 truncate text-[10px] font-semibold uppercase tracking-wide text-slate-400'}>{label}</p><p className={'mt-0.5 truncate text-xs font-bold text-slate-800 sm:text-sm'}>{value}</p></div>
}

const formatCompactCurrency = (value: number) => new Intl.NumberFormat('fr-MA', { style: 'currency', currency: 'MAD', notation: 'compact', maximumFractionDigits: 1 }).format(value)

function StrengthList({ label, tone, values }: { label: string; tone: 'blue' | 'violet'; values: string[] }) {
  const shown = values.slice(0, 4)
  return <div className={cn('rounded-2xl border p-4', tone === 'blue' ? 'border-blue-100 bg-blue-50/60' : 'border-violet-100 bg-violet-50/60')}><p className={cn('truncate text-xs font-bold uppercase tracking-wider', tone === 'blue' ? 'text-blue-700' : 'text-violet-700')}>{label}</p>{shown.length > 0 ? <ul className={'mt-3 space-y-2'}>{shown.map(value => <li key={value} className={'flex items-start gap-2 text-sm text-slate-700'}><span className={cn('mt-0.5 grid size-4 shrink-0 place-items-center rounded-full text-white', tone === 'blue' ? 'bg-blue-500' : 'bg-violet-500')}><Check className={'size-2.5'}/></span><span>{value}</span></li>)}</ul> : <p className={'mt-3 text-sm text-slate-500'}>Aucun point fort renseigné.</p>}</div>
}

function ComparisonTable({ leftName, rightName, metrics }: { leftName: string; rightName: string; metrics: ComparisonMetric[] }) {
  const leftWins = metrics.filter(metric => metricWinner(metric) === 'left').length
  const rightWins = metrics.filter(metric => metricWinner(metric) === 'right').length
  return <section className={'surface-card overflow-hidden'}>
    <div className={'flex flex-col justify-between gap-4 border-b border-slate-200 px-5 py-5 sm:flex-row sm:items-center sm:px-6'}><div><div className={'flex items-center gap-2'}><BarChart3 className={'size-5 text-blue-600'}/><h2 className={'font-bold text-slate-950'}>Matrice de décision</h2></div><p className={'mt-1 text-sm text-slate-500'}>Les meilleures valeurs sont mises en évidence automatiquement.</p></div><div className={'flex gap-2 text-xs font-bold'}><span className={'rounded-full bg-blue-50 px-3 py-1.5 text-blue-700'}>A · {leftWins} avantage(s)</span><span className={'rounded-full bg-violet-50 px-3 py-1.5 text-violet-700'}>B · {rightWins} avantage(s)</span></div></div>
    <div className={'overflow-x-auto'}><table className={'w-full min-w-[680px] border-collapse text-sm'}><thead><tr className={'bg-slate-50 text-left text-xs uppercase tracking-wider text-slate-400'}><th className={'w-[32%] px-6 py-4'}>{leftName}</th><th className={'w-[36%] px-6 py-4 text-center'}>Indicateur</th><th className={'w-[32%] px-6 py-4 text-right'}>{rightName}</th></tr></thead><tbody className={'divide-y divide-slate-100'}>{metrics.map(metric => <MetricRow key={metric.label} metric={metric}/>)}</tbody></table></div>
  </section>
}

function MetricRow({ metric }: { metric: ComparisonMetric }) {
  const winner = metricWinner(metric)
  const difference = Math.abs(metric.left - metric.right)
  return <tr className={'transition hover:bg-slate-50/70'}><td className={'px-6 py-4'}><MetricValue value={metric.format(metric.left)} winning={winner === 'left'} tone={'blue'}/></td><td className={'px-6 py-4 text-center'}><p className={'font-semibold text-slate-700'}>{metric.label}</p>{metric.preference !== 'neutral' && difference > 0 && <p className={'mt-0.5 text-[11px] text-slate-400'}>écart {metric.format(difference)}</p>}</td><td className={'px-6 py-4'}><MetricValue value={metric.format(metric.right)} winning={winner === 'right'} tone={'violet'} align={'right'}/></td></tr>
}

function MetricValue({ value, winning, tone, align = 'left' }: { value: string; winning: boolean; tone: 'blue' | 'violet'; align?: 'left' | 'right' }) {
  return <div className={cn('flex items-center gap-2', align === 'right' && 'justify-end')}><strong className={cn('rounded-lg px-2.5 py-1.5 text-sm', winning ? tone === 'blue' ? 'bg-blue-50 text-blue-700' : 'bg-violet-50 text-violet-700' : 'text-slate-700')}>{value}</strong>{winning && <span className={cn('grid size-5 place-items-center rounded-full text-white', tone === 'blue' ? 'bg-blue-600' : 'bg-violet-600')}><Check className={'size-3'}/></span>}</div>
}
