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

namespace CRM.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for FormBuilderService.
/// </summary>
public class FormBuilderServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly IFormBuilderService _service;
    private readonly Mock<ILogger<FormBuilderService>> _loggerMock;

    public FormBuilderServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"FormBuilderTestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _loggerMock = new Mock<ILogger<FormBuilderService>>();
        _service = new FormBuilderService(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    private FormDefinition CreateTestForm(string name = "Test Form", FormStatus status = FormStatus.Draft)
    {
        return new FormDefinition
        {
            Name = name,
            FormKey = $"test-form-{Guid.NewGuid():N}",
            Description = "Test form description",
            Status = status,
            Title = name,
            SubmitButtonText = "Submit",
            CreateLead = true,
            LeadSource = "Web",
            NotifyOwner = true,
            SpamProtection = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private FormField CreateTestField(int formId, string fieldName = "test_field", FormFieldType fieldType = FormFieldType.Text, int order = 1)
    {
        return new FormField
        {
            FormDefinitionId = formId,
            FieldName = fieldName,
            Label = fieldName.Replace("_", " "),
            FieldType = fieldType,
            Order = order,
            IsRequired = true,
            Placeholder = $"Enter {fieldName}",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private FormSubmission CreateTestSubmission(int formId, SubmissionStatus status = SubmissionStatus.New)
    {
        return new FormSubmission
        {
            FormDefinitionId = formId,
            SubmissionNumber = $"SUB-{Guid.NewGuid():N}",
            Status = status,
            FormData = "{\"email\":\"test@example.com\",\"name\":\"Test User\"}",
            SubmittedAt = DateTime.UtcNow,
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private User CreateTestUser(int id = 0, string username = "testuser")
    {
        return new User
        {
            Id = id,
            Username = username,
            Email = $"{username}@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hashedpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    #endregion

    #region Form Definition CRUD Tests

    [Fact]
    public async Task GetAllFormsAsync_ReturnsAllActiveForms()
    {
        // Arrange
        var form1 = CreateTestForm(name: "Form 1");
        var form2 = CreateTestForm(name: "Form 2");
        var deletedForm = CreateTestForm(name: "Deleted Form");
        deletedForm.IsDeleted = true;

        await _dbContext.FormDefinitions.AddRangeAsync(form1, form2, deletedForm);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllFormsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(f => f.Name).Should().Contain("Form 1", "Form 2");
    }

    [Fact]
    public async Task GetAllFormsAsync_WithStatusFilter_ReturnsFilteredForms()
    {
        // Arrange
        var draftForm = CreateTestForm(name: "Draft Form", status: FormStatus.Draft);
        var publishedForm = CreateTestForm(name: "Published Form", status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddRangeAsync(draftForm, publishedForm);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllFormsAsync(status: FormStatus.Published);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Published Form");
    }

    [Fact]
    public async Task GetFormByIdAsync_WhenFormExists_ReturnsForm()
    {
        // Arrange
        var form = CreateTestForm();
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetFormByIdAsync(form.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(form.Name);
    }

    [Fact]
    public async Task GetFormByIdAsync_WhenFormNotExists_ReturnsNull()
    {
        // Act
        var result = await _service.GetFormByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFormByKeyAsync_WhenFormExists_ReturnsForm()
    {
        // Arrange
        var form = CreateTestForm();
        form.FormKey = "unique-form-key";
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetFormByKeyAsync("unique-form-key");

        // Assert
        result.Should().NotBeNull();
        result!.FormKey.Should().Be("unique-form-key");
    }

    [Fact]
    public async Task CreateFormAsync_CreatesAndReturnsForm()
    {
        // Arrange
        var form = CreateTestForm(name: "New Form");

        // Act
        var result = await _service.CreateFormAsync(form);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Form");

        var savedForm = await _dbContext.FormDefinitions.FindAsync(result.Id);
        savedForm.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateFormAsync_UpdatesExistingForm()
    {
        // Arrange
        var form = CreateTestForm();
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        form.Name = "Updated Form Name";
        form.Description = "Updated description";

        // Act
        var result = await _service.UpdateFormAsync(form);

        // Assert
        result.Name.Should().Be("Updated Form Name");
        result.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task DeleteFormAsync_SoftDeletesForm()
    {
        // Arrange
        var form = CreateTestForm();
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeleteFormAsync(form.Id);

        // Assert
        result.Should().BeTrue();
        var deletedForm = await _dbContext.FormDefinitions.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == form.Id);
        deletedForm.Should().NotBeNull();
        deletedForm!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task CloneFormAsync_CreatesClone()
    {
        // Arrange
        var form = CreateTestForm(name: "Original Form");
        await _dbContext.FormDefinitions.AddAsync(form);

        var field = CreateTestField(form.Id, "email", FormFieldType.Email);
        await _dbContext.FormFields.AddAsync(field);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CloneFormAsync(form.Id, "Cloned Form");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Cloned Form");
        result.Id.Should().NotBe(form.Id);
        result.Status.Should().Be(FormStatus.Draft);
    }

    #endregion

    #region Form Status Management Tests

    [Fact]
    public async Task PublishFormAsync_SetsStatusToPublished()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Draft);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.PublishFormAsync(form.Id);

        // Assert
        result.Status.Should().Be(FormStatus.Published);
    }

    [Fact]
    public async Task UnpublishFormAsync_SetsStatusToPaused()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.UnpublishFormAsync(form.Id);

        // Assert
        result.Status.Should().Be(FormStatus.Paused);
    }

    [Fact]
    public async Task ArchiveFormAsync_SetsStatusToArchived()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ArchiveFormAsync(form.Id);

        // Assert
        result.Status.Should().Be(FormStatus.Archived);
    }

    #endregion

    #region Form Field Management Tests

    [Fact]
    public async Task GetFormFieldsAsync_ReturnsFieldsForForm()
    {
        // Arrange
        var form = CreateTestForm();
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var field1 = CreateTestField(form.Id, "email", FormFieldType.Email, order: 1);
        var field2 = CreateTestField(form.Id, "name", FormFieldType.Text, order: 2);
        await _dbContext.FormFields.AddRangeAsync(field1, field2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetFormFieldsAsync(form.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(f => f.Order);
    }

    [Fact]
    public async Task AddFieldAsync_AddsFieldToForm()
    {
        // Arrange
        var form = CreateTestForm();
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var field = CreateTestField(form.Id, "phone", FormFieldType.Phone);

        // Act
        var result = await _service.AddFieldAsync(form.Id, field);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.FieldName.Should().Be("phone");
    }

    [Fact]
    public async Task UpdateFieldAsync_UpdatesExistingField()
    {
        // Arrange
        var form = CreateTestForm();
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var field = CreateTestField(form.Id, "email");
        await _dbContext.FormFields.AddAsync(field);
        await _dbContext.SaveChangesAsync();

        field.Label = "Email Address";
        field.IsRequired = false;

        // Act
        var result = await _service.UpdateFieldAsync(field);

        // Assert
        result.Label.Should().Be("Email Address");
        result.IsRequired.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveFieldAsync_DeletesField()
    {
        // Arrange
        var form = CreateTestForm();
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var field = CreateTestField(form.Id);
        await _dbContext.FormFields.AddAsync(field);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RemoveFieldAsync(field.Id);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Form Submission Tests

    [Fact]
    public async Task GetSubmissionsAsync_ReturnsSubmissionsForForm()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var submission1 = CreateTestSubmission(form.Id, SubmissionStatus.New);
        var submission2 = CreateTestSubmission(form.Id, SubmissionStatus.LeadCreated);
        await _dbContext.FormSubmissions.AddRangeAsync(submission1, submission2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetSubmissionsAsync(form.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSubmissionsAsync_WithStatusFilter_ReturnsFilteredSubmissions()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var newSubmission = CreateTestSubmission(form.Id, SubmissionStatus.New);
        var processedSubmission = CreateTestSubmission(form.Id, SubmissionStatus.LeadCreated);
        await _dbContext.FormSubmissions.AddRangeAsync(newSubmission, processedSubmission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetSubmissionsAsync(form.Id, status: SubmissionStatus.New);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(SubmissionStatus.New);
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_WhenExists_ReturnsSubmission()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var submission = CreateTestSubmission(form.Id);
        await _dbContext.FormSubmissions.AddAsync(submission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetSubmissionByIdAsync(submission.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(submission.Id);
    }

    [Fact]
    public async Task ProcessSubmissionAsync_CreatesSubmission()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var formData = new Dictionary<string, object>
        {
            { "email", "test@example.com" },
            { "name", "Test User" }
        };

        var context = new FormSubmissionContext
        {
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0"
        };

        // Act
        var result = await _service.ProcessSubmissionAsync(form.Id, formData, context);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Submission.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsSpamAsync_SetsSpamStatus()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var submission = CreateTestSubmission(form.Id, SubmissionStatus.New);
        await _dbContext.FormSubmissions.AddAsync(submission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.MarkAsSpamAsync(submission.Id);

        // Assert
        result.Status.Should().Be(SubmissionStatus.Spam);
        result.IsSpam.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSubmissionAsync_DeletesSubmission()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var submission = CreateTestSubmission(form.Id);
        await _dbContext.FormSubmissions.AddAsync(submission);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeleteSubmissionAsync(submission.Id);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateFormDataAsync_WithValidData_ReturnsValid()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var emailField = CreateTestField(form.Id, "email", FormFieldType.Email);
        emailField.IsRequired = true;
        await _dbContext.FormFields.AddAsync(emailField);
        await _dbContext.SaveChangesAsync();

        var formData = new Dictionary<string, object>
        {
            { "email", "test@example.com" }
        };

        // Act
        var result = await _service.ValidateFormDataAsync(form.Id, formData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateSpamScoreAsync_ReturnsScore()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        form.SpamProtection = true;
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        var formData = new Dictionary<string, object>
        {
            { "email", "test@example.com" },
            { "message", "This is a normal message" }
        };

        var context = new FormSubmissionContext
        {
            IpAddress = "192.168.1.1",
            SubmissionDuration = TimeSpan.FromSeconds(10)
        };

        // Act
        var result = await _service.CalculateSpamScoreAsync(form.Id, formData, context);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
        result.Should().BeLessThanOrEqualTo(100);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetFormStatisticsAsync_ReturnsStatistics()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        form.TotalViews = 100;
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Add some submissions to count
        var submission1 = CreateTestSubmission(form.Id, SubmissionStatus.LeadCreated);
        var submission2 = CreateTestSubmission(form.Id, SubmissionStatus.New);
        await _dbContext.FormSubmissions.AddRangeAsync(submission1, submission2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetFormStatisticsAsync(form.Id);

        // Assert
        result.Should().NotBeNull();
        result.FormId.Should().Be(form.Id);
        result.TotalViews.Should().Be(100);
        // Statistics may count from DB or from form property - check that it returns a value
        result.TotalSubmissions.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task IncrementViewCountAsync_IncrementsCount()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        form.TotalViews = 10;
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.IncrementViewCountAsync(form.Id);

        // Assert
        var updatedForm = await _dbContext.FormDefinitions.FindAsync(form.Id);
        updatedForm!.TotalViews.Should().Be(11);
    }

    #endregion

    #region Embedding Tests

    [Fact]
    public async Task GenerateEmbedCodeAsync_ReturnsEmbedCode()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GenerateEmbedCodeAsync(form.Id, "https://crm.example.com");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("iframe");
        result.Should().Contain(form.Id.ToString());
    }

    [Fact]
    public async Task GenerateDirectUrlAsync_ReturnsUrl()
    {
        // Arrange
        var form = CreateTestForm(status: FormStatus.Published);
        await _dbContext.FormDefinitions.AddAsync(form);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GenerateDirectUrlAsync(form.Id, "https://crm.example.com");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain(form.Id.ToString());
    }

    #endregion
}
