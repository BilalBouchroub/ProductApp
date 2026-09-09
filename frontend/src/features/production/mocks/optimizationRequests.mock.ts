import type { OptimizationRequest } from '../types/production.types'

export const mockOptimizationRequests: OptimizationRequest[] = [
  { id: 'opt-001', productId: 'prd-003', productName: 'Cookie Dattes Sans Sucre', productVersion: 1, commercialManagerName: 'Salma Benali', message: 'Le prix cible dépasse le segment retenu.', priority: 'High', requestedChanges: ['Réduire le coût matière de 8 %', 'Limiter le taux de casse sous 5 %'], createdAt: '2026-07-24T10:20:00Z', status: 'New' },
  { id: 'opt-002', productId: 'prd-002', productName: 'Sablé Avoine & Miel', productVersion: 2, commercialManagerName: 'Omar Alaoui', message: 'Revoir le grammage pour le canal retail.', priority: 'Medium', requestedChanges: ['Tester un format 45 g', 'Conserver la marge cible'], createdAt: '2026-07-19T14:10:00Z', status: 'InProgress' },
  { id: 'opt-003', productId: 'prd-007', productName: 'Biscuit Protéiné Cacao', productVersion: 1, commercialManagerName: 'Salma Benali', message: 'Améliorer la perception gustative avant relance.', priority: 'Urgent', requestedChanges: ['Réduire l’amertume', 'Nouvel essai consommateur'], createdAt: '2026-07-12T09:00:00Z', status: 'New' },
]
