import { Beaker, Boxes, Clock3, Coins, Layers3, PackageOpen, Recycle, Weight } from 'lucide-react'
import type { CommercialProductView } from '../../types/commercial.types'
import { minutesToLabel } from '../../../production/utils/productionCalculations'
import { formatCurrency } from '../../../production/utils/productionFormatters'

export function ProductTechnicalSummary({ view }: { view: CommercialProductView }) {
  const product = view.product
  const items = [
    ['Coût de revient', formatCurrency(view.productionCost), Coins, 'violet'],
    ['Prix cible', formatCurrency(product.targetSalePrice), PackageOpen, 'emerald'],
    ['Durée totale', minutesToLabel(view.totalDurationMinutes), Clock3, 'blue'],
    ['Étapes', String(view.stepCount), Layers3, 'blue'],
    ['Ressources', String(view.resources.length), Boxes, 'amber'],
    ['Expériences', String(view.experiments.length), Beaker, 'violet'],
    ['Lot planifié', `${product.batchQuantity} ${product.productionUnit}`, Weight, 'blue'],
    ['Taux de perte', `${view.wasteRate}%`, Recycle, 'amber'],
  ] as const
  const tones = { blue: 'bg-blue-50 text-blue-600', emerald: 'bg-emerald-50 text-emerald-600', amber: 'bg-amber-50 text-amber-600', violet: 'bg-violet-50 text-violet-600' }

  return <section className="surface-card p-5 sm:p-6"><div className="flex flex-wrap items-center justify-between gap-3"><div><h2 className="font-bold text-slate-950">Synthèse technique synchronisée</h2><p className="mt-1 text-xs text-slate-500">Données de la version {product.version} transmises par la Production.</p></div><span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">Données réelles</span></div><div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">{items.map(([label, value, Icon, tone]) => <div key={label} className="rounded-2xl border border-slate-100 bg-slate-50/70 p-4"><span className={`grid size-9 place-items-center rounded-xl ${tones[tone]}`}><Icon className="size-4" /></span><p className="mt-3 text-xs text-slate-400">{label}</p><p className="mt-0.5 font-bold text-slate-900">{value}</p></div>)}</div></section>
}
