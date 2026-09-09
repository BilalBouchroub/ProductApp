export const PRODUCT_STATUSES = ['Draft', 'InConfiguration', 'InExperiment', 'ReadyForMarketStudy', 'UnderMarketStudy', 'Approved', 'ToOptimize', 'Rejected', 'Archived'] as const
export type ProductStatus = (typeof PRODUCT_STATUSES)[number]
export const PRODUCTION_UNITS = ['kg', 'tonne', 'unité', 'lot', 'carton'] as const
export type ProductionUnit = (typeof PRODUCTION_UNITS)[number]

export interface Product {
  id: string
  name: string
  internalReference: string
  sapCode: string
  category: string
  description: string
  imageUrl: string | null
  themeColor?: string
  targetSalePrice: number
  batchQuantity: number
  productionUnit: ProductionUnit
  status: ProductStatus
  version: number
  createdAt: string
  updatedAt: string
  productionManagerName: string
}

export type ProductFormInput = Pick<Product, 'name' | 'internalReference' | 'sapCode' | 'category' | 'description' | 'imageUrl' | 'targetSalePrice' | 'batchQuantity' | 'productionUnit' | 'status'> & { themeColor: string }

export interface ProductCategoryOption { id: string; code: string; name: string; description: string | null }

export const STEP_STATUSES = ['Draft', 'Active', 'Inactive', 'Validated'] as const
export type ProductionStepStatus = (typeof STEP_STATUSES)[number]
export const PRODUCTION_STEP_ICONS = ['Automatic', 'Materials', 'Preparation', 'Mixing', 'Processing', 'Heating', 'Cooling', 'QualityControl', 'Packaging', 'Storage', 'Palletizing'] as const
export type ProductionStepIcon = (typeof PRODUCTION_STEP_ICONS)[number]
export const RESOURCE_TYPES = ['RawMaterial', 'Machine', 'Equipment', 'Labor', 'Energy', 'Consumable', 'Packaging'] as const
export type ResourceType = (typeof RESOURCE_TYPES)[number]
export const AVAILABILITY_STATUSES = ['Available', 'LowStock', 'Unavailable', 'ToOrder'] as const
export type ResourceAvailabilityStatus = (typeof AVAILABILITY_STATUSES)[number]

export interface StepResource {
  id: string
  stepId: string
  sapCode: string
  designation: string
  resourceType: ResourceType
  plannedQuantity: number
  actualQuantity: number
  unit: string
  unitCost: number
  totalCost: number
  availableStock: number
  supplier: string
  batchNumber: string
  expirationDate: string | null
  availabilityStatus: ResourceAvailabilityStatus
}

export type StepResourceInput = Omit<StepResource, 'id' | 'stepId' | 'totalCost' | 'availabilityStatus'>

export interface ProductionStep {
  id: string
  productId: string
  name: string
  icon?: ProductionStepIcon
  description: string
  order: number
  plannedDurationMinutes: number
  actualDurationMinutes: number
  plannedCost: number
  actualCost: number
  temperature: number | null
  pressure: number | null
  humidity: number | null
  plannedOutputQuantity: number
  actualOutputQuantity: number
  wasteQuantity: number
  equipment: string[]
  operatorCount: number
  laborCost: number
  energyConsumption: number
  instructions: string
  validationCriteria: string
  observations: string
  status: ProductionStepStatus
  resources: StepResource[]
}

export type ProductionStepInput = Omit<ProductionStep, 'id' | 'productId' | 'order' | 'resources'>

export interface ProductionProcess {
  id: string
  productId: string
  productVersion: number
  version: number
  steps: ProductionStep[]
  updatedAt: string
  savedAt: string | null
}

export interface ProcessSummary {
  plannedCost: number
  actualCost: number
  plannedDurationMinutes: number
  actualDurationMinutes: number
  resourceCount: number
  wasteQuantity: number
}

