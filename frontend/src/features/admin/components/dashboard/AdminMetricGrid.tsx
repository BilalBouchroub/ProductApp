import { AlertTriangle, Beaker, Boxes, CircleOff, CircleUserRound, FlaskConical, ShieldCheck, ShoppingBag, Sparkles, UserCheck, UserCog, Users, Wrench } from 'lucide-react'
import { StatCard } from '../../../../components/common/StatCard'
import type { DashboardMetric } from '../../types/admin.types'

const icons = {
  'users-total': Users,
  'users-active': UserCheck,
  'users-inactive': CircleUserRound,
  'production-managers': UserCog,
  'commercial-managers': ShoppingBag,
  administrators: ShieldCheck,
  products: Boxes,
  experiments: Beaker,
  studies: FlaskConical,
  viable: Sparkles,
  optimize: Wrench,
  'non-viable': CircleOff,
  errors: AlertTriangle,
} as const

export function AdminMetricGrid({ metrics }: { metrics: readonly DashboardMetric[] }) {
  return <section aria-label="Indicateurs administrateur" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4 2xl:grid-cols-5">{metrics.map((metric) => { const Icon = icons[metric.id as keyof typeof icons] ?? Boxes; return <StatCard key={metric.id} label={metric.label} value={metric.value.toLocaleString('fr-FR')} detail={metric.detail} icon={Icon} accent={metric.tone} /> })}</section>
}
