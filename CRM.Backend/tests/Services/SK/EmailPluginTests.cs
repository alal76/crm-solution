// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.AI.SK.Plugins;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for the EmailPlugin Semantic Kernel plugin.
/// </summary>
public class EmailPluginTests
{
    private readonly Mock<IEmailTemplateService> _emailTemplateSvcMock;
    private readonly Mock<INotificationPort> _notificationPortMock;
    private readonly Mock<ILogger<EmailPlugin>> _loggerMock;
    private readonly EmailPlugin _sut;

    public EmailPluginTests()
    {
        _emailTemplateSvcMock = new Mock<IEmailTemplateService>();
        _notificationPortMock = new Mock<INotificationPort>();
        _loggerMock = new Mock<ILogger<EmailPlugin>>();
        _sut = new EmailPlugin(_emailTemplateSvcMock.Object, _notificationPortMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenEmailTemplateServiceIsNull()
    {
        var act = () => new EmailPlugin(null!, _notificationPortMock.Object, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("emailTemplateService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNotificationPortIsNull()
    {
        var act = () => new EmailPlugin(_emailTemplateSvcMock.Object, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("notificationPort");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new EmailPlugin(_emailTemplateSvcMock.Object, _notificationPortMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Plugin Metadata Tests

    [Fact]
    public void PluginName_ShouldReturn_Email()
    {
        _sut.PluginName.Should().Be("Email");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetEmailTemplatesAsync Tests

    [Fact]
    public async Task GetEmailTemplatesAsync_ShouldReturnSuccessJson_WithActiveTemplates()
    {
        var templates = new List<EmailTemplate>
        {
            new EmailTemplate { Id = 1, Name = "Welcome Email", Subject = "Welcome!", Category = EmailTemplateCategory.Transactional, IsActive = true, Slug = "welcome-email" }
        };
        _emailTemplateSvcMock
            .Setup(s => s.GetAllAsync(It.IsAny<EmailTemplateCategory?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var result = await _sut.GetEmailTemplatesAsync(activeOnly: true);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task GetEmailTemplatesAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _emailTemplateSvcMock
            .Setup(s => s.GetAllAsync(It.IsAny<EmailTemplateCategory?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service unavailable"));

        var result = await _sut.GetEmailTemplatesAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("GetEmailTemplates");
    }

    #endregion

    #region SearchTemplatesAsync Tests

    [Fact]
    public async Task SearchTemplatesAsync_ShouldReturnSuccessJson_WithMatchingTemplates()
    {
        var templates = new List<EmailTemplate>
        {
            new EmailTemplate { Id = 1, Name = "Welcome Email", Subject = "Welcome!", Category = EmailTemplateCategory.Transactional, IsActive = true, Slug = "welcome-email" },
            new EmailTemplate { Id = 2, Name = "Reset Password", Subject = "Password Reset", Category = EmailTemplateCategory.Transactional, IsActive = true, Slug = "reset-password" }
        };
        _emailTemplateSvcMock
            .Setup(s => s.GetAllAsync(It.IsAny<EmailTemplateCategory?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var result = await _sut.SearchTemplatesAsync("welcome");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task SearchTemplatesAsync_ShouldReturnZeroCount_WhenNoMatch()
    {
        var templates = new List<EmailTemplate>
        {
            new EmailTemplate { Id = 1, Name = "Welcome Email", Subject = "Welcome!", Category = EmailTemplateCategory.Transactional, IsActive = true, Slug = "welcome" }
        };
        _emailTemplateSvcMock
            .Setup(s => s.GetAllAsync(It.IsAny<EmailTemplateCategory?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var result = await _sut.SearchTemplatesAsync("xyznomatch");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task SearchTemplatesAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _emailTemplateSvcMock
            .Setup(s => s.GetAllAsync(It.IsAny<EmailTemplateCategory?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _sut.SearchTemplatesAsync("anything");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region PreviewTemplateAsync Tests

    [Fact]
    public async Task PreviewTemplateAsync_ShouldReturnSuccessJson_WhenPreviewSucceeds()
    {
        var rendered = new RenderedEmail
        {
            Subject = "Welcome to CRM!",
            HtmlBody = "<p>Hello World</p>",
            TextBody = "Hello World",
            Warnings = new List<string>()
        };
        _emailTemplateSvcMock
            .Setup(s => s.PreviewAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rendered);

        var result = await _sut.PreviewTemplateAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("subject").GetString().Should().Be("Welcome to CRM!");
    }

    [Fact]
    public async Task PreviewTemplateAsync_ShouldReturnErrorJson_WhenTemplateNotFound()
    {
        _emailTemplateSvcMock
            .Setup(s => s.PreviewAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Template not found"));

        var result = await _sut.PreviewTemplateAsync(999);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("PreviewTemplate");
    }

    #endregion

    #region SendEmailAsync Tests

    [Fact]
    public async Task SendEmailAsync_ShouldReturnSuccessJson_WhenEmailSent()
    {
        var notifResult = new NotificationResult { Success = true, MessageId = "email-001" };
        _notificationPortMock
            .Setup(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifResult);

        var result = await _sut.SendEmailAsync("user@example.com", "Test Subject", "<p>Hello</p>");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetProperty("messageId").GetString().Should().Be("email-001");
    }

    [Fact]
    public async Task SendEmailAsync_ShouldReturnErrorJson_WhenPortThrows()
    {
        _notificationPortMock
            .Setup(p => p.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP connection failed"));

        var result = await _sut.SendEmailAsync("a@b.com", "Subject", "Body");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("SendEmail");
    }

    #endregion

    #region SendTemplateEmailAsync Tests

    [Fact]
    public async Task SendTemplateEmailAsync_ShouldReturnSuccessWithSuccessFalse_WhenTemplateNotFound()
    {
        _emailTemplateSvcMock
            .Setup(s => s.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailTemplate?)null);
        _emailTemplateSvcMock
            .Setup(s => s.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailTemplate?)null);

        var result = await _sut.SendTemplateEmailAsync("nonexistent-template", "user@example.com");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ShouldReturnSuccessJson_WhenTemplateFoundByName()
    {
        var template = new EmailTemplate
        {
            Id = 3,
            Name = "Welcome Email",
            Subject = "Welcome!",
            Category = EmailTemplateCategory.Transactional,
            IsActive = true,
            Slug = "welcome-email"
        };
        _emailTemplateSvcMock
            .Setup(s => s.GetByNameAsync("Welcome Email", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        var notifResult = new NotificationResult { Success = true, MessageId = "tmpl-001" };
        _notificationPortMock
            .Setup(p => p.SendTemplateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifResult);

        var result = await _sut.SendTemplateEmailAsync("Welcome Email", "user@example.com");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ShouldReturnSuccessJson_WhenTemplateFoundBySlug()
    {
        var template = new EmailTemplate
        {
            Id = 4,
            Name = "Welcome Email",
            Subject = "Welcome!",
            Category = EmailTemplateCategory.Transactional,
            IsActive = true,
            Slug = "welcome-email"
        };
        _emailTemplateSvcMock
            .Setup(s => s.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailTemplate?)null);
        _emailTemplateSvcMock
            .Setup(s => s.GetBySlugAsync("welcome-email", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        var notifResult = new NotificationResult { Success = true, MessageId = "tmpl-002" };
        _notificationPortMock
            .Setup(p => p.SendTemplateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifResult);

        var result = await _sut.SendTemplateEmailAsync("welcome-email", "user@example.com");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ShouldReturnErrorJson_WhenPortThrows()
    {
        var template = new EmailTemplate { Id = 1, Name = "Test", Subject = "Test", Category = EmailTemplateCategory.Transactional, IsActive = true, Slug = "test" };
        _emailTemplateSvcMock
            .Setup(s => s.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _notificationPortMock
            .Setup(p => p.SendTemplateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email delivery failed"));

        var result = await _sut.SendTemplateEmailAsync("Test", "a@b.com");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion
}
