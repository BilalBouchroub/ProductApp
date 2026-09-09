import {
  ChevronDown,
  Clock3,
  Coins,
  Factory,
  Gauge,
  PackageOpen,
  Thermometer,
  Users,
  Zap,
} from 'lucide-react'
import { ResourceAvailabilityBadge } from '../../../production/components/resources/ResourceAvailabilityBadge'
import { getProductionStepVisual } from '../../../production/components/process/productionStepVisuals'
import type { ProductionProcess, ProductionStep } from '../../../production/types/production.types'
import { formatCurrency } from '../../../production/utils/productionFormatters'

const statusLabels: Record<ProductionStep['status'], string> = {
  Draft: 'Brouillon',
  Active: 'Active',
  Inactive: 'Inactive',
  Validated: 'Validée',
}

function Value({ label, value, icon: Icon }: { label: string; value: string; icon: typeof Clock3 }) {
  return <div className="rounded-xl bg-slate-50 p-3"><Icon className="size-4 text-slate-400" /><p className="mt-2 text-[11px] font-medium uppercase tracking-wide text-slate-400">{label}</p><p className="mt-0.5 text-sm font-bold text-slate-800">{value}</p></div>
}

function TextBlock({ label, value }: { label: string; value: string }) {
  return <div><p className="text-xs font-semibold uppercase tracking-wide text-slate-400">{label}</p><p className="mt-1 whitespace-pre-wrap text-sm leading-6 text-slate-600">{value || 'Non renseigné'}</p></div>
}

export function ProductionChainReadOnly({ process }: { process: ProductionProcess }) {
  if (!process.steps.length) return <div className="rounded-2xl border border-dashed border-amber-300 bg-amber-50 p-8 text-center"><Factory className="mx-auto size-7 text-amber-500" /><p className="mt-3 font-semibold text-amber-900">Aucune étape disponible pour cette version.</p><p className="mt-1 text-sm text-amber-700">La Production doit enregistrer puis transmettre la chaîne de cette version.</p></div>

  return <div className="relative space-y-4 before:absolute before:bottom-8 before:left-[1.35rem] before:top-8 before:w-px before:bg-slate-200">
    {process.steps.map((step, index) => {
      const visual = getProductionStepVisual(step)
      const StepIcon = visual.Icon
      return <article key={step.id} className="relative overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm transition duration-300 hover:border-blue-200 hover:shadow-md">
        <div className="flex flex-col gap-4 p-5 sm:flex-row sm:items-start">
          <span className="relative z-10 grid size-11 shrink-0 place-items-center rounded-2xl" style={{ color: visual.color, backgroundColor: visual.background, border: `1px solid ${visual.border}` }}><StepIcon className="size-5" /></span>
          <div className="min-w-0 flex-1"><div className="flex flex-wrap items-center justify-between gap-3"><div><p className="text-xs font-semibold uppercase tracking-wider text-blue-600">Étape {step.order} sur {process.steps.length}</p><h3 className="mt-1 text-lg font-bold text-slate-950">{step.name}</h3></div><span className="rounded-full px-3 py-1 text-xs font-semibold" style={{ color: visual.color, backgroundColor: visual.background }}>{statusLabels[step.status]}</span></div>
            {step.description && <p className="mt-2 text-sm leading-6 text-slate-600">{step.description}</p>}
            <div className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-4"><Value icon={Clock3} label="Durée prévue / réelle" value={`${step.plannedDurationMinutes} / ${step.actualDurationMinutes || '—'} min`} /><Value icon={Coins} label="Coût prévu / réel" value={`${formatCurrency(step.plannedCost)} / ${step.actualCost ? formatCurrency(step.actualCost) : '—'}`} /><Value icon={PackageOpen} label="Sortie prévue / réelle" value={`${step.plannedOutputQuantity || '—'} / ${step.actualOutputQuantity || '—'}`} /><Value icon={Factory} label="Ressources" value={String(step.resources.length)} /></div>

            <details className="group mt-4 rounded-xl border border-slate-200 bg-slate-50/60" open={index === 0}><summary className="flex cursor-pointer list-none items-center justify-between px-4 py-3 text-sm font-semibold text-slate-700">Tous les paramètres de l’étape<ChevronDown className="size-4 transition group-open:rotate-180" /></summary><div className="border-t border-slate-200 p-4">
              <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3"><Value icon={Thermometer} label="Température" value={step.temperature === null ? 'Non renseignée' : `${step.temperature} °C`} /><Value icon={Gauge} label="Pression / humidité" value={`${step.pressure ?? '—'} / ${step.humidity ?? '—'} %`} /><Value icon={Users} label="Opérateurs" value={String(step.operatorCount)} /><Value icon={Coins} label="Main-d’œuvre" value={formatCurrency(step.laborCost)} /><Value icon={Zap} label="Énergie" value={`${step.energyConsumption} kWh`} /><Value icon={Factory} label="Équipement" value={step.equipment.join(', ') || 'Non renseigné'} /></div>
              <div className="mt-5 grid gap-5 lg:grid-cols-3"><TextBlock label="Instructions" value={step.instructions} /><TextBlock label="Critères de validation" value={step.validationCriteria} /><TextBlock label="Observations" value={step.observations} /></div>
              <div className="mt-5"><p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Ressources associées</p>{step.resources.length > 0 ? <div className="mt-2 grid gap-2 md:grid-cols-2">{step.resources.map(resource => <div key={resource.id} className="flex items-center justify-between gap-3 rounded-xl border border-slate-200 bg-white p-3"><div className="min-w-0"><p className="truncate text-sm font-semibold text-slate-800">{resource.designation}</p><p className="mt-0.5 text-xs text-slate-500">{resource.plannedQuantity} {resource.unit} · {formatCurrency(resource.totalCost)}</p></div><ResourceAvailabilityBadge status={resource.availabilityStatus} /></div>)}</div> : <p className="mt-2 text-sm text-slate-500">Aucune ressource associée.</p>}</div>
            </div></details>
          </div>
        </div>
      </article>
    })}
  </div>
}
