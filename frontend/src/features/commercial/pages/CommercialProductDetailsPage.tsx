import { ArrowLeft, BarChart3, Boxes } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { PageHeader } from '../../../components/common/PageHeader'
import { Button } from '../../../components/ui/Button'
import { ErrorState } from '../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { ResourceAvailabilityBadge } from '../../production/components/resources/ResourceAvailabilityBadge'
import { formatCurrency, formatProductionDate } from '../../production/utils/productionFormatters'
import { ProductTechnicalSummary } from '../components/products/ProductTechnicalSummary'
import { ProductionChainReadOnly } from '../components/products/ProductionChainReadOnly'
import { ProductionExperimentsSummary } from '../components/products/ProductionExperimentsSummary'
import { useCommercialProduct } from '../hooks/useCommercialProducts'

export function CommercialProductDetailsPage() {
  const { productId = '' } = useParams()
  const productQuery = useCommercialProduct(productId)
  if (productQuery.isLoading) return <LoadingSkeleton lines={8} />
  if (productQuery.isError || !productQuery.data) {
    const reason = productQuery.error instanceof Error ? productQuery.error.message : 'Ce produit n’est pas accessible pour une étude.'
    return <ErrorState title="Produit indisponible" description={reason} onRetry={() => productQuery.refetch()} />
  }

  const view = productQuery.data
  const product = view.product
  const facts = [
    ['Référence interne', product.internalReference],
    ['Code SAP', product.sapCode || 'Non renseigné'],
    ['Catégorie', product.category],
    ['Version transmise', `Version ${product.version}`],
    ['Lot de production', `${product.batchQuantity} ${product.productionUnit}`],
    ['Dernière mise à jour', formatProductionDate(product.updatedAt)],
  ]

  return <div className="space-y-6">
    <PageHeader eyebrow={`${product.category} · version ${product.version}`} title={product.name} description={product.description} actions={<><Link to="/commercial/products"><Button variant="secondary"><ArrowLeft className="size-4" />Retour</Button></Link><Link to={view.study ? `/commercial/studies/${view.study.id}` : `/commercial/products/${product.id}/market-study`}><Button><BarChart3 className="size-4" />{view.study ? 'Continuer l’étude' : 'Démarrer l’étude'}</Button></Link></>} />

    <section className="surface-card p-5 sm:p-6"><div className="flex items-center gap-3"><span className="grid size-10 place-items-center rounded-xl bg-blue-50 text-blue-600"><Boxes className="size-5" /></span><div><h2 className="font-bold text-slate-950">Informations du produit</h2><p className="text-xs text-slate-500">Référentiel transmis par le Responsable Production.</p></div></div><dl className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">{facts.map(([label, value]) => <div key={label} className="rounded-xl bg-slate-50 p-3"><dt className="text-xs text-slate-400">{label}</dt><dd className="mt-1 text-sm font-semibold text-slate-800">{value}</dd></div>)}</dl></section>

    <ProductTechnicalSummary view={view} />

    <section className="surface-card p-5 sm:p-6"><div className="mb-5"><h2 className="font-bold text-slate-950">Chaîne de production complète</h2><p className="mt-1 text-sm text-slate-500">{view.stepCount} étape{view.stepCount > 1 ? 's' : ''} synchronisée{view.stepCount > 1 ? 's' : ''} depuis la Production.</p></div><ProductionChainReadOnly process={view.process} /></section>

    <section className="surface-card p-5 sm:p-6"><div className="mb-4"><h2 className="font-bold text-slate-950">Ressources et coût détaillé</h2><p className="mt-1 text-sm text-slate-500">Toutes les matières, machines et ressources associées aux étapes.</p></div>{view.resources.length > 0 ? <div className="overflow-x-auto"><table className="w-full text-left text-sm"><thead className="text-xs uppercase text-slate-400"><tr><th className="p-3">Ressource</th><th className="p-3">Type</th><th className="p-3">Quantité prévue / réelle</th><th className="p-3">Coût</th><th className="p-3">Disponibilité</th></tr></thead><tbody className="divide-y divide-slate-100">{view.resources.map(resource => <tr key={resource.id}><td className="p-3 font-semibold">{resource.designation}</td><td className="p-3">{resource.resourceType}</td><td className="p-3">{resource.plannedQuantity} / {resource.actualQuantity || '—'} {resource.unit}</td><td className="p-3">{formatCurrency(resource.totalCost)}</td><td className="p-3"><ResourceAvailabilityBadge status={resource.availabilityStatus} /></td></tr>)}</tbody></table></div> : <div className="rounded-xl border border-dashed border-slate-300 bg-slate-50 p-6 text-center text-sm text-slate-500">Aucune ressource n’est associée aux étapes de cette version.</div>}</section>

    <section className="surface-card p-5 sm:p-6"><div className="mb-4"><h2 className="font-bold text-slate-950">Expériences de production</h2><p className="mt-1 text-sm text-slate-500">Essais, objectifs, hypothèses et conclusions de la version {product.version}.</p></div><ProductionExperimentsSummary experiments={view.experiments} /></section>
  </div>
}
