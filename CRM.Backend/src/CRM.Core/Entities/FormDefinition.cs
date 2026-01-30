using CRM.Core.Models;

namespace CRM.Core.Entities;

#region Form Enumerations

/// <summary>
/// FUNCTIONAL: Form field type.
/// TECHNICAL: Determines rendering and validation.
/// </summary>
public enum FormFieldType
{
    /// <summary>Single line text</summary>
    Text = 0,
    
    /// <summary>Multi-line text</summary>
    TextArea = 1,
    
    /// <summary>Email address</summary>
    Email = 2,
    
    /// <summary>Phone number</summary>
    Phone = 3,
    
    /// <summary>Number input</summary>
    Number = 4,
    
    /// <summary>Date picker</summary>
    Date = 5,
    
    /// <summary>Date and time picker</summary>
    DateTime = 6,
    
    /// <summary>Dropdown select</summary>
    Dropdown = 7,
    
    /// <summary>Multiple select</summary>
    MultiSelect = 8,
    
    /// <summary>Radio buttons</summary>
    Radio = 9,
    
    /// <summary>Checkboxes</summary>
    Checkbox = 10,
    
    /// <summary>File upload</summary>
    FileUpload = 11,
    
    /// <summary>Hidden field</summary>
    Hidden = 12,
    
    /// <summary>Country picker</summary>
    Country = 13,
    
    /// <summary>State/region picker</summary>
    State = 14,
    
    /// <summary>URL input</summary>
    Url = 15,
    
    /// <summary>Rating scale</summary>
    Rating = 16,
    
    /// <summary>Range slider</summary>
    Range = 17,
    
    /// <summary>Consent checkbox</summary>
    Consent = 18,
    
    /// <summary>CAPTCHA verification</summary>
    Captcha = 19,
    
    /// <summary>Heading/label only</summary>
    Heading = 20,
    
    /// <summary>Paragraph text (not input)</summary>
    Paragraph = 21,
    
    /// <summary>Divider/separator</summary>
    Divider = 22
}

/// <summary>
/// FUNCTIONAL: Form status lifecycle.
/// TECHNICAL: Controls whether form accepts submissions.
/// </summary>
public enum FormStatus
{
    /// <summary>Form is in draft mode</summary>
    Draft = 0,
    
    /// <summary>Form is published and active</summary>
    Published = 1,
    
    /// <summary>Form is paused</summary>
    Paused = 2,
    
    /// <summary>Form is archived</summary>
    Archived = 3
}

/// <summary>
/// FUNCTIONAL: What happens after form submission.
/// TECHNICAL: Determines redirect/message behavior.
/// </summary>
public enum FormSubmitAction
{
    /// <summary>Show a thank you message</summary>
    ShowMessage = 0,
    
    /// <summary>Redirect to URL</summary>
    Redirect = 1,
    
    /// <summary>Show another form</summary>
    ShowForm = 2,
    
    /// <summary>Stay on page</summary>
    StayOnPage = 3
}

/// <summary>
/// FUNCTIONAL: Submission processing status.
/// TECHNICAL: Controls workflow and notifications.
/// </summary>
public enum SubmissionStatus
{
    /// <summary>New submission, not processed</summary>
    New = 0,
    
    /// <summary>Submission is being processed</summary>
    Processing = 1,
    
    /// <summary>Lead created successfully</summary>
    LeadCreated = 2,
    
    /// <summary>Contact created/updated</summary>
    ContactCreated = 3,
    
    /// <summary>Submitted to external system</summary>
    SubmittedExternal = 4,
    
    /// <summary>Processing failed</summary>
    Failed = 5,
    
    /// <summary>Marked as spam</summary>
    Spam = 6,
    
    /// <summary>Duplicate submission ignored</summary>
    Duplicate = 7
}

#endregion

/// <summary>
/// Form definition for lead capture and data collection.
/// Drag-and-drop form builder with field configuration.
/// </summary>
public class FormDefinition : BaseEntity
{
    #region Identification
    
    /// <summary>Form name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Internal form identifier</summary>
    public string FormKey { get; set; } = string.Empty;
    
    /// <summary>Form description</summary>
    public string? Description { get; set; }
    
    /// <summary>Current status</summary>
    public FormStatus Status { get; set; } = FormStatus.Draft;
    
    #endregion
    
    #region Display Settings
    
    /// <summary>Form title (displayed to user)</summary>
    public string? Title { get; set; }
    
    /// <summary>Form subtitle</summary>
    public string? Subtitle { get; set; }
    
    /// <summary>Submit button text</summary>
    public string SubmitButtonText { get; set; } = "Submit";
    
    /// <summary>Form width (px or %)</summary>
    public string? Width { get; set; }
    
    /// <summary>CSS classes for styling</summary>
    public string? CssClasses { get; set; }
    
    /// <summary>Custom CSS</summary>
    public string? CustomCss { get; set; }
    
    /// <summary>Custom JavaScript</summary>
    public string? CustomJs { get; set; }
    
