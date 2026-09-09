import { axiosClient } from './axiosClient'
import type { AuthSession, AuthUser, LoginInput, UserRole } from '../types/auth'

interface AuthUserResponse { id: string; email: string; fullName: string; role: UserRole; permissions: string[]; mustChangePassword: boolean }
interface TokenPairResponse { accessToken: string; refreshToken: string; expiresAt: string; user: AuthUserResponse }
const mapUser = (user: AuthUserResponse): AuthUser => ({ id: user.id, email: user.email, name: user.fullName, role: user.role,
  initials: user.fullName.split(/\s+/).map(part => part[0]).join('').slice(0, 2).toUpperCase() })
export const authApi = {
  async login(input: LoginInput): Promise<AuthSession> { const { data } = await axiosClient.post<TokenPairResponse>('/auth/login', input); return { user: mapUser(data.user), accessToken: data.accessToken, refreshToken: data.refreshToken, expiresAt: data.expiresAt, authenticatedAt: new Date().toISOString() } },
  async logout(refreshToken: string, accessToken: string): Promise<void> { await axiosClient.post('/auth/logout', { refreshToken }, { headers: { Authorization: `Bearer ${accessToken}` } }) },
}
