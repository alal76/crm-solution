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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class PipelineServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<PipelineService>> _mockLogger;
    private readonly PipelineService _service;

    public PipelineServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<PipelineService>>();
        _service = new PipelineService(_mockContext.Object, _mockLogger.Object);
    }

    private void SetupOpportunities(List<Opportunity>? opportunities = null)
    {
        opportunities ??= new List<Opportunity>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(opportunities);
        _mockContext.Setup(c => c.Opportunities).Returns(mockSet.Object);
    }

    private static Opportunity CreateOpportunity(int id, OpportunityStage stage, decimal amount, bool isDeleted = false)
    {
        return new Opportunity
        {
            Id = id,
            Name = $"Opportunity {id}",
            Stage = stage,
            Amount = amount,
            AccountId = 1,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = isDeleted
        };
    }

    [Fact]
    public async Task GetPipelinesAsync_ShouldReturnDefaultPipeline()
    {
        var result = (await _service.GetPipelinesAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].IsDefault.Should().BeTrue();
        result[0].Stages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDefaultPipeline_WhenIdMatches()
    {
        var pipelines = await _service.GetPipelinesAsync();
        var defaultPipeline = pipelines.First();

        var result = await _service.GetByIdAsync(defaultPipeline.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(defaultPipeline.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotMatch()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStatsAsync_ShouldAggregateOpportunitiesAndExcludeDeleted()
    {
        var opportunities = new List<Opportunity>
        {
            CreateOpportunity(1, OpportunityStage.Qualification, 1000m),
            CreateOpportunity(2, OpportunityStage.Proposal, 2000m),
            CreateOpportunity(3, OpportunityStage.Qualification, 500m, isDeleted: true)
        };
        SetupOpportunities(opportunities);

        var pipelines = await _service.GetPipelinesAsync();
        var pipelineId = pipelines.First().Id;

        var stats = await _service.GetStatsAsync(pipelineId);

        stats.TotalOpportunities.Should().Be(2);
        stats.TotalValue.Should().Be(3000m);
        stats.Stats.Should().HaveCount(9);
        stats.Stats.Should().Contain(s => s.Stage == "Qualification" && s.Count == 1);
    }

    [Fact]
    public void GetDefaultStages_ShouldReturnDefaultStages()
    {
        var stages = _service.GetDefaultStages().ToList();

        stages.Should().NotBeEmpty();
        stages.Should().Contain(s => s.Key == "Qualification");
    }
}
