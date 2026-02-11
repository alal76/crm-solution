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
