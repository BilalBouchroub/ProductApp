import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { PageHeader } from '../../../components/common/PageHeader'
import { ExperimentForm } from '../components/experiments/ExperimentForm'
import { useExperimentMutations } from '../hooks/useProductionExperiments'
import { useProductionProducts } from '../hooks/useProductionProducts'
export function ExperimentCreatePage(){const products=useProductionProducts();const {create}=useExperimentMutations();const navigate=useNavigate();return <div className="space-y-6"><PageHeader eyebrow="Expériences" title="Nouvel essai de production" description="Documentez les hypothèses, valeurs prévues, résultats réels et conclusion."/><ExperimentForm products={products.data??[]} loading={create.isPending} onSubmit={v=>create.mutate(v,{onSuccess:e=>{toast.success('Expérience créée.');navigate(`/production/experiments/${e.id}`)},onError:e=>toast.error(e.message)})}/></div>}
