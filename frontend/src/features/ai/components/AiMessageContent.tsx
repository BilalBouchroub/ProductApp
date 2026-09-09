import { Check, Copy, RefreshCw, ThumbsDown, ThumbsUp } from 'lucide-react'
import { useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { AiMessage } from '../types/ai.types'

export function AiMessageContent({ message, onRegenerate, onFeedback }: { message: AiMessage; onRegenerate?: () => void; onFeedback?: (rating: 'Helpful' | 'NotHelpful') => void }) {
  const [copied, setCopied] = useState(false)
  const copy = async () => { await navigator.clipboard.writeText(message.content); setCopied(true); window.setTimeout(() => setCopied(false), 1500) }
  return <article className="group relative"><Markdown content={message.content} />{message.structuredContentJson && <StructuredVisual value={message.structuredContentJson} />}
    {message.sources.length > 0 && <div className="mt-5 border-t border-slate-200/80 pt-4"><p className="mb-2 text-[11px] font-bold tracking-[.14em] text-slate-400 uppercase">Sources ProductApp</p><div className="flex flex-wrap gap-2">{message.sources.map(source => source.internalUrl ? <Link key={source.id} to={source.internalUrl} title={source.excerpt ?? undefined} className="rounded-full border border-blue-200 bg-blue-50 px-3 py-1.5 text-xs font-semibold text-blue-700 transition hover:border-blue-300 hover:bg-blue-100">{source.label}</Link> : <span key={source.id} title={source.excerpt ?? undefined} className="rounded-full border border-slate-200 bg-slate-50 px-3 py-1.5 text-xs font-medium text-slate-600">{source.label}</span>)}</div></div>}
    {message.status !== 'Streaming' && <div className="mt-3 flex items-center gap-1 text-slate-400"><Action title="Copier" onClick={copy}>{copied ? <Check /> : <Copy />}</Action><Action title="Utile" onClick={() => onFeedback?.('Helpful')}><ThumbsUp /></Action><Action title="Pas utile" onClick={() => onFeedback?.('NotHelpful')}><ThumbsDown /></Action>{onRegenerate && <Action title="Régénérer" onClick={onRegenerate}><RefreshCw /></Action>}</div>}
  </article>
}

function Action({ title, onClick, children }: { title: string; onClick: () => void; children: ReactNode }) { return <button type="button" title={title} aria-label={title} onClick={onClick} className="grid size-8 place-items-center rounded-lg transition hover:bg-slate-100 hover:text-slate-700 [&_svg]:size-4">{children}</button> }

function Markdown({ content }: { content: string }) {
  const parts = content.split(/```([\w-]*)\n([\s\S]*?)```/g)
  return <div className="space-y-3 text-[15px] leading-7 text-slate-700">{parts.map((part, index) => index % 3 === 2 ? <pre key={index} className="overflow-x-auto rounded-2xl bg-slate-950 p-4 text-sm leading-6 text-slate-100"><code>{part}</code></pre> : index % 3 === 1 ? null : <TextBlock key={index} value={part} />)}</div>
}

function TextBlock({ value }: { value: string }) {
  const lines = value.split('\n'); const output: ReactNode[] = []
  for (let index = 0; index < lines.length;) {
    const line = (lines[index] ?? '').trimEnd(); if (!line.trim()) { index++; continue }
    if (line.includes('|') && index + 1 < lines.length && /^\s*\|?\s*:?-+/.test(lines[index + 1] ?? '')) {
      const rows: string[][] = []; const header = cells(line); index += 2
      while (index < lines.length && (lines[index] ?? '').includes('|')) rows.push(cells(lines[index++] ?? ''))
      output.push(<div key={`t-${index}`} className="overflow-x-auto rounded-xl border border-slate-200"><table className="w-full min-w-[420px] text-left text-sm"><thead className="bg-slate-50 text-xs uppercase text-slate-500"><tr>{header.map((cell, i) => <th key={i} className="px-3 py-2.5">{inline(cell)}</th>)}</tr></thead><tbody className="divide-y divide-slate-100">{rows.map((row, r) => <tr key={r}>{row.map((cell, c) => <td key={c} className="px-3 py-2.5 align-top">{inline(cell)}</td>)}</tr>)}</tbody></table></div>); continue
    }
    const heading = line.match(/^(#{1,3})\s+(.+)/); if (heading) { const level = (heading[1] ?? '').length; const title = heading[2] ?? ''; output.push(level === 1 ? <h2 key={index} className="pt-2 text-xl font-bold text-slate-950">{inline(title)}</h2> : <h3 key={index} className="pt-2 text-base font-bold text-slate-900">{inline(title)}</h3>); index++; continue }
    if (/^[-*]\s+/.test(line)) { const items: string[] = []; while (index < lines.length && /^\s*[-*]\s+/.test(lines[index] ?? '')) items.push((lines[index++] ?? '').replace(/^\s*[-*]\s+/, '')); output.push(<ul key={`u-${index}`} className="list-disc space-y-1 pl-5">{items.map((item, i) => <li key={i}>{inline(item)}</li>)}</ul>); continue }
    if (/^\d+\.\s+/.test(line)) { const items: string[] = []; while (index < lines.length && /^\s*\d+\.\s+/.test(lines[index] ?? '')) items.push((lines[index++] ?? '').replace(/^\s*\d+\.\s+/, '')); output.push(<ol key={`o-${index}`} className="list-decimal space-y-1 pl-5">{items.map((item, i) => <li key={i}>{inline(item)}</li>)}</ol>); continue }
    if (line.startsWith('> ')) { output.push(<blockquote key={index} className="border-l-4 border-blue-300 bg-blue-50/70 px-4 py-2 text-slate-600">{inline(line.slice(2))}</blockquote>); index++; continue }
    const paragraph: string[] = [line]; index++; while (index < lines.length && (lines[index] ?? '').trim() && !/^(#{1,3}|[-*]\s|\d+\.\s|>\s)/.test((lines[index] ?? '').trim())) paragraph.push((lines[index++] ?? '').trim())
    output.push(<p key={`p-${index}`}>{inline(paragraph.join(' '))}</p>)
  }
  return <>{output}</>
}

function cells(line: string) { return line.trim().replace(/^\||\|$/g, '').split('|').map(cell => cell.trim()) }
function inline(text: string): ReactNode[] { return text.split(/(`[^`]+`|\*\*[^*]+\*\*)/g).filter(Boolean).map((part, index) => part.startsWith('`') ? <code key={index} className="rounded bg-slate-100 px-1.5 py-0.5 text-[.9em] text-violet-700">{part.slice(1, -1)}</code> : part.startsWith('**') ? <strong key={index} className="font-bold text-slate-900">{part.slice(2, -2)}</strong> : part) }

function StructuredVisual({ value }: { value: string }) {
  try { const parsed = JSON.parse(value) as { type?: string; title?: string; data?: { label: string; value: number }[]; unit?: string }; if (parsed.type !== 'chart' || !parsed.data?.length) return null
    return <div className="mt-4 rounded-2xl border border-slate-200 bg-white p-4"><h4 className="mb-3 text-sm font-bold text-slate-900">{parsed.title ?? 'Visualisation'}</h4><div className="h-64"><ResponsiveContainer width="100%" height="100%"><BarChart data={parsed.data}><CartesianGrid strokeDasharray="3 3" vertical={false} /><XAxis dataKey="label" tick={{ fontSize: 11 }} /><YAxis tick={{ fontSize: 11 }} /><Tooltip formatter={(v) => `${String(v)} ${parsed.unit ?? ''}`} /><Bar dataKey="value" fill="#6366f1" radius={[6, 6, 0, 0]} /></BarChart></ResponsiveContainer></div></div>
  } catch { return null }
}
