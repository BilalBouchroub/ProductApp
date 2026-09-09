import type { MarketStudyInput, PricingAnalysis, ProductionCommercialSnapshot, SalesForecast } from '../types/commercial.types'
import type { ProductionExperiment, ProductionProcess } from '../../production/types/production.types'
import { calculateProcessSummary } from '../../production/utils/productionCalculations'

export function calculatePricing(input: Pick<PricingAnalysis, 'productionCost'|'proposedSalePrice'|'estimatedMarketingCost'|'estimatedDistributionCost'|'estimatedCommission'>): Pick<PricingAnalysis, 'calculatedMargin'|'marginRate'|'totalEstimatedCost'|'breakEvenUnits'> {
  const totalEstimatedCost = input.productionCost + input.estimatedMarketingCost + input.estimatedDistributionCost + input.estimatedCommission
  const calculatedMargin = input.proposedSalePrice - totalEstimatedCost
  const marginRate = input.proposedSalePrice > 0 ? calculatedMargin / input.proposedSalePrice * 100 : 0
  const breakEvenUnits = calculatedMargin > 0 ? Math.ceil((input.estimatedMarketingCost + input.estimatedDistributionCost) / calculatedMargin) : 0
  return { calculatedMargin, marginRate, totalEstimatedCost, breakEvenUnits }
}
export function calculateForecast(price: number, monthlyVolume: number, annualVolume: number): Pick<SalesForecast, 'monthlyRevenue'|'annualRevenue'> { return { monthlyRevenue: price * monthlyVolume, annualRevenue: price * annualVolume } }
export function createProductionSnapshot(process: ProductionProcess, experiments: ProductionExperiment[]): ProductionCommercialSnapshot {
  const summary = calculateProcessSummary(process)
  const resources = process.steps.flatMap((step) => step.resources)
  const available = resources.filter((resource) => resource.availabilityStatus === 'Available').length
  const completed = experiments.filter((experiment) => experiment.endDate)
  const latest = completed.slice().sort((a,b) => b.startDate.localeCompare(a.startDate))[0]
  const successes = completed.filter((experiment) => experiment.result === 'Success').length
  return { productionCost: summary.plannedCost, totalDurationMinutes: summary.plannedDurationMinutes, stepCount: process.steps.length, resourceAvailabilityRate: resources.length ? available / resources.length * 100 : 0, wasteRate: latest?.wasteRate ?? 0, latestExperimentResult: latest?.result ?? null, experimentCount: experiments.length, processStability: completed.length ? successes / completed.length * 100 : 35, productionCapacity: process.steps.at(-1)?.plannedOutputQuantity ?? 0 }
}
export function enrichStudyCalculations(study: MarketStudyInput): MarketStudyInput { const pricing = { ...study.pricing, ...calculatePricing(study.pricing) }; const forecast = { ...study.forecast, ...calculateForecast(pricing.proposedSalePrice, study.forecast.monthlySalesVolume, study.forecast.annualSalesVolume) }; return { ...study, pricing, forecast } }
