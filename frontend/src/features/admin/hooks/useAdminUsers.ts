import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { UserRole } from '../../../types/auth'
import { adminMockService } from '../services/adminMockService'
import { usersApi } from '../../../api/usersApi'
import { useApiMocks } from '../../../api/apiMode'
import type { AdminUser, UserFormInput, UserStatus } from '../types/admin.types'

export const adminUserKeys = { all: ['admin', 'users'] as const, detail: (id: string) => ['admin', 'users', id] as const }

export function useAdminUsers() { return useQuery({ queryKey: adminUserKeys.all, queryFn: () => useApiMocks ? adminMockService.getUsers() : usersApi.getAll() }) }
export function useAdminUser(id: string) { return useQuery({ queryKey: adminUserKeys.detail(id), queryFn: () => useApiMocks ? adminMockService.getUser(id) : usersApi.get(id), enabled: Boolean(id) }) }

export function useAdminUserMutations() {
  const queryClient = useQueryClient()
  const refresh = async (id?: string) => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: adminUserKeys.all }),
      queryClient.invalidateQueries({ queryKey: ['admin', 'dashboard'] }),
      queryClient.invalidateQueries({ queryKey: ['admin', 'logs'] }),
      ...(id ? [queryClient.invalidateQueries({ queryKey: adminUserKeys.detail(id) })] : []),
    ])
  }
  return {
    createUser: useMutation({
      mutationFn: (input: UserFormInput) => {
        if (!useApiMocks) return usersApi.create(input)
        const safeInput: UserFormInput = {
          firstName: input.firstName,
          lastName: input.lastName,
          email: input.email,
          phoneNumber: input.phoneNumber,
          role: input.role,
          status: input.status,
        }
        return adminMockService.createUser(safeInput)
      },
      onSuccess: (user) => { void refresh(user.id) },
    }),
    updateUser: useMutation({ mutationFn: ({ id, input }: { id: string; input: UserFormInput }) => useApiMocks ? adminMockService.updateUser(id, input) : usersApi.update(id, input), onSuccess: (user) => { void refresh(user.id) } }),
    deleteUser: useMutation({
      mutationFn: (id: string) => useApiMocks ? adminMockService.deleteUser(id) : usersApi.remove(id),
      onSuccess: (_, id) => {
        queryClient.setQueryData<AdminUser[]>(adminUserKeys.all, users => users?.filter(user => user.id !== id))
        queryClient.removeQueries({ queryKey: adminUserKeys.detail(id) })
        void refresh()
      },
    }),
    setStatus: useMutation({
      mutationFn: ({ id, status }: { id: string; status: UserStatus }) => useApiMocks ? adminMockService.setUserStatus(id, status) : usersApi.setStatus(id, status),
      onSuccess: (user) => {
        queryClient.setQueryData<AdminUser[]>(adminUserKeys.all, users => users?.map(item => item.id === user.id ? user : item))
        queryClient.setQueryData(adminUserKeys.detail(user.id), user)
        void refresh(user.id)
      },
    }),
    changeRole: useMutation({
      mutationFn: ({ id, role }: { id: string; role: UserRole }) => useApiMocks ? adminMockService.changeUserRole(id, role) : usersApi.changeRole(id, role),
      onSuccess: (user) => {
        queryClient.setQueryData<AdminUser[]>(adminUserKeys.all, users => users?.map(item => item.id === user.id ? user : item))
        queryClient.setQueryData(adminUserKeys.detail(user.id), user)
        void refresh(user.id)
      },
    }),
    resetPassword: useMutation({
      mutationFn: (id: string) => useApiMocks ? adminMockService.simulatePasswordReset(id) : usersApi.requestPasswordReset(id),
      onSuccess: () => { void queryClient.invalidateQueries({ queryKey: ['admin', 'logs'] }) },
    }),
  }
}
