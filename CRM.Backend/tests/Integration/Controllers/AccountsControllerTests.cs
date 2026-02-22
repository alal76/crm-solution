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
    public class AccountsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public AccountsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Accounts_Succeeds()
        {
            // FIX: Removed duplicate property names from anonymous type definition. See comments below.
            // The original anonymous type had duplicate property names, which caused build errors (CS0833).
            // Commented out the old code for traceability.
            // var create = new { ... };
            var create = new
            {
                Category = "Test",
                FirstName = "Test",
                LastName = "Test",
                Salutation = "Test",
                Suffix = "Test",
                DateOfBirth = DateTime.UtcNow,
                Gender = "Test",
                LinkedContactId = 1,
                LinkedContactName = "Test",
                Company = "Test",
                LegalName = "Test",
                DbaName = "Test",
                TaxId = "Test",
                RegistrationNumber = "Test",
                YearFounded = 1,
                PrimaryContactId = 1,
                PrimaryContactName = "Test",
                Email = "Test",
                SecondaryEmail = "Test",
                Phone = "Test",
                MobilePhone = "Test",
                FaxNumber = "Test",
                JobTitle = "Test",
                Website = "Test",
                Address = "Test",
                Address2 = "Test",
                City = "Test",
                State = "Test",
                ZipCode = "Test",
                Country = "Test",
                ShippingAddress = "Test",
                ShippingAddress2 = "Test",
                ShippingCity = "Test",
                ShippingState = "Test",
                ShippingZipCode = "Test",
                ShippingCountry = "Test",
                ShippingSameAsBilling = true,
                Industry = "Test",
                SubIndustry = "Test",
                NumberOfEmployees = 1,
                EmployeeRange = "Test",
                AnnualRevenue = 1,
                RevenueRange = "Test",
                AccountType = "Test",
                Priority = "Test",
                StockSymbol = "Test",
                Ownership = "Test",
                LifecycleStage = "Test",
                LeadSource = "Test",
                FirstContactDate = DateTime.UtcNow,
                ConversionDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow,
                NextFollowUpDate = DateTime.UtcNow,
                TotalPurchases = 1,
                AccountBalance = 1,
                CreditLimit = 1,
                PaymentTerms = "Test",
                PreferredPaymentMethod = "Test",
                Currency = "Test",
                BillingCycle = "Test",
                LeadScore = 1,
                AccountHealthScore = 1,
                NpsScore = 1,
                SatisfactionRating = 1,
                LinkedInUrl = "Test",
                TwitterHandle = "Test",
                FacebookUrl = "Test",
                OptInEmail = true,
                OptInSms = true,
                OptInPhone = true,
                PreferredContactMethod = "Test",
                PreferredContactTime = "Test",
                Timezone = "Test",
                PreferredLanguage = "Test",
                AssignedToUserId = 1,
                AssignedToUserName = "Test",
                AccountManagerId = 1,
                AccountManagerName = "Test",
                Territory = "Test",
                Region = "Test",
                Tags = "Test",
                Segment = "Test",
                ReferralSource = "Test",
                ReferredByAccountId = 1,
                ReferredByAccountName = "Test",
                ParentAccountId = 1,
                ParentAccountName = "Test",
                Notes = "Test",
                InternalNotes = "Test",
                Description = "Test",
                CustomFields = "Test",
                ContactCount = 1
            };
            var cRes = await _client.PostAsJsonAsync("/api/accounts", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/accounts/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            // FIX: Removed duplicate property names from anonymous type definition for patch. See comments below.
            // var patch = new { ... };
            var patch = new
            {
                Category = "Test2",
                FirstName = "Test",
                LastName = "Test",
                Salutation = "Test",
                Suffix = "Test",
                DateOfBirth = DateTime.UtcNow,
                Gender = "Test",
                LinkedContactId = 1,
                LinkedContactName = "Test",
                Company = "Test",
                LegalName = "Test",
                DbaName = "Test",
                TaxId = "Test",
                RegistrationNumber = "Test",
                YearFounded = 1,
                PrimaryContactId = 1,
                PrimaryContactName = "Test",
                Email = "Test",
                SecondaryEmail = "Test",
                Phone = "Test",
                MobilePhone = "Test",
                FaxNumber = "Test",
                JobTitle = "Test",
                Website = "Test",
                Address = "Test",
                Address2 = "Test",
                City = "Test",
                State = "Test",
                ZipCode = "Test",
                Country = "Test",
                ShippingAddress = "Test",
                ShippingAddress2 = "Test",
                ShippingCity = "Test",
                ShippingState = "Test",
                ShippingZipCode = "Test",
                ShippingCountry = "Test",
                ShippingSameAsBilling = true,
                Industry = "Test",
                SubIndustry = "Test",
                NumberOfEmployees = 1,
                EmployeeRange = "Test",
                AnnualRevenue = 1,
                RevenueRange = "Test",
                AccountType = "Test",
                Priority = "Test",
                StockSymbol = "Test",
                Ownership = "Test",
                LifecycleStage = "Test",
                LeadSource = "Test",
                FirstContactDate = DateTime.UtcNow,
                ConversionDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow,
                NextFollowUpDate = DateTime.UtcNow,
                TotalPurchases = 1,
                AccountBalance = 1,
                CreditLimit = 1,
                PaymentTerms = "Test",
                PreferredPaymentMethod = "Test",
                Currency = "Test",
                BillingCycle = "Test",
                LeadScore = 1,
                AccountHealthScore = 1,
                NpsScore = 1,
                SatisfactionRating = 1,
                LinkedInUrl = "Test",
                TwitterHandle = "Test",
                FacebookUrl = "Test",
                OptInEmail = true,
                OptInSms = true,
                OptInPhone = true,
                PreferredContactMethod = "Test",
                PreferredContactTime = "Test",
                Timezone = "Test",
                PreferredLanguage = "Test",
                AssignedToUserId = 1,
                AssignedToUserName = "Test",
                AccountManagerId = 1,
                AccountManagerName = "Test",
                Territory = "Test",
                Region = "Test",
                Tags = "Test",
                Segment = "Test",
                ReferralSource = "Test",
                ReferredByAccountId = 1,
                ReferredByAccountName = "Test",
                ParentAccountId = 1,
                ParentAccountName = "Test",
                Notes = "Test",
                InternalNotes = "Test",
                Description = "Test",
                CustomFields = "Test",
                ContactCount = 1
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/accounts/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/accounts/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/accounts/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/accounts/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
