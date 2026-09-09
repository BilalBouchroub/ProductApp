import { Boxes, Copy, Eye, Pencil, Send, Trash2 } from 'lucide-react'
import { ErrorState } from '../../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../../components/ui/LoadingSkeleton'
import type { Product } from '../../types/production.types'
import { formatCurrency, formatProductionDate } from '../../utils/productionFormatters'
import { ProductStatusBadge } from './ProductStatusBadge'

type ProductAction = 'duplicate' | 'send' | 'delete'
interface ProductTableProps {
  products: readonly Product[]
  loading?: boolean
  error?: string
  pendingAction?: { id: string; action: ProductAction } | null
  onDuplicate: (product: Product) => void
  onSend: (product: Product) => void
  onDelete: (product: Product) => void
}

const actionClass = 'inline-flex h-9 items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-blue-100 disabled:cursor-wait disabled:opacity-50'

export function ProductTable(props: ProductTableProps) {
  const { products, loading = false, error, pendingAction = null, onDuplicate, onSend, onDelete } = props
  if (loading) return <div className='surface-card p-6'><LoadingSkeleton lines={6} /></div>
  if (error) return <div className='surface-card'><ErrorState description={error} /></div>
  if (!products.length) return <div className='surface-card p-8 text-center'>Aucun produit trouv&eacute;</div>
  const pending = (product: Product, action: ProductAction) => pendingAction?.id === product.id && pendingAction.action === action

  return <section aria-label='Liste des produits' className='space-y-3'>
    {products.map(product => {
      const themeColor = product.themeColor ?? '#2563EB'
      return <article key={product.id} className='surface-card overflow-hidden border-l-[6px] p-5'
        style={{ borderLeftColor: themeColor }} data-theme-color={themeColor}>
        <div className='flex flex-col gap-5 sm:flex-row'>
          <div className='h-28 w-full shrink-0 overflow-hidden rounded-2xl border border-slate-200 bg-slate-50 sm:w-32'>
            {product.imageUrl
              ? <img src={product.imageUrl} alt={`Image de ${product.name}`} loading='lazy' className='size-full object-cover' />
              : <div className='grid size-full place-items-center' style={{ backgroundColor: `${themeColor}18`, color: themeColor }}>
                <Boxes className='size-9' aria-hidden='true' />
              </div>}
          </div>
          <div className='min-w-0 flex-1'>
            <header className='flex flex-wrap items-start justify-between gap-3'>
              <div className='min-w-0'>
                <strong className='block truncate text-lg text-slate-950'>{product.name}</strong>
                <p className='mt-1 text-sm text-slate-500'>{product.internalReference} / {product.sapCode}</p>
              </div>
              <ProductStatusBadge status={product.status} />
            </header>
            <div className='mt-3 flex flex-wrap items-center gap-2 text-xs text-slate-500'>
              <span className='rounded-full px-2.5 py-1 font-bold' style={{ backgroundColor: `${themeColor}18`, color: themeColor }}>{product.category}</span>
              <span>Version {product.version}</span><span aria-hidden='true'>/</span>
              <span>{formatCurrency(product.targetSalePrice)}</span><span aria-hidden='true'>/</span>
              <span>Modifie {formatProductionDate(product.updatedAt)}</span>
            </div>
            <div className='mt-4 flex flex-wrap gap-2 border-t border-slate-100 pt-4'>
              <a href={`/production/products/${product.id}`} className={actionClass} aria-label={`Consulter ${product.name}`}><Eye />Voir</a>
              <a href={`/production/products/${product.id}/edit`} className={actionClass} aria-label={`Modifier ${product.name}`}><Pencil />Modifier</a>
              <button type='button' className={actionClass} disabled={pending(product, 'duplicate')} onClick={() => onDuplicate(product)} aria-label={`Dupliquer ${product.name}`}><Copy />Dupliquer</button>
              <button type='button' className={actionClass} disabled={pending(product, 'send')} onClick={() => onSend(product)} aria-label={`Envoyer ${product.name} au commercial`}><Send />Envoyer</button>
              {product.status === 'Draft' && <button type='button' className={actionClass} disabled={pending(product, 'delete')} onClick={() => onDelete(product)} aria-label={`Supprimer ${product.name}`}><Trash2 />Supprimer</button>}
            </div>
          </div>
        </div>
      </article>
    })}
  </section>
}
