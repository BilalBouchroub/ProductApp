import { Select } from '../../../../components/ui/Select'
import { useEquipmentOptions } from '../../hooks/useProductionReferences'
import type { ProductionStep } from '../../types/production.types'
import { ProductionStepForm } from './ProductionStepForm'

export function ProductionStepEditor({ step, onUpdate }: {
  step: ProductionStep
  onUpdate: (step: ProductionStep) => void
}) {
  const equipment = useEquipmentOptions()
  const options = [{ label: equipment.isLoading ? 'Chargement des machines...' : 'Choisir une machine', value: '' },
    ...(equipment.data ?? []).map(item => ({ label: `${item.name} - ${item.status}`, value: item.name }))]

  return <div className='space-y-5'>
    <section className='rounded-2xl border border-slate-200 bg-slate-50 p-4'>
      <h2 className='font-bold text-slate-900'>Machine de cette etape</h2>
      <p className='mt-1 mb-3 text-sm text-slate-500'>Liste provenant du referentiel Equipment de SQL Server.</p>
      <Select label='Machine' options={options} value={step.equipment[0] ?? ''} disabled={equipment.isLoading}
        onChange={event => onUpdate({ ...step, equipment: event.target.value ? [event.target.value] : [] })}
        error={equipment.isError ? 'Machines indisponibles.' : undefined} />
    </section>
    <ProductionStepForm step={step} onUpdate={onUpdate} />
  </div>
}
