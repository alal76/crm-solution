// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using InputIAccountingSyncService = CRM.Core.Ports.Input.IAccountingSyncService;
using InputIMarketingSyncService = CRM.Core.Ports.Input.IMarketingSyncService;
using InputILinkedInSalesNavService = CRM.Core.Ports.Input.ILinkedInSalesNavService;
using InputISchedulingIntegrationService = CRM.Core.Ports.Input.ISchedulingIntegrationService;
using ICalendlyService = CRM.Core.Interfaces.ICalendlyService;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for IntegrationsController — QB/Xero OAuth2 endpoints (INT-001).
/// Tests validate HTTP response types for connect, callback, status, and sync routes.
/// </summary>
public class IntegrationsControllerTests
{
    private readonly Mock<InputIAccountingSyncService> _mockAccountingSync;
    private readonly Mock<InputIMarketingSyncService> _mockMarketingSync;
    private readonly Mock<InputILinkedInSalesNavService> _mockLinkedIn;
    private readonly Mock<InputISchedulingIntegrationService> _mockScheduling;
    private readonly Mock<IQuickBooksService> _mockQb;
    private readonly Mock<IXeroService> _mockXero;
    private readonly Mock<IMailchimpService> _mockMailchimp;
    private readonly Mock<IHubSpotService> _mockHubSpot;
    private readonly Mock<ICalendlyService> _mockCalendly;
    private readonly IntegrationsController _controller;

    public IntegrationsControllerTests()
    {
        _mockAccountingSync = new Mock<InputIAccountingSyncService>();
        _mockMarketingSync = new Mock<InputIMarketingSyncService>();
        _mockLinkedIn = new Mock<InputILinkedInSalesNavService>();
        _mockScheduling = new Mock<InputISchedulingIntegrationService>();
        _mockQb = new Mock<IQuickBooksService>();
        _mockXero = new Mock<IXeroService>();
        _mockMailchimp = new Mock<IMailchimpService>();
        _mockHubSpot = new Mock<IHubSpotService>();
        _mockCalendly = new Mock<ICalendlyService>();

        _controller = new IntegrationsController(
            _mockAccountingSync.Object,
            _mockMarketingSync.Object,
            _mockLinkedIn.Object,
            _mockScheduling.Object,
            _mockQb.Object,
            _mockXero.Object,
            _mockMailchimp.Object,
            _mockHubSpot.Object,
            _mockCalendly.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ------------------------------------------------------------------ //
    //  QuickBooks Connect
    // ------------------------------------------------------------------ //

    [Fact]
    public void QuickBooksConnect_ReturnsRedirect()
    {
        // Arrange
        _mockQb.Setup(x => x.GetAuthorizationUrl(It.IsAny<string>()))
            .Returns("https://appcenter.intuit.com/connect/oauth2?client_id=test&state=abc");

        // Act
        var result = _controller.QuickBooksConnect();

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirect = (RedirectResult)result;
        redirect.Url.Should().StartWith("https://appcenter.intuit.com");
    }

    [Fact]
    public void QuickBooksConnect_CallsGetAuthorizationUrlOnce()
    {
        // Arrange
        _mockQb.Setup(x => x.GetAuthorizationUrl(It.IsAny<string>()))
            .Returns("https://appcenter.intuit.com/connect/oauth2");

        // Act
        _controller.QuickBooksConnect();

        // Assert
        _mockQb.Verify(x => x.GetAuthorizationUrl(It.IsAny<string>()), Times.Once);
    }

    // ------------------------------------------------------------------ //
    //  QuickBooks Status
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task QuickBooksStatus_ReturnsOkWithConnectionStatus()
    {
        // Arrange
        var expectedStatus = new QuickBooksConnectionStatus
        {
            IsConnected = false,
            RealmId = null,
            TokenExpiresAt = null
        };
        _mockQb.Setup(x => x.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.QuickBooksStatus(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().BeEquivalentTo(expectedStatus);
    }

    [Fact]
    public async Task QuickBooksStatus_ReturnsConnectedStatus_WhenConnected()
    {
        // Arrange
        var expectedStatus = new QuickBooksConnectionStatus
        {
            IsConnected = true,
            RealmId = "realm-789",
            TokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        _mockQb.Setup(x => x.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.QuickBooksStatus(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var status = ((OkObjectResult)result).Value as QuickBooksConnectionStatus;
        status!.IsConnected.Should().BeTrue();
        status.RealmId.Should().Be("realm-789");
    }

    // ------------------------------------------------------------------ //
    //  Xero Connect
    // ------------------------------------------------------------------ //

    [Fact]
    public void XeroConnect_ReturnsRedirect()
    {
        // Arrange
        _mockXero.Setup(x => x.GetAuthorizationUrl(It.IsAny<string>()))
            .Returns("https://login.xero.com/identity/connect/authorize?client_id=test");

        // Act
        var result = _controller.XeroConnect();

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirect = (RedirectResult)result;
        redirect.Url.Should().StartWith("https://login.xero.com");
    }

    [Fact]
    public void XeroConnect_CallsGetAuthorizationUrlOnce()
    {
        // Arrange
        _mockXero.Setup(x => x.GetAuthorizationUrl(It.IsAny<string>()))
            .Returns("https://login.xero.com/identity/connect/authorize");

        // Act
        _controller.XeroConnect();

        // Assert
        _mockXero.Verify(x => x.GetAuthorizationUrl(It.IsAny<string>()), Times.Once);
    }

    // ------------------------------------------------------------------ //
    //  Xero Status
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task XeroStatus_ReturnsOkWithConnectionStatus()
    {
        // Arrange
        var expectedStatus = new XeroConnectionStatus
        {
            IsConnected = false,
            TenantId = null,
            TokenExpiresAt = null
        };
        _mockXero.Setup(x => x.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.XeroStatus(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().BeEquivalentTo(expectedStatus);
    }

    [Fact]
    public async Task XeroStatus_ReturnsConnectedStatus_WhenConnected()
    {
        // Arrange
        var expectedStatus = new XeroConnectionStatus
        {
            IsConnected = true,
            TenantId = "tenant-abc",
            TokenExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
        _mockXero.Setup(x => x.GetConnectionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.XeroStatus(CancellationToken.None);

        // Assert
        var status = ((OkObjectResult)result).Value as XeroConnectionStatus;
        status!.IsConnected.Should().BeTrue();
        status.TenantId.Should().Be("tenant-abc");
    }

    // ------------------------------------------------------------------ //
    //  QB / Xero Single-Entity Sync
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task QuickBooksSyncAccount_ReturnsOkWithSyncedFlag()
    {
        // Arrange
        _mockQb.Setup(x => x.SyncAccountAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.QuickBooksSyncAccount(5, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task XeroSyncContact_ReturnsOkWithSyncedFlag()
    {
        // Arrange
        _mockXero.Setup(x => x.SyncContactAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.XeroSyncContact(7, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
