import { Button } from './Button'
import { Modal } from './Modal'

interface ConfirmDialogProps { open: boolean; onClose: () => void; onConfirm: () => void; title: string; description: string; confirmLabel?: string; loading?: boolean }

export function ConfirmDialog({ open, onClose, onConfirm, title, description, confirmLabel = 'Confirmer', loading }: ConfirmDialogProps) {
  return <Modal open={open} onClose={onClose} title={title} footer={<><Button variant="secondary" onClick={onClose}>Annuler</Button><Button variant="danger" loading={loading} onClick={onConfirm}>{confirmLabel}</Button></>}><p className="text-sm leading-6 text-slate-600">{description}</p></Modal>
}
