export interface PagedResponse<T> { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number }
export interface ListParams { page?: number; pageSize?: number; search?: string; sortBy?: string; descending?: boolean }