    /// <summary>Theme/style name</summary>
    public string? Theme { get; set; }
    
    #endregion
    
    #region Submission Settings
    
    /// <summary>Action after submission</summary>
    public FormSubmitAction SubmitAction { get; set; } = FormSubmitAction.ShowMessage;
    
    /// <summary>Thank you message</summary>
    public string? ThankYouMessage { get; set; }
    
    /// <summary>Redirect URL after submission</summary>
    public string? RedirectUrl { get; set; }
    
    /// <summary>Enable double opt-in</summary>
    public bool DoubleOptIn { get; set; } = false;
    
    /// <summary>Double opt-in email template ID</summary>
    public int? DoubleOptInTemplateId { get; set; }
    
    /// <summary>Enable spam protection</summary>
    public bool SpamProtection { get; set; } = true;
    
    /// <summary>CAPTCHA type (recaptcha, hcaptcha, none)</summary>
    public string? CaptchaType { get; set; }
    
    /// <summary>Honeypot field name</summary>
    public string? HoneypotFieldName { get; set; }
    
    #endregion
    
    #region Lead Creation Settings
    
    /// <summary>Create lead on submission</summary>
    public bool CreateLead { get; set; } = true;
    
    /// <summary>Lead source for created leads</summary>
    public string? LeadSource { get; set; }
    
    /// <summary>Default lead owner ID</summary>
    public int? DefaultLeadOwnerId { get; set; }
    
    /// <summary>Lead routing rule ID</summary>
    public int? LeadRoutingRuleId { get; set; }
    
    /// <summary>Navigation to routing rule</summary>
    public LeadRoutingRule? LeadRoutingRule { get; set; }
    
    /// <summary>Update existing lead if found</summary>
    public bool UpdateExistingLead { get; set; } = true;
    
    /// <summary>Match field for existing lead (email, phone)</summary>
    public string? ExistingLeadMatchField { get; set; } = "Email";
    
    #endregion
    
    #region Campaign Integration
    
    /// <summary>Associated campaign ID</summary>
    public int? CampaignId { get; set; }
    
    /// <summary>Navigation to campaign</summary>
    public MarketingCampaign? Campaign { get; set; }
    
    /// <summary>Campaign member status on submission</summary>
    public string? CampaignMemberStatus { get; set; }
    
    #endregion
    
    #region Notifications
    
    /// <summary>Send notification to owner on submission</summary>
    public bool NotifyOwner { get; set; } = true;
    
    /// <summary>Additional notification recipients (comma-separated)</summary>
    public string? NotificationRecipients { get; set; }
    
    /// <summary>Notification email template ID</summary>
    public int? NotificationTemplateId { get; set; }
    
    /// <summary>Send autoresponder to submitter</summary>
    public bool SendAutoresponder { get; set; } = false;
    
    /// <summary>Autoresponder email template ID</summary>
    public int? AutoresponderTemplateId { get; set; }
    
    #endregion
    
    #region Embedding
    
    /// <summary>Embed code (auto-generated)</summary>
    public string? EmbedCode { get; set; }
    
    /// <summary>Direct URL for standalone form</summary>
    public string? DirectUrl { get; set; }
    
    /// <summary>Allowed domains for embedding (comma-separated)</summary>
    public string? AllowedDomains { get; set; }
    
    #endregion
    
    #region Statistics
    
    /// <summary>Total views</summary>
    public int TotalViews { get; set; } = 0;
    
    /// <summary>Total submissions</summary>
    public int TotalSubmissions { get; set; } = 0;
    
    /// <summary>Conversion rate</summary>
    public decimal ConversionRate => TotalViews > 0 ? (decimal)TotalSubmissions / TotalViews * 100 : 0;
    
    #endregion
    
    #region Relationships
    
    /// <summary>Owner user ID</summary>
    public int? OwnerId { get; set; }
    
    /// <summary>Navigation to owner</summary>
    public User? Owner { get; set; }
    
    /// <summary>Form fields</summary>
    public ICollection<FormField> Fields { get; set; } = new List<FormField>();
    
    /// <summary>Form submissions</summary>
    public ICollection<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();
    
    #endregion
}

/// <summary>
/// Individual field within a form.
/// </summary>
public class FormField : BaseEntity
{
    #region Identification
    
    /// <summary>Field name (internal)</summary>
    public string FieldName { get; set; } = string.Empty;
    
    /// <summary>Field label (displayed)</summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>Field type</summary>
    public FormFieldType FieldType { get; set; } = FormFieldType.Text;
    
    /// <summary>Display order</summary>
    public int Order { get; set; } = 0;
    
    #endregion
    
    #region Validation
    
    /// <summary>Whether field is required</summary>
    public bool IsRequired { get; set; } = false;
    
    /// <summary>Required validation message</summary>
    public string? RequiredMessage { get; set; }
    
    /// <summary>Minimum length</summary>
    public int? MinLength { get; set; }
    
    /// <summary>Maximum length</summary>
    public int? MaxLength { get; set; }
    
