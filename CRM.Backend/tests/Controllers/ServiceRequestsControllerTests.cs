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
/// Comprehensive unit tests for ServiceRequestsController
/// Covers: CRUD operations, status, priority, assignment, SLA, categories, workflow
/// </summary>
public class ServiceRequestsControllerTests
{
    private readonly Mock<IServiceRequestService> _mockServiceRequestService;
    private readonly Mock<ILogger<ServiceRequestsController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly ServiceRequestsController _controller;

    public ServiceRequestsControllerTests()
    {
        _mockServiceRequestService = new Mock<IServiceRequestService>();
        _mockLogger = new Mock<ILogger<ServiceRequestsController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new ServiceRequestsController(_mockServiceRequestService.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithServiceRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestDto>
        {
            new ServiceRequestDto { Id = 1, Subject = "Cannot login", Status = "Open" },
            new ServiceRequestDto { Id = 2, Subject = "Email not working", Status = "InProgress" }
        };

        _mockServiceRequestService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(requests);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRequests = okResult.Value as IEnumerable<ServiceRequestDto>;
        returnedRequests.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStatus_ReturnsFilteredRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestDto>
        {
            new ServiceRequestDto { Id = 1, Status = "Open" }
        };

        _mockServiceRequestService.Setup(s => s.GetByStatusAsync("Open"))
            .ReturnsAsync(requests);

        // Act
        var result = await _controller.GetByStatus("Open");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByPriority_ReturnsFilteredRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestDto>
        {
            new ServiceRequestDto { Id = 1, Priority = "High" }
        };

        _mockServiceRequestService.Setup(s => s.GetByPriorityAsync("High"))
            .ReturnsAsync(requests);

        // Act
        var result = await _controller.GetByPriority("High");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByAssignee_ReturnsAssigneeRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestDto>
        {
            new ServiceRequestDto { Id = 1, AssignedToId = 1 }
        };

        _mockServiceRequestService.Setup(s => s.GetByAssigneeAsync(1))
            .ReturnsAsync(requests);

        // Act
        var result = await _controller.GetByAssignee(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetMyRequests_ReturnsCurrentUserRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestDto>
        {
            new ServiceRequestDto { Id = 1, AssignedToId = 1 }
        };

        _mockServiceRequestService.Setup(s => s.GetByAssigneeAsync(1))
            .ReturnsAsync(requests);

        // Act
        var result = await _controller.GetMyRequests();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByCategory_ReturnsFilteredRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestDto>
        {
            new ServiceRequestDto { Id = 1, CategoryId = 1 }
        };

        _mockServiceRequestService.Setup(s => s.GetByCategoryAsync(1))
            .ReturnsAsync(requests);

        // Act
        var result = await _controller.GetByCategory(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetOverdue_ReturnsOverdueRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestDto>
        {
            new ServiceRequestDto { Id = 1, SLADueDate = DateTime.Today.AddDays(-1) }
        };

        _mockServiceRequestService.Setup(s => s.GetOverdueAsync())
            .ReturnsAsync(requests);

        // Act
        var result = await _controller.GetOverdue();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingRequest_ReturnsOkWithRequest()
    {
        // Arrange
        var request = new ServiceRequestDto { Id = 1, Subject = "Cannot login" };

        _mockServiceRequestService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(request);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRequest = okResult.Value as ServiceRequestDto;
        returnedRequest!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingRequest_ReturnsNotFound()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((ServiceRequestDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByTicketNumber_ExistingRequest_ReturnsRequest()
    {
        // Arrange
        var request = new ServiceRequestDto { Id = 1, TicketNumber = "SR-2024-001" };

        _mockServiceRequestService.Setup(s => s.GetByTicketNumberAsync("SR-2024-001"))
            .ReturnsAsync(request);

        // Act
        var result = await _controller.GetByTicketNumber("SR-2024-001");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedWithRequest()
    {
        // Arrange
        var createDto = new CreateServiceRequestDto
        {
            Subject = "Application crashes on startup",
            Description = "The application crashes whenever I try to open it",
            Priority = "High",
            CategoryId = 1
        };

        var createdRequest = new ServiceRequestDto
        {
            Id = 1,
            Subject = createDto.Subject,
            Status = "Open",
            TicketNumber = "SR-2024-001"
        };

        _mockServiceRequestService.Setup(s => s.CreateAsync(It.IsAny<CreateServiceRequestDto>()))
            .ReturnsAsync(createdRequest);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedRequest = createdResult.Value as ServiceRequestDto;
        returnedRequest!.Status.Should().Be("Open");
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
    public async Task Create_MissingSubject_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateServiceRequestDto { Description = "No subject" };

        _mockServiceRequestService.Setup(s => s.CreateAsync(It.IsAny<CreateServiceRequestDto>()))
            .ThrowsAsync(new ArgumentException("Subject is required"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_InvalidCategory_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateServiceRequestDto
        {
            Subject = "Test",
            CategoryId = 9999
        };

        _mockServiceRequestService.Setup(s => s.CreateAsync(It.IsAny<CreateServiceRequestDto>()))
            .ThrowsAsync(new ArgumentException("Invalid category"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidRequest_ReturnsOkWithUpdatedRequest()
    {
        // Arrange
        var updateDto = new UpdateServiceRequestDto
        {
            Id = 1,
            Subject = "Updated subject",
            Description = "Updated description"
        };

        var updatedRequest = new ServiceRequestDto
        {
            Id = 1,
            Subject = "Updated subject"
        };

        _mockServiceRequestService.Setup(s => s.UpdateAsync(It.IsAny<UpdateServiceRequestDto>()))
            .ReturnsAsync(updatedRequest);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateServiceRequestDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingRequest_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateServiceRequestDto { Id = 999 };

        _mockServiceRequestService.Setup(s => s.UpdateAsync(It.IsAny<UpdateServiceRequestDto>()))
            .ReturnsAsync((ServiceRequestDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public async Task UpdateStatus_ValidStatus_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.UpdateStatusAsync(1, "InProgress"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateStatus(1, "InProgress");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Close_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.CloseAsync(1, "Resolved issue"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Close(1, "Resolved issue");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Close_AlreadyClosed_ReturnsConflict()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.CloseAsync(1, "Resolved"))
            .ThrowsAsync(new InvalidOperationException("Request is already closed"));

        // Act
        var result = await _controller.Close(1, "Resolved");

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Reopen_ClosedRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.ReopenAsync(1, "Additional information received"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Reopen(1, "Additional information received");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Cancel_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.CancelAsync(1, "Duplicate request"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Cancel(1, "Duplicate request");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Hold_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.PutOnHoldAsync(1, "Awaiting customer response"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Hold(1, "Awaiting customer response");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Assignment Tests

    [Fact]
    public async Task Assign_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.AssignAsync(1, 2))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Assign(1, 2);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Unassign_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.UnassignAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Unassign(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task AssignToMe_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.AssignAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AssignToMe(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task AutoAssign_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.AutoAssignAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AutoAssign(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Priority Tests

    [Fact]
    public async Task UpdatePriority_ValidPriority_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.UpdatePriorityAsync(1, "Critical"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdatePriority(1, "Critical");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Escalate_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.EscalateAsync(1, "Customer is VIP"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Escalate(1, "Customer is VIP");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region SLA Tests

    [Fact]
    public async Task GetSLAInfo_ValidRequest_ReturnsSLAInfo()
    {
        // Arrange
        var slaInfo = new ServiceRequestSLADto
        {
            RequestId = 1,
            SLAPolicyId = 1,
            ResponseDue = DateTime.Today.AddHours(4),
            ResolutionDue = DateTime.Today.AddDays(1),
            ResponseSLAMet = true,
            ResolutionSLAMet = null
        };

        _mockServiceRequestService.Setup(s => s.GetSLAInfoAsync(1))
            .ReturnsAsync(slaInfo);

        // Act
        var result = await _controller.GetSLAInfo(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetSLABreached_ReturnsBreachedRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestDto>
        {
            new ServiceRequestDto { Id = 1, SLABreached = true }
        };

        _mockServiceRequestService.Setup(s => s.GetSLABreachedAsync())
            .ReturnsAsync(requests);

        // Act
        var result = await _controller.GetSLABreached();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task PauseSLA_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.PauseSLAAsync(1, "Waiting for customer"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.PauseSLA(1, "Waiting for customer");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ResumeSLA_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.ResumeSLAAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResumeSLA(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Comment Tests

    [Fact]
    public async Task AddComment_ValidComment_ReturnsCreated()
    {
        // Arrange
        var commentDto = new AddCommentDto
        {
            Content = "This is a comment",
            IsInternal = false
        };

        var createdComment = new ServiceRequestCommentDto
        {
            Id = 1,
            Content = commentDto.Content,
            CreatedAt = DateTime.Now
        };

        _mockServiceRequestService.Setup(s => s.AddCommentAsync(1, commentDto))
            .ReturnsAsync(createdComment);

        // Act
        var result = await _controller.AddComment(1, commentDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GetComments_ReturnsComments()
    {
        // Arrange
        var comments = new List<ServiceRequestCommentDto>
        {
            new ServiceRequestCommentDto { Id = 1, Content = "Comment 1" },
            new ServiceRequestCommentDto { Id = 2, Content = "Comment 2" }
        };

        _mockServiceRequestService.Setup(s => s.GetCommentsAsync(1))
            .ReturnsAsync(comments);

        // Act
        var result = await _controller.GetComments(1);

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
        file.Setup(f => f.FileName).Returns("screenshot.png");

        _mockServiceRequestService.Setup(s => s.AddAttachmentAsync(1, It.IsAny<byte[]>(), "screenshot.png"))
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
            new AttachmentDto { Id = 1, FileName = "screenshot.png" }
        };

        _mockServiceRequestService.Setup(s => s.GetAttachmentsAsync(1))
            .ReturnsAsync(attachments);

        // Act
        var result = await _controller.GetAttachments(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Knowledge Base Integration Tests

    [Fact]
    public async Task LinkArticle_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.LinkArticleAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.LinkArticle(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetSuggestedArticles_ReturnsSuggestions()
    {
        // Arrange
        var articles = new List<KnowledgeArticleSummaryDto>
        {
            new KnowledgeArticleSummaryDto { Id = 1, Title = "How to reset password" }
        };

        _mockServiceRequestService.Setup(s => s.GetSuggestedArticlesAsync(1))
            .ReturnsAsync(articles);

        // Act
        var result = await _controller.GetSuggestedArticles(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Related Requests Tests

    [Fact]
    public async Task GetRelatedRequests_ReturnsRelated()
    {
        // Arrange
        var related = new List<ServiceRequestDto>
        {
            new ServiceRequestDto { Id = 2, Subject = "Similar issue" }
        };

        _mockServiceRequestService.Setup(s => s.GetRelatedRequestsAsync(1))
            .ReturnsAsync(related);

        // Act
        var result = await _controller.GetRelatedRequests(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Merge_ValidRequests_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.MergeRequestsAsync(1, new List<int> { 2, 3 }))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Merge(1, new List<int> { 2, 3 });

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task MarkAsDuplicate_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.MarkAsDuplicateAsync(2, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkAsDuplicate(2, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkAssign_ValidRequest_ReturnsCount()
    {
        // Arrange
        var request = new BulkAssignServiceRequestsRequest
        {
            RequestIds = new List<int> { 1, 2, 3 },
            AssigneeId = 1
        };

        _mockServiceRequestService.Setup(s => s.BulkAssignAsync(request.RequestIds, request.AssigneeId))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkAssign(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkClose_ValidRequest_ReturnsCount()
    {
        // Arrange
        var request = new BulkCloseServiceRequestsRequest
        {
            RequestIds = new List<int> { 1, 2, 3 },
            Resolution = "Closed in bulk"
        };

        _mockServiceRequestService.Setup(s => s.BulkCloseAsync(request.RequestIds, request.Resolution))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkClose(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkUpdatePriority_ValidRequest_ReturnsCount()
    {
        // Arrange
        var request = new BulkUpdatePriorityRequest
        {
            RequestIds = new List<int> { 1, 2, 3 },
            Priority = "High"
        };

        _mockServiceRequestService.Setup(s => s.BulkUpdatePriorityAsync(request.RequestIds, request.Priority))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkUpdatePriority(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingRequests()
    {
        // Arrange
        var requests = new List<ServiceRequestDto>
        {
            new ServiceRequestDto { Id = 1, Subject = "Cannot login" }
        };

        _mockServiceRequestService.Setup(s => s.SearchAsync("login"))
            .ReturnsAsync(requests);

        // Act
        var result = await _controller.Search("login");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatistics_ReturnsStats()
    {
        // Arrange
        var stats = new ServiceRequestStatisticsDto
        {
            TotalOpen = 50,
            TotalClosed = 200,
            AverageResolutionTime = TimeSpan.FromHours(24),
            SLAComplianceRate = 0.95m
        };

        _mockServiceRequestService.Setup(s => s.GetStatisticsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingRequest_ReturnsNoContent()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingRequest_ReturnsNotFound()
    {
        // Arrange
        _mockServiceRequestService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}
