import { StatusBadge } from '../../../../components/common/StatusBadge'
import type { CommercialRecommendation } from '../../types/commercial.types'
const data:Record<CommercialRecommendation,{label:string;tone:'success'|'warning'|'danger'}>={Viable:{label:'Produit viable',tone:'success'},ToOptimize:{label:'À optimiser',tone:'warning'},NotViable:{label:'Non viable',tone:'danger'}}
export function RecommendationBadge({recommendation}:{recommendation:CommercialRecommendation}){return <StatusBadge {...data[recommendation]}/>}
