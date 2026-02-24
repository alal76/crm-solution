// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Jobs;

/// <summary>
/// Background job for automatically expiring contracts when their EndDate passes.
/// Designed as a stub for Hangfire/Quartz integration.
/// </summary>
public class ContractExpirationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContractExpirationJob> _logger;

    public ContractExpirationJob(
        IServiceProvider serviceProvider,
        ILogger<ContractExpirationJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the contract expiration job.
    /// Finds active contracts past their end date and marks them as expired.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of contracts expired</returns>
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting contract expiration job at {Time}", DateTime.UtcNow);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ICrmDbContext>();
        var contractService = scope.ServiceProvider.GetRequiredService<IContractService>();

        try
        {
            var now = DateTime.UtcNow.Date;
            
            // Find active contracts that have passed their end date
            var expiredContracts = await context.Contracts
                .Where(c => !c.IsDeleted)
                .Where(c => c.Status == ContractStatus.Active)
                .Where(c => c.EndDate < now)
                .ToListAsync(cancellationToken);

            if (!expiredContracts.Any())
            {
                _logger.LogInformation("No contracts to expire");
                return 0;
            }

            _logger.LogInformation("Found {Count} contracts to expire", expiredContracts.Count);

            var expiredCount = 0;
            foreach (var contract in expiredContracts)
            {
                try
                {
                    await contractService.ExpireAsync(contract.Id, cancellationToken);
                    expiredCount++;
                    _logger.LogInformation("Contract {ContractNumber} expired (EndDate: {EndDate})",
                        contract.ContractNumber, contract.EndDate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to expire contract {ContractId}", contract.Id);
                }
            }

            _logger.LogInformation("Contract expiration job completed. Expired {Count} contracts", expiredCount);
            return expiredCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Contract expiration job failed");
            throw;
        }
    }

    /// <summary>
    /// Gets contracts that are due to expire within a specified number of days.
    /// Used for sending renewal reminders.
    /// </summary>
    /// <param name="withinDays">Number of days to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of contracts expiring soon</returns>
    public async Task<IEnumerable<Contract>> GetContractsExpiringSoonAsync(
        int withinDays = 30,
        CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ICrmDbContext>();

        var cutoffDate = DateTime.UtcNow.Date.AddDays(withinDays);
        var today = DateTime.UtcNow.Date;

        var contracts = await context.Contracts
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .Where(c => !c.IsDeleted)
            .Where(c => c.Status == ContractStatus.Active)
            .Where(c => c.EndDate >= today && c.EndDate <= cutoffDate)
            .OrderBy(c => c.EndDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} contracts expiring within {Days} days", 
            contracts.Count, withinDays);

        return contracts;
    }

    /// <summary>
    /// Sends renewal reminders for contracts expiring soon.
    /// Stub for notification integration.
    /// </summary>
    /// <param name="withinDays">Days before expiration to send reminder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of reminders sent</returns>
    public async Task<int> SendRenewalRemindersAsync(
        int withinDays = 30,
        CancellationToken cancellationToken = default)
    {
        var contracts = await GetContractsExpiringSoonAsync(withinDays, cancellationToken);
        var reminderCount = 0;

        foreach (var contract in contracts)
        {
            try
            {
                // Stub: In production, send actual notification
                _logger.LogInformation(
                    "Would send renewal reminder for contract {ContractNumber} (expires: {EndDate}) to {Account}",
                    contract.ContractNumber,
                    contract.EndDate,
                    contract.Account?.Company ?? "Unknown");
                reminderCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send renewal reminder for contract {ContractId}", contract.Id);
            }
        }

        return reminderCount;
    }

    /// <summary>
    /// Cron expression for scheduling with Hangfire.
    /// Default: Run daily at 1:00 AM UTC.
    /// </summary>
    public static string CronExpression => "0 1 * * *";

    /// <summary>
    /// Job identifier for Hangfire.
    /// </summary>
    public static string JobId => "contract-expiration-job";
}

/// <summary>
/// Extension methods for registering the contract expiration job.
/// </summary>
public static class ContractExpirationJobExtensions
{
    /// <summary>
    /// Registers the contract expiration job with Hangfire (when enabled).
    /// Call this in Program.cs after Hangfire is configured.
    /// </summary>
    /// <example>
    /// // In Program.cs after app.UseHangfireDashboard()
    /// app.RegisterContractExpirationJob();
    /// </example>
    public static void RegisterContractExpirationJob(this IServiceProvider serviceProvider)
    {
        // Hangfire registration stub
        // RecurringJob.AddOrUpdate<ContractExpirationJob>(
        //     ContractExpirationJob.JobId,
        //     job => job.ExecuteAsync(CancellationToken.None),
        //     ContractExpirationJob.CronExpression);

        var logger = serviceProvider.GetService<ILogger<ContractExpirationJob>>();
        logger?.LogInformation("Contract expiration job registered (Cron: {Cron})", ContractExpirationJob.CronExpression);
    }
}
