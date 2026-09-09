import { useQuery } from '@tanstack/react-query'
import { commercialMockService } from '../services/commercialMockService'
import { commercialProductsApi } from '../../../api/commercialProductsApi'
import { useApiMocks } from '../../../api/apiMode'
export const commercialProductKeys={all:['commercial','products'] as const,detail:(id:string)=>['commercial','products',id] as const}
export function useCommercialProducts(){return useQuery({queryKey:commercialProductKeys.all,queryFn:()=>useApiMocks?commercialMockService.getProducts():commercialProductsApi.getAll()})}
export function useCommercialProduct(id:string){return useQuery({queryKey:commercialProductKeys.detail(id),queryFn:()=>useApiMocks?commercialMockService.getProduct(id):commercialProductsApi.get(id),enabled:Boolean(id)})}
