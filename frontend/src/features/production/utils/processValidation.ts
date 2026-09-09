import type { ProcessValidationIssue, ProductionProcess, ProductionExperiment } from '../types/production.types'

export function validateProcess(process: ProductionProcess): ProcessValidationIssue[] {
  const issues: ProcessValidationIssue[] = []
  if (process.steps.length === 0) issues.push({ id: 'process-empty', stepId: null, field: 'steps', message: 'Ajoutez au moins une étape à la chaîne.', severity: 'error' })
  process.steps.forEach((step) => {
    if (!step.name.trim()) issues.push({ id: `${step.id}-name`, stepId: step.id, field: 'name', message: `L’étape ${step.order} doit avoir un nom.`, severity: 'error' })
    if (step.plannedDurationMinutes <= 0) issues.push({ id: `${step.id}-duration`, stepId: step.id, field: 'plannedDurationMinutes', message: `${step.name || `Étape ${step.order}`} : durée prévue manquante.`, severity: 'error' })
    if (step.resources.length === 0) issues.push({ id: `${step.id}-resources`, stepId: step.id, field: 'resources', message: `${step.name || `Étape ${step.order}`} ne contient aucune ressource.`, severity: 'warning' })
    if (!step.validationCriteria.trim()) issues.push({ id: `${step.id}-criteria`, stepId: step.id, field: 'validationCriteria', message: `${step.name || `Étape ${step.order}`} : critères de validation manquants.`, severity: 'warning' })
  })
  return issues
}

export function validateCommercialReadiness(process: ProductionProcess | undefined, experiments: readonly ProductionExperiment[]): string[] {
  const errors: string[] = []
  if (!process || process.steps.length === 0) errors.push('Le produit ne possède aucune étape de production.')
  if (process && process.steps.some((step) => step.plannedCost <= 0 && step.resources.length === 0)) errors.push('Certains coûts de production ne sont pas renseignés.')
  if (!experiments.some((experiment) => experiment.endDate && experiment.result !== 'Cancelled')) errors.push('Au moins une expérience terminée est requise.')
  return errors
}
