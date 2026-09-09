import type { ExperimentResult, Product, ProductionExperiment, ProductionProcess, StepResource } from '../../production/types/production.types'

export const DEMAND_LEVELS = ['VeryLow', 'Low', 'Medium', 'High', 'VeryHigh'] as const
export type DemandLevel = (typeof DEMAND_LEVELS)[number]
export const MARKET_TRENDS = ['Declining', 'Stable', 'Growing', 'RapidlyGrowing'] as const
export type MarketTrend = (typeof MARKET_TRENDS)[number]
export const STUDY_STATUSES = ['Draft', 'InProgress', 'Completed', 'Validated'] as const
export type MarketStudyStatus = (typeof STUDY_STATUSES)[number]
export const RECOMMENDATIONS = ['Viable', 'ToOptimize', 'NotViable'] as const
export type CommercialRecommendation = (typeof RECOMMENDATIONS)[number]
export const RISK_TYPES = ['Competitive', 'Financial', 'LowDemand', 'Pricing', 'Supply', 'Production', 'Regulatory'] as const
export type RiskType = (typeof RISK_TYPES)[number]
export const COMMERCIAL_PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const
export type CommercialPriority = (typeof COMMERCIAL_PRIORITIES)[number]

export interface Competitor { id: string; competitorName: string; productName: string; price: number; quantity: number; estimatedQuality: number; marketShare: number; strengths: string; weaknesses: string; salesChannels: string; customerRating: number; imageUrl: string | null }
export interface CommercialRisk { id: string; riskType: RiskType; probability: number; impact: number; description: string; mitigationAction: string }
export interface CommercialScores { demandPotentialScore: number; pricePositioningScore: number; competitiveAdvantageScore: number; distributionScore: number; productAttractivenessScore: number; profitabilityScore: number; commercialRiskScore: number; marketFitScore: number }
export interface PricingAnalysis { productionCost: number; proposedSalePrice: number; averageMarketPrice: number; minimumAcceptablePrice: number; maximumAcceptablePrice: number; desiredMargin: number; calculatedMargin: number; marginRate: number; estimatedMarketingCost: number; estimatedDistributionCost: number; estimatedCommission: number; totalEstimatedCost: number; breakEvenUnits: number }
export interface SalesForecast { monthlySalesVolume: number; annualSalesVolume: number; monthlyRevenue: number; annualRevenue: number; breakEvenPeriodMonths: number; expectedGrowthRate: number }

export interface MarketStudyInput {
  studyName: string; productId: string; productVersion: number; targetMarket: string; geographicArea: string; customerSegment: string; studyDate: string; managerName: string
  estimatedMarketSize: number; annualGrowthRate: number; demandLevel: DemandLevel; marketTrend: MarketTrend; seasonality: string; distributionChannels: string; commercializationArea: string
  customerType: string; ageRange: string; purchasingPower: string; consumptionHabits: string; purchaseFrequency: string; preferences: string; priceSensitivity: number
  competitors: Competitor[]; pricing: PricingAnalysis; forecast: SalesForecast; risks: CommercialRisk[]; scores: CommercialScores
}
export interface MarketStudy extends MarketStudyInput { id: string; status: MarketStudyStatus; createdAt: string; updatedAt: string; feasibility: FeasibilityResult | null }
export interface ProductionCommercialSnapshot { productionCost: number; totalDurationMinutes: number; stepCount: number; resourceAvailabilityRate: number; wasteRate: number; latestExperimentResult: ExperimentResult | null; experimentCount: number; processStability: number; productionCapacity: number }
export interface FeasibilityInput { production: ProductionCommercialSnapshot; study: MarketStudyInput }
export interface FeasibilityResult { productionScore: number; marketScore: number; financialScore: number; riskScore: number; globalScore: number; recommendation: CommercialRecommendation; strengths: string[]; weaknesses: string[]; mainRisks: string[]; improvements: string[]; advisedPrice: number; minimumVolume: number }
export interface CommercialProductView { product: Product; process: ProductionProcess; experiments: ProductionExperiment[]; resources: StepResource[]; productionCost: number; totalDurationMinutes: number; stepCount: number; latestExperimentResult: ExperimentResult | null; wasteRate: number; study: MarketStudy | null }
export interface CommercialOptimizationRequest { id: string; productId: string; productVersion: number; message: string; priority: CommercialPriority; requestedChanges: string[]; commercialManagerName: string; createdAt: string; status: 'New' | 'InProgress' | 'Resolved' }
export interface MarketTrendPoint { month: string; demand: number; forecastSales: number; revenue: number }
export interface CommercialDashboardMetrics {
  waiting: number
  inProgress: number
  studied: number
  viable: number
  toOptimize: number
  notViable: number
  averageScore: number
  averageMargin: number
  best: MarketStudy | null
  riskiest: MarketStudy | null
}
export interface CommercialDashboardData {
  products: CommercialProductView[]
  studies: MarketStudy[]
  trends: MarketTrendPoint[]
  metrics: CommercialDashboardMetrics
  updatedAt: string
}
