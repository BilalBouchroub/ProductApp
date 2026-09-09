import type { RoleDefinition } from '../types/admin.types'

export const mockRoles: RoleDefinition[] = [
  { role: 'Administrator', title: 'Administrateur', description: 'Supervise la plateforme, les accès et la traçabilité globale.', userCount: 3, permissions: ['Gérer les utilisateurs', 'Attribuer les rôles', 'Consulter tous les produits', 'Consulter les expériences', 'Consulter les études commerciales', 'Consulter les logs', 'Gérer la configuration de la plateforme'] },
  { role: 'ProductionManager', title: 'Responsable Production', description: 'Pilote les produits, les ressources et les expérimentations industrielles.', userCount: 8, permissions: ['Créer et modifier les produits', 'Configurer la chaîne de production', 'Gérer les ressources', 'Créer des expériences', 'Envoyer un produit vers l’étude commerciale'] },
  { role: 'CommercialManager', title: 'Responsable Commercial / Marketing', description: 'Analyse les opportunités et formule les recommandations commerciales.', userCount: 7, permissions: ['Consulter les produits prêts', 'Créer une étude de marché', 'Analyser les concurrents', 'Calculer la faisabilité', 'Demander une optimisation'] },
]
