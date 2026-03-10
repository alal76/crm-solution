// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ContactModel = CRM.Core.Models.Contact;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for EmailTemplateService.
/// </summary>
public class EmailTemplateServiceTests : ServiceTestFixtureBase<EmailTemplateService>
{    private readonly EmailTemplateService _service;

    private readonly List<EmailTemplate> _templates;
    private readonly List<EmailTemplateVersion> _versions;
    private readonly List<Account> _accounts;
    private readonly List<ContactModel> _contacts;
    private readonly List<Opportunity> _opportunities;

    public EmailTemplateServiceTests()
    {        _templates = new List<EmailTemplate>();
        _versions = new List<EmailTemplateVersion>();
        _accounts = new List<Account>();
        _contacts = new List<ContactModel>();
        _opportunities = new List<Opportunity>();

        var mockTemplates = MockDbSetFactory.CreateMockDbSet(_templates);
        var mockVersions = MockDbSetFactory.CreateMockDbSet(_versions);
        var mockAccounts = MockDbSetFactory.CreateMockDbSet(_accounts);
        var mockContacts = MockDbSetFactory.CreateMockDbSet(_contacts);
        var mockOpportunities = MockDbSetFactory.CreateMockDbSet(_opportunities);

        // Add FindAsync(object[], CancellationToken) overload - MockDbSetFactory only sets up FindAsync(object[])
        mockTemplates.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                if (id == null)
                    return ValueTask.FromResult<EmailTemplate?>(default);
                return ValueTask.FromResult(_templates.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });

        MockContext.Setup(c => c.EmailTemplates).Returns(mockTemplates.Object);
        MockContext.Setup(c => c.EmailTemplateVersions).Returns(mockVersions.Object);
        MockContext.Setup(c => c.Accounts).Returns(mockAccounts.Object);
        MockContext.Setup(c => c.Contacts).Returns(mockContacts.Object);
        MockContext.Setup(c => c.Opportunities).Returns(mockOpportunities.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new EmailTemplateService(MockContext.Object, MockLogger.Object);
    }

