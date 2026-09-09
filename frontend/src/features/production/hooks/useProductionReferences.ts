import { useQuery } from '@tanstack/react-query'
import { useApiMocks } from '../../../api/apiMode'
import { productionReferencesApi } from '../../../api/productionReferencesApi'
import { mockEquipment } from '../mocks/equipment.mock'
import { mockProductCategories } from '../mocks/productCategories.mock'

export function useProductCategories() {
  return useQuery({ queryKey: ['production', 'references', 'categories'],
    queryFn: () => useApiMocks ? Promise.resolve(mockProductCategories) : productionReferencesApi.getCategories() })
}

export function useEquipmentOptions() {
  return useQuery({ queryKey: ['production', 'references', 'equipment'],
    queryFn: () => useApiMocks ? Promise.resolve(mockEquipment) : productionReferencesApi.getEquipment() })
}
