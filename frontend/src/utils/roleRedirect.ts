import type { UserRole } from '../types/auth'

export const ROLE_HOME: Record<UserRole, string> = {
  Administrator: '/admin/dashboard',
  ProductionManager: '/production/dashboard',
  CommercialManager: '/commercial/dashboard',
}

export function getRoleHome(role: UserRole): string {
  return ROLE_HOME[role]
}