    // ========================================================================
    // GetAllAsync
    // ========================================================================
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedTemplates()
    {
        // Arrange
        _templates.AddRange(new[]
        {
            new EmailTemplate { Id = 1, Name = "Welcome", IsActive = true, IsDeleted = false },
            new EmailTemplate { Id = 2, Name = "Followup", IsActive = true, IsDeleted = false },
            new EmailTemplate { Id = 3, Name = "Deleted", IsActive = true, IsDeleted = true }
        });

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCategory()
    {
        // Arrange
        _templates.AddRange(new[]
        {
            new EmailTemplate { Id = 1, Name = "T1", Category = EmailTemplateCategory.Sales, IsDeleted = false },
            new EmailTemplate { Id = 2, Name = "T2", Category = EmailTemplateCategory.Marketing, IsDeleted = false },
            new EmailTemplate { Id = 3, Name = "T3", Category = EmailTemplateCategory.Sales, IsDeleted = false }
        });

        // Act
        var result = await _service.GetAllAsync(category: EmailTemplateCategory.Sales);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.Category == EmailTemplateCategory.Sales);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByIsActive()
    {
        // Arrange
        _templates.AddRange(new[]
        {
            new EmailTemplate { Id = 1, Name = "Active1", IsActive = true, IsDeleted = false },
            new EmailTemplate { Id = 2, Name = "Inactive", IsActive = false, IsDeleted = false },
            new EmailTemplate { Id = 3, Name = "Active2", IsActive = true, IsDeleted = false }
        });

        // Act
        var result = await _service.GetAllAsync(isActive: true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.IsActive);
    }

    // ========================================================================
    // GetByIdAsync / GetByNameAsync / GetBySlugAsync
    // ========================================================================
    [Fact]
    public async Task GetByIdAsync_ShouldReturnTemplate_WhenExists()
    {
        // Arrange
        _templates.Add(new EmailTemplate { Id = 1, Name = "Welcome Email", IsDeleted = false });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Welcome Email");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnTemplate()
    {
        // Arrange
        _templates.Add(new EmailTemplate { Id = 1, Name = "Onboarding", IsDeleted = false });

        // Act
        var result = await _service.GetByNameAsync("Onboarding");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Onboarding");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnTemplate()
    {
        // Arrange
        _templates.Add(new EmailTemplate { Id = 1, Name = "Test", Slug = "welcome-email", IsDeleted = false });

        // Act
        var result = await _service.GetBySlugAsync("welcome-email");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("welcome-email");
    }

    // ========================================================================
    // CreateAsync
    // ========================================================================
    [Fact]
    public async Task CreateAsync_ShouldGenerateSlugFromName()
    {
        // Arrange
        var template = new EmailTemplate { Name = "My Test Template", Subject = "Hello" };

        // Act
        var result = await _service.CreateAsync(template);

        // Assert
        result.Should().NotBeNull();
        result.Slug.Should().Be("my-test-template");
        result.Version.Should().Be(1);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateInitialVersion()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Name = "New Template",
            Subject = "Subject Line",
            HtmlBody = "<p>Hello</p>",
            PlainTextBody = "Hello"
        };

        // Act
        await _service.CreateAsync(template);

        // Assert
        _versions.Should().ContainSingle();
        var version = _versions.First();
        version.Version.Should().Be(1);
        version.Subject.Should().Be("Subject Line");
        version.HtmlBody.Should().Be("<p>Hello</p>");
        version.ChangeDescription.Should().Be("Initial version");
    }

    // ========================================================================
    // UpdateAsync / DeleteAsync
    // ========================================================================
    [Fact]
    public async Task UpdateAsync_ShouldSetUpdatedAt()
    {
        // Arrange
        _templates.Add(new EmailTemplate { Id = 1, Name = "Old Name", IsDeleted = false });

        var updated = _templates.First();
        updated.Name = "New Name";

        // Act
        var result = await _service.UpdateAsync(updated);

        // Assert
        result.Should().NotBeNull();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_ForSystemTemplate()
    {
        // Arrange
        _templates.Add(new EmailTemplate { Id = 1, Name = "System", IsSystem = true, IsDeleted = false });

        // Act
        var act = async () => await _service.DeleteAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*system*");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_ForNonSystemTemplate()
    {
        // Arrange
        _templates.Add(new EmailTemplate { Id = 1, Name = "Custom", IsSystem = false, IsDeleted = false });

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _templates.First(t => t.Id == 1).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // ValidateAsync / ExtractVariablesAsync
    // ========================================================================
    [Fact]
    public async Task ValidateAsync_ShouldExtractVariables()
    {
        // Arrange
        var content = "Hello {{firstName}}, your order {{orderId}} is ready.";

        // Act
        var result = await _service.ValidateAsync(content);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.UsedVariables.Should().Contain("firstName");
        result.UsedVariables.Should().Contain("orderId");
    }

    [Fact]
    public async Task ExtractVariablesAsync_ShouldFindAllVariables()
    {
        // Arrange
        var content = "Dear {{name}}, your {{product}} subscription expires on {{date}}.";

        // Act
        var result = await _service.ExtractVariablesAsync(content);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain("name");
        result.Should().Contain("product");
        result.Should().Contain("date");
    }

    // ========================================================================
    // CloneAsync
    // ========================================================================
    [Fact]
    public async Task CloneAsync_ShouldCreateCopyWithNewName()
    {
        // Arrange
        _templates.Add(new EmailTemplate
        {
            Id = 1,
            Name = "Original",
            Slug = "original",
            Subject = "Original Subject",
            HtmlBody = "<p>Original</p>",
            PlainTextBody = "Original",
            Category = EmailTemplateCategory.Sales,
            IsActive = true,
            IsSystem = false,
            IsDeleted = false
        });

        // Act
        var result = await _service.CloneAsync(1, "Cloned Template");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Cloned Template");
        result.Slug.Should().Be("cloned-template");
        result.Subject.Should().Be("Original Subject");
        result.IsSystem.Should().BeFalse();
    }

    // ========================================================================
    // RecordUsageAsync / SetAsDefaultAsync
    // ========================================================================
    [Fact]
    public async Task RecordUsageAsync_ShouldIncrementUsageCount()
    {
        // Arrange
        _templates.Add(new EmailTemplate { Id = 1, Name = "Template", UsageCount = 5, IsDeleted = false });

        // Act
        await _service.RecordUsageAsync(1);

        // Assert
        _templates.First(t => t.Id == 1).UsageCount.Should().Be(6);
        _templates.First(t => t.Id == 1).LastUsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SetAsDefaultAsync_ShouldSetSlugFromPurpose()
    {
        // Arrange
        _templates.Add(new EmailTemplate { Id = 1, Name = "Template", Slug = "old-slug", IsDeleted = false });

        // Act
        var result = await _service.SetAsDefaultAsync(1, EmailTemplatePurpose.WelcomeEmail);

        // Assert
        result.Should().BeTrue();
        _templates.First(t => t.Id == 1).Slug.Should().Be("welcomeemail");
    }
}

// TCOV-036-EXPANDED
// Additional tests appended to expand coverage

/// <summary>Additional coverage tests for EmailTemplateService (TCOV-036 expansion).</summary>
public class EmailTemplateServiceExpandedTests : ServiceTestFixtureBase<EmailTemplateService>
{
    private readonly EmailTemplateService _service;
    private readonly List<EmailTemplate> _templates;
    private readonly List<EmailTemplateVersion> _versions;

    public EmailTemplateServiceExpandedTests()
    {
        _templates = new List<EmailTemplate>();
        _versions = new List<EmailTemplateVersion>();

        var mockTemplates = MockDbSetFactory.CreateMockDbSet(_templates);
        mockTemplates.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                return ValueTask.FromResult(_templates.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });
        var mockVersions = MockDbSetFactory.CreateMockDbSet(_versions);

        MockContext.Setup(c => c.EmailTemplates).Returns(mockTemplates.Object);
        MockContext.Setup(c => c.EmailTemplateVersions).Returns(mockVersions.Object);
        MockContext.Setup(c => c.Accounts).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.Account>()).Object);
        MockContext.Setup(c => c.Contacts).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Models.Contact>()).Object);
        MockContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.Opportunity>()).Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new EmailTemplateService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByNameAsync("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetBySlugAsync("no-slug");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveTemplates_WhenIsActiveFilterApplied()
    {
        _templates.AddRange(new[]
        {
            new EmailTemplate { Id = 1, Name = "Active", IsActive = true, IsDeleted = false },
            new EmailTemplate { Id = 2, Name = "Inactive", IsActive = false, IsDeleted = false }
        });

        var result = (await _service.GetAllAsync(isActive: true)).ToList();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenTemplateNotFound()
    {
        var template = new EmailTemplate { Id = 999, Name = "Ghost", IsDeleted = false };
        Func<Task> act = () => _service.UpdateAsync(template);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTemplateIsNotFoundAtAll()
    {
        var result = await _service.DeleteAsync(9999);
        result.Should().BeFalse();
    }
}
