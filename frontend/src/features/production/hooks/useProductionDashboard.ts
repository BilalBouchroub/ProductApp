import { useQuery } from '@tanstack/react-query'
import { useApiMocks } from '../../../api/apiMode'
import { optimizationRequestsApi } from '../../../api/optimizationRequestsApi'
import type { ProductionDashboardData } from '../types/production.types'
import { productionDashboardService } from '../services/productionDashboardService'
import { productionMockService } from '../services/productionMockService'

async function getDashboard(): Promise<ProductionDashboardData> {
  if (!useApiMocks) return productionDashboardService.getDashboard()
  const dashboard = await productionMockService.getDashboard()
  return { ...dashboard, warnings: [], updatedAt: new Date().toISOString() }
}

export function useProductionDashboard() {
  return useQuery({
    queryKey: ['production', 'dashboard'],
    queryFn: getDashboard,
    staleTime: 30_000,
    retry: 1,
    refetchOnWindowFocus: false,
  })
}
export function useOptimizationRequests() { return useQuery({ queryKey: ['production', 'optimization-requests'], queryFn: () => useApiMocks ? productionMockService.getOptimizationRequests() : optimizationRequestsApi.getAll() }) }
