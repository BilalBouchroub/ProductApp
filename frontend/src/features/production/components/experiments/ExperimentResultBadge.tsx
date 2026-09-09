import { StatusBadge } from '../../../../components/common/StatusBadge'
import type { ExperimentResult } from '../../types/production.types'
const data:Record<ExperimentResult,{label:string;tone:'success'|'warning'|'danger'|'neutral'}>={Success:{label:'Réussie',tone:'success'},PartialSuccess:{label:'Partiellement réussie',tone:'warning'},Failed:{label:'Échouée',tone:'danger'},Cancelled:{label:'Annulée',tone:'neutral'}}
export function ExperimentResultBadge({result}:{result:ExperimentResult}){return <StatusBadge {...data[result]}/>}
