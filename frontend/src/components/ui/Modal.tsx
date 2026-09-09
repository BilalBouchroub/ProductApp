import { X } from 'lucide-react'
import { useRef, type ReactNode } from 'react'
import { createPortal } from 'react-dom'
import { Button } from './Button'
import { cn } from '../../utils/cn'
import { useDialogFocus } from '../../hooks/useDialogFocus'

interface ModalProps { open: boolean; onClose: () => void; title: string; description?: string; children: ReactNode; footer?: ReactNode; className?: string }

export function Modal({ open, onClose, title, description, children, footer, className }: ModalProps) {
  const panelRef = useRef<HTMLDivElement>(null)
  useDialogFocus(open, onClose, panelRef)
  if (!open) return null
  return createPortal(<div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/50 p-4 backdrop-blur-sm" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose() }}>
    <div ref={panelRef} tabIndex={-1} role="dialog" aria-modal="true" aria-labelledby="modal-title" aria-describedby={description ? 'modal-description' : undefined} className={cn('w-full max-w-lg rounded-2xl bg-white shadow-2xl outline-none', className)}>
      <div className="flex items-start justify-between border-b border-slate-200 px-6 py-5"><div><h2 id="modal-title" className="text-lg font-semibold text-slate-950">{title}</h2>{description && <p id="modal-description" className="mt-1 text-sm text-slate-500">{description}</p>}</div><Button type="button" variant="ghost" size="icon" onClick={onClose} aria-label="Fermer"><X className="size-5" /></Button></div>
      <div className="px-6 py-5">{children}</div>
      {footer && <div className="flex justify-end gap-3 border-t border-slate-200 px-6 py-4">{footer}</div>}
    </div>
  </div>, document.body)
}
