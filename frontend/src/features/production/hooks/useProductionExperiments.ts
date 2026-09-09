import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { productionMockService } from '../services/productionMockService'
import type { ExperimentFormInput } from '../types/production.types'
import { experimentsApi } from '../../../api/experimentsApi'
import { useApiMocks } from '../../../api/apiMode'
export const experimentKeys = { all: ['production', 'experiments'] as const, detail: (id: string) => ['production', 'experiments', id] as const }
export function useProductionExperiments() { return useQuery({ queryKey: experimentKeys.all, queryFn: () => useApiMocks ? productionMockService.getExperiments() : experimentsApi.getAll() }) }
export function useProductionExperiment(id: string) { return useQuery({ queryKey: experimentKeys.detail(id), queryFn: () => useApiMocks ? productionMockService.getExperiment(id) : experimentsApi.get(id), enabled: Boolean(id) }) }
export function useExperimentMutations() { const client = useQueryClient(); const refresh = async () => { await Promise.all([client.invalidateQueries({ queryKey: experimentKeys.all }), client.invalidateQueries({ queryKey: ['production', 'dashboard'] }), client.invalidateQueries({ queryKey: ['commercial', 'products'] }), client.invalidateQueries({ queryKey: ['admin', 'logs'] }), client.invalidateQueries({ queryKey: ['notifications'] })]) }; return { create: useMutation({ mutationFn: (input: ExperimentFormInput) => useApiMocks ? productionMockService.createExperiment(input) : experimentsApi.create(input), onSuccess: refresh }), duplicate: useMutation({ mutationFn: productionMockService.duplicateExperiment, onSuccess: refresh }) } }