    /// <summary>Minimum value (for numbers)</summary>
    public decimal? MinValue { get; set; }
    
    /// <summary>Maximum value (for numbers)</summary>
    public decimal? MaxValue { get; set; }
    
    /// <summary>Regex pattern for validation</summary>
    public string? ValidationPattern { get; set; }
    
    /// <summary>Validation error message</summary>
    public string? ValidationMessage { get; set; }
    
    #endregion
    
    #region Display
    
    /// <summary>Placeholder text</summary>
    public string? Placeholder { get; set; }
    
    /// <summary>Help text</summary>
    public string? HelpText { get; set; }
    
    /// <summary>Default value</summary>
    public string? DefaultValue { get; set; }
    
    /// <summary>Width (full, half, third)</summary>
    public string? Width { get; set; } = "full";
    
    /// <summary>CSS classes</summary>
    public string? CssClasses { get; set; }
    
    /// <summary>Whether field is hidden</summary>
    public bool IsHidden { get; set; } = false;
    
    /// <summary>Whether field is read-only</summary>
    public bool IsReadOnly { get; set; } = false;
    
    #endregion
    
    #region Options (for select/radio/checkbox)
    
    /// <summary>Options (JSON array or newline-separated)</summary>
    public string? Options { get; set; }
    
    /// <summary>Option value field (for dynamic options)</summary>
    public string? OptionValueField { get; set; }
    
    /// <summary>Option label field (for dynamic options)</summary>
    public string? OptionLabelField { get; set; }
    
    /// <summary>Allow "Other" option</summary>
    public bool AllowOther { get; set; } = false;
    
    #endregion
    
    #region CRM Mapping
    
    /// <summary>Map to CRM field (Lead.FirstName, etc.)</summary>
    public string? CrmFieldMapping { get; set; }
    
    /// <summary>Entity to map to (Lead, Contact)</summary>
    public string? CrmEntityMapping { get; set; }
    
    #endregion
    
    #region Conditional Logic
    
    /// <summary>Enable conditional visibility</summary>
    public bool HasConditionalLogic { get; set; } = false;
    
    /// <summary>Conditional logic rules (JSON)</summary>
    public string? ConditionalLogic { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Parent form ID</summary>
    public int FormDefinitionId { get; set; }
    
    /// <summary>Navigation to form</summary>
    public FormDefinition? FormDefinition { get; set; }
    
    #endregion
}

/// <summary>
/// Individual form submission.
/// </summary>
public class FormSubmission : BaseEntity
{
    #region Submission Info
    
    /// <summary>Submission reference number</summary>
    public string SubmissionNumber { get; set; } = string.Empty;
    
    /// <summary>Submission timestamp</summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Processing status</summary>
    public SubmissionStatus Status { get; set; } = SubmissionStatus.New;
    
    /// <summary>Processing error message</summary>
    public string? ErrorMessage { get; set; }
    
    #endregion
    
    #region Submitted Data
    
    /// <summary>Form data (JSON)</summary>
    public string FormData { get; set; } = "{}";
    
    /// <summary>Raw form data as submitted</summary>
    public string? RawData { get; set; }
    
    #endregion
    
    #region Submitter Info
    
    /// <summary>Submitter IP address</summary>
    public string? IpAddress { get; set; }
    
    /// <summary>User agent</summary>
    public string? UserAgent { get; set; }
    
    /// <summary>Referrer URL</summary>
    public string? Referrer { get; set; }
    
    /// <summary>Page URL where form was submitted</summary>
    public string? PageUrl { get; set; }
    
    #endregion
    
    #region UTM Parameters
    
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }
    
    #endregion
    
    #region Processing Results
    
    /// <summary>Date processed</summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>Whether double opt-in was confirmed</summary>
    public bool OptInConfirmed { get; set; } = false;
    
    /// <summary>Opt-in confirmation date</summary>
    public DateTime? OptInConfirmedAt { get; set; }
    
    /// <summary>Spam score (0-100)</summary>
    public int? SpamScore { get; set; }
    
    /// <summary>Whether marked as spam</summary>
    public bool IsSpam { get; set; } = false;
    
    #endregion
    
    #region Relationships
    
    /// <summary>Form ID</summary>
    public int FormDefinitionId { get; set; }
    
    /// <summary>Navigation to form</summary>
    public FormDefinition? FormDefinition { get; set; }
    
    /// <summary>Created lead ID</summary>
    public int? LeadId { get; set; }
    
    /// <summary>Navigation to lead</summary>
    public Lead? Lead { get; set; }
    
    /// <summary>Created/updated contact ID</summary>
    public int? ContactId { get; set; }
    
    /// <summary>Navigation to contact</summary>
    public Contact? Contact { get; set; }
    
    /// <summary>Web visitor ID</summary>
    public int? WebVisitorId { get; set; }
    
    /// <summary>Navigation to web visitor</summary>
    public WebVisitor? WebVisitor { get; set; }
    
    #endregion
}
