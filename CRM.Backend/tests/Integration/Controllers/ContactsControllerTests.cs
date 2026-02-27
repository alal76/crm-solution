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
    [Trait("Category", "Integration")]
    public class ContactsControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public ContactsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Contacts_Succeeds()
        {
            var create = new
            {
                Platform = "Test",
                Url = "Test",
                Handle = "Test",
                ContactType = "Test",
                FirstName = "Test",
                LastName = "Test",
                MiddleName = "Test",
                EmailPrimary = "Test",
                EmailSecondary = "Test",
                PhonePrimary = "Test",
                PhoneSecondary = "Test",
                Address = "Test",
                City = "Test",
                State = "Test",
                Country = "Test",
                ZipCode = "Test",
                JobTitle = "Test",
                Department = "Test",
                Company = "Test",
                ReportsTo = "Test",
                Notes = "Test",
                DateOfBirth = DateTime.UtcNow,
                DateAdded = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                ModifiedBy = "Test",
                Salutation = "Test",
                Suffix = "Test",
                Nickname = "Test",
                Gender = "Test",
                PhoneMobile = "Test",
                PhoneFax = "Test",
                Website = "Test",
                LinkedInUrl = "Test",
                TwitterHandle = "Test",
                DoNotContact = true,
                PreferredContactMethod = "Test",
                LeadStatus = "Test",
                AccountId = 1,
                Status = "Test"
            };
            var cRes = await _client.PostAsJsonAsync("/api/contacts", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/contacts/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/contacts/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
