import { experimentsApi } from '../../../api/experimentsApi'
import { optimizationRequestsApi } from '../../../api/optimizationRequestsApi'
import { productionStepsApi } from '../../../api/productionStepsApi'
import { productsApi } from '../../../api/productsApi'
import type {
  OptimizationRequest,
  ProductionDashboardData,
  ProductionExperiment,
  ProductionProcess,
  Product,
  ProductStatus,
} from '../types/production.types'
import { calculateProcessSummary } from '../utils/productionCalculations'

const productStatusLabels: Record<ProductStatus, string> = {
  Draft: 'Brouillon',
  InConfiguration: 'Configuration',
  InExperiment: 'Expérimentation',
  ReadyForMarketStudy: 'Prêt pour étude',
  UnderMarketStudy: 'Étude marché',
  Approved: 'Approuvé',
  ToOptimize: 'À optimiser',
  Rejected: 'Rejeté',
  Archived: 'Archivé',
}

const experimentResultLabels: Record<ProductionExperiment['result'], string> = {
  Success: 'Réussie',
  PartialSuccess: 'Partielle',
  Failed: 'Échouée',
  Cancelled: 'En attente',
}

const shortName = (value: string, length = 18) => value.length > length ? `${value.slice(0, length - 1)}…` : value
const average = (values: number[]) => values.length > 0 ? values.reduce((total, value) => total + value, 0) / values.length : 0

export function buildProductionDashboard(
  products: Product[],
  experiments: ProductionExperiment[],
  processes: ProductionProcess[],
  optimizationRequests: OptimizationRequest[],
  warnings: string[] = [],
): ProductionDashboardData {
  const summaries = processes.map(process => ({ process, summary: calculateProcessSummary(process) }))
  const completedExperiments = experiments.filter(experiment => Boolean(experiment.endDate))
  const resources = processes.flatMap(process => process.steps.flatMap(step => step.resources))
  const productById = new Map(products.map(product => [product.id, product]))

  const statusCounts = new Map<ProductStatus, number>()
  products.forEach(product => statusCounts.set(product.status, (statusCounts.get(product.status) ?? 0) + 1))

  const resourceTotals = new Map<string, { planned: number; actual: number }>()
  resources.forEach(resource => {
    const current = resourceTotals.get(resource.designation) ?? { planned: 0, actual: 0 }
    resourceTotals.set(resource.designation, {
      planned: current.planned + resource.plannedQuantity,
      actual: current.actual + resource.actualQuantity,
    })
  })

  const stepCosts = new Map<string, number>()
  processes.flatMap(process => process.steps).forEach(step => {
    stepCosts.set(step.name, (stepCosts.get(step.name) ?? 0) + step.plannedCost)
  })

  return {
    metrics: {
      totalProducts: products.length,
      drafts: products.filter(product => product.status === 'Draft').length,
      configuring: products.filter(product => product.status === 'InConfiguration').length,
      experimenting: products.filter(product => product.status === 'InExperiment').length,
      ready: products.filter(product => product.status === 'ReadyForMarketStudy').length,
      totalExperiments: experiments.length,
      successes: experiments.filter(experiment => experiment.result === 'Success').length,
      partialSuccesses: experiments.filter(experiment => experiment.result === 'PartialSuccess').length,
      failures: experiments.filter(experiment => experiment.result === 'Failed').length,
      averageCost: average(summaries.map(item => item.summary.plannedCost)),
      averageDuration: average(summaries.map(item => item.summary.plannedDurationMinutes)),
      averageWasteRate: average(completedExperiments.map(experiment => experiment.wasteRate)),
      lowStockResources: resources.filter(resource => resource.availabilityStatus === 'LowStock' || resource.availabilityStatus === 'Unavailable').length,
    },
    costByProduct: summaries
      .map(({ process, summary }) => ({ name: shortName(productById.get(process.productId)?.name ?? process.productId), cost: summary.plannedCost }))
      .sort((left, right) => right.cost - left.cost)
      .slice(0, 8),
    durationByProduct: summaries
      .map(({ process, summary }) => ({ name: shortName(productById.get(process.productId)?.name ?? process.productId), duration: summary.plannedDurationMinutes }))
      .sort((left, right) => right.duration - left.duration)
      .slice(0, 8),
    productsByStatus: Array.from(statusCounts.entries())
      .map(([status, value]) => ({ name: productStatusLabels[status], value }))
      .filter(item => item.value > 0),
    experimentResults: (Object.keys(experimentResultLabels) as ProductionExperiment['result'][])
      .map(result => ({ name: experimentResultLabels[result], value: experiments.filter(experiment => experiment.result === result).length })),
    costComparison: experiments.slice(0, 8).map(experiment => ({
      name: shortName(experiment.productName || experiment.name, 15),
      planned: experiment.plannedCost,
      actual: experiment.actualCost,
    })),
    durationComparison: experiments.slice(0, 8).map(experiment => ({
      name: shortName(experiment.productName || experiment.name, 15),
      planned: experiment.plannedDuration,
      actual: experiment.actualDuration,
    })),
    resourceConsumption: Array.from(resourceTotals.entries())
      .map(([name, values]) => ({ name: shortName(name, 16), ...values }))
      .sort((left, right) => right.planned - left.planned)
      .slice(0, 8),
    costByStep: Array.from(stepCosts.entries())
      .map(([name, cost]) => ({ name: shortName(name, 18), cost }))
      .sort((left, right) => right.cost - left.cost)
      .slice(0, 8),
    recentProducts: [...products].sort((left, right) => right.updatedAt.localeCompare(left.updatedAt)).slice(0, 6),
    recentExperiments: [...experiments].sort((left, right) => right.startDate.localeCompare(left.startDate)).slice(0, 6),
    optimizationRequests,
    warnings,
    updatedAt: new Date().toISOString(),
  }
}

export const productionDashboardService = {
  async getDashboard(): Promise<ProductionDashboardData> {
    const products = await productsApi.getAll()
    const activeProducts = products.filter(product => product.status !== 'Archived')
    const [experimentsResult, requestsResult, processResults] = await Promise.all([
      experimentsApi.getAll().then(value => ({ ok: true as const, value })).catch(() => ({ ok: false as const, value: [] as ProductionExperiment[] })),
      optimizationRequestsApi.getAll().then(value => ({ ok: true as const, value })).catch(() => ({ ok: false as const, value: [] as OptimizationRequest[] })),
      Promise.allSettled(activeProducts.map(product => productionStepsApi.getProcess(product.id))),
    ])

    const warnings: string[] = []
    if (!experimentsResult.ok) warnings.push('Les expériences ne sont momentanément pas disponibles.')
    if (!requestsResult.ok) warnings.push('Les demandes d’optimisation ne sont momentanément pas disponibles.')
    const failedProcesses = processResults.filter(result => result.status === 'rejected').length
    if (failedProcesses > 0) warnings.push(`${failedProcesses} chaîne${failedProcesses > 1 ? 's' : ''} de production n’a pas pu être chargée.`)

    const processes = processResults
      .filter((result): result is PromiseFulfilledResult<ProductionProcess> => result.status === 'fulfilled')
      .map(result => result.value)

    return buildProductionDashboard(products, experimentsResult.value, processes, requestsResult.value, warnings)
  },
}
