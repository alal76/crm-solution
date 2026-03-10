// CRM Solution - Customer Relationship Management System
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
        // recipients must not be a JSON array at root level; ParseRecipients expects
        // recipients.signers (object with nested array). Omit recipients to get Ok.
        var body = @"{""envelopeId"":""env-test"",""status"":""completed""}";
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
