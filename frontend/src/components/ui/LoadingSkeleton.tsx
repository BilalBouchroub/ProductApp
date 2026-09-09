import { cn } from '../../utils/cn'

interface LoadingSkeletonProps { className?: string; lines?: number }
export function LoadingSkeleton({ className, lines = 1 }: LoadingSkeletonProps) {
  return <div role="status" aria-label="Chargement" className={cn('space-y-2', className)}>{Array.from({ length: lines }, (_, index) => <div key={index} className="h-4 animate-pulse rounded-lg bg-slate-200" style={{ width: index === lines - 1 && lines > 1 ? '70%' : '100%' }} />)}</div>
}
