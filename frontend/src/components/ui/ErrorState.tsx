import { AlertTriangle } from 'lucide-react'
import { Button } from './Button'

interface ErrorStateProps { title?: string; description?: string; onRetry?: () => void }
export function ErrorState({ title = 'Une erreur est survenue', description = 'Impossible de charger ces informations.', onRetry }: ErrorStateProps) {
  return <div role="alert" className="flex flex-col items-center px-6 py-12 text-center"><span className="mb-4 rounded-2xl bg-red-50 p-3 text-red-600"><AlertTriangle className="size-6" /></span><h3 className="font-semibold text-slate-900">{title}</h3><p className="mt-1 max-w-sm text-sm text-slate-500">{description}</p>{onRetry && <Button variant="secondary" className="mt-5" onClick={onRetry}>Réessayer</Button>}</div>
}
