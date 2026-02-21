// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
using System;

namespace CRM.Core.Dtos
{
    /// <summary>
    /// DTO for cache statistics and monitoring.
    /// </summary>
    public class PermissionCacheStatisticsDto
    {
        /// <summary>
        /// Total number of cached users
        /// </summary>
        public int CachedUserCount { get; set; }

        /// <summary>
        /// Total cache hits since last reset
        /// </summary>
        public long TotalHits { get; set; }

        /// <summary>
        /// Total cache misses since last reset
        /// </summary>
        public long TotalMisses { get; set; }

        /// <summary>
        /// Cache hit rate percentage (0-100)
        /// </summary>
        public decimal HitRatePercentage => TotalHits + TotalMisses > 0
            ? (TotalHits * 100) / (TotalHits + TotalMisses)
            : 0;

        /// <summary>
        /// Average permissions per cached user
        /// </summary>
        public double AveragePermissionsPerUser { get; set; }

        /// <summary>
        /// Current cache memory usage in bytes (approximate)
        /// </summary>
        public long ApproximateMemoryUsageBytes { get; set; }

        /// <summary>
        /// When statistics were last reset
        /// </summary>
        public DateTime? LastResetAt { get; set; }

        /// <summary>
        /// When statistics were last updated
        /// </summary>
        public DateTime LastUpdatedUtc { get; set; }
    }
}
