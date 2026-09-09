import { StatusBadge } from '../../../../components/common/StatusBadge'
import type { ResourceAvailabilityStatus } from '../../types/production.types'
const labels:Record<ResourceAvailabilityStatus,string>={Available:'Disponible',LowStock:'Stock faible',Unavailable:'Indisponible',ToOrder:'À commander'}
export function ResourceAvailabilityBadge({status}:{status:ResourceAvailabilityStatus}){return <StatusBadge label={labels[status]} tone={status==='Available'?'success':status==='LowStock'?'warning':'danger'}/>}
