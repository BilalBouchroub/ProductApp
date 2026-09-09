import type { AuthSession } from '../types/auth'

const STORAGE_KEY = 'productapp.auth-session'

export function readSession(): AuthSession | null {
  const value = sessionStorage.getItem(STORAGE_KEY)
  if (!value) return null
  try { const session = JSON.parse(value) as AuthSession; if (!session.accessToken || !session.refreshToken) { sessionStorage.removeItem(STORAGE_KEY); return null } return session } catch { sessionStorage.removeItem(STORAGE_KEY); return null }
}

export function writeSession(session: AuthSession): void { sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session)) }
export function clearSession(): void { sessionStorage.removeItem(STORAGE_KEY) }
