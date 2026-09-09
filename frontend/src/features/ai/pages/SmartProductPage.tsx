import { Archive, BarChart3, Bot, BrainCircuit, ChevronLeft, CircleDollarSign, FileBarChart, FlaskConical, Menu, PackageSearch, PanelLeftClose, PanelLeftOpen, SearchCheck, Sparkles } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { toast } from 'sonner'
import { AiMessageContent } from '../components/AiMessageContent'
import { ChatComposer } from '../components/ChatComposer'
import { ConversationSidebar } from '../components/ConversationSidebar'
import { aiApi } from '../services/aiApi'
import type { AiConversationSummary, AiMessage, AiStreamEvent } from '../types/ai.types'

const quickActions = [
  [PackageSearch, 'Analyser un produit', 'Analyse profondément le produit sélectionné : informations clés, chaîne, coûts, temps, ressources, expériences et anomalies.'],
  [FlaskConical, 'Comparer des expériences', 'Compare les expériences pertinentes et distingue les faits, calculs, interprétations et recommandations.'],
  [CircleDollarSign, 'Optimiser les coûts', 'Analyse les coûts par étape et propose un plan réaliste de réduction, chiffré à partir des données disponibles.'],
  [SearchCheck, 'Détecter les anomalies', 'Détecte les écarts et anomalies de coût, durée, consommation, rendement et déchets.'],
  [BarChart3, 'Analyser les ressources', 'Analyse la consommation et la disponibilité des ressources, machines et matières premières.'],
  [FileBarChart, 'Générer un rapport', 'Prépare un rapport ProductApp structuré avec synthèse, KPI, constats, risques, recommandations et sources.'],
] as const

