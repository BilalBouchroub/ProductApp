import { ArrowLeft } from 'lucide-react'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { PageHeader } from '../../../components/common/PageHeader'
import { Button } from '../../../components/ui/Button'
import { ProductForm } from '../components/products/ProductForm'
import { useProductMutations } from '../hooks/useProductionProducts'
export function ProductCreatePage(){ const navigate=useNavigate(); const {create}=useProductMutations(); return <div className="space-y-6"><PageHeader eyebrow="Produits" title="Créer une fiche produit" description="Définissez une base industrielle prête à recevoir sa gamme de production." actions={<Link to="/production/products"><Button variant="secondary"><ArrowLeft className="size-4"/>Retour</Button></Link>}/><ProductForm loading={create.isPending} onSubmit={(value)=>create.mutate(value,{onSuccess:(p)=>{toast.success('Produit créé.');navigate(`/production/products/${p.id}`)},onError:(e)=>toast.error(e.message)})}/></div> }
