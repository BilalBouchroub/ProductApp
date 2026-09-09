import { useQuery } from '@tanstack/react-query'
import { useApiMocks } from '../../../api/apiMode'
import { commercialMockService } from '../services/commercialMockService'
import { commercialDashboardService } from '../services/commercialDashboardService'

export function useCommercialDashboard(){
  return useQuery({
    queryKey:['commercial','dashboard'],
    queryFn:()=>useApiMocks?commercialMockService.getDashboard():commercialDashboardService.getDashboard(),
  })
}
