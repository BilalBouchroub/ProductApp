import { Activity, CheckCircle2, PackageOpen } from 'lucide-react'
import type { ProcessSummary, ProductionProcess, Product } from '../../types/production.types'
import { minutesToLabel } from '../../utils/productionCalculations'
import { formatCurrency } from '../../utils/productionFormatters'
import { getCurrentProductionOrder, getProductionStepVisual } from './productionStepVisuals'

interface ProductionRunSummaryProps {
  product: Product
  process: ProductionProcess
  summary: ProcessSummary
}

const number = new Intl.NumberFormat('fr-FR', { maximumFractionDigits: 1 })

export function ProductionRunSummary({ product, process, summary }: ProductionRunSummaryProps) {
  const ordered = [...process.steps].sort((left, right) => left.order - right.order)
  const lastWithOutput = [...ordered].reverse().find(step => step.actualOutputQuantity > 0)
  const plannedQuantity = Math.max(product.batchQuantity, ...ordered.map(step => step.plannedOutputQuantity), 0)
  const producedQuantity = lastWithOutput?.actualOutputQuantity ?? 0
  const progress = plannedQuantity > 0 ? Math.min(100, Math.round((producedQuantity / plannedQuantity) * 1000) / 10) : 0
  const currentOrder = getCurrentProductionOrder(ordered)
  const visuals = ordered.map(step => getProductionStepVisual(step, currentOrder))
  const hasProblem = visuals.some(visual => visual.state === 'problem')
  const finished = visuals.length > 0 && visuals.every(visual => visual.state === 'completed')
  const status = hasProblem ? 'Attention requise' : finished ? 'Production terminee' : visuals.some(visual => visual.state === 'running') ? 'Production en cours' : 'En attente'
  const accent = product.themeColor ?? '#2563EB'
  const metrics = [
    { label: 'Produit', value: product.name },
    { label: 'Quantite prevue', value: `${number.format(plannedQuantity)} ${product.productionUnit}` },
    { label: 'Quantite produite', value: `${number.format(producedQuantity)} ${product.productionUnit}` },
    { label: 'Duree planifiee', value: minutesToLabel(summary.plannedDurationMinutes) },
    { label: 'Cout planifie', value: formatCurrency(summary.plannedCost) },
  ]

  return <section className='overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm'>
    <div className='flex flex-wrap items-center justify-between gap-4 border-b border-slate-100 px-5 py-4'>
      <div className='flex items-center gap-3'>
        <span className='grid size-10 place-items-center rounded-xl' style={{ color: accent, backgroundColor: `${accent}15` }}><PackageOpen className='size-5' /></span>
        <div><p className='text-xs font-semibold uppercase tracking-wide text-slate-400'>Ordre de production</p><h2 className='font-bold text-slate-950'>Production #{product.internalReference}-V{process.productVersion}</h2></div>
      </div>
      <span className={`inline-flex items-center gap-2 rounded-full px-3 py-1.5 text-sm font-bold ${hasProblem ? 'bg-red-50 text-red-700' : finished ? 'bg-emerald-50 text-emerald-700' : 'bg-amber-50 text-amber-700'}`}>
        {finished ? <CheckCircle2 className='size-4' /> : <Activity className='size-4' />}{status}
      </span>
    </div>
    <div className='grid gap-px bg-slate-100 sm:grid-cols-2 xl:grid-cols-5'>
      {metrics.map(metric => <div key={metric.label} className='bg-white px-5 py-4'><p className='text-xs text-slate-400'>{metric.label}</p><p className='mt-1 truncate font-bold text-slate-800'>{metric.value}</p></div>)}
    </div>
    <div className='px-5 py-4'>
      <div className='flex justify-between text-sm'><span className='font-semibold text-slate-700'>Progression globale</span><strong style={{ color: accent }}>{number.format(progress)}%</strong></div>
      <div className='mt-2 h-2 overflow-hidden rounded-full bg-slate-100'><div className='h-full rounded-full transition-[width] duration-700' style={{ width: `${progress}%`, backgroundColor: accent }} /></div>
    </div>
  </section>
}
