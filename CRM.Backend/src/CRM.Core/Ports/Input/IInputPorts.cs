// CRM Solution - Hexagonal Architecture
// Input Ports (Driving Ports)
//
// HEXAGONAL ARCHITECTURE NOTE:
// Input ports define the use cases that external actors can trigger.
// These interfaces alias/extend existing service interfaces for consistency.
// Controllers (primary adapters) depend on these ports, not on concrete services.
//
// Pattern: [Controller] → [Input Port] → [Service Implementation] → [Output Port] → [Repository/External]

using CRM.Core.Interfaces;

namespace CRM.Core.Ports.Input;

/// <summary>
/// Input port for Customer domain.
/// Inherits from ICustomerService to maintain backward compatibility.
/// </summary>
public interface ICustomerInputPort : ICustomerService { }

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
