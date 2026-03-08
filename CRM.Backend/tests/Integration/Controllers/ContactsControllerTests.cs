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
    [Collection("IntegrationTests")]
    public class ContactsControllerTests
    {
        private readonly HttpClient _client;
        public ContactsControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_Contacts_Succeeds()
        {
            var uid = Guid.NewGuid().ToString("N")[..8];
            var create = new
            {
                FirstName = $"Test_{uid}",
                LastName = $"Test_{uid}",
                MiddleName = "Test",
                EmailPrimary = "test@example.com",
                EmailSecondary = "test2@example.com",
                PhonePrimary = "+15551234567",
                PhoneSecondary = "+15559876543",
                Address = "Test",
                City = "Test",
                State = "Test",
                Country = "Test",
                ZipCode = "12345",
                JobTitle = "Test",
                Department = "Test",
                Company = "Test",
                Notes = "Test",
                DateOfBirth = DateTime.UtcNow,
                Salutation = "Mr",
                Suffix = "Jr",
                Nickname = "Test",
                Gender = "Male",
                PhoneMobile = "+15551112222",
                Website = "https://example.com",
                LinkedInUrl = "https://linkedin.com/in/test",
                TwitterHandle = "@TestHandle",
                DoNotContact = true,
                PreferredContactMethod = "Email",
                Status = "Active"
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
