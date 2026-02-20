/**
 * ITSM Module Types
 * Incidents, Problems, Changes, Configuration Items, SLA Management
 * 
 * ITSM (IT Service Management) - ITIL framework implementation
 */

import { BaseEntity } from './common';

// ============================================================================
// INCIDENTS / SERVICE REQUESTS
// ============================================================================

export enum IncidentStatus {
  New = 'new',
  Active = 'active',
  OnHold = 'on_hold',
  Pending = 'pending',
  Resolved = 'resolved',
  Closed = 'closed',
  Reopened = 'reopened'
}

export enum IncidentPriority {
  Low = 4,
  Medium = 3,
  High = 2,
  Critical = 1
}

export enum IncidentUrgency {
  Low = 3,
  Medium = 2,
  High = 1
}

export interface Incident extends BaseEntity {
  number?: string;
  title: string;
  description: string;
  status: IncidentStatus;
  priority: IncidentPriority;
  urgency: IncidentUrgency;
  impact?: number; // 1-3: Low, Medium, High
  accountId?: number;
  accountName?: string;
  contactId?: number;
  contactName?: string;
  assignedTo?: number;
  assignedToName?: string;
  createdBy?: number;
  createdByName?: string;
  category?: string;
  subcategory?: string;
  affectedService?: string;
  affectedCI?: string;
  rootCause?: string;
  resolution?: string;
  resolutionDate?: string;
  closureDate?: string;
  slaId?: number;
  slaStatus?: 'met' | 'breached' | 'at_risk';
  responseTime?: number; // Minutes
  resolutionTime?: number; // Minutes
  attachments?: string[];
  relatedIncidents?: number[];
  keywords?: string[];
}

export interface CreateIncidentDto {
  title: string;
  description: string;
  priority: IncidentPriority;
  urgency: IncidentUrgency;
  accountId?: number;
  contactId?: number;
  category?: string;
  affectedService?: string;
}

export interface UpdateIncidentDto {
  status?: IncidentStatus;
  priority?: IncidentPriority;
  urgency?: IncidentUrgency;
  assignedTo?: number;
  resolution?: string;
  resolutionDate?: string;
}

// ============================================================================
// PROBLEMS
// ============================================================================

export enum ProblemStatus {
  Identified = 'identified',
  Investigating = 'investigating',
  KnownError = 'known_error',
  Resolved = 'resolved',
  Closed = 'closed'
}

export interface Problem extends BaseEntity {
  number?: string;
  title: string;
  description: string;
  status: ProblemStatus;
  priority: IncidentPriority;
  affectedCI?: string;
  affectedService?: string;
  rootCause?: string;
  symptomsDescription?: string;
  workaround?: string;
  permanentSolution?: string;
  relatedIncidents?: number[];
  relatedChanges?: number[];
  investigationNotes?: string;
  resolvedDate?: string;
  createdBy?: number;
  assignedTo?: number;
}

export interface CreateProblemDto {
  title: string;
  description: string;
  priority: IncidentPriority;
  affectedCI?: string;
  affectedService?: string;
}

export interface UpdateProblemDto {
  status?: ProblemStatus;
  rootCause?: string;
  workaround?: string;
  permanentSolution?: string;
  resolvedDate?: string;
}

// ============================================================================
// CHANGES
// ============================================================================

export enum ChangeStatus {
  Draft = 'draft',
  SubmittedForApproval = 'submitted_for_approval',
  ApprovalInProgress = 'approval_in_progress',
  Approved = 'approved',
  Rejected = 'rejected',
  Scheduled = 'scheduled',
  InProgress = 'in_progress',
  OnHold = 'on_hold',
  Completed = 'completed',
  RolledBack = 'rolled_back',
  Cancelled = 'cancelled'
}

export enum ChangePriority {
  Planning = 4,
  Low = 3,
  Medium = 2,
  High = 1,
  Critical = 0
}

export enum ChangeRiskLevel {
  Low = 1,
  Medium = 2,
  High = 3,
  VeryHigh = 4
}

export enum ApprovalStatus {
  Pending = 'pending',
  Approved = 'approved',
  Rejected = 'rejected',
  Deferred = 'deferred'
}

export interface Change extends BaseEntity {
  number?: string;
  title: string;
  description: string;
  status: ChangeStatus;
  priority: ChangePriority;
  riskLevel: ChangeRiskLevel;
  createdById?: number;
  createdByName?: string;
  implementedById?: number;
  implementedByName?: string;
  startDate: string;
  endDate: string;
  actualStartDate?: string;
  actualEndDate?: string;
  impactAnalysis: string;
  rollbackPlan: string;
  testingPlan?: string;
  communicationPlan?: string;
  affectedServices?: string[];
  affectedCI?: string[];
  relatedIncidents?: number[];
  relatedProblems?: number[];
  approvals?: ChangeApproval[];
  cabVotes?: CABVote[];
}

export interface ChangeApproval {
  id?: number;
  changeId?: number;
  approverId: number;
  approverName?: string;
  status: ApprovalStatus;
  comments?: string;
  approvalDate?: string;
}

