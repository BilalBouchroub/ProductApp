import { axiosClient, ApiError } from '../../../api/axiosClient'
import { readSession } from '../../../store/authStorage'
import type { AiAttachment, AiConversation, AiConversationSummary, AiStreamEvent, CreateConversationInput, UpdateConversationInput } from '../types/ai.types'

const apiBase = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5220/api'

export const aiApi = {
  async create(input: CreateConversationInput = {}): Promise<AiConversation> { const { data } = await axiosClient.post<AiConversation>('/ai/conversations', input); return data },
  async search(query = '', includeArchived = false): Promise<AiConversationSummary[]> { const { data } = await axiosClient.get<AiConversationSummary[]>('/ai/conversations', { params: { query: query || undefined, includeArchived } }); return data },
  async get(id: string): Promise<AiConversation> { const { data } = await axiosClient.get<AiConversation>(`/ai/conversations/${id}`); return data },
  async update(id: string, input: UpdateConversationInput): Promise<AiConversation> { const { data } = await axiosClient.patch<AiConversation>(`/ai/conversations/${id}`, input); return data },
  async remove(id: string): Promise<void> { await axiosClient.delete(`/ai/conversations/${id}`) },
  async stop(id: string): Promise<void> { await axiosClient.post(`/ai/conversations/${id}/stop`) },
  async upload(id: string, file: File): Promise<AiAttachment> { const body = new FormData(); body.append('file', file); const { data } = await axiosClient.post<AiAttachment>(`/ai/conversations/${id}/attachments`, body, { headers: { 'Content-Type': 'multipart/form-data' }, timeout: 120_000 }); return data },
  async feedback(messageId: string, rating: 'Helpful' | 'NotHelpful'): Promise<void> { await axiosClient.post(`/ai/messages/${messageId}/feedback`, { rating }) },
  async stream(id: string, content: string, attachmentIds: string[], onEvent: (event: AiStreamEvent) => void, signal: AbortSignal): Promise<void> {
    const token = readSession()?.accessToken
    const response = await fetch(`${apiBase}/ai/conversations/${id}/messages`, { method: 'POST', signal,
      headers: { 'Content-Type': 'application/json', Accept: 'text/event-stream', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
      body: JSON.stringify({ content, attachmentIds }) })
    if (!response.ok || !response.body) {
      if (response.status === 401) window.dispatchEvent(new Event('productapp:auth-expired'))
      throw new ApiError(response.status === 429 ? 'Limite SMART PRODUCT atteinte. Patientez un instant.' : 'Impossible de démarrer la réponse SMART PRODUCT.', response.status)
    }
    const reader = response.body.getReader(); const decoder = new TextDecoder(); let buffer = ''
    while (true) {
      const { value, done } = await reader.read(); if (done) break; buffer += decoder.decode(value, { stream: true })
      const frames = buffer.split('\n\n'); buffer = frames.pop() ?? ''
      for (const frame of frames) {
        const eventName = frame.split('\n').find(line => line.startsWith('event:'))?.slice(6).trim()
        const payload = frame.split('\n').filter(line => line.startsWith('data:')).map(line => line.slice(5).trim()).join('\n')
        if (eventName && payload) onEvent({ type: eventName, data: JSON.parse(payload) } as AiStreamEvent)
      }
    }
  },
}
