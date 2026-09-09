import { describe, expect, it } from 'vitest'
import type { Product, ProductionExperiment, ProductionProcess } from '../features/production/types/production.types'
import { buildCommercialProductView } from './commercialProductsApi'

const product: Product = {
  id: 'product-1', name: 'Petit Beurre', internalReference: 'PB-001', sapCode: 'SAP-PB-001', category: 'Biscuits',
  description: 'Biscuit classique', imageUrl: null, themeColor: '#2563EB', targetSalePrice: 15, batchQuantity: 1_200,
  productionUnit: 'kg', status: 'ReadyForMarketStudy', version: 1, createdAt: '2026-08-01T08:00:00Z',
  updatedAt: '2026-08-10T08:00:00Z', productionManagerName: 'Responsable Production',
}

const process: ProductionProcess = {
  id: 'process-1', productId: product.id, productVersion: 1, version: 1, updatedAt: product.updatedAt, savedAt: product.updatedAt,
  steps: [{
    id: 'step-1', productId: product.id, name: 'Cuisson', icon: 'Heating', description: 'Cuisson au four', order: 1,
    plannedDurationMinutes: 30, actualDurationMinutes: 31, plannedCost: 40, actualCost: 102, temperature: 180,
    pressure: null, humidity: 20, plannedOutputQuantity: 1_200, actualOutputQuantity: 1_180, wasteQuantity: 20,
    equipment: ['Four 1'], operatorCount: 2, laborCost: 10, energyConsumption: 25, instructions: 'Préchauffer le four',
    validationCriteria: 'Couleur dorée', observations: 'Stable', status: 'Validated',
    resources: [{ id: 'resource-1', stepId: 'step-1', sapCode: 'FAR-001', designation: 'Farine', resourceType: 'RawMaterial',
      plannedQuantity: 10, actualQuantity: 10, unit: 'kg', unitCost: 5, totalCost: 50, availableStock: 100,
      supplier: 'Fournisseur', batchNumber: 'LOT-1', expirationDate: null, availabilityStatus: 'Available' }],
  }],
}

const experiment = (version: number): ProductionExperiment => ({
  id: `experiment-${version}`, name: `Essai v${version}`, productId: product.id, productName: product.name, productVersion: version,
  startDate: '2026-08-09T08:00:00Z', endDate: '2026-08-09T10:00:00Z', managerName: 'Responsable Production',
  objective: 'Valider la gamme', hypothesis: 'Processus stable', plannedQuantity: 1_200, actualQuantity: 1_180,
  plannedCost: 100, actualCost: 102, plannedDuration: 30, actualDuration: 31, wasteRate: 1.67, result: 'Success',
  observations: 'Conforme', conclusion: 'Validée', incidents: [], stepResults: [],
})

describe('buildCommercialProductView', () => {
  it('reprend la chaîne, les ressources et les calculs réels de Production', () => {
    const view = buildCommercialProductView(product, process, [experiment(1), experiment(2)], [])

    expect(view.stepCount).toBe(1)
    expect(view.process.steps[0]?.name).toBe('Cuisson')
    expect(view.resources.map(resource => resource.designation)).toEqual(['Farine'])
    expect(view.productionCost).toBe(100)
    expect(view.totalDurationMinutes).toBe(30)
    expect(view.experiments.map(item => item.name)).toEqual(['Essai v1'])
    expect(view.latestExperimentResult).toBe('Success')
  })
})
