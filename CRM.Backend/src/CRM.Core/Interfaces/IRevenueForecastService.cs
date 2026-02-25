// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for generating revenue forecasts based on pipeline and historical data.
/// Implements TODO-AI-10.
/// </summary>
public interface IRevenueForecastService
{
    /// <summary>
    /// Generates a monthly revenue forecast for the specified number of months ahead.
    /// </summary>
    /// <param name="months">Number of months to forecast (1–24).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Revenue forecast with monthly breakdowns.</returns>
    Task<RevenueForecastDto> ForecastRevenueAsync(int months = 6, CancellationToken ct = default);
}

/// <summary>
/// Revenue forecast covering multiple months.
/// </summary>
public class RevenueForecastDto
{
    /// <summary>Monthly forecast data points.</summary>
    public ForecastMonthDto[] Months { get; set; } = Array.Empty<ForecastMonthDto>();

    /// <summary>Total forecasted revenue across all months.</summary>
    public decimal TotalForecastedRevenue { get; set; }

    /// <summary>Overall confidence percentage (0–100).</summary>
    public int OverallConfidencePct { get; set; }

    /// <summary>When the forecast was generated.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Forecast data for a single month.
/// </summary>
public class ForecastMonthDto
{
    /// <summary>Month label in "yyyy-MM" format (e.g. "2026-03").</summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>Forecasted revenue for the month (weighted pipeline + historical trend).</summary>
    public decimal ForecastedRevenue { get; set; }

    /// <summary>Lower confidence bound.</summary>
    public decimal ConfidenceLow { get; set; }

    /// <summary>Upper confidence bound.</summary>
    public decimal ConfidenceHigh { get; set; }

    /// <summary>Total weighted pipeline value expected to close in this month.</summary>
    public decimal PipelineRevenue { get; set; }

    /// <summary>Revenue already closed/confirmed in this month.</summary>
    public decimal ClosedRevenue { get; set; }
}
