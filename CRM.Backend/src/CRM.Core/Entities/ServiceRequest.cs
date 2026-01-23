/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Core.Models;

namespace CRM.Core.Entities;

#region Service Request Enumerations

/// <summary>
/// Channel through which the service request was received
/// </summary>
public enum ServiceRequestChannel
{
    /// <summary>Request received via WhatsApp messaging</summary>
    WhatsApp = 0,
    
    /// <summary>Request received via Email</summary>
    Email = 1,
    
    /// <summary>Request received via Phone call</summary>
    Phone = 2,
    
    /// <summary>Request received in person</summary>
    InPerson = 3,
    
    /// <summary>Request submitted through self-service portal</summary>
    SelfServicePortal = 4,
    
    /// <summary>Request from social media</summary>
    SocialMedia = 5,
    
    /// <summary>Request from live chat</summary>
    LiveChat = 6,
    
    /// <summary>Request from API integration</summary>
    API = 7
}

/// <summary>
/// Current status of the service request
/// </summary>
public enum ServiceRequestStatus
{
    /// <summary>Newly created request, not yet reviewed</summary>
    New = 0,
    
    /// <summary>Request has been opened and acknowledged</summary>
    Open = 1,
    
    /// <summary>Request is actively being worked on</summary>
    InProgress = 2,
    
    /// <summary>Waiting for customer response or external dependency</summary>
    PendingCustomer = 3,
    
    /// <summary>Waiting for internal approval or resource</summary>
    PendingInternal = 4,
    
    /// <summary>Request has been escalated</summary>
    Escalated = 5,
    
    /// <summary>Request has been resolved</summary>
    Resolved = 6,
    
    /// <summary>Request has been closed after resolution</summary>
    Closed = 7,
    
    /// <summary>Request was cancelled</summary>
    Cancelled = 8,
    
    /// <summary>Request is on hold</summary>
    OnHold = 9,
    
    /// <summary>Reopened after being closed/resolved</summary>
    Reopened = 10
}

/// <summary>
/// Priority level of the service request
/// </summary>
public enum ServiceRequestPriority
{
    /// <summary>Low priority - can be handled in normal queue</summary>
    Low = 0,
    
    /// <summary>Medium priority - standard handling</summary>
    Medium = 1,
    
    /// <summary>High priority - expedited handling required</summary>
    High = 2,
    
    /// <summary>Critical priority - immediate attention required</summary>
    Critical = 3,
    
    /// <summary>Urgent priority - business critical</summary>
    Urgent = 4
}

/// <summary>
/// Type of custom field
/// </summary>
public enum CustomFieldType
{
    /// <summary>Single line text input</summary>
    Text = 0,
    
    /// <summary>Multi-line text area</summary>
    TextArea = 1,
    
    /// <summary>Numeric input</summary>
    Number = 2,
    
    /// <summary>Decimal/currency input</summary>
    Decimal = 3,
    
    /// <summary>Date picker</summary>
    Date = 4,
    
    /// <summary>Date and time picker</summary>
    DateTime = 5,
    
    /// <summary>Single select dropdown</summary>
    Dropdown = 6,
    
    /// <summary>Multi-select dropdown</summary>
    MultiSelect = 7,
    
    /// <summary>Boolean checkbox</summary>
    Boolean = 8,
    
    /// <summary>Email input</summary>
    Email = 9,
    
    /// <summary>Phone number input</summary>
    Phone = 10,
    
    /// <summary>URL input</summary>
    Url = 11
}

#endregion

#region Service Request Category

/// <summary>
/// Category for organizing service requests
/// </summary>
public class ServiceRequestCategory : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>Display order in lists</summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>Whether this category is active</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Icon name for UI display</summary>
    [MaxLength(50)]
    public string? IconName { get; set; }
    
    /// <summary>Color code for UI display</summary>
    [MaxLength(20)]
    public string? ColorCode { get; set; }
    
    /// <summary>Default SLA response time in hours</summary>
    public int? DefaultResponseTimeHours { get; set; }
    
    /// <summary>Default SLA resolution time in hours</summary>
    public int? DefaultResolutionTimeHours { get; set; }
    
    // Navigation properties
    public virtual ICollection<ServiceRequestSubcategory> Subcategories { get; set; } = new List<ServiceRequestSubcategory>();
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    public virtual ICollection<ServiceRequestType> ServiceRequestTypes { get; set; } = new List<ServiceRequestType>();
}

