// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Input;
using CRM.Infrastructure.Providers.Twilio;
using CRM.Infrastructure.Services.Integrations;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services.Integrations;

/// <summary>
/// Unit tests for <see cref="TwilioCallLoggingService"/>.
///
/// Twilio handling: the service calls <c>TwilioClient.Init()</c> in its constructor and
/// <c>CallResource.FetchAsync()</c> (by Call SID) to enrich log entries with authoritative
/// call data. There is no HttpClient injection — the Twilio SDK client is static — so, following
/// the same pattern established by <c>TwilioProviderTests</c> / <c>TwilioSmsServiceTests</c>,
/// these tests avoid ever reaching the real Twilio API by using either:
///   - an invalid configuration (AccountSid/AuthToken empty) → client never initializes, so the
///     Twilio fetch is always skipped and the caller-supplied event data is used verbatim; or
///   - a valid configuration with <c>TestMode = true</c> → the fetch is explicitly bypassed.
/// Both paths exercise the service's real fallback/merge logic without any network call.
///
/// MANDATORY: Written after verifying source for:
///   Class: TwilioCallLoggingService, Namespace: CRM.Infrastructure.Services.Integrations
///   Constructor: (IOptions&lt;TwilioConfiguration&gt;, ILogger&lt;TwilioCallLoggingService&gt;)
///   SDK: Twilio.TwilioClient.Init() + CallResource.FetchAsync() (skipped when not initialized/TestMode)
/// </summary>
public class TwilioCallLoggingServiceTests
{
    private static TwilioConfiguration InvalidConfig() => new()
    {
        AccountSid = string.Empty,
        AuthToken = string.Empty,
        FromPhoneNumber = string.Empty
    };

    private static TwilioConfiguration ValidTestModeConfig() => new()
    {
        AccountSid = "ACtest00000000000000000000000000",
        AuthToken = "auth_token_test_value_1234567890",
        FromPhoneNumber = "+12025550123",
        TestMode = true
    };

    private static TwilioCallLoggingService CreateService(TwilioConfiguration? config = null)
    {
        var options = Options.Create(config ?? InvalidConfig());
        var logger = new Mock<ILogger<TwilioCallLoggingService>>();
        return new TwilioCallLoggingService(options, logger.Object);
    }

    private static TwilioCallEvent SampleInboundEvent(string callSid = "CA0001") => new()
    {
        CallSid = callSid,
        From = "+12025550100",
        To = "+12025550199",
        Direction = "inbound",
        Status = "ringing",
        Duration = null,
        Timestamp = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        RecordingUrl = null
    };

