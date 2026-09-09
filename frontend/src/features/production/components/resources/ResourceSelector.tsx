import { Select } from '../../../../components/ui/Select'
import { mockSapMaterials } from '../../mocks/sapMaterials.mock'
import type { SapMaterialOption } from '../../types/production.types'
export function ResourceSelector({onSelect}:{onSelect:(material:SapMaterialOption)=>void}){return <Select label="Matière SAP mockée" defaultValue="" options={[{label:'Sélectionner une matière…',value:''},...mockSapMaterials.map((m)=>({label:`${m.sapCode} — ${m.designation}`,value:m.sapCode}))]} onChange={(e)=>{const m=mockSapMaterials.find((item)=>item.sapCode===e.target.value);if(m)onSelect(m)}}/>}
