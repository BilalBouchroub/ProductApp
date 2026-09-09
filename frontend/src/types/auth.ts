export const USER_ROLES = ['Administrator', 'ProductionManager', 'CommercialManager'] as const
export type UserRole = (typeof USER_ROLES)[number]

export interface AuthUser {
  id: string
  name: string
  email: string
  role: UserRole
  initials: string
}

export interface AuthSession {
  user: AuthUser
  authenticatedAt: string
  accessToken: string
  refreshToken: string
  expiresAt: string
}

export interface LoginInput {
  email: string
  password: string
}
