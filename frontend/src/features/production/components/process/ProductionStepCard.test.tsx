import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import type { ProductionStep } from '../../types/production.types'
import { ProductionStepCard } from './ProductionStepCard'

const step: ProductionStep = {
  id: 'step-status',
  productId: 'product-status',
  name: 'Refroidissement',
  icon: 'Cooling',
  description: 'Refroidissement du produit',
  order: 1,
  plannedDurationMinutes: 10,
  actualDurationMinutes: 0,
  plannedCost: 0,
  actualCost: 0,
  temperature: 20,
  pressure: null,
  humidity: null,
  plannedOutputQuantity: 100,
  actualOutputQuantity: 0,
  wasteQuantity: 0,
  equipment: [],
  operatorCount: 1,
  laborCost: 0,
  energyConsumption: 0,
  instructions: '',
  validationCriteria: '',
  observations: '',
  status: 'Inactive',
  resources: [],
}

describe('ProductionStepCard', () => {
  it('affiche en permanence le statut enregistré de l’étape', () => {
    render(<ProductionStepCard step={step} selected={false} onSelect={vi.fn()} />)

    expect(screen.getByText(/Inactif/)).toBeInTheDocument()
    expect(screen.getByRole('button')).toHaveAccessibleName(/statut Inactif/)
  })
})