#endregion

#region Service Request Subcategory

/// <summary>
/// Subcategory under a service request category
/// </summary>
public class ServiceRequestSubcategory : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>Parent category ID</summary>
    public int CategoryId { get; set; }
    
    /// <summary>Display order in lists</summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>Whether this subcategory is active</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Override SLA response time in hours (optional)</summary>
    public int? ResponseTimeHours { get; set; }
    
    /// <summary>Override SLA resolution time in hours (optional)</summary>
    public int? ResolutionTimeHours { get; set; }
    
    /// <summary>Default priority for requests in this subcategory</summary>
    public ServiceRequestPriority? DefaultPriority { get; set; }
    
    /// <summary>Default workflow to apply</summary>
    public int? DefaultWorkflowId { get; set; }
    
    // Navigation properties
    [ForeignKey("CategoryId")]
    public virtual ServiceRequestCategory? Category { get; set; }
    
    [ForeignKey("DefaultWorkflowId")]
    public virtual Workflow? DefaultWorkflow { get; set; }
    
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    public virtual ICollection<ServiceRequestType> ServiceRequestTypes { get; set; } = new List<ServiceRequestType>();
}

#endregion

#region Service Request Type

/// <summary>
/// Defines a specific type of service request within a subcategory.
/// Contains templates for handling specific service request scenarios including
/// descriptions, workflows, possible resolutions, and customer resolution options.
/// </summary>
public class ServiceRequestType : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Type classification: Complaint or Request</summary>
    [Required]
    [MaxLength(50)]
    public string RequestType { get; set; } = "Request";
    
    /// <summary>Detailed description explaining when this type applies</summary>
    [MaxLength(2000)]
    public string? DetailedDescription { get; set; }
    
    /// <summary>Name of the workflow to apply for this request type</summary>
    [MaxLength(200)]
    public string? WorkflowName { get; set; }
    
    /// <summary>Possible technical/internal resolutions (semicolon-separated)</summary>
    [MaxLength(4000)]
    public string? PossibleResolutions { get; set; }
    
    /// <summary>Final customer-facing resolution options (semicolon-separated)</summary>
    [MaxLength(2000)]
    public string? FinalCustomerResolutions { get; set; }
    
    /// <summary>Parent category ID</summary>
    public int CategoryId { get; set; }
    
    /// <summary>Parent subcategory ID</summary>
    public int SubcategoryId { get; set; }
    
    /// <summary>Display order within subcategory</summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>Whether this type is active</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Default priority for this request type</summary>
    public ServiceRequestPriority? DefaultPriority { get; set; }
    
    /// <summary>Default SLA response time in hours</summary>
    public int? ResponseTimeHours { get; set; }
    
    /// <summary>Default SLA resolution time in hours</summary>
    public int? ResolutionTimeHours { get; set; }
    
    /// <summary>Tags for quick filtering (comma-separated)</summary>
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    // Navigation properties
    [ForeignKey("CategoryId")]
    public virtual ServiceRequestCategory? Category { get; set; }
    
    [ForeignKey("SubcategoryId")]
    public virtual ServiceRequestSubcategory? Subcategory { get; set; }
}

#endregion

#region Custom Field Definition

/// <summary>
/// Definition of a custom field for service requests
/// Supports up to 15 custom fields per configuration
/// </summary>
public class ServiceRequestCustomFieldDefinition : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FieldKey { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>Type of the field</summary>
    public CustomFieldType FieldType { get; set; } = CustomFieldType.Text;
    
    /// <summary>Whether this field is required</summary>
    public bool IsRequired { get; set; } = false;
    
    /// <summary>Whether this field is active</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Display order (1-15)</summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>Default value for the field</summary>
    [MaxLength(500)]
    public string? DefaultValue { get; set; }
    
