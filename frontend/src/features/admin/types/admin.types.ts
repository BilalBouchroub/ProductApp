import type { UserRole } from '../../../types/auth'

export const USER_STATUSES = ['Active', 'Inactive', 'Suspended'] as const
export type UserStatus = (typeof USER_STATUSES)[number]

export interface AdminUser {
  id: string
  firstName: string
  lastName: string
  fullName: string
  email: string
  phoneNumber: string
  role: UserRole
  status: UserStatus
  createdAt: string
  updatedAt: string
  lastLoginAt: string | null
  avatarUrl: string | null
}

export interface UserFormInput {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  role: UserRole
  status: UserStatus
  temporaryPassword?: string
}

export const LOG_LEVELS = ['Information', 'Warning', 'Error', 'Critical'] as const
export type LogLevel = (typeof LOG_LEVELS)[number]

export const LOG_MODULES = ['Authentication', 'Users', 'Products', 'Production', 'Experiments', 'Commercial', 'MarketStudies', 'System'] as const
export type LogModule = (typeof LOG_MODULES)[number]

export interface PlatformLog {
  id: string
  timestamp: string
  userId: string
  userName: string
  userRole: UserRole
  action: string
  module: LogModule
  description: string
  level: LogLevel
  ipAddress: string
  entityType: string | null
  entityId: string | null
}

export interface RoleDefinition {
  role: UserRole
  title: string
  description: string
  permissions: readonly string[]
  userCount: number
}

export interface DashboardMetric {
  id: string
  label: string
  value: number
  detail: string
  tone: 'blue' | 'emerald' | 'amber' | 'violet'
}

export interface AdminDashboardData {
  metrics: DashboardMetric[]
  usersByRole: readonly { role: string; count: number }[]
  productsEvolution: readonly { month: string; products: number }[]
  productsByStatus: readonly { status: string; count: number }[]
  recommendations: readonly { type: string; count: number }[]
  platformActivity: readonly { day: string; actions: number; errors: number }[]
  actionsByModule: readonly { module: string; actions: number }[]
  recentLogs: PlatformLog[]
}
