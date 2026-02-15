// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for webhook management operations.
/// Handles webhook registration, delivery tracking, and management.
/// </summary>
public interface IWebhookManagementService
{
    #region Webhook CRUD

    /// <summary>Gets all registered webhooks.</summary>
    Task<IEnumerable<WebhookDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a webhook by ID.</summary>
    Task<WebhookDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new webhook registration.</summary>
    Task<WebhookDto> CreateAsync(CreateWebhookDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates a webhook registration.</summary>
    Task<WebhookDto> UpdateAsync(int id, UpdateWebhookDto dto, CancellationToken cancellationToken = default);

    /// <summary>Deletes a webhook registration (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Webhook Management

    /// <summary>Toggles webhook active/inactive status.</summary>
    Task<WebhookDto> ToggleActiveAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Tests webhook delivery with a test payload.</summary>
    Task<WebhookTestResultDto> TestAsync(int id, WebhookTestDto testData, CancellationToken cancellationToken = default);

    #endregion

    #region Delivery Tracking

    /// <summary>Gets delivery history for a webhook.</summary>
    Task<WebhookDeliveryHistoryDto> GetDeliveriesAsync(int webhookId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>Gets details of a specific delivery.</summary>
    Task<WebhookDeliveryDto?> GetDeliveryDetailAsync(int webhookId, int deliveryId, CancellationToken cancellationToken = default);

    /// <summary>Retries a failed delivery.</summary>
    Task<WebhookDeliveryDto> RetryDeliveryAsync(int webhookId, int deliveryId, CancellationToken cancellationToken = default);

    /// <summary>Gets statistics for a webhook.</summary>
    Task<WebhookStatisticsDto> GetStatisticsAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Event Management

    /// <summary>Gets list of available webhook events.</summary>
    Task<IEnumerable<WebhookEventDto>> GetAvailableEventsAsync(CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Service interface for webhook delivery/dispatch operations.
/// Handles dispatching webhook payloads to registered endpoints.
/// </summary>
public interface IWebhookDispatcherService
{
    /// <summary>Dispatches an event to relevant webhooks.</summary>
    Task DispatchAsync(string eventType, object payload, CancellationToken cancellationToken = default);

    /// <summary>Dispatches multiple events in batch.</summary>
    Task DispatchBatchAsync(List<(string EventType, object Payload)> events, CancellationToken cancellationToken = default);

    /// <summary>Processes pending webhook queue.</summary>
    Task ProcessQueueAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for campaign execution operations.
/// Handles campaign launching, pausing, and delivery.
/// </summary>
public interface ICampaignExecutionService
{
    /// <summary>Executes/launches a campaign to recipients.</summary>
    Task<CampaignExecutionResultDto> ExecuteAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>Pauses an active campaign.</summary>
    Task<bool> PauseAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>Resumes a paused campaign.</summary>
    Task<bool> ResumeAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>Schedules a campaign for future execution.</summary>
    Task<bool> ScheduleAsync(int campaignId, DateTime scheduledDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for campaign recipient management.
/// Handles targeting, filtering, and recipient list operations.
/// </summary>
public interface ICampaignRecipientService
{
    /// <summary>Gets recipients for a campaign.</summary>
    Task<List<CampaignRecipientDto>> GetRecipientsAsync(int campaignId, int page = 1, int pageSize = 100, CancellationToken cancellationToken = default);

    /// <summary>Adds recipients to a campaign.</summary>
    Task<int> AddRecipientsAsync(int campaignId, AddCampaignRecipientsDto dto, CancellationToken cancellationToken = default);

    /// <summary>Removes a recipient from a campaign.</summary>
    Task<bool> RemoveRecipientAsync(int campaignId, int recipientId, CancellationToken cancellationToken = default);

    /// <summary>Filters and targets recipients based on criteria.</summary>
    Task<List<CampaignRecipientDto>> FilterAsync(int campaignId, string criteria, CancellationToken cancellationToken = default);

    /// <summary>Gets total recipient count.</summary>
    Task<int> GetCountAsync(int campaignId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for campaign metrics and analytics.
/// Handles performance tracking and ROI calculations.
/// </summary>
public interface ICampaignMetricsService
{
    /// <summary>Gets metrics for a campaign.</summary>
    Task<CampaignMetricsDto> GetMetricsAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>Gets analysis and insights for a campaign.</summary>
    Task<CampaignAnalysisDto> AnalyzeAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>Generates preview of campaign content.</summary>
    Task<CampaignPreviewDto> PreviewAsync(int campaignId, CancellationToken cancellationToken = default);

    /// <summary>Clones/duplicates a campaign.</summary>
    Task<int> DuplicateAsync(int campaignId, DuplicateCampaignDto dto, CancellationToken cancellationToken = default);

    /// <summary>Retargets a campaign to different audience.</summary>
    Task<bool> RetargetAsync(int campaignId, RetargetCampaignDto dto, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for commission calculation operations.
/// Handles complex commission calculations with tiers, accelerators, and splits.
/// </summary>
public interface ICommissionCalculationService
{
    /// <summary>Calculates commission for an opportunity/deal.</summary>
    Task<CommissionCalculationResultDto> CalculateDealAsync(int opportunityId, int? planId = null, CancellationToken cancellationToken = default);

    /// <summary>Calculates commission for an order.</summary>
    Task<CommissionCalculationResultDto> CalculateOrderAsync(int orderId, int? planId = null, CancellationToken cancellationToken = default);

    /// <summary>Calculates commission for a period (monthly, quarterly, etc).</summary>
    Task<CommissionStatisticsDto> CalculatePeriodAsync(int userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>Applies tier-based calculations.</summary>
    Task<decimal> ApplyTierAsync(int planId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>Applies accelerator/bonus calculations.</summary>
    Task<decimal> ApplyAcceleratorAsync(int planId, decimal baseAmount, decimal achievementPercent, CancellationToken cancellationToken = default);

    /// <summary>Validates commission calculation against business rules.</summary>
    Task<bool> ValidateAsync(CommissionCalculationResultDto calculation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for commission approval workflows.
/// Handles multi-level approvals and audit trails.
/// </summary>
public interface ICommissionApprovalService
{
    /// <summary>Approves a commission for payout.</summary>
    Task<bool> ApproveAsync(int commissionId, int approvedById, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>Rejects a commission with reason.</summary>
    Task<bool> RejectAsync(int commissionId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Gets pending approvals for a reviewer.</summary>
    Task<List<CommissionDto>> GetPendingAsync(int reviewerId, CancellationToken cancellationToken = default);

    /// <summary>Gets approval history for a commission.</summary>
    Task<List<object>> GetHistoryAsync(int commissionId, CancellationToken cancellationToken = default);

    /// <summary>Bulk approves multiple commissions.</summary>
    Task<int> BulkApproveAsync(List<int> commissionIds, int approvedById, CancellationToken cancellationToken = default);

    /// <summary>Sends approval notification.</summary>
    Task<bool> NotifyAsync(int commissionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for commission payout operations.
/// Handles payments, reconciliation, and financial integration.
/// </summary>
public interface ICommissionPayoutService
{
    /// <summary>Marks commission as paid.</summary>
    Task<bool> MarkPaidAsync(int commissionId, DateTime? paidDate = null, string? reference = null, CancellationToken cancellationToken = default);

    /// <summary>Claws back a paid commission.</summary>
    Task<bool> ClawbackAsync(int commissionId, string reason, decimal? amount = null, CancellationToken cancellationToken = default);

    /// <summary>Generates payout statement.</summary>
    Task<CommissionStatementDto> GenerateStatementAsync(int userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>Finalizes a statement for processing.</summary>
    Task<bool> FinalizeStatementAsync(int statementId, CancellationToken cancellationToken = default);

    /// <summary>Reconciles payouts against financial records.</summary>
    Task<bool> ReconcileAsync(int statementId, CancellationToken cancellationToken = default);

    /// <summary>Gets payout schedule/calendar.</summary>
    Task<List<object>> GetPayoutScheduleAsync(int userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for email sequence operations.
/// Handles sequence CRUD, enrollment, and execution.
/// </summary>
public interface IEmailSequenceManagementService
{
    #region Sequence CRUD

    /// <summary>Gets all email sequences.</summary>
    Task<IEnumerable<EmailSequenceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a sequence by ID.</summary>
    Task<EmailSequenceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new email sequence.</summary>
    Task<EmailSequenceDto> CreateAsync(CreateEmailSequenceDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing sequence.</summary>
    Task<EmailSequenceDto> UpdateAsync(int id, UpdateEmailSequenceDto dto, CancellationToken cancellationToken = default);

    /// <summary>Deletes a sequence (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Steps

    /// <summary>Adds a step to a sequence.</summary>
    Task<EmailSequenceStepDto> AddStepAsync(int sequenceId, CreateEmailSequenceStepDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates a sequence step.</summary>
    Task<EmailSequenceStepDto> UpdateStepAsync(int sequenceId, int stepId, CreateEmailSequenceStepDto dto, CancellationToken cancellationToken = default);

    /// <summary>Removes a step from sequence.</summary>
    Task<bool> RemoveStepAsync(int sequenceId, int stepId, CancellationToken cancellationToken = default);

    /// <summary>Reorders steps in a sequence.</summary>
    Task<bool> ReorderStepsAsync(int sequenceId, List<int> stepOrder, CancellationToken cancellationToken = default);

    #endregion

    #region Enrollments

    /// <summary>Enrolls a contact/lead in a sequence.</summary>
    Task<EmailSequenceEnrollmentDto> EnrollAsync(int sequenceId, CreateEmailSequenceEnrollmentDto dto, CancellationToken cancellationToken = default);

    /// <summary>Gets enrollments for a sequence.</summary>
    Task<List<EmailSequenceEnrollmentDto>> GetEnrollmentsAsync(int sequenceId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>Pauses an enrollment.</summary>
    Task<bool> PauseEnrollmentAsync(int sequenceId, int enrollmentId, CancellationToken cancellationToken = default);

    /// <summary>Resumes a paused enrollment.</summary>
    Task<bool> ResumeEnrollmentAsync(int sequenceId, int enrollmentId, CancellationToken cancellationToken = default);

    /// <summary>Exits an enrollment from sequence.</summary>
    Task<bool> ExitEnrollmentAsync(int sequenceId, int enrollmentId, string? reason = null, CancellationToken cancellationToken = default);

    #endregion

    #region Execution & Analytics

    /// <summary>Gets sequence analytics/metrics.</summary>
    Task<EmailSequenceAnalyticsDto> GetAnalyticsAsync(int sequenceId, CancellationToken cancellationToken = default);

    /// <summary>Executes pending steps in sequence.</summary>
    Task<EmailSequenceExecutionResultDto> ExecuteAsync(int sequenceId, CancellationToken cancellationToken = default);

    /// <summary>Clones/duplicates a sequence.</summary>
    Task<int> DuplicateAsync(int sequenceId, string newName, CancellationToken cancellationToken = default);

    #endregion
}
