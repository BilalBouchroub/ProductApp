import { flexRender, getCoreRowModel, getPaginationRowModel, getSortedRowModel, useReactTable, type ColumnDef, type PaginationState, type SortingState } from '@tanstack/react-table'
import { ArrowDown, ArrowUp, ArrowUpDown } from 'lucide-react'
import { useEffect, useState } from 'react'
import { EmptyState } from '../ui/EmptyState'
import { ErrorState } from '../ui/ErrorState'
import { LoadingSkeleton } from '../ui/LoadingSkeleton'
import { Pagination } from '../ui/Pagination'

interface DataTableProps<TData> { data: readonly TData[]; columns: ColumnDef<TData>[]; loading?: boolean; error?: string; emptyTitle?: string; onRetry?: () => void; pageSize?: number }

export function DataTable<TData>({ data, columns, loading = false, error, emptyTitle, onRetry, pageSize }: DataTableProps<TData>) {
  const [sorting, setSorting] = useState<SortingState>([])
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: pageSize ?? 10 })
  useEffect(() => {
    if (!pageSize) return
    setPagination((current) => current.pageIndex === 0 && current.pageSize === pageSize ? current : { pageIndex: 0, pageSize })
  }, [data, pageSize])
  const table = useReactTable({ data: [...data], columns, state: { sorting, pagination }, onSortingChange: setSorting, onPaginationChange: setPagination, getCoreRowModel: getCoreRowModel(), getSortedRowModel: getSortedRowModel(), getPaginationRowModel: getPaginationRowModel() })
  if (loading) return <div className="surface-card p-6"><LoadingSkeleton lines={6} /></div>
  if (error) return <div className="surface-card"><ErrorState description={error} onRetry={onRetry} /></div>
  if (data.length === 0) return <div className="surface-card"><EmptyState title={emptyTitle} /></div>
  const rows = pageSize ? table.getPaginationRowModel().rows : table.getSortedRowModel().rows
  return <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm"><div className="overflow-x-auto"><table className="w-full border-collapse text-left text-sm"><thead className="bg-slate-50/80">{table.getHeaderGroups().map((group) => <tr key={group.id}>{group.headers.map((header) => { const direction = header.column.getIsSorted(); return <th key={header.id} scope="col" aria-sort={direction === 'asc' ? 'ascending' : direction === 'desc' ? 'descending' : 'none'} className="whitespace-nowrap border-b border-slate-200 px-5 py-3.5 text-xs font-semibold tracking-wide text-slate-500 uppercase">{header.isPlaceholder ? null : header.column.getCanSort() ? <button type="button" onClick={header.column.getToggleSortingHandler()} className="inline-flex items-center gap-1.5 rounded-md hover:text-slate-900 focus-visible:outline-none">{flexRender(header.column.columnDef.header, header.getContext())}{direction === 'asc' ? <ArrowUp className="size-3" /> : direction === 'desc' ? <ArrowDown className="size-3" /> : <ArrowUpDown className="size-3" />}</button> : flexRender(header.column.columnDef.header, header.getContext())}</th> })}</tr>)}</thead><tbody className="divide-y divide-slate-100">{rows.map((row) => <tr key={row.id} className="transition hover:bg-slate-50/70">{row.getVisibleCells().map((cell) => <td key={cell.id} className="whitespace-nowrap px-5 py-4 text-slate-600">{flexRender(cell.column.columnDef.cell, cell.getContext())}</td>)}</tr>)}</tbody></table></div>{pageSize && table.getPageCount() > 1 && <div className="border-t border-slate-200 px-5 py-4"><Pagination page={pagination.pageIndex + 1} totalPages={table.getPageCount()} onPageChange={(page) => table.setPageIndex(page - 1)} /></div>}</div>
}
