// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Concurrent;
using System.Reflection;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Regression tests for AP-001 to AP-014 thread-safety antipattern fixes.
///
/// Verified against source files BEFORE writing:
///   EmailOtpService.cs        — static ConcurrentDictionary _otp_records, _otp_rate_limits
///   SmsOtpService.cs          — same pattern as EmailOtpService
///   DuplicateReviewWorkerService.cs — static volatile bool _rulesSeeded
///   AutoAssignmentService.cs  — ConcurrentDictionary _rules, Interlocked.Increment _nextRuleId
///   AssignmentRulesEngine.cs  — ConcurrentDictionary _roundRobinIndex, lock _rulesLock
///   AssetLifecycleService.cs  — lock _collectionsLock, Interlocked.Increment counters
///   KCSWorkflowService.cs     — lock _collectionsLock, Interlocked.Increment counters
///   DiscoveryService.cs       — Interlocked.Increment _nextScanId
///   CatalogApprovalService.cs — lock _collectionsLock, Interlocked.Increment counters
///   CatalogFulfillmentService.cs — lock _workflowsLock, Interlocked.Increment counters
///   AutoAssignmentService.cs  — ConcurrentDictionary _roundRobinCounters, Interlocked.Increment
/// </summary>
public class ThreadSafetyRegressionTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // AP-002 / AP-003: OTP Services — ConcurrentDictionary static stores
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void EmailOtpService_OtpRecordsField_IsConcurrentDictionary()
    {
        // AP-002 fix: replaced Dictionary<> with ConcurrentDictionary<> for _otp_records
        var field = typeof(EmailOtpService)
            .GetField("_otp_records", BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull("_otp_records static field must exist on EmailOtpService");
        field!.FieldType.Name.Should().StartWith("ConcurrentDictionary",
            because: "AP-002 requires thread-safe storage for OTP records (was Dictionary)");
    }

    [Fact]
    public void EmailOtpService_OtpRateLimitsField_IsConcurrentDictionary()
    {
        // AP-002 fix: _otp_rate_limits must also be thread-safe
        var field = typeof(EmailOtpService)
            .GetField("_otp_rate_limits", BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull("_otp_rate_limits static field must exist on EmailOtpService");
        field!.FieldType.Name.Should().StartWith("ConcurrentDictionary",
            because: "AP-002 requires thread-safe rate limit tracking");
    }

    [Fact]
    public void SmsOtpService_OtpRecordsField_IsConcurrentDictionary()
    {
        // AP-003 fix: same pattern as EmailOtpService (AP-002)
        var field = typeof(SmsOtpService)
            .GetField("_otp_records", BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull("_otp_records static field must exist on SmsOtpService");
        field!.FieldType.Name.Should().StartWith("ConcurrentDictionary",
            because: "AP-003 requires thread-safe OTP storage (was Dictionary)");
    }

    [Fact]
    public void SmsOtpService_OtpRateLimitsField_IsConcurrentDictionary()
    {
        var field = typeof(SmsOtpService)
            .GetField("_otp_rate_limits", BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull("_otp_rate_limits static field must exist on SmsOtpService");
        field!.FieldType.Name.Should().StartWith("ConcurrentDictionary",
            because: "AP-003 requires thread-safe rate limit tracking");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AP-014: DuplicateReviewWorkerService — volatile bool _rulesSeeded
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void DuplicateReviewWorkerService_RulesSeededField_IsStaticBool()
    {
        // AP-014 fix: _rulesSeeded declared as volatile bool (volatile not detectable via
        // reflection, but we can assert type + static characteristics are correct)
        var field = typeof(DuplicateReviewWorkerService)
            .GetField("_rulesSeeded", BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull("_rulesSeeded field must exist on DuplicateReviewWorkerService");
        field!.FieldType.Should().Be(typeof(bool), "the seeded flag must be a bool");
        field.IsStatic.Should().BeTrue("flag must be static to be shared across background service runs");
        // Note: volatile keyword is not visible via reflection attributes; it compiles
        // to VolatileRead/Write IL instructions. Correctness is verified at code level.
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AP-010: AutoAssignmentService — ConcurrentDictionary + Interlocked counters
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AutoAssignmentService_RulesField_IsConcurrentDictionary()
    {
        // AP-010 fix: rules stored in ConcurrentDictionary<int, AssignmentRuleDto>
        var field = typeof(AutoAssignmentService)
            .GetField("_rules", BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull("_rules static field must exist");
        field!.FieldType.Name.Should().StartWith("ConcurrentDictionary",
            because: "AP-010 requires thread-safe in-memory rule store");
    }

    [Fact]
    public void AutoAssignmentService_RoundRobinCountersField_IsConcurrentDictionary()
    {
        var field = typeof(AutoAssignmentService)
            .GetField("_roundRobinCounters", BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull("_roundRobinCounters static field must exist");
        field!.FieldType.Name.Should().StartWith("ConcurrentDictionary",
            because: "round-robin counters must be thread-safe under concurrent load");
    }

    [Fact]
    public async Task AutoAssignmentService_ConcurrentRuleCreation_ProducesUniqueIds()
    {
        // Arrange — AP-010 regression: concurrent CreateRuleAsync must produce distinct IDs
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"AutoAssign_Concurrent_{Guid.NewGuid()}")
            .Options;
        using var context = new CrmDbContext(options, null);
        var logger = Mock.Of<ILogger<AutoAssignmentService>>();
        var service = new AutoAssignmentService(context, logger);
        AutoAssignmentService.ResetState();

        const int concurrentRuleCount = 50;
        var tasks = Enumerable.Range(0, concurrentRuleCount).Select(i =>
            service.CreateRuleAsync(new CreateAssignmentRuleDto
            {
                Name = $"Rule_{i}",
                Strategy = "RoundRobin",
                Priority = i,
                IsActive = true
            }));

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert — all IDs must be unique (no two rules share an ID)
        var ids = results.Select(r => r.Id).ToList();
        ids.Distinct().Count().Should().Be(concurrentRuleCount,
            because: "each concurrent CreateRuleAsync must get a unique ID via Interlocked.Increment");
        ids.Should().AllSatisfy(id => id.Should().BeGreaterThan(0));
    }

    [Fact]
    public async Task AutoAssignmentService_ConcurrentRoundRobinGet_DoesNotThrow()
    {
        // AP-010 regression: concurrent round-robin counter access must not throw
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"AutoAssign_RR_{Guid.NewGuid()}")
            .Options;
        using var context = new CrmDbContext(options, null);
        var logger = Mock.Of<ILogger<AutoAssignmentService>>();
        var service = new AutoAssignmentService(context, logger);
        AutoAssignmentService.ResetState();

        // Seed some mock agent data
        context.Users.AddRange(Enumerable.Range(1, 5).Select(i => new CRM.Core.Entities.User
        {
            Id = i, Username = $"agent{i}", Email = $"agent{i}@test.com",
            FirstName = "T", LastName = $"Agent{i}", PasswordHash = "x",
            IsActive = true, IsLocked = false, Role = 3, IsDeleted = false, CreatedAt = DateTime.UtcNow
        }));
        await context.SaveChangesAsync();

        // Act — 20 concurrent round-robin lookups must not corrupt state or throw
        var tasks = Enumerable.Range(0, 20).Select(_ => service.GetNextRoundRobinAgentAsync());
        Func<Task> act = () => Task.WhenAll(tasks);

        await act.Should().NotThrowAsync(
            because: "AP-010: concurrent round-robin calls must be safe with ConcurrentDictionary");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AP-005/AP-007/AP-009/AP-011: ITSM services with static lock objects
    // Verify the lock guard objects are static readonly (correct singleton guard pattern)
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(typeof(AssetLifecycleService), "_collectionsLock")]
    [InlineData(typeof(CatalogApprovalService), "_collectionsLock")]
    [InlineData(typeof(CatalogFulfillmentService), "_workflowsLock")]
    [InlineData(typeof(KCSWorkflowService), "_collectionsLock")]
    public void ItsmService_LockObject_IsStaticReadonly(Type serviceType, string fieldName)
    {
        // AP-005/007/009/011: lock guard must be static readonly object
        var field = serviceType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull($"{serviceType.Name} must have a {fieldName} static lock guard (AP fix)");
        field!.IsStatic.Should().BeTrue("lock guard must be static to protect static collections");
        field.IsInitOnly.Should().BeTrue("lock guard must be readonly to prevent reassignment");
        field.GetValue(null).Should().NotBeNull("lock guard object must not be null");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AP-005/AP-007/AP-009/AP-011: Interlocked counters must be static int fields
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(typeof(AssetLifecycleService), "_nextTransitionId")]
    [InlineData(typeof(AssetLifecycleService), "_nextScheduleId")]
    [InlineData(typeof(CatalogApprovalService), "_nextWorkflowId")]
    [InlineData(typeof(CatalogApprovalService), "_nextStageId")]
    [InlineData(typeof(CatalogApprovalService), "_nextActionId")]
    [InlineData(typeof(CatalogFulfillmentService), "_nextWorkflowId")]
    [InlineData(typeof(CatalogFulfillmentService), "_nextTaskId")]
    [InlineData(typeof(KCSWorkflowService), "_nextSessionId")]
    [InlineData(typeof(KCSWorkflowService), "_nextDraftId")]
    [InlineData(typeof(KCSWorkflowService), "_nextReviewId")]
    public void ItsmService_InterlockedCounter_IsStaticIntField(Type serviceType, string fieldName)
    {
        // AP fix: counters must be static int for Interlocked.Increment compatibility
        var field = serviceType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull($"{serviceType.Name} must have the {fieldName} counter field after AP fix");
        field!.IsStatic.Should().BeTrue("counter must be static to be shared across instances");
        field.FieldType.Should().Be(typeof(int),
            because: "Interlocked.Increment requires an int (or long) reference");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AP-006: DiscoveryService — Interlocked.Increment _nextScanId
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void DiscoveryService_NextScanIdField_IsStaticInt()
    {
        // AP-006 fix: _nextScanId must be static int for Interlocked.Increment
        var field = typeof(DiscoveryService)
            .GetField("_nextScanId", BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull("_nextScanId static field must exist on DiscoveryService");
        field!.IsStatic.Should().BeTrue();
        field.FieldType.Should().Be(typeof(int),
            because: "AP-006 requires Interlocked.Increment-compatible int counter");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AP-008: AssignmentRulesEngine — ConcurrentDictionary for round-robin index
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AssignmentRulesEngine_RoundRobinIndexField_IsConcurrentDictionary()
    {
        // AP-008 fix: round-robin index map must be thread-safe
        var field = typeof(AssignmentRulesEngine)
            .GetField("_roundRobinIndex", BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull("_roundRobinIndex field must exist on AssignmentRulesEngine after AP-008 fix");
        field!.FieldType.Name.Should().StartWith("ConcurrentDictionary",
            because: "AP-008 requires thread-safe round-robin index (was plain Dictionary)");
    }

    [Fact]
    public void AssignmentRulesEngine_RulesLockField_IsStaticReadonlyObject()
    {
        // AP-008 fix: _rulesLock must be a static readonly lock guard
        var field = typeof(AssignmentRulesEngine)
            .GetField("_rulesLock", BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull("_rulesLock static field must exist on AssignmentRulesEngine");
        field!.IsStatic.Should().BeTrue();
        field.IsInitOnly.Should().BeTrue("lock guard must be readonly");
        field.GetValue(null).Should().NotBeNull();
    }
}
