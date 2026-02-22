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
    public class InvoicesControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public InvoicesControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Invoices_Succeeds()
        {
            var create = new
            {
                InvoiceNumber = "Test",
                AccountId = 1,
                AccountName = "Test",
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                SentDate = DateTime.UtcNow,
                ViewedDate = DateTime.UtcNow,
                PaidDate = DateTime.UtcNow,
                VoidedDate = DateTime.UtcNow,
                Status = (object?)null,
                InvoiceType = (object?)null,
                PaymentTerms = (object?)null,
                Subtotal = 1,
                DiscountAmount = 1,
                TaxAmount = 1,
                ShippingAmount = 1,
                FeesAmount = 1,
                TotalAmount = 1,
                AmountPaid = 1,
                BalanceDue = 1,
                CurrencyCode = "Test",
                Description = "Test",
                Notes = "Test",
                VoidReason = "Test",
                OrderId = 1,
                QuoteId = 1,
                ServicePeriodStart = DateTime.UtcNow,
                ServicePeriodEnd = DateTime.UtcNow,
                DiscountPercent = 1,
                AmountCredited = 1,
                ExchangeRate = 1,
                EarlyPaymentDiscountPercent = 1,
                EarlyPaymentDiscountDays = 1,
                EarlyPaymentDiscountAmount = 1,
                LateFeePercent = 1,
                LateFeeAmount = 1,
                BillingName = "Test",
                BillingCompany = "Test",
                BillingStreet = "Test",
                BillingCity = "Test",
                BillingState = "Test",
                BillingPostalCode = "Test",
                BillingCountry = "Test",
                BillingEmail = "Test",
                BillingPhone = "Test",
                ReminderCount = 1,
                LastReminderDate = DateTime.UtcNow,
                NextReminderDate = DateTime.UtcNow,
                InCollections = true,
                InternalNotes = "Test",
                Footer = "Test",
                TermsAndConditions = "Test",
                DisputeReason = "Test",
                PdfUrl = "Test",
                ContactId = 1,
                SubscriptionId = 1,
                OriginalInvoiceId = 1,
                FromDate = DateTime.UtcNow,
                ToDate = DateTime.UtcNow,
                Page = 1,
                PageSize = 1,
                SortBy = "Test",
                SortOrder = "Test",
                InvoiceId = 1,
                LineNumber = 1,
                ProductId = 1,
                ProductName = "Test",
                Quantity = 1,
                UnitPrice = 1,
                TotalCount = 1
            };
            var cRes = await _client.PostAsJsonAsync("/api/invoices", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/invoices/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                InvoiceNumber = "Test2",
                AccountId = 1,
                AccountName = "Test",
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                SentDate = DateTime.UtcNow,
                ViewedDate = DateTime.UtcNow,
                PaidDate = DateTime.UtcNow,
                VoidedDate = DateTime.UtcNow,
                Status = (object?)null,
                InvoiceType = (object?)null,
                PaymentTerms = (object?)null,
                Subtotal = 1,
                DiscountAmount = 1,
                TaxAmount = 1,
                ShippingAmount = 1,
                FeesAmount = 1,
                TotalAmount = 1,
                AmountPaid = 1,
                BalanceDue = 1,
                CurrencyCode = "Test",
                Description = "Test",
                Notes = "Test",
                VoidReason = "Test",
                OrderId = 1,
                QuoteId = 1,
                ServicePeriodStart = DateTime.UtcNow,
                ServicePeriodEnd = DateTime.UtcNow,
                DiscountPercent = 1,
                AmountCredited = 1,
                ExchangeRate = 1,
                EarlyPaymentDiscountPercent = 1,
                EarlyPaymentDiscountDays = 1,
                EarlyPaymentDiscountAmount = 1,
                LateFeePercent = 1,
                LateFeeAmount = 1,
                BillingName = "Test",
                BillingCompany = "Test",
                BillingStreet = "Test",
                BillingCity = "Test",
                BillingState = "Test",
                BillingPostalCode = "Test",
                BillingCountry = "Test",
                BillingEmail = "Test",
                BillingPhone = "Test",
                ReminderCount = 1,
                LastReminderDate = DateTime.UtcNow,
                NextReminderDate = DateTime.UtcNow,
                InCollections = true,
                InternalNotes = "Test",
                Footer = "Test",
                TermsAndConditions = "Test",
                DisputeReason = "Test",
                PdfUrl = "Test",
                ContactId = 1,
                SubscriptionId = 1,
                OriginalInvoiceId = 1,
                FromDate = DateTime.UtcNow,
                ToDate = DateTime.UtcNow,
                Page = 1,
                PageSize = 1,
                SortBy = "Test",
                SortOrder = "Test",
                InvoiceId = 1,
                LineNumber = 1,
                ProductId = 1,
                ProductName = "Test",
                Quantity = 1,
                UnitPrice = 1,
                TotalCount = 1
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/invoices/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/invoices/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/invoices/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/invoices/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
