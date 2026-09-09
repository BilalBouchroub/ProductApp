import { DndContext, PointerSensor, closestCenter, useSensor, useSensors, type DragEndEvent } from '@dnd-kit/core'
import { SortableContext, rectSortingStrategy } from '@dnd-kit/sortable'
import { ArrowRight, ChevronDown, Factory } from 'lucide-react'
import type { ProductionStep } from '../../types/production.types'
import { getCurrentProductionOrder } from './productionStepVisuals'
import { SortableProductionStep } from './SortableProductionStep'

interface ProductionChainCanvasProps {
  steps: ProductionStep[]
  selectedId: string | null
  onSelect: (id: string) => void
  onReorder: (active: string, over: string) => void
}

const legend = [
  { label: 'Terminee', color: '#16A34A' },
  { label: 'En cours', color: '#F59E0B' },
  { label: 'Probleme', color: '#DC2626' },
  { label: 'En attente', color: '#94A3B8' },
]

export function ProductionChainCanvas({ steps, selectedId, onSelect, onReorder }: ProductionChainCanvasProps) {
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 6 } }))
  const currentOrder = getCurrentProductionOrder(steps)
  const handleDragEnd = (event: DragEndEvent) => {
    if (event.over && event.active.id !== event.over.id) onReorder(String(event.active.id), String(event.over.id))
  }

  return <section className='rounded-2xl border border-slate-200 bg-white p-5 shadow-sm'>
    <div className='flex flex-wrap items-start justify-between gap-4'>
      <div><h2 className='text-lg font-bold text-slate-950'>Chaine de Production</h2><p className='mt-1 text-sm text-slate-500'>Cliquer sur une etape pour afficher les details</p></div>
      <div className='flex flex-wrap gap-3'>{legend.map(item => <span key={item.label} className='inline-flex items-center gap-1.5 text-xs font-semibold text-slate-500'><span className='size-2.5 rounded-full' style={{ backgroundColor: item.color }} />{item.label}</span>)}</div>
    </div>

    {!steps.length ? <div className='mt-5 grid min-h-56 place-items-center rounded-2xl border-2 border-dashed border-slate-200 bg-slate-50'>
      <div className='text-center'><Factory className='mx-auto size-9 text-slate-400' /><p className='mt-3 font-semibold text-slate-700'>La chaine est vide</p><p className='text-sm text-slate-500'>Ajoutez votre premiere etape pour commencer.</p></div>
    </div> : <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
      <SortableContext items={steps.map(step => step.id)} strategy={rectSortingStrategy}>
        <div className='mt-6 overflow-x-auto pb-2'>
          <div className='flex min-w-0 flex-col items-stretch sm:min-w-max sm:flex-row sm:items-start'>
            {steps.map((step, index) => <div key={step.id} className='flex w-full flex-col sm:w-auto sm:flex-row sm:items-start'>
              <SortableProductionStep id={step.id} step={step} selected={selectedId === step.id} currentOrder={currentOrder} onSelect={() => onSelect(step.id)} />
              {index < steps.length - 1 && <div className='ml-7 flex h-9 items-center text-slate-300 sm:ml-0 sm:h-16 sm:w-9 sm:justify-center sm:pt-1' aria-hidden='true'>
                <ChevronDown className='size-5 sm:hidden' />
                <ArrowRight className='hidden size-5 sm:block' />
              </div>}
            </div>)}
          </div>
        </div>
      </SortableContext>
    </DndContext>}
  </section>
}
