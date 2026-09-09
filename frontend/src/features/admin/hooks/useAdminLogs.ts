import { useQuery } from '@tanstack/react-query'
import { adminMockService } from '../services/adminMockService'
import { auditApi } from '../../../api/auditApi'
import { useApiMocks } from '../../../api/apiMode'

export function useAdminLogs() {
  return useQuery({ queryKey: ['admin', 'logs'], queryFn: () => useApiMocks ? adminMockService.getLogs() : auditApi.getAll() })
}
