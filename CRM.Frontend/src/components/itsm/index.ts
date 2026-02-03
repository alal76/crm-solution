// ITSM Components Index
// Export all ITSM-related components for easy importing

// Incident Management
export { SLACountdownWidget } from './SLACountdownWidget';
export type { SLACountdownWidgetProps, SLAInfo } from './SLACountdownWidget';

export { ImpactUrgencyMatrix } from './ImpactUrgencyMatrix';
export type { ImpactUrgencyMatrixProps, PriorityResult } from './ImpactUrgencyMatrix';

export { IncidentTimeline } from './IncidentTimeline';
export type { IncidentTimelineProps, TimelineActivity, ActivityType } from './IncidentTimeline';

export { SLABreachAlert, SLABreachBanner } from './SLABreachAlert';
export type { SLABreachAlertProps, SLABreachBannerProps, SLABreachInfo, BreachType, BreachSeverity } from './SLABreachAlert';

// Problem Management
export { RootCauseAnalysisTemplate } from './RootCauseAnalysisTemplate';
export type { RootCauseAnalysisTemplateProps, RCAData, WhyStep } from './RootCauseAnalysisTemplate';

export { RelatedIncidentsWidget } from './RelatedIncidentsWidget';
export type { RelatedIncidentsWidgetProps, RelatedIncident } from './RelatedIncidentsWidget';

// Change Management
export { ApprovalWorkflowPanel } from './ApprovalWorkflowPanel';
export type { ApprovalWorkflowPanelProps, ApprovalStep, ApprovalLevel } from './ApprovalWorkflowPanel';

export { RiskAssessmentForm } from './RiskAssessmentForm';
export type { RiskAssessmentFormProps, RiskFactor, RiskAnswer, RiskAssessmentResult, RiskLevel } from './RiskAssessmentForm';

export { ChangeConflictDetector } from './ChangeConflictDetector';
export type { ChangeConflictDetectorProps, ChangeConflict, ConflictingChange, ConflictType, ConflictSeverity } from './ChangeConflictDetector';

// Knowledge Base
export { ArticleSuggestions } from './ArticleSuggestions';
export type { ArticleSuggestionsProps, SuggestedArticle } from './ArticleSuggestions';

export { ArticleFeedbackWidget } from './ArticleFeedbackWidget';
export type { ArticleFeedbackWidgetProps, ArticleFeedback, FeedbackRating } from './ArticleFeedbackWidget';

// Service Catalog
export { CatalogCategoryBrowser } from './CatalogCategoryBrowser';
export type { CatalogCategoryBrowserProps, CatalogCategory, CatalogItem } from './CatalogCategoryBrowser';

export { CatalogRequestForm } from './CatalogRequestForm';
export type { CatalogRequestFormProps, CatalogItemDetails, FormField, FieldType, FieldOption } from './CatalogRequestForm';

// CMDB / Configuration Management
export { RelationshipDiagram } from './RelationshipDiagram';
export type { RelationshipDiagramProps, CINode, CIRelationship as RelDiagramRelationship } from './RelationshipDiagram';

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
