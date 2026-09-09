import { axiosClient } from './axiosClient'
import type { PagedResponse } from './api.types'
import { productsApi } from './productsApi'
import type { CommercialOptimizationRequest, CommercialRecommendation, FeasibilityResult, MarketStudy, MarketStudyInput } from '../features/commercial/types/commercial.types'

interface CompetitorResponse { id: string; name: string; productName: string; price: number; quantity: number; estimatedQualityScore: number | null; marketShare: number | null; strengths: string | null; weaknesses: string | null; salesChannels: string | null; customerRating: number | null }
interface RiskResponse { id: string; riskType: string; probability: number; impact: number; description: string; mitigationAction: string | null }
interface StudyResponse { id: string; productVersionId: string; name: string; targetMarket: string; geographicArea: string; customerSegment: string; studyDate: string; status: MarketStudy['status']; estimatedMarketSize: number; annualGrowthRate: number; productionCost: number; proposedSalePrice: number; averageMarketPrice: number; calculatedMargin: number; marginRate: number; monthlySalesVolume: number; annualRevenue: number; productionScore: number; marketScore: number; financialScore: number; riskScore: number; globalScore: number; recommendation: CommercialRecommendation | null; competitors: CompetitorResponse[]; risks: RiskResponse[]; createdAt: string; updatedAt: string }
interface ResultResponse { recommendation: CommercialRecommendation; productionScore: number; marketScore: number; financialScore: number; riskScore: number; globalScore: number; strengths: string[]; weaknesses: string[]; mainRisks: string[]; improvements: string[]; advisedPrice: number; minimumVolume: number }

async function versionIdentity(versionId: string): Promise<{ productId: string; version: number }> {
  const products = await productsApi.getAll()
  for (const product of products) { const version = (await productsApi.getVersions(product.id)).find(item => item.id === versionId); if (version) return { productId: product.id, version: version.versionNumber } }
  return { productId: '', version: 1 }
}
async function versionIdentities(): Promise<Map<string, { productId: string; version: number }>> {
  const products = await productsApi.getAll()
  const versions = await Promise.all(products.map(async product => ({ product, versions: await productsApi.getVersions(product.id) })))
  return new Map(versions.flatMap(({ product, versions: productVersions }) => productVersions.map(version =>
    [version.id, { productId: product.id, version: version.versionNumber }] as const)))
}
const feasibility = (study: StudyResponse, result?: ResultResponse): FeasibilityResult | null => {
  const recommendation = result?.recommendation ?? study.recommendation
  if (!recommendation) return null
  return { recommendation, productionScore: result?.productionScore ?? study.productionScore, marketScore: result?.marketScore ?? study.marketScore,
    financialScore: result?.financialScore ?? study.financialScore, riskScore: result?.riskScore ?? study.riskScore,
    globalScore: result?.globalScore ?? study.globalScore, strengths: result?.strengths ?? [], weaknesses: result?.weaknesses ?? [],
    mainRisks: result?.mainRisks ?? [], improvements: result?.improvements ?? [], advisedPrice: result?.advisedPrice ?? study.proposedSalePrice,
    minimumVolume: result?.minimumVolume ?? Math.ceil(study.productionCost / Math.max(study.calculatedMargin, 1)) }
}
async function mapStudy(study: StudyResponse, knownIdentity?: { productId: string; version: number }): Promise<MarketStudy> {
  const identity = knownIdentity ?? await versionIdentity(study.productVersionId)
  let result: ResultResponse | undefined
  if (study.status === 'Validated') { const response = await axiosClient.get<ResultResponse>(`/market-studies/${study.id}/result`); result = response.data }
  return { id: study.id, studyName: study.name, productId: identity.productId, productVersion: identity.version, targetMarket: study.targetMarket,
    geographicArea: study.geographicArea, customerSegment: study.customerSegment, studyDate: study.studyDate, managerName: 'ProductApp API',
    estimatedMarketSize: study.estimatedMarketSize, annualGrowthRate: study.annualGrowthRate, demandLevel: 'Medium', marketTrend: study.annualGrowthRate > 5 ? 'Growing' : 'Stable',
    seasonality: '', distributionChannels: '', commercializationArea: study.geographicArea, customerType: '', ageRange: '', purchasingPower: '',
    consumptionHabits: '', purchaseFrequency: '', preferences: '', priceSensitivity: 50,
    competitors: study.competitors.map(item => ({ id: item.id, competitorName: item.name, productName: item.productName, price: item.price,
      quantity: item.quantity, estimatedQuality: item.estimatedQualityScore ?? 0, marketShare: item.marketShare ?? 0, strengths: item.strengths ?? '',
      weaknesses: item.weaknesses ?? '', salesChannels: item.salesChannels ?? '', customerRating: item.customerRating ?? 0, imageUrl: null })),
    pricing: { productionCost: study.productionCost, proposedSalePrice: study.proposedSalePrice, averageMarketPrice: study.averageMarketPrice,
      minimumAcceptablePrice: study.productionCost, maximumAcceptablePrice: study.averageMarketPrice * 1.2, desiredMargin: 0,
      calculatedMargin: study.calculatedMargin, marginRate: study.marginRate, estimatedMarketingCost: 0, estimatedDistributionCost: 0,
      estimatedCommission: 0, totalEstimatedCost: study.productionCost, breakEvenUnits: Math.ceil(study.productionCost / Math.max(study.calculatedMargin, 1)) },
    forecast: { monthlySalesVolume: study.monthlySalesVolume, annualSalesVolume: study.monthlySalesVolume * 12,
      monthlyRevenue: study.monthlySalesVolume * study.proposedSalePrice, annualRevenue: study.annualRevenue, breakEvenPeriodMonths: 0, expectedGrowthRate: study.annualGrowthRate },
    risks: study.risks.map(item => ({ id: item.id, riskType: item.riskType as MarketStudy['risks'][number]['riskType'], probability: item.probability,
      impact: item.impact, description: item.description, mitigationAction: item.mitigationAction ?? '' })),
    scores: { demandPotentialScore: study.marketScore, pricePositioningScore: study.financialScore, competitiveAdvantageScore: study.marketScore,
      distributionScore: study.marketScore, productAttractivenessScore: study.globalScore, profitabilityScore: study.financialScore,
      commercialRiskScore: study.riskScore, marketFitScore: study.globalScore }, status: study.status, createdAt: study.createdAt, updatedAt: study.updatedAt,
    feasibility: feasibility(study, result) }
}
const values = (input: MarketStudyInput) => ({ name: input.studyName, targetMarket: input.targetMarket, geographicArea: input.geographicArea,
  customerSegment: input.customerSegment, studyDate: input.studyDate, estimatedMarketSize: input.estimatedMarketSize,
  annualGrowthRate: input.annualGrowthRate, productionCost: input.pricing.productionCost, proposedSalePrice: input.pricing.proposedSalePrice,
  averageMarketPrice: input.pricing.averageMarketPrice, monthlySalesVolume: input.forecast.monthlySalesVolume })

