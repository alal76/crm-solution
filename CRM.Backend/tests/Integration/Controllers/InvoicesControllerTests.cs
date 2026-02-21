using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
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
            var item = await cRes.Content.ReadFromJsonAsync<dynamic>();

            item.InvoiceNumber.Should().Be(create.InvoiceNumber);
            item.AccountId.Should().Be(create.AccountId);
            item.AccountName.Should().Be(create.AccountName);
            item.InvoiceDate.Should().Be(create.InvoiceDate);
            item.DueDate.Should().Be(create.DueDate);
            item.SentDate.Should().Be(create.SentDate);
            item.ViewedDate.Should().Be(create.ViewedDate);
            item.PaidDate.Should().Be(create.PaidDate);
            item.VoidedDate.Should().Be(create.VoidedDate);
            item.Status.Should().Be(create.Status);
            item.InvoiceType.Should().Be(create.InvoiceType);
            item.PaymentTerms.Should().Be(create.PaymentTerms);
            item.Subtotal.Should().Be(create.Subtotal);
            item.DiscountAmount.Should().Be(create.DiscountAmount);
            item.TaxAmount.Should().Be(create.TaxAmount);
            item.ShippingAmount.Should().Be(create.ShippingAmount);
            item.FeesAmount.Should().Be(create.FeesAmount);
            item.TotalAmount.Should().Be(create.TotalAmount);
            item.AmountPaid.Should().Be(create.AmountPaid);
            item.BalanceDue.Should().Be(create.BalanceDue);
            item.CurrencyCode.Should().Be(create.CurrencyCode);
            item.Description.Should().Be(create.Description);
            item.Notes.Should().Be(create.Notes);
            item.VoidReason.Should().Be(create.VoidReason);
            item.OrderId.Should().Be(create.OrderId);
            item.QuoteId.Should().Be(create.QuoteId);
            item.ServicePeriodStart.Should().Be(create.ServicePeriodStart);
            item.ServicePeriodEnd.Should().Be(create.ServicePeriodEnd);
            item.DiscountPercent.Should().Be(create.DiscountPercent);
            item.AmountCredited.Should().Be(create.AmountCredited);
            item.ExchangeRate.Should().Be(create.ExchangeRate);
            item.EarlyPaymentDiscountPercent.Should().Be(create.EarlyPaymentDiscountPercent);
            item.EarlyPaymentDiscountDays.Should().Be(create.EarlyPaymentDiscountDays);
            item.EarlyPaymentDiscountAmount.Should().Be(create.EarlyPaymentDiscountAmount);
            item.LateFeePercent.Should().Be(create.LateFeePercent);
            item.LateFeeAmount.Should().Be(create.LateFeeAmount);
            item.BillingName.Should().Be(create.BillingName);
            item.BillingCompany.Should().Be(create.BillingCompany);
            item.BillingStreet.Should().Be(create.BillingStreet);
            item.BillingCity.Should().Be(create.BillingCity);
            item.BillingState.Should().Be(create.BillingState);
            item.BillingPostalCode.Should().Be(create.BillingPostalCode);
            item.BillingCountry.Should().Be(create.BillingCountry);
            item.BillingEmail.Should().Be(create.BillingEmail);
            item.BillingPhone.Should().Be(create.BillingPhone);
            item.ReminderCount.Should().Be(create.ReminderCount);
            item.LastReminderDate.Should().Be(create.LastReminderDate);
            item.NextReminderDate.Should().Be(create.NextReminderDate);
            item.InCollections.Should().Be(create.InCollections);
            item.InternalNotes.Should().Be(create.InternalNotes);
            item.Footer.Should().Be(create.Footer);
            item.TermsAndConditions.Should().Be(create.TermsAndConditions);
            item.DisputeReason.Should().Be(create.DisputeReason);
            item.PdfUrl.Should().Be(create.PdfUrl);
            item.ContactId.Should().Be(create.ContactId);
            item.SubscriptionId.Should().Be(create.SubscriptionId);
            item.OriginalInvoiceId.Should().Be(create.OriginalInvoiceId);
            item.AccountId.Should().Be(create.AccountId);
            item.InvoiceDate.Should().Be(create.InvoiceDate);
            item.DueDate.Should().Be(create.DueDate);
            item.Status.Should().Be(create.Status);
            item.InvoiceType.Should().Be(create.InvoiceType);
            item.PaymentTerms.Should().Be(create.PaymentTerms);
            item.Subtotal.Should().Be(create.Subtotal);
            item.DiscountAmount.Should().Be(create.DiscountAmount);
            item.TaxAmount.Should().Be(create.TaxAmount);
            item.ShippingAmount.Should().Be(create.ShippingAmount);
            item.FeesAmount.Should().Be(create.FeesAmount);
            item.CurrencyCode.Should().Be(create.CurrencyCode);
            item.Description.Should().Be(create.Description);
            item.Notes.Should().Be(create.Notes);
            item.OrderId.Should().Be(create.OrderId);
            item.QuoteId.Should().Be(create.QuoteId);
            item.InternalNotes.Should().Be(create.InternalNotes);
            item.TermsAndConditions.Should().Be(create.TermsAndConditions);
            item.BillingName.Should().Be(create.BillingName);
            item.BillingStreet.Should().Be(create.BillingStreet);
            item.BillingCity.Should().Be(create.BillingCity);
            item.BillingState.Should().Be(create.BillingState);
            item.BillingCountry.Should().Be(create.BillingCountry);
            item.DueDate.Should().Be(create.DueDate);
            item.Status.Should().Be(create.Status);
            item.InvoiceType.Should().Be(create.InvoiceType);
            item.PaymentTerms.Should().Be(create.PaymentTerms);
            item.DiscountAmount.Should().Be(create.DiscountAmount);
            item.ShippingAmount.Should().Be(create.ShippingAmount);
            item.FeesAmount.Should().Be(create.FeesAmount);
            item.Description.Should().Be(create.Description);
            item.Notes.Should().Be(create.Notes);
            item.InternalNotes.Should().Be(create.InternalNotes);
            item.TermsAndConditions.Should().Be(create.TermsAndConditions);
            item.BillingName.Should().Be(create.BillingName);
            item.BillingStreet.Should().Be(create.BillingStreet);
            item.BillingCity.Should().Be(create.BillingCity);
            item.BillingState.Should().Be(create.BillingState);
            item.BillingCountry.Should().Be(create.BillingCountry);
            item.AccountId.Should().Be(create.AccountId);
            item.Status.Should().Be(create.Status);
            item.InvoiceType.Should().Be(create.InvoiceType);
            item.FromDate.Should().Be(create.FromDate);
            item.ToDate.Should().Be(create.ToDate);
            item.Page.Should().Be(create.Page);
            item.PageSize.Should().Be(create.PageSize);
            item.SortBy.Should().Be(create.SortBy);
            item.SortOrder.Should().Be(create.SortOrder);
            item.InvoiceId.Should().Be(create.InvoiceId);
            item.LineNumber.Should().Be(create.LineNumber);
            item.ProductId.Should().Be(create.ProductId);
            item.ProductName.Should().Be(create.ProductName);
            item.Description.Should().Be(create.Description);
            item.Quantity.Should().Be(create.Quantity);
            item.UnitPrice.Should().Be(create.UnitPrice);
            item.DiscountAmount.Should().Be(create.DiscountAmount);
            item.TaxAmount.Should().Be(create.TaxAmount);
            item.TotalAmount.Should().Be(create.TotalAmount);
            item.ProductId.Should().Be(create.ProductId);
            item.Description.Should().Be(create.Description);
            item.Quantity.Should().Be(create.Quantity);
            item.UnitPrice.Should().Be(create.UnitPrice);
            item.DiscountAmount.Should().Be(create.DiscountAmount);
            item.TaxAmount.Should().Be(create.TaxAmount);
            item.TotalCount.Should().Be(create.TotalCount);
            item.Page.Should().Be(create.Page);
            item.PageSize.Should().Be(create.PageSize);

            var getRes = await _client.GetAsync($"/api/invoices/{{item.Id}}");
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
            var pRes = await _client.PatchAsJsonAsync($"/api/invoices/{{item.Id}}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/invoices/{{item.Id}}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/invoices/{{item.Id}}");
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

