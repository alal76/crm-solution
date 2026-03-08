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

// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.

using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Interface for CMDB auto-discovery operations.
/// </summary>
public interface IDiscoveryService
{
    /// <summary>
    /// Run a discovery scan for a specific network range or domain.
    /// </summary>
    Task<DiscoveryScanResult> RunDiscoveryScanAsync(DiscoveryScanRequest request);

    /// <summary>
    /// Get the status of a running or completed discovery scan.
    /// </summary>
    Task<DiscoveryScanStatus> GetScanStatusAsync(int scanId);

    /// <summary>
    /// Get discovered assets pending approval for CMDB import.
    /// </summary>
    Task<List<DiscoveredAsset>> GetPendingAssetsAsync();

    /// <summary>
    /// Approve and import discovered assets into CMDB.
    /// </summary>
    Task<CmdbImportResult> ImportAssetsAsync(List<int> assetIds, int approvedById);

    /// <summary>
    /// Reconcile discovered assets with existing CMDB entries.
    /// </summary>
    Task<ReconciliationResult> ReconcileAssetsAsync(int scanId);

    /// <summary>
    /// Get discovery schedules.
    /// </summary>
    Task<List<DiscoverySchedule>> GetSchedulesAsync();

    /// <summary>
    /// Create or update a discovery schedule.
    /// </summary>
    Task<DiscoverySchedule> SaveScheduleAsync(DiscoverySchedule schedule);
}

/// <summary>
/// Request parameters for a discovery scan.
/// </summary>
public class DiscoveryScanRequest
{
    public string Name { get; set; } = string.Empty;
    public DiscoveryType Type { get; set; }
    public string Target { get; set; } = string.Empty; // IP range, domain, etc.
    public List<string> Credentials { get; set; } = new(); // Credential references
    public bool ScanPorts { get; set; }
    public bool DetectSoftware { get; set; }
    public bool ResolveRelationships { get; set; }
    public int InitiatedById { get; set; }
}

public enum DiscoveryType
{
    NetworkScan,
    AgentBased,
    CloudProvider,
    ActiveDirectory,
    VMware,
    Azure,
    AWS
}

