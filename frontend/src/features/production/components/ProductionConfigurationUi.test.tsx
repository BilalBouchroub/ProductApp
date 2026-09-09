import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState, type ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { productionReferencesApi } from '../../../api/productionReferencesApi'
import { mockProcesses } from '../mocks/processes.mock'
import { ProductionChainCanvas } from './process/ProductionChainCanvas'
import { ProductionStepDetailsPanel } from './process/ProductionStepDetailsPanel'
import { ProductionStepEditor } from './process/ProductionStepEditor'
import { ProductForm } from './products/ProductForm'

function renderWithQueryClient(element: ReactNode) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(<QueryClientProvider client={client}>{element}</QueryClientProvider>)
}

function InteractiveChain({ steps }: { steps: typeof mockProcesses[number]['steps'] }) {
  const [selectedId, setSelectedId] = useState(steps[0]?.id ?? null)
  const selected = steps.find(step => step.id === selectedId)
  return <><ProductionChainCanvas steps={steps} selectedId={selectedId} onSelect={setSelectedId} onReorder={vi.fn()} />
    {selected && <ProductionStepDetailsPanel step={selected} onDuplicate={vi.fn()} onToggle={vi.fn()} onDelete={vi.fn()} />}</>
}

describe('configuration de production', () => {
  it('charge les categories de reference dans la liste produit', async () => {
    vi.spyOn(productionReferencesApi, 'getCategories').mockResolvedValue([
      { id: 'cat-1', code: 'ELECTRONICS', name: 'Electronique', description: null },
    ])

    renderWithQueryClient(<ProductForm onSubmit={vi.fn()} />)

    expect(await screen.findByRole('option', { name: 'Electronique' })).toBeInTheDocument()
    expect(screen.getByLabelText('Ou choisir une image')).toHaveAttribute('accept', 'image/jpeg,image/png,image/webp')
  })

  it('charge les machines et applique le choix a une etape', async () => {
    const user = userEvent.setup()
    const onUpdate = vi.fn()
    const step = mockProcesses[0]!.steps[0]!
    vi.spyOn(productionReferencesApi, 'getEquipment').mockResolvedValue([
      { id: 'eq-1', sapCode: 'EQ-OVEN', name: 'Four industriel', category: 'Machine', status: 'Available', hourlyCost: 520 },
    ])

    renderWithQueryClient(<ProductionStepEditor step={step} onUpdate={onUpdate} />)
    expect(await screen.findByRole('option', { name: 'Four industriel - Available' })).toBeInTheDocument()
    await user.selectOptions(screen.getByLabelText('Machine'), 'Four industriel')

    expect(onUpdate).toHaveBeenCalledWith(expect.objectContaining({ equipment: ['Four industriel'] }))
  })

  it('affiche la chaine responsive avec des couleurs distinctes et une selection', async () => {
    const user = userEvent.setup()
    const steps = mockProcesses[0]!.steps.slice(0, 2)
    render(<InteractiveChain steps={steps} />)

    expect(screen.getByRole('heading', { name: 'Chaine de Production' })).toBeInTheDocument()
    const first = screen.getByRole('button', { name: `${steps[0]!.name}, statut Actif, Terminee` })
    const second = screen.getByRole('button', { name: `${steps[1]!.name}, statut Actif, En cours` })
    expect(first).toHaveAttribute('aria-pressed', 'true')
    expect(first.firstElementChild?.getAttribute('style')).not.toEqual(second.firstElementChild?.getAttribute('style'))
    expect(screen.getByRole('heading', { name: new RegExp(steps[0]!.name) })).toBeInTheDocument()
    await user.click(second)
    expect(second).toHaveAttribute('aria-pressed', 'true')
    expect(screen.getByRole('heading', { name: new RegExp(steps[1]!.name) })).toBeInTheDocument()
  })
})
