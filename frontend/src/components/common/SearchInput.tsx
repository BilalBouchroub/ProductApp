import { Search, X } from 'lucide-react'
import type { ChangeEvent } from 'react'

interface SearchInputProps { value: string; onChange: (value: string) => void; placeholder?: string; label?: string }
export function SearchInput({ value, onChange, placeholder = 'Rechercher…', label = 'Rechercher' }: SearchInputProps) {
  return <div className="relative min-w-0 flex-1"><label htmlFor="global-search" className="sr-only">{label}</label><Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-slate-400" /><input id="global-search" type="search" value={value} onChange={(event: ChangeEvent<HTMLInputElement>) => onChange(event.target.value)} placeholder={placeholder} className="field-control pr-9 pl-9" />{value && <button type="button" onClick={() => onChange('')} className="absolute top-1/2 right-2 -translate-y-1/2 rounded-md p-1 text-slate-400 hover:bg-slate-100" aria-label="Effacer la recherche"><X className="size-4" /></button>}</div>
}
