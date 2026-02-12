// -----------------------------------------------------------------------
// CRM Solution - Semantic Kernel AI Plugins
// Copyright (c) 2024-2026 Abhishek Lal (CRM Solution). All rights reserved.
// Licensed under the GNU Affero General Public License v3.0.
// See LICENSE file in the project root for full license information.
//
// This file is part of the CRM Solution, an enterprise-grade
// Customer Relationship Management system.
//
// Author: Abhishek Lal
// Repository: https://github.com/abhisheklal04/crm-solution
// Documentation: See /docs folder for architecture and API reference
//
// IMPORTANT: This is proprietary code. Unauthorized copying, modification,
// or distribution is strictly prohibited.
// -----------------------------------------------------------------------

#nullable enable

using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for CRM contract management operations.
/// Provides AI-accessible read-only functions for querying contracts, checking renewals, and viewing statistics.
/// </summary>
public class ContractPlugin : CrmPluginBase
{
    private readonly IContractService _contractService;

    /// <inheritdoc />
    public override string PluginName => "Contract";

    /// <inheritdoc />
    public override string Description => "Query CRM contracts — search contracts, view details, check expiring/renewal-due contracts, and view contract statistics.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ContractPlugin"/> class.
    /// </summary>
    /// <param name="contractService">The contract service for contract operations.</param>
    /// <param name="logger">The logger instance.</param>
    public ContractPlugin(
        IContractService contractService,
        ILogger<ContractPlugin> logger) : base(logger)
    {
        _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
    }

    #region Read Operations

    /// <summary>
    /// Retrieves a specific contract by its ID.
    /// </summary>
    /// <param name="contractId">The contract ID to retrieve.</param>
    /// <returns>A JSON object with the contract details.</returns>
    [KernelFunction("GetContract")]
    [Description("Get a specific contract by its ID including status, dates, and value.")]
    public async Task<string> GetContractAsync(
        [Description("The ID of the contract to retrieve")] int contractId)
    {
        try
        {
            var contract = await _contractService.GetByIdAsync(contractId);

            if (contract == null)
            {
                return SuccessResult(new { found = false, message = $"Contract {contractId} not found" });
            }

            return SuccessResult(new
            {
                contract.Id,
                contract.ContractNumber,
                contract.Name,
                contract.Description,
                Status = contract.Status.ToString(),
                ContractType = contract.ContractType.ToString(),
                contract.StartDate,
                contract.EndDate,
                contract.Value,
                contract.CurrencyCode,
                contract.AccountId,
                contract.AutoRenew,
                contract.RenewalNoticeDays,
                contract.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return ErrorResult("GetContract", ex.Message);
        }
    }

    /// <summary>
    /// Searches contracts by keyword or filters by customer and status.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by contract name, number, or description.</param>
    /// <param name="customerId">Optional customer ID to filter by.</param>
    /// <param name="status">Optional status filter (e.g., "Draft", "Active", "Expired", "Terminated").</param>
    /// <returns>A JSON array of matching contracts.</returns>
    [KernelFunction("SearchContracts")]
    [Description("Search contracts by keyword, customer, or status.")]
    public async Task<string> SearchContractsAsync(
        [Description("Search term for contract name, number, or description (optional)")] string? searchTerm = null,
        [Description("Customer ID to filter by (optional)")] int? customerId = null,
        [Description("Status filter: Draft, Active, Expired, Terminated, Suspended (optional)")] string? status = null)
    {
        try
        {
            IEnumerable<Contract> contracts;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                contracts = await _contractService.SearchAsync(searchTerm);
            }
            else
            {
                ContractStatus? parsedStatus = null;
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<ContractStatus>(status, true, out var cs))
                {
                    parsedStatus = cs;
                }

                contracts = await _contractService.GetAllAsync(
                    customerId: customerId,
                    status: parsedStatus);
            }

            var summaries = contracts.Select(c => new
            {
                c.Id,
                c.ContractNumber,
                c.Name,
                Status = c.Status.ToString(),
                ContractType = c.ContractType.ToString(),
                c.StartDate,
                c.EndDate,
                c.Value,
                c.AccountId,
                c.AutoRenew
            });

            return SuccessResult(summaries);
        }
        catch (Exception ex)
        {
            return ErrorResult("SearchContracts", ex.Message);
        }
    }

    /// <summary>
    /// Retrieves contracts that are expiring within a specified number of days.
    /// </summary>
    /// <param name="withinDays">Number of days to look ahead for expiring contracts. Defaults to 30.</param>
    /// <returns>A JSON array of contracts expiring within the specified period.</returns>
    [KernelFunction("GetExpiringContracts")]
    [Description("Get contracts that are expiring within a specified number of days.")]
    public async Task<string> GetExpiringContractsAsync(
        [Description("Number of days to look ahead for expiring contracts")] int withinDays = 30)
    {
        try
        {
            var contracts = await _contractService.GetContractsDueForRenewalAsync(withinDays);

            var summaries = contracts.Select(c => new
            {
                c.Id,
                c.ContractNumber,
                c.Name,
                Status = c.Status.ToString(),
                c.EndDate,
                c.Value,
                c.AccountId,
                c.AutoRenew,
                DaysUntilExpiry = (int)(c.EndDate - DateTime.UtcNow).TotalDays
            });

            return SuccessResult(summaries);
        }
        catch (Exception ex)
        {
            return ErrorResult("GetExpiringContracts", ex.Message);
        }
    }

    /// <summary>
    /// Retrieves active contracts for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID to get active contracts for.</param>
    /// <returns>A JSON array of the customer's active contracts.</returns>
    [KernelFunction("GetActiveContracts")]
    [Description("Get all active contracts for a specific customer.")]
    public async Task<string> GetActiveContractsAsync(
        [Description("Customer ID to get active contracts for")] int customerId)
    {
        try
        {
            var contracts = await _contractService.GetActiveContractsAsync(customerId);

            var summaries = contracts.Select(c => new
            {
                c.Id,
                c.ContractNumber,
                c.Name,
                ContractType = c.ContractType.ToString(),
                c.StartDate,
                c.EndDate,
                c.Value,
                c.AutoRenew
            });

            return SuccessResult(summaries);
        }
        catch (Exception ex)
        {
            return ErrorResult("GetActiveContracts", ex.Message);
        }
    }

    /// <summary>
    /// Retrieves contract statistics for a given date range.
    /// </summary>
    /// <param name="daysBack">Number of days to look back for statistics. Defaults to 90.</param>
    /// <returns>A JSON object with contract statistics including counts, values, and renewal rates.</returns>
    [KernelFunction("GetContractStatistics")]
    [Description("Get contract statistics including counts by status, total values, and renewal rates for a given period.")]
    public async Task<string> GetContractStatisticsAsync(
        [Description("Number of days to look back for statistics")] int daysBack = 90)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-daysBack);
            var stats = await _contractService.GetStatisticsAsync(fromDate, DateTime.UtcNow);

            return SuccessResult(stats);
        }
        catch (Exception ex)
        {
            return ErrorResult("GetContractStatistics", ex.Message);
        }
    }

    #endregion
}
