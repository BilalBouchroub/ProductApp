import { describe, expect, it } from 'vitest'
import type { CommercialProductView, MarketStudy } from '../types/commercial.types'
import { buildCommercialDashboard } from './commercialDashboardService'

const product = (id: string, version: number): CommercialProductView => ({
  product: { id, name: id, internalReference: id, sapCode: id, category: 'Biscuits', description: '', imageUrl: null,
    themeColor: '#2563eb', targetSalePrice: 20, batchQuantity: 100, productionUnit: 'kg', status: 'ReadyForMarketStudy', version,
    createdAt: '2026-01-01T00:00:00Z', updatedAt: '2026-01-01T00:00:00Z', productionManagerName: 'Production' },
  process: { id: 'process-' + id, productId: id, productVersion: version, version: 1, updatedAt: '2026-01-01T00:00:00Z', savedAt: '2026-01-01T00:00:00Z', steps: [] },
  experiments: [], resources: [], productionCost: 10, totalDurationMinutes: 0, stepCount: 0, latestExperimentResult: null, wasteRate: 0, study: null,
})

const study = (id: string, productId: string, status: MarketStudy['status'], updatedAt: string,
  recommendation: MarketStudy['feasibility'] extends infer _ ? 'Viable' | 'ToOptimize' | 'NotViable' : never = 'Viable'): MarketStudy => ({
  id, studyName: id, productId, productVersion: 1, targetMarket: 'Maroc', geographicArea: 'Maroc', customerSegment: 'B2C',
  studyDate: '2026-08-01', managerName: 'Commercial', estimatedMarketSize: 1000, annualGrowthRate: 5, demandLevel: 'High', marketTrend: 'Growing',
  seasonality: '', distributionChannels: '', commercializationArea: 'Maroc', customerType: 'B2C', ageRange: '', purchasingPower: '', consumptionHabits: '', purchaseFrequency: '', preferences: '', priceSensitivity: 50,
  competitors: [], risks: [], pricing: { productionCost: 10, proposedSalePrice: 20, averageMarketPrice: 21, minimumAcceptablePrice: 12, maximumAcceptablePrice: 25, desiredMargin: 30, calculatedMargin: 8, marginRate: 40, estimatedMarketingCost: 1, estimatedDistributionCost: 1, estimatedCommission: 0, totalEstimatedCost: 12, breakEvenUnits: 10 },
  forecast: { monthlySalesVolume: 100, annualSalesVolume: 1200, monthlyRevenue: 2000, annualRevenue: 24000, breakEvenPeriodMonths: 1, expectedGrowthRate: 5 },
  scores: { demandPotentialScore: 80, pricePositioningScore: 70, competitiveAdvantageScore: 70, distributionScore: 70, productAttractivenessScore: 70, profitabilityScore: 70, commercialRiskScore: 70, marketFitScore: 75 },
  status, createdAt: '2026-08-01T00:00:00Z', updatedAt,
  feasibility: status === 'Validated' ? { productionScore: 80, marketScore: 80, financialScore: 80, riskScore: recommendation === 'NotViable' ? 20 : 70,
    globalScore: recommendation === 'NotViable' ? 30 : 80, recommendation, strengths: [], weaknesses: [], mainRisks: [], improvements: [], advisedPrice: 20, minimumVolume: 10 } : null,
})

describe('buildCommercialDashboard', () => {
  it('calcule les indicateurs à partir des études réelles et déduplique les décisions par version', () => {
    const data = buildCommercialDashboard([product('p1', 1), product('p2', 1), product('p3', 1)], [
      study('draft', 'p1', 'Draft', '2026-08-02T00:00:00Z'),
      study('old', 'p2', 'Validated', '2026-08-02T00:00:00Z', 'NotViable'),
      study('latest', 'p2', 'Validated', '2026-08-03T00:00:00Z', 'Viable'),
    ], '2026-08-12T10:00:00Z')

    expect(data.metrics).toMatchObject({ waiting: 1, inProgress: 1, studied: 1, viable: 1, toOptimize: 0, notViable: 0, averageScore: 80, averageMargin: 40 })
    expect(data.metrics.best?.id).toBe('latest')
    expect(data.trends).toHaveLength(1)
    expect(data.updatedAt).toBe('2026-08-12T10:00:00Z')
  })

  it('retourne des moyennes sûres lorsque la plateforme ne contient aucune décision validée', () => {
    const data = buildCommercialDashboard([], [])
    expect(data.metrics.averageScore).toBe(0)
    expect(data.metrics.averageMargin).toBe(0)
    expect(data.trends).toEqual([])
  })
})