    // ── Construction ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_DoesNotThrow_WhenConfigInvalid()
    {
        var act = () => CreateService(InvalidConfig());
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_DoesNotThrow_WhenConfigValidAndTestMode()
    {
        var act = () => CreateService(ValidTestModeConfig());
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_Throws_WhenOptionsNull()
    {
        var logger = new Mock<ILogger<TwilioCallLoggingService>>();
        var act = () => new TwilioCallLoggingService(null!, logger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Throws_WhenLoggerNull()
    {
        var options = Options.Create(InvalidConfig());
        var act = () => new TwilioCallLoggingService(options, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── LogInboundCallAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task LogInboundCallAsync_UsesEventData_WhenTwilioNotConfigured()
    {
        var service = CreateService(InvalidConfig());
        var callEvent = SampleInboundEvent();

        var id = await service.LogInboundCallAsync(callEvent);

        var history = await service.GetCallHistoryAsync(callEvent.From);
        var entry = history.Single(c => c.Id == id);

        entry.CallSid.Should().Be(callEvent.CallSid);
        entry.From.Should().Be(callEvent.From);
        entry.To.Should().Be(callEvent.To);
        entry.Direction.Should().Be("inbound");
        entry.Status.Should().Be(callEvent.Status);
        entry.StartedAt.Should().Be(callEvent.Timestamp);
        entry.EndedAt.Should().BeNull();
    }

    [Fact]
    public async Task LogInboundCallAsync_UsesEventData_WhenTestModeEnabled()
    {
        var service = CreateService(ValidTestModeConfig());
        var callEvent = SampleInboundEvent("CATestMode001");

        var id = await service.LogInboundCallAsync(callEvent);

        var history = await service.GetCallHistoryAsync(callEvent.From);
        var entry = history.Single(c => c.Id == id);

        entry.Status.Should().Be(callEvent.Status);
        entry.Direction.Should().Be("inbound");
    }

    [Fact]
    public async Task LogInboundCallAsync_ReturnsIncrementingIds_ForSuccessiveCalls()
    {
        var service = CreateService(InvalidConfig());

        var firstId = await service.LogInboundCallAsync(SampleInboundEvent("CA1"));
        var secondId = await service.LogInboundCallAsync(SampleInboundEvent("CA2"));

        secondId.Should().BeGreaterThan(firstId);
    }

    // ── LogOutboundCallAsync ────────────────────────────────────────────────

    [Fact]
    public async Task LogOutboundCallAsync_SetsDirectionOutbound()
    {
        var service = CreateService(InvalidConfig());
        var callEvent = SampleInboundEvent("CAOut001") with { Status = "in-progress" };

        var id = await service.LogOutboundCallAsync(callEvent);

        var history = await service.GetCallHistoryAsync(callEvent.From);
        var entry = history.Single(c => c.Id == id);

        entry.Direction.Should().Be("outbound");
        entry.Status.Should().Be("in-progress");
    }

    // ── UpdateCallStatusAsync ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateCallStatusAsync_UpdatesStatusAndDuration_WhenEntryExists()
    {
        var service = CreateService(InvalidConfig());
        var callEvent = SampleInboundEvent("CAUpdate001");
        await service.LogInboundCallAsync(callEvent);

        await service.UpdateCallStatusAsync(callEvent.CallSid, "completed", duration: 42);

        var history = await service.GetCallHistoryAsync(callEvent.From);
        var entry = history.Single(c => c.CallSid == callEvent.CallSid);

        entry.Status.Should().Be("completed");
        entry.Duration.Should().Be(42);
        entry.EndedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("completed")]
    [InlineData("failed")]
    [InlineData("no-answer")]
    [InlineData("busy")]
    public async Task UpdateCallStatusAsync_SetsEndedAt_ForTerminalStatuses(string terminalStatus)
    {
        var service = CreateService(InvalidConfig());
        var callEvent = SampleInboundEvent($"CATerm-{terminalStatus}");
        await service.LogInboundCallAsync(callEvent);

        await service.UpdateCallStatusAsync(callEvent.CallSid, terminalStatus, duration: 10);

        var history = await service.GetCallHistoryAsync(callEvent.From);
        var entry = history.Single(c => c.CallSid == callEvent.CallSid);

        entry.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateCallStatusAsync_DoesNotSetEndedAt_ForNonTerminalStatus()
    {
        var service = CreateService(InvalidConfig());
        var callEvent = SampleInboundEvent("CANonTerm001");
        await service.LogInboundCallAsync(callEvent);

        await service.UpdateCallStatusAsync(callEvent.CallSid, "ringing");

        var history = await service.GetCallHistoryAsync(callEvent.From);
        var entry = history.Single(c => c.CallSid == callEvent.CallSid);

        entry.EndedAt.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCallStatusAsync_DoesNotThrow_WhenCallNotFound()
    {
        var service = CreateService(InvalidConfig());

        var act = async () => await service.UpdateCallStatusAsync("CAUnknown999", "completed");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateCallStatusAsync_LeavesDurationNull_WhenNotConfiguredAndNoDurationSupplied()
    {
        // Terminal status with no explicit duration triggers a fallback fetch from Twilio;
        // with an invalid/unconfigured client that fetch is skipped and duration stays as-is.
        var service = CreateService(InvalidConfig());
        var callEvent = SampleInboundEvent("CANoDuration001");
        await service.LogInboundCallAsync(callEvent);

        await service.UpdateCallStatusAsync(callEvent.CallSid, "completed");

        var history = await service.GetCallHistoryAsync(callEvent.From);
        var entry = history.Single(c => c.CallSid == callEvent.CallSid);

        entry.Duration.Should().BeNull();
        entry.Status.Should().Be("completed");
    }

    // ── GetCallHistoryAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetCallHistoryAsync_FiltersByPhoneNumber()
    {
        var service = CreateService(InvalidConfig());
        await service.LogInboundCallAsync(SampleInboundEvent("CAHistA") with { From = "+10000000001" });
        await service.LogInboundCallAsync(SampleInboundEvent("CAHistB") with { From = "+10000000002" });

        var history = await service.GetCallHistoryAsync("+10000000001");

        history.Should().ContainSingle();
        history.Single().CallSid.Should().Be("CAHistA");
    }

    [Fact]
    public async Task GetCallHistoryAsync_FiltersByDateRange()
    {
        var service = CreateService(InvalidConfig());
        var early = SampleInboundEvent("CADateA") with { Timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        var late = SampleInboundEvent("CADateB") with { Timestamp = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), From = early.From };
        await service.LogInboundCallAsync(early);
        await service.LogInboundCallAsync(late);

        var history = await service.GetCallHistoryAsync(
            early.From,
            startDate: new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));

        history.Should().ContainSingle();
        history.Single().CallSid.Should().Be("CADateB");
    }

    // ── LinkCallToEntityAsync ───────────────────────────────────────────────

    [Fact]
    public async Task LinkCallToEntityAsync_UpdatesEntry_WhenCallExists()
    {
        var service = CreateService(InvalidConfig());
        var callEvent = SampleInboundEvent("CALink001");
        await service.LogInboundCallAsync(callEvent);

        await service.LinkCallToEntityAsync(callEvent.CallSid, "Contact", 55);

        var history = await service.GetCallHistoryAsync(callEvent.From);
        var entry = history.Single(c => c.CallSid == callEvent.CallSid);

        entry.LinkedEntityType.Should().Be("Contact");
        entry.LinkedEntityId.Should().Be(55);
    }

    // ── GetCallStatisticsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetCallStatisticsAsync_ComputesAggregates()
    {
        var service = CreateService(InvalidConfig());
        var periodStart = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc);

        var inbound = SampleInboundEvent("CAStatIn") with { Timestamp = periodStart.AddDays(1) };
        var outbound = SampleInboundEvent("CAStatOut") with { Timestamp = periodStart.AddDays(2) };
        await service.LogInboundCallAsync(inbound);
        await service.LogOutboundCallAsync(outbound);

        await service.UpdateCallStatusAsync(inbound.CallSid, "completed", duration: 60);
        await service.UpdateCallStatusAsync(outbound.CallSid, "no-answer");

        var stats = await service.GetCallStatisticsAsync(periodStart, periodEnd);

        stats.TotalCalls.Should().Be(2);
        stats.InboundCalls.Should().Be(1);
        stats.OutboundCalls.Should().Be(1);
        stats.TotalDurationSeconds.Should().Be(60);
        stats.MissedCalls.Should().Be(1);
    }

    [Fact]
    public async Task GetCallStatisticsAsync_ReturnsZeroTotals_WhenNoCallsInRange()
    {
        var service = CreateService(InvalidConfig());

        var stats = await service.GetCallStatisticsAsync(
            new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 31, 0, 0, 0, DateTimeKind.Utc));

        stats.TotalCalls.Should().Be(0);
        stats.AverageDurationSeconds.Should().Be(0);
        stats.TotalDurationSeconds.Should().Be(0);
    }
}
