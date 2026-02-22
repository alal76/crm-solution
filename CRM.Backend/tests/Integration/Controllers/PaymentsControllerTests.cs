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
    public class PaymentsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public PaymentsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Payments_Succeeds()
        {
            var create = new
            {
                PaymentNumber = "Test",
                InvoiceId = 1,
                InvoiceNumber = "Test",
                AccountId = 1,
                AccountName = "Test",
                Amount = 1,
                RefundedAmount = 1,
                AmountApplied = 1,
                PaymentMethod = (object?)null,
                PaymentType = (object?)null,
                Status = (object?)null,
                PaymentDate = DateTime.UtcNow,
                ProcessedDate = DateTime.UtcNow,
                RefundDate = DateTime.UtcNow,
                ScheduledDate = DateTime.UtcNow,
                TransactionId = "Test",
                AuthorizationCode = "Test",
                CardLast4 = "Test",
                CardholderName = "Test",
                BankReference = "Test",
                IsReconciled = true,
                ReconciledDate = DateTime.UtcNow,
                Description = "Test",
                FailureReason = "Test",
                RetryCount = 1,
                OriginalPaymentId = 1,
                ExternalPaymentId = "Test",
                GatewayReference = "Test",
                CheckNumber = "Test",
                ProcessingFee = 1,
                NetAmount = 1,
                ExchangeRate = 1,
                SettledDate = DateTime.UtcNow,
                DepositDate = DateTime.UtcNow,
                CardBrand = "Test",
                CardExpMonth = 1,
                CardExpYear = 1,
                BankName = "Test",
                AccountLast4 = "Test",
                AccountType = "Test",
                Gateway = "Test",
                GatewayResponseCode = "Test",
                GatewayResponseMessage = "Test",
                OrderId = 1,
                SubscriptionId = 1,
                InternalNotes = "Test",
                TokenizedCardId = "Test",
                FromDate = DateTime.UtcNow,
                ToDate = DateTime.UtcNow,
                Page = 1,
                PageSize = 1,
                SortBy = "Test",
                SortOrder = "Test",
                RefundAmount = 1,
                Reason = "Test"
            };
            var cRes = await _client.PostAsJsonAsync("/api/payments", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/payments/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                PaymentNumber = "Test2",
                InvoiceId = 1,
                InvoiceNumber = "Test",
                AccountId = 1,
                AccountName = "Test",
                Amount = 1,
                RefundedAmount = 1,
                AmountApplied = 1,
                PaymentMethod = (object?)null,
                PaymentType = (object?)null,
                Status = (object?)null,
                PaymentDate = DateTime.UtcNow,
                ProcessedDate = DateTime.UtcNow,
                RefundDate = DateTime.UtcNow,
                ScheduledDate = DateTime.UtcNow,
                TransactionId = "Test",
                AuthorizationCode = "Test",
                CardLast4 = "Test",
                CardholderName = "Test",
                BankReference = "Test",
                IsReconciled = true,
                ReconciledDate = DateTime.UtcNow,
                Description = "Test",
                FailureReason = "Test",
                RetryCount = 1,
                OriginalPaymentId = 1,
                ExternalPaymentId = "Test",
                GatewayReference = "Test",
                CheckNumber = "Test",
                ProcessingFee = 1,
                NetAmount = 1,
                ExchangeRate = 1,
                SettledDate = DateTime.UtcNow,
                DepositDate = DateTime.UtcNow,
                CardBrand = "Test",
                CardExpMonth = 1,
                CardExpYear = 1,
                BankName = "Test",
                AccountLast4 = "Test",
                AccountType = "Test",
                Gateway = "Test",
                GatewayResponseCode = "Test",
                GatewayResponseMessage = "Test",
                OrderId = 1,
                SubscriptionId = 1,
                InternalNotes = "Test",
                TokenizedCardId = "Test",
                FromDate = DateTime.UtcNow,
                ToDate = DateTime.UtcNow,
                Page = 1,
                PageSize = 1,
                SortBy = "Test",
                SortOrder = "Test",
                RefundAmount = 1,
                Reason = "Test"
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/payments/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/payments/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/payments/{id}");
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
