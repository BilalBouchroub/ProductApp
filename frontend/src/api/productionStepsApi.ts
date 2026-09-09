import { axiosClient } from './axiosClient'
import { productsApi } from './productsApi'
import type {
  ProductionProcess,
  ProductionStep,
  ProductionStepIcon,
  ProductionStepStatus,
  StepResource,
} from '../features/production/types/production.types'

type ApiRecordStatus = 'Draft' | 'Active' | 'Inactive' | 'Archived' | 'Validated'

interface ApiStepResource {
  id: string
  productionStepId: string
  resourceType: StepResource['resourceType']
  designation: string
  unit: string
  plannedQuantity: number
  actualQuantity: number | null
  unitCost: number
  totalCost: number
  availableStock: number | null
  remainingStock: number | null
  availability: StepResource['availabilityStatus']
}

interface ApiProductionStep {
  id: string
  productId: string
  order: number
  name: string
  icon?: ProductionStepIcon
  description: string | null
  durationMinutes: number
  actualDurationMinutes: number | null
  plannedCost: number | null
  actualCost: number | null
  temperature: number
  pressure: number | null
  humidity: number | null
  plannedOutputQuantity: number | null
  actualOutputQuantity: number | null
  wasteQuantity: number | null
  equipmentName: string
  operatorCount: number
  laborCost: number
  energyConsumption: number
  instructions: string | null
  validationCriteria: string | null
  observations: string | null
  status: ApiRecordStatus
  resources: ApiStepResource[]
}

const frontendStatus = (status: ApiRecordStatus): ProductionStepStatus =>
  status === 'Archived' ? 'Inactive' : status

const apiStatus = (status: ProductionStepStatus): ApiRecordStatus => status

const mapResource = (resource: ApiStepResource): StepResource => ({
  id: resource.id,
  stepId: resource.productionStepId,
  sapCode: '',
  designation: resource.designation,
  resourceType: resource.resourceType,
  plannedQuantity: resource.plannedQuantity,
  actualQuantity: resource.actualQuantity ?? 0,
  unit: resource.unit,
  unitCost: resource.unitCost,
  totalCost: resource.totalCost,
  availableStock: resource.availableStock ?? 0,
  supplier: '',
  batchNumber: '',
  expirationDate: null,
  availabilityStatus: resource.availability,
})

const mapStep = (step: ApiProductionStep): ProductionStep => ({
  id: step.id,
  productId: step.productId,
  name: step.name,
  icon: step.icon ?? 'Automatic',
  description: step.description ?? '',
  order: step.order,
  plannedDurationMinutes: step.durationMinutes,
  actualDurationMinutes: step.actualDurationMinutes ?? 0,
  plannedCost: step.plannedCost ?? 0,
  actualCost: step.actualCost ?? 0,
  temperature: step.temperature,
  pressure: step.pressure,
  humidity: step.humidity,
  plannedOutputQuantity: step.plannedOutputQuantity ?? 0,
  actualOutputQuantity: step.actualOutputQuantity ?? 0,
  wasteQuantity: step.wasteQuantity ?? 0,
  equipment: step.equipmentName === 'Équipement non renseigné' ? [] : [step.equipmentName],
  operatorCount: step.operatorCount,
  laborCost: step.laborCost,
  energyConsumption: step.energyConsumption,
  instructions: step.instructions ?? '',
  validationCriteria: step.validationCriteria ?? '',
  observations: step.observations ?? '',
  status: frontendStatus(step.status),
  resources: step.resources.map(mapResource),
})

const stepPayload = (step: ProductionStep, order = step.order) => ({
  order,
  name: step.name,
  icon: step.icon ?? 'Automatic',
  description: step.description || null,
  durationMinutes: step.plannedDurationMinutes,
  temperature: step.temperature ?? 0,
  equipmentName: step.equipment[0] ?? 'Équipement non renseigné',
  laborCost: step.laborCost,
  actualDurationMinutes: step.actualDurationMinutes,
  plannedCost: step.plannedCost,
  actualCost: step.actualCost,
  pressure: step.pressure,
  humidity: step.humidity,
  plannedOutputQuantity: step.plannedOutputQuantity,
  actualOutputQuantity: step.actualOutputQuantity,
  wasteQuantity: step.wasteQuantity,
  operatorCount: step.operatorCount,
  energyConsumption: step.energyConsumption,
  instructions: step.instructions || null,
  validationCriteria: step.validationCriteria || null,
  observations: step.observations || null,
  status: apiStatus(step.status),
})

