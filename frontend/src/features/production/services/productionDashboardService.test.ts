import { describe, expect, it } from 'vitest'
import type { ProductionExperiment, ProductionProcess, Product } from '../types/production.types'
import { buildProductionDashboard } from './productionDashboardService'

const product: Product = {
  id: 'product-1',
  name: 'Biscuit pilote',
  internalReference: 'BIS-001',
  sapCode: 'SAP-001',
  category: 'Biscuits',
  description: 'Produit pilote',
  imageUrl: null,
  themeColor: '#2563EB',
  targetSalePrice: 12,
  batchQuantity: 1_000,
  productionUnit: 'kg',
  status: 'InExperiment',
  version: 1,
  createdAt: '2026-08-01T08:00:00Z',
  updatedAt: '2026-08-10T08:00:00Z',
  productionManagerName: 'Responsable Production',
}

const process: ProductionProcess = {
  id: 'process-1',
  productId: product.id,
  productVersion: 1,
  version: 1,
  updatedAt: product.updatedAt,
  savedAt: product.updatedAt,
  steps: [{
    id: 'step-1',
    productId: product.id,
    name: 'Cuisson',
    icon: 'Heating',
    description: '',
    order: 1,
    plannedDurationMinutes: 30,
    actualDurationMinutes: 32,
    plannedCost: 40,
    actualCost: 104,
    temperature: 180,
    pressure: null,
    humidity: null,
    plannedOutputQuantity: 1_000,
    actualOutputQuantity: 970,
    wasteQuantity: 30,
    equipment: ['Four'],
    operatorCount: 1,
    laborCost: 10,
    energyConsumption: 25,
    instructions: '',
    validationCriteria: '',
    observations: '',
    status: 'Active',
    resources: [{
      id: 'resource-1',
      stepId: 'step-1',
      sapCode: 'FAR-001',
      designation: 'Farine',
      resourceType: 'RawMaterial',
      plannedQuantity: 10,
      actualQuantity: 11,
      unit: 'kg',
      unitCost: 5,
      totalCost: 50,
      availableStock: 8,
      supplier: '',
      batchNumber: '',
      expirationDate: null,
      availabilityStatus: 'LowStock',
    }],
  }],
}

const experiment: ProductionExperiment = {
  id: 'experiment-1',
  name: 'Essai pilote',
  productId: product.id,
  productName: product.name,
  productVersion: 1,
  startDate: '2026-08-09T08:00:00Z',
  endDate: '2026-08-09T10:00:00Z',
  managerName: 'Responsable Production',
  objective: 'Valider la gamme',
  hypothesis: 'La gamme est stable',
  plannedQuantity: 1_000,
  actualQuantity: 970,
  plannedCost: 100,
  actualCost: 104,
  plannedDuration: 30,
  actualDuration: 32,
  wasteRate: 3,
  result: 'Success',
  observations: '',
  conclusion: 'Essai validé',
  incidents: [],
  stepResults: [],
}

describe('buildProductionDashboard', () => {
  it('calcule les statistiques et séries à partir des données de production', () => {
    const dashboard = buildProductionDashboard([product], [experiment], [process], [])

    expect(dashboard.metrics).toMatchObject({
      totalProducts: 1,
      experimenting: 1,
      totalExperiments: 1,
      successes: 1,
      averageCost: 100,
      averageDuration: 30,
      averageWasteRate: 3,
      lowStockResources: 1,
    })
    expect(dashboard.costByProduct).toEqual([{ name: 'Biscuit pilote', cost: 100 }])
    expect(dashboard.resourceConsumption).toEqual([{ name: 'Farine', planned: 10, actual: 11 }])
    expect(dashboard.productsByStatus).toEqual([{ name: 'Expérimentation', value: 1 }])
  })

  it('retourne un dashboard vide exploitable sans valeurs invalides', () => {
    const dashboard = buildProductionDashboard([], [], [], [], ['Données partielles'])

    expect(dashboard.metrics.averageCost).toBe(0)
    expect(dashboard.metrics.averageWasteRate).toBe(0)
    expect(dashboard.costByProduct).toEqual([])
    expect(dashboard.warnings).toEqual(['Données partielles'])
  })
})