    /// <summary>Placeholder text</summary>
    [MaxLength(200)]
    public string? Placeholder { get; set; }
    
    /// <summary>Help text to display</summary>
    [MaxLength(500)]
    public string? HelpText { get; set; }
    
    /// <summary>
    /// Dropdown options (JSON array for Dropdown/MultiSelect types)
    /// Example: ["Option 1", "Option 2", "Option 3"]
    /// </summary>
    [MaxLength(2000)]
    public string? DropdownOptions { get; set; }
    
    /// <summary>Minimum value (for Number/Decimal types)</summary>
    public decimal? MinValue { get; set; }
    
    /// <summary>Maximum value (for Number/Decimal types)</summary>
    public decimal? MaxValue { get; set; }
    
    /// <summary>Maximum length (for Text types)</summary>
    public int? MaxLength { get; set; }
    
    /// <summary>Regex pattern for validation</summary>
    [MaxLength(500)]
    public string? ValidationPattern { get; set; }
    
    /// <summary>Error message for validation failure</summary>
    [MaxLength(200)]
    public string? ValidationMessage { get; set; }
    
    /// <summary>Optional: Restrict to specific category</summary>
    public int? CategoryId { get; set; }
    
    /// <summary>Optional: Restrict to specific subcategory</summary>
    public int? SubcategoryId { get; set; }
    
    // Navigation properties
    [ForeignKey("CategoryId")]
    public virtual ServiceRequestCategory? Category { get; set; }
    
    [ForeignKey("SubcategoryId")]
    public virtual ServiceRequestSubcategory? Subcategory { get; set; }
    
    public virtual ICollection<ServiceRequestCustomFieldValue> FieldValues { get; set; } = new List<ServiceRequestCustomFieldValue>();
}

#endregion

#region Custom Field Value

/// <summary>
/// Stores the value of a custom field for a specific service request
/// </summary>
public class ServiceRequestCustomFieldValue : BaseEntity
{
    /// <summary>The service request this value belongs to</summary>
    public int ServiceRequestId { get; set; }
    
    /// <summary>The custom field definition</summary>
    public int CustomFieldDefinitionId { get; set; }
    
    /// <summary>Text value storage</summary>
    [MaxLength(4000)]
    public string? TextValue { get; set; }
    
    /// <summary>Numeric value storage</summary>
    public decimal? NumericValue { get; set; }
    
    /// <summary>Date value storage</summary>
    public DateTime? DateValue { get; set; }
    
    /// <summary>Boolean value storage</summary>
    public bool? BooleanValue { get; set; }
    
    // Navigation properties
    [ForeignKey("ServiceRequestId")]
    public virtual ServiceRequest? ServiceRequest { get; set; }
    
    [ForeignKey("CustomFieldDefinitionId")]
    public virtual ServiceRequestCustomFieldDefinition? CustomFieldDefinition { get; set; }
}

#endregion

#region Service Request Entity

/// <summary>
/// Main service request entity for managing customer service requests
/// across multiple channels
/// </summary>
public class ServiceRequest : BaseEntity
{
    #region Basic Information
    
    /// <summary>Unique ticket/case number</summary>
    [Required]
    [MaxLength(50)]
    public string TicketNumber { get; set; } = string.Empty;
    
    /// <summary>Subject/title of the service request</summary>
    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>Detailed description of the request</summary>
    [MaxLength(10000)]
    public string? Description { get; set; }
    
    /// <summary>Channel through which request was received</summary>
    public ServiceRequestChannel Channel { get; set; } = ServiceRequestChannel.SelfServicePortal;
    
    /// <summary>Current status of the request</summary>
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.New;
    
    /// <summary>Priority level</summary>
    public ServiceRequestPriority Priority { get; set; } = ServiceRequestPriority.Medium;
    
    #endregion
    
    #region Categorization
    
    /// <summary>Category of the service request</summary>
    public int? CategoryId { get; set; }
    
    /// <summary>Subcategory of the service request</summary>
    public int? SubcategoryId { get; set; }
    
    #endregion
    
    #region Customer & Contact Association
    
    /// <summary>Associated customer/account ID</summary>
    public int? CustomerId { get; set; }
    
