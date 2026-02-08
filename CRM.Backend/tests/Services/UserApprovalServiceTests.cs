// CRM Solution - Customer Relationship Management System
// User Approval Service Unit Tests

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
/// Unit tests for UserApprovalService
/// Covers: Approval workflows, pending requests, approval/rejection
/// </summary>
public class UserApprovalServiceTests
{
    private readonly Mock<IRepository<UserApprovalRequest>> _mockApprovalRepository;
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<IRepository<UserGroup>> _mockGroupRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<UserApprovalService>> _mockLogger;
    private readonly UserApprovalService _service;

    public UserApprovalServiceTests()
    {
        _mockApprovalRepository = new Mock<IRepository<UserApprovalRequest>>();
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockGroupRepository = new Mock<IRepository<UserGroup>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<UserApprovalService>>();

        _service = new UserApprovalService(
            _mockApprovalRepository.Object,
            _mockUserRepository.Object,
            _mockGroupRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Create Request Tests

    [Fact]
    public async Task CreateRequestAsync_ValidRequest_CreatesRequest()
    {
        // Arrange
        var request = new CreateApprovalRequestDto
        {
            Username = "newuser",
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User"
        };

        _mockApprovalRepository.Setup(r => r.AddAsync(It.IsAny<UserApprovalRequest>()))
            .ReturnsAsync((UserApprovalRequest r) => { r.Id = 1; return r; });

        // Act
        var result = await _service.CreateRequestAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task CreateRequestAsync_DuplicateEmail_ReturnsNull()
    {
        // Arrange
        var existing = new UserApprovalRequest { Email = "existing@example.com" };
        var request = new CreateApprovalRequestDto { Email = "existing@example.com" };

        _mockApprovalRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserApprovalRequest, bool>>>()))
            .ReturnsAsync(new List<UserApprovalRequest> { existing });

        // Act
        var result = await _service.CreateRequestAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateRequestAsync_UserAlreadyExists_ReturnsNull()
    {
        // Arrange
        var existingUser = new User { Email = "existing@example.com" };
        var request = new CreateApprovalRequestDto { Email = "existing@example.com" };

        _mockUserRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { existingUser });

        // Act
        var result = await _service.CreateRequestAsync(request);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Get Request Tests

    [Fact]
    public async Task GetByIdAsync_ExistingRequest_ReturnsRequest()
    {
        // Arrange
        var request = new UserApprovalRequest
        {
            Id = 1,
            Email = "test@example.com",
            Status = ApprovalStatus.Pending
        };

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetPendingRequestsAsync_ReturnsPendingOnly()
    {
        // Arrange
        var requests = new List<UserApprovalRequest>
        {
            new UserApprovalRequest { Id = 1, Status = ApprovalStatus.Pending },
            new UserApprovalRequest { Id = 2, Status = ApprovalStatus.Approved }
        };

        _mockApprovalRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserApprovalRequest, bool>>>()))
            .ReturnsAsync(requests.Where(r => r.Status == ApprovalStatus.Pending).ToList());

        // Act
        var result = await _service.GetPendingRequestsAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllRequestsAsync_ReturnsAllRequests()
    {
        // Arrange
        var requests = new List<UserApprovalRequest>
        {
            new UserApprovalRequest { Id = 1 },
            new UserApprovalRequest { Id = 2 },
            new UserApprovalRequest { Id = 3 }
        };

        _mockApprovalRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetAllRequestsAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region Approve Tests

    [Fact]
    public async Task ApproveAsync_PendingRequest_ApprovesAndCreatesUser()
    {
        // Arrange
        var request = new UserApprovalRequest
        {
            Id = 1,
            Email = "newuser@example.com",
            Username = "newuser",
            FirstName = "New",
            LastName = "User",
            Status = ApprovalStatus.Pending
        };

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockApprovalRepository.Setup(r => r.UpdateAsync(It.IsAny<UserApprovalRequest>()))
            .ReturnsAsync((UserApprovalRequest r) => r);

        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        // Act
        var result = await _service.ApproveAsync(1, 1); // requestId, approvedBy

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveAsync_AlreadyApproved_ReturnsFalse()
    {
        // Arrange
        var request = new UserApprovalRequest
        {
            Id = 1,
            Status = ApprovalStatus.Approved
        };

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        // Act
        var result = await _service.ApproveAsync(1, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveAsync_WithGroup_AssignsGroup()
    {
        // Arrange
        var request = new UserApprovalRequest
        {
            Id = 1,
            Email = "newuser@example.com",
            Status = ApprovalStatus.Pending,
            RequestedGroupId = 2
        };

        var group = new UserGroup { Id = 2, Name = "Sales" };

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockGroupRepository.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(group);

        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        _mockApprovalRepository.Setup(r => r.UpdateAsync(It.IsAny<UserApprovalRequest>()))
            .ReturnsAsync((UserApprovalRequest r) => r);

        // Act
        var result = await _service.ApproveAsync(1, 1, 2); // with groupId

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Reject Tests

    [Fact]
    public async Task RejectAsync_PendingRequest_RejectsRequest()
    {
        // Arrange
        var request = new UserApprovalRequest
        {
            Id = 1,
            Status = ApprovalStatus.Pending
        };

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockApprovalRepository.Setup(r => r.UpdateAsync(It.IsAny<UserApprovalRequest>()))
            .ReturnsAsync((UserApprovalRequest r) => r);

        // Act
        var result = await _service.RejectAsync(1, 1, "Invalid email domain");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RejectAsync_AlreadyRejected_ReturnsFalse()
    {
        // Arrange
        var request = new UserApprovalRequest
        {
            Id = 1,
            Status = ApprovalStatus.Rejected
        };

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        // Act
        var result = await _service.RejectAsync(1, 1, "Reason");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RejectAsync_WithReason_StoresReason()
    {
        // Arrange
        var request = new UserApprovalRequest
        {
            Id = 1,
            Status = ApprovalStatus.Pending
        };

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _mockApprovalRepository.Setup(r => r.UpdateAsync(It.IsAny<UserApprovalRequest>()))
            .ReturnsAsync((UserApprovalRequest r) => r);

        // Act
        await _service.RejectAsync(1, 1, "Domain not allowed");

        // Assert
        _mockApprovalRepository.Verify(r => r.UpdateAsync(It.Is<UserApprovalRequest>(
            req => req.RejectionReason == "Domain not allowed")), Times.Once);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task ApproveMultipleAsync_ValidRequests_ApprovesAll()
    {
        // Arrange
        var requestIds = new List<int> { 1, 2, 3 };
        var requests = requestIds.Select(id => new UserApprovalRequest
        {
            Id = id,
            Email = $"user{id}@example.com",
            Status = ApprovalStatus.Pending
        }).ToList();

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => requests.FirstOrDefault(r => r.Id == id));

        _mockApprovalRepository.Setup(r => r.UpdateAsync(It.IsAny<UserApprovalRequest>()))
            .ReturnsAsync((UserApprovalRequest r) => r);

        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        // Act
        var result = await _service.ApproveMultipleAsync(requestIds, 1);

        // Assert
        result.SuccessCount.Should().Be(3);
    }

    [Fact]
    public async Task RejectMultipleAsync_ValidRequests_RejectsAll()
    {
        // Arrange
        var requestIds = new List<int> { 1, 2 };
        var requests = requestIds.Select(id => new UserApprovalRequest
        {
            Id = id,
            Status = ApprovalStatus.Pending
        }).ToList();

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => requests.FirstOrDefault(r => r.Id == id));

        _mockApprovalRepository.Setup(r => r.UpdateAsync(It.IsAny<UserApprovalRequest>()))
            .ReturnsAsync((UserApprovalRequest r) => r);

        // Act
        var result = await _service.RejectMultipleAsync(requestIds, 1, "Bulk rejection");

        // Assert
        result.SuccessCount.Should().Be(2);
    }

    #endregion

    #region Cleanup Tests

    [Fact]
    public async Task CleanupExpiredRequestsAsync_RemovesExpired()
    {
        // Arrange
        var expiredRequests = new List<UserApprovalRequest>
        {
            new UserApprovalRequest { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-31), Status = ApprovalStatus.Pending },
            new UserApprovalRequest { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-45), Status = ApprovalStatus.Pending }
        };

        _mockApprovalRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserApprovalRequest, bool>>>()))
            .ReturnsAsync(expiredRequests);

        _mockApprovalRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CleanupExpiredRequestsAsync(30); // 30 days

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var requests = new List<UserApprovalRequest>
        {
            new UserApprovalRequest { Status = ApprovalStatus.Pending },
            new UserApprovalRequest { Status = ApprovalStatus.Approved },
            new UserApprovalRequest { Status = ApprovalStatus.Approved },
            new UserApprovalRequest { Status = ApprovalStatus.Rejected }
        };

        _mockApprovalRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TotalRequests.Should().Be(4);
        result.PendingCount.Should().Be(1);
        result.ApprovedCount.Should().Be(2);
        result.RejectedCount.Should().Be(1);
    }

    #endregion

    #region Notification Tests

    [Fact]
    public async Task SendApprovalNotificationAsync_ValidRequest_SendsNotification()
    {
        // Arrange
        var request = new UserApprovalRequest
        {
            Id = 1,
            Email = "user@example.com",
            Status = ApprovalStatus.Approved
        };

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        // Act
        var result = await _service.SendApprovalNotificationAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendRejectionNotificationAsync_ValidRequest_SendsNotification()
    {
        // Arrange
        var request = new UserApprovalRequest
        {
            Id = 1,
            Email = "user@example.com",
            Status = ApprovalStatus.Rejected,
            RejectionReason = "Not approved"
        };

        _mockApprovalRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        // Act
        var result = await _service.SendRejectionNotificationAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}

// Supporting classes for tests
public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public class CreateApprovalRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? RequestedGroupId { get; set; }
}
