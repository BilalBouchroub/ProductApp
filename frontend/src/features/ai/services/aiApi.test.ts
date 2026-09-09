import { describe, expect, it, vi } from 'vitest'
import { aiApi } from './aiApi'

describe('aiApi', () => {
  it('parse les événements SSE progressifs', async () => {
    const encoder = new TextEncoder()
    const stream = new ReadableStream({ start(controller) { controller.enqueue(encoder.encode('event: response.delta\ndata: {"messageId":"m1","delta":"Bonjour"}\n\n')); controller.enqueue(encoder.encode('event: response.completed\ndata: {"messageId":"m1","content":"Bonjour","inputTokens":2,"outputTokens":1,"sources":[]}\n\n')); controller.close() } })
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(stream, { status: 200 })))
    const events: string[] = []
    await aiApi.stream('c1', 'hello', [], event => events.push(event.type), new AbortController().signal)
    expect(events).toEqual(['response.delta', 'response.completed'])
    vi.unstubAllGlobals()
  })
})
