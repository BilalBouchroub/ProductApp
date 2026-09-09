import type { ProductionStep } from '../../types/production.types'
import { getProductionStepVisual } from './productionStepVisuals'

interface ProductionStepCardProps {
  step: ProductionStep
  selected: boolean
  currentOrder?: number | null
  onSelect: () => void
  dragHandleProps?: React.HTMLAttributes<HTMLButtonElement>
}

const statusLabels: Record<ProductionStep['status'], string> = {
  Draft: 'Brouillon',
  Active: 'Actif',
  Inactive: 'Inactif',
  Validated: 'Validé',
}

export function ProductionStepCard({ step, selected, currentOrder, onSelect, dragHandleProps }: ProductionStepCardProps) {
  const visual = getProductionStepVisual(step, currentOrder)
  const { Icon } = visual
  const statusLabel = statusLabels[step.status]
  return <button type='button' {...dragHandleProps} onClick={onSelect} aria-pressed={selected}
    aria-label={`${step.name}, statut ${statusLabel}, ${visual.stateLabel}`}
    className='group flex w-full touch-none items-center gap-4 rounded-2xl px-2 py-2 text-left outline-none transition duration-200 hover:bg-white/70 focus-visible:ring-4 focus-visible:ring-blue-100 sm:w-28 sm:flex-col sm:gap-2 sm:text-center'>
    <span className={`relative grid size-14 shrink-0 place-items-center rounded-full border shadow-sm transition duration-200 group-hover:scale-105 ${selected ? 'scale-110 shadow-lg ring-4 ring-white' : ''}`}
      style={{ color: visual.color, backgroundColor: visual.background, borderColor: selected ? visual.color : visual.border,
        boxShadow: selected ? `0 8px 24px ${visual.color}30` : undefined }}>
      <Icon className='size-6' strokeWidth={1.9} aria-hidden='true' />
      <span className='absolute -top-0.5 -right-0.5 size-3.5 rounded-full border-2 border-white'
        style={{ backgroundColor: visual.statusColor }} aria-hidden='true' />
    </span>
    <span className='min-w-0 sm:w-28'>
      <span className='block truncate text-sm font-bold text-slate-800'>{step.name}</span>
      <span className='mt-0.5 block text-[11px] font-semibold' style={{ color: visual.statusColor }}>
        {statusLabel} · {visual.stateLabel}{visual.state === 'running' ? ` · ${visual.progress}%` : ''}
      </span>
    </span>
  </button>
}
