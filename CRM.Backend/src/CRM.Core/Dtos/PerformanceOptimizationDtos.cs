// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
using System;
using System.Collections.Generic;

namespace CRM.Core.Dtos
{
    /// <summary>
    /// DTO for performance dashboard
    /// </summary>
    public class PerformanceDashboardDto
    {
        public double AverageResponseTimeMs { get; set; }
        public int P95ResponseTimeMs { get; set; }
        public int P99ResponseTimeMs { get; set; }
        public double CacheHitRate { get; set; }
        public double ErrorRate { get; set; }
        public int TotalRequestsLastHour { get; set; }
        public int TotalRequestsLastDay { get; set; }
        public PerformanceStatisticsDto[] TopEndpoints { get; set; } = Array.Empty<PerformanceStatisticsDto>();
        public PerformanceRecommendationDto[] Recommendations { get; set; } = Array.Empty<PerformanceRecommendationDto>();
    }

    /// <summary>
    /// DTO for error statistics
    /// </summary>
    public class ErrorStatisticsDto
    {
        public int TotalErrors { get; set; }
        public double ErrorRate { get; set; }
        public Dictionary<int, int> ErrorsByStatus { get; set; } = new();
        public Dictionary<string, int> ErrorsByEndpoint { get; set; } = new();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