const addResource = async (stepId: string, resource: StepResource): Promise<void> => {
  await axiosClient.post(`/production-steps/${stepId}/resources`, {
    resourceType: resource.resourceType,
    designation: resource.designation,
    unit: resource.unit,
    plannedQuantity: resource.plannedQuantity,
    actualQuantity: resource.actualQuantity,
    unitCost: resource.unitCost,
    availableStock: resource.availableStock,
    rawMaterialId: null,
    equipmentId: null,
  })
}

async function syncResources(stepId: string, local: StepResource[], remote: ApiStepResource[]): Promise<void> {
  const localIds = new Set(local.map(resource => resource.id))
  for (const resource of remote.filter(item => !localIds.has(item.id)))
    await axiosClient.delete(`/production-steps/${stepId}/resources/${resource.id}`)

  const remoteById = new Map(remote.map(resource => [resource.id, resource]))
  for (const resource of local) {
    const current = remoteById.get(resource.id)
    if (!current) {
      await addResource(stepId, resource)
      continue
    }
    const immutableChanged = current.resourceType !== resource.resourceType
      || current.designation !== resource.designation || current.unit !== resource.unit
    if (immutableChanged) {
      await axiosClient.delete(`/production-steps/${stepId}/resources/${resource.id}`)
      await addResource(stepId, resource)
      continue
    }
    await axiosClient.put(`/production-steps/${stepId}/resources/${resource.id}`, {
      plannedQuantity: resource.plannedQuantity,
      actualQuantity: resource.actualQuantity,
      unitCost: resource.unitCost,
      availableStock: resource.availableStock,
    })
  }
}

async function getSteps(productId: string): Promise<ApiProductionStep[]> {
  const { data } = await axiosClient.get<ApiProductionStep[]>(`/products/${productId}/production-steps`)
  return data
}

export const productionStepsApi = {
  async getProcess(productId: string): Promise<ProductionProcess> {
    const [product, steps] = await Promise.all([productsApi.get(productId), getSteps(productId)])
    const now = new Date().toISOString()
    return {
      id: `api-process-${productId}-v${product.version}`,
      productId,
      productVersion: product.version,
      version: 1,
      steps: steps.sort((left, right) => left.order - right.order).map(mapStep),
      updatedAt: now,
      savedAt: steps.length > 0 ? now : null,
    }
  },

  async saveProcess(process: ProductionProcess): Promise<ProductionProcess> {
    const remote = await getSteps(process.productId)
    const draftIds = new Set(process.steps.map(step => step.id))
    for (const step of remote.filter(item => !draftIds.has(item.id)))
      await axiosClient.delete(`/products/${process.productId}/production-steps/${step.id}`)

    const remainingRemote = remote.filter(item => draftIds.has(item.id))
    const remoteById = new Map(remainingRemote.map(step => [step.id, step]))
    const persistedIds = new Map<string, string>()

    for (const step of process.steps) {
      const current = remoteById.get(step.id)
      if (current) {
        const { data } = await axiosClient.put<ApiProductionStep>(
          `/products/${process.productId}/production-steps/${step.id}`,
          stepPayload(step, current.order),
        )
        persistedIds.set(step.id, data.id)
        await syncResources(data.id, step.resources, current.resources)
      }
    }

    let temporaryOrder = Math.max(0, ...remainingRemote.map(step => step.order))
    for (const step of process.steps.filter(item => !remoteById.has(item.id))) {
      temporaryOrder += 1
      const { data } = await axiosClient.post<ApiProductionStep>(
        `/products/${process.productId}/production-steps`,
        stepPayload(step, temporaryOrder),
      )
      persistedIds.set(step.id, data.id)
      await syncResources(data.id, step.resources, [])
    }

    const orderedStepIds = process.steps.map(step => persistedIds.get(step.id) ?? step.id)
    if (orderedStepIds.length > 0)
      await axiosClient.put(`/products/${process.productId}/production-steps/reorder`, { orderedStepIds })

    const saved = await this.getProcess(process.productId)
    return { ...saved, version: process.version, savedAt: new Date().toISOString() }
  },
}
