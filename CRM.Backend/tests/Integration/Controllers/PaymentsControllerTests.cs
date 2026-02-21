using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class PaymentsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public PaymentsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Payments_Succeeds()
        {
            var create = new { PaymentNumber = "Test", InvoiceId = 1, InvoiceNumber = "Test", AccountId = 1, AccountName = "Test", Amount = 1, RefundedAmount = 1, AmountApplied = 1, PaymentMethod = null, PaymentType = null, Status = null, PaymentDate = DateTime.UtcNow, ProcessedDate = DateTime.UtcNow, RefundDate = DateTime.UtcNow, ScheduledDate = DateTime.UtcNow, TransactionId = "Test", AuthorizationCode = "Test", CardLast4 = "Test", CardholderName = "Test", BankReference = "Test", IsReconciled = true, ReconciledDate = DateTime.UtcNow, Description = "Test", FailureReason = "Test", RetryCount = 1, OriginalPaymentId = 1, ExternalPaymentId = "Test", GatewayReference = "Test", CheckNumber = "Test", ProcessingFee = 1, NetAmount = 1, ExchangeRate = 1, SettledDate = DateTime.UtcNow, DepositDate = DateTime.UtcNow, CardBrand = "Test", CardExpMonth = 1, CardExpYear = 1, BankName = "Test", AccountLast4 = "Test", AccountType = "Test", Gateway = "Test", GatewayResponseCode = "Test", GatewayResponseMessage = "Test", OrderId = 1, SubscriptionId = 1, InternalNotes = "Test", InvoiceId = 1, AccountId = 1, Amount = 1, PaymentMethod = null, PaymentType = null, Status = null, ScheduledDate = DateTime.UtcNow, Description = "Test", TokenizedCardId = "Test", Status = null, Description = "Test", PaymentDate = DateTime.UtcNow, AccountId = 1, InvoiceId = 1, Status = null, PaymentMethod = null, FromDate = DateTime.UtcNow, ToDate = DateTime.UtcNow, Page = 1, PageSize = 1, SortBy = "Test", SortOrder = "Test", Amount = 1, PaymentMethod = null, TokenizedCardId = "Test", AuthorizationCode = "Test", Description = "Test", RefundAmount = 1, Reason = "Test" };
            var cRes = await _client.PostAsJsonAsync("/api/payments", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.PaymentNumber.Should().Be(create.PaymentNumber);
            item.InvoiceId.Should().Be(create.InvoiceId);
            item.InvoiceNumber.Should().Be(create.InvoiceNumber);
            item.AccountId.Should().Be(create.AccountId);
            item.AccountName.Should().Be(create.AccountName);
            item.Amount.Should().Be(create.Amount);
            item.RefundedAmount.Should().Be(create.RefundedAmount);
            item.AmountApplied.Should().Be(create.AmountApplied);
            item.PaymentMethod.Should().Be(create.PaymentMethod);
            item.PaymentType.Should().Be(create.PaymentType);
            item.Status.Should().Be(create.Status);
            item.PaymentDate.Should().Be(create.PaymentDate);
            item.ProcessedDate.Should().Be(create.ProcessedDate);
            item.RefundDate.Should().Be(create.RefundDate);
            item.ScheduledDate.Should().Be(create.ScheduledDate);
            item.TransactionId.Should().Be(create.TransactionId);
            item.AuthorizationCode.Should().Be(create.AuthorizationCode);
            item.CardLast4.Should().Be(create.CardLast4);
            item.CardholderName.Should().Be(create.CardholderName);
            item.BankReference.Should().Be(create.BankReference);
            item.IsReconciled.Should().Be(create.IsReconciled);
            item.ReconciledDate.Should().Be(create.ReconciledDate);
            item.Description.Should().Be(create.Description);
            item.FailureReason.Should().Be(create.FailureReason);
            item.RetryCount.Should().Be(create.RetryCount);
            item.OriginalPaymentId.Should().Be(create.OriginalPaymentId);
            item.ExternalPaymentId.Should().Be(create.ExternalPaymentId);
            item.GatewayReference.Should().Be(create.GatewayReference);
            item.CheckNumber.Should().Be(create.CheckNumber);
            item.ProcessingFee.Should().Be(create.ProcessingFee);
            item.NetAmount.Should().Be(create.NetAmount);
            item.ExchangeRate.Should().Be(create.ExchangeRate);
            item.SettledDate.Should().Be(create.SettledDate);
            item.DepositDate.Should().Be(create.DepositDate);
            item.CardBrand.Should().Be(create.CardBrand);
            item.CardExpMonth.Should().Be(create.CardExpMonth);
            item.CardExpYear.Should().Be(create.CardExpYear);
            item.BankName.Should().Be(create.BankName);
            item.AccountLast4.Should().Be(create.AccountLast4);
            item.AccountType.Should().Be(create.AccountType);
            item.Gateway.Should().Be(create.Gateway);
            item.GatewayResponseCode.Should().Be(create.GatewayResponseCode);
            item.GatewayResponseMessage.Should().Be(create.GatewayResponseMessage);
            item.OrderId.Should().Be(create.OrderId);
            item.SubscriptionId.Should().Be(create.SubscriptionId);
            item.InternalNotes.Should().Be(create.InternalNotes);
            item.InvoiceId.Should().Be(create.InvoiceId);
            item.AccountId.Should().Be(create.AccountId);
            item.Amount.Should().Be(create.Amount);
            item.PaymentMethod.Should().Be(create.PaymentMethod);
            item.PaymentType.Should().Be(create.PaymentType);
            item.Status.Should().Be(create.Status);
            item.ScheduledDate.Should().Be(create.ScheduledDate);
            item.Description.Should().Be(create.Description);
            item.TokenizedCardId.Should().Be(create.TokenizedCardId);
            item.Status.Should().Be(create.Status);
            item.Description.Should().Be(create.Description);
            item.PaymentDate.Should().Be(create.PaymentDate);
            item.AccountId.Should().Be(create.AccountId);
            item.InvoiceId.Should().Be(create.InvoiceId);
            item.Status.Should().Be(create.Status);
            item.PaymentMethod.Should().Be(create.PaymentMethod);
            item.FromDate.Should().Be(create.FromDate);
            item.ToDate.Should().Be(create.ToDate);
            item.Page.Should().Be(create.Page);
            item.PageSize.Should().Be(create.PageSize);
            item.SortBy.Should().Be(create.SortBy);
            item.SortOrder.Should().Be(create.SortOrder);
            item.Amount.Should().Be(create.Amount);
            item.PaymentMethod.Should().Be(create.PaymentMethod);
            item.TokenizedCardId.Should().Be(create.TokenizedCardId);
            item.AuthorizationCode.Should().Be(create.AuthorizationCode);
            item.Description.Should().Be(create.Description);
            item.RefundAmount.Should().Be(create.RefundAmount);
            item.Reason.Should().Be(create.Reason);

            var getRes = await _client.GetAsync($"/api/payments/{{item.Id}}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new { PaymentNumber = "Test2", InvoiceId = 1, InvoiceNumber = "Test", AccountId = 1, AccountName = "Test", Amount = 1, RefundedAmount = 1, AmountApplied = 1, PaymentMethod = null, PaymentType = null, Status = null, PaymentDate = DateTime.UtcNow, ProcessedDate = DateTime.UtcNow, RefundDate = DateTime.UtcNow, ScheduledDate = DateTime.UtcNow, TransactionId = "Test", AuthorizationCode = "Test", CardLast4 = "Test", CardholderName = "Test", BankReference = "Test", IsReconciled = true, ReconciledDate = DateTime.UtcNow, Description = "Test", FailureReason = "Test", RetryCount = 1, OriginalPaymentId = 1, ExternalPaymentId = "Test", GatewayReference = "Test", CheckNumber = "Test", ProcessingFee = 1, NetAmount = 1, ExchangeRate = 1, SettledDate = DateTime.UtcNow, DepositDate = DateTime.UtcNow, CardBrand = "Test", CardExpMonth = 1, CardExpYear = 1, BankName = "Test", AccountLast4 = "Test", AccountType = "Test", Gateway = "Test", GatewayResponseCode = "Test", GatewayResponseMessage = "Test", OrderId = 1, SubscriptionId = 1, InternalNotes = "Test", InvoiceId = 1, AccountId = 1, Amount = 1, PaymentMethod = null, PaymentType = null, Status = null, ScheduledDate = DateTime.UtcNow, Description = "Test", TokenizedCardId = "Test", Status = null, Description = "Test", PaymentDate = DateTime.UtcNow, AccountId = 1, InvoiceId = 1, Status = null, PaymentMethod = null, FromDate = DateTime.UtcNow, ToDate = DateTime.UtcNow, Page = 1, PageSize = 1, SortBy = "Test", SortOrder = "Test", Amount = 1, PaymentMethod = null, TokenizedCardId = "Test", AuthorizationCode = "Test", Description = "Test", RefundAmount = 1, Reason = "Test" };
            var pRes = await _client.PatchAsJsonAsync($"/api/payments/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/payments/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/payments/{{item.Id}}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/payments/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}

