import { axiosClient } from './axiosClient'
import type { PagedResponse } from './api.types'
import { productsApi } from './productsApi'
import type { ExperimentFormInput, ExperimentResult, ProductionExperiment } from '../features/production/types/production.types'

interface ExperimentStepResponse { id: string; productionStepId: string; order: number; plannedCost: number; actualCost: number | null; plannedDurationMinutes: number; actualDurationMinutes: number | null; plannedOutputQuantity: number | null; actualOutputQuantity: number | null; isValidated: boolean }
interface ExperimentResponse { id: string; productVersionId: string; name: string; objective?: string; hypothesis?: string | null; startDate: string; endDate: string | null; plannedQuantity: number; actualQuantity: number | null; plannedCost: number; actualCost: number | null; plannedDurationMinutes: number; actualDurationMinutes: number | null; wasteRate: number | null; result: ExperimentResult; observations?: string | null; conclusion?: string | null; steps: ExperimentStepResponse[] }
const mapExperiment = (item: ExperimentResponse, productId = '', productName = '', productVersion = 1): ProductionExperiment => ({
  id: item.id, name: item.name, productId, productName, productVersion, startDate: item.startDate, endDate: item.endDate,
  managerName: 'ProductApp API', objective: item.objective ?? '', hypothesis: item.hypothesis ?? '', plannedQuantity: item.plannedQuantity, actualQuantity: item.actualQuantity ?? 0,
  plannedCost: item.plannedCost, actualCost: item.actualCost ?? 0, plannedDuration: item.plannedDurationMinutes, actualDuration: item.actualDurationMinutes ?? 0,
  wasteRate: item.wasteRate ?? 0, result: item.result, observations: item.observations ?? '', conclusion: item.conclusion ?? '', incidents: [],
  stepResults: item.steps.map(step => ({ stepId: step.productionStepId, stepName: `Étape ${step.order}`, plannedCost: step.plannedCost,
    actualCost: step.actualCost ?? 0, plannedDuration: step.plannedDurationMinutes, actualDuration: step.actualDurationMinutes ?? 0,
    plannedOutputQuantity: step.plannedOutputQuantity ?? 0, actualOutputQuantity: step.actualOutputQuantity ?? 0, observations: '' })) })

export const experimentsApi = {
  async getAll(): Promise<ProductionExperiment[]> {
    const products = await productsApi.getAll()
    const groups = await Promise.all(products.map(async product => {
      const versions = await productsApi.getVersions(product.id)
      const byVersion = await Promise.all(versions.map(async version => {
        const { data } = await axiosClient.get<PagedResponse<ExperimentResponse>>('/experiments', { params: { productId: product.id, versionNumber: version.versionNumber, pageSize: 100 } })
        return data.items.map(item => mapExperiment(item, product.id, product.name, version.versionNumber))
      }))
      return byVersion.flat()
    }))
    return groups.flat().sort((a, b) => b.startDate.localeCompare(a.startDate))
  },
  async get(id: string): Promise<ProductionExperiment> {
    const { data } = await axiosClient.get<ExperimentResponse>(`/experiments/${id}`)
    const products = await productsApi.getAll()
    for (const product of products) {
      const version = (await productsApi.getVersions(product.id)).find(item => item.id === data.productVersionId)
      if (version) return mapExperiment(data, product.id, product.name, version.versionNumber)
    }
    return mapExperiment(data)
  },
  async create(input: ExperimentFormInput): Promise<ProductionExperiment> {
    const { data } = await axiosClient.post<ExperimentResponse>('/experiments', { productId: input.productId, versionNumber: input.productVersion,
      name: input.name, objective: input.objective, hypothesis: input.hypothesis || null, startDate: input.startDate,
      plannedQuantity: input.plannedQuantity, observations: input.observations || null, conclusion: input.conclusion || null })
    return mapExperiment(data, input.productId, '', input.productVersion)
  },
  async updateNarrative(id: string, input: Pick<ExperimentFormInput, 'objective' | 'hypothesis' | 'observations' | 'conclusion'>): Promise<ProductionExperiment> {
    const { data } = await axiosClient.put<ExperimentResponse>(`/production-experiments/${id}/narrative`, {
      objective: input.objective,
      hypothesis: input.hypothesis || null,
      observations: input.observations || null,
      conclusion: input.conclusion || null,
    })
    return mapExperiment(data)
  },
}