export const marketStudyApi = {
  async getAll(): Promise<MarketStudy[]> {
    const [{ data: firstPage }, identities] = await Promise.all([
      axiosClient.get<PagedResponse<StudyResponse>>('/market-studies', { params: { pageSize: 100 } }),
      versionIdentities(),
    ])
    const remainingPages = firstPage.totalPages > 1
      ? await Promise.all(Array.from({ length: firstPage.totalPages - 1 }, (_, index) =>
          axiosClient.get<PagedResponse<StudyResponse>>('/market-studies', { params: { page: index + 2, pageSize: 100 } })))
      : []
    const studies = [firstPage, ...remainingPages.map(response => response.data)].flatMap(page => page.items)
    return Promise.all(studies.map(study => mapStudy(study, identities.get(study.productVersionId))))
  },
  async get(id: string): Promise<MarketStudy> { const { data } = await axiosClient.get<StudyResponse>(`/market-studies/${id}`); return mapStudy(data) },
  async save(input: MarketStudyInput, status: MarketStudy['status'] = 'Draft', id?: string): Promise<MarketStudy> {
    let studyId = id
    if (studyId) await axiosClient.put(`/market-studies/${studyId}`, values(input))
    else {
      const version = (await productsApi.getVersions(input.productId)).find(item => item.versionNumber === input.productVersion)
      if (!version) throw new Error('La version du produit est introuvable dans l’API.')
      const { data } = await axiosClient.post<StudyResponse>('/market-studies', { productVersionId: version.id, values: values(input) }); studyId = data.id
      await Promise.all([
        ...input.competitors.map(item => axiosClient.post(`/market-studies/${studyId}/competitors`, { name: item.competitorName, productName: item.productName, price: item.price, quantity: item.quantity, estimatedQualityScore: item.estimatedQuality, marketShare: item.marketShare, strengths: item.strengths, weaknesses: item.weaknesses, salesChannels: item.salesChannels, customerRating: item.customerRating })),
        ...input.risks.map(item => axiosClient.post(`/market-studies/${studyId}/risks`, { riskType: item.riskType, probability: item.probability, impact: item.impact, description: item.description, mitigationAction: item.mitigationAction })),
      ])
    }
    if (status === 'Validated') await axiosClient.post(`/market-studies/${studyId}/validate`)
    return this.get(studyId)
  },
  async createOptimizationRequest(
    input: Omit<CommercialOptimizationRequest, 'id' | 'createdAt' | 'status'>,
  ): Promise<CommercialOptimizationRequest> {
    const study = (await this.getAll()).find(item =>
      item.productId === input.productId && item.productVersion === input.productVersion && item.status === 'Validated')
    if (!study) throw new Error('Aucune étude validée ne correspond à cette version du produit.')
    const { data } = await axiosClient.post<{
      id: string; message: string; priority: CommercialOptimizationRequest['priority'];
      requestedChanges: string[]; status: 'New'; createdAt: string
    }>('/optimization-requests', {
      marketStudyId: study.id,
      message: input.message,
      priority: input.priority,
      requestedChanges: input.requestedChanges,
    })
    return { ...input, ...data }
  },
}
