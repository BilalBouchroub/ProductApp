import { X } from 'lucide-react'
import { useRef, type ReactNode } from 'react'
import { createPortal } from 'react-dom'
import { cn } from '../../utils/cn'
import { useDialogFocus } from '../../hooks/useDialogFocus'

interface DrawerProps { open: boolean; onClose: () => void; title: string; children: ReactNode; side?: 'left' | 'right' }

export function Drawer({ open, onClose, title, children, side = 'right' }: DrawerProps) {
  const panelRef = useRef<HTMLElement>(null)
  useDialogFocus(open, onClose, panelRef)
  if (!open) return null
  return createPortal(<div className="fixed inset-0 z-50 bg-slate-950/45" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose() }}>
    <aside ref={panelRef} tabIndex={-1} role="dialog" aria-modal="true" aria-label={title} className={cn('absolute inset-y-0 w-[min(88vw,22rem)] bg-white shadow-2xl outline-none', side === 'left' ? 'left-0' : 'right-0')}>
      <div className="flex h-16 items-center justify-between border-b border-slate-200 px-5"><h2 className="font-semibold text-slate-950">{title}</h2><button type="button" onClick={onClose} className="rounded-lg p-2 text-slate-500 hover:bg-slate-100" aria-label="Fermer"><X className="size-5" /></button></div>
      <div className="h-[calc(100%-4rem)] overflow-y-auto">{children}</div>
    </aside>
  </div>, document.body)
}
