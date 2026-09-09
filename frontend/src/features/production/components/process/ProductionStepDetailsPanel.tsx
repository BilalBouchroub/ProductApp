import { AlertTriangle, Copy, Power, Trash2 } from 'lucide-react'
import { Button } from '../../../../components/ui/Button'
import type { ProductionStep } from '../../types/production.types'
import { getProductionStepVisual } from './productionStepVisuals'

interface ProductionStepDetailsPanelProps {
  step: ProductionStep
  currentOrder?: number | null
  saving?: boolean
  onDuplicate: () => void
  onToggle: () => void
  onDelete: () => void
}

const number = new Intl.NumberFormat('fr-FR', { maximumFractionDigits: 1 })

const statusLabels: Record<ProductionStep['status'], string> = {
  Draft: 'Brouillon',
  Active: 'Actif',
  Inactive: 'Inactif',
  Validated: 'Validé',
}

export function ProductionStepDetailsPanel({ step, currentOrder, saving = false, onDuplicate, onToggle, onDelete }: ProductionStepDetailsPanelProps) {
  const visual = getProductionStepVisual(step, currentOrder)
  const alerts = step.resources.filter(resource => resource.availabilityStatus !== 'Available')
  const values = [
    ['Statut enregistré', statusLabels[step.status]],
    ['État opérationnel', visual.stateLabel],
    ['Ordre', step.order.toString().padStart(2, '0')],
    ['Machine', step.equipment[0] ?? 'Non renseignee'],
    ['Operateurs', `${step.operatorCount} operateur(s)`],
    ['Date de debut', 'Non planifiee'],
    ['Date de fin', 'Non planifiee'],
    ['Duree prevue', `${number.format(step.plannedDurationMinutes)} min`],
    ['Duree reelle', step.actualDurationMinutes > 0 ? `${number.format(step.actualDurationMinutes)} min` : ''],
    ['Quantite prevue', `${number.format(step.plannedOutputQuantity)} unites`],
    ['Quantite produite', `${number.format(step.actualOutputQuantity)} unites`],
    ['Temperature', step.temperature === null ? '' : `${number.format(step.temperature)} �C`],
    ['Ressources', String(step.resources.length)],
  ]

  return <section key={step.id} className='step-details-enter rounded-2xl border border-slate-200 bg-white p-5 shadow-sm'>
    <div className='flex flex-wrap items-start justify-between gap-4'>
      <div>
        <p className='text-xs font-bold uppercase tracking-[0.16em]' style={{ color: visual.color }}>Etape {step.order.toString().padStart(2, '0')}</p>
        <h2 className='mt-1 text-xl font-bold text-slate-950'>Details  {step.name}</h2>
        <p className='mt-1 max-w-3xl text-sm text-slate-500'>{step.description || 'Aucune description renseignee.'}</p>
      </div>
      <div className='flex gap-1'>
        <Button size='icon' variant='ghost' onClick={onDuplicate} aria-label={`Dupliquer ${step.name}`}><Copy className='size-4' /></Button>
        <Button size='icon' variant='ghost' disabled={saving} onClick={onToggle} aria-label={`Changer et enregistrer le statut de ${step.name}`}><Power className='size-4' /></Button>
        <Button size='icon' variant='ghost' onClick={onDelete} aria-label={`Supprimer ${step.name}`}><Trash2 className='size-4 text-red-500' /></Button>
      </div>
    </div>

    <div className='mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4'>
      {values.map(([label, value]) => <div key={label} className='rounded-xl border border-slate-100 bg-slate-50/80 px-4 py-3'>
        <p className='text-[11px] font-semibold uppercase tracking-wide text-slate-400'>{label}</p>
        <p className='mt-1 font-bold text-slate-800'>{value}</p>
      </div>)}
    </div>

    <div className='mt-5 rounded-xl border border-slate-100 bg-slate-50/80 p-4'>
      <div className='flex items-center justify-between text-sm'><span className='font-semibold text-slate-700'>Progression</span><strong style={{ color: visual.color }}>{visual.progress}%</strong></div>
      <div className='mt-2 h-2.5 overflow-hidden rounded-full bg-slate-200'>
        <div className='h-full rounded-full transition-[width] duration-700 ease-out' style={{ width: `${visual.progress}%`, backgroundColor: visual.color }} />
      </div>
    </div>

    {alerts.length > 0 && <div className='mt-4 rounded-xl border border-red-100 bg-red-50 p-4'>
      <div className='flex items-center gap-2 font-bold text-red-700'><AlertTriangle className='size-4' />Alertes ressources</div>
      <ul className='mt-2 space-y-1 text-sm text-red-700'>{alerts.map(resource => <li key={resource.id}>" {resource.designation}  {resource.availabilityStatus}</li>)}</ul>
    </div>}
  </section>
}
