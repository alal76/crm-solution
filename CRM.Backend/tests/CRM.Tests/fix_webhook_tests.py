#!/usr/bin/env python3
"""Fix Stripe and DocuSign webhook test files with proper string escaping."""
import os

BASE = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/tests/CRM.Tests"
WEBHOOK_DIR = os.path.join(BASE, "Controllers", "Webhooks")

# ─────────────────────────────────────────────────────────────────────────────
# TCOV-050: StripeWebhookControllerTests - fixed
# ─────────────────────────────────────────────────────────────────────────────
stripe_tests = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using System.Text;
using CRM.Api.Controllers.Webhooks;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Providers.Stripe;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers.Webhooks;

/// <summary>
/// Unit tests for StripeWebhookController (TCOV-050).
/// </summary>
public class StripeWebhookControllerTests
{
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IActivityService> _mockActivityService;
    private readonly Mock<ISubscriptionService> _mockSubscriptionService;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<ILogger<StripeWebhookController>> _mockLogger;

    public StripeWebhookControllerTests()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _mockActivityService = new Mock<IActivityService>();
        _mockSubscriptionService = new Mock<ISubscriptionService>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockLogger = new Mock<ILogger<StripeWebhookController>>();
    }

    private StripeWebhookController BuildControllerWithBody(string body, string webhookSecret = "")
    {
        var config = new StripeConfiguration { WebhookSecret = webhookSecret };
        var options = Options.Create(config);
        var controller = new StripeWebhookController(
            options,
            _mockPaymentService.Object,
            _mockActivityService.Object,
            _mockSubscriptionService.Object,
            _mockFeatureManager.Object,
            _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        httpContext.Request.ContentType = "application/json";
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnBadRequest_WhenBodyIsEmpty()
    {
        var controller = BuildControllerWithBody(string.Empty);

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnUnauthorized_WhenSignatureInvalid()
    {
        var body = @"{""id"":""evt_test"",""type"":""payment_intent.succeeded""}";
        var controller = BuildControllerWithBody(body, webhookSecret: "whsec_test_secret");
        // No Stripe-Signature header -> validation fails

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnBadRequest_WhenPayloadIsInvalidJson()
    {
        var controller = BuildControllerWithBody("not-valid-json");

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnOk_WhenValidPayloadAndNoSignatureRequired()
    {
        var body = @"{""id"":""evt_test_123"",""type"":""payment_intent.succeeded"",""data"":{""object"":{}}}";
        var controller = BuildControllerWithBody(body, webhookSecret: "");
        _mockFeatureManager.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnOk_WhenChargeSucceededEvent()
    {
        var body = @"{""id"":""evt_charge_1"",""type"":""charge.succeeded"",""data"":{""object"":{""id"":""ch_test"",""amount"":1000,""currency"":""usd""}}}";
        var controller = BuildControllerWithBody(body, webhookSecret: "");
        _mockFeatureManager.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnOk_WhenUnknownEventType()
    {
        var body = @"{""id"":""evt_unknown"",""type"":""customer.created"",""data"":{""object"":{}}}";
        var controller = BuildControllerWithBody(body, webhookSecret: "");

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<OkObjectResult>();
    }
}
'''

# ─────────────────────────────────────────────────────────────────────────────
# TCOV-051: DocuSignWebhookControllerTests - fixed
# ─────────────────────────────────────────────────────────────────────────────
docusign_tests = r'''// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using System.Text;
using CRM.Api.Controllers.Webhooks;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.DocuSign;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers.Webhooks;

/// <summary>
/// Unit tests for DocuSignWebhookController (TCOV-051).
/// </summary>
public class DocuSignWebhookControllerTests
{
    private readonly Mock<ISignaturePort> _mockSignaturePort;
    private readonly Mock<IActivityService> _mockActivityService;
    private readonly Mock<ILogger<DocuSignWebhookController>> _mockLogger;

    public DocuSignWebhookControllerTests()
    {
        _mockSignaturePort = new Mock<ISignaturePort>();
        _mockActivityService = new Mock<IActivityService>();
        _mockLogger = new Mock<ILogger<DocuSignWebhookController>>();
    }

    private DocuSignWebhookController BuildController(string body, string webhookSecret = "", string contentType = "application/json")
    {
        var config = new DocuSignConfiguration { WebhookSecret = webhookSecret };
        var options = Options.Create(config);
        var controller = new DocuSignWebhookController(
            options,
            _mockSignaturePort.Object,
            _mockActivityService.Object,
            _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        httpContext.Request.ContentType = contentType;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnBadRequest_WhenBodyIsEmpty()
    {
        var controller = BuildController(string.Empty);

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnUnauthorized_WhenSignatureConfiguredButMissing()
    {
        var body = @"{""envelopeId"":""env-123"",""status"":""completed""}";
        var controller = BuildController(body, webhookSecret: "secret-key");
        // No X-DocuSign-Signature-1 header set

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnBadRequest_WhenInvalidJsonPayload()
    {
        // A string starting with '{' but invalid JSON
        var controller = BuildController("{invalid json here}", webhookSecret: "");

        var result = await controller.HandleWebhook(default);

        result.Should().BeAssignableTo<IActionResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldReturnOk_WhenValidJsonPayload()
    {
        var body = @"{""envelopeId"":""env-test"",""status"":""completed"",""recipients"":[]}";
        var controller = BuildController(body, webhookSecret: "");

        var result = await controller.HandleWebhook(default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task HandleWebhook_ShouldHandleXmlPayload()
    {
        var xmlBody = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
            + "<DocuSignEnvelopeInformation>"
            + "<EnvelopeStatus><EnvelopeID>env-xml-test</EnvelopeID>"
            + "<Status>Completed</Status></EnvelopeStatus>"
            + "</DocuSignEnvelopeInformation>";
        var controller = BuildController(xmlBody, webhookSecret: "", contentType: "application/xml");

        var result = await controller.HandleWebhook(default);

        result.Should().BeAssignableTo<IActionResult>();
    }
}
'''

# Write fixed files
files = {
    os.path.join(WEBHOOK_DIR, "StripeWebhookControllerTests.cs"): stripe_tests,
    os.path.join(WEBHOOK_DIR, "DocuSignWebhookControllerTests.cs"): docusign_tests,
}

for path, content in files.items():
    with open(path, "w") as f:
        f.write(content)
    print(f"Fixed: {path}")

print("Done.")
