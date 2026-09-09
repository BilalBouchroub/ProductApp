import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi } from '../api/authApi'
import type { AuthSession } from '../types/auth'
import { AuthContext, type AuthContextValue } from './authContext'
import { clearSession, readSession, writeSession } from './authStorage'
import { useQueryClient } from '@tanstack/react-query'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(readSession)
  const queryClient = useQueryClient()
  useEffect(() => { const expire = () => { queryClient.clear(); setSession(null) }; window.addEventListener('productapp:auth-expired', expire); return () => window.removeEventListener('productapp:auth-expired', expire) }, [queryClient])
  const value = useMemo<AuthContextValue>(() => ({ session, isAuthenticated: session !== null,
    login: async input => { const next = await authApi.login(input); queryClient.clear(); writeSession(next); setSession(next); return next },
    logout: () => { const current = session; clearSession(); queryClient.clear(); setSession(null); if (current) void authApi.logout(current.refreshToken, current.accessToken).catch(() => undefined) },
  }), [queryClient, session])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
