// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using CRM.Core.Entities.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CRM.Infrastructure.AI.SK.Agents;

/// <summary>
/// AI agent specialized in CRM data analysis. Answers data-driven questions,
/// provides statistics, identifies trends, and generates ad-hoc reports
/// grounded in specific numbers from the CRM database.
/// </summary>
public sealed class DataAnalystAgent : CrmAgentBase
{
    #region Agent Identity

    /// <inheritdoc />
    public override string AgentName => "Data Analyst";

    /// <inheritdoc />
    public override AgentType AgentType => AgentType.DataAnalyst;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public override double Temperature => 0.2;

    /// <inheritdoc />
    public override int MaxTokens => 4096;

    /// <inheritdoc />
    public override IReadOnlyList<string> AllowedPlugins { get; } = new[]
    {
        "Account",
        "Opportunity",
        "Lead",
        "Quote",
        "Contract",
        "Search"
    };

    #endregion

    #region System Prompt

    /// <inheritdoc />
    public override string SystemPrompt =>
        """
        You are a data analyst agent embedded in a CRM system. Your role is to answer
        data questions, provide statistics, identify trends, and generate insights.

        ## Core Capabilities
        - Answer quantitative questions about CRM data
        - Calculate KPIs: conversion rates, win rates, average deal size, pipeline velocity
        - Identify trends across time periods
        - Segment data by region, industry, team, product, or time period
        - Generate summary reports and comparisons

        ## Analysis Types
        - **Pipeline Analysis**: total value, stage distribution, velocity, bottlenecks
        - **Performance Metrics**: rep performance, team comparisons, quota attainment
        - **Customer Analysis**: top accounts by revenue, growth trends, churn indicators
        - **Lead Analysis**: source effectiveness, conversion rates, time-to-convert
        - **Revenue Analysis**: MRR/ARR trends, average contract value, expansion revenue

        ## Guidelines
        - ALWAYS cite specific numbers and data points in your responses
        - Use percentages, averages, and comparisons for context
        - When presenting data, use structured formats (tables, lists)
        - Clearly state the time period and scope of any analysis
        - Distinguish between exact figures and estimates
        - If data is insufficient for a reliable answer, say so explicitly

        ## Response Format
        - For metrics: present as "Metric: Value (context/comparison)"
        - For trends: describe direction, magnitude, and time period
        - For reports: use headers, tables, and summary sections
        - Always include a brief insight or takeaway with raw data

        ## Rules
        - Never fabricate or estimate numbers without stating it is an estimate
        - Always specify the date range of the data being analyzed
        - Round percentages to one decimal place
        - Use currency formatting for monetary values
        """;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DataAnalystAgent"/> class.
    /// </summary>
    /// <param name="kernel">The Semantic Kernel instance.</param>
    /// <param name="logger">The logger instance.</param>
    public DataAnalystAgent(Kernel kernel, ILogger<DataAnalystAgent> logger)
        : base(kernel, logger)
    {
    }

    #endregion

    #region Routing

    /// <summary>
    /// Handles requests related to reports, analytics, and data queries.
    /// </summary>
    /// <param name="entityType">The CRM entity type.</param>
    /// <param name="intent">The optional detected intent.</param>
    /// <returns><c>true</c> if the entity type is report, analytics, or data.</returns>
    public override bool CanHandle(string entityType, string? intent)
    {
        return string.Equals(entityType, "report", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "analytics", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entityType, "data", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
