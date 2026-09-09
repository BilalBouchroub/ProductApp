import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from './Button'

interface PaginationProps { page: number; totalPages: number; onPageChange: (page: number) => void }

export function Pagination({ page, totalPages, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null
  return <nav aria-label="Pagination" className="flex items-center justify-between gap-3"><p className="text-sm text-slate-500">Page <strong className="text-slate-700">{page}</strong> sur {totalPages}</p><div className="flex gap-2"><Button variant="secondary" size="icon" disabled={page <= 1} onClick={() => onPageChange(page - 1)} aria-label="Page précédente"><ChevronLeft className="size-4" /></Button><Button variant="secondary" size="icon" disabled={page >= totalPages} onClick={() => onPageChange(page + 1)} aria-label="Page suivante"><ChevronRight className="size-4" /></Button></div></nav>
}
