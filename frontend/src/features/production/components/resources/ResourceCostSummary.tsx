import { CircleDollarSign } from 'lucide-react'
import type { StepResource } from '../../types/production.types'
import { formatCurrency } from '../../utils/productionFormatters'
export function ResourceCostSummary({resources}:{resources:StepResource[]}){const total=resources.reduce((sum,r)=>sum+r.plannedQuantity*r.unitCost,0);return <div className="flex items-center justify-between rounded-xl bg-slate-900 px-4 py-3 text-white"><span className="inline-flex items-center gap-2 text-sm"><CircleDollarSign className="size-4"/>Coût total des ressources</span><strong>{formatCurrency(total)}</strong></div>}
