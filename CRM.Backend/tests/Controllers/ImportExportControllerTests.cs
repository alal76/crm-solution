// CRM Solution - Customer Relationship Management System
// Import Export Controller Unit Tests

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
using System.IO;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for ImportExportController
/// Covers: Data import, export, mapping, validation
/// </summary>
public class ImportExportControllerTests
{
    private readonly Mock<IImportExportService> _mockImportExportService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<ImportExportController>> _mockLogger;
    private readonly ImportExportController _controller;

    public ImportExportControllerTests()
    {
        _mockImportExportService = new Mock<IImportExportService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<ImportExportController>>();

        _controller = new ImportExportController(
            _mockImportExportService.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);

        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    private Mock<IFormFile> CreateMockFile(string fileName, string content)
    {
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        return fileMock;
    }

    #region Export Tests

    [Fact]
    public async Task ExportAccounts_ReturnsFile()
    {
        // Arrange
        var exportData = System.Text.Encoding.UTF8.GetBytes("Id,Name,Email\n1,Acme Corp,info@acme.com");

        _mockImportExportService.Setup(s => s.ExportAccountsAsync("csv", null))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.ExportAccounts("csv");

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public async Task ExportAccounts_WithFilters_ReturnsFilteredFile()
    {
        // Arrange
        var filters = new ExportFilterDto
        {
            DateFrom = DateTime.UtcNow.AddMonths(-1),
            DateTo = DateTime.UtcNow,
            Fields = new List<string> { "Id", "Name", "Email" }
        };

        var exportData = new byte[] { 0x01, 0x02 };

        _mockImportExportService.Setup(s => s.ExportAccountsAsync("xlsx", filters))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.ExportAccounts("xlsx", filters);

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task ExportContacts_ReturnsFile()
    {
        // Arrange
        var exportData = new byte[] { 0x01, 0x02 };

        _mockImportExportService.Setup(s => s.ExportContactsAsync("csv", null))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.ExportContacts("csv");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task ExportLeads_ReturnsFile()
    {
        // Arrange
        var exportData = new byte[] { 0x01, 0x02 };

        _mockImportExportService.Setup(s => s.ExportLeadsAsync("csv", null))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.ExportLeads("csv");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task ExportOpportunities_ReturnsFile()
    {
        // Arrange
        var exportData = new byte[] { 0x01, 0x02 };

        _mockImportExportService.Setup(s => s.ExportOpportunitiesAsync("csv", null))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.ExportOpportunities("csv");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task Export_InvalidFormat_ReturnsBadRequest()
    {
        // Arrange
        _mockImportExportService.Setup(s => s.ExportAccountsAsync("invalid", null))
            .ThrowsAsync(new ArgumentException("Invalid export format"));

        // Act
        var result = await _controller.ExportAccounts("invalid");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Import Tests

    [Fact]
    public async Task ImportAccounts_ValidFile_ReturnsOkWithResult()
    {
        // Arrange
        var csvContent = "Name,Email\nAcme Corp,info@acme.com";
        var fileMock = CreateMockFile("accounts.csv", csvContent);

        var importResult = new ImportResultDto
        {
            TotalRecords = 1,
            SuccessCount = 1,
            FailureCount = 0,
            Errors = new List<ImportErrorDto>()
        };

        _mockImportExportService.Setup(s => s.ImportAccountsAsync(fileMock.Object, null))
            .ReturnsAsync(importResult);

        // Act
        var result = await _controller.ImportAccounts(fileMock.Object);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value.Should().BeOfType<ImportResultDto>().Subject;
        returnedResult.SuccessCount.Should().Be(1);
    }

    [Fact]
    public async Task ImportAccounts_WithErrors_ReturnsOkWithErrors()
    {
        // Arrange
        var csvContent = "Name,Email\nAcme Corp,invalid-email";
        var fileMock = CreateMockFile("accounts.csv", csvContent);

        var importResult = new ImportResultDto
        {
            TotalRecords = 1,
            SuccessCount = 0,
            FailureCount = 1,
            Errors = new List<ImportErrorDto>
            {
                new ImportErrorDto { Row = 2, Field = "Email", Message = "Invalid email format" }
            }
        };

        _mockImportExportService.Setup(s => s.ImportAccountsAsync(fileMock.Object, null))
            .ReturnsAsync(importResult);

        // Act
        var result = await _controller.ImportAccounts(fileMock.Object);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value.Should().BeOfType<ImportResultDto>().Subject;
        returnedResult.FailureCount.Should().Be(1);
        returnedResult.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task ImportAccounts_NoFile_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.ImportAccounts(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ImportContacts_ValidFile_ReturnsOkWithResult()
    {
        // Arrange
        var csvContent = "FirstName,LastName,Email\nJohn,Doe,john@example.com";
        var fileMock = CreateMockFile("contacts.csv", csvContent);

        var importResult = new ImportResultDto
        {
            TotalRecords = 1,
            SuccessCount = 1,
            FailureCount = 0
        };

        _mockImportExportService.Setup(s => s.ImportContactsAsync(fileMock.Object, null))
            .ReturnsAsync(importResult);

        // Act
        var result = await _controller.ImportContacts(fileMock.Object);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ImportLeads_ValidFile_ReturnsOkWithResult()
    {
        // Arrange
        var csvContent = "Name,Email,Company\nJohn,john@example.com,Acme";
        var fileMock = CreateMockFile("leads.csv", csvContent);

        var importResult = new ImportResultDto
        {
            TotalRecords = 1,
            SuccessCount = 1,
            FailureCount = 0
        };

        _mockImportExportService.Setup(s => s.ImportLeadsAsync(fileMock.Object, null))
            .ReturnsAsync(importResult);

        // Act
        var result = await _controller.ImportLeads(fileMock.Object);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateImportFile_ValidFile_ReturnsOkWithValidation()
    {
        // Arrange
        var csvContent = "Name,Email\nAcme Corp,info@acme.com";
        var fileMock = CreateMockFile("accounts.csv", csvContent);

        var validationResult = new ImportValidationResultDto
        {
            IsValid = true,
            TotalRows = 1,
            ColumnMappings = new List<ColumnMappingDto>
            {
                new ColumnMappingDto { SourceColumn = "Name", TargetField = "Name" },
                new ColumnMappingDto { SourceColumn = "Email", TargetField = "Email" }
            }
        };

        _mockImportExportService.Setup(s => s.ValidateImportFileAsync("Account", fileMock.Object))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _controller.ValidateImportFile("Account", fileMock.Object);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value.Should().BeOfType<ImportValidationResultDto>().Subject;
        returnedResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateImportFile_InvalidFile_ReturnsValidationErrors()
    {
        // Arrange
        var csvContent = "InvalidColumn\nData";
        var fileMock = CreateMockFile("accounts.csv", csvContent);

        var validationResult = new ImportValidationResultDto
        {
            IsValid = false,
            ValidationErrors = new List<string>
            {
                "Required column 'Name' is missing",
                "Required column 'Email' is missing"
            }
        };

        _mockImportExportService.Setup(s => s.ValidateImportFileAsync("Account", fileMock.Object))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _controller.ValidateImportFile("Account", fileMock.Object);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value.Should().BeOfType<ImportValidationResultDto>().Subject;
        returnedResult.IsValid.Should().BeFalse();
    }

    #endregion

    #region Column Mapping Tests

    [Fact]
    public async Task GetFieldMappings_ReturnsAvailableFields()
    {
        // Arrange
        var fields = new List<FieldMappingDto>
        {
            new FieldMappingDto { FieldName = "Name", DisplayName = "Account Name", IsRequired = true },
            new FieldMappingDto { FieldName = "Email", DisplayName = "Email", IsRequired = false },
            new FieldMappingDto { FieldName = "Phone", DisplayName = "Phone", IsRequired = false }
        };

        _mockImportExportService.Setup(s => s.GetFieldMappingsAsync("Account"))
            .ReturnsAsync(fields);

        // Act
        var result = await _controller.GetFieldMappings("Account");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFields = okResult.Value.Should().BeAssignableTo<IEnumerable<FieldMappingDto>>().Subject;
        returnedFields.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetFieldMappings_InvalidEntity_ReturnsBadRequest()
    {
        // Arrange
        _mockImportExportService.Setup(s => s.GetFieldMappingsAsync("InvalidEntity"))
            .ThrowsAsync(new ArgumentException("Invalid entity type"));

        // Act
        var result = await _controller.GetFieldMappings("InvalidEntity");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SaveColumnMapping_ValidMapping_ReturnsOk()
    {
        // Arrange
        var mapping = new SaveColumnMappingDto
        {
            Name = "My Account Import",
            EntityType = "Account",
            Mappings = new List<ColumnMappingDto>
            {
                new ColumnMappingDto { SourceColumn = "Company", TargetField = "Name" }
            }
        };

        _mockImportExportService.Setup(s => s.SaveColumnMappingAsync(mapping))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.SaveColumnMapping(mapping);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { MappingId = 1 });
    }

    [Fact]
    public async Task GetSavedMappings_ReturnsMappings()
    {
        // Arrange
        var mappings = new List<SavedMappingDto>
        {
            new SavedMappingDto { Id = 1, Name = "Default Account Import", EntityType = "Account" },
            new SavedMappingDto { Id = 2, Name = "Custom Contact Import", EntityType = "Contact" }
        };

        _mockImportExportService.Setup(s => s.GetSavedMappingsAsync("Account"))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetSavedMappings("Account");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<SavedMappingDto>>();
    }

    [Fact]
    public async Task DeleteSavedMapping_ExistingMapping_ReturnsNoContent()
    {
        // Arrange
        _mockImportExportService.Setup(s => s.DeleteSavedMappingAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteSavedMapping(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region Template Tests

    [Fact]
    public async Task GetImportTemplate_ReturnsTemplate()
    {
        // Arrange
        var templateData = System.Text.Encoding.UTF8.GetBytes("Name,Email,Phone\n");

        _mockImportExportService.Setup(s => s.GetImportTemplateAsync("Account", "csv"))
            .ReturnsAsync(templateData);

        // Act
        var result = await _controller.GetImportTemplate("Account", "csv");

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public async Task GetImportTemplate_InvalidEntity_ReturnsBadRequest()
    {
        // Arrange
        _mockImportExportService.Setup(s => s.GetImportTemplateAsync("Invalid", "csv"))
            .ThrowsAsync(new ArgumentException("Invalid entity type"));

        // Act
        var result = await _controller.GetImportTemplate("Invalid", "csv");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Import History Tests

    [Fact]
    public async Task GetImportHistory_ReturnsHistory()
    {
        // Arrange
        var history = new List<ImportHistoryDto>
        {
            new ImportHistoryDto
            {
                Id = 1,
                EntityType = "Account",
                FileName = "accounts.csv",
                TotalRecords = 100,
                SuccessCount = 95,
                FailureCount = 5,
                ImportedAt = DateTime.UtcNow.AddDays(-1),
                ImportedBy = "admin@example.com"
            }
        };

        _mockImportExportService.Setup(s => s.GetImportHistoryAsync(null, 1, 20))
            .ReturnsAsync(new PagedResult<ImportHistoryDto> { Items = history, TotalCount = 1 });

        // Act
        var result = await _controller.GetImportHistory();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<PagedResult<ImportHistoryDto>>();
    }

    [Fact]
    public async Task GetImportHistoryById_ExistingRecord_ReturnsDetails()
    {
        // Arrange
        var historyDetail = new ImportHistoryDetailDto
        {
            Id = 1,
            EntityType = "Account",
            FileName = "accounts.csv",
            TotalRecords = 100,
            SuccessCount = 95,
            FailureCount = 5,
            Errors = new List<ImportErrorDto>
            {
                new ImportErrorDto { Row = 10, Field = "Email", Message = "Invalid email" }
            }
        };

        _mockImportExportService.Setup(s => s.GetImportHistoryByIdAsync(1))
            .ReturnsAsync(historyDetail);

        // Act
        var result = await _controller.GetImportHistoryById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<ImportHistoryDetailDto>();
    }

    #endregion

    #region Export History Tests

    [Fact]
    public async Task GetExportHistory_ReturnsHistory()
    {
        // Arrange
        var history = new List<ExportHistoryDto>
        {
            new ExportHistoryDto
            {
                Id = 1,
                EntityType = "Account",
                Format = "csv",
                RecordCount = 500,
                ExportedAt = DateTime.UtcNow.AddDays(-1),
                ExportedBy = "admin@example.com"
            }
        };

        _mockImportExportService.Setup(s => s.GetExportHistoryAsync(null, 1, 20))
            .ReturnsAsync(new PagedResult<ExportHistoryDto> { Items = history, TotalCount = 1 });

        // Act
        var result = await _controller.GetExportHistory();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<PagedResult<ExportHistoryDto>>();
    }

    #endregion

    #region Bulk Import Tests

    [Fact]
    public async Task StartBulkImport_ReturnsJobId()
    {
        // Arrange
        var csvContent = "Name,Email\nAcme Corp,info@acme.com";
        var fileMock = CreateMockFile("large_accounts.csv", csvContent);

        var bulkImportRequest = new BulkImportRequestDto
        {
            EntityType = "Account",
            File = fileMock.Object
        };

        _mockImportExportService.Setup(s => s.StartBulkImportAsync(bulkImportRequest))
            .ReturnsAsync("job-123");

        // Act
        var result = await _controller.StartBulkImport(bulkImportRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { JobId = "job-123" });
    }

    [Fact]
    public async Task GetBulkImportStatus_ReturnsStatus()
    {
        // Arrange
        var status = new BulkImportStatusDto
        {
            JobId = "job-123",
            Status = "InProgress",
            Progress = 50,
            ProcessedRecords = 500,
            TotalRecords = 1000
        };

        _mockImportExportService.Setup(s => s.GetBulkImportStatusAsync("job-123"))
            .ReturnsAsync(status);

        // Act
        var result = await _controller.GetBulkImportStatus("job-123");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStatus = okResult.Value.Should().BeOfType<BulkImportStatusDto>().Subject;
        returnedStatus.Progress.Should().Be(50);
    }

    [Fact]
    public async Task CancelBulkImport_ReturnsOk()
    {
        // Arrange
        _mockImportExportService.Setup(s => s.CancelBulkImportAsync("job-123"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CancelBulkImport("job-123");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion
}
