// CRM Solution — Unit Tests
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for CalendarSyncService (TCOV-037).</summary>
public class CalendarSyncServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly CalendarSyncService _service;

    public CalendarSyncServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);

        var logger = new Mock<ILogger<CalendarSyncService>>().Object;
        var config = new ConfigurationBuilder().Build();
        var httpFactory = new Mock<IHttpClientFactory>().Object;

        _service = new CalendarSyncService(_context, logger, config, httpFactory);
    }

    public void Dispose() => _context.Dispose();

    // ── GetGoogleAuthUrlAsync ────────────────────────────────────────────────
    [Fact]
    public async Task GetGoogleAuthUrlAsync_ShouldReturnUrl_WithExpectedQueryParams()
    {
        var url = await _service.GetGoogleAuthUrlAsync(userId: 1);
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("accounts.google.com");
    }

    // ── GetOutlookAuthUrlAsync ───────────────────────────────────────────────
    [Fact]
    public async Task GetOutlookAuthUrlAsync_ShouldReturnUrl_WithExpectedQueryParams()
    {
        var url = await _service.GetOutlookAuthUrlAsync(userId: 1);
        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("login.microsoftonline.com");
    }

    // ── GetIntegrationAsync ──────────────────────────────────────────────────
    [Fact]
    public async Task GetIntegrationAsync_ShouldReturnNull_WhenNoIntegrationExists()
    {
        var result = await _service.GetIntegrationAsync(userId: 1, CalendarProvider.Google);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetIntegrationAsync_ShouldReturnIntegration_WhenItExists()
    {
        _context.CalendarIntegrations.Add(new CalendarIntegration
        {
            Id = 1,
            UserId = 5,
            Provider = CalendarProvider.Google,
            IsDeleted = false
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetIntegrationAsync(userId: 5, CalendarProvider.Google);
        result.Should().NotBeNull();
        result!.UserId.Should().Be(5);
    }

    // ── GetUserIntegrationsAsync ─────────────────────────────────────────────
    [Fact]
    public async Task GetUserIntegrationsAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetUserIntegrationsAsync(userId: 99);
        result.Should().BeEmpty();
    }

    // ── DisconnectAsync ──────────────────────────────────────────────────────
    [Fact]
    public async Task DisconnectAsync_ShouldReturnFalse_WhenNoIntegrationExists()
    {
        var result = await _service.DisconnectAsync(userId: 1, CalendarProvider.Outlook);
        result.Should().BeFalse();
    }
}
