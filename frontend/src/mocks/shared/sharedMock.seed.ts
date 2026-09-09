import { mockLogs } from '../../features/admin/mocks/logs.mock'
import { mockUsers } from '../../features/admin/mocks/users.mock'
import { mockMarketStudies } from '../../features/commercial/mocks/marketStudies.mock'
import { mockExperiments } from '../../features/production/mocks/experiments.mock'
import { mockOptimizationRequests } from '../../features/production/mocks/optimizationRequests.mock'
import { mockProcesses } from '../../features/production/mocks/processes.mock'
import { mockProducts } from '../../features/production/mocks/products.mock'
import type { SharedMockDatabase, SharedProductVersion } from './sharedMock.types'
import { productVersionId } from './sharedMock.types'

export function createSharedMockSeed():SharedMockDatabase {
  const processes=mockProcesses.map(({steps:_,...process})=>process)
  const productionSteps=mockProcesses.flatMap(process=>process.steps.map(({resources:_,...step})=>({...step,productVersionId:productVersionId(process.productId,process.productVersion)})))
  const stepResources=mockProcesses.flatMap(process=>process.steps.flatMap(step=>step.resources.map(resource=>({...resource,productVersionId:productVersionId(process.productId,process.productVersion)}))))
  const marketStudies=mockMarketStudies.map(({competitors:_,...study})=>({...study,productVersionId:productVersionId(study.productId,study.productVersion)}))
  const competitors=mockMarketStudies.flatMap(study=>study.competitors.map(competitor=>({...competitor,studyId:study.id})))
  const versions=new Map<string,{productId:string;version:number}>()
  const addVersion=(productId:string,version:number)=>versions.set(productVersionId(productId,version),{productId,version})
  mockProducts.forEach(product=>addVersion(product.id,product.version))
  mockProcesses.forEach(process=>addVersion(process.productId,process.productVersion))
  mockExperiments.forEach(experiment=>addVersion(experiment.productId,experiment.productVersion))
  mockMarketStudies.forEach(study=>addVersion(study.productId,study.productVersion))
  mockOptimizationRequests.forEach(request=>addVersion(request.productId,request.productVersion))
  const productVersions:SharedProductVersion[]=[...versions.entries()].map(([id,item])=>{const product=mockProducts.find(value=>value.id===item.productId);const current=product?.version===item.version;return{id,productId:item.productId,version:item.version,status:current?product.status:'Archived',createdAt:product?.createdAt??'2026-01-01T00:00:00Z',updatedAt:product?.updatedAt??'2026-01-01T00:00:00Z'}})
  return {schemaVersion:1,users:structuredClone(mockUsers),products:structuredClone(mockProducts),productVersions,processes:structuredClone(processes),productionSteps:structuredClone(productionSteps),stepResources:structuredClone(stepResources),experiments:mockExperiments.map(experiment=>({...structuredClone(experiment),productVersionId:productVersionId(experiment.productId,experiment.productVersion)})),marketStudies:structuredClone(marketStudies),competitors:structuredClone(competitors),optimizationRequests:mockOptimizationRequests.map(request=>({...request,productVersionId:productVersionId(request.productId,request.productVersion),priority:request.priority==='Urgent'?'Critical':request.priority})),notifications:[{id:'notification-error',recipientRole:'Administrator',title:'Erreur simulée',message:'Une erreur de synchronisation mockée a été détectée.',type:'error',createdAt:'2026-07-30T08:58:00Z',isRead:false,relatedEntityType:'System',relatedEntityId:'simulated-error'},{id:'notification-ready',recipientRole:'CommercialManager',title:'Produit prêt pour étude',message:'Biscuit Avoine Plus est disponible pour une étude de marché.',type:'information',createdAt:'2026-07-29T15:30:00Z',isRead:false,relatedEntityType:'Product',relatedEntityId:'prd-002'},{id:'notification-risk',recipientRole:'ProductionManager',title:'Optimisation prioritaire',message:'Une amélioration de coût et de stabilité est demandée.',type:'warning',createdAt:'2026-07-24T11:00:00Z',isRead:false,relatedEntityType:'Product',relatedEntityId:'prd-003'}],auditLogs:[{id:'log-simulated-error',timestamp:'2026-07-30T08:58:00Z',userId:'system',userName:'Service système',userRole:'Administrator',action:'SIMULATED_ERROR',module:'System',description:'Erreur de démonstration générée.',level:'Error',ipAddress:'127.0.0.1',entityType:'System',entityId:'simulated-error'},...structuredClone(mockLogs)]}
}
