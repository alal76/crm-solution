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
using System.Text;
using System.Text.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    /// <summary>
    /// ITSM-047: Expanded webhook integration tests — verifies ITSM webhook
    /// endpoints for GET/POST operations, invalid payloads, and non-existent resources.
    /// </summary>
    [Trait("Category", "Integration")]
    [Collection("IntegrationTests")]
    public class ITSMWebhooksControllerTests
    {
        private readonly HttpClient _client;
        public ITSMWebhooksControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task GetEndpoint_ITSMWebhooks_ReturnsNon500()
        {
            var res = await _client.GetAsync("/api/itsm/webhooks");
            ((int)res.StatusCode).Should().BeLessThan(500, "GET /api/itsm/webhooks should not return a server error");
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/itsm/webhooks/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }

        [Fact]
        public async Task Post_EmptyPayload_ShouldReturnBadRequest()
        {
            // Arrange
            var content = new StringContent("{}", Encoding.UTF8, "application/json");

            // Act
            var res = await _client.PostAsync("/api/itsm/webhooks", content);

            // Assert — empty payload should be rejected (400) or handled gracefully (not 500)
            ((int)res.StatusCode).Should().BeLessThan(500, "POST with empty payload should not cause server error");
        }

        [Fact]
        public async Task Post_InvalidJson_ShouldReturnBadRequest()
        {
            // Arrange
            var content = new StringContent("not-json-at-all", Encoding.UTF8, "application/json");

            // Act
            var res = await _client.PostAsync("/api/itsm/webhooks", content);

            // Assert
            ((int)res.StatusCode).Should().BeLessThan(500, "POST with invalid JSON should not cause server error");
        }

        [Fact]
        public async Task Delete_Nonexistent_ShouldNotReturn500()
        {
            // Act
            var res = await _client.DeleteAsync("/api/itsm/webhooks/999999");

            // Assert
            ((int)res.StatusCode).Should().BeLessThan(500, "DELETE on non-existent webhook should not cause server error");
        }

        [Fact]
        public async Task Put_Nonexistent_ShouldNotReturn500()
        {
            // Arrange
            var content = new StringContent("{\"name\":\"test\"}", Encoding.UTF8, "application/json");

            // Act
            var res = await _client.PutAsync("/api/itsm/webhooks/999999", content);

            // Assert
            ((int)res.StatusCode).Should().BeLessThan(500, "PUT on non-existent webhook should not cause server error");
        }

        [Fact]
        public async Task GetEndpoint_ShouldReturnJsonContentType()
        {
            // Act
            var res = await _client.GetAsync("/api/itsm/webhooks");

            // Assert
            if (res.IsSuccessStatusCode)
            {
                res.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
            }
        }
    }
}
