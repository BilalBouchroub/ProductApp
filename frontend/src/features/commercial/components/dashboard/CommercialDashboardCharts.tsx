import { Bar, BarChart, CartesianGrid, Cell, Legend, Line, LineChart, Pie, PieChart, ResponsiveContainer, Scatter, ScatterChart, Tooltip, XAxis, YAxis, ZAxis } from 'recharts'
import { ChartCard } from '../../../../components/charts/ChartCard'
import { EmptyState } from '../../../../components/ui/EmptyState'
import type { CommercialProductView, MarketStudy, MarketTrendPoint } from '../../types/commercial.types'
import { CommercialCriteriaRadar } from '../analytics/CommercialVisualizations'

interface DashboardChartsProps { products: CommercialProductView[]; studies: MarketStudy[]; trends: MarketTrendPoint[] }

function latestValidatedStudies(studies: MarketStudy[]): MarketStudy[] {
  const latest = new Map<string, MarketStudy>()
  for (const study of [...studies].sort((left, right) => right.updatedAt.localeCompare(left.updatedAt))) {
    if (study.status !== 'Validated' || !study.feasibility) continue
    const key = study.productId + ':' + study.productVersion
    if (!latest.has(key)) latest.set(key, study)
  }
  return [...latest.values()]
}

function NoChartData({ description }: { description: string }) {
  return <EmptyState title={'Données insuffisantes'} description={description} />
}

