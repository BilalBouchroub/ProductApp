import type { CommercialProductView, MarketStudy } from '../features/commercial/types/commercial.types'
import type { Product, ProductStatus, ProductionExperiment, ProductionProcess } from '../features/production/types/production.types'
import { calculateProcessSummary } from '../features/production/utils/productionCalculations'
import { experimentsApi } from './experimentsApi'
import { marketStudyApi } from './marketStudyApi'
import { productionStepsApi } from './productionStepsApi'
import { productsApi } from './productsApi'

const allowed = new Set<ProductStatus>(['ReadyForMarketStudy', 'UnderMarketStudy', 'Approved', 'ToOptimize', 'Rejected'])

export function buildCommercialProductView(
  product: Product,
  process: ProductionProcess,
  experiments: ProductionExperiment[],
  studies: MarketStudy[],
): CommercialProductView {
  const versionExperiments = experiments.filter(experiment =>
    experiment.productId === product.id && experiment.productVersion === product.version)
  const latest = versionExperiments
    .filter(experiment => Boolean(experiment.endDate))
    .sort((left, right) => right.startDate.localeCompare(left.startDate))[0]
  const summary = calculateProcessSummary(process)

  return {
    product,
    process,
    experiments: versionExperiments,
    resources: process.steps.flatMap(step => step.resources),
    productionCost: summary.plannedCost,
    totalDurationMinutes: summary.plannedDurationMinutes,
    stepCount: process.steps.length,
    latestExperimentResult: latest?.result ?? null,
    wasteRate: latest?.wasteRate ?? 0,
    study: studies.find(study => study.productId === product.id && study.productVersion === product.version) ?? null,
  }
}

export const commercialProductsApi = {
  async getAll(studiesRequest: Promise<MarketStudy[]> = marketStudyApi.getAll()): Promise<CommercialProductView[]> {
    const products = (await productsApi.getAll()).filter(product => allowed.has(product.status))
    const [experiments, studies, processes] = await Promise.all([
      experimentsApi.getAll(),
      studiesRequest,
      Promise.all(products.map(product => productionStepsApi.getProcess(product.id))),
    ])
    return products.map((product, index) => buildCommercialProductView(product, processes[index]!, experiments, studies))
  },

  async get(id: string): Promise<CommercialProductView> {
    const product = await productsApi.get(id)
    if (!allowed.has(product.status)) throw new Error('Ce produit n’est pas disponible pour une étude commerciale.')
    const [process, experiments, studies] = await Promise.all([
      productionStepsApi.getProcess(product.id),
      experimentsApi.getAll(),
      marketStudyApi.getAll(),
    ])
    return buildCommercialProductView(product, process, experiments, studies)
  },
}
