import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { useForm, type Resolver } from 'react-hook-form'
import { toast } from 'sonner'
import { Button } from '../../../../components/ui/Button'
import { ConfirmDialog } from '../../../../components/ui/ConfirmDialog'
import { Input } from '../../../../components/ui/Input'
import { Select } from '../../../../components/ui/Select'
import { TextArea } from '../../../../components/ui/TextArea'
import { productionStepSchema, type ProductionStepFormValues } from '../../schemas/productionStepSchema'
import type { ProductionStep, StepResource, StepResourceInput } from '../../types/production.types'
import { STEP_STATUSES } from '../../types/production.types'
import { createResource } from '../../utils/productionCalculations'
import { ResourceCostSummary } from '../resources/ResourceCostSummary'
import { StepResourceForm } from '../resources/StepResourceForm'
import { StepResourcesTable } from '../resources/StepResourcesTable'
import { productionStepIconOptions } from './productionStepVisuals'

const formDefaults = (step: ProductionStep): ProductionStepFormValues => ({
  ...step,
  icon: step.icon ?? 'Automatic',
})

export function ProductionStepForm({ step, onUpdate }: { step: ProductionStep; onUpdate: (step: ProductionStep) => void }) {
  const { register, handleSubmit, reset, formState: { errors } } = useForm<ProductionStepFormValues>({
    resolver: zodResolver(productionStepSchema) as Resolver<ProductionStepFormValues>,
    defaultValues: formDefaults(step),
  })
  const [resourceOpen, setResourceOpen] = useState(false)
  const [editing, setEditing] = useState<StepResource | null>(null)
  const [deleting, setDeleting] = useState<StepResource | null>(null)

  useEffect(() => reset(formDefaults(step)), [step, reset])

  const save = (value: ProductionStepFormValues) => {
    onUpdate({ ...step, ...value })
    toast.success('Étape mise à jour.')
  }
  const resourceInput = (resource: StepResource): StepResourceInput => ({
    sapCode: resource.sapCode,
    designation: resource.designation,
    resourceType: resource.resourceType,
    plannedQuantity: resource.plannedQuantity,
    actualQuantity: resource.actualQuantity,
    unit: resource.unit,
    unitCost: resource.unitCost,
    availableStock: resource.availableStock,
    supplier: resource.supplier,
    batchNumber: resource.batchNumber,
    expirationDate: resource.expirationDate,
  })

  return <div className='space-y-5'>
    <form onSubmit={handleSubmit(save)} className='space-y-5'>
      <section>
        <h2 className='font-bold text-slate-900'>Configuration de l’étape</h2>
        <div className='mt-4 grid gap-3 md:grid-cols-2'>
          <Input label='Nom' {...register('name')} error={errors.name?.message} />
          <Select label='Icône de l’étape' options={productionStepIconOptions} {...register('icon')} error={errors.icon?.message} />
          <Select label='Statut' options={STEP_STATUSES.map(status => ({ label: status, value: status }))} {...register('status')} />
          <Input label='Durée prévue (min)' type='number' {...register('plannedDurationMinutes')} error={errors.plannedDurationMinutes?.message} />
          <Input label='Coût opérationnel (MAD)' type='number' step='0.01' {...register('plannedCost')} />
          <Input label='Sortie prévue' type='number' {...register('plannedOutputQuantity')} />
          <Input label='Opérateurs' type='number' {...register('operatorCount')} />
          <Input label='Coût main-d’œuvre' type='number' {...register('laborCost')} />
          <Input label='Énergie (kWh)' type='number' {...register('energyConsumption')} />
        </div>
        <p className='mt-2 text-xs text-slate-500'>Le mode automatique se base sur le nom de l’étape. Un choix manuel reste inchangé même si vous réorganisez la chaîne.</p>
        <div className='mt-3'><TextArea label='Description' rows={2} {...register('description')} error={errors.description?.message} /></div>
      </section>

      <section>
        <h3 className='font-semibold text-slate-800'>Paramètres physiques optionnels</h3>
        <div className='mt-3 grid gap-3 sm:grid-cols-3'>
          <Input label='Température °C' type='number' {...register('temperature')} />
          <Input label='Pression bar' type='number' {...register('pressure')} />
          <Input label='Humidité %' type='number' {...register('humidity')} />
        </div>
      </section>

      <section className='grid gap-3'>
        <TextArea label='Instructions' rows={2} {...register('instructions')} />
        <TextArea label='Critères de validation' rows={2} {...register('validationCriteria')} />
        <TextArea label='Observations' rows={2} {...register('observations')} />
      </section>
      <Button type='submit' className='w-full'>Appliquer les modifications</Button>
    </form>

    <section className='border-t border-slate-200 pt-5'>
      <div className='mb-3 flex items-center justify-between'>
        <div><h3 className='font-bold text-slate-900'>Ressources</h3><p className='text-xs text-slate-500'>Matières, machines, énergie et main-d’œuvre.</p></div>
        <Button size='sm' onClick={() => { setEditing(null); setResourceOpen(true) }}>Ajouter</Button>
      </div>
      <StepResourcesTable resources={step.resources} onEdit={resource => { setEditing(resource); setResourceOpen(true) }} onDelete={setDeleting} />
      <div className='mt-3'><ResourceCostSummary resources={step.resources} /></div>
    </section>

    <StepResourceForm open={resourceOpen} initial={editing ? resourceInput(editing) : undefined}
      onClose={() => setResourceOpen(false)} onSubmit={input => {
        const resource = editing ? { ...createResource(input, step.id), id: editing.id } : createResource(input, step.id)
        onUpdate({ ...step, resources: editing ? step.resources.map(item => item.id === editing.id ? resource : item) : [...step.resources, resource] })
        setResourceOpen(false)
        toast.success(editing ? 'Ressource modifiée.' : 'Ressource ajoutée.')
      }} />
    <ConfirmDialog open={Boolean(deleting)} title='Supprimer la ressource' description='Cette ressource sera retirée de l’étape.'
      onClose={() => setDeleting(null)} onConfirm={() => {
        if (deleting) onUpdate({ ...step, resources: step.resources.filter(resource => resource.id !== deleting.id) })
        setDeleting(null)
      }} />
  </div>
}
