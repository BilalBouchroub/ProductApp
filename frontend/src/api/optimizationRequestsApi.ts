import type { OptimizationRequest } from '../features/production/types/production.types'
import { axiosClient } from './axiosClient'
import type { PagedResponse } from './api.types'

interface OptimizationRequestResponse {
  id: string
  productId: string
  productName: string
  productVersion: number
  message: string
  priority: 'Low' | 'Medium' | 'High' | 'Critical'
  requestedChanges: string[]
  status: OptimizationRequest['status']
  createdAt: string
}

export const optimizationRequestsApi = {
  async getAll(): Promise<OptimizationRequest[]> {
    const { data } = await axiosClient.get<PagedResponse<OptimizationRequestResponse>>(
      '/optimization-requests', { params: { pageSize: 100 } },
    )
    return data.items.map(item => ({
      ...item,
      priority: item.priority === 'Critical' ? 'Urgent' : item.priority,
      commercialManagerName: 'Responsable Commercial',
    }))
  },
}
