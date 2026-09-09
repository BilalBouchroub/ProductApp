import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { notificationApi } from '../api/notificationApi'
import { useApiMocks } from '../api/apiMode'
import type { UserRole } from '../types/auth'

export function useSharedNotifications(role: UserRole) {
  const client = useQueryClient()
  const key = ['notifications', role] as const
  const query = useQuery({ queryKey: key, queryFn: async () => {
    if (!useApiMocks) return notificationApi.getAll(role)
    const { sharedMockRepository } = await import('../mocks/shared/sharedMock.repository')
    return sharedMockRepository.read().notifications.filter(item => item.recipientRole === role).sort((a, b) => b.createdAt.localeCompare(a.createdAt))
  } })
  const read = useMutation({ mutationFn: async () => {
    const unread = (query.data ?? []).filter(item => !item.isRead)
    if (useApiMocks) {
      const { sharedMockRepository } = await import('../mocks/shared/sharedMock.repository')
      sharedMockRepository.transaction({}, db => { db.notifications = db.notifications.map(item => item.recipientRole === role ? { ...item, isRead: true } : item) })
    } else await Promise.all(unread.map(item => notificationApi.markRead(item.id)))
  }, onSuccess: () => client.invalidateQueries({ queryKey: key }) })
  const notifications = query.data ?? []
  return { notifications, unread: notifications.filter(item => !item.isRead).length, markAllRead: () => read.mutate(), isLoading: query.isLoading }
}
