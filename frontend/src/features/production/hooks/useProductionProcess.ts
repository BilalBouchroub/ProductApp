import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { productionMockService } from '../services/productionMockService'
import type { ProductionProcess } from '../types/production.types'
import { productionStepsApi } from '../../../api/productionStepsApi'
import { useApiMocks } from '../../../api/apiMode'
export const processKey = (productId: string) => ['production', 'process', productId] as const
export function useProductionProcess(productId: string) { return useQuery({ queryKey: processKey(productId), queryFn: async () => { if (useApiMocks) return productionMockService.getProcess(productId); const apiProcess = await productionStepsApi.getProcess(productId); if (apiProcess.steps.length > 0) return apiProcess; const legacyProcess = await productionMockService.getProcess(productId); return legacyProcess.steps.length > 0 ? { ...apiProcess, steps: legacyProcess.steps, savedAt: null } : apiProcess }, enabled: Boolean(productId) }) }
export function useSaveProductionProcess() { const client = useQueryClient(); return useMutation({ mutationFn: (process: ProductionProcess) => useApiMocks ? productionMockService.saveProcess(process) : productionStepsApi.saveProcess(process), onSuccess: async (process) => { await Promise.all([client.invalidateQueries({ queryKey: processKey(process.productId) }), client.invalidateQueries({ queryKey: ['production', 'products'] }), client.invalidateQueries({ queryKey: ['production', 'dashboard'] })]) } }) }