    /// <summary>Associated contact ID</summary>
    public int? ContactId { get; set; }
    
    /// <summary>Requester name (for anonymous requests)</summary>
    [MaxLength(200)]
    public string? RequesterName { get; set; }
    
    /// <summary>Requester email</summary>
    [MaxLength(200)]
    public string? RequesterEmail { get; set; }
    
    /// <summary>Requester phone</summary>
    [MaxLength(50)]
    public string? RequesterPhone { get; set; }
    
    #endregion
    
    #region Assignment & Ownership
    
    /// <summary>Assigned user/agent ID</summary>
    public int? AssignedToUserId { get; set; }
    
    /// <summary>Assigned team/group ID</summary>
    public int? AssignedToGroupId { get; set; }
    
    /// <summary>User who created the request</summary>
    public int? CreatedByUserId { get; set; }
    
    /// <summary>User who last modified the request</summary>
    public int? LastModifiedByUserId { get; set; }
    
    #endregion
    
    #region Workflow
    
    /// <summary>Applied workflow ID</summary>
    public int? WorkflowId { get; set; }
    
    /// <summary>Current workflow step/stage</summary>
    [MaxLength(100)]
    public string? CurrentWorkflowStep { get; set; }
    
    /// <summary>Workflow execution ID for tracking</summary>
    public int? WorkflowExecutionId { get; set; }
    
    #endregion
    
    #region SLA & Timing
    
    /// <summary>Due date for first response</summary>
    public DateTime? ResponseDueDate { get; set; }
    
    /// <summary>Due date for resolution</summary>
    public DateTime? ResolutionDueDate { get; set; }
    
    /// <summary>Actual first response date</summary>
    public DateTime? FirstResponseDate { get; set; }
    
    /// <summary>Actual resolution date</summary>
    public DateTime? ResolvedDate { get; set; }
    
    /// <summary>Date when request was closed</summary>
    public DateTime? ClosedDate { get; set; }
    
    /// <summary>Whether SLA for response was breached</summary>
    public bool ResponseSlaBreached { get; set; } = false;
    
    /// <summary>Whether SLA for resolution was breached</summary>
    public bool ResolutionSlaBreached { get; set; } = false;
    
    #endregion
    
    #region Channel-Specific Information
    
    /// <summary>External reference ID (e.g., email message ID, WhatsApp message ID)</summary>
    [MaxLength(500)]
    public string? ExternalReferenceId { get; set; }
    
    /// <summary>Source phone number (for phone/WhatsApp channels)</summary>
    [MaxLength(50)]
    public string? SourcePhoneNumber { get; set; }
    
    /// <summary>Source email address (for email channel)</summary>
    [MaxLength(200)]
    public string? SourceEmailAddress { get; set; }
    
    /// <summary>Conversation/thread ID for tracking</summary>
    [MaxLength(500)]
    public string? ConversationId { get; set; }
    
    #endregion
    
    #region Resolution & Feedback
    
    /// <summary>Resolution summary</summary>
    [MaxLength(5000)]
    public string? ResolutionSummary { get; set; }
    
    /// <summary>Resolution code/type</summary>
    [MaxLength(100)]
    public string? ResolutionCode { get; set; }
    
    /// <summary>Root cause (if applicable)</summary>
    [MaxLength(1000)]
    public string? RootCause { get; set; }
    
    /// <summary>Customer satisfaction rating (1-5)</summary>
    public int? SatisfactionRating { get; set; }
    
    /// <summary>Customer feedback comments</summary>
    [MaxLength(2000)]
    public string? CustomerFeedback { get; set; }
    
    #endregion
    
    #region Related Entities
    
    /// <summary>Related opportunity ID</summary>
    public int? RelatedOpportunityId { get; set; }
    
    /// <summary>Related product ID</summary>
    public int? RelatedProductId { get; set; }
    
    /// <summary>Parent service request (for linked cases)</summary>
    public int? ParentServiceRequestId { get; set; }
    
    #endregion
    
    #region Additional Information
    
