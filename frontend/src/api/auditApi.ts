import { axiosClient } from './axiosClient'
import type { PagedResponse, ListParams } from './api.types'
import type { LogLevel, LogModule, PlatformLog } from '../features/admin/types/admin.types'
import type { UserRole } from '../types/auth'

interface AuditResponse { id: string; timestamp: string; userId: string | null; userName: string | null; action: string; module: string; description: string; level: LogLevel; ipAddress: string | null; entityType: string | null; entityId: string | null }
export const auditApi = {
  async getAll(params: ListParams & { module?: string; level?: LogLevel; userId?: string; from?: string; to?: string } = {}): Promise<PlatformLog[]> {
    const { data } = await axiosClient.get<PagedResponse<AuditResponse>>('/audit-logs', { params: { pageSize: 100, ...params } })
    return data.items.map(log => ({ ...log, userId: log.userId ?? '', userName: log.userName ?? 'Système', userRole: 'Administrator' as UserRole,
      module: log.module as LogModule, ipAddress: log.ipAddress ?? '', entityType: log.entityType, entityId: log.entityId }))
  },
}
