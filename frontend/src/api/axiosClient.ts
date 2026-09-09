import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { clearSession, readSession, writeSession } from '../store/authStorage'
import type { AuthSession, UserRole } from '../types/auth'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5220/api'
export const axiosClient = axios.create({ baseURL, headers: { 'Content-Type': 'application/json' }, timeout: 15_000 })

type RetryConfig = InternalAxiosRequestConfig & { _retry?: boolean }
interface RefreshResponse { accessToken: string; refreshToken: string; expiresAt: string; user: { id: string; email: string; fullName: string; role: UserRole } }
let refreshPromise: Promise<AuthSession> | null = null

function refreshSession(): Promise<AuthSession> {
  const current = readSession()
  if (!current) return Promise.reject(new Error('Session expirée.'))
  refreshPromise ??= axios.post<RefreshResponse>(`${baseURL}/auth/refresh`, { refreshToken: current.refreshToken }, { timeout: 10_000 })
    .then(({ data }) => {
      const session: AuthSession = { accessToken: data.accessToken, refreshToken: data.refreshToken, expiresAt: data.expiresAt,
        authenticatedAt: current.authenticatedAt, user: { id: data.user.id, email: data.user.email, name: data.user.fullName,
          role: data.user.role, initials: data.user.fullName.split(/\s+/).map(part => part[0]).join('').slice(0, 2).toUpperCase() } }
      writeSession(session); return session
    }).finally(() => { refreshPromise = null })
  return refreshPromise
}

axiosClient.interceptors.request.use(config => {
  const token = readSession()?.accessToken
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

axiosClient.interceptors.response.use(response => response, async (error: AxiosError) => {
  const config = error.config as RetryConfig | undefined
  const isAuthRoute = config?.url?.includes('/auth/login') || config?.url?.includes('/auth/refresh')
  if (error.response?.status === 401 && config && !config._retry && !isAuthRoute && readSession()?.refreshToken) {
    config._retry = true
    try { const session = await refreshSession(); config.headers.Authorization = `Bearer ${session.accessToken}`; return axiosClient(config) }
    catch { clearSession(); window.dispatchEvent(new Event('productapp:auth-expired')) }
  }
  return Promise.reject(toApiError(error))
})

interface ProblemDetails { title?: string; detail?: string; errors?: Record<string, string[]>; traceId?: string }
export class ApiError extends Error {
  readonly status?: number
  readonly traceId?: string
  readonly errors?: Record<string, string[]>
  constructor(message: string, status?: number, traceId?: string, errors?: Record<string, string[]>) {
    super(message); this.name = 'ApiError'; this.status = status; this.traceId = traceId; this.errors = errors
  }
}
export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error
  if (!axios.isAxiosError<ProblemDetails>(error)) return new ApiError(error instanceof Error ? error.message : 'Une erreur inattendue est survenue.')
  const data = error.response?.data
  const validation = data?.errors ? Object.values(data.errors).flat().join(' ') : undefined
  const defaults: Record<number, string> = { 401: 'Votre session a expiré.', 403: 'Vous ne disposez pas des autorisations nécessaires.', 404: 'La ressource demandée est introuvable.', 409: 'Cette opération entre en conflit avec les données existantes.', 500: 'Le serveur a rencontré une erreur.', 503: 'Le service d’envoi d’e-mails est temporairement indisponible.' }
  return new ApiError(validation || data?.detail || data?.title || (error.response?.status ? defaults[error.response.status] : undefined) || 'Impossible de joindre le serveur.', error.response?.status, data?.traceId, data?.errors)
}
