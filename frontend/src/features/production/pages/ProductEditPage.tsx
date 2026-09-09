import { useNavigate, useParams } from 'react-router-dom'
import { toast } from 'sonner'
import { PageHeader } from '../../../components/common/PageHeader'
import { ErrorState } from '../../../components/ui/ErrorState'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { ProductForm } from '../components/products/ProductForm'
import { useProductionProduct, useProductMutations } from '../hooks/useProductionProducts'
export function ProductEditPage(){const {id=''}=useParams();const navigate=useNavigate();const query=useProductionProduct(id);const {update}=useProductMutations();if(query.isLoading)return <LoadingSkeleton/>;if(!query.data)return <ErrorState title="Produit introuvable" description="La fiche demandée n’existe pas."/>;return <div className="space-y-6"><PageHeader eyebrow="Produits" title={`Modifier ${query.data.name}`} description={`Version ${query.data.version} · ${query.data.internalReference}`}/><ProductForm initialValues={query.data} loading={update.isPending} onSubmit={(input)=>update.mutate({id,input},{onSuccess:()=>{toast.success('Produit mis à jour.');navigate(`/production/products/${id}`)},onError:(e)=>toast.error(e.message)})}/></div>}
