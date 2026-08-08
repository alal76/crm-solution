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
/// Background job that sends scheduled email digests (REV-FE-002).
/// Designed for Hangfire integration — mirrors the shape of ContractExpirationJob
/// (constructor takes IServiceProvider + ILogger, ExecuteAsync does the work inside a fresh
/// DI scope). Registered as an hourly Hangfire RecurringJob in Program.cs: on each tick it finds
/// enabled EmailDigestConfig rows that are due to send this hour (matching Frequency/DayOfWeek/
/// DayOfMonth/TimeOfDay against the user's Timezone), assembles the digest via IEmailDigestService,
/// and sends it.
/// </summary>
public class EmailDigestJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailDigestJob> _logger;

    public EmailDigestJob(
        IServiceProvider serviceProvider,
        ILogger<EmailDigestJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the email digest job: finds configs due this hour and sends them.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of digests successfully sent</returns>
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;
        _logger.LogInformation("Starting email digest job at {Time}", nowUtc);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ICrmDbContext>();
        var emailDigestService = scope.ServiceProvider.GetRequiredService<IEmailDigestService>();

        try
        {
            var candidates = await context.EmailDigestConfigs
                .Where(c => !c.IsDeleted && c.IsEnabled)
                .ToListAsync(cancellationToken);

            var due = candidates.Where(c => IsDueThisHour(c, nowUtc)).ToList();

            if (due.Count == 0)
            {
                _logger.LogInformation("No email digests due this hour");
                return 0;
            }

            _logger.LogInformation("Found {Count} email digest(s) due this hour", due.Count);

            var sentCount = 0;
            foreach (var config in due)
            {
                try
                {
                    var user = await context.Users
                        .FirstOrDefaultAsync(u => u.Id == config.UserId && !u.IsDeleted, cancellationToken);
                    if (user == null)
                    {
                        _logger.LogWarning("Skipping email digest for missing/deleted user {UserId}", config.UserId);
                        continue;
                    }

                    var sent = await emailDigestService.SendDigestAsync(user, config, isPreview: false, cancellationToken);
                    if (sent)
                    {
                        sentCount++;
                        _logger.LogInformation("Email digest sent to user {UserId}", config.UserId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email digest for config {ConfigId} (user {UserId})", config.Id, config.UserId);
                }
            }

            _logger.LogInformation("Email digest job completed. Sent {Count} digest(s)", sentCount);
            return sentCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email digest job failed");
            throw;
        }
    }

    /// <summary>
    /// Determines whether a digest config is due to send during the hour containing <paramref name="nowUtc"/>,
    /// based on the user's configured Timezone/Frequency/DayOfWeek/DayOfMonth/TimeOfDay.
    /// Guards against double-sending within the same due hour via LastSentAt.
    /// </summary>
    internal static bool IsDueThisHour(EmailDigestConfig config, DateTime nowUtc)
    {
        if (config.LastSentAt.HasValue && (nowUtc - config.LastSentAt.Value) < TimeSpan.FromMinutes(55))
        {
            // Already sent within this same due hour (or job re-ran) — skip.
            return false;
        }

        DateTime localNow;
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(string.IsNullOrWhiteSpace(config.Timezone) ? "UTC" : config.Timezone);
            localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);
        }
        catch (TimeZoneNotFoundException)
        {
            localNow = nowUtc; // Fall back to UTC for an unrecognized timezone identifier.
        }
        catch (InvalidTimeZoneException)
        {
            localNow = nowUtc;
        }

        if (localNow.Hour != config.TimeOfDay.Hours)
        {
            return false;
        }

        return config.Frequency switch
        {
            EmailDigestFrequency.Daily => true,
            EmailDigestFrequency.Weekly => config.DayOfWeek.HasValue && (int)localNow.DayOfWeek == config.DayOfWeek.Value,
            EmailDigestFrequency.Monthly => config.DayOfMonth.HasValue && IsMonthlyDueDay(localNow, config.DayOfMonth.Value),
            _ => false
        };
    }

    /// <summary>
    /// Matches the configured day-of-month, clamping to the last day of shorter months
    /// (e.g. DayOfMonth=31 fires on Feb 28/29) so months without that day aren't skipped entirely.
    /// </summary>
    private static bool IsMonthlyDueDay(DateTime localNow, int dayOfMonth)
    {
        var lastDayOfMonth = DateTime.DaysInMonth(localNow.Year, localNow.Month);
        var effectiveDay = Math.Min(dayOfMonth, lastDayOfMonth);
        return localNow.Day == effectiveDay;
    }

    /// <summary>
    /// Cron expression for scheduling with Hangfire. Runs at the top of every hour.
    /// </summary>
    public static string CronExpression => "0 * * * *";

    /// <summary>
    /// Job identifier for Hangfire.
    /// </summary>
    public static string JobId => "email-digest-job";
}

/// <summary>
/// Extension methods for registering the email digest job.
/// </summary>
public static class EmailDigestJobExtensions
{
    /// <summary>
    /// Registers the email digest job with Hangfire (when enabled).
    /// Call this in Program.cs after Hangfire is configured.
    /// </summary>
    public static void RegisterEmailDigestJob(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<EmailDigestJob>>();
        logger?.LogInformation("Email digest job registered (Cron: {Cron})", EmailDigestJob.CronExpression);
    }
}
