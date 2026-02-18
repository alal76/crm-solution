// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Interfaces;

namespace CRM.Core.Ports.Input;

/// <summary>
/// Input port for Customer domain.
/// Inherits from IAccountService to maintain backward compatibility.
/// </summary>
public interface ICustomerInputPort : IAccountService { }

/// <summary>
/// Input port for Contact domain.
/// </summary>
public interface IContactInputPort : IContactsService { }

/// <summary>
/// Input port for Opportunity domain (Sales Pipeline).
/// </summary>
public interface IOpportunityInputPort : IOpportunityService { }

/// <summary>
/// Input port for Product domain.
/// </summary>
public interface IProductInputPort : IProductService { }

/// <summary>
/// Input port for Marketing Campaign domain.
/// </summary>
public interface ICampaignInputPort : IMarketingCampaignService { }

/// <summary>
/// Input port for Authentication domain.
/// </summary>
public interface IAuthInputPort : IAuthenticationService { }

/// <summary>
/// Input port for User Management domain.
/// </summary>
public interface IUserInputPort : IUserService { }

/// <summary>
/// Input port for User Group domain.
/// </summary>
public interface IUserGroupInputPort : IUserGroupService { }

/// <summary>
/// Input port for System Settings domain.
/// </summary>
public interface ISystemSettingsInputPort : ISystemSettingsService { }

/// <summary>
/// Input port for Service Request domain.
/// </summary>
public interface IServiceRequestInputPort : IServiceRequestService { }

/// <summary>
/// Input port for Account domain.
/// </summary>
public interface IAccountInputPort : IAccountService { }

/// <summary>
/// Input port for Database Backup operations.
/// </summary>
public interface IDatabaseBackupInputPort : IDatabaseBackupService { }
/// <summary>
/// Input port for Commission Plan domain.
/// </summary>
public interface ICommissionPlanInputPort : ICommissionPlanService { }

/// <summary>
/// Input port for Commission Calculation domain.
/// </summary>
public interface ICommissionCalculationInputPort : ICommissionCalculationService { }

/// <summary>
/// Input port for Commission Approval domain.
/// </summary>
public interface ICommissionApprovalInputPort : ICommissionApprovalService { }

/// <summary>
/// Input port for Commission Payout domain.
/// </summary>
public interface ICommissionPayoutInputPort : ICommissionPayoutService { }

/// <summary>
/// Input port for Campaign Recipient Management domain.
/// </summary>
public interface ICampaignRecipientInputPort : ICampaignRecipientService { }

/// <summary>
/// Input port for Campaign Metrics domain.
/// </summary>
public interface ICampaignMetricsInputPort : ICampaignMetricsService { }

/// <summary>
/// Input port for Campaign Execution domain.
/// </summary>
public interface ICampaignExecutionInputPort : ICampaignExecutionService { }

/// <summary>
/// Input port for Email Sequence Management domain.
/// </summary>
public interface IEmailSequenceManagementInputPort : IEmailSequenceManagementService { }

/// <summary>
/// Input port for Webhook Management domain.
/// </summary>
public interface IWebhookManagementInputPort : IWebhookManagementService { }

/// <summary>
/// Input port for Webhook Dispatcher domain.
/// </summary>
public interface IWebhookDispatcherInputPort : IWebhookDispatcherService { }