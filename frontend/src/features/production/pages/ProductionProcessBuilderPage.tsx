import { arrayMove } from '@dnd-kit/sortable'
import { ChevronDown, Plus, Save, SlidersHorizontal } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import { toast } from 'sonner'
import { PageHeader } from '../../../components/common/PageHeader'
import { Button } from '../../../components/ui/Button'
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog'
import { ErrorState } from '../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { ProcessValidationPanel } from '../components/process/ProcessValidationPanel'
import { ProductionChainCanvas } from '../components/process/ProductionChainCanvas'
import { ProductionRunSummary } from '../components/process/ProductionRunSummary'
import { ProductionStepDetailsPanel } from '../components/process/ProductionStepDetailsPanel'
import { ProductionStepEditor } from '../components/process/ProductionStepEditor'
import { getCurrentProductionOrder } from '../components/process/productionStepVisuals'
import { useProductionProcess, useSaveProductionProcess } from '../hooks/useProductionProcess'
import { useProductionProduct } from '../hooks/useProductionProducts'
import type { ProductionProcess, ProductionStep } from '../types/production.types'
import { calculateProcessSummary } from '../utils/productionCalculations'
import { validateProcess } from '../utils/processValidation'

const newStep = (productId: string, order: number): ProductionStep => ({
  id: `step-${crypto.randomUUID()}`,
  productId,
  name: `Nouvelle etape ${order}`,
  icon: 'Automatic',
  description: 'Decrire cette operation de production.',
  order,
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
})

export function ProductionProcessBuilderPage() {
  const { id = '' } = useParams()
  const product = useProductionProduct(id)
  const query = useProductionProcess(id)
  const save = useSaveProductionProcess()
  const [draft, setDraft] = useState<ProductionProcess | null>(null)
  const [selected, setSelected] = useState<string | null>(null)
  const [deleting, setDeleting] = useState<string | null>(null)
  const process = draft ?? query.data
  const steps = process?.steps ?? []
  const selectedStep = steps.find(step => step.id === selected) ?? steps[0]
  const summary = useMemo(() => process ? calculateProcessSummary(process) : null, [process])
  const issues = useMemo(() => process ? validateProcess(process) : [], [process])
  const currentOrder = getCurrentProductionOrder(steps)

  const normalizeSteps = (next: ProductionStep[]) => next.map((step, index) => ({ ...step, order: index + 1 }))
  const change = (next: ProductionStep[]) => {
    if (process) setDraft({ ...process, steps: normalizeSteps(next) })
  }
  const persist = (next: ProductionStep[], message: string, selectedOrder?: number) => {
    if (!process || save.isPending) return
    const nextProcess = {
      ...process,
      version: process.version + 1,
      savedAt: new Date().toISOString(),
      steps: normalizeSteps(next),
    }
    setDraft(nextProcess)
    save.mutate(nextProcess, {
      onSuccess: saved => {
        setDraft(null)
        setSelected(saved.steps.find(step => step.order === selectedOrder)?.id ?? saved.steps[0]?.id ?? null)
        toast.success(message)
      },
      onError: error => toast.error(`Enregistrement impossible : ${error.message}`),
    })
  }
  const duplicateStep = (stepId: string) => {
    const source = steps.find(step => step.id === stepId)
    if (!source) return
    const clone = { ...structuredClone(source), id: `step-${crypto.randomUUID()}`, name: `${source.name}  copie` }
    change([...steps, clone])
    setSelected(clone.id)
  }
  const toggleStep = (stepId: string) => {
    const current = steps.find(step => step.id === stepId)
    if (!current) return
    const status: ProductionStep['status'] = current.status === 'Inactive' ? 'Active' : 'Inactive'
    const next = steps.map(step => step.id === stepId ? { ...step, status } : step)
    persist(next, `Statut « ${status} » enregistré définitivement.`, current.order)
  }

  if (query.isLoading || product.isLoading) return <LoadingSkeleton lines={8} />
  if (query.isError || product.isError) return <ErrorState title='Chaine indisponible'
    description='Impossible de charger les etapes enregistrees pour ce produit.'
    onRetry={() => void Promise.all([query.refetch(), product.refetch()])} />
  if (!process || !summary || !product.data) return null

  const addStep = () => {
    const step = newStep(id, steps.length + 1)
    change([...steps, step])
    setSelected(step.id)
  }
  const saveProcess = () => persist(steps, `${steps.length} étape(s) enregistrée(s) dans la chaîne.`, selectedStep?.order)

  return <div className='space-y-6'>
    <PageHeader eyebrow='Espace Responsable Production' title={`Chaine de production  ${product.data.name}`}
      description='Visualisez le flux industriel, identifiez les blocages et consultez les informations de chaque operation.'
      actions={<><Button variant='secondary' onClick={addStep}><Plus className='size-4' />Ajouter une etape</Button><Button loading={save.isPending} onClick={saveProcess}><Save className='size-4' />Sauvegarder la chaine</Button></>} />

    <ProductionRunSummary product={product.data} process={process} summary={summary} />

    <ProductionChainCanvas steps={steps} selectedId={selectedStep?.id ?? null} onSelect={setSelected}
      onReorder={(active, over) => {
        const oldIndex = steps.findIndex(step => step.id === active)
        const newIndex = steps.findIndex(step => step.id === over)
        if (oldIndex >= 0 && newIndex >= 0) change(arrayMove(steps, oldIndex, newIndex))
      }} />

    {selectedStep && <ProductionStepDetailsPanel step={selectedStep} currentOrder={currentOrder} saving={save.isPending}
      onDuplicate={() => duplicateStep(selectedStep.id)} onToggle={() => toggleStep(selectedStep.id)}
      onDelete={() => setDeleting(selectedStep.id)} />}

    <div className='grid items-start gap-5 xl:grid-cols-[minmax(0,1fr)_320px]'>
      {selectedStep && <details className='group surface-card overflow-hidden'>
        <summary className='flex cursor-pointer list-none items-center justify-between gap-3 p-5'>
          <span className='flex items-center gap-3'><span className='rounded-xl bg-blue-50 p-2 text-blue-600'><SlidersHorizontal className='size-5' /></span><span><strong className='block text-slate-900'>Configurer {selectedStep.name}</strong><span className='text-sm text-slate-500'>Machines, durees, quantites et ressources</span></span></span>
          <ChevronDown className='size-5 text-slate-400 transition group-open:rotate-180' />
        </summary>
        <div className='border-t border-slate-100 p-5'><ProductionStepEditor step={selectedStep}
          onUpdate={updated => {
            const previous = steps.find(step => step.id === updated.id)
            const message = previous?.status !== updated.status
              ? `Statut « ${updated.status} » enregistré définitivement.`
              : `Étape « ${updated.name} » enregistrée.`
            persist(steps.map(step => step.id === updated.id ? updated : step), message, updated.order)
          }} /></div>
      </details>}
      <ProcessValidationPanel issues={issues} />
    </div>

    <ConfirmDialog open={Boolean(deleting)} title='Supprimer letape'
      description='letape et ses ressources seront retirees de la chaine lors de la sauvegarde.'
      onClose={() => setDeleting(null)} onConfirm={() => {
        change(steps.filter(step => step.id !== deleting))
        setDeleting(null)
      }} />
  </div>
}
