// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for EmailTemplatesController
/// Covers: Template CRUD, categories, preview, usage tracking
/// </summary>
public class EmailTemplatesControllerTests
{
    private readonly Mock<IEmailTemplateService> _mockTemplateService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<EmailTemplatesController>> _mockLogger;
    private readonly EmailTemplatesController _controller;

    public EmailTemplatesControllerTests()
    {
        _mockTemplateService = new Mock<IEmailTemplateService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<EmailTemplatesController>>();

        _controller = new EmailTemplatesController(
            _mockTemplateService.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);

        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkWithTemplates()
    {
        // Arrange
        var templates = new List<EmailTemplateDto>
        {
            new EmailTemplateDto { Id = 1, Name = "Welcome Email", Subject = "Welcome!" },
            new EmailTemplateDto { Id = 2, Name = "Follow-up Email", Subject = "Following up" }
        };

        _mockTemplateService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(templates);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTemplates = okResult.Value.Should().BeAssignableTo<IEnumerable<EmailTemplateDto>>().Subject;
        returnedTemplates.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByCategory_ReturnsFilteredTemplates()
    {
        // Arrange
        var templates = new List<EmailTemplateDto>
        {
            new EmailTemplateDto { Id = 1, Name = "Welcome", Category = "Onboarding" }
        };

        _mockTemplateService.Setup(s => s.GetByCategoryAsync("Onboarding"))
            .ReturnsAsync(templates);

        // Act
        var result = await _controller.GetByCategory("Onboarding");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTemplates = okResult.Value.Should().BeAssignableTo<IEnumerable<EmailTemplateDto>>().Subject;
        returnedTemplates.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActive_ReturnsActiveTemplates()
    {
        // Arrange
        var templates = new List<EmailTemplateDto>
        {
            new EmailTemplateDto { Id = 1, IsActive = true }
        };

        _mockTemplateService.Setup(s => s.GetActiveAsync())
            .ReturnsAsync(templates);

        // Act
        var result = await _controller.GetActive();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<EmailTemplateDto>>();
    }

    [Fact]
    public async Task GetCategories_ReturnsDistinctCategories()
    {
        // Arrange
        var categories = new List<string> { "Onboarding", "Sales", "Support", "Marketing" };

        _mockTemplateService.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCategories = okResult.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;
        returnedCategories.Should().HaveCount(4);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingTemplate_ReturnsOk()
    {
        // Arrange
        var template = new EmailTemplateDto
        {
            Id = 1,
            Name = "Welcome Email",
            Subject = "Welcome to our platform!",
            HtmlBody = "<h1>Welcome!</h1>",
            PlainTextBody = "Welcome!"
        };

        _mockTemplateService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(template);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTemplate = okResult.Value.Should().BeOfType<EmailTemplateDto>().Subject;
        returnedTemplate.Id.Should().Be(1);
        returnedTemplate.Name.Should().Be("Welcome Email");
    }

    [Fact]
    public async Task GetById_NonExistingTemplate_ReturnsNotFound()
    {
        // Arrange
        _mockTemplateService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((EmailTemplateDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByName_ExistingTemplate_ReturnsOk()
    {
        // Arrange
        var template = new EmailTemplateDto { Id = 1, Name = "Welcome Email" };

        _mockTemplateService.Setup(s => s.GetByNameAsync("Welcome Email"))
            .ReturnsAsync(template);

        // Act
        var result = await _controller.GetByName("Welcome Email");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<EmailTemplateDto>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidTemplate_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateEmailTemplateDto
        {
            Name = "New Template",
            Subject = "Subject Line",
            HtmlBody = "<p>Body content</p>",
            Category = "Marketing"
        };

        var createdTemplate = new EmailTemplateDto
        {
            Id = 1,
            Name = "New Template",
            Subject = "Subject Line",
            HtmlBody = "<p>Body content</p>",
            Category = "Marketing"
        };

        _mockTemplateService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdTemplate);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateEmailTemplateDto { Name = "Existing Template" };

        _mockTemplateService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Template with this name already exists"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_WithVariables_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateEmailTemplateDto
        {
            Name = "Personalized Welcome",
            Subject = "Welcome {{FirstName}}!",
            HtmlBody = "<p>Hello {{FirstName}} {{LastName}}</p>",
            Variables = new List<string> { "FirstName", "LastName" }
        };

        var createdTemplate = new EmailTemplateDto
        {
            Id = 1,
            Name = "Personalized Welcome",
            Variables = new List<string> { "FirstName", "LastName" }
        };

        _mockTemplateService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdTemplate);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidTemplate_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateEmailTemplateDto
        {
            Id = 1,
            Name = "Updated Template",
            Subject = "Updated Subject"
        };

        var updatedTemplate = new EmailTemplateDto
        {
            Id = 1,
            Name = "Updated Template",
            Subject = "Updated Subject"
        };

        _mockTemplateService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync(updatedTemplate);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTemplate = okResult.Value.Should().BeOfType<EmailTemplateDto>().Subject;
        returnedTemplate.Name.Should().Be("Updated Template");
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateEmailTemplateDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingTemplate_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateEmailTemplateDto { Id = 999 };

        _mockTemplateService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync((EmailTemplateDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingTemplate_ReturnsNoContent()
    {
        // Arrange
        _mockTemplateService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityDeletedAsync(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingTemplate_ReturnsNotFound()
    {
        // Arrange
        _mockTemplateService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_TemplateInUse_ReturnsConflict()
    {
        // Arrange
        _mockTemplateService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Template is in use by campaigns"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    #endregion

    #region Preview Tests

    [Fact]
    public async Task Preview_ValidTemplate_ReturnsRenderedHtml()
    {
        // Arrange
        var previewRequest = new PreviewTemplateDto
        {
            TemplateId = 1,
            Variables = new Dictionary<string, string>
            {
                { "FirstName", "John" },
                { "LastName", "Doe" }
            }
        };

        var renderedHtml = "<p>Hello John Doe</p>";

        _mockTemplateService.Setup(s => s.PreviewAsync(1, previewRequest.Variables))
            .ReturnsAsync(renderedHtml);

        // Act
        var result = await _controller.Preview(previewRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { Html = renderedHtml });
    }

    [Fact]
    public async Task Preview_NonExistingTemplate_ReturnsNotFound()
    {
        // Arrange
        var previewRequest = new PreviewTemplateDto { TemplateId = 999 };

        _mockTemplateService.Setup(s => s.PreviewAsync(999, It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _controller.Preview(previewRequest);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task SendTestEmail_ValidRequest_ReturnsOk()
    {
        // Arrange
        var testRequest = new SendTestEmailDto
        {
            TemplateId = 1,
            RecipientEmail = "test@example.com",
            Variables = new Dictionary<string, string> { { "FirstName", "Test" } }
        };

        _mockTemplateService.Setup(s => s.SendTestEmailAsync(testRequest))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendTestEmail(testRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SendTestEmail_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var testRequest = new SendTestEmailDto
        {
            TemplateId = 1,
            RecipientEmail = "invalid-email"
        };

        // Act
        var result = await _controller.SendTestEmail(testRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public async Task Clone_ExistingTemplate_ReturnsCreatedAtAction()
    {
        // Arrange
        var cloneRequest = new CloneTemplateDto
        {
            SourceTemplateId = 1,
            NewName = "Cloned Template"
        };

        var clonedTemplate = new EmailTemplateDto
        {
            Id = 2,
            Name = "Cloned Template"
        };

        _mockTemplateService.Setup(s => s.CloneAsync(1, "Cloned Template"))
            .ReturnsAsync(clonedTemplate);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Clone(cloneRequest);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Clone_NonExistingSource_ReturnsNotFound()
    {
        // Arrange
        var cloneRequest = new CloneTemplateDto
        {
            SourceTemplateId = 999,
            NewName = "Cloned Template"
        };

        _mockTemplateService.Setup(s => s.CloneAsync(999, "Cloned Template"))
            .ReturnsAsync((EmailTemplateDto?)null);

        // Act
        var result = await _controller.Clone(cloneRequest);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public async Task Activate_ExistingTemplate_ReturnsOk()
    {
        // Arrange
        _mockTemplateService.Setup(s => s.ActivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Activate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Deactivate_ExistingTemplate_ReturnsOk()
    {
        // Arrange
        _mockTemplateService.Setup(s => s.DeactivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Deactivate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Activate_NonExistingTemplate_ReturnsNotFound()
    {
        // Arrange
        _mockTemplateService.Setup(s => s.ActivateAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Activate(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Variables Tests

    [Fact]
    public async Task GetVariables_ReturnsTemplateVariables()
    {
        // Arrange
        var variables = new List<string> { "FirstName", "LastName", "Email", "Company" };

        _mockTemplateService.Setup(s => s.GetVariablesAsync(1))
            .ReturnsAsync(variables);

        // Act
        var result = await _controller.GetVariables(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedVariables = okResult.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;
        returnedVariables.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetAvailableVariables_ReturnsAllVariables()
    {
        // Arrange
        var variables = new Dictionary<string, string>
        {
            { "FirstName", "Contact's first name" },
            { "LastName", "Contact's last name" },
            { "CompanyName", "Account company name" }
        };

        _mockTemplateService.Setup(s => s.GetAvailableVariablesAsync())
            .ReturnsAsync(variables);

        // Act
        var result = await _controller.GetAvailableVariables();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<Dictionary<string, string>>();
    }

    #endregion

    #region Usage Tracking Tests

    [Fact]
    public async Task GetUsageStatistics_ReturnsStats()
    {
        // Arrange
        var stats = new TemplateUsageStatsDto
        {
            TemplateId = 1,
            TotalSent = 1000,
            Opened = 450,
            Clicked = 120,
            OpenRate = 45.0m,
            ClickRate = 12.0m
        };

        _mockTemplateService.Setup(s => s.GetUsageStatisticsAsync(1))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetUsageStatistics(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<TemplateUsageStatsDto>().Subject;
        returnedStats.TotalSent.Should().Be(1000);
    }

    [Fact]
    public async Task GetCampaignsUsingTemplate_ReturnsCampaignList()
    {
        // Arrange
        var campaigns = new List<CampaignSummaryDto>
        {
            new CampaignSummaryDto { Id = 1, Name = "Campaign 1" },
            new CampaignSummaryDto { Id = 2, Name = "Campaign 2" }
        };

        _mockTemplateService.Setup(s => s.GetCampaignsUsingTemplateAsync(1))
            .ReturnsAsync(campaigns);

        // Act
        var result = await _controller.GetCampaignsUsingTemplate(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCampaigns = okResult.Value.Should().BeAssignableTo<IEnumerable<CampaignSummaryDto>>().Subject;
        returnedCampaigns.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingTemplates()
    {
        // Arrange
        var templates = new List<EmailTemplateDto>
        {
            new EmailTemplateDto { Id = 1, Name = "Welcome Email" }
        };

        _mockTemplateService.Setup(s => s.SearchAsync("Welcome"))
            .ReturnsAsync(templates);

        // Act
        var result = await _controller.Search("Welcome");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var searchResults = okResult.Value.Should().BeAssignableTo<IEnumerable<EmailTemplateDto>>().Subject;
        searchResults.Should().HaveCount(1);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkDelete_ValidIds_ReturnsOkWithCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockTemplateService.Setup(s => s.BulkDeleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDelete(ids);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { DeletedCount = 3 });
    }

    [Fact]
    public async Task BulkActivate_ValidIds_ReturnsOkWithCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockTemplateService.Setup(s => s.BulkActivateAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkActivate(ids);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { ActivatedCount = 3 });
    }

    #endregion
}