export function CommercialDashboardCharts({ products, studies, trends }: DashboardChartsProps) {
  const decisions = latestValidatedStudies(studies)
  const decisionByProduct = new Map(decisions.map(study => [study.productId + ':' + study.productVersion, study]))
  const productData = products.map(view => {
    const study = decisionByProduct.get(view.product.id + ':' + view.product.version)
    return { name: view.product.name.slice(0, 18), cost: view.productionCost, target: view.product.targetSalePrice,
      market: study?.pricing.averageMarketPrice ?? null, margin: study?.pricing.marginRate ?? null,
      score: study?.feasibility?.globalScore ?? null }
  })
  const recommendations = [
    { name: 'Viable', value: decisions.filter(study => study.feasibility?.recommendation === 'Viable').length },
    { name: 'À optimiser', value: decisions.filter(study => study.feasibility?.recommendation === 'ToOptimize').length },
    { name: 'Non viable', value: decisions.filter(study => study.feasibility?.recommendation === 'NotViable').length },
  ]
  const categoryScores = new Map<string, number[]>()
  for (const product of products) {
    const study = decisionByProduct.get(product.product.id + ':' + product.product.version)
    if (!study) continue
    const scores = categoryScores.get(product.product.category) ?? []
    scores.push(study.feasibility!.globalScore)
    categoryScores.set(product.product.category, scores)
  }
  const categories = [...categoryScores].map(([name, scores]) => ({ name: name.slice(0, 18),
    potential: scores.reduce((total, score) => total + score, 0) / scores.length }))
  const competitors = decisions.flatMap(study => study.competitors.map(competitor => ({
    name: competitor.competitorName, prix: competitor.price, qualité: competitor.estimatedQuality, part: competitor.marketShare })))
  const risks = decisions.flatMap(study => study.risks.map(risk => ({
    x: risk.probability, y: risk.impact, z: risk.probability * risk.impact, name: risk.description })))
  const common = <CartesianGrid strokeDasharray={'3 3'} vertical={false} />
  const hasDecisions = decisions.length > 0
  return <section className={'grid min-w-0 gap-5 xl:grid-cols-2'}>
    <ChartCard title={'Coût, prix cible et prix marché'} description={'Prix marché uniquement issu des études validées'}>
      {productData.length > 0 ? <ResponsiveContainer><BarChart data={productData}>{common}<XAxis dataKey={'name'}/><YAxis/><Tooltip/><Legend/><Bar dataKey={'cost'} name={'Coût de production'} fill={'#64748b'}/><Bar dataKey={'target'} name={'Prix cible'} fill={'#2563eb'}/><Bar dataKey={'market'} name={'Prix marché'} fill={'#10b981'}/></BarChart></ResponsiveContainer> : <NoChartData description={'Aucun produit commercial n’est disponible.'} />}
    </ChartCard>
    <ChartCard title={'Marge par produit'} description={'Marge des dernières décisions validées'}>
      {hasDecisions ? <ResponsiveContainer><BarChart data={productData}>{common}<XAxis dataKey={'name'}/><YAxis/><Tooltip/><Bar dataKey={'margin'} name={'Marge (%)'} fill={'#8b5cf6'} radius={[6,6,0,0]}/></BarChart></ResponsiveContainer> : <NoChartData description={'Validez une étude pour calculer les marges.'} />}
    </ChartCard>
    <ChartCard title={'Répartition des recommandations'} description={'Une décision récente par version de produit'}>
      {hasDecisions ? <ResponsiveContainer><PieChart><Pie data={recommendations} dataKey={'value'} nameKey={'name'} innerRadius={55} outerRadius={90}><Cell fill={'#10b981'}/><Cell fill={'#f59e0b'}/><Cell fill={'#ef4444'}/></Pie><Tooltip/><Legend/></PieChart></ResponsiveContainer> : <NoChartData description={'Aucune recommandation validée n’est disponible.'} />}
    </ChartCard>
    <ChartCard title={'Score global par produit'} description={'Score de la dernière étude validée'}>
      {hasDecisions ? <ResponsiveContainer><BarChart data={productData}>{common}<XAxis dataKey={'name'}/><YAxis domain={[0,100]}/><Tooltip/><Bar dataKey={'score'} name={'Score / 100'} fill={'#2563eb'} radius={[6,6,0,0]}/></BarChart></ResponsiveContainer> : <NoChartData description={'Aucun score validé n’est disponible.'} />}
    </ChartCard>
    <ChartCard title={'Score global moyen par catégorie'} description={'Moyenne des décisions validées'}>
      {categories.length > 0 ? <ResponsiveContainer><BarChart data={categories} layout={'vertical'}><XAxis type={'number'} domain={[0,100]}/><YAxis dataKey={'name'} type={'category'} width={115}/><Tooltip/><Bar dataKey={'potential'} name={'Score / 100'} fill={'#10b981'}/></BarChart></ResponsiveContainer> : <NoChartData description={'Aucun score de catégorie validé n’est disponible.'} />}
    </ChartCard>
    <ChartCard title={'Score marché moyen'} description={'Regroupé par mois d’étude validée'}>
      {trends.length > 0 ? <ResponsiveContainer><LineChart data={trends}>{common}<XAxis dataKey={'month'}/><YAxis domain={[0,100]}/><Tooltip/><Line dataKey={'demand'} name={'Score / 100'} stroke={'#2563eb'} strokeWidth={3}/></LineChart></ResponsiveContainer> : <NoChartData description={'Aucun historique de score marché validé n’est disponible.'} />}
    </ChartCard>
    <ChartCard title={'Volume mensuel prévu'} description={'Somme des prévisions par mois d’étude validée'}>
      {trends.length > 0 ? <ResponsiveContainer><LineChart data={trends}>{common}<XAxis dataKey={'month'}/><YAxis/><Tooltip/><Line dataKey={'forecastSales'} name={'Unités prévues'} stroke={'#8b5cf6'} strokeWidth={3}/></LineChart></ResponsiveContainer> : <NoChartData description={'Aucune prévision de ventes validée n’est disponible.'} />}
    </ChartCard>
    <ChartCard title={'Comparaison concurrentielle'} description={'Concurrents renseignés dans les décisions validées'}>
      {competitors.length > 0 ? <ResponsiveContainer><BarChart data={competitors}>{common}<XAxis dataKey={'name'}/><YAxis/><Tooltip/><Legend/><Bar dataKey={'prix'} fill={'#2563eb'}/><Bar dataKey={'qualité'} fill={'#10b981'}/><Bar dataKey={'part'} name={'Part de marché'} fill={'#f59e0b'}/></BarChart></ResponsiveContainer> : <NoChartData description={'Aucun concurrent n’est renseigné dans les études validées.'} />}
    </ChartCard>
    <ChartCard title={'Radar des critères'} description={'Dernière étude validée'}>
      {decisions[0] ? <CommercialCriteriaRadar scores={decisions[0].scores}/> : <NoChartData description={'Validez une étude pour afficher ses critères.'} />}
    </ChartCard>
    <ChartCard title={'Matrice risque et impact'} description={'Risques des décisions validées'}>
      {risks.length > 0 ? <ResponsiveContainer><ScatterChart><CartesianGrid/><XAxis dataKey={'x'} name={'Probabilité'} type={'number'} domain={[0,5]}/><YAxis dataKey={'y'} name={'Impact'} type={'number'} domain={[0,5]}/><ZAxis dataKey={'z'} range={[80,450]}/><Tooltip/><Scatter data={risks} fill={'#ef4444'}/></ScatterChart></ResponsiveContainer> : <NoChartData description={'Aucun risque n’est renseigné dans les études validées.'} />}
    </ChartCard>
    <ChartCard title={'Chiffre d’affaires mensuel prévu'} description={'Somme des prévisions par mois d’étude validée'}>
      {trends.length > 0 ? <ResponsiveContainer><LineChart data={trends}>{common}<XAxis dataKey={'month'}/><YAxis/><Tooltip/><Line dataKey={'revenue'} name={'Chiffre d’affaires'} stroke={'#10b981'} strokeWidth={3}/></LineChart></ResponsiveContainer> : <NoChartData description={'Aucune prévision de chiffre d’affaires validée n’est disponible.'} />}
    </ChartCard>
  </section>
}
