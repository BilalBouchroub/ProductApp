export type AiMessageRole = 'User' | 'Assistant' | 'System' | 'Tool'
export type AiMessageStatus = 'Pending' | 'Streaming' | 'Completed' | 'Cancelled' | 'Failed'
export type AiAttachmentStatus = 'Uploaded' | 'Indexing' | 'Indexed' | 'Failed'
export type AiSourceKind = 'Product' | 'ProductVersion' | 'ProductionStep' | 'Experiment' | 'Resource' | 'MarketStudy' | 'Document' | 'Calculation' | 'UserContext'

export interface AiSource { id: string; kind: AiSourceKind; entityId: string | null; label: string; reference: string | null; internalUrl: string | null; excerpt: string | null }
export interface AiMessage { id: string; role: AiMessageRole; content: string; status: AiMessageStatus; model: string | null; inputTokens: number | null; outputTokens: number | null; durationMilliseconds: number | null; errorCode: string | null; structuredContentJson: string | null; createdAt: string; sources: AiSource[] }
export interface AiAttachment { id: string; fileName: string; mimeType: string; fileSize: number; status: AiAttachmentStatus; errorCode: string | null; createdAt: string }
export interface AiConversationSummary { id: string; title: string; isArchived: boolean; productId: string | null; experimentId: string | null; createdAt: string; updatedAt: string; lastMessageAt: string; preview: string | null }
export interface AiConversation extends Omit<AiConversationSummary, 'lastMessageAt' | 'preview'> { messages: AiMessage[]; attachments: AiAttachment[] }
export type AiStreamEvent =
  | { type: 'message.started'; data: { userMessageId: string; assistantMessageId: string; title: string } }
  | { type: 'response.delta'; data: { messageId: string; delta: string } }
  | { type: 'response.completed'; data: { messageId: string; content: string; structuredContentJson?: string | null; inputTokens: number | null; outputTokens: number | null; sources: AiSource[] } }
  | { type: 'response.error'; data: { messageId?: string; code: string; message: string } }

export interface CreateConversationInput { productId?: string | null; experimentId?: string | null }
export interface UpdateConversationInput { title?: string; isArchived?: boolean }
