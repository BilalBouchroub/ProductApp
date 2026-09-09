import type { FeasibilityInput, FeasibilityResult } from '../types/commercial.types'

const clamp = (value: number): number => Math.round(Math.max(0, Math.min(100, value)))
const demandPoints = { VeryLow: 20, Low: 38, Medium: 58, High: 78, VeryHigh: 94 } as const
const trendPoints = { Declining: 20, Stable: 52, Growing: 76, RapidlyGrowing: 94 } as const

/** Règles déterministes: Production 25 %, marché 30 %, finance 30 %, maîtrise des risques 15 %. */
export function calculateMarketFeasibility({ production, study }: FeasibilityInput): FeasibilityResult {
  const experiment = production.latestExperimentResult === 'Success' ? 95 : production.latestExperimentResult === 'PartialSuccess' ? 65 : production.latestExperimentResult === 'Failed' ? 25 : 40
  const complexity = clamp(100 - Math.max(0, production.stepCount - 5) * 6 - production.totalDurationMinutes / 18)
  const waste = clamp(100 - production.wasteRate * 5)
  const productionScore = clamp(production.resourceAvailabilityRate * .25 + production.processStability * .25 + experiment * .2 + complexity * .15 + waste * .15)
  const competition = study.competitors.length ? clamp(100 - study.competitors.reduce((sum, item) => sum + item.marketShare, 0) / study.competitors.length) : 75
  const marketScore = clamp(demandPoints[study.demandLevel] * .25 + trendPoints[study.marketTrend] * .2 + study.scores.demandPotentialScore * .2 + study.scores.marketFitScore * .2 + competition * .15)
  const margin = clamp(study.pricing.marginRate * 2.4)
  const priceFit = study.pricing.averageMarketPrice > 0 ? clamp(100 - Math.abs(study.pricing.proposedSalePrice - study.pricing.averageMarketPrice) / study.pricing.averageMarketPrice * 180) : 50
  const financialScore = clamp(margin * .4 + priceFit * .25 + study.scores.profitabilityScore * .2 + clamp(study.forecast.expectedGrowthRate * 5) * .15)
  const exposure = study.risks.length ? study.risks.reduce((sum, risk) => sum + risk.probability * risk.impact, 0) / study.risks.length : 7
  const riskScore = clamp(100 - exposure * 4 + study.scores.commercialRiskScore * .2)
  const globalScore = clamp(productionScore * .25 + marketScore * .3 + financialScore * .3 + riskScore * .15)
  const critical = productionScore < 35 || marketScore < 35 || financialScore < 30 || riskScore < 25
  const recommendation = globalScore >= 70 && !critical ? 'Viable' : globalScore >= 48 && marketScore >= 45 ? 'ToOptimize' : 'NotViable'
  const strengths = [marketScore >= 70 ? 'Potentiel de demande solide' : '', financialScore >= 70 ? 'Rentabilité estimée attractive' : '', productionScore >= 70 ? 'Processus industriel maîtrisé' : ''].filter(Boolean)
  const weaknesses = [productionScore < 60 ? 'Performance de production à consolider' : '', financialScore < 60 ? 'Marge ou positionnement prix insuffisant' : '', marketScore < 60 ? 'Adéquation marché à renforcer' : ''].filter(Boolean)
  const mainRisks = study.risks.slice().sort((a,b) => b.probability*b.impact-a.probability*a.impact).slice(0,3).map((risk) => risk.description)
  const improvements = [...weaknesses, ...(production.wasteRate > 7 ? ['Réduire le taux de perte industriel'] : []), ...(riskScore < 60 ? ['Mettre en œuvre les actions de mitigation prioritaires'] : [])]
  const advisedPrice = Math.max(study.pricing.minimumAcceptablePrice, Math.min(study.pricing.maximumAcceptablePrice, study.pricing.averageMarketPrice * .98))
  const minimumVolume = study.pricing.calculatedMargin > 0 ? Math.ceil((study.pricing.estimatedMarketingCost + study.pricing.estimatedDistributionCost) / study.pricing.calculatedMargin) : 0
  return { productionScore, marketScore, financialScore, riskScore, globalScore, recommendation, strengths, weaknesses, mainRisks, improvements, advisedPrice, minimumVolume }
}
