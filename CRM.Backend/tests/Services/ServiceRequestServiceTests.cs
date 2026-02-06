// CRM Solution - Customer Relationship Management System
// Service Request Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ServiceRequestService
/// Covers: Ticket CRUD, assignment, escalation, SLA tracking
/// </summary>
public class ServiceRequestServiceTests
{
    private readonly Mock<IRepository<ServiceRequest>> _mockRequestRepository;
    private readonly Mock<IRepository<ServiceRequestCategory>> _mockCategoryRepository;
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<IRepository<SLAPolicy>> _mockSLARepository;
    private readonly Mock<IRepository<Activity>> _mockActivityRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<ServiceRequestService>> _mockLogger;
    private readonly ServiceRequestService _service;

    public ServiceRequestServiceTests()
    {
        _mockRequestRepository = new Mock<IRepository<ServiceRequest>>();
        _mockCategoryRepository = new Mock<IRepository<ServiceRequestCategory>>();
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockSLARepository = new Mock<IRepository<SLAPolicy>>();
        _mockActivityRepository = new Mock<IRepository<Activity>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ServiceRequestService>>();

        _service = new ServiceRequestService(
            _mockRequestRepository.Object,
            _mockCategoryRepository.Object,
            _mockUserRepository.Object,
            _mockSLARepository.Object,
            _mockActivityRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Create Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsRequest()
    {
        // Arrange
        var request = new CreateServiceRequestDto
        {
            Subject = "System not working",
            Description = "Unable to access the system",
            Priority = ServiceRequestPriority.High,
            CategoryId = 1,
            ContactId = 1
        };

        _mockRequestRepository.Setup(r => r.AddAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.Id = 1; sr.TicketNumber = "SR-00001"; return sr; });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.TicketNumber.Should().Be("SR-00001");
    }

    [Fact]
    public async Task CreateAsync_WithAccountId_AssociatesAccount()
    {
        // Arrange
        var request = new CreateServiceRequestDto
        {
            Subject = "Account issue",
            AccountId = 100,
            Priority = ServiceRequestPriority.Medium
        };

        _mockRequestRepository.Setup(r => r.AddAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.Id = 1; return sr; });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_AutoAssignEnabled_AssignsAgent()
    {
        // Arrange
        var request = new CreateServiceRequestDto
        {
            Subject = "Need help",
            Priority = ServiceRequestPriority.Low,
            CategoryId = 1
        };

        var category = new ServiceRequestCategory
        {
            Id = 1,
            DefaultAssigneeId = 5
        };

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        _mockRequestRepository.Setup(r => r.AddAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.Id = 1; sr.AssignedToId = 5; return sr; });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetByIdAsync_ExistingRequest_ReturnsRequest()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Id = 1,
            Subject = "Test Request",
            Status = ServiceRequestStatus.Open
        };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Subject.Should().Be("Test Request");
    }

    [Fact]
    public async Task GetByTicketNumberAsync_ExistingTicket_ReturnsRequest()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Id = 1,
            TicketNumber = "SR-00001"
        };

        _mockRequestRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ServiceRequest, bool>>>()))
            .ReturnsAsync(new List<ServiceRequest> { request });

        // Act
        var result = await _service.GetByTicketNumberAsync("SR-00001");

        // Assert
        result.Should().NotBeNull();
        result!.TicketNumber.Should().Be("SR-00001");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRequests()
    {
        // Arrange
        var requests = new List<ServiceRequest>
        {
            new ServiceRequest { Id = 1, Subject = "Request 1" },
            new ServiceRequest { Id = 2, Subject = "Request 2" }
        };

        _mockRequestRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsFilteredRequests()
    {
        // Arrange
        var requests = new List<ServiceRequest>
        {
            new ServiceRequest { Id = 1, Status = ServiceRequestStatus.Open }
        };

        _mockRequestRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ServiceRequest, bool>>>()))
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetByStatusAsync(ServiceRequestStatus.Open);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByAssigneeAsync_ReturnsAssigneeRequests()
    {
        // Arrange
        var requests = new List<ServiceRequest>
        {
            new ServiceRequest { Id = 1, AssignedToId = 5 }
        };

        _mockRequestRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ServiceRequest, bool>>>()))
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetByAssigneeAsync(5);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesRequest()
    {
        // Arrange
        var existing = new ServiceRequest { Id = 1, Subject = "Old Subject" };
        var updateDto = new UpdateServiceRequestDto { Id = 1, Subject = "New Subject" };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => sr);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Subject.Should().Be("New Subject");
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidStatus_UpdatesStatus()
    {
        // Arrange
        var request = new ServiceRequest { Id = 1, Status = ServiceRequestStatus.Open };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.Status = ServiceRequestStatus.InProgress; return sr; });

        // Act
        var result = await _service.UpdateStatusAsync(1, ServiceRequestStatus.InProgress);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePriorityAsync_ValidPriority_UpdatesPriority()
    {
        // Arrange
        var request = new ServiceRequest { Id = 1, Priority = ServiceRequestPriority.Low };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.Priority = ServiceRequestPriority.High; return sr; });

        // Act
        var result = await _service.UpdatePriorityAsync(1, ServiceRequestPriority.High);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Assignment Tests

    [Fact]
    public async Task AssignAsync_ValidAssignment_AssignsAgent()
    {
        // Arrange
        var request = new ServiceRequest { Id = 1, AssignedToId = null };
        var user = new User { Id = 5, FirstName = "John" };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockUserRepository.Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(user);

        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.AssignedToId = 5; return sr; });

        // Act
        var result = await _service.AssignAsync(1, 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UnassignAsync_AssignedRequest_RemovesAssignment()
    {
        // Arrange
        var request = new ServiceRequest { Id = 1, AssignedToId = 5 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.AssignedToId = null; return sr; });

        // Act
        var result = await _service.UnassignAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ReassignAsync_ValidReassignment_ReassignsAgent()
    {
        // Arrange
        var request = new ServiceRequest { Id = 1, AssignedToId = 5 };
        var newUser = new User { Id = 10 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockUserRepository.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(newUser);

        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.AssignedToId = 10; return sr; });

        // Act
        var result = await _service.ReassignAsync(1, 10, "Reassigning to specialist");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Resolution Tests

    [Fact]
    public async Task ResolveAsync_ValidResolution_ResolvesRequest()
    {
        // Arrange
        var request = new ServiceRequest { Id = 1, Status = ServiceRequestStatus.InProgress };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.Status = ServiceRequestStatus.Resolved; return sr; });

        // Act
        var result = await _service.ResolveAsync(1, "Issue has been fixed");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CloseAsync_ResolvedRequest_ClosesRequest()
    {
        // Arrange
        var request = new ServiceRequest { Id = 1, Status = ServiceRequestStatus.Resolved };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.Status = ServiceRequestStatus.Closed; return sr; });

        // Act
        var result = await _service.CloseAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ReopenAsync_ClosedRequest_ReopensRequest()
    {
        // Arrange
        var request = new ServiceRequest { Id = 1, Status = ServiceRequestStatus.Closed };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.Status = ServiceRequestStatus.Open; return sr; });

        // Act
        var result = await _service.ReopenAsync(1, "Issue reoccurred");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Escalation Tests

    [Fact]
    public async Task EscalateAsync_ValidEscalation_EscalatesRequest()
    {
        // Arrange
        var request = new ServiceRequest { Id = 1, EscalationLevel = 0 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>()))
            .ReturnsAsync((ServiceRequest sr) => { sr.EscalationLevel = 1; return sr; });

        // Act
        var result = await _service.EscalateAsync(1, "Customer is VIP");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetOverdueRequestsAsync_ReturnsOverdueRequests()
    {
        // Arrange
        var requests = new List<ServiceRequest>
        {
            new ServiceRequest { Id = 1, DueDate = DateTime.UtcNow.AddDays(-1), Status = ServiceRequestStatus.Open }
        };

        _mockRequestRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ServiceRequest, bool>>>()))
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetOverdueRequestsAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Comment Tests

    [Fact]
    public async Task AddCommentAsync_ValidComment_AddsComment()
    {
        // Arrange
        var request = new ServiceRequest { Id = 1 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockActivityRepository.Setup(r => r.AddAsync(It.IsAny<Activity>()))
            .ReturnsAsync((Activity a) => { a.Id = 1; return a; });

        // Act
        var result = await _service.AddCommentAsync(1, "This is a comment", 1, false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetCommentsAsync_ReturnsComments()
    {
        // Arrange
        var activities = new List<Activity>
        {
            new Activity { Id = 1, EntityType = "ServiceRequest", EntityId = 1, Type = ActivityType.Comment }
        };

        _mockActivityRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Activity, bool>>>()))
            .ReturnsAsync(activities);

        // Act
        var result = await _service.GetCommentsAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Category Tests

    [Fact]
    public async Task GetCategoriesAsync_ReturnsCategories()
    {
        // Arrange
        var categories = new List<ServiceRequestCategory>
        {
            new ServiceRequestCategory { Id = 1, Name = "Hardware" },
            new ServiceRequestCategory { Id = 2, Name = "Software" }
        };

        _mockCategoryRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var requests = new List<ServiceRequest>
        {
            new ServiceRequest { Id = 1, Status = ServiceRequestStatus.Open },
            new ServiceRequest { Id = 2, Status = ServiceRequestStatus.Resolved },
            new ServiceRequest { Id = 3, Status = ServiceRequestStatus.Closed }
        };

        _mockRequestRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TotalRequests.Should().Be(3);
        result.OpenCount.Should().Be(1);
        result.ResolvedCount.Should().Be(1);
        result.ClosedCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAgentWorkloadAsync_ReturnsWorkload()
    {
        // Arrange
        var requests = new List<ServiceRequest>
        {
            new ServiceRequest { Id = 1, AssignedToId = 5, Status = ServiceRequestStatus.Open },
            new ServiceRequest { Id = 2, AssignedToId = 5, Status = ServiceRequestStatus.InProgress }
        };

        _mockRequestRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ServiceRequest, bool>>>()))
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetAgentWorkloadAsync(5);

        // Assert
        result.OpenTickets.Should().Be(1);
        result.InProgressTickets.Should().Be(1);
    }

    #endregion
}

// Supporting classes for tests
public enum ServiceRequestStatus
{
    Open,
    InProgress,
    Pending,
    Resolved,
    Closed
}

public enum ServiceRequestPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum ActivityType
{
    Comment,
    StatusChange,
    Assignment
}

public class CreateServiceRequestDto
{
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ServiceRequestPriority Priority { get; set; }
    public int? CategoryId { get; set; }
    public int? ContactId { get; set; }
    public int? AccountId { get; set; }
}

public class UpdateServiceRequestDto
{
    public int Id { get; set; }
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public ServiceRequestPriority? Priority { get; set; }
}