export interface ProcessValidationIssue { id: string; stepId: string | null; field: string; message: string; severity: 'error' | 'warning' }

export interface EquipmentOption { id: string; sapCode: string; name: string; category: 'Machine' | 'Equipment'; status: 'Available' | 'Maintenance' | 'Unavailable'; hourlyCost: number }
export interface SapMaterialOption { sapCode: string; designation: string; unit: string; unitCost: number; availableStock: number; supplier: string; batchNumber: string; expirationDate: string | null }

export const EXPERIMENT_RESULTS = ['Success', 'PartialSuccess', 'Failed', 'Cancelled'] as const
export type ExperimentResult = (typeof EXPERIMENT_RESULTS)[number]
export interface ExperimentIncident { id: string; title: string; description: string; severity: 'Low' | 'Medium' | 'High'; occurredAt: string }
export interface ExperimentStepResult { stepId: string; stepName: string; plannedCost: number; actualCost: number; plannedDuration: number; actualDuration: number; plannedOutputQuantity: number; actualOutputQuantity: number; observations: string }

export interface ProductionExperiment {
  id: string
  name: string
  productId: string
  productName: string
  productVersion: number
  startDate: string
  endDate: string | null
  managerName: string
  objective: string
  hypothesis: string
  plannedQuantity: number
  actualQuantity: number
  plannedCost: number
  actualCost: number
  plannedDuration: number
  actualDuration: number
  wasteRate: number
  result: ExperimentResult
  observations: string
  conclusion: string
  incidents: ExperimentIncident[]
  stepResults: ExperimentStepResult[]
}

export type ExperimentFormInput = Pick<ProductionExperiment, 'name' | 'productId' | 'productVersion' | 'startDate' | 'endDate' | 'objective' | 'hypothesis' | 'plannedQuantity' | 'actualQuantity' | 'plannedCost' | 'actualCost' | 'plannedDuration' | 'actualDuration' | 'wasteRate' | 'result' | 'observations' | 'conclusion'>

export const OPTIMIZATION_PRIORITIES = ['Low', 'Medium', 'High', 'Urgent'] as const
export type OptimizationPriority = (typeof OPTIMIZATION_PRIORITIES)[number]
export const OPTIMIZATION_STATUSES = ['New', 'InReview', 'InProgress', 'Resolved', 'Rejected'] as const
export type OptimizationRequestStatus = (typeof OPTIMIZATION_STATUSES)[number]
export interface OptimizationRequest { id: string; productId: string; productName: string; productVersion: number; commercialManagerName: string; message: string; priority: OptimizationPriority; requestedChanges: string[]; createdAt: string; status: OptimizationRequestStatus }
export interface ProductionNotification { id: string; title: string; message: string; createdAt: string; recipientRole: 'CommercialManager'; read: boolean }
export interface ProductionAuditEntry { id: string; action: string; entityId: string; description: string; createdAt: string; actorName: string }

export interface ProductionDashboardMetrics {
  totalProducts: number
  drafts: number
  configuring: number
  experimenting: number
  ready: number
  totalExperiments: number
  successes: number
  partialSuccesses: number
  failures: number
  averageCost: number
  averageDuration: number
  averageWasteRate: number
  lowStockResources: number
}

export interface ProductionDashboardData {
  metrics: ProductionDashboardMetrics
  costByProduct: { name: string; cost: number }[]
  durationByProduct: { name: string; duration: number }[]
  productsByStatus: { name: string; value: number }[]
  experimentResults: { name: string; value: number }[]
  costComparison: { name: string; planned: number; actual: number }[]
  durationComparison: { name: string; planned: number; actual: number }[]
  resourceConsumption: { name: string; planned: number; actual: number }[]
  costByStep: { name: string; cost: number }[]
  recentProducts: Product[]
  recentExperiments: ProductionExperiment[]
  optimizationRequests: OptimizationRequest[]
  warnings: string[]
  updatedAt: string
}
