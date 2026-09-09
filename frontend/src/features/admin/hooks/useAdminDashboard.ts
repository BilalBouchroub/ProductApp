import { useQuery } from '@tanstack/react-query'
import { adminMockService } from '../services/adminMockService'

export function useAdminDashboard() {
  return useQuery({ queryKey: ['admin', 'dashboard'], queryFn: adminMockService.getDashboard })
}
