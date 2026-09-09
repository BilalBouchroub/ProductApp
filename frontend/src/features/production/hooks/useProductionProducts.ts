import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { productionMockService } from '../services/productionMockService'
import type { ProductFormInput } from '../types/production.types'
import { productsApi } from '../../../api/productsApi'
import { useApiMocks } from '../../../api/apiMode'

export const productKeys = { all: ['production', 'products'] as const, detail: (id: string) => ['production', 'products', id] as const }
export function useProductionProducts() { return useQuery({ queryKey: productKeys.all, queryFn: () => useApiMocks ? productionMockService.getProducts() : productsApi.getAll() }) }
export function useProductionProduct(id: string) { return useQuery({ queryKey: productKeys.detail(id), queryFn: () => useApiMocks ? productionMockService.getProduct(id) : productsApi.get(id), enabled: Boolean(id) }) }
export function useProductMutations() { const client = useQueryClient(); const refresh = async (id?: string) => { await Promise.all([client.invalidateQueries({ queryKey: productKeys.all }), client.invalidateQueries({ queryKey: ['production', 'dashboard'] }), client.invalidateQueries({ queryKey: ['commercial', 'products'] }), client.invalidateQueries({ queryKey: ['admin', 'logs'] }), client.invalidateQueries({ queryKey: ['notifications'] }), ...(id ? [client.invalidateQueries({ queryKey: productKeys.detail(id) }), client.invalidateQueries({ queryKey: ['commercial', 'products', id] })] : [])]) }; return {
  create: useMutation({ mutationFn: (input: ProductFormInput) => useApiMocks ? productionMockService.createProduct(input) : productsApi.create(input), onSuccess: (p) => refresh(p.id) }),
  update: useMutation({ mutationFn: ({ id, input }: { id: string; input: ProductFormInput }) => useApiMocks ? productionMockService.updateProduct(id, input) : productsApi.update(id, input), onSuccess: (p) => refresh(p.id) }),
  remove: useMutation({ mutationFn: (id: string) => useApiMocks ? productionMockService.deleteDraft(id) : productsApi.remove(id), onSuccess: () => { void refresh() } }),
  duplicate: useMutation({ mutationFn: (id: string) => useApiMocks ? productionMockService.duplicateProduct(id) : productsApi.duplicate(id), onSuccess: (p) => { void refresh(p.id) } }),
  version: useMutation({ mutationFn: (id: string) => useApiMocks ? productionMockService.createVersion(id) : productsApi.createVersion(id), onSuccess: (p) => refresh(p.id) }),
  archive: useMutation({ mutationFn: (id: string) => useApiMocks ? productionMockService.archiveProduct(id) : productsApi.archive(id), onSuccess: (p) => refresh(p.id) }),
  send: useMutation({ mutationFn: (id: string) => useApiMocks ? productionMockService.sendToCommercial(id) : productsApi.sendToCommercial(id), onSuccess: (p) => { void refresh(p.id) } }),
} }
