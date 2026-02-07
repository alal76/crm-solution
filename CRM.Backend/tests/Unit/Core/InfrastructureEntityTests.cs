// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Licensed under the GNU Affero General Public License v3.0.
// See LICENSE file in the project root for full license information.

using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for Infrastructure entities: Cloud Deployment, Database Backup,
/// Backup Schedule, Calendar Integration, and related enums.
/// </summary>
public class InfrastructureEntityTests
{
    #region CloudProviderType Enum Tests

    [Fact]
    public void CloudProviderType_ShouldHaveCorrectValues()
    {
        ((int)CloudProviderType.AWS).Should().Be(1);
        ((int)CloudProviderType.Azure).Should().Be(2);
        ((int)CloudProviderType.GoogleCloud).Should().Be(3);
        ((int)CloudProviderType.DigitalOcean).Should().Be(4);
        ((int)CloudProviderType.Kubernetes).Should().Be(5);
        ((int)CloudProviderType.Docker).Should().Be(6);
        ((int)CloudProviderType.OnPremise).Should().Be(7);
    }

    [Fact]
    public void CloudProviderType_ShouldHave7Values()
    {
        Enum.GetValues<CloudProviderType>().Should().HaveCount(7);
    }

    #endregion

    #region DeploymentStatus Enum Tests

    [Fact]
    public void DeploymentStatus_ShouldHaveCorrectValues()
    {
        ((int)DeploymentStatus.Pending).Should().Be(0);
        ((int)DeploymentStatus.Provisioning).Should().Be(1);
        ((int)DeploymentStatus.Building).Should().Be(2);
        ((int)DeploymentStatus.Deploying).Should().Be(3);
        ((int)DeploymentStatus.Running).Should().Be(4);
        ((int)DeploymentStatus.Stopped).Should().Be(5);
        ((int)DeploymentStatus.Failed).Should().Be(6);
        ((int)DeploymentStatus.Terminated).Should().Be(7);
    }

    [Fact]
    public void DeploymentStatus_ShouldHave8Values()
    {
        Enum.GetValues<DeploymentStatus>().Should().HaveCount(8);
    }

    #endregion

    #region HealthStatus Enum Tests

    [Fact]
    public void HealthStatus_ShouldHaveCorrectValues()
    {
        ((int)HealthStatus.Unknown).Should().Be(0);
        ((int)HealthStatus.Healthy).Should().Be(1);
        ((int)HealthStatus.Degraded).Should().Be(2);
        ((int)HealthStatus.Unhealthy).Should().Be(3);
        ((int)HealthStatus.Offline).Should().Be(4);
    }

    [Fact]
    public void HealthStatus_ShouldHave5Values()
    {
        Enum.GetValues<HealthStatus>().Should().HaveCount(5);
    }

    #endregion

    #region CalendarProvider Enum Tests

    [Fact]
    public void CalendarProvider_ShouldHaveCorrectValues()
    {
        ((int)CalendarProvider.Google).Should().Be(0);
        ((int)CalendarProvider.Outlook).Should().Be(1);
        ((int)CalendarProvider.Apple).Should().Be(2);
    }

    [Fact]
    public void CalendarProvider_ShouldHave3Values()
    {
        Enum.GetValues<CalendarProvider>().Should().HaveCount(3);
    }

    #endregion

    #region CalendarSyncDirection Enum Tests

    [Fact]
    public void CalendarSyncDirection_ShouldHaveCorrectValues()
    {
        ((int)CalendarSyncDirection.Import).Should().Be(0);
        ((int)CalendarSyncDirection.Export).Should().Be(1);
        ((int)CalendarSyncDirection.Bidirectional).Should().Be(2);
    }

    [Fact]
    public void CalendarSyncDirection_ShouldHave3Values()
    {
        Enum.GetValues<CalendarSyncDirection>().Should().HaveCount(3);
    }

    #endregion

    #region CalendarSyncStatus Enum Tests

