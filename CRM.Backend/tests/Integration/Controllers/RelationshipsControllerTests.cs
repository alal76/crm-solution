// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class RelationshipsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public RelationshipsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Relationships_Succeeds()
        {
            var create = new
            {
                TypeName = "Test",
                TypeCategory = "Test",
                Description = "Test",
                IsBidirectional = true,
                ReverseTypeName = "Test",
                Icon = "Test",
                Color = "Test",
                IsActive = true,
                IsSystem = true,
                DisplayOrder = 1,
                SourceAccountId = 1,
                TargetAccountId = 1,
                RelationshipTypeId = 1,
                SourceAccountName = "Test",
                TargetAccountName = "Test",
                RelationshipTypeName = "Test",
                RelationshipTypeCategory = "Test",
                RelationshipTypeColor = "Test",
                RelationshipTypeIcon = "Test",
                Status = "Test",
                StrengthScore = 1,
                StrategicImportance = "Test",
                RelationshipStartDate = DateTime.UtcNow,
                RelationshipEndDate = DateTime.UtcNow,
                LastReviewedDate = DateTime.UtcNow,
                NextReviewDate = DateTime.UtcNow,
                AnnualRevenueImpact = 1,
                CostSavings = 1,
                Notes = "Test",
                TermsConditions = "Test",
                InteractionCount = 1,
                LastInteractionDate = DateTime.UtcNow,
                CreatedBy = 1,
                CreatedByName = "Test",
                AccountRelationshipId = 1,
                InteractionType = "Test",
                Subject = "Test",
                InteractionDate = DateTime.UtcNow,
                DurationMinutes = 1,
                Outcome = "Test",
                ActionItems = "Test",
                NextSteps = "Test",
                FollowUpDate = DateTime.UtcNow,
                SentimentScore = 1,
                HealthImpact = "Test",
                Location = "Test",
                MeetingLink = "Test",
                AccountId = 1,
                AccountName = "Test",
                SnapshotDate = DateTime.UtcNow,
                OverallHealthScore = 1,
                EngagementScore = 1,
                ProductAdoptionScore = 1,
                SupportSatisfactionScore = 1,
                FinancialHealthScore = 1,
                RelationshipScore = 1,
                ActiveUsersCount = 1,
                FeatureAdoptionRate = 1,
                SupportTicketsCount = 1,
                SupportTicketsResolved = 1,
                AverageResponseTimeHours = 1,
                NPSScore = 1,
                AnalystNotes = "Test",
                PreviousHealthScore = 1,
                HealthTrend = "Test",
                MapName = "Test",
                CentralAccountId = 1,
                CentralAccountName = "Test",
                RelationshipDepth = 1,
                MinRelationshipStrength = 1,
                DateRangeStart = DateTime.UtcNow,
                DateRangeEnd = DateTime.UtcNow,
                IsPublic = true,
                Label = "Test",
                Type = "Test",
                Industry = "Test",
                HealthScore = 1,
                RiskLevel = "Test",
                LifetimeValue = 1,
                RelationshipCount = 1,
                IsCentral = true,
                SourceId = 1,
                TargetId = 1,
                RelationshipType = "Test",
                TerritoryName = "Test",
                TerritoryCode = "Test",
                RevenueRangeMin = 1,
                RevenueRangeMax = 1,
                PrimaryOwnerId = 1,
                PrimaryOwnerName = "Test",
                AnnualQuota = 1,
                QuotaCurrency = "Test",
                TargetAccountCount = 1,
                AssignedAccountCount = 1
            };
            var cRes = await _client.PostAsJsonAsync("/api/relationships", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/relationships/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                TypeName = "Test2",
                TypeCategory = "Test",
                Description = "Test",
                IsBidirectional = true,
                ReverseTypeName = "Test",
                Icon = "Test",
                Color = "Test",
                IsActive = true,
                IsSystem = true,
                DisplayOrder = 1,
                SourceAccountId = 1,
                TargetAccountId = 1,
                RelationshipTypeId = 1,
                SourceAccountName = "Test",
                TargetAccountName = "Test",
                RelationshipTypeName = "Test",
                RelationshipTypeCategory = "Test",
                RelationshipTypeColor = "Test",
                RelationshipTypeIcon = "Test",
                Status = "Test",
                StrengthScore = 1,
                StrategicImportance = "Test",
                RelationshipStartDate = DateTime.UtcNow,
                RelationshipEndDate = DateTime.UtcNow,
                LastReviewedDate = DateTime.UtcNow,
                NextReviewDate = DateTime.UtcNow,
                AnnualRevenueImpact = 1,
                CostSavings = 1,
                Notes = "Test",
                TermsConditions = "Test",
                InteractionCount = 1,
                LastInteractionDate = DateTime.UtcNow,
                CreatedBy = 1,
                CreatedByName = "Test",
                AccountRelationshipId = 1,
                InteractionType = "Test",
                Subject = "Test",
                InteractionDate = DateTime.UtcNow,
                DurationMinutes = 1,
                Outcome = "Test",
                ActionItems = "Test",
                NextSteps = "Test",
                FollowUpDate = DateTime.UtcNow,
                SentimentScore = 1,
                HealthImpact = "Test",
                Location = "Test",
                MeetingLink = "Test",
                AccountId = 1,
                AccountName = "Test",
                SnapshotDate = DateTime.UtcNow,
                OverallHealthScore = 1,
                EngagementScore = 1,
                ProductAdoptionScore = 1,
                SupportSatisfactionScore = 1,
                FinancialHealthScore = 1,
                RelationshipScore = 1,
                ActiveUsersCount = 1,
                FeatureAdoptionRate = 1,
                SupportTicketsCount = 1,
                SupportTicketsResolved = 1,
                AverageResponseTimeHours = 1,
                NPSScore = 1,
                AnalystNotes = "Test",
                PreviousHealthScore = 1,
                HealthTrend = "Test",
                MapName = "Test",
                CentralAccountId = 1,
                CentralAccountName = "Test",
                RelationshipDepth = 1,
                MinRelationshipStrength = 1,
                DateRangeStart = DateTime.UtcNow,
                DateRangeEnd = DateTime.UtcNow,
                IsPublic = true,
                Label = "Test",
                Type = "Test",
                Industry = "Test",
                HealthScore = 1,
                RiskLevel = "Test",
                LifetimeValue = 1,
                RelationshipCount = 1,
                IsCentral = true,
                SourceId = 1,
                TargetId = 1,
                RelationshipType = "Test",
                TerritoryName = "Test",
                TerritoryCode = "Test",
                RevenueRangeMin = 1,
                RevenueRangeMax = 1,
                PrimaryOwnerId = 1,
                PrimaryOwnerName = "Test",
                AnnualQuota = 1,
                QuotaCurrency = "Test",
                TargetAccountCount = 1,
                AssignedAccountCount = 1
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/relationships/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/relationships/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/relationships/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/relationships/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