export interface CABVote {
  id?: number;
  changeId?: number;
  voterId: number;
  voterName?: string;
  vote: 'approve' | 'reject' | 'abstain';
  comments?: string;
  votedAt?: string;
}

export interface CreateChangeDto {
  title: string;
  description: string;
  priority: ChangePriority;
  riskLevel: ChangeRiskLevel;
  startDate: string;
  endDate: string;
  impactAnalysis: string;
  rollbackPlan: string;
  affectedServices?: string[];
}

export interface UpdateChangeDto {
  status?: ChangeStatus;
  title?: string;
  description?: string;
  endDate?: string;
  impactAnalysis?: string;
}

// ============================================================================
// SLA POLICIES
// ============================================================================

export interface SLAPolicy extends BaseEntity {
  name: string;
  description?: string;
  responseTime?: number; // Minutes
  resolutionTime?: number; // Minutes
  priority?: IncidentPriority;
  affectedService?: string;
  isActive?: boolean;
}

export interface SLAStatus {
  incidentId: number;
  slaId?: number;
  slaName?: string;
  responseDeadline?: string;
  resolutionDeadline?: string;
  responseBreached: boolean;
  resolutionBreached: boolean;
  status: 'met' | 'breached' | 'at_risk';
}

// ============================================================================
// CONFIGURATION ITEMS (CMDB)
// ============================================================================

export interface ConfigurationItem extends BaseEntity {
  name: string;
  ciType: string; // 'server', 'database', 'network', 'software', 'hardware'
  status: 'active' | 'inactive' | 'retired';
  location?: string;
  owner?: string;
  serialNumber?: string;
  warrantyExpiry?: string;
  lastAuditDate?: string;
  relationships?: string[]; // Related CIs
}

// ============================================================================
// KNOWLEDGE ARTICLES (KB)
// ============================================================================

export interface KnowledgeArticle extends BaseEntity {
  title: string;
  content: string;
  category?: string;
  subcategory?: string;
  keywords?: string[];
  status: 'draft' | 'published' | 'archived';
  views?: number;
  helpfulness?: number; // 0-100
  attachments?: string[];
  relatedArticles?: number[];
  seeAlso?: string[];
  author?: string;
  publishedDate?: string;
}

// ============================================================================
// ESCALATION RULES
// ============================================================================

export interface EscalationRule extends BaseEntity {
  name: string;
  description?: string;
  triggerType: 'time_based' | 'condition_based';
  triggerValue?: number; // Minutes for time-based
  triggerCondition?: string; // For condition-based
  escalationPath: EscalationStep[];
  isActive?: boolean;
}

export interface EscalationStep {
  sequence: number;
  escalateToUserId?: number;
  escalateToGroupId?: number;
  escalateToManager?: boolean;
  notificationMethod?: 'email' | 'sms' | 'phone' | 'slack';
  waitTime?: number; // Minutes before next step
}

// ============================================================================
// WORKFLOW ENGINE
// ============================================================================

export interface WorkflowDefinition extends BaseEntity {
  name: string;
  description?: string;
  entityType: 'Incident' | 'Problem' | 'Change' | 'ServiceRequest';
  initialState: string;
  states: WorkflowState[];
  transitions: WorkflowTransition[];
  isActive?: boolean;
}

export interface WorkflowState {
  id: string;
  name: string;
  type: 'start' | 'active' | 'end';
  rules?: WorkflowRule[];
}

export interface WorkflowTransition {
  fromState: string;
  toState: string;
  trigger: string;
  condition?: string;
  action?: string;
}

export interface WorkflowRule {
  id: string;
  name: string;
  description?: string;
  condition: string;
  action: string;
  order: number;
}

// ============================================================================
// TICKETS / SERVICE REQUESTS
// ============================================================================

export interface ServiceRequest extends BaseEntity {
  number?: string;
  title: string;
  description: string;
  status: IncidentStatus;
  priority: IncidentPriority;
  category?: string;
  accountId?: number;
  contactId?: number;
  assignedTo?: number;
  assignedToName?: string;
  resolution?: string;
  resolutionDate?: string;
  closureDate?: string;
  satisfactionRating?: number; // 1-5
  satisfactionComment?: string;

  // From analysis: missing ServiceRequest fields
  createdByUserId?: number;
  createdByUserName?: string;
  workflowId?: number;
  workflowName?: string;
  currentWorkflowStep?: string;
  dueDate?: string;
  firstResponseDate?: string;
  closedDate?: string;
  resolvedDate?: string;
  resolutionSummary?: string;
  resolutionCode?: string;
  rootCause?: string;
  relatedProductId?: number;
  relatedProductName?: string;
  customerFeedback?: string;
  tags?: string;
  internalNotes?: string;
  escalationLevel?: number;    // 0-3
  reopenCount?: number;
  isVipAccount?: boolean;
  estimatedEffortHours?: number;
  actualEffortHours?: number;
  slaStatus?: 'within' | 'at_risk' | 'breached';
  externalReferenceId?: string;
  sourceEmailAddress?: string;
  isEscalated?: boolean;
}

export interface CreateServiceRequestDto {
  title: string;
  description: string;
  priority: IncidentPriority;
  category?: string;
  accountId?: number;
  contactId?: number;
}