    [Fact]
    public void CalendarSyncStatus_ShouldHaveCorrectValues()
    {
        ((int)CalendarSyncStatus.Success).Should().Be(0);
        ((int)CalendarSyncStatus.InProgress).Should().Be(1);
        ((int)CalendarSyncStatus.Failed).Should().Be(2);
        ((int)CalendarSyncStatus.Pending).Should().Be(3);
    }

    [Fact]
    public void CalendarSyncStatus_ShouldHave4Values()
    {
        Enum.GetValues<CalendarSyncStatus>().Should().HaveCount(4);
    }

    #endregion

    #region CloudProvider Entity Tests

    [Fact]
    public void CloudProvider_ShouldInitializeWithDefaults()
    {
        var provider = new CloudProvider();

        provider.Name.Should().Be(string.Empty);
        // ProviderType defaults to 0 (uninitialized) since enum starts at AWS=1
        ((int)provider.ProviderType).Should().Be(0);
        provider.Description.Should().BeNull();
        provider.AccessKeyId.Should().BeNull();
        provider.SecretAccessKey.Should().BeNull();
        provider.TenantId.Should().BeNull();
        provider.SubscriptionId.Should().BeNull();
        provider.ProjectId.Should().BeNull();
        provider.Region.Should().BeNull();
        provider.Endpoint.Should().BeNull();
        provider.Configuration.Should().BeNull();
        provider.IsActive.Should().BeTrue();
        provider.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void CloudProvider_ShouldSetProperties()
    {
        var provider = new CloudProvider
        {
            Name = "AWS Production",
            ProviderType = CloudProviderType.AWS,
            Description = "Production AWS account",
            AccessKeyId = "AKIAIOSFODNN7EXAMPLE",
            SecretAccessKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
            Region = "us-east-1",
            IsActive = true,
            IsDefault = true
        };

        provider.Name.Should().Be("AWS Production");
        provider.ProviderType.Should().Be(CloudProviderType.AWS);
        provider.Region.Should().Be("us-east-1");
        provider.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void CloudProvider_AzureConfiguration_ShouldSetProperties()
    {
        var provider = new CloudProvider
        {
            Name = "Azure Production",
            ProviderType = CloudProviderType.Azure,
            TenantId = "tenant-guid",
            SubscriptionId = "subscription-guid"
        };

        provider.ProviderType.Should().Be(CloudProviderType.Azure);
        provider.TenantId.Should().Be("tenant-guid");
        provider.SubscriptionId.Should().Be("subscription-guid");
    }

    [Fact]
    public void CloudProvider_Collections_ShouldInitializeEmpty()
    {
        var provider = new CloudProvider();

        provider.Deployments.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region CloudDeployment Entity Tests

    [Fact]
    public void CloudDeployment_ShouldInitializeWithDefaults()
    {
        var deployment = new CloudDeployment();

        deployment.Name.Should().Be(string.Empty);
        deployment.Description.Should().BeNull();
        deployment.CloudProviderId.Should().Be(0);
        deployment.ClusterName.Should().BeNull();
        deployment.Namespace.Should().BeNull();
        deployment.SslEnabled.Should().BeTrue();
        deployment.CpuUnits.Should().Be(256);
        deployment.MemoryMb.Should().Be(512);
        deployment.Replicas.Should().Be(1);
        deployment.Status.Should().Be(DeploymentStatus.Pending);
        deployment.HealthStatus.Should().Be(HealthStatus.Unknown);
    }

    [Fact]
    public void CloudDeployment_ShouldSetProperties()
    {
        var deployedAt = DateTime.UtcNow;
        var deployment = new CloudDeployment
        {
            Name = "crm-production",
            Description = "Production CRM deployment",
            CloudProviderId = 1,
            ClusterName = "eks-cluster-1",
            Namespace = "crm-prod",
            BackendImage = "crm-api:v1.0.0",
            FrontendImage = "crm-frontend:v1.0.0",
            FrontendUrl = "https://crm.example.com",
            ApiUrl = "https://api.crm.example.com",
            CpuUnits = 1024,
            MemoryMb = 2048,
            Replicas = 3,
            Status = DeploymentStatus.Running,
            HealthStatus = HealthStatus.Healthy,
            DeployedAt = deployedAt
        };

        deployment.Name.Should().Be("crm-production");
        deployment.ClusterName.Should().Be("eks-cluster-1");
        deployment.BackendImage.Should().Be("crm-api:v1.0.0");
        deployment.Replicas.Should().Be(3);
        deployment.Status.Should().Be(DeploymentStatus.Running);
        deployment.HealthStatus.Should().Be(HealthStatus.Healthy);
        deployment.DeployedAt.Should().Be(deployedAt);
    }

    [Fact]
    public void CloudDeployment_Collections_ShouldInitializeEmpty()
    {
        var deployment = new CloudDeployment();

        deployment.Attempts.Should().NotBeNull().And.BeEmpty();
        deployment.HealthChecks.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region DeploymentAttempt Entity Tests

    [Fact]
    public void DeploymentAttempt_ShouldInitializeWithDefaults()
    {
        var attempt = new DeploymentAttempt();

        attempt.CloudDeploymentId.Should().Be(0);
        attempt.AttemptNumber.Should().Be(string.Empty);
        attempt.Status.Should().Be(DeploymentStatus.Pending);
        attempt.GitCommitHash.Should().BeNull();
        attempt.GitBranch.Should().BeNull();
        attempt.BuildNumber.Should().BeNull();
        attempt.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        attempt.CompletedAt.Should().BeNull();
        attempt.DurationSeconds.Should().BeNull();
    }

    [Fact]
    public void DeploymentAttempt_ShouldSetProperties()
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddMinutes(10);

        var attempt = new DeploymentAttempt
        {
            CloudDeploymentId = 1,
            AttemptNumber = "1",
            Status = DeploymentStatus.Running,
            GitCommitHash = "abc123",
            GitBranch = "main",
            BuildNumber = "build-42",
            BackendImageTag = "crm-api:v1.0.0-build-42",
            FrontendImageTag = "crm-frontend:v1.0.0-build-42",
            StartedAt = startTime,
            CompletedAt = endTime,
            DurationSeconds = 600,
            TriggeredByUserId = 1,
            TriggerType = "Manual"
        };

        attempt.AttemptNumber.Should().Be("1");
        attempt.GitCommitHash.Should().Be("abc123");
        attempt.GitBranch.Should().Be("main");
        attempt.DurationSeconds.Should().Be(600);
        attempt.TriggerType.Should().Be("Manual");
    }

    [Fact]
    public void DeploymentAttempt_Failed_ShouldTrackErrorDetails()
    {
        var attempt = new DeploymentAttempt
        {
            Status = DeploymentStatus.Failed,
            ErrorMessage = "Docker build failed",
            ErrorStackTrace = "at DockerService.Build()...",
            BuildLog = "Step 1/10 : FROM mcr.microsoft.com/dotnet/aspnet:8.0..."
        };

        attempt.Status.Should().Be(DeploymentStatus.Failed);
        attempt.ErrorMessage.Should().Be("Docker build failed");
        attempt.ErrorStackTrace.Should().StartWith("at DockerService");
    }

    #endregion

    #region HealthCheckLog Entity Tests

    [Fact]
    public void HealthCheckLog_ShouldInitializeWithDefaults()
    {
        var log = new HealthCheckLog();

        log.CloudDeploymentId.Should().Be(0);
        log.Status.Should().Be(HealthStatus.Unknown);
        log.CheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        log.ApiHealthy.Should().BeNull();
        log.FrontendHealthy.Should().BeNull();
        log.DatabaseHealthy.Should().BeNull();
    }

    [Fact]
    public void HealthCheckLog_Healthy_ShouldSetProperties()
    {
        var checkTime = DateTime.UtcNow;

        var log = new HealthCheckLog
        {
            CloudDeploymentId = 1,
            Status = HealthStatus.Healthy,
            CheckedAt = checkTime,
            ApiHealthy = true,
            FrontendHealthy = true,
            DatabaseHealthy = true,
            ApiResponseTimeMs = 50,
            FrontendResponseTimeMs = 120,
            DatabaseResponseTimeMs = 10
        };

        log.Status.Should().Be(HealthStatus.Healthy);
        log.ApiHealthy.Should().BeTrue();
        log.ApiResponseTimeMs.Should().Be(50);
        log.DatabaseResponseTimeMs.Should().Be(10);
    }

    [Fact]
    public void HealthCheckLog_Degraded_ShouldTrackPartialFailure()
    {
        var log = new HealthCheckLog
        {
            Status = HealthStatus.Degraded,
            ApiHealthy = true,
            FrontendHealthy = true,
            DatabaseHealthy = false,
            ErrorDetails = "Database connection timeout"
        };

        log.Status.Should().Be(HealthStatus.Degraded);
        log.DatabaseHealthy.Should().BeFalse();
        log.ErrorDetails.Should().Contain("timeout");
    }

    #endregion

    #region DatabaseBackup Entity Tests

    [Fact]
    public void DatabaseBackup_ShouldInitializeWithDefaults()
    {
        var backup = new DatabaseBackup();

        backup.BackupName.Should().Be(string.Empty);
        backup.FilePath.Should().Be(string.Empty);
        backup.FileSizeBytes.Should().Be(0);
        backup.SourceDatabase.Should().Be(string.Empty);
        backup.BackupType.Should().Be("Full");
        backup.CreatedByUserId.Should().BeNull();
        backup.Description.Should().BeNull();
        backup.IsCompressed.Should().BeTrue();
        backup.ChecksumHash.Should().BeNull();
    }

    [Fact]
    public void DatabaseBackup_ShouldSetProperties()
    {
        var backup = new DatabaseBackup
        {
            BackupName = "crm_db_backup_20260201",
            FilePath = "/backups/crm_db_backup_20260201.sql.gz",
            FileSizeBytes = 1024 * 1024 * 500, // 500 MB
            SourceDatabase = "MariaDB",
            BackupType = "Full",
            CreatedByUserId = 1,
            Description = "Scheduled daily backup",
            IsCompressed = true,
            ChecksumHash = "sha256:abc123..."
        };

        backup.BackupName.Should().Be("crm_db_backup_20260201");
        backup.FilePath.Should().Contain(".sql.gz");
        backup.FileSizeBytes.Should().Be(524288000);
        backup.SourceDatabase.Should().Be("MariaDB");
        backup.BackupType.Should().Be("Full");
    }

    [Fact]
    public void DatabaseBackup_BackupTypes_ShouldBeValid()
    {
        var fullBackup = new DatabaseBackup { BackupType = "Full" };
        var incrementalBackup = new DatabaseBackup { BackupType = "Incremental" };
        var differentialBackup = new DatabaseBackup { BackupType = "Differential" };

        fullBackup.BackupType.Should().Be("Full");
        incrementalBackup.BackupType.Should().Be("Incremental");
        differentialBackup.BackupType.Should().Be("Differential");
    }

    #endregion

    #region BackupSchedule Entity Tests

    [Fact]
    public void BackupSchedule_ShouldInitializeWithDefaults()
    {
        var schedule = new BackupSchedule();

        schedule.Name.Should().Be(string.Empty);
        schedule.IsEnabled.Should().BeTrue();
        schedule.BackupType.Should().Be("Full");
        schedule.CronExpression.Should().Be("0 2 * * *");
        schedule.BackupPath.Should().Be("DatabaseBackups");
        schedule.RetentionDays.Should().Be(30);
        schedule.MaxBackupsToKeep.Should().Be(10);
        schedule.CompressBackups.Should().BeTrue();
        schedule.LastBackupAt.Should().BeNull();
        schedule.NextBackupAt.Should().BeNull();
        schedule.LastError.Should().BeNull();
        schedule.SuccessfulBackups.Should().Be(0);
        schedule.FailedBackups.Should().Be(0);
    }

    [Fact]
    public void BackupSchedule_ShouldSetProperties()
    {
        var lastBackup = DateTime.UtcNow.AddDays(-1);
        var nextBackup = DateTime.UtcNow.AddHours(2);

        var schedule = new BackupSchedule
        {
            Name = "Daily Production Backup",
            IsEnabled = true,
            BackupType = "Full",
            CronExpression = "0 3 * * *", // 3 AM daily
            BackupPath = "/mnt/backups/production",
            RetentionDays = 90,
            MaxBackupsToKeep = 30,
            CompressBackups = true,
            LastBackupAt = lastBackup,
            NextBackupAt = nextBackup,
            SuccessfulBackups = 365,
            FailedBackups = 2
        };

        schedule.Name.Should().Be("Daily Production Backup");
        schedule.CronExpression.Should().Be("0 3 * * *");
        schedule.RetentionDays.Should().Be(90);
        schedule.SuccessfulBackups.Should().Be(365);
    }

    [Fact]
    public void BackupSchedule_WithError_ShouldTrackFailure()
    {
        var schedule = new BackupSchedule
        {
            Name = "Failed Schedule",
            LastError = "Disk space insufficient",
            FailedBackups = 5
        };

        schedule.LastError.Should().Contain("Disk space");
        schedule.FailedBackups.Should().Be(5);
    }

    #endregion

    #region CalendarIntegration Entity Tests

    [Fact]
    public void CalendarIntegration_ShouldInitializeWithDefaults()
    {
        var integration = new CalendarIntegration();

        integration.UserId.Should().Be(0);
        integration.Provider.Should().Be(CalendarProvider.Google);
        integration.AccessToken.Should().Be(string.Empty);
        integration.RefreshToken.Should().Be(string.Empty);
        integration.SyncDirection.Should().Be(CalendarSyncDirection.Bidirectional);
        integration.LastSyncStatus.Should().Be(CalendarSyncStatus.Pending);
        integration.SyncIntervalMinutes.Should().Be(15);
        integration.IsActive.Should().BeTrue();
        integration.TotalEventsSynced.Should().Be(0);
    }

    [Fact]
    public void CalendarIntegration_Google_ShouldSetProperties()
    {
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var lastSync = DateTime.UtcNow.AddMinutes(-15);

        var integration = new CalendarIntegration
        {
            UserId = 1,
            Provider = CalendarProvider.Google,
            AccessToken = "ya29.access-token",
            RefreshToken = "1//refresh-token",
            TokenExpiresAt = expiresAt,
            CalendarId = "primary",
            CalendarName = "Work Calendar",
            ExternalEmail = "user@gmail.com",
            SyncDirection = CalendarSyncDirection.Bidirectional,
            LastSyncAt = lastSync,
            LastSyncStatus = CalendarSyncStatus.Success,
            TotalEventsSynced = 150
        };

        integration.Provider.Should().Be(CalendarProvider.Google);
        integration.CalendarId.Should().Be("primary");
        integration.ExternalEmail.Should().Be("user@gmail.com");
        integration.TotalEventsSynced.Should().Be(150);
    }

    [Fact]
    public void CalendarIntegration_Outlook_ShouldSetProperties()
    {
        var integration = new CalendarIntegration
        {
            Provider = CalendarProvider.Outlook,
            CalendarId = "AAkALgAAAAAAHYQD...calendarId",
            ExternalEmail = "user@outlook.com"
        };

        integration.Provider.Should().Be(CalendarProvider.Outlook);
        integration.ExternalEmail.Should().Contain("@outlook.com");
    }

    [Fact]
    public void CalendarIntegration_Collections_ShouldInitializeEmpty()
    {
        var integration = new CalendarIntegration();

        integration.SyncLogs.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region CalendarSyncLog Entity Tests

    [Fact]
    public void CalendarSyncLog_ShouldInitializeWithDefaults()
    {
        var log = new CalendarSyncLog();

        log.CalendarIntegrationId.Should().Be(0);
        log.Status.Should().Be(CalendarSyncStatus.Success);
        log.EventsCreated.Should().Be(0);
        log.EventsUpdated.Should().Be(0);
        log.EventsDeleted.Should().Be(0);
        log.ConflictsResolved.Should().Be(0);
        log.Direction.Should().Be(CalendarSyncDirection.Import);
    }

    [Fact]
    public void CalendarSyncLog_Successful_ShouldTrackMetrics()
    {
        var startTime = DateTime.UtcNow.AddSeconds(-30);
        var endTime = DateTime.UtcNow;

        var log = new CalendarSyncLog
        {
            CalendarIntegrationId = 1,
            StartedAt = startTime,
            CompletedAt = endTime,
            Status = CalendarSyncStatus.Success,
            EventsCreated = 5,
            EventsUpdated = 10,
            EventsDeleted = 2,
            ConflictsResolved = 1,
            Direction = CalendarSyncDirection.Bidirectional
        };

        log.Status.Should().Be(CalendarSyncStatus.Success);
        log.EventsCreated.Should().Be(5);
        log.EventsUpdated.Should().Be(10);
        log.ConflictsResolved.Should().Be(1);
    }

    [Fact]
    public void CalendarSyncLog_Failed_ShouldTrackError()
    {
        var log = new CalendarSyncLog
        {
            Status = CalendarSyncStatus.Failed,
            ErrorMessage = "OAuth token expired",
            ErrorStackTrace = "at CalendarService.RefreshToken()..."
        };

        log.Status.Should().Be(CalendarSyncStatus.Failed);
        log.ErrorMessage.Should().Contain("token expired");
    }

    #endregion

    #region CalendarEventMapping Entity Tests

    [Fact]
    public void CalendarEventMapping_ShouldInitializeWithDefaults()
    {
        var mapping = new CalendarEventMapping();

        mapping.ActivityId.Should().Be(0);
        mapping.CalendarIntegrationId.Should().Be(0);
        mapping.ExternalEventId.Should().Be(string.Empty);
        mapping.ExternalEventUid.Should().BeNull();
        mapping.ExternalETag.Should().BeNull();
        mapping.CreatedFromExternal.Should().BeFalse();
    }

    [Fact]
    public void CalendarEventMapping_ShouldSetProperties()
    {
        var syncTime = DateTime.UtcNow;
        var externalModified = DateTime.UtcNow.AddMinutes(-5);

        var mapping = new CalendarEventMapping
        {
            ActivityId = 100,
            CalendarIntegrationId = 1,
            ExternalEventId = "event123abc",
            ExternalEventUid = "uid-12345@google.com",
            ExternalETag = "\"etag-abc123\"",
            LastSyncedAt = syncTime,
            ExternalLastModified = externalModified,
            CrmLastModified = syncTime,
            CreatedFromExternal = true
        };

        mapping.ActivityId.Should().Be(100);
        mapping.ExternalEventId.Should().Be("event123abc");
        mapping.ExternalEventUid.Should().Contain("@google.com");
        mapping.CreatedFromExternal.Should().BeTrue();
    }

    [Fact]
    public void CalendarEventMapping_CrmCreated_ShouldNotBeFromExternal()
    {
        var mapping = new CalendarEventMapping
        {
            ActivityId = 50,
            ExternalEventId = "synced-event-123",
            CreatedFromExternal = false
        };

        mapping.CreatedFromExternal.Should().BeFalse();
    }

    #endregion
}
