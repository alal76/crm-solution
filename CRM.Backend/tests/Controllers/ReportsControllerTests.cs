// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Dtos.Reports;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for ReportsController
/// Covers: Report definitions, execution, scheduling, export, folders
/// </summary>
public class ReportsControllerTests
{
    private readonly Mock<IReportService> _mockReportService;
    private readonly Mock<ILogger<ReportsController>> _mockLogger;
    private readonly ReportsController _controller;

    public ReportsControllerTests()
    {
        _mockReportService = new Mock<IReportService>();
        _mockLogger = new Mock<ILogger<ReportsController>>();

        _controller = new ReportsController(_mockReportService.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithReports()
    {
        // Arrange
        var reports = new List<ReportDefinitionDto>
        {
            new ReportDefinitionDto { Id = 1, Name = "Sales Report", Category = "Sales" },
            new ReportDefinitionDto { Id = 2, Name = "Marketing Report", Category = "Marketing" }
        };

        _mockReportService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reports);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedReports = okResult.Value as IEnumerable<ReportDefinitionDto>;
        returnedReports.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByCategory_ReturnsFilteredReports()
    {
        // Arrange
        var reports = new List<ReportDefinitionDto>
        {
            new ReportDefinitionDto { Id = 1, Category = "Sales" }
        };

        _mockReportService.Setup(s => s.GetByCategoryAsync("Sales", It.IsAny<CancellationToken>()))
            .ReturnsAsync(reports);

        // Act
        var result = await _controller.GetByCategory("Sales");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByFolder_ReturnsReportsInFolder()
    {
        // Arrange
        var reports = new List<ReportDefinitionDto>
        {
            new ReportDefinitionDto { Id = 1, FolderId = 1 }
        };

        _mockReportService.Setup(s => s.GetByFolderAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reports);

        // Act
        var result = await _controller.GetByFolder(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMyReports_ReturnsUserReports()
    {
        // Arrange
        var reports = new List<ReportDefinitionDto>
        {
            new ReportDefinitionDto { Id = 1, CreatedById = 1 }
        };

        _mockReportService.Setup(s => s.GetByUserAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reports);

        // Act
        var result = await _controller.GetMyReports();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStandardReports_ReturnsStandardReports()
    {
        // Arrange
        var reports = new List<ReportDefinitionDto>
        {
            new ReportDefinitionDto { Id = 1, IsStandard = true }
        };

        _mockReportService.Setup(s => s.GetStandardReportsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reports);

        // Act
        var result = await _controller.GetStandardReports();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingReport_ReturnsOkWithReport()
    {
        // Arrange
        var report = new ReportDefinitionDto { Id = 1, Name = "Sales Report" };

        _mockReportService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedReport = okResult.Value as ReportDefinitionDto;
        returnedReport!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingReport_ReturnsNotFound()
    {
        // Arrange
        _mockReportService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReportDefinitionDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidReport_ReturnsCreatedWithReport()
    {
        // Arrange
        var createDto = new CreateReportDefinitionDto
        {
            Name = "New Sales Report",
            Description = "Monthly sales analysis",
            Query = "SELECT * FROM Orders",
            Category = "Sales"
        };

        var createdReport = new ReportDefinitionDto
        {
            Id = 1,
            Name = "New Sales Report",
            Category = "Sales"
        };

        _mockReportService.Setup(s => s.CreateAsync(It.IsAny<CreateReportDefinitionDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdReport);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateReportDefinitionDto { Name = "Existing Report" };

        _mockReportService.Setup(s => s.CreateAsync(It.IsAny<CreateReportDefinitionDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Report with name already exists"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidReport_ReturnsOkWithUpdatedReport()
    {
        // Arrange
        var updateDto = new UpdateReportDefinitionDto
        {
            Id = 1,
            Name = "Updated Report"
        };

        var updatedReport = new ReportDefinitionDto
        {
            Id = 1,
            Name = "Updated Report"
        };

        _mockReportService.Setup(s => s.UpdateAsync(It.IsAny<UpdateReportDefinitionDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedReport);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateReportDefinitionDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Execution Tests

    [Fact]
    public async Task Execute_ValidReport_ReturnsResults()
    {
        // Arrange
        var executionResult = new ReportExecutionResultDto
        {
            ReportId = 1,
            Columns = new List<string> { "Name", "Revenue" },
            Data = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "Name", "Product A" }, { "Revenue", 1000 } }
            },
            ExecutedAt = DateTime.Now
        };

        _mockReportService.Setup(s => s.ExecuteAsync(1, It.IsAny<ReportParametersDto?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(executionResult);

        // Act
        var result = await _controller.Execute(1, null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Execute_WithParameters_ReturnsFilteredResults()
    {
        // Arrange
        var parameters = new ReportParametersDto
        {
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today,
            Filters = new Dictionary<string, object> { { "Region", "North" } }
        };

        var executionResult = new ReportExecutionResultDto
        {
            ReportId = 1,
            Data = new List<Dictionary<string, object>>()
        };

        _mockReportService.Setup(s => s.ExecuteAsync(1, parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(executionResult);

        // Act
        var result = await _controller.Execute(1, parameters);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Execute_InvalidQuery_ReturnsBadRequest()
    {
        // Arrange
        _mockReportService.Setup(s => s.ExecuteAsync(1, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid SQL query"));

        // Act
        var result = await _controller.Execute(1, null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Preview_ReturnsLimitedResults()
    {
        // Arrange
        var executionResult = new ReportExecutionResultDto
        {
            ReportId = 1,
            Data = new List<Dictionary<string, object>>()
        };

        _mockReportService.Setup(s => s.PreviewAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(executionResult);

        // Act
        var result = await _controller.Preview(1, 10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Export Tests

    [Fact]
    public async Task Export_ToCsv_ReturnsFile()
    {
        // Arrange
        var csvData = new byte[] { 78, 97, 109, 101, 44, 82, 101, 118 }; // "Name,Rev..."

        _mockReportService.Setup(s => s.ExportAsync(1, "csv", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(csvData);

        // Act
        var result = await _controller.Export(1, "csv", null);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public async Task Export_ToExcel_ReturnsFile()
    {
        // Arrange
        var excelData = new byte[] { 80, 75, 3, 4 }; // ZIP header (xlsx)

        _mockReportService.Setup(s => s.ExportAsync(1, "xlsx", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(excelData);

        // Act
        var result = await _controller.Export(1, "xlsx", null);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
    }

    [Fact]
    public async Task Export_ToPdf_ReturnsFile()
    {
        // Arrange
        var pdfData = new byte[] { 37, 80, 68, 70 }; // PDF header

        _mockReportService.Setup(s => s.ExportAsync(1, "pdf", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pdfData);

        // Act
        var result = await _controller.Export(1, "pdf", null);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task Export_UnsupportedFormat_ThrowsArgumentException()
    {
        // Arrange
        _mockReportService.Setup(s => s.ExportAsync(1, "invalid", null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Unsupported export format"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _controller.Export(1, "invalid", null));
    }

    #endregion

    #region Schedule Tests

    [Fact]
    public async Task CreateSchedule_ValidSchedule_ReturnsCreated()
    {
        // Arrange
        var scheduleDto = new CreateReportScheduleDto
        {
            ReportId = 1,
            Frequency = "Daily",
            Time = TimeOnly.Parse("09:00"),
            Recipients = new List<string> { "user@example.com" },
            ExportFormat = "csv"
        };

        var createdSchedule = new ReportScheduleDto { Id = 1, ReportId = 1 };

        _mockReportService.Setup(s => s.CreateScheduleAsync(scheduleDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSchedule);

        // Act
        var result = await _controller.CreateSchedule(1, scheduleDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GetSchedules_ReturnsSchedules()
    {
        // Arrange
        var schedules = new List<ReportScheduleDto>
        {
            new ReportScheduleDto { Id = 1, ReportId = 1, Frequency = "Daily" },
            new ReportScheduleDto { Id = 2, ReportId = 1, Frequency = "Weekly" }
        };

        _mockReportService.Setup(s => s.GetSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedules);

        // Act
        var result = await _controller.GetSchedules(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateSchedule_ValidSchedule_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateReportScheduleDto
        {
            Id = 1,
            Frequency = "Weekly"
        };

        var updatedSchedule = new ReportScheduleDto { Id = 1, Frequency = "Weekly" };

        _mockReportService.Setup(s => s.UpdateScheduleAsync(updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedSchedule);

        // Act
        var result = await _controller.UpdateSchedule(1, 1, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteSchedule_ReturnsNoContent()
    {
        // Arrange
        _mockReportService.Setup(s => s.DeleteScheduleAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteSchedule(1, 1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ToggleSchedule_EnableSchedule_ReturnsOk()
    {
        // Arrange
        _mockReportService.Setup(s => s.ToggleScheduleAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ToggleSchedule(1, 1, true);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Folder Tests

    [Fact]
    public async Task GetFolders_ReturnsFolders()
    {
        // Arrange
        var folders = new List<ReportFolderDto>
        {
            new ReportFolderDto { Id = 1, Name = "Sales Reports" },
            new ReportFolderDto { Id = 2, Name = "Marketing Reports" }
        };

        _mockReportService.Setup(s => s.GetFoldersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(folders);

        // Act
        var result = await _controller.GetFolders();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateFolder_ValidFolder_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateReportFolderDto
        {
            Name = "New Folder",
            ParentId = null
        };

        var createdFolder = new ReportFolderDto { Id = 1, Name = "New Folder" };

        _mockReportService.Setup(s => s.CreateFolderAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdFolder);

        // Act
        var result = await _controller.CreateFolder(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateFolder_ValidFolder_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateReportFolderDto
        {
            Id = 1,
            Name = "Renamed Folder"
        };

        var updatedFolder = new ReportFolderDto { Id = 1, Name = "Renamed Folder" };

        _mockReportService.Setup(s => s.UpdateFolderAsync(updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedFolder);

        // Act
        var result = await _controller.UpdateFolder(1, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteFolder_EmptyFolder_ReturnsNoContent()
    {
        // Arrange
        _mockReportService.Setup(s => s.DeleteFolderAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteFolder(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteFolder_NonEmptyFolder_ReturnsConflict()
    {
        // Arrange
        _mockReportService.Setup(s => s.DeleteFolderAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Folder contains reports"));

        // Act
        var result = await _controller.DeleteFolder(1);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task MoveReportToFolder_ValidMove_ReturnsOk()
    {
        // Arrange
        _mockReportService.Setup(s => s.MoveToFolderAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MoveReportToFolder(1, 2);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Execution History Tests

    [Fact]
    public async Task GetExecutionHistory_ReturnsHistory()
    {
        // Arrange
        var history = new List<ReportExecutionHistoryDto>
        {
            new ReportExecutionHistoryDto { Id = 1, ReportId = 1, ExecutedAt = DateTime.Now.AddDays(-1) },
            new ReportExecutionHistoryDto { Id = 2, ReportId = 1, ExecutedAt = DateTime.Now }
        };

        _mockReportService.Setup(s => s.GetExecutionHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetExecutionHistory(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetExecutionResult_ReturnsStoredResult()
    {
        // Arrange
        var executionResult = new ReportExecutionResultDto
        {
            ReportId = 1,
            ExecutedAt = DateTime.Now.AddDays(-1),
            Data = new List<Dictionary<string, object>>()
        };

        _mockReportService.Setup(s => s.GetExecutionResultAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(executionResult);

        // Act
        var result = await _controller.GetExecutionResult(1, 1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Clone and Share Tests

    [Fact]
    public async Task Clone_ValidReport_ReturnsOk()
    {
        // Arrange
        var clonedReport = new ReportDefinitionDto { Id = 2, Name = "Sales Report (Copy)" };

        _mockReportService.Setup(s => s.CloneAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clonedReport);

        // Act
        var result = await _controller.Clone(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Share_ValidReport_ReturnsOk()
    {
        // Arrange
        var shareDto = new ShareReportDto
        {
            UserIds = new List<int> { 2, 3 },
            GroupIds = new List<int> { 1 }
        };

        _mockReportService.Setup(s => s.ShareAsync(1, shareDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Share(1, shareDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSharedWith_ReturnsSharedUsers()
    {
        // Arrange
        var sharedWith = new ReportSharingDto
        {
            Users = new List<UserDto> { new UserDto { Id = 2 } },
            Groups = new List<UserGroupDto> { new UserGroupDto { Id = 1 } }
        };

        _mockReportService.Setup(s => s.GetSharedWithAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sharedWith);

        // Act
        var result = await _controller.GetSharedWith(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Favorites Tests

    [Fact]
    public async Task AddToFavorites_ReturnsOk()
    {
        // Arrange
        _mockReportService.Setup(s => s.AddToFavoritesAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AddToFavorites(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RemoveFromFavorites_ReturnsOk()
    {
        // Arrange
        _mockReportService.Setup(s => s.RemoveFromFavoritesAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveFromFavorites(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetFavorites_ReturnsFavoriteReports()
    {
        // Arrange
        var favorites = new List<ReportDefinitionDto>
        {
            new ReportDefinitionDto { Id = 1, Name = "Favorite Report" }
        };

        _mockReportService.Setup(s => s.GetFavoritesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(favorites);

        // Act
        var result = await _controller.GetFavorites();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingReport_ReturnsNoContent()
    {
        // Arrange
        _mockReportService.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingReport_ReturnsNotFound()
    {
        // Arrange
        _mockReportService.Setup(s => s.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_StandardReport_ReturnsForbid()
    {
        // Arrange
        _mockReportService.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Cannot delete standard report"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    #endregion
}
