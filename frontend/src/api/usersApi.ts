import { axiosClient } from './axiosClient'
import type { PagedResponse, ListParams } from './api.types'
import type { AdminUser, UserFormInput, UserStatus } from '../features/admin/types/admin.types'
import type { UserRole } from '../types/auth'

interface UserResponse { id: string; firstName: string; lastName: string; fullName: string; email: string; phoneNumber: string | null; role: UserRole; status: UserStatus; createdAt: string; updatedAt: string; lastLoginAt: string | null }
type UserAction = 'Activate' | 'Deactivate' | 'Suspend' | 'ChangeRole' | 'Delete' | 'ResetPassword'
interface UserActionResponse { deleted: boolean; user: UserResponse | null }
const mapUser = (user: UserResponse): AdminUser => ({ ...user, phoneNumber: user.phoneNumber ?? '', avatarUrl: null })
async function executeAction(id: string, action: UserAction, role?: UserRole): Promise<UserActionResponse> {
  const { data } = await axiosClient.post<UserActionResponse>(`/users/${id}/actions`, { action, role })
  return data
}
export const usersApi = {
  async getAll(params: ListParams & { role?: UserRole; status?: UserStatus } = {}): Promise<AdminUser[]> {
    const { data } = await axiosClient.get<PagedResponse<UserResponse>>('/users', { params: { pageSize: 100, ...params } }); return data.items.map(mapUser)
  },
  async get(id: string): Promise<AdminUser> { const { data } = await axiosClient.get<UserResponse>(`/users/${id}`); return mapUser(data) },
  async create(input: UserFormInput): Promise<AdminUser> {
    if (!input.temporaryPassword) throw new Error('Le mot de passe temporaire est obligatoire.')
    const { data } = await axiosClient.post<UserResponse>('/users', { firstName: input.firstName, lastName: input.lastName, email: input.email,
      phoneNumber: input.phoneNumber || null, role: input.role, temporaryPassword: input.temporaryPassword })
    if (input.status !== 'Active') return this.setStatus(data.id, input.status)
    return mapUser(data)
  },
  async update(id: string, input: UserFormInput): Promise<AdminUser> {
    await axiosClient.put(`/users/${id}`, { firstName: input.firstName, lastName: input.lastName, phoneNumber: input.phoneNumber || null })
    await executeAction(id, 'ChangeRole', input.role)
    const result = await executeAction(id, input.status === 'Active' ? 'Activate' : input.status === 'Suspended' ? 'Suspend' : 'Deactivate')
    if (!result.user) throw new Error('Le backend n’a pas renvoyé l’utilisateur modifié.')
    return mapUser(result.user)
  },
  async remove(id: string): Promise<void> { await executeAction(id, 'Delete') },
  async setStatus(id: string, status: UserStatus): Promise<AdminUser> {
    const result = await executeAction(id, status === 'Active' ? 'Activate' : status === 'Suspended' ? 'Suspend' : 'Deactivate')
    if (!result.user) throw new Error('Le backend n’a pas renvoyé l’utilisateur modifié.')
    return mapUser(result.user)
  },
  async changeRole(id: string, role: UserRole): Promise<AdminUser> {
    const result = await executeAction(id, 'ChangeRole', role)
    if (!result.user) throw new Error('Le backend n’a pas renvoyé l’utilisateur modifié.')
    return mapUser(result.user)
  },
  async requestPasswordReset(id: string): Promise<void> { await executeAction(id, 'ResetPassword') },
}
