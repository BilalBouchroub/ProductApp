import type { MarketStudy } from '../../features/commercial/types/commercial.types'
import type { ProductionProcess } from '../../features/production/types/production.types'
import { createSharedMockSeed } from './sharedMock.seed'
import { clearSharedMockStorage, readSharedMockStorage, writeSharedMockStorage } from './sharedMock.storage'
import type { MockActor, SharedMockDatabase, TransactionContext } from './sharedMock.types'
import { productVersionId } from './sharedMock.types'

type Listener=()=>void
let database=readSharedMockStorage()??createSharedMockSeed()
let actor:MockActor={id:'system',name:'Service système',role:'Administrator',ipAddress:'127.0.0.1'}
const listeners=new Set<Listener>()
const emit=()=>listeners.forEach(listener=>listener())
const clone=<T,>(value:T):T=>structuredClone(value)

function appendEvents(context:TransactionContext):void{const current=context.actor??actor,now=new Date().toISOString();if(context.audit)database.auditLogs=[{id:`log-${crypto.randomUUID()}`,timestamp:now,userId:current.id,userName:current.name,userRole:current.role,action:context.audit.action,module:context.audit.module,description:context.audit.description,level:context.audit.level??'Information',ipAddress:current.ipAddress,entityType:context.audit.entityType??null,entityId:context.audit.entityId??null},...database.auditLogs];if(context.notifications?.length)database.notifications=[...context.notifications.map(item=>({id:`notification-${crypto.randomUUID()}`,recipientRole:item.recipientRole,title:item.title,message:item.message,type:item.type,createdAt:now,isRead:false,relatedEntityType:item.relatedEntityType??null,relatedEntityId:item.relatedEntityId??null})),...database.notifications]}
function persist():void{writeSharedMockStorage(database);emit()}

export const sharedMockRepository={
  subscribe(listener:Listener){listeners.add(listener);return()=>{listeners.delete(listener)}},
  setActor(next:MockActor){actor=next},getActor(){return clone(actor)},
  read(){return clone(database)},
  transaction(context:TransactionContext,mutate:(draft:SharedMockDatabase)=>void){mutate(database);appendEvents(context);persist()},
  reset(){database=createSharedMockSeed();clearSharedMockStorage();writeSharedMockStorage(database);emit()},
  getProcess(productId:string,version?:number):ProductionProcess{const product=database.products.find(item=>item.id===productId);const targetVersion=version??product?.version??1;const process=database.processes.find(item=>item.productId===productId&&item.productVersion===targetVersion)??{id:`process-${productId}-v${targetVersion}`,productId,productVersion:targetVersion,version:1,updatedAt:new Date().toISOString(),savedAt:null};const versionId=productVersionId(productId,targetVersion);const steps=database.productionSteps.filter(step=>step.productVersionId===versionId).sort((a,b)=>a.order-b.order).map(step=>{const{productVersionId:_,...plain}=step;return{...plain,resources:database.stepResources.filter(resource=>resource.stepId===step.id).map(resource=>{const{productVersionId:__,...item}=resource;return item})}});return clone({...process,steps})},
  getStudies():MarketStudy[]{return clone(database.marketStudies.map(study=>{const{productVersionId:_,...plain}=study;return{...plain,competitors:database.competitors.filter(item=>item.studyId===study.id).map(item=>{const{studyId:__,...competitor}=item;return competitor})}}))},
}
