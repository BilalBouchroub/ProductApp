import {
  Boxes,
  ClipboardCheck,
  Factory,
  Flame,
  Layers3,
  PackageCheck,
  RefreshCw,
  Settings2,
  Snowflake,
  Warehouse,
  type LucideIcon,
} from 'lucide-react'
import type { ProductionStep, ProductionStepIcon } from '../../types/production.types'

export type StepOperationalState = 'completed' | 'running' | 'problem' | 'waiting'

export interface ProductionStepVisual {
  Icon: LucideIcon
  color: string
  background: string
  border: string
  state: StepOperationalState
  stateLabel: string
  statusColor: string
  progress: number
}

const palettes = [
  { color: '#3B82F6', background: '#EFF6FF', border: '#BFDBFE' },
  { color: '#16A34A', background: '#F0FDF4', border: '#BBF7D0' },
  { color: '#D97706', background: '#FFFBEB', border: '#FDE68A' },
  { color: '#E06464', background: '#FEF2F2', border: '#FECACA' },
  { color: '#8B5CF6', background: '#F5F3FF', border: '#DDD6FE' },
  { color: '#0891B2', background: '#ECFEFF', border: '#A5F3FC' },
  { color: '#DB2777', background: '#FDF2F8', border: '#FBCFE8' },
  { color: '#CA8A04', background: '#FEFCE8', border: '#FEF08A' },
  { color: '#1D4ED8', background: '#EEF2FF', border: '#C7D2FE' },
]

const iconComponents: Record<Exclude<ProductionStepIcon, 'Automatic'>, LucideIcon> = {
  Materials: Boxes,
  Preparation: Settings2,
  Mixing: RefreshCw,
  Processing: Factory,
  Heating: Flame,
  Cooling: Snowflake,
  QualityControl: ClipboardCheck,
  Packaging: PackageCheck,
  Storage: Warehouse,
  Palletizing: Layers3,
}

export const productionStepIconOptions: ReadonlyArray<{ value: ProductionStepIcon; label: string }> = [
  { value: 'Automatic', label: 'Automatique (selon le nom)' },
  { value: 'Materials', label: 'Réception des matières' },
  { value: 'Preparation', label: 'Préparation / dosage' },
  { value: 'Mixing', label: 'Mélange / pétrissage' },
  { value: 'Processing', label: 'Transformation / façonnage' },
  { value: 'Heating', label: 'Chauffage / cuisson' },
  { value: 'Cooling', label: 'Refroidissement' },
  { value: 'QualityControl', label: 'Contrôle qualité' },
  { value: 'Packaging', label: 'Conditionnement / emballage' },
  { value: 'Storage', label: 'Stockage' },
  { value: 'Palletizing', label: 'Palettisation' },
]

function normalizedName(name: string) {
  return name.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase()
}

function iconForStep(step: ProductionStep): LucideIcon {
  if (step.icon && step.icon !== 'Automatic') return iconComponents[step.icon]
  const name = normalizedName(step.name)
  if (/matiere|reception|ingredient|raw/.test(name)) return Boxes
  if (/preparation|pesee|dosage/.test(name)) return Settings2
  if (/melange|petrissage|mix/.test(name)) return RefreshCw
  if (/transformation|faconnage|assemblage/.test(name)) return Factory
  if (/cuisson|four|chauff/.test(name)) return Flame
  if (/refroid|froid/.test(name)) return Snowflake
  if (/qualite|controle|test/.test(name)) return ClipboardCheck
  if (/conditionnement|emballage|pack/.test(name)) return PackageCheck
  if (/stock|entrepos/.test(name)) return Warehouse
  if (/palett/.test(name)) return Layers3
  return Factory
}

export function getCurrentProductionOrder(steps: readonly ProductionStep[]): number | null {
  const started = steps.filter(step => step.status === 'Active' && step.actualOutputQuantity > 0)
    .sort((left, right) => right.order - left.order)[0]
  return started?.order ?? steps.find(step => step.status === 'Active')?.order ?? null
}

function operationalState(step: ProductionStep, currentOrder?: number | null): Pick<ProductionStepVisual, 'state' | 'stateLabel' | 'statusColor'> {
  if (step.resources.some(resource => resource.availabilityStatus === 'Unavailable'))
    return { state: 'problem', stateLabel: 'Probleme', statusColor: '#DC2626' }
  if (step.status === 'Validated' || (currentOrder !== null && currentOrder !== undefined && step.status === 'Active' && step.order < currentOrder))
    return { state: 'completed', stateLabel: 'Terminee', statusColor: '#16A34A' }
  if (step.status === 'Active' && (currentOrder === null || currentOrder === undefined || step.order === currentOrder))
    return { state: 'running', stateLabel: 'En cours', statusColor: '#F59E0B' }
  return { state: 'waiting', stateLabel: 'En attente', statusColor: '#94A3B8' }
}

export function getProductionStepVisual(step: ProductionStep, currentOrder?: number | null): ProductionStepVisual {
  const palette = palettes[(Math.max(step.order, 1) - 1) % palettes.length] ?? palettes[0]!
  const state = operationalState(step, currentOrder)
  const progress = state.state === 'completed' ? 100 : step.plannedOutputQuantity > 0
    ? Math.min(100, Math.max(0, Math.round((step.actualOutputQuantity / step.plannedOutputQuantity) * 100)))
    : 0
  return { ...palette, ...state, Icon: iconForStep(step), progress }
}
