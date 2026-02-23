// ITSM Components Index
// Export all ITSM-related components for easy importing

// Incident Management - NEW
export { IncidentStatusBadge } from './IncidentStatusBadge';
export { IncidentPriorityBadge } from './IncidentPriorityBadge';
export { IncidentSLAIndicator } from './IncidentSLAIndicator';
export { IncidentAssignmentModal } from './IncidentAssignmentModal';
export { IncidentActivityTimeline } from './IncidentActivityTimeline';
export { IncidentBulkActionTools } from './IncidentBulkActionTools';

// Existing Incident Management
export { SLACountdownWidget } from './SLACountdownWidget';
export type { SLACountdownWidgetProps, SLAInstanceData, SLAStatus } from './SLACountdownWidget';

export { ImpactUrgencyMatrix } from './ImpactUrgencyMatrix';
export type { ImpactUrgencyMatrixProps, ImpactLevel, UrgencyLevel, PriorityLevel } from './ImpactUrgencyMatrix';

export { IncidentTimeline } from './IncidentTimeline';
export type { IncidentTimelineProps, TimelineActivity, ActivityType } from './IncidentTimeline';

export { SLABreachAlert, SLABreachBanner } from './SLABreachAlert';
export type { SLABreachAlertProps, SLABreachBannerProps, SLABreachInfo, BreachType, BreachSeverity } from './SLABreachAlert';

// Problem Management - NEW
export { ProblemRelatedIncidentsList } from './ProblemRelatedIncidentsList';

// Existing Problem Management
export { RootCauseAnalysisTemplate } from './RootCauseAnalysisTemplate';
export type { RootCauseAnalysisTemplateProps, RootCauseAnalysis, WhyStep } from './RootCauseAnalysisTemplate';

export { RelatedIncidentsWidget } from './RelatedIncidentsWidget';
export type { RelatedIncidentsWidgetProps, RelatedIncident } from './RelatedIncidentsWidget';

// Change Management - NEW
export { ChangeImpactAnalysisPanel } from './ChangeImpactAnalysisPanel';
export { ChangeApprovalWorkflow } from './ChangeApprovalWorkflowPanel';
export { RiskAssessmentForm as RiskAssessmentPanelForm } from './RiskAssessmentPanel';
export { RiskAssessmentForm } from './RiskAssessmentForm';

// Existing Change Management
export { ApprovalWorkflowPanel } from './ApprovalWorkflowPanel';
export type { ApprovalWorkflowPanelProps, ApprovalStep, ApprovalStatus, Approver } from './ApprovalWorkflowPanel';

export { ChangeConflictDetector } from './ChangeConflictDetector';
export type { ChangeConflictDetectorProps, ChangeConflict, ConflictingChange, ConflictType, ConflictSeverity } from './ChangeConflictDetector';

// Knowledge Base
export { ArticleSuggestions } from './ArticleSuggestions';
export type { ArticleSuggestionsProps, SuggestedArticle, ArticleType } from './ArticleSuggestions';

export { ArticleFeedbackWidget } from './ArticleFeedbackWidget';
export type { ArticleFeedbackWidgetProps, FeedbackStats } from './ArticleFeedbackWidget';

// Service Catalog
export { CatalogCategoryBrowser } from './CatalogCategoryBrowser';
export type { CatalogCategoryBrowserProps, CatalogCategory, CatalogItem } from './CatalogCategoryBrowser';

export { CatalogRequestForm } from './CatalogRequestForm';
export type { CatalogRequestFormProps, CatalogItemDetails, FormField, FieldType, FieldOption } from './CatalogRequestForm';

// CMDB / Configuration Management
export { RelationshipDiagram } from './RelationshipDiagram';
export type { 
  RelationshipDiagramProps, 
  ConfigurationItemNode,
  CIRelationship as RelDiagramRelationship,
  CIType as RelDiagramCIType,
  RelationshipType as RelDiagramRelType
} from './RelationshipDiagram';

export { ServiceMap } from './ServiceMap';
export type { ServiceMapProps, ServiceNode, ServiceStatus, ServiceType } from './ServiceMap';

export { CIRelationshipDiagram } from './CIRelationshipDiagram';
export type { 
  CIRelationshipDiagramProps, 
  ConfigurationItem, 
  CIRelationship, 
  CIType, 
  CIStatus, 
  RelationshipType 
} from './CIRelationshipDiagram';

// Service Desk Sub-Components
export { default as ServiceRequestCard } from './ServiceRequestCard';
export type { ServiceRequestCardProps } from './ServiceRequestCard';

export { default as StatusTransitionButtons } from './StatusTransitionButtons';
export type { StatusTransitionButtonsProps } from './StatusTransitionButtons';

export { default as ResolutionForm } from './ResolutionForm';
export type { ResolutionFormProps, ResolutionData } from './ResolutionForm';

export { default as FeedbackForm } from './FeedbackForm';
export type { FeedbackFormProps, FeedbackData } from './FeedbackForm';

export { default as SLAStatusBadge } from './SLAStatusBadge';
export type { SLAStatusBadgeProps } from './SLAStatusBadge';

export { default as ServiceRequestStats } from './ServiceRequestStats';
export type { ServiceRequestStatsProps } from './ServiceRequestStats';
