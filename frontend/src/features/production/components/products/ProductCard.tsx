import { ArrowRight, Boxes } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { Product } from '../../types/production.types'
import { formatCurrency, formatProductionDate } from '../../utils/productionFormatters'
import { ProductStatusBadge } from './ProductStatusBadge'

export function ProductCard({ product }: { product: Product }) {
  const themeColor = product.themeColor ?? '#2563EB'
  return <article className='surface-card overflow-hidden border-t-4' style={{ borderTopColor: themeColor }}>
    {product.imageUrl
      ? <img src={product.imageUrl} alt='' className='h-32 w-full object-cover' />
      : <div className='grid h-24 place-items-center bg-slate-50' style={{ color: themeColor }}><Boxes className='size-9' /></div>}
    <div className='p-5'>
      <div className='flex items-start justify-between gap-3'>
        <span className='rounded-full px-2.5 py-1 text-xs font-bold text-white' style={{ backgroundColor: themeColor }}>{product.category}</span>
        <ProductStatusBadge status={product.status} />
      </div>
      <h3 className='mt-4 font-bold text-slate-950'>{product.name}</h3>
      <p className='mt-1 text-xs text-slate-500'>{product.internalReference} � v{product.version}</p>
      <div className='mt-4 grid grid-cols-2 gap-3 text-sm'>
        <div><span className='text-xs text-slate-400'>Lot</span><p className='font-semibold'>{product.batchQuantity} {product.productionUnit}</p></div>
        <div><span className='text-xs text-slate-400'>Prix cible</span><p className='font-semibold'>{formatCurrency(product.targetSalePrice)}</p></div>
      </div>
      <div className='mt-4 flex items-center justify-between border-t border-slate-100 pt-4 text-xs text-slate-500'>
        <span>Modifie {formatProductionDate(product.updatedAt)}</span>
        <Link className='inline-flex items-center gap-1 font-semibold' style={{ color: themeColor }} to={`/production/products/${product.id}`}>Ouvrir <ArrowRight className='size-3' /></Link>
      </div>
    </div>
  </article>
}
