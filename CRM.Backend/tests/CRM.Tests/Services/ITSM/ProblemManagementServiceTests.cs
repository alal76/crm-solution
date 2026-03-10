// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Exceptions;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class ProblemManagementServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<ProblemManagementService>> _mockLogger;
    private readonly ProblemManagementService _service;

    public ProblemManagementServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"ProblemManagementTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<ProblemManagementService>>();
        _service = new ProblemManagementService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateProblemAsync_ShouldThrow_WhenShortDescriptionIsEmpty()
    {
        var dto = new CreateProblemDto
        {
            ShortDescription = "",
            Priority = ProblemPriority.Medium
        };

        var act = async () => await _service.CreateProblemAsync(dto, createdById: 1, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateProblemAsync_ShouldAddProblemToDatabase_WhenValid()
    {
        var dto = new CreateProblemDto
        {
            ShortDescription = "Application crashes on login",
            Description = "Users report crash when accessing the login screen",
            Priority = ProblemPriority.High
        };

        var result = await _service.CreateProblemAsync(dto, createdById: 1, CancellationToken.None);

        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Application crashes on login");
        _context.Problems.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateProblemAsync_ShouldGenerateProblemNumber()
    {
        var dto = new CreateProblemDto
        {
            ShortDescription = "Numbered Problem",
            Priority = ProblemPriority.Low
        };

        var result = await _service.CreateProblemAsync(dto, createdById: 1, CancellationToken.None);

        result.Number.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetProblemByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Service throws EntityNotFoundException rather than returning null
        await Assert.ThrowsAsync<CRM.Core.Exceptions.EntityNotFoundException>(
            () => _service.GetProblemByIdAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task GetProblemByIdAsync_ShouldReturnProblem_WhenExists()
    {
        var problem = new Problem
        {
            ProblemId = 5,
            Number = "PRB0000001",
            ShortDescription = "Existing Problem",
            State = ProblemState.New,
            Priority = ProblemPriority.Medium,
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        var result = await _service.GetProblemByIdAsync(5, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ShortDescription.Should().Be("Existing Problem");
    }

    [Fact]
    public async Task ListProblemsAsync_ShouldReturnAll_WhenNoFilter()
    {
        var problems = new[]
        {
            new Problem { ProblemId = 1, Number = "PRB001", ShortDescription = "Prob 1", State = ProblemState.New, Priority = ProblemPriority.Critical, CreatedById = 1, CreatedAt = DateTime.UtcNow },
            new Problem { ProblemId = 2, Number = "PRB002", ShortDescription = "Prob 2", State = ProblemState.Investigating, Priority = ProblemPriority.High, CreatedById = 1, CreatedAt = DateTime.UtcNow },
        };
        _context.Problems.AddRange(problems);
        await _context.SaveChangesAsync();

        var result = await _service.ListProblemsAsync(new ProblemFilterDto(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ResolveProblemAsync_ShouldSetStateToResolved()
    {
        var problem = new Problem
        {
            ProblemId = 10,
            Number = "PRB0000010",
            ShortDescription = "Resolve Me",
            State = ProblemState.Investigating,
            Priority = ProblemPriority.Medium,
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        var dto = new ResolveProblemDto
        {
            RootCause = "Found root cause",
            Solution = "Applied the fix",
            Workaround = null
        };

        var result = await _service.ResolveProblemAsync(10, dto, resolvedById: 1, CancellationToken.None);

        result.Should().NotBeNull();
        result!.State.Should().Be(ProblemState.Resolved);
    }

    [Fact]
    public async Task CreateProblemAsync_ShouldThrow_WhenShortDescriptionIsWhitespace()
    {
        var dto = new CreateProblemDto
        {
            ShortDescription = "   ",
            Priority = ProblemPriority.Medium
        };

        var act = async () => await _service.CreateProblemAsync(dto, createdById: 1, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
