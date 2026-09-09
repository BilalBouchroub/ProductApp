import { axiosClient } from './axiosClient'
import type { EquipmentOption, ProductCategoryOption } from '../features/production/types/production.types'

interface ApiCategory { id: string; code: string; name: string; description: string | null }
interface ApiEquipment { id: string; code: string; name: string; availabilityStatus: EquipmentOption['status']; hourlyCost: number }

export const productionReferencesApi = {
  async getCategories(): Promise<ProductCategoryOption[]> {
    const { data } = await axiosClient.get<ApiCategory[]>('/production-references/product-categories')
    return data
  },
  async getEquipment(): Promise<EquipmentOption[]> {
    const { data } = await axiosClient.get<ApiEquipment[]>('/production-references/equipment')
    return data.map(item => ({ id: item.id, sapCode: item.code, name: item.name,
      category: 'Machine', status: item.availabilityStatus, hourlyCost: item.hourlyCost }))
  },
}
