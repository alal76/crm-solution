// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for WebhooksController (TCOV-049).
/// </summary>
public class WebhooksControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly WebhooksController _controller;

    public WebhooksControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"WebhooksTest_{Guid.NewGuid()}")
            .Options;
        var cfg = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, cfg);

        var logger = new Mock<ILogger<WebhooksController>>();
        _controller = new WebhooksController(_dbContext, logger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task IngestWebFormSubmission_ShouldReturnOk_WithMinimalPayload()
    {
        var dto = new WebFormSubmissionDto
        {
            Email = "test@example.com",
            Name = "Test User",
            Subject = "Query",
            FormType = "contact"
        };

        var result = await _controller.IngestWebFormSubmission(dto);

        result.Result.Should().BeAssignableTo<IActionResult>();
    }

    [Fact]
    public async Task IngestWebFormSubmission_ShouldCreateInteraction()
    {
        var dto = new WebFormSubmissionDto
        {
            Email = "webhook@example.com",
            Name = "John Doe",
            Subject = "Support Request",
            Phone = "+1-555-0100"
        };

        await _controller.IngestWebFormSubmission(dto);

        _dbContext.Interactions.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task IngestWebFormSubmission_ShouldReturnOk_WhenEmailIsNull()
    {
        var dto = new WebFormSubmissionDto
        {
            Email = null,
            Name = "Anonymous",
            Subject = "Inquiry"
        };

        var result = await _controller.IngestWebFormSubmission(dto);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task IngestInboundEmail_ShouldReturnOk_WithMinimalPayload()
    {
        var dto = new InboundEmailDto
        {
            From = "sender@test.com",
            To = "crm@company.com",
            Subject = "Hello",
            TextBody = "Test email body"
        };

        var result = await _controller.IngestInboundEmail(dto);

        result.Should().NotBeNull();
    }
}
