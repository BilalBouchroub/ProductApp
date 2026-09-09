import { PageHeader } from '../../../components/common/PageHeader'
import { LoadingSkeleton } from '../../../components/ui/LoadingSkeleton'
import { ExperimentCard } from '../components/experiments/ExperimentCard'
import { useProductionExperiments } from '../hooks/useProductionExperiments'
export function RecentExperimentsPage(){const query=useProductionExperiments();if(query.isLoading)return <LoadingSkeleton lines={6}/>;return <div className="space-y-6"><PageHeader eyebrow="Suivi visuel" title="Expériences récentes" description="Les derniers écarts de coût, durée, rendement et qualité en un coup d’œil."/><div className="grid gap-5 lg:grid-cols-2">{query.data?.slice().sort((a,b)=>b.startDate.localeCompare(a.startDate)).slice(0,6).map(e=><ExperimentCard key={e.id} experiment={e}/>)}</div></div>}
