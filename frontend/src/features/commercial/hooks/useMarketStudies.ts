import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { commercialMockService } from '../services/commercialMockService'
import type { CommercialOptimizationRequest, MarketStudy, MarketStudyInput } from '../types/commercial.types'
import { marketStudyApi } from '../../../api/marketStudyApi'
import { useApiMocks } from '../../../api/apiMode'
export const studyKeys={all:['commercial','studies'] as const,detail:(id:string)=>['commercial','studies',id] as const}
export function useMarketStudies(){return useQuery({queryKey:studyKeys.all,queryFn:()=>useApiMocks?commercialMockService.getStudies():marketStudyApi.getAll()})}
export function useMarketStudy(id:string){return useQuery({queryKey:studyKeys.detail(id),queryFn:()=>useApiMocks?commercialMockService.getStudy(id):marketStudyApi.get(id),enabled:Boolean(id)})}
export function useMarketStudyMutations(){const client=useQueryClient();const refresh=async(id?:string)=>Promise.all([client.invalidateQueries({queryKey:studyKeys.all}),client.invalidateQueries({queryKey:['commercial','dashboard']}),client.invalidateQueries({queryKey:commercialProductKeys.all}),client.invalidateQueries({queryKey:['admin','logs']}),client.invalidateQueries({queryKey:['notifications']}),...(id?[client.invalidateQueries({queryKey:studyKeys.detail(id)})]:[])]);return{save:useMutation({mutationFn:({input,status,id}:{input:MarketStudyInput;status?:MarketStudy['status'];id?:string})=>useApiMocks?commercialMockService.saveStudy(input,status,id):marketStudyApi.save(input,status,id),onSuccess:s=>refresh(s.id)}),requestOptimization:useMutation({mutationFn:(input:Omit<CommercialOptimizationRequest,'id'|'createdAt'|'status'>)=>useApiMocks?commercialMockService.createOptimizationRequest(input):marketStudyApi.createOptimizationRequest(input),onSuccess:()=>refresh()})}}
import { commercialProductKeys } from './useCommercialProducts'