/// <summary>
/// Result of a discovery scan.
/// </summary>
public class DiscoveryScanResult
{
    public int ScanId { get; set; }
    public DiscoveryScanStatus Status { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<DiscoveredAsset> DiscoveredAssets { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Status of a discovery scan.
/// </summary>
public class DiscoveryScanStatus
{
    public int ScanId { get; set; }
    public ScanState State { get; set; }
    public int ProgressPercent { get; set; }
    public int AssetsDiscovered { get; set; }
    public int NewAssets { get; set; }
    public int UpdatedAssets { get; set; }
    public int FailedAssets { get; set; }
    public DateTime? EstimatedCompletion { get; set; }
    public string? CurrentTarget { get; set; }
}

public enum ScanState
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// An asset discovered during a scan.
/// </summary>
public class DiscoveredAsset
{
    public int DiscoveredAssetId { get; set; }
    public int ScanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public string? MACAddress { get; set; }
    public string? HostName { get; set; }
    public string? OperatingSystem { get; set; }
    public string? OSVersion { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int? CpuCores { get; set; }
    public int? MemoryMB { get; set; }
    public int? StorageGB { get; set; }
    public DateTime DiscoveredAt { get; set; }
    public AssetMatchStatus MatchStatus { get; set; }
    public int? MatchedCIId { get; set; }
    public double MatchConfidence { get; set; }
    public List<DiscoveredSoftware> InstalledSoftware { get; set; } = new();
    public List<DiscoveredRelationship> Relationships { get; set; } = new();
    public Dictionary<string, string> CustomAttributes { get; set; } = new();
}

public enum AssetMatchStatus
{
    New,
    Matched,
    PotentialMatch,
    Updated,
    Conflict
}

/// <summary>
/// Software discovered on an asset.
/// </summary>
public class DiscoveredSoftware
{
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? Vendor { get; set; }
    public DateTime? InstallDate { get; set; }
}

/// <summary>
/// Relationship discovered between assets.
/// </summary>
public class DiscoveredRelationship
{
    public string SourceAssetId { get; set; } = string.Empty;
    public string TargetAssetId { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public string? Details { get; set; }
}

/// <summary>
/// Result of importing assets into CMDB.
/// </summary>
public class CmdbImportResult
{
    public int TotalProcessed { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public List<ImportError> Errors { get; set; } = new();
    public List<int> CreatedCIIds { get; set; } = new();
}

public class ImportError
{
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Result of reconciliation between discovered assets and CMDB.
/// </summary>
public class ReconciliationResult
{
    public int TotalAssets { get; set; }
    public int ExactMatches { get; set; }
    public int PotentialMatches { get; set; }
    public int NewAssets { get; set; }
    public int Conflicts { get; set; }
    public List<ReconciliationConflict> ConflictDetails { get; set; } = new();
}

public class ReconciliationConflict
{
    public int DiscoveredAssetId { get; set; }
    public int ExistingCIId { get; set; }
    public string ConflictType { get; set; } = string.Empty;
    public Dictionary<string, FieldConflict> FieldConflicts { get; set; } = new();
}

public class FieldConflict
{
    public string FieldName { get; set; } = string.Empty;
    public string? DiscoveredValue { get; set; }
    public string? ExistingValue { get; set; }
}

/// <summary>
/// A scheduled discovery scan.
/// </summary>
public class DiscoverySchedule
{
    public int ScheduleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DiscoveryType Type { get; set; }
    public string Target { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }
    public bool AutoImportMatches { get; set; }
}

/// <summary>
/// Service for CMDB auto-discovery integration.
/// </summary>
public class DiscoveryService : IDiscoveryService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<DiscoveryService> _logger;

    // In-memory storage for demo (would be database in production)
    private static readonly Dictionary<int, DiscoveryScanResult> _scans = new();
    private static readonly List<DiscoveredAsset> _pendingAssets = new();
    private static readonly List<DiscoverySchedule> _schedules = new();
    private static int _nextScanId = 1;

    public DiscoveryService(
        ICrmDbContext context,
        ILogger<DiscoveryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DiscoveryScanResult> RunDiscoveryScanAsync(DiscoveryScanRequest request)
    {
        var scanId = _nextScanId++;

        var result = new DiscoveryScanResult
        {
            ScanId = scanId,
            StartTime = DateTime.UtcNow,
            Status = new DiscoveryScanStatus
            {
                ScanId = scanId,
                State = ScanState.Running,
                ProgressPercent = 0
            }
        };

        _scans[scanId] = result;

        _logger.LogInformation(
            "Starting discovery scan {ScanId}: {Name} ({Type}) on {Target}",
            scanId, request.Name, request.Type, request.Target);

        // Simulate discovery process
        await Task.Run(async () =>
        {
            try
            {
                // Phase 1: Network enumeration (simulated)
                result.Status.ProgressPercent = 10;
                result.Status.CurrentTarget = "Enumerating network...";
                await Task.Delay(100);

                // Phase 2: Asset discovery (simulated)
                result.Status.ProgressPercent = 30;
                result.Status.CurrentTarget = request.Target;

                // Generate simulated discovered assets based on type
                var assets = GenerateSimulatedAssets(request, scanId);
                result.DiscoveredAssets = assets;
                result.Status.AssetsDiscovered = assets.Count;

                await Task.Delay(100);

                // Phase 3: Reconciliation
                result.Status.ProgressPercent = 70;
                result.Status.CurrentTarget = "Reconciling with CMDB...";
                await ReconcileDiscoveredAssetsAsync(result.DiscoveredAssets);

                result.Status.NewAssets = assets.Count(a => a.MatchStatus == AssetMatchStatus.New);
                result.Status.UpdatedAssets = assets.Count(a => a.MatchStatus == AssetMatchStatus.Updated);

                // Phase 4: Complete
                result.Status.ProgressPercent = 100;
                result.Status.State = ScanState.Completed;
                result.Status.CurrentTarget = null;
                result.EndTime = DateTime.UtcNow;

                // Add new assets to pending list
                _pendingAssets.AddRange(
                    assets.Where(a => a.MatchStatus == AssetMatchStatus.New ||
                                      a.MatchStatus == AssetMatchStatus.PotentialMatch));

                _logger.LogInformation(
                    "Discovery scan {ScanId} completed: {Count} assets discovered, {New} new, {Updated} updated",
                    scanId, assets.Count, result.Status.NewAssets, result.Status.UpdatedAssets);
            }
            catch (Exception ex)
            {
                result.Status.State = ScanState.Failed;
                result.Errors.Add(ex.Message);
                _logger.LogError(ex, "Discovery scan {ScanId} failed", scanId);
            }
        });

        return await Task.FromResult(result);
    }

    public async Task<DiscoveryScanStatus> GetScanStatusAsync(int scanId)
    {
        if (_scans.TryGetValue(scanId, out var result))
        {
            return await Task.FromResult(result.Status);
        }

        throw new ArgumentException($"Scan {scanId} not found");
    }

    public async Task<List<DiscoveredAsset>> GetPendingAssetsAsync()
    {
        return await Task.FromResult(_pendingAssets.ToList());
    }

    public async Task<CmdbImportResult> ImportAssetsAsync(List<int> assetIds, int approvedById)
    {
        var result = new CmdbImportResult();

        var assetsToImport = _pendingAssets
            .Where(a => assetIds.Contains(a.DiscoveredAssetId))
            .ToList();

        foreach (var asset in assetsToImport)
        {
            result.TotalProcessed++;

            try
            {
                if (asset.MatchedCIId.HasValue && asset.MatchStatus == AssetMatchStatus.Matched)
                {
                    // Update existing CI
                    var existingCI = await _context.ConfigurationItems
                        .FirstOrDefaultAsync(ci => ci.CIId == asset.MatchedCIId.Value);

                    if (existingCI != null)
                    {
                        UpdateCIFromAsset(existingCI, asset);
                        existingCI.ModifiedAt = DateTime.UtcNow;
                        existingCI.LastDiscovered = asset.DiscoveredAt;
                        result.Updated++;
                    }
                }
                else
                {
                    // Create new CI
                    var newCI = new ConfigurationItem
                    {
                        CIName = asset.Name,
                        CIType = MapAssetTypeToCIType(asset.AssetType),
                        OperationalStatus = CRM.Core.Entities.ITSM.OperationalStatus.Operational,
                        Criticality = CRM.Core.Entities.ITSM.CICriticality.Medium,
                        Manufacturer = asset.Manufacturer,
                        ModelNumber = asset.Model,
                        SerialNumber = asset.SerialNumber,
                        IPAddress = asset.IPAddress,
                        LastDiscovered = asset.DiscoveredAt,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    };

                    _context.ConfigurationItems.Add(newCI);
                    await _context.SaveChangesAsync();

                    result.Created++;
                    result.CreatedCIIds.Add(newCI.CIId);
                }

                // Remove from pending list
                _pendingAssets.Remove(asset);
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add(new ImportError
                {
                    AssetId = asset.DiscoveredAssetId,
                    AssetName = asset.Name,
                    ErrorMessage = ex.Message
                });
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "CMDB import completed: {Created} created, {Updated} updated, {Failed} failed",
            result.Created, result.Updated, result.Failed);

        return result;
    }

    public async Task<ReconciliationResult> ReconcileAssetsAsync(int scanId)
    {
        if (!_scans.TryGetValue(scanId, out var scan))
        {
            throw new ArgumentException($"Scan {scanId} not found");
        }

        var result = new ReconciliationResult
        {
            TotalAssets = scan.DiscoveredAssets.Count
        };

        foreach (var asset in scan.DiscoveredAssets)
        {
            switch (asset.MatchStatus)
            {
                case AssetMatchStatus.Matched:
                    result.ExactMatches++;
                    break;
                case AssetMatchStatus.PotentialMatch:
                    result.PotentialMatches++;
                    break;
                case AssetMatchStatus.New:
                    result.NewAssets++;
                    break;
                case AssetMatchStatus.Conflict:
                    result.Conflicts++;
                    result.ConflictDetails.Add(new ReconciliationConflict
                    {
                        DiscoveredAssetId = asset.DiscoveredAssetId,
                        ExistingCIId = asset.MatchedCIId ?? 0,
                        ConflictType = "Data Mismatch"
                    });
                    break;
            }
        }

        return await Task.FromResult(result);
    }

    public async Task<List<DiscoverySchedule>> GetSchedulesAsync()
    {
        if (!_schedules.Any())
        {
            // Return default schedules
            _schedules.AddRange(new[]
            {
                new DiscoverySchedule
                {
                    ScheduleId = 1,
                    Name = "Weekly Network Scan",
                    Type = DiscoveryType.NetworkScan,
                    Target = "10.0.0.0/8",
                    CronExpression = "0 0 2 * * SUN", // Every Sunday at 2 AM
                    IsActive = true,
                    AutoImportMatches = true
                },
                new DiscoverySchedule
                {
                    ScheduleId = 2,
                    Name = "Daily Cloud Sync",
                    Type = DiscoveryType.Azure,
                    Target = "subscription-id",
                    CronExpression = "0 0 * * * *", // Every hour
                    IsActive = true,
                    AutoImportMatches = false
                }
            });
        }

        return await Task.FromResult(_schedules.ToList());
    }

    public async Task<DiscoverySchedule> SaveScheduleAsync(DiscoverySchedule schedule)
    {
        if (schedule.ScheduleId == 0)
        {
            schedule.ScheduleId = _schedules.Count + 1;
            _schedules.Add(schedule);
        }
        else
        {
            var existing = _schedules.FirstOrDefault(s => s.ScheduleId == schedule.ScheduleId);
            if (existing != null)
            {
                _schedules.Remove(existing);
            }
            _schedules.Add(schedule);
        }

        _logger.LogInformation(
            "Saved discovery schedule: {Name} ({Type})",
            schedule.Name, schedule.Type);

        return await Task.FromResult(schedule);
    }

    private List<DiscoveredAsset> GenerateSimulatedAssets(DiscoveryScanRequest request, int scanId)
    {
        var assets = new List<DiscoveredAsset>();
        var random = new Random();
        var assetId = 1;

        // Generate based on scan type
        switch (request.Type)
        {
            case DiscoveryType.NetworkScan:
                // Simulate servers
                for (int i = 1; i <= 3; i++)
                {
                    assets.Add(new DiscoveredAsset
                    {
                        DiscoveredAssetId = assetId++,
                        ScanId = scanId,
                        Name = $"SRV-PROD-{i:D3}",
                        IPAddress = $"10.0.1.{i + 10}",
                        HostName = $"srv-prod-{i:d3}.corp.local",
                        OperatingSystem = "Windows Server",
                        OSVersion = "2022",
                        AssetType = "Server",
                        Manufacturer = "Dell",
                        Model = "PowerEdge R750",
                        CpuCores = 32,
                        MemoryMB = 131072,
                        StorageGB = 2000,
                        DiscoveredAt = DateTime.UtcNow,
                        MatchStatus = random.Next(3) == 0 ? AssetMatchStatus.Matched : AssetMatchStatus.New,
                        MatchConfidence = random.NextDouble() * 0.3 + 0.7
                    });
                }

                // Simulate workstations
                for (int i = 1; i <= 5; i++)
                {
                    assets.Add(new DiscoveredAsset
                    {
                        DiscoveredAssetId = assetId++,
                        ScanId = scanId,
                        Name = $"WKS-{random.Next(1000, 9999)}",
                        IPAddress = $"10.0.2.{i + 100}",
                        OperatingSystem = "Windows",
                        OSVersion = "11",
                        AssetType = "Workstation",
                        Manufacturer = "Lenovo",
                        Model = "ThinkPad X1 Carbon",
                        CpuCores = 8,
                        MemoryMB = 16384,
                        StorageGB = 512,
                        DiscoveredAt = DateTime.UtcNow,
                        MatchStatus = AssetMatchStatus.New,
                        MatchConfidence = 0
                    });
                }
                break;

            case DiscoveryType.Azure:
            case DiscoveryType.AWS:
                // Simulate cloud resources
                var vmNames = new[] { "web-app-01", "api-server-01", "db-primary", "cache-01" };
                foreach (var name in vmNames)
                {
                    assets.Add(new DiscoveredAsset
                    {
                        DiscoveredAssetId = assetId++,
                        ScanId = scanId,
                        Name = name,
                        AssetType = "Virtual Machine",
                        OperatingSystem = name.Contains("db") ? "Ubuntu" : "Windows Server",
                        OSVersion = name.Contains("db") ? "22.04 LTS" : "2022",
                        CpuCores = name.Contains("db") ? 16 : 4,
                        MemoryMB = name.Contains("db") ? 65536 : 8192,
                        DiscoveredAt = DateTime.UtcNow,
                        MatchStatus = random.Next(2) == 0 ? AssetMatchStatus.PotentialMatch : AssetMatchStatus.New,
                        MatchConfidence = 0.6 + random.NextDouble() * 0.4
                    });
                }
                break;

            case DiscoveryType.VMware:
                // Simulate VMs
                for (int i = 1; i <= 4; i++)
                {
                    assets.Add(new DiscoveredAsset
                    {
                        DiscoveredAssetId = assetId++,
                        ScanId = scanId,
                        Name = $"VM-APP-{i:D3}",
                        AssetType = "Virtual Machine",
                        OperatingSystem = "Windows Server",
                        OSVersion = "2019",
                        CpuCores = 4,
                        MemoryMB = 8192,
                        DiscoveredAt = DateTime.UtcNow,
                        MatchStatus = AssetMatchStatus.New
                    });
                }
                break;
        }

        return assets;
    }

    private async Task ReconcileDiscoveredAssetsAsync(List<DiscoveredAsset> assets)
    {
        var existingCIs = await _context.ConfigurationItems.ToListAsync();

        foreach (var asset in assets)
        {
            // Try to match by serial number first
            if (!string.IsNullOrEmpty(asset.SerialNumber))
            {
                var match = existingCIs.FirstOrDefault(ci =>
                    ci.SerialNumber == asset.SerialNumber);

                if (match != null)
                {
                    asset.MatchStatus = AssetMatchStatus.Matched;
                    asset.MatchedCIId = match.CIId;
                    asset.MatchConfidence = 1.0;
                    continue;
                }
            }

            // Try to match by IP address
            if (!string.IsNullOrEmpty(asset.IPAddress))
            {
                var match = existingCIs.FirstOrDefault(ci =>
                    ci.IPAddress == asset.IPAddress);

                if (match != null)
                {
                    asset.MatchStatus = AssetMatchStatus.PotentialMatch;
                    asset.MatchedCIId = match.CIId;
                    asset.MatchConfidence = 0.8;
                    continue;
                }
            }

            // Try to match by name
            var nameMatch = existingCIs.FirstOrDefault(ci =>
                ci.CIName.Equals(asset.Name, StringComparison.OrdinalIgnoreCase));

            if (nameMatch != null)
            {
                asset.MatchStatus = AssetMatchStatus.PotentialMatch;
                asset.MatchedCIId = nameMatch.CIId;
                asset.MatchConfidence = 0.7;
            }
            else
            {
                asset.MatchStatus = AssetMatchStatus.New;
                asset.MatchConfidence = 0;
            }
        }
    }

    private static void UpdateCIFromAsset(ConfigurationItem ci, DiscoveredAsset asset)
    {
        if (!string.IsNullOrEmpty(asset.IPAddress))
            ci.IPAddress = asset.IPAddress;
        if (!string.IsNullOrEmpty(asset.SerialNumber))
            ci.SerialNumber = asset.SerialNumber;
        if (!string.IsNullOrEmpty(asset.Manufacturer))
            ci.Manufacturer = asset.Manufacturer;
        if (!string.IsNullOrEmpty(asset.Model))
            ci.ModelNumber = asset.Model;
    }

    private static CIType MapAssetTypeToCIType(string assetType)
    {
        return assetType.ToLower() switch
        {
            "server" => CIType.Server,
            "workstation" => CIType.WorkStation,
            "laptop" => CIType.WorkStation,
            "virtual machine" => CIType.VirtualMachine,
            "network device" => CIType.NetworkDevice,
            "storage" => CIType.Storage,
            _ => CIType.Server
        };
    }
}


