import { LayoutGrid, List, Plus } from 'lucide-react'
import { useMemo, useState } from 'react'
import { toast } from 'sonner'
import { PageHeader } from '../../../components/common/PageHeader'
import { Button } from '../../../components/ui/Button'
import { DeleteProductDialog } from '../components/products/DeleteProductDialog'
import { DuplicateProductDialog } from '../components/products/DuplicateProductDialog'
import { ProductCard } from '../components/products/ProductCard'
import { ProductFilters, type ProductFilterValue } from '../components/products/ProductFilters'
import { ProductTable } from '../components/products/ProductTable'
import { SendToCommercialDialog } from '../components/products/SendToCommercialDialog'
import { useProductionProducts, useProductMutations } from '../hooks/useProductionProducts'
import type { Product } from '../types/production.types'

export function ProductsPage() {
  const query = useProductionProducts()
  const mutations = useProductMutations()
  const [filters, setFilters] = useState<ProductFilterValue>({ search: '', status: '', category: '' })
  const [grid, setGrid] = useState(false)
  const [duplicate, setDuplicate] = useState<Product | null>(null)
  const [send, setSend] = useState<Product | null>(null)
  const [deleting, setDeleting] = useState<Product | null>(null)

  const products = useMemo(() => query.data?.filter((product) => {
    const search = filters.search.toLowerCase()
    return (!search || `${product.name} ${product.internalReference} ${product.sapCode}`.toLowerCase().includes(search))
      && (!filters.status || product.status === filters.status)
      && (!filters.category || product.category === filters.category)
  }) ?? [], [query.data, filters])

  const categories = [...new Set(query.data?.map(product => product.category) ?? [])]
  const run = async (promise: Promise<unknown>, success: string, close: () => void) => {
    try {
      await promise
      toast.success(success)
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Operation impossible.')
    } finally {
      close()
    }
  }

  const viewButton = <Button variant='secondary' size='icon' onClick={() => setGrid(!grid)} aria-label='Changer la vue'>{grid ? <List /> : <LayoutGrid />}</Button>
  const newLink = <a href='/production/products/new' className='inline-flex h-10 items-center gap-2 rounded-xl bg-blue-600 px-4 font-semibold text-white'><Plus />Nouveau produit</a>
  const table = <ProductTable products={products} loading={query.isLoading} error={query.error instanceof Error ? query.error.message : undefined} onDuplicate={setDuplicate} onSend={setSend} onDelete={setDeleting} />
  const gridView = <div className='grid gap-4 md:grid-cols-2 xl:grid-cols-3'>{products.map(product => <ProductCard key={product.id} product={product} />)}</div>
  const duplicateDialog = <DuplicateProductDialog open={Boolean(duplicate)} name={duplicate?.name ?? ''} loading={mutations.duplicate.isPending} onClose={() => setDuplicate(null)} onConfirm={() => duplicate && void run(mutations.duplicate.mutateAsync(duplicate.id), 'Produit duplique.', () => setDuplicate(null))} />
  const sendDialog = <SendToCommercialDialog open={Boolean(send)} name={send?.name ?? ''} loading={mutations.send.isPending} onClose={() => setSend(null)} onConfirm={() => send && void run(mutations.send.mutateAsync(send.id), 'Produit transmis au Commercial.', () => setSend(null))} />
  const deleteDialog = <DeleteProductDialog open={Boolean(deleting)} name={deleting?.name ?? ''} loading={mutations.remove.isPending} onClose={() => setDeleting(null)} onConfirm={() => deleting && void run(mutations.remove.mutateAsync(deleting.id), 'Produit supprim\u00e9.', () => setDeleting(null))} />
  const dialogs = <>{duplicateDialog}{sendDialog}{deleteDialog}</>
  const headerActions = <>{viewButton}{newLink}</>
  return <div className='space-y-6'>
    <PageHeader eyebrow='Production' title='Produits industriels' description={'Pilotez les fiches produit, leurs versions et leur passage vers l\u2019\u00e9tude commerciale.'} actions={headerActions} />
    <ProductFilters value={filters} categories={categories} onChange={setFilters} />
    {grid ? gridView : table}
    {dialogs}
  </div>
}
