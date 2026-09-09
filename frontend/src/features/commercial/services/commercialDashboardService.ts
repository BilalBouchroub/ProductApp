import { commercialProductsApi } from '../../../api/commercialProductsApi'
import { marketStudyApi } from '../../../api/marketStudyApi'
import type {
  CommercialDashboardData,
  CommercialProductView,
  MarketStudy,
  MarketTrendPoint,
} from '../types/commercial.types'

const average = (values: number[]): number =>
  values.length > 0 ? values.reduce((total, value) => total + value, 0) / values.length : 0

const studyKey = (study: Pick<MarketStudy, 'productId' | 'productVersion'>): string =>
  `${study.productId}:${study.productVersion}`

const productKey = (view: CommercialProductView): string =>
  `${view.product.id}:${view.product.version}`

function latestValidatedStudies(studies: MarketStudy[]): MarketStudy[] {
  const latestByVersion = new Map<string, MarketStudy>()
  const validated = studies
    .filter(study => study.status === 'Validated' && study.feasibility !== null)
    .sort((left, right) => right.updatedAt.localeCompare(left.updatedAt))

  for (const study of validated) {
    const key = studyKey(study)
    if (!latestByVersion.has(key)) latestByVersion.set(key, study)
  }
  return [...latestByVersion.values()]
}

function buildMarketTrends(studies: MarketStudy[]): MarketTrendPoint[] {
  const monthly = new Map<string, { demand: number[]; forecastSales: number; revenue: number }>()

  for (const study of studies) {
    const date = new Date(study.studyDate)
    if (Number.isNaN(date.getTime())) continue
    const key = `${date.getUTCFullYear()}-${String(date.getUTCMonth() + 1).padStart(2, '0')}`
    const current = monthly.get(key) ?? { demand: [], forecastSales: 0, revenue: 0 }
    current.demand.push(study.feasibility?.marketScore ?? 0)
    current.forecastSales += study.forecast.monthlySalesVolume
    current.revenue += study.forecast.monthlyRevenue
    monthly.set(key, current)
  }

  return [...monthly.entries()]
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([key, values]) => {
      const [year, month] = key.split('-').map(Number)
      return {
        month: new Intl.DateTimeFormat('fr-MA', { month: 'short', year: '2-digit', timeZone: 'UTC' })
          .format(new Date(Date.UTC(year!, month! - 1, 1))),
        demand: average(values.demand),
        forecastSales: values.forecastSales,
        revenue: values.revenue,
      }
    })
}

export function buildCommercialDashboard(
  products: CommercialProductView[],
  studies: MarketStudy[],
  updatedAt = new Date().toISOString(),
): CommercialDashboardData {
  const orderedStudies = [...studies].sort((left, right) => right.updatedAt.localeCompare(left.updatedAt))
  const decisions = latestValidatedStudies(orderedStudies)
  const studiedVersions = new Set(orderedStudies.map(studyKey))
  const viable = decisions.filter(study => study.feasibility?.recommendation === 'Viable')
  const toOptimize = decisions.filter(study => study.feasibility?.recommendation === 'ToOptimize')
  const notViable = decisions.filter(study => study.feasibility?.recommendation === 'NotViable')

  return {
    products,
    studies: orderedStudies,
    trends: buildMarketTrends(decisions),
    updatedAt,
    metrics: {
      waiting: products.filter(product => !studiedVersions.has(productKey(product))).length,
      inProgress: orderedStudies.filter(study => study.status === 'Draft' || study.status === 'InProgress').length,
      studied: decisions.length,
      viable: viable.length,
      toOptimize: toOptimize.length,
      notViable: notViable.length,
      averageScore: average(decisions.map(study => study.feasibility!.globalScore)),
      averageMargin: average(decisions.map(study => study.pricing.marginRate)),
      best: [...decisions].sort((left, right) => right.feasibility!.globalScore - left.feasibility!.globalScore)[0] ?? null,
      riskiest: [...decisions].sort((left, right) => left.feasibility!.riskScore - right.feasibility!.riskScore)[0] ?? null,
    },
  }
}

export const commercialDashboardService = {
  async getDashboard(): Promise<CommercialDashboardData> {
    const studiesRequest = marketStudyApi.getAll()
    const [products, studies] = await Promise.all([
      commercialProductsApi.getAll(studiesRequest),
      studiesRequest,
    ])
    return buildCommercialDashboard(products, studies)
  },
}
