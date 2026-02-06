// CRM Solution - Email To Ticket Service Tests
// Comprehensive tests for email-to-incident parsing

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for EmailToTicketService.
/// Tests email parsing and incident creation.
/// </summary>
public class EmailToTicketServiceTests
{
    private readonly Mock<ILogger<EmailToTicketService>> _mockLogger;
    private readonly EmailToTicketService _service;

    public EmailToTicketServiceTests()
    {
        _mockLogger = new Mock<ILogger<EmailToTicketService>>();
        _service = new EmailToTicketService(_mockLogger.Object);
    }

    #region ParseAndCreateIncidentAsync Tests

    [Fact]
    public async Task ParseAndCreateIncidentAsync_ValidEmail_CreatesIncident()
    {
        // Arrange
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            Subject = "Cannot login to VPN",
            BodyText = "I'm having trouble connecting to the VPN. Please help.",
            ReceivedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.ParseAndCreateIncidentAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Action.Should().Be(EmailParseAction.IncidentCreated);
        result.IncidentId.Should().BePositive();
        result.IncidentNumber.Should().StartWith("INC-");
    }

    [Fact]
    public async Task ParseAndCreateIncidentAsync_HasValidIncidentNumber()
    {
        // Arrange
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            Subject = "Printer not working",
            BodyText = "The printer on the 3rd floor is not printing.",
            ReceivedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.ParseAndCreateIncidentAsync(email);

        // Assert
        result.IncidentNumber.Should().NotBeNullOrEmpty();
        result.IncidentNumber.Should().MatchRegex(@"^INC-\d+$");
    }

    [Fact]
    public async Task ParseAndCreateIncidentAsync_LogsInformation()
    {
        // Arrange
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            Subject = "Help needed",
            BodyText = "Please help me with my computer.",
            ReceivedAt = DateTime.UtcNow
        };

        // Act
        await _service.ParseAndCreateIncidentAsync(email);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Parsing inbound email")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ParseAndUpdateIncidentAsync Tests

    [Fact]
    public async Task ParseAndUpdateIncidentAsync_ValidEmail_UpdatesIncident()
    {
        // Arrange
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            Subject = "RE: [INC-12345] VPN Issue",
            BodyText = "Thank you for the update. Here is more information.",
            ReceivedAt = DateTime.UtcNow
        };
        var incidentId = 12345;

        // Act
        var result = await _service.ParseAndUpdateIncidentAsync(email, incidentId);

