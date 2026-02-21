using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class CommissionCalculationsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public CommissionCalculationsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_CommissionCalculations_Succeeds()
        {
            var create = new
            {
                RuleId = 1,
                RuleName = "Test",
                DealAmount = 1,
                Commission = 1,
                Tier = "Test",
                CommissionRate = 1,
                AppliedCap = 1,
                ClawbackAmount = 1,
                NetCommission = 1,
                UserId = 1,
                UserName = "Test",
                OpportunityId = 1,
                OrderId = 1,
                InvoiceId = 1,
                Status = "Test",
                Notes = "Test",
                CalculatedAt = DateTime.UtcNow,
                AdjustmentAmount = 1,
                DealName = "Test",
                CommissionTier = "Test",
                OrderNumber = "Test",
                OrderAmount = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                TotalDealAmount = 1,
                TotalCommission = 1,
                DealCount = 1,
                IsValid = true,
                CalculatedCommission = 1,
                ValidationMessage = "Test",
                CommissionCalculationId = 1,
                Reason = "Test",
                CreatedById = 1,
                TotalRecords = 1,
                TotalAmount = 1,
                ReconciliationDate = DateTime.UtcNow
            };
            var cRes = await _client.PostAsJsonAsync("/api/commissioncalculations", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.RuleId.Should().Be(create.RuleId);
            item.RuleName.Should().Be(create.RuleName);
            item.DealAmount.Should().Be(create.DealAmount);
            item.Commission.Should().Be(create.Commission);
            item.Tier.Should().Be(create.Tier);
            item.CommissionRate.Should().Be(create.CommissionRate);
            item.AppliedCap.Should().Be(create.AppliedCap);
            item.ClawbackAmount.Should().Be(create.ClawbackAmount);
            item.NetCommission.Should().Be(create.NetCommission);
            item.UserId.Should().Be(create.UserId);
            item.UserName.Should().Be(create.UserName);
            item.OpportunityId.Should().Be(create.OpportunityId);
            item.OrderId.Should().Be(create.OrderId);
            item.InvoiceId.Should().Be(create.InvoiceId);
            item.Status.Should().Be(create.Status);
            item.Notes.Should().Be(create.Notes);
            item.CalculatedAt.Should().Be(create.CalculatedAt);
            item.RuleId.Should().Be(create.RuleId);
            item.DealAmount.Should().Be(create.DealAmount);
            item.UserId.Should().Be(create.UserId);
            item.OpportunityId.Should().Be(create.OpportunityId);
            item.OrderId.Should().Be(create.OrderId);
            item.InvoiceId.Should().Be(create.InvoiceId);
            item.Notes.Should().Be(create.Notes);
            item.DealAmount.Should().Be(create.DealAmount);
            item.AdjustmentAmount.Should().Be(create.AdjustmentAmount);
            item.Status.Should().Be(create.Status);
            item.Notes.Should().Be(create.Notes);
            item.RuleName.Should().Be(create.RuleName);
            item.DealAmount.Should().Be(create.DealAmount);
            item.Commission.Should().Be(create.Commission);
            item.UserName.Should().Be(create.UserName);
            item.Tier.Should().Be(create.Tier);
            item.Status.Should().Be(create.Status);
            item.OpportunityId.Should().Be(create.OpportunityId);
            item.DealName.Should().Be(create.DealName);
            item.DealAmount.Should().Be(create.DealAmount);
            item.Commission.Should().Be(create.Commission);
            item.CommissionTier.Should().Be(create.CommissionTier);
            item.CommissionRate.Should().Be(create.CommissionRate);
            item.UserId.Should().Be(create.UserId);
            item.UserName.Should().Be(create.UserName);
            item.CalculatedAt.Should().Be(create.CalculatedAt);
            item.OrderId.Should().Be(create.OrderId);
            item.OrderNumber.Should().Be(create.OrderNumber);
            item.OrderAmount.Should().Be(create.OrderAmount);
            item.Commission.Should().Be(create.Commission);
            item.UserId.Should().Be(create.UserId);
            item.UserName.Should().Be(create.UserName);
            item.UserId.Should().Be(create.UserId);
            item.StartDate.Should().Be(create.StartDate);
            item.EndDate.Should().Be(create.EndDate);
            item.UserId.Should().Be(create.UserId);
            item.UserName.Should().Be(create.UserName);
            item.StartDate.Should().Be(create.StartDate);
            item.EndDate.Should().Be(create.EndDate);
            item.TotalDealAmount.Should().Be(create.TotalDealAmount);
            item.TotalCommission.Should().Be(create.TotalCommission);
            item.DealCount.Should().Be(create.DealCount);
            item.RuleId.Should().Be(create.RuleId);
            item.DealAmount.Should().Be(create.DealAmount);
            item.UserId.Should().Be(create.UserId);
            item.IsValid.Should().Be(create.IsValid);
            item.CalculatedCommission.Should().Be(create.CalculatedCommission);
            item.ValidationMessage.Should().Be(create.ValidationMessage);
            item.CommissionCalculationId.Should().Be(create.CommissionCalculationId);
            item.ClawbackAmount.Should().Be(create.ClawbackAmount);
            item.Reason.Should().Be(create.Reason);
            item.CreatedById.Should().Be(create.CreatedById);
            item.StartDate.Should().Be(create.StartDate);
            item.EndDate.Should().Be(create.EndDate);
            item.TotalRecords.Should().Be(create.TotalRecords);
            item.TotalAmount.Should().Be(create.TotalAmount);
            item.Status.Should().Be(create.Status);
            item.ReconciliationDate.Should().Be(create.ReconciliationDate);
            item.Notes.Should().Be(create.Notes);

            var getRes = await _client.GetAsync($"/api/commissioncalculations/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                RuleId = 1,
                RuleName = "Test2",
                DealAmount = 1,
                Commission = 1,
                Tier = "Test",
                CommissionRate = 1,
                AppliedCap = 1,
                ClawbackAmount = 1,
                NetCommission = 1,
                UserId = 1,
                UserName = "Test",
                OpportunityId = 1,
                OrderId = 1,
                InvoiceId = 1,
                Status = "Test",
                Notes = "Test",
                CalculatedAt = DateTime.UtcNow,
                AdjustmentAmount = 1,
                DealName = "Test",
                CommissionTier = "Test",
                OrderNumber = "Test",
                OrderAmount = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                TotalDealAmount = 1,
                TotalCommission = 1,
                DealCount = 1,
                IsValid = true,
                CalculatedCommission = 1,
                ValidationMessage = "Test",
                CommissionCalculationId = 1,
                Reason = "Test",
                CreatedById = 1,
                TotalRecords = 1,
                TotalAmount = 1,
                ReconciliationDate = DateTime.UtcNow
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/commissioncalculations/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/commissioncalculations/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/commissioncalculations/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/commissioncalculations/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

