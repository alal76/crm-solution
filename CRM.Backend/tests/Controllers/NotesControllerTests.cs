// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Notes Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Api.Hubs;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for NotesController
/// Covers: CRUD operations, entity associations, search, pins, visibility
/// </summary>
public class NotesControllerTests
{
    private readonly Mock<INoteService> _mockNoteService;
    private readonly Mock<ILogger<NotesController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly NotesController _controller;

    public NotesControllerTests()
    {
        _mockNoteService = new Mock<INoteService>();
        _mockLogger = new Mock<ILogger<NotesController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new NotesController(_mockNoteService.Object, _mockLogger.Object, _mockNotificationService.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithNotes()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, Content = "Note 1", CreatedAt = DateTime.Today },
            new NoteDto { Id = 2, Content = "Note 2", CreatedAt = DateTime.Today }
        };

        _mockNoteService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedNotes = okResult.Value as IEnumerable<NoteDto>;
        returnedNotes.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMyNotes_ReturnsUserNotes()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, CreatedById = 1 }
        };

        _mockNoteService.Setup(s => s.GetByUserAsync(1))
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.GetMyNotes();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetRecent_ReturnsRecentNotes()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, CreatedAt = DateTime.Today }
        };

        _mockNoteService.Setup(s => s.GetRecentAsync(10))
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.GetRecent(10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetPinned_ReturnsPinnedNotes()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, IsPinned = true }
        };

        _mockNoteService.Setup(s => s.GetPinnedAsync(1))
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.GetPinned();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingNote_ReturnsOkWithNote()
    {
        // Arrange
        var note = new NoteDto { Id = 1, Content = "Test note" };

        _mockNoteService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(note);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedNote = okResult.Value as NoteDto;
        returnedNote!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingNote_ReturnsNotFound()
    {
        // Arrange
        _mockNoteService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((NoteDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_PrivateNoteOtherUser_ReturnsForbid()
    {
        // Arrange
        _mockNoteService.Setup(s => s.GetByIdAsync(1))
            .ThrowsAsync(new UnauthorizedAccessException("Cannot access private note"));

        // Act
        var result = await _controller.GetById(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidNote_ReturnsCreatedWithNote()
    {
        // Arrange
        var createDto = new CreateNoteDto
        {
            Content = "New note content",
            EntityType = "Account",
            EntityId = 1
        };

        var createdNote = new NoteDto
        {
            Id = 1,
            Content = createDto.Content,
            CreatedAt = DateTime.Now
        };

        _mockNoteService.Setup(s => s.CreateAsync(It.IsAny<CreateNoteDto>()))
            .ReturnsAsync(createdNote);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedNote = createdResult.Value as NoteDto;
        returnedNote!.Content.Should().Be("New note content");
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
    public async Task Create_EmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateNoteDto { Content = "" };

        _mockNoteService.Setup(s => s.CreateAsync(It.IsAny<CreateNoteDto>()))
            .ThrowsAsync(new ArgumentException("Content is required"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_InvalidEntity_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateNoteDto
        {
            Content = "Test",
            EntityType = "InvalidType",
            EntityId = 1
        };

        _mockNoteService.Setup(s => s.CreateAsync(It.IsAny<CreateNoteDto>()))
            .ThrowsAsync(new ArgumentException("Invalid entity type"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidNote_ReturnsOkWithUpdatedNote()
    {
        // Arrange
        var updateDto = new UpdateNoteDto
        {
            Id = 1,
            Content = "Updated content"
        };

        var updatedNote = new NoteDto
        {
            Id = 1,
            Content = "Updated content"
        };

        _mockNoteService.Setup(s => s.UpdateAsync(It.IsAny<UpdateNoteDto>()))
            .ReturnsAsync(updatedNote);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateNoteDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingNote_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateNoteDto { Id = 999 };

        _mockNoteService.Setup(s => s.UpdateAsync(It.IsAny<UpdateNoteDto>()))
            .ReturnsAsync((NoteDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_OtherUsersNote_ReturnsForbid()
    {
        // Arrange
        var updateDto = new UpdateNoteDto { Id = 1, Content = "Updated" };

        _mockNoteService.Setup(s => s.UpdateAsync(It.IsAny<UpdateNoteDto>()))
            .ThrowsAsync(new UnauthorizedAccessException("Cannot update other user's note"));

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Entity Association Tests

    [Fact]
    public async Task GetByAccount_ReturnsAccountNotes()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, EntityType = "Account", EntityId = 1 }
        };

        _mockNoteService.Setup(s => s.GetByEntityAsync("Account", 1))
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.GetByAccount(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByContact_ReturnsContactNotes()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, EntityType = "Contact", EntityId = 1 }
        };

        _mockNoteService.Setup(s => s.GetByEntityAsync("Contact", 1))
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.GetByContact(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByOpportunity_ReturnsOpportunityNotes()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, EntityType = "Opportunity", EntityId = 1 }
        };

        _mockNoteService.Setup(s => s.GetByEntityAsync("Opportunity", 1))
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.GetByOpportunity(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByLead_ReturnsLeadNotes()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, EntityType = "Lead", EntityId = 1 }
        };

        _mockNoteService.Setup(s => s.GetByEntityAsync("Lead", 1))
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.GetByLead(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByTask_ReturnsTaskNotes()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, EntityType = "Task", EntityId = 1 }
        };

        _mockNoteService.Setup(s => s.GetByEntityAsync("Task", 1))
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.GetByTask(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Pin Tests

    [Fact]
    public async Task Pin_ValidNote_ReturnsOk()
    {
        // Arrange
        _mockNoteService.Setup(s => s.PinAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Pin(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Unpin_ValidNote_ReturnsOk()
    {
        // Arrange
        _mockNoteService.Setup(s => s.UnpinAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Unpin(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Pin_NonExistingNote_ReturnsNotFound()
    {
        // Arrange
        _mockNoteService.Setup(s => s.PinAsync(999, 1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Pin(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Visibility Tests

    [Fact]
    public async Task SetPrivate_ValidNote_ReturnsOk()
    {
        // Arrange
        _mockNoteService.Setup(s => s.SetPrivateAsync(1, true))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SetPrivate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SetPublic_ValidNote_ReturnsOk()
    {
        // Arrange
        _mockNoteService.Setup(s => s.SetPrivateAsync(1, false))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SetPublic(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SetPrivate_OtherUsersNote_ReturnsForbid()
    {
        // Arrange
        _mockNoteService.Setup(s => s.SetPrivateAsync(1, true))
            .ThrowsAsync(new UnauthorizedAccessException("Cannot change visibility of other user's note"));

        // Act
        var result = await _controller.SetPrivate(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingNotes()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, Content = "Meeting notes about project" }
        };

        _mockNoteService.Setup(s => s.SearchAsync("meeting"))
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.Search("meeting");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Search_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Search("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SearchByDateRange_ReturnsNotesInRange()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;

        var notes = new List<NoteDto>
        {
            new NoteDto { Id = 1, CreatedAt = DateTime.Today.AddDays(-3) }
        };

        _mockNoteService.Setup(s => s.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(notes);

        // Act
        var result = await _controller.SearchByDateRange(startDate, endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Attachment Tests

    [Fact]
    public async Task AddAttachment_ValidFile_ReturnsOk()
    {
        // Arrange
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(1000);
        file.Setup(f => f.FileName).Returns("document.pdf");

        _mockNoteService.Setup(s => s.AddAttachmentAsync(1, It.IsAny<byte[]>(), "document.pdf"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AddAttachment(1, file.Object);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetAttachments_ReturnsAttachments()
    {
        // Arrange
        var attachments = new List<AttachmentDto>
        {
            new AttachmentDto { Id = 1, FileName = "document.pdf" }
        };

        _mockNoteService.Setup(s => s.GetAttachmentsAsync(1))
            .ReturnsAsync(attachments);

        // Act
        var result = await _controller.GetAttachments(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task RemoveAttachment_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockNoteService.Setup(s => s.RemoveAttachmentAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveAttachment(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkDelete_ValidIds_ReturnsCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockNoteService.Setup(s => s.BulkDeleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDelete(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkPin_ValidIds_ReturnsCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2 };

        _mockNoteService.Setup(s => s.BulkPinAsync(ids, 1))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.BulkPin(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkSetPrivate_ValidIds_ReturnsCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2 };

        _mockNoteService.Setup(s => s.BulkSetPrivateAsync(ids, true))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.BulkSetPrivate(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingNote_ReturnsNoContent()
    {
        // Arrange
        _mockNoteService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingNote_ReturnsNotFound()
    {
        // Arrange
        _mockNoteService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_OtherUsersNote_ReturnsForbid()
    {
        // Arrange
        _mockNoteService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new UnauthorizedAccessException("Cannot delete other user's note"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region History Tests

    [Fact]
    public async Task GetHistory_ReturnsNoteHistory()
    {
        // Arrange
        var history = new List<NoteHistoryDto>
        {
            new NoteHistoryDto { Id = 1, Version = 1, Content = "Original content" },
            new NoteHistoryDto { Id = 2, Version = 2, Content = "Updated content" }
        };

        _mockNoteService.Setup(s => s.GetHistoryAsync(1))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetHistory(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task RestoreVersion_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockNoteService.Setup(s => s.RestoreVersionAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RestoreVersion(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Export Tests

    [Fact]
    public async Task Export_ValidFormat_ReturnsFile()
    {
        // Arrange
        var exportData = new byte[] { 1, 2, 3 };

        _mockNoteService.Setup(s => s.ExportAsync(It.IsAny<List<int>>(), "pdf"))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.Export(new List<int> { 1, 2 }, "pdf");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    #endregion
}