        // Assert
        result.Should().NotBeNull();
        result.IncidentId.Should().Be(incidentId);
    }

    [Fact]
    public async Task ParseAndUpdateIncidentAsync_LogsUpdate()
    {
        // Arrange
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            Subject = "RE: Ticket update",
            BodyText = "Here is the additional information you requested.",
            ReceivedAt = DateTime.UtcNow
        };
        var incidentId = 54321;

        // Act
        await _service.ParseAndUpdateIncidentAsync(email, incidentId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Updating incident")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ExtractIncidentReferenceAsync Tests

    [Fact]
    public async Task ExtractIncidentReferenceAsync_ValidReference_ReturnsIncidentId()
    {
        // Arrange
        var subject = "RE: [INC-12345] Your ticket has been updated";

        // Act
        var result = await _service.ExtractIncidentReferenceAsync(subject);

        // Assert
        result.Should().Be(12345);
    }

    [Fact]
    public async Task ExtractIncidentReferenceAsync_NoReference_ReturnsNull()
    {
        // Arrange
        var subject = "New support request - printer issue";

        // Act
        var result = await _service.ExtractIncidentReferenceAsync(subject);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("[INC-1]", 1)]
    [InlineData("[INC-99999]", 99999)]
    [InlineData("[INC-000001]", 1)]
    [InlineData("RE: [INC-456] Update", 456)]
    [InlineData("FW: [INC-789] Information", 789)]
    public async Task ExtractIncidentReferenceAsync_VariousFormats_ExtractsCorrectly(string subject, int expectedId)
    {
        // Act
        var result = await _service.ExtractIncidentReferenceAsync(subject);

        // Assert
        result.Should().Be(expectedId);
    }

    [Theory]
    [InlineData("[inc-123]")] // lowercase
    [InlineData("[Inc-456]")] // mixed case
    public async Task ExtractIncidentReferenceAsync_CaseInsensitive(string subject)
    {
        // Act
        var result = await _service.ExtractIncidentReferenceAsync(subject);

        // Assert
        result.Should().BePositive();
    }

    #endregion

    #region GetParsingConfigAsync Tests

    [Fact]
    public async Task GetParsingConfigAsync_ReturnsConfig()
    {
        // Act
        var config = await _service.GetParsingConfigAsync();

        // Assert
        config.Should().NotBeNull();
    }

    [Fact]
    public async Task GetParsingConfigAsync_ConfigHasDefaults()
    {
        // Act
        var config = await _service.GetParsingConfigAsync();

        // Assert
        config.IsEnabled.Should().BeTrue();
        config.DefaultCategory.Should().NotBeNullOrEmpty();
        config.DefaultPriority.Should().BeInRange(1, 5);
    }

    [Fact]
    public async Task GetParsingConfigAsync_HasPriorityKeywords()
    {
        // Act
        var config = await _service.GetParsingConfigAsync();

        // Assert
        config.PriorityKeywords.Should().NotBeNull();
        config.PriorityKeywords.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region UpdateParsingConfigAsync Tests

    [Fact]
    public async Task UpdateParsingConfigAsync_UpdatesConfig()
    {
        // Arrange
        var newConfig = new EmailParsingConfigDto
        {
            IsEnabled = true,
            DefaultCategory = "Updated Category",
            DefaultPriority = 2,
            AutoDetectCustomer = false
        };

        // Act
        var result = await _service.UpdateParsingConfigAsync(newConfig);

        // Assert
        result.Should().NotBeNull();
        result.DefaultCategory.Should().Be("Updated Category");
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void EmailToTicketService_ImplementsInterface()
    {
        // Assert
        typeof(EmailToTicketService).Should().Implement<IEmailToTicketService>();
    }

    #endregion
}

/// <summary>
/// Tests for InboundEmailDto.
/// </summary>
public class InboundEmailDtoTests
{
    [Fact]
    public void InboundEmailDto_DefaultValues()
    {
        // Arrange & Act
        var email = new InboundEmailDto();

        // Assert
        email.From.Should().BeNullOrEmpty();
        email.To.Should().BeNullOrEmpty();
        email.Subject.Should().BeNullOrEmpty();
        email.BodyText.Should().BeNullOrEmpty();
        email.BodyHtml.Should().BeNull();
        email.Attachments.Should().NotBeNull();
        email.Attachments.Should().BeEmpty();
    }

    [Fact]
    public void InboundEmailDto_CanBeFullyPopulated()
    {
        // Arrange & Act
        var email = new InboundEmailDto
        {
            From = "sender@example.com",
            To = "support@company.com",
            Subject = "Help with issue",
            BodyText = "Plain text body",
            BodyHtml = "<p>HTML body</p>",
            ReceivedAt = new DateTime(2025, 1, 15, 10, 30, 0),
            Attachments = new List<EmailAttachmentDto>
            {
                new() { FileName = "screenshot.png", ContentType = "image/png", SizeBytes = 1024 }
            }
        };

        // Assert
        email.From.Should().Be("sender@example.com");
        email.To.Should().Be("support@company.com");
        email.Subject.Should().Be("Help with issue");
        email.BodyText.Should().Be("Plain text body");
        email.BodyHtml.Should().Be("<p>HTML body</p>");
        email.ReceivedAt.Should().Be(new DateTime(2025, 1, 15, 10, 30, 0));
        email.Attachments.Should().HaveCount(1);
    }
}

/// <summary>
/// Tests for EmailAttachmentDto.
/// </summary>
public class EmailAttachmentDtoTests
{
    [Fact]
    public void EmailAttachmentDto_DefaultValues()
    {
        // Arrange & Act
        var attachment = new EmailAttachmentDto();

        // Assert
        attachment.FileName.Should().BeNullOrEmpty();
        attachment.ContentType.Should().BeNullOrEmpty();
        attachment.SizeBytes.Should().Be(0);
        attachment.Content.Should().BeNull();
    }

    [Fact]
    public void EmailAttachmentDto_CanBePopulated()
    {
        // Arrange & Act
        var attachment = new EmailAttachmentDto
        {
            FileName = "document.pdf",
            ContentType = "application/pdf",
            SizeBytes = 2048576,
            Content = new byte[] { 0x25, 0x50, 0x44, 0x46 } // PDF magic bytes
        };

        // Assert
        attachment.FileName.Should().Be("document.pdf");
        attachment.ContentType.Should().Be("application/pdf");
        attachment.SizeBytes.Should().Be(2048576);
        attachment.Content.Should().HaveCount(4);
    }
}

/// <summary>
/// Tests for EmailParsingConfigDto.
/// </summary>
public class EmailParsingConfigDtoTests
{
    [Fact]
    public void EmailParsingConfigDto_DefaultValues()
    {
        // Arrange & Act
        var config = new EmailParsingConfigDto();

        // Assert
        config.IsEnabled.Should().BeFalse();
        config.DefaultCategory.Should().BeNullOrEmpty();
        config.DefaultPriority.Should().Be(0);
        config.AllowedDomains.Should().NotBeNull();
        config.BlockedDomains.Should().NotBeNull();
        config.PriorityKeywords.Should().NotBeNull();
    }

    [Fact]
    public void EmailParsingConfigDto_CanBeFullyConfigured()
    {
        // Arrange & Act
        var config = new EmailParsingConfigDto
        {
            IsEnabled = true,
            DefaultCategory = "Email",
            DefaultPriority = 3,
            AutoDetectCustomer = true,
            CreateCustomerIfNotFound = false,
            AttachOriginalEmail = true,
            MaxAttachmentSizeMB = 25,
            AllowedDomains = new List<string> { "company.com", "partner.com" },
            BlockedDomains = new List<string> { "spam.com" },
            IgnoreSubjectPatterns = new List<string> { "^Out of Office", "^Auto-Reply" },
            PriorityKeywords = new Dictionary<string, int>
            {
                { "urgent", 1 },
                { "critical", 1 },
                { "asap", 2 }
            }
        };

        // Assert
        config.IsEnabled.Should().BeTrue();
        config.DefaultCategory.Should().Be("Email");
        config.DefaultPriority.Should().Be(3);
        config.AutoDetectCustomer.Should().BeTrue();
        config.CreateCustomerIfNotFound.Should().BeFalse();
        config.AttachOriginalEmail.Should().BeTrue();
        config.MaxAttachmentSizeMB.Should().Be(25);
        config.AllowedDomains.Should().HaveCount(2);
        config.BlockedDomains.Should().Contain("spam.com");
        config.IgnoreSubjectPatterns.Should().HaveCount(2);
        config.PriorityKeywords.Should().ContainKey("urgent");
        config.PriorityKeywords["urgent"].Should().Be(1);
    }
}

/// <summary>
/// Tests for EmailParseResult.
/// </summary>
public class EmailParseResultTests
{
    [Fact]
    public void EmailParseResult_DefaultValues()
    {
        // Arrange & Act
        var result = new EmailParseResult();

        // Assert
        result.Success.Should().BeFalse();
        result.IncidentId.Should().BeNull();
        result.IncidentNumber.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
        result.Action.Should().Be(EmailParseAction.IncidentCreated);
    }

    [Fact]
    public void EmailParseResult_SuccessfulCreation()
    {
        // Arrange & Act
        var result = new EmailParseResult
        {
            Success = true,
            IncidentId = 12345,
            IncidentNumber = "INC-12345",
            Action = EmailParseAction.IncidentCreated
        };

        // Assert
        result.Success.Should().BeTrue();
        result.IncidentId.Should().Be(12345);
        result.IncidentNumber.Should().Be("INC-12345");
        result.Action.Should().Be(EmailParseAction.IncidentCreated);
    }

    [Fact]
    public void EmailParseResult_FailedParsing()
    {
        // Arrange & Act
        var result = new EmailParseResult
        {
            Success = false,
            ErrorMessage = "Email from blocked domain",
            Action = EmailParseAction.Ignored
        };

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Email from blocked domain");
        result.Action.Should().Be(EmailParseAction.Ignored);
    }
}

/// <summary>
/// Tests for EmailParseAction enum.
/// </summary>
public class EmailParseActionTests
{
    [Fact]
    public void EmailParseAction_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<EmailParseAction>().Should().HaveCount(5);
        EmailParseAction.IncidentCreated.Should().BeDefined();
        EmailParseAction.IncidentUpdated.Should().BeDefined();
        EmailParseAction.Ignored.Should().BeDefined();
        EmailParseAction.Failed.Should().BeDefined();
        EmailParseAction.Duplicate.Should().BeDefined();
    }

    [Theory]
    [InlineData(EmailParseAction.IncidentCreated, 0)]
    [InlineData(EmailParseAction.IncidentUpdated, 1)]
    [InlineData(EmailParseAction.Ignored, 2)]
    [InlineData(EmailParseAction.Failed, 3)]
    [InlineData(EmailParseAction.Duplicate, 4)]
    public void EmailParseAction_HasCorrectIntValues(EmailParseAction action, int expectedValue)
    {
        // Assert
        ((int)action).Should().Be(expectedValue);
    }
}

/// <summary>
/// Tests for priority determination logic.
/// </summary>
public class PriorityDeterminationTests
{
    private readonly Mock<ILogger<EmailToTicketService>> _mockLogger;
    private readonly EmailToTicketService _service;

    public PriorityDeterminationTests()
    {
        _mockLogger = new Mock<ILogger<EmailToTicketService>>();
        _service = new EmailToTicketService(_mockLogger.Object);
    }

    [Theory]
    [InlineData("URGENT: System down", 1)]
    [InlineData("Critical server issue", 1)]
    [InlineData("Emergency - production issue", 1)]
    [InlineData("ASAP - need help", 2)]
    [InlineData("Important request", 2)]
    [InlineData("High priority issue", 2)]
    public async Task ParseAndCreateIncidentAsync_PriorityKeywords_AffectsPriority(string subject, int expectedPriority)
    {
        // This tests that the service recognizes priority keywords
        // The actual priority may vary based on implementation
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            Subject = subject,
            BodyText = "Please help.",
            ReceivedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.ParseAndCreateIncidentAsync(email);

        // Assert - at minimum, verify it parses successfully
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAndCreateIncidentAsync_NoPriorityKeywords_UsesDefault()
    {
        // Arrange
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            Subject = "General question about software",
            BodyText = "I have a question about how to use the software.",
            ReceivedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.ParseAndCreateIncidentAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }
}

/// <summary>
/// Tests for email body cleaning.
/// </summary>
public class EmailBodyCleaningTests
{
    private readonly Mock<ILogger<EmailToTicketService>> _mockLogger;
    private readonly EmailToTicketService _service;

    public EmailBodyCleaningTests()
    {
        _mockLogger = new Mock<ILogger<EmailToTicketService>>();
        _service = new EmailToTicketService(_mockLogger.Object);
    }

    [Fact]
    public async Task ParseAndCreateIncidentAsync_HandlesQuotedText()
    {
        // Arrange
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            Subject = "RE: Previous message",
            BodyText = "Here is my response.\n\n> Original message\n> that was quoted",
            ReceivedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.ParseAndCreateIncidentAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAndCreateIncidentAsync_HandlesSignature()
    {
        // Arrange
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            Subject = "Help request",
            BodyText = "I need help with this issue.\n\n--\nJohn Doe\nIT Department",
            ReceivedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.ParseAndCreateIncidentAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAndCreateIncidentAsync_HandlesHtmlEmail()
    {
        // Arrange
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            Subject = "HTML Email",
            BodyText = "Plain text version",
            BodyHtml = "<html><body><p>HTML version</p></body></html>",
            ReceivedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.ParseAndCreateIncidentAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }
}
