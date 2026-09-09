import type { ProductionProcess, ProductionStep, StepResource } from '../types/production.types'

function resource(id: string, stepId: string, designation: string, sapCode: string, quantity: number, unitCost: number, stock: number, type: StepResource['resourceType'] = 'RawMaterial'): StepResource {
  const availabilityStatus = stock <= 0 ? 'Unavailable' : stock < quantity ? 'ToOrder' : stock <= quantity * 1.2 ? 'LowStock' : 'Available'
  return { id, stepId, sapCode, designation, resourceType: type, plannedQuantity: quantity, actualQuantity: quantity * 0.98, unit: type === 'Energy' ? 'kWh' : type === 'Packaging' ? 'm' : 'kg', unitCost, totalCost: Math.round(quantity * unitCost * 100) / 100, availableStock: stock, supplier: type === 'Packaging' ? 'Pack Maroc' : 'Fournisseur SAP mock', batchNumber: `LOT-${id.toUpperCase()}`, expirationDate: type === 'RawMaterial' ? '2027-03-30' : null, availabilityStatus }
}

function step(productId: string, order: number, name: string, duration: number, cost: number, equipment: string[], resources: StepResource[], extra: Partial<ProductionStep> = {}): ProductionStep {
  const id = `stp-${productId}-${order}`
  extra = { icon: 'Automatic', ...extra }
  return { id, productId, name, description: `Opération de ${name.toLowerCase()} contrôlée selon la gamme biscuit.`, order, plannedDurationMinutes: duration, actualDurationMinutes: Math.round(duration * 1.05), plannedCost: cost, actualCost: Math.round(cost * 1.04 * 100) / 100, temperature: null, pressure: null, humidity: null, plannedOutputQuantity: 1000 - (order - 1) * 8, actualOutputQuantity: 992 - (order - 1) * 9, wasteQuantity: 8, equipment, operatorCount: order === 1 ? 3 : 2, laborCost: duration * 2.3, energyConsumption: duration * 1.8, instructions: `Respecter la fiche opératoire de ${name.toLowerCase()} et enregistrer les contrôles.`, validationCriteria: 'Paramètres conformes, traçabilité lot complétée et sortie dans la tolérance.', observations: '', status: 'Active', resources: resources.map((item) => ({ ...item, stepId: id })), ...extra }
}

const cacaoSteps: ProductionStep[] = [
  step('prd-001', 1, 'Réception des matières', 35, 180, ['Balance industrielle connectée'], [resource('r-001', '', 'Farine de blé T55', 'SAP-RM-FLR-001', 520, 4.2, 4850), resource('r-002', '', 'Poudre de cacao 22/24', 'SAP-RM-COC-004', 62, 72, 58)]),
  step('prd-001', 2, 'Pesée', 45, 240, ['Balance industrielle connectée'], [resource('r-003', '', 'Main-d’œuvre pesée', 'LAB-WGH-01', 3, 95, 12, 'Labor')]),
  step('prd-001', 3, 'Mélange', 28, 420, ['Mélangeur horizontal 500 L'], [resource('r-004', '', 'Énergie électrique', 'ENE-ELEC', 58, 1.35, 9000, 'Energy')], { humidity: 46 }),
  step('prd-001', 4, 'Pétrissage', 24, 510, ['Pétrin industriel spirale'], [resource('r-005', '', 'Beurre pâtissier', 'SAP-RM-BUT-003', 125, 48, 110)], { temperature: 24 }),
  step('prd-001', 5, 'Façonnage', 40, 680, ['Façonneuse rotative'], [resource('r-006', '', 'Agent de démoulage', 'CON-DEM-01', 8, 22, 40, 'Consumable')]),
  step('prd-001', 6, 'Cuisson', 18, 1450, ['Four tunnel ligne 02'], [resource('r-007', '', 'Gaz naturel', 'ENE-GAZ', 185, 0.92, 3200, 'Energy')], { temperature: 185, humidity: 18 }),
  step('prd-001', 7, 'Refroidissement', 32, 380, ['Convoyeur de refroidissement'], [resource('r-008', '', 'Énergie convoyeur', 'ENE-ELEC', 36, 1.35, 9000, 'Energy')], { temperature: 28 }),
  step('prd-001', 8, 'Emballage', 55, 920, ['Ensacheuse horizontale'], [resource('r-009', '', 'Film OPP imprimé', 'SAP-PK-FIL-011', 2100, 0.42, 2300, 'Packaging')]),
]

const oatSteps = cacaoSteps.map((item, index) => ({ ...item, id: `stp-prd-002-${index + 1}`, productId: 'prd-002', resources: item.resources.map((res) => ({ ...res, id: `${res.id}-oat`, stepId: `stp-prd-002-${index + 1}` })), plannedOutputQuantity: 800 - index * 6, actualOutputQuantity: 794 - index * 7 }))
const datesSteps = cacaoSteps.slice(0, 6).map((item, index) => ({ ...item, id: `stp-prd-003-${index + 1}`, productId: 'prd-003', resources: item.resources.map((res) => ({ ...res, id: `${res.id}-dat`, stepId: `stp-prd-003-${index + 1}` })), status: index > 3 ? 'Draft' as const : 'Active' as const }))

export const mockProcesses: ProductionProcess[] = [
  { id: 'proc-001', productId: 'prd-001', productVersion: 3, version: 5, steps: cacaoSteps, updatedAt: '2026-07-30T08:40:00Z', savedAt: '2026-07-30T08:40:00Z' },
  { id: 'proc-002', productId: 'prd-002', productVersion: 2, version: 3, steps: oatSteps, updatedAt: '2026-07-29T15:25:00Z', savedAt: '2026-07-29T15:25:00Z' },
  { id: 'proc-003', productId: 'prd-003', productVersion: 1, version: 1, steps: datesSteps, updatedAt: '2026-07-28T11:10:00Z', savedAt: null },
  { id: 'proc-004', productId: 'prd-004', productVersion: 1, version: 1, steps: [], updatedAt: '2026-07-27T14:05:00Z', savedAt: null },
]