export function SmartProductPage() {
  const { conversationId } = useParams(); const navigate = useNavigate(); const queryClient = useQueryClient(); const [params] = useSearchParams()
  const [search, setSearch] = useState(''); const [showArchived, setShowArchived] = useState(false); const [input, setInput] = useState('')
  const [messages, setMessages] = useState<AiMessage[]>([]); const [files, setFiles] = useState<File[]>([]); const [streaming, setStreaming] = useState(false)
  const [sidebarOpen, setSidebarOpen] = useState(false); const [desktopSidebar, setDesktopSidebar] = useState(true); const abort = useRef<AbortController | null>(null); const bottom = useRef<HTMLDivElement>(null)
  const conversations = useQuery({ queryKey: ['ai-conversations', search, showArchived], queryFn: () => aiApi.search(search, showArchived), staleTime: 10_000 })
  const conversation = useQuery({ queryKey: ['ai-conversation', conversationId], queryFn: () => aiApi.get(conversationId!), enabled: Boolean(conversationId) })
  useEffect(() => { if (conversation.data) setMessages(conversation.data.messages) }, [conversation.data])
  useEffect(() => { bottom.current?.scrollIntoView({ behavior: streaming ? 'auto' : 'smooth' }) }, [messages, streaming])

  const refresh = async (id?: string) => { await queryClient.invalidateQueries({ queryKey: ['ai-conversations'] }); if (id) await queryClient.invalidateQueries({ queryKey: ['ai-conversation', id] }) }
  const create = async () => { const item = await aiApi.create({ productId: params.get('productId'), experimentId: params.get('experimentId') }); await refresh(); navigate(`/ai/${item.id}`); setSidebarOpen(false); return item.id }
  const open = (id: string) => { navigate(`/ai/${id}`); setSidebarOpen(false) }
  const rename = async (item: AiConversationSummary) => { const title = window.prompt('Nouveau titre', item.title)?.trim(); if (!title) return; await aiApi.update(item.id, { title }); await refresh(item.id) }
  const archiveConversation = async (item: AiConversationSummary) => { await aiApi.update(item.id, { isArchived: !item.isArchived }); if (item.id === conversationId) navigate('/ai'); await refresh() }
  const remove = async (item: AiConversationSummary) => { if (!window.confirm(`Supprimer définitivement « ${item.title} » ?`)) return; await aiApi.remove(item.id); if (item.id === conversationId) navigate('/ai'); await refresh() }

  const send = async (prepared?: string) => {
    const content = (prepared ?? input).trim(); if (!content || streaming) return
    let id = conversationId
    try {
      if (!id) id = await create()
      const uploaded = await Promise.all(files.map(file => aiApi.upload(id!, file)))
      const now = new Date().toISOString(); const tempUser = `local-user-${Date.now()}`; const tempAssistant = `local-ai-${Date.now()}`
      setMessages(current => [...current, { id: tempUser, role: 'User', content, status: 'Completed', model: null, inputTokens: null, outputTokens: null, durationMilliseconds: null, errorCode: null, structuredContentJson: null, createdAt: now, sources: [] }, { id: tempAssistant, role: 'Assistant', content: '', status: 'Streaming', model: 'SMART PRODUCT', inputTokens: null, outputTokens: null, durationMilliseconds: null, errorCode: null, structuredContentJson: null, createdAt: now, sources: [] }])
      setInput(''); setFiles([]); setStreaming(true); const controller = new AbortController(); abort.current = controller
      await aiApi.stream(id, content, uploaded.map(item => item.id), event => applyEvent(event, tempUser, tempAssistant), controller.signal)
      await refresh(id)
    } catch (error) {
      if (!(error instanceof DOMException && error.name === 'AbortError')) { toast.error(error instanceof Error ? error.message : 'Réponse SMART PRODUCT impossible.'); setMessages(current => current.map(item => item.status === 'Streaming' ? { ...item, status: 'Failed', errorCode: 'stream_failed' } : item)) }
    } finally { setStreaming(false); abort.current = null }
  }

  const applyEvent = (event: AiStreamEvent, tempUser: string, tempAssistant: string) => {
    if (event.type === 'message.started') setMessages(current => current.map(item => item.id === tempUser ? { ...item, id: event.data.userMessageId } : item.id === tempAssistant ? { ...item, id: event.data.assistantMessageId } : item))
    if (event.type === 'response.delta') setMessages(current => current.map(item => (item.id === tempAssistant || item.id === event.data.messageId) ? { ...item, id: event.data.messageId, content: item.content + event.data.delta } : item))
    if (event.type === 'response.completed') setMessages(current => current.map(item => (item.id === tempAssistant || item.id === event.data.messageId) ? { ...item, id: event.data.messageId, content: event.data.content, status: 'Completed', structuredContentJson: event.data.structuredContentJson ?? null, sources: event.data.sources, inputTokens: event.data.inputTokens, outputTokens: event.data.outputTokens } : item))
    if (event.type === 'response.error') { toast.error(event.data.message); setMessages(current => current.map(item => item.status === 'Streaming' ? { ...item, status: 'Failed', errorCode: event.data.code } : item)) }
  }
  const stop = async () => { abort.current?.abort(); if (conversationId) await aiApi.stop(conversationId).catch(() => undefined); setStreaming(false); setMessages(current => current.map(item => item.status === 'Streaming' ? { ...item, status: 'Cancelled' } : item)) }
  const lastUserPrompt = useMemo(() => [...messages].reverse().find(item => item.role === 'User')?.content ?? '', [messages])

  const sidebar = <ConversationSidebar conversations={conversations.data ?? []} activeId={conversationId} search={search} onSearch={setSearch} onNew={() => void create()} onOpen={open} onRename={item => void rename(item)} onArchive={item => void archiveConversation(item)} onDelete={item => void remove(item)} />
  return <div className="relative flex h-[calc(100vh-4.5rem)] min-h-[620px] overflow-hidden bg-white lg:rounded-2xl lg:border lg:border-slate-200 lg:shadow-xl">
    {desktopSidebar && <div className="hidden w-[290px] shrink-0 lg:block">{sidebar}</div>}
    {sidebarOpen && <div className="fixed inset-0 z-50 lg:hidden"><button className="absolute inset-0 bg-slate-950/50 backdrop-blur-sm" onClick={() => setSidebarOpen(false)} aria-label="Fermer" /><div className="absolute inset-y-0 left-0 w-[min(88vw,310px)]">{sidebar}</div></div>}
    <section className="flex min-w-0 flex-1 flex-col bg-[radial-gradient(circle_at_top,#f5f3ff_0,white_34%)]"><header className="flex h-16 shrink-0 items-center justify-between border-b border-slate-200/80 bg-white/80 px-4 backdrop-blur-xl"><div className="flex min-w-0 items-center gap-2"><button onClick={() => setSidebarOpen(true)} className="grid size-9 place-items-center rounded-xl hover:bg-slate-100 lg:hidden"><Menu className="size-5" /></button><button onClick={() => setDesktopSidebar(value => !value)} className="hidden size-9 place-items-center rounded-xl text-slate-500 hover:bg-slate-100 lg:grid">{desktopSidebar ? <PanelLeftClose className="size-4" /> : <PanelLeftOpen className="size-4" />}</button><div className="min-w-0"><h1 className="truncate text-sm font-bold text-slate-950">{conversation.data?.title ?? 'SMART PRODUCT'}</h1><p className="flex items-center gap-1 text-[10px] font-semibold text-emerald-600"><span className="size-1.5 rounded-full bg-emerald-500" />Données contrôlées par ProductApp</p></div></div><div className="flex items-center gap-2"><button onClick={() => setShowArchived(value => !value)} className={`grid size-9 place-items-center rounded-xl ${showArchived ? 'bg-violet-100 text-violet-700' : 'text-slate-500 hover:bg-slate-100'}`} title="Afficher les archives"><Archive className="size-4" /></button></div></header>
      <div className="min-h-0 flex-1 overflow-y-auto"><div className="mx-auto flex min-h-full max-w-3xl flex-col px-4 py-8 sm:px-6">{messages.length === 0 ? <Welcome onAction={prompt => { setInput(prompt); }} context={Boolean(params.get('productId') || params.get('experimentId'))} /> : <div className="space-y-8">{messages.map(message => message.role === 'User' ? <div key={message.id} className="flex justify-end"><div className="max-w-[85%] rounded-[1.35rem] rounded-br-md bg-slate-950 px-4 py-3 text-[15px] leading-6 whitespace-pre-wrap text-white shadow-sm">{message.content}</div></div> : message.role === 'Assistant' ? <div key={message.id} className="grid grid-cols-[34px_1fr] gap-3"><span className="grid size-8 place-items-center rounded-xl bg-gradient-to-br from-violet-600 to-blue-600 text-white shadow-md"><Sparkles className="size-3.5" /></span><div className="min-w-0 pt-0.5">{message.content ? <AiMessageContent message={message} onRegenerate={lastUserPrompt ? () => void send(lastUserPrompt) : undefined} onFeedback={rating => void aiApi.feedback(message.id, rating).then(() => toast.success('Merci pour votre retour.'))} /> : <Thinking />}{message.status === 'Cancelled' && <p className="mt-2 text-xs italic text-slate-400">Génération arrêtée.</p>}{message.status === 'Failed' && <button onClick={() => void send(lastUserPrompt)} className="mt-3 rounded-lg bg-red-50 px-3 py-2 text-xs font-semibold text-red-700">Relancer la réponse</button>}</div></div> : null)}<div ref={bottom} /></div>}</div></div>
      <ChatComposer value={input} onChange={setInput} onSend={() => void send()} onStop={() => void stop()} streaming={streaming} files={files} onFiles={setFiles} disabled={conversation.isLoading} />
    </section>
  </div>
}

