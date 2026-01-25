using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for OpportunitiesController
/// </summary>
public class OpportunitiesControllerTests
{
    private readonly Mock<IOpportunityService> _mockOpportunityService;
    private readonly Mock<ILogger<OpportunitiesController>> _mockLogger;
    private readonly OpportunitiesController _controller;

    public OpportunitiesControllerTests()
    {
        _mockOpportunityService = new Mock<IOpportunityService>();
        _mockLogger = new Mock<ILogger<OpportunitiesController>>();
        _controller = new OpportunitiesController(_mockOpportunityService.Object, _mockLogger.Object);
    }

    #region GetOpen Tests

    [Fact]
    public async Task GetOpen_ReturnsOkResult_WithOpportunities()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new() { Id = 1, Name = "Enterprise Deal", Stage = OpportunityStage.ProposalQuote, Amount = 50000m },
            new() { Id = 2, Name = "SMB Sale", Stage = OpportunityStage.Qualification, Amount = 10000m }
        };
        _mockOpportunityService.Setup(s => s.GetOpenOpportunitiesAsync())
            .ReturnsAsync(opportunities);

        // Act
        var result = await _controller.GetOpen();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOpportunities = okResult.Value.Should().BeAssignableTo<IEnumerable<Opportunity>>().Subject;
        returnedOpportunities.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOpen_ReturnsEmptyList_WhenNoOpenOpportunities()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetOpenOpportunitiesAsync())
            .ReturnsAsync(new List<Opportunity>());

        // Act
        var result = await _controller.GetOpen();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOpportunities = okResult.Value.Should().BeAssignableTo<IEnumerable<Opportunity>>().Subject;
        returnedOpportunities.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOpen_Returns500_OnException()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetOpenOpportunitiesAsync())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetOpen();

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ReturnsOkResult_WhenOpportunityExists()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Big Deal",
            Stage = OpportunityStage.NegotiationReview,
            Amount = 100000m,
            Probability = 0.75
        };
        _mockOpportunityService.Setup(s => s.GetOpportunityByIdAsync(1))
            .ReturnsAsync(opportunity);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOpportunity = okResult.Value.Should().BeOfType<Opportunity>().Subject;
        returnedOpportunity.Id.Should().Be(1);
        returnedOpportunity.Name.Should().Be("Big Deal");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOpportunityDoesNotExist()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetOpportunityByIdAsync(999))
            .ReturnsAsync((Opportunity?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_Returns500_OnException()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetOpportunityByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetByCustomerId Tests

    [Fact]
    public async Task GetByCustomerId_ReturnsOkResult_WithOpportunities()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new() { Id = 1, Name = "Opportunity 1", CustomerId = 5 },
            new() { Id = 2, Name = "Opportunity 2", CustomerId = 5 }
        };
        _mockOpportunityService.Setup(s => s.GetOpportunitiesByCustomerAsync(5))
            .ReturnsAsync(opportunities);

        // Act
        var result = await _controller.GetByCustomerId(5);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOpportunities = okResult.Value.Should().BeAssignableTo<IEnumerable<Opportunity>>().Subject;
        returnedOpportunities.Should().HaveCount(2);
        returnedOpportunities.All(o => o.CustomerId == 5).Should().BeTrue();
    }

    [Fact]
    public async Task GetByCustomerId_ReturnsEmptyList_WhenNoOpportunitiesForCustomer()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetOpportunitiesByCustomerAsync(999))
            .ReturnsAsync(new List<Opportunity>());

        // Act
        var result = await _controller.GetByCustomerId(999);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOpportunities = okResult.Value.Should().BeAssignableTo<IEnumerable<Opportunity>>().Subject;
        returnedOpportunities.Should().BeEmpty();
    }

    #endregion

    #region GetTotalPipeline Tests

    [Fact]
    public async Task GetTotalPipeline_ReturnsOkResult_WithTotal()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetTotalPipelineAsync())
            .ReturnsAsync(250000m);

        // Act
        var result = await _controller.GetTotalPipeline();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTotalPipeline_ReturnsZero_WhenNoPipeline()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.GetTotalPipelineAsync())
            .ReturnsAsync(0m);

        // Act
        var result = await _controller.GetTotalPipeline();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ReturnsCreatedResult_WhenSuccessful()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Name = "New Opportunity",
            Stage = OpportunityStage.Prospecting,
            Amount = 25000m,
            CustomerId = 1
        };
        _mockOpportunityService.Setup(s => s.CreateOpportunityAsync(It.IsAny<Opportunity>()))
            .ReturnsAsync(10);

        // Act
        var result = await _controller.Create(opportunity);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(OpportunitiesController.GetById));
        createdResult.RouteValues!["id"].Should().Be(10);
    }

    [Fact]
    public async Task Create_Returns500_OnException()
    {
        // Arrange
        var opportunity = new Opportunity { Name = "Test" };
        _mockOpportunityService.Setup(s => s.CreateOpportunityAsync(It.IsAny<Opportunity>()))
            .ThrowsAsync(new Exception("Creation failed"));

        // Act
        var result = await _controller.Create(opportunity);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Updated Opportunity",
            Stage = OpportunityStage.ClosedWon
        };
        _mockOpportunityService.Setup(s => s.UpdateOpportunityAsync(It.IsAny<Opportunity>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, opportunity);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.DeleteOpportunityAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_Returns500_OnException()
    {
        // Arrange
        _mockOpportunityService.Setup(s => s.DeleteOpportunityAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion
}
