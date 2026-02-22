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
    public class CommissionPayoutsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public CommissionPayoutsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_CommissionPayouts_Succeeds()
        {
            var create = new
            {
                PayoutNumber = "Test",
                UserId = 1,
                UserName = "Test",
                CommissionPlanId = 1,
                TotalCommissionAmount = 1,
                CommissionCount = 1,
                PeriodStartDate = DateTime.UtcNow,
                PeriodEndDate = DateTime.UtcNow,
                ScheduledPayoutDate = DateTime.UtcNow,
                ActualPayoutDate = DateTime.UtcNow,
                PayoutMethod = "Test",
                Status = "Test",
                TotalDeductions = 1,
                NetPayoutAmount = 1,
                PaymentReferenceId = "Test",
                Notes = "Test",
                ApprovedById = 1,
                ApprovedByName = "Test",
                ApprovedAt = DateTime.UtcNow,
                CommissionId = 1,
                CommissionNumber = "Test",
                CommissionAmount = 1,
                OpportunityId = 1,
                OpportunityName = "Test",
                InvoiceId = 1
            };
            var cRes = await _client.PostAsJsonAsync("/api/commissionpayouts", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/commissionpayouts/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                PayoutNumber = "Test2",
                UserId = 1,
                UserName = "Test",
                CommissionPlanId = 1,
                TotalCommissionAmount = 1,
                CommissionCount = 1,
                PeriodStartDate = DateTime.UtcNow,
                PeriodEndDate = DateTime.UtcNow,
                ScheduledPayoutDate = DateTime.UtcNow,
                ActualPayoutDate = DateTime.UtcNow,
                PayoutMethod = "Test",
                Status = "Test",
                TotalDeductions = 1,
                NetPayoutAmount = 1,
                PaymentReferenceId = "Test",
                Notes = "Test",
                ApprovedById = 1,
                ApprovedByName = "Test",
                ApprovedAt = DateTime.UtcNow,
                CommissionId = 1,
                CommissionNumber = "Test",
                CommissionAmount = 1,
                OpportunityId = 1,
                OpportunityName = "Test",
                InvoiceId = 1
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/commissionpayouts/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/commissionpayouts/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/commissionpayouts/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/commissionpayouts/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