function Welcome({ onAction, context }: { onAction: (prompt: string) => void; context: boolean }) { return <div className="my-auto py-8 text-center"><div className="mx-auto grid size-16 place-items-center rounded-2xl bg-gradient-to-br from-violet-600 via-indigo-600 to-blue-500 text-white shadow-2xl shadow-violet-200"><BrainCircuit className="size-8" /></div><p className="mt-6 text-xs font-bold tracking-[.22em] text-violet-600 uppercase">ProductApp AI</p><h2 className="mt-2 text-3xl font-black tracking-tight text-slate-950 sm:text-4xl">Comment puis-je vous aider<br className="hidden sm:block" /> avec vos produits ?</h2><p className="mx-auto mt-3 max-w-xl text-sm leading-6 text-slate-500">{context ? 'Le contexte métier de cette page est déjà attaché à la conversation.' : 'Analysez vos données réelles, comparez vos expériences et transformez les constats en décisions traçables.'}</p><div className="mt-8 grid gap-3 sm:grid-cols-2">{quickActions.map(([Icon, label, prompt]) => <button key={label} onClick={() => onAction(prompt)} className="group flex items-center gap-3 rounded-2xl border border-slate-200 bg-white p-4 text-left shadow-sm transition hover:-translate-y-0.5 hover:border-violet-300 hover:shadow-lg"><span className="grid size-10 shrink-0 place-items-center rounded-xl bg-slate-50 text-violet-600 transition group-hover:bg-violet-100"><Icon className="size-5" /></span><span><span className="block text-sm font-bold text-slate-900">{label}</span><span className="mt-0.5 block text-xs text-slate-400">Préparer l’analyse</span></span><ChevronLeft className="ml-auto size-4 rotate-180 text-slate-300" /></button>)}</div><div className="mt-7 flex items-center justify-center gap-2 text-[11px] text-slate-400"><Bot className="size-3.5" />Lecture seule · permissions ProductApp · sources internes</div></div> }
function Thinking() { return <div className="flex items-center gap-2 py-2 text-sm text-slate-500"><span className="flex gap-1"><i className="size-1.5 animate-bounce rounded-full bg-violet-500 [animation-delay:-.3s]" /><i className="size-1.5 animate-bounce rounded-full bg-indigo-500 [animation-delay:-.15s]" /><i className="size-1.5 animate-bounce rounded-full bg-blue-500" /></span><span>SMART PRODUCT analyse les données autorisées…</span></div> }
