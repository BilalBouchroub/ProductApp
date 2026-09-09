import type { AdminUser, LogLevel, LogModule, PlatformLog } from '../../features/admin/types/admin.types'
import type { Competitor, MarketStudy } from '../../features/commercial/types/commercial.types'
import type { Product, ProductStatus, ProductionExperiment, ProductionProcess, ProductionStep, StepResource } from '../../features/production/types/production.types'
import type { UserRole } from '../../types/auth'

export type SharedProductVersion = { id:string;productId:string;version:number;status:ProductStatus;createdAt:string;updatedAt:string }
export type SharedProcess = Omit<ProductionProcess,'steps'>
export type SharedStep = Omit<ProductionStep,'resources'> & { productVersionId:string }
export type SharedResource = StepResource & { productVersionId:string }
export type SharedExperiment = ProductionExperiment & { productVersionId:string }
export type SharedMarketStudy = Omit<MarketStudy,'competitors'> & { productVersionId:string }
export type SharedCompetitor = Competitor & { studyId:string }
export type SharedPriority='Low'|'Medium'|'High'|'Critical'
export type SharedOptimizationRequest={id:string;productId:string;productVersionId:string;productVersion:number;message:string;priority:SharedPriority;requestedChanges:string[];commercialManagerName:string;createdAt:string;status:'New'|'InReview'|'InProgress'|'Resolved'|'Rejected'}
export type NotificationType='success'|'information'|'warning'|'error'
export type SharedNotification={id:string;recipientRole:UserRole;title:string;message:string;type:NotificationType;createdAt:string;isRead:boolean;relatedEntityType:string|null;relatedEntityId:string|null}
export interface SharedMockDatabase { schemaVersion:1;users:AdminUser[];products:Product[];productVersions:SharedProductVersion[];processes:SharedProcess[];productionSteps:SharedStep[];stepResources:SharedResource[];experiments:SharedExperiment[];marketStudies:SharedMarketStudy[];competitors:SharedCompetitor[];optimizationRequests:SharedOptimizationRequest[];notifications:SharedNotification[];auditLogs:PlatformLog[] }
export interface MockActor { id:string;name:string;role:UserRole;ipAddress:string }
export interface AuditEvent { action:string;module:LogModule;description:string;level?:LogLevel;entityType?:string|null;entityId?:string|null }
export interface NotificationEvent { recipientRole:UserRole;title:string;message:string;type:NotificationType;relatedEntityType?:string|null;relatedEntityId?:string|null }
export interface TransactionContext { actor?:MockActor;audit?:AuditEvent;notifications?:NotificationEvent[] }
export const productVersionId=(productId:string,version:number)=>`pv-${productId}-v${version}`
