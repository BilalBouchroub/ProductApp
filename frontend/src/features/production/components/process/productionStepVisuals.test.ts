import { Factory, Flame, Snowflake } from 'lucide-react'
import { describe, expect, it } from 'vitest'
import type { ProductionStep } from '../../types/production.types'
import { getProductionStepVisual } from './productionStepVisuals'

const makeStep = (overrides: Partial<ProductionStep> = {}): ProductionStep => ({
  id: 'step-1',
  productId: 'product-1',
  name: 'Opération personnalisée',
  icon: 'Automatic',
  description: 'Description de l’opération',
  order: 2,
  plannedDurationMinutes: 10,
  actualDurationMinutes: 0,
  plannedCost: 0,
  actualCost: 0,
  temperature: null,
  pressure: null,
  humidity: null,
  plannedOutputQuantity: 0,
  actualOutputQuantity: 0,
  wasteQuantity: 0,
  equipment: [],
  operatorCount: 1,
  laborCost: 0,
  energyConsumption: 0,
  instructions: '',
  validationCriteria: '',
  observations: '',
  status: 'Draft',
  resources: [],
  ...overrides,
})

describe('getProductionStepVisual', () => {
  it('utilise toujours l’icône choisie manuellement, quel que soit l’ordre', () => {
    const visual = getProductionStepVisual(makeStep({ icon: 'Heating', order: 2 }))

    expect(visual.Icon).toBe(Flame)
  })

  it('déduit le refroidissement depuis le nom en mode automatique', () => {
    const visual = getProductionStepVisual(makeStep({ name: 'Refroidissement rapide', order: 2 }))

    expect(visual.Icon).toBe(Snowflake)
  })

  it('ne choisit plus une icône au hasard d’après le numéro', () => {
    const visual = getProductionStepVisual(makeStep({ name: 'Opération spéciale', order: 9 }))

    expect(visual.Icon).toBe(Factory)
  })
})
