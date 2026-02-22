using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
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
            var item = (await cRes.Content.ReadFromJsonAsync<dynamic>())!;

            item.TypeName.Should().Be(create.TypeName);
            item.TypeCategory.Should().Be(create.TypeCategory);
            item.Description.Should().Be(create.Description);
            item.IsBidirectional.Should().Be(create.IsBidirectional);
            item.ReverseTypeName.Should().Be(create.ReverseTypeName);
            item.Icon.Should().Be(create.Icon);
            item.Color.Should().Be(create.Color);
            item.IsActive.Should().Be(create.IsActive);
            item.IsSystem.Should().Be(create.IsSystem);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.TypeName.Should().Be(create.TypeName);
            item.TypeCategory.Should().Be(create.TypeCategory);
            item.Description.Should().Be(create.Description);
            item.IsBidirectional.Should().Be(create.IsBidirectional);
            item.ReverseTypeName.Should().Be(create.ReverseTypeName);
            item.Icon.Should().Be(create.Icon);
            item.Color.Should().Be(create.Color);
            item.IsActive.Should().Be(create.IsActive);
            item.DisplayOrder.Should().Be(create.DisplayOrder);
            item.SourceAccountId.Should().Be(create.SourceAccountId);
            item.TargetAccountId.Should().Be(create.TargetAccountId);
            item.RelationshipTypeId.Should().Be(create.RelationshipTypeId);
            item.SourceAccountName.Should().Be(create.SourceAccountName);
            item.TargetAccountName.Should().Be(create.TargetAccountName);
            item.RelationshipTypeName.Should().Be(create.RelationshipTypeName);
            item.RelationshipTypeCategory.Should().Be(create.RelationshipTypeCategory);
            item.RelationshipTypeColor.Should().Be(create.RelationshipTypeColor);
            item.RelationshipTypeIcon.Should().Be(create.RelationshipTypeIcon);
            item.Status.Should().Be(create.Status);
            item.StrengthScore.Should().Be(create.StrengthScore);
            item.StrategicImportance.Should().Be(create.StrategicImportance);
            item.RelationshipStartDate.Should().Be(create.RelationshipStartDate);
            item.RelationshipEndDate.Should().Be(create.RelationshipEndDate);
            item.LastReviewedDate.Should().Be(create.LastReviewedDate);
            item.NextReviewDate.Should().Be(create.NextReviewDate);
            item.AnnualRevenueImpact.Should().Be(create.AnnualRevenueImpact);
            item.CostSavings.Should().Be(create.CostSavings);
            item.Description.Should().Be(create.Description);
            item.Notes.Should().Be(create.Notes);
            item.TermsConditions.Should().Be(create.TermsConditions);
            item.InteractionCount.Should().Be(create.InteractionCount);
            item.LastInteractionDate.Should().Be(create.LastInteractionDate);
            item.CreatedBy.Should().Be(create.CreatedBy);
            item.CreatedByName.Should().Be(create.CreatedByName);
            item.SourceAccountId.Should().Be(create.SourceAccountId);
            item.TargetAccountId.Should().Be(create.TargetAccountId);
            item.RelationshipTypeId.Should().Be(create.RelationshipTypeId);
            item.Status.Should().Be(create.Status);
            item.StrengthScore.Should().Be(create.StrengthScore);
            item.StrategicImportance.Should().Be(create.StrategicImportance);
            item.RelationshipStartDate.Should().Be(create.RelationshipStartDate);
            item.RelationshipEndDate.Should().Be(create.RelationshipEndDate);
            item.NextReviewDate.Should().Be(create.NextReviewDate);
            item.AnnualRevenueImpact.Should().Be(create.AnnualRevenueImpact);
            item.CostSavings.Should().Be(create.CostSavings);
            item.Description.Should().Be(create.Description);
            item.Notes.Should().Be(create.Notes);
            item.TermsConditions.Should().Be(create.TermsConditions);
            item.AccountRelationshipId.Should().Be(create.AccountRelationshipId);
            item.InteractionType.Should().Be(create.InteractionType);
            item.Subject.Should().Be(create.Subject);
            item.Description.Should().Be(create.Description);
            item.InteractionDate.Should().Be(create.InteractionDate);
            item.DurationMinutes.Should().Be(create.DurationMinutes);
            item.Outcome.Should().Be(create.Outcome);
            item.ActionItems.Should().Be(create.ActionItems);
            item.NextSteps.Should().Be(create.NextSteps);
            item.FollowUpDate.Should().Be(create.FollowUpDate);
            item.SentimentScore.Should().Be(create.SentimentScore);
            item.HealthImpact.Should().Be(create.HealthImpact);
            item.Location.Should().Be(create.Location);
            item.MeetingLink.Should().Be(create.MeetingLink);
            item.CreatedBy.Should().Be(create.CreatedBy);
            item.CreatedByName.Should().Be(create.CreatedByName);
            item.AccountRelationshipId.Should().Be(create.AccountRelationshipId);
            item.InteractionType.Should().Be(create.InteractionType);
            item.Subject.Should().Be(create.Subject);
            item.Description.Should().Be(create.Description);
            item.InteractionDate.Should().Be(create.InteractionDate);
            item.DurationMinutes.Should().Be(create.DurationMinutes);
            item.Outcome.Should().Be(create.Outcome);
            item.ActionItems.Should().Be(create.ActionItems);
            item.NextSteps.Should().Be(create.NextSteps);
            item.FollowUpDate.Should().Be(create.FollowUpDate);
            item.SentimentScore.Should().Be(create.SentimentScore);
            item.HealthImpact.Should().Be(create.HealthImpact);
            item.Location.Should().Be(create.Location);
            item.MeetingLink.Should().Be(create.MeetingLink);
            item.AccountId.Should().Be(create.AccountId);
            item.AccountName.Should().Be(create.AccountName);
            item.SnapshotDate.Should().Be(create.SnapshotDate);
            item.OverallHealthScore.Should().Be(create.OverallHealthScore);
            item.EngagementScore.Should().Be(create.EngagementScore);
            item.ProductAdoptionScore.Should().Be(create.ProductAdoptionScore);
            item.SupportSatisfactionScore.Should().Be(create.SupportSatisfactionScore);
            item.FinancialHealthScore.Should().Be(create.FinancialHealthScore);
            item.RelationshipScore.Should().Be(create.RelationshipScore);
            item.ActiveUsersCount.Should().Be(create.ActiveUsersCount);
            item.FeatureAdoptionRate.Should().Be(create.FeatureAdoptionRate);
            item.SupportTicketsCount.Should().Be(create.SupportTicketsCount);
            item.SupportTicketsResolved.Should().Be(create.SupportTicketsResolved);
            item.AverageResponseTimeHours.Should().Be(create.AverageResponseTimeHours);
            item.NPSScore.Should().Be(create.NPSScore);
            item.AnalystNotes.Should().Be(create.AnalystNotes);
            item.PreviousHealthScore.Should().Be(create.PreviousHealthScore);
            item.HealthTrend.Should().Be(create.HealthTrend);
            item.AccountId.Should().Be(create.AccountId);
            item.SnapshotDate.Should().Be(create.SnapshotDate);
            item.OverallHealthScore.Should().Be(create.OverallHealthScore);
            item.EngagementScore.Should().Be(create.EngagementScore);
            item.ProductAdoptionScore.Should().Be(create.ProductAdoptionScore);
            item.SupportSatisfactionScore.Should().Be(create.SupportSatisfactionScore);
            item.FinancialHealthScore.Should().Be(create.FinancialHealthScore);
            item.RelationshipScore.Should().Be(create.RelationshipScore);
            item.ActiveUsersCount.Should().Be(create.ActiveUsersCount);
            item.FeatureAdoptionRate.Should().Be(create.FeatureAdoptionRate);
            item.SupportTicketsCount.Should().Be(create.SupportTicketsCount);
            item.SupportTicketsResolved.Should().Be(create.SupportTicketsResolved);
            item.AverageResponseTimeHours.Should().Be(create.AverageResponseTimeHours);
            item.NPSScore.Should().Be(create.NPSScore);
            item.AnalystNotes.Should().Be(create.AnalystNotes);
            item.MapName.Should().Be(create.MapName);
            item.Description.Should().Be(create.Description);
            item.CentralAccountId.Should().Be(create.CentralAccountId);
            item.CentralAccountName.Should().Be(create.CentralAccountName);
            item.RelationshipDepth.Should().Be(create.RelationshipDepth);
            item.MinRelationshipStrength.Should().Be(create.MinRelationshipStrength);
            item.DateRangeStart.Should().Be(create.DateRangeStart);
            item.DateRangeEnd.Should().Be(create.DateRangeEnd);
            item.IsPublic.Should().Be(create.IsPublic);
            item.CreatedBy.Should().Be(create.CreatedBy);
            item.CreatedByName.Should().Be(create.CreatedByName);
            item.Label.Should().Be(create.Label);
            item.Type.Should().Be(create.Type);
            item.Industry.Should().Be(create.Industry);
            item.HealthScore.Should().Be(create.HealthScore);
            item.RiskLevel.Should().Be(create.RiskLevel);
            item.LifetimeValue.Should().Be(create.LifetimeValue);
            item.RelationshipCount.Should().Be(create.RelationshipCount);
            item.IsCentral.Should().Be(create.IsCentral);
            item.SourceId.Should().Be(create.SourceId);
            item.TargetId.Should().Be(create.TargetId);
            item.RelationshipType.Should().Be(create.RelationshipType);
            item.Color.Should().Be(create.Color);
            item.StrengthScore.Should().Be(create.StrengthScore);
            item.Status.Should().Be(create.Status);
            item.TerritoryName.Should().Be(create.TerritoryName);
            item.TerritoryCode.Should().Be(create.TerritoryCode);
            item.Description.Should().Be(create.Description);
            item.RevenueRangeMin.Should().Be(create.RevenueRangeMin);
            item.RevenueRangeMax.Should().Be(create.RevenueRangeMax);
            item.PrimaryOwnerId.Should().Be(create.PrimaryOwnerId);
            item.PrimaryOwnerName.Should().Be(create.PrimaryOwnerName);
            item.AnnualQuota.Should().Be(create.AnnualQuota);
            item.QuotaCurrency.Should().Be(create.QuotaCurrency);
            item.TargetAccountCount.Should().Be(create.TargetAccountCount);
            item.AssignedAccountCount.Should().Be(create.AssignedAccountCount);
            item.IsActive.Should().Be(create.IsActive);

            var getRes = await _client.GetAsync($"/api/relationships/{{item.Id}}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/relationships/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/relationships/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/relationships/{{item.Id}}");
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

