import { axiosClient } from './axiosClient'
import type { PagedResponse } from './api.types'
import type { SharedNotification, NotificationType } from '../mocks/shared/sharedMock.types'
import type { UserRole } from '../types/auth'

interface NotificationResponse { id: string; title: string; message: string; type: 'Information' | 'Success' | 'Warning' | 'Error'; isRead: boolean; createdAt: string; relatedEntityType: string | null; relatedEntityId: string | null }
const types: Record<NotificationResponse['type'], NotificationType> = { Information: 'information', Success: 'success', Warning: 'warning', Error: 'error' }
export const notificationApi = {
  async getAll(role: UserRole): Promise<SharedNotification[]> { const { data } = await axiosClient.get<PagedResponse<NotificationResponse>>('/notifications', { params: { pageSize: 100 } }); return data.items.map(item => ({ ...item, recipientRole: role, type: types[item.type] })) },
  async markRead(id: string): Promise<void> { await axiosClient.patch(`/notifications/${id}/read`) },
}
