import { axiosClient } from './axiosClient'
import type { PagedResponse, ListParams } from './api.types'
import { PRODUCTION_UNITS, type Product, type ProductFormInput, type ProductStatus } from '../features/production/types/production.types'

interface ProductResponse { id: string; code: string; name: string; description: string | null; versionNumber: number; status: 'Active' | 'Archived'; themeColor: string;
  versionStatus: ProductStatus; sapCode: string | null; imageUrl: string | null; targetSalePrice: number | null; batchQuantity: number | null;
  productionUnit: string | null; productCategoryId: string | null; categoryName: string | null; createdAtUtc: string; updatedAtUtc: string }
export interface ProductVersionResponse { id: string; productId: string; versionNumber: number; name: string; description: string | null; status: ProductStatus; changeSummary: string | null; createdAt: string; updatedAt: string }
const productionUnit = (value: string | null): Product['productionUnit'] =>
  PRODUCTION_UNITS.find(unit => unit === value) ?? 'unité'
const baseProduct = (item: ProductResponse): Product => ({ id: item.id, name: item.name, internalReference: item.code,
  sapCode: item.sapCode ?? item.code, category: item.categoryName ?? 'Non renseignée', description: item.description ?? '', imageUrl: item.imageUrl, themeColor: item.themeColor ?? '#2563EB',
  targetSalePrice: item.targetSalePrice ?? 0, batchQuantity: item.batchQuantity ?? 0,
  productionUnit: productionUnit(item.productionUnit), status: item.status === 'Archived' ? 'Archived' : item.versionStatus, version: item.versionNumber,
  createdAt: item.createdAtUtc, updatedAt: item.updatedAtUtc, productionManagerName: 'ProductApp API' })

export const productsApi = {
  async uploadImage(file: File): Promise<string> {
    const formData = new FormData()
    formData.append('file', file)
    const { data } = await axiosClient.post<{ url: string }>('/products/images', formData,
      { headers: { 'Content-Type': 'multipart/form-data' } })
    return data.url
  },
  async getVersions(productId: string): Promise<ProductVersionResponse[]> { const { data } = await axiosClient.get<ProductVersionResponse[]>(`/products/${productId}/versions`); return data },
  async getAll(params: ListParams & { status?: string } = {}): Promise<Product[]> {
    const pageSize = params.pageSize ?? 100
    const { data: firstPage } = await axiosClient.get<PagedResponse<ProductResponse>>('/products', {
      params: { includeArchived: true, ...params, page: params.page ?? 1, pageSize },
    })
    if (params.page || firstPage.totalPages <= 1) return firstPage.items.map(baseProduct)
    const remainingPages = await Promise.all(
      Array.from({ length: firstPage.totalPages - 1 }, (_, index) =>
        axiosClient.get<PagedResponse<ProductResponse>>('/products', {
          params: { includeArchived: true, ...params, page: index + 2, pageSize },
        })),
    )
    return [firstPage, ...remainingPages.map(response => response.data)]
      .flatMap(page => page.items)
      .map(baseProduct)
  },
  async get(id: string): Promise<Product> { const { data } = await axiosClient.get<ProductResponse>(`/products/${id}`); return baseProduct(data) },
  async create(input: ProductFormInput): Promise<Product> { const { data } = await axiosClient.post<ProductResponse>('/products', { code: input.internalReference,
    name: input.name, description: input.description, sapCode: input.sapCode, imageUrl: input.imageUrl, themeColor: input.themeColor, targetSalePrice: input.targetSalePrice,
    batchQuantity: input.batchQuantity, productionUnit: input.productionUnit, categoryCode: input.category }); return baseProduct(data) },
  async update(id: string, input: ProductFormInput): Promise<Product> { const { data } = await axiosClient.put<ProductResponse>(`/products/${id}`, {
    name: input.name, description: input.description, sapCode: input.sapCode, imageUrl: input.imageUrl, themeColor: input.themeColor, targetSalePrice: input.targetSalePrice,
    batchQuantity: input.batchQuantity, productionUnit: input.productionUnit, categoryCode: input.category }); return baseProduct(data) },
  async remove(id: string): Promise<void> { await axiosClient.delete(`/products/${id}`) },
  async duplicate(id: string): Promise<Product> { const source = await this.get(id); const { data } = await axiosClient.post<ProductResponse>(`/products/${id}/duplicate`, { code: `${source.internalReference}-COPY-${Date.now().toString().slice(-5)}` }); return baseProduct(data) },
  async createVersion(id: string): Promise<Product> { await axiosClient.post(`/products/${id}/versions`, { changeSummary: 'Nouvelle version créée depuis le frontend' }); return this.get(id) },
  async archive(id: string): Promise<Product> { await axiosClient.post(`/products/${id}/archive`); return this.get(id) },
  async sendToCommercial(id: string): Promise<Product> { const product = await this.get(id); await axiosClient.post(`/products/${id}/versions/${product.version}/publish`); return this.get(id) },
}