    /// <summary>Tags for categorization (comma-separated)</summary>
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    /// <summary>Internal notes (not visible to customer)</summary>
    [MaxLength(5000)]
    public string? InternalNotes { get; set; }
    
    /// <summary>Escalation level (0 = not escalated)</summary>
    public int EscalationLevel { get; set; } = 0;
    
    /// <summary>Number of times reopened</summary>
    public int ReopenCount { get; set; } = 0;
    
    /// <summary>Whether this is a VIP/priority customer</summary>
    public bool IsVipCustomer { get; set; } = false;
    
    /// <summary>Estimated effort in hours</summary>
    public decimal? EstimatedEffortHours { get; set; }
    
    /// <summary>Actual effort in hours</summary>
    public decimal? ActualEffortHours { get; set; }
    
    #endregion
    
    #region Navigation Properties
    
    [ForeignKey("CategoryId")]
    public virtual ServiceRequestCategory? Category { get; set; }
    
    [ForeignKey("SubcategoryId")]
    public virtual ServiceRequestSubcategory? Subcategory { get; set; }
    
    [ForeignKey("CustomerId")]
    public virtual Customer? Customer { get; set; }
    
    [ForeignKey("ContactId")]
    public virtual Models.Contact? Contact { get; set; }
    
    [ForeignKey("AssignedToUserId")]
    public virtual User? AssignedToUser { get; set; }
    
    [ForeignKey("AssignedToGroupId")]
    public virtual UserGroup? AssignedToGroup { get; set; }
    
    [ForeignKey("CreatedByUserId")]
    public virtual User? CreatedByUser { get; set; }
    
    [ForeignKey("WorkflowId")]
    public virtual Workflow? Workflow { get; set; }
    
    [ForeignKey("WorkflowExecutionId")]
    public virtual WorkflowExecution? WorkflowExecution { get; set; }
    
    [ForeignKey("RelatedOpportunityId")]
    public virtual Opportunity? RelatedOpportunity { get; set; }
    
    [ForeignKey("RelatedProductId")]
    public virtual Product? RelatedProduct { get; set; }
    
    [ForeignKey("ParentServiceRequestId")]
    public virtual ServiceRequest? ParentServiceRequest { get; set; }
    
    public virtual ICollection<ServiceRequest> ChildServiceRequests { get; set; } = new List<ServiceRequest>();
    public virtual ICollection<ServiceRequestCustomFieldValue> CustomFieldValues { get; set; } = new List<ServiceRequestCustomFieldValue>();
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    
    #endregion
    
    #region Computed Properties
    
    /// <summary>Whether the request is still open</summary>
    [NotMapped]
    public bool IsOpen => Status != ServiceRequestStatus.Closed && 
                          Status != ServiceRequestStatus.Cancelled && 
                          Status != ServiceRequestStatus.Resolved;
    
    /// <summary>Age of the request in hours</summary>
    [NotMapped]
    public double AgeInHours => (DateTime.UtcNow - CreatedAt).TotalHours;
    
    /// <summary>Time to first response in hours (if responded)</summary>
    [NotMapped]
    public double? TimeToFirstResponseHours => FirstResponseDate.HasValue 
        ? (FirstResponseDate.Value - CreatedAt).TotalHours 
        : null;
    
    /// <summary>Time to resolution in hours (if resolved)</summary>
    [NotMapped]
    public double? TimeToResolutionHours => ResolvedDate.HasValue 
        ? (ResolvedDate.Value - CreatedAt).TotalHours 
        : null;
    
    /// <summary>Whether response SLA is at risk</summary>
    [NotMapped]
    public bool IsResponseSlaAtRisk => !FirstResponseDate.HasValue && 
                                        ResponseDueDate.HasValue && 
                                        ResponseDueDate.Value <= DateTime.UtcNow.AddHours(2);
    
    /// <summary>Whether resolution SLA is at risk</summary>
    [NotMapped]
    public bool IsResolutionSlaAtRisk => !ResolvedDate.HasValue && 
                                          ResolutionDueDate.HasValue && 
                                          ResolutionDueDate.Value <= DateTime.UtcNow.AddHours(4);
    
    #endregion
}

#endregion
