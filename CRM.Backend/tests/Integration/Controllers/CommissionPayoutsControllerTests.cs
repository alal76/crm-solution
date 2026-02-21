using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
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
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.PayoutNumber.Should().Be(create.PayoutNumber);
            item.UserId.Should().Be(create.UserId);
            item.UserName.Should().Be(create.UserName);
            item.CommissionPlanId.Should().Be(create.CommissionPlanId);
            item.TotalCommissionAmount.Should().Be(create.TotalCommissionAmount);
            item.CommissionCount.Should().Be(create.CommissionCount);
            item.PeriodStartDate.Should().Be(create.PeriodStartDate);
            item.PeriodEndDate.Should().Be(create.PeriodEndDate);
            item.ScheduledPayoutDate.Should().Be(create.ScheduledPayoutDate);
            item.ActualPayoutDate.Should().Be(create.ActualPayoutDate);
            item.PayoutMethod.Should().Be(create.PayoutMethod);
            item.Status.Should().Be(create.Status);
            item.TotalDeductions.Should().Be(create.TotalDeductions);
            item.NetPayoutAmount.Should().Be(create.NetPayoutAmount);
            item.PaymentReferenceId.Should().Be(create.PaymentReferenceId);
            item.Notes.Should().Be(create.Notes);
            item.ApprovedById.Should().Be(create.ApprovedById);
            item.ApprovedByName.Should().Be(create.ApprovedByName);
            item.ApprovedAt.Should().Be(create.ApprovedAt);
            item.CommissionId.Should().Be(create.CommissionId);
            item.CommissionNumber.Should().Be(create.CommissionNumber);
            item.CommissionAmount.Should().Be(create.CommissionAmount);
            item.OpportunityId.Should().Be(create.OpportunityId);
            item.OpportunityName.Should().Be(create.OpportunityName);
            item.InvoiceId.Should().Be(create.InvoiceId);
            item.UserId.Should().Be(create.UserId);
            item.CommissionPlanId.Should().Be(create.CommissionPlanId);
            item.PeriodStartDate.Should().Be(create.PeriodStartDate);
            item.PeriodEndDate.Should().Be(create.PeriodEndDate);
            item.ScheduledPayoutDate.Should().Be(create.ScheduledPayoutDate);
            item.PayoutMethod.Should().Be(create.PayoutMethod);
            item.Notes.Should().Be(create.Notes);
            item.ScheduledPayoutDate.Should().Be(create.ScheduledPayoutDate);
            item.PayoutMethod.Should().Be(create.PayoutMethod);
            item.Status.Should().Be(create.Status);
            item.Notes.Should().Be(create.Notes);
            item.PayoutNumber.Should().Be(create.PayoutNumber);
            item.UserName.Should().Be(create.UserName);
            item.TotalCommissionAmount.Should().Be(create.TotalCommissionAmount);
            item.NetPayoutAmount.Should().Be(create.NetPayoutAmount);
            item.Status.Should().Be(create.Status);
            item.ScheduledPayoutDate.Should().Be(create.ScheduledPayoutDate);
            item.ActualPayoutDate.Should().Be(create.ActualPayoutDate);
            item.CommissionCount.Should().Be(create.CommissionCount);

            var getRes = await _client.GetAsync($"/api/commissionpayouts/{{item.Id}}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/commissionpayouts/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/commissionpayouts/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/commissionpayouts/{{item.Id}}");
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

