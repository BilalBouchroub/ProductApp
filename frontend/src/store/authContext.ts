import { createContext } from 'react'
import type { AuthSession, LoginInput } from '../types/auth'

export interface AuthContextValue {
  session: AuthSession | null
  isAuthenticated: boolean
  login: (input: LoginInput) => Promise<AuthSession>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)
