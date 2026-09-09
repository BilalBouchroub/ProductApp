import type { ColumnDef } from '@tanstack/react-table'
import { Copy, Eye } from 'lucide-react'
import { Link } from 'react-router-dom'
import { DataTable } from '../../../../components/common/DataTable'
import { Button } from '../../../../components/ui/Button'
import type { ProductionExperiment } from '../../types/production.types'
import { formatCurrency, formatProductionDate } from '../../utils/productionFormatters'
import { ExperimentResultBadge } from './ExperimentResultBadge'
export function ExperimentTable({data,loading,onDuplicate}:{data:ProductionExperiment[];loading?:boolean;onDuplicate:(id:string)=>void}){const columns:ColumnDef<ProductionExperiment>[]=[{accessorKey:'name',header:'Expérience',cell:({row})=><div><strong className="text-slate-900">{row.original.name}</strong><p className="text-xs text-slate-400">{row.original.productName} · v{row.original.productVersion}</p></div>},{accessorKey:'startDate',header:'Date',cell:({getValue})=>formatProductionDate(getValue<string>())},{accessorKey:'plannedCost',header:'Prévu / réel',cell:({row})=>`${formatCurrency(row.original.plannedCost)} / ${formatCurrency(row.original.actualCost)}`},{accessorKey:'wasteRate',header:'Pertes',cell:({getValue})=>`${getValue<number>()}%`},{accessorKey:'result',header:'Résultat',cell:({row})=><ExperimentResultBadge result={row.original.result}/>},{id:'actions',header:'Actions',cell:({row})=><div><Link to={`/production/experiments/${row.original.id}`}><Button variant="ghost" size="icon"><Eye className="size-4"/></Button></Link><Button variant="ghost" size="icon" onClick={()=>onDuplicate(row.original.id)}><Copy className="size-4"/></Button></div>}];return <DataTable data={data} columns={columns} loading={loading} pageSize={7} emptyTitle="Aucune expérience"/>}
