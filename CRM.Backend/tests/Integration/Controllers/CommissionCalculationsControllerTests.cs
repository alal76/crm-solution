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
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/commissioncalculations/{id}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/commissioncalculations/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/commissioncalculations/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/commissioncalculations/{id}");
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
