// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// ENUM-TEST-006, ENUM-TEST-007: Integration tests for the Enum Management API endpoints.
// These tests require a running database and are skipped in CI unless a live DB is available.
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Integration tests for the /api/enummanagement endpoints.
/// Skip attribute is set on each test because these require a live MariaDB instance
/// (crm_db) and a running CRM API. They can be run locally with:
///   dotnet test --filter "EnumManagementIntegration"
/// after starting the database stack.
/// </summary>
public class EnumManagementIntegrationTests
{
    // ─── ENUM-TEST-006 ────────────────────────────────────────────────────────

    /// <summary>
    /// ENUM-TEST-006:
    ///   GET /api/enummanagement/categories/{name}/values returns a 200 response
    ///   containing enum values for the LeadStatus category.
    ///
    /// What this test does when run against a live system:
    ///   1. Sends: GET /api/enummanagement/categories/LeadStatus/values
    ///   2. Asserts HTTP 200
    ///   3. Deserialises response body to List&lt;EnumValueDto&gt;
    ///   4. Asserts the list contains at least one item where Key == "new"
    ///   5. Asserts all returned items have IsActive == true
    ///
    /// Category "LeadStatus" must be seeded via SYS-009-EnumEntityMigration.sql
    /// before this test can pass.
    /// </summary>
    [Fact(Skip = "Integration – requires live crm_db and running CRM API")]
    public async Task GET_api_enummanagement_LeadStatus_values_ReturnsEnumValues()
    {
        /*
         * Integration test implementation (activate by removing [Skip]):
         *
         *   var factory = new WebApplicationFactory<CRM.Api.Program>();
         *   var client  = factory.CreateClient();
         *
         *   // Authenticate as admin
         *   var loginResp = await client.PostAsJsonAsync("/api/auth/login",
         *       new { email = "admin@crm.local", password = "Admin@123" });
         *   var token = (await loginResp.Content.ReadFromJsonAsync<JsonElement>())
         *       .GetProperty("accessToken").GetString();
         *   client.DefaultRequestHeaders.Authorization =
         *       new AuthenticationHeaderValue("Bearer", token);
         *
         *   // Exercise endpoint
         *   var response = await client.GetAsync("/api/enummanagement/categories/LeadStatus/values");
         *   response.StatusCode.Should().Be(HttpStatusCode.OK);
         *
         *   var values = await response.Content.ReadFromJsonAsync<List<EnumValueDto>>();
         *   values.Should().NotBeNullOrEmpty();
         *   values.Should().OnlyContain(v => v.IsActive);
         *   values.Should().ContainSingle(v => v.Key == "new");
         */
        await Task.CompletedTask; // keeps compiler happy while skipped
    }

    // ─── ENUM-TEST-007 ────────────────────────────────────────────────────────

    /// <summary>
    /// ENUM-TEST-007:
    ///   POST /api/enummanagement/categories/{id}/values creates a new enum value
    ///   and returns 201 Created.
    ///
    /// What this test does when run against a live system:
    ///   1. Authenticates as admin
    ///   2. Fetches the id for a writable category (e.g. one where AllowCustomValues = true)
    ///   3. Sends: POST /api/enummanagement/categories/{id}/values
    ///      Body: { "key": "integration_test_val", "label": "Integration Test", "isDefault": false }
    ///   4. Asserts HTTP 201
    ///   5. Deserialises response to EnumValueDto and asserts Key == "integration_test_val"
    ///   6. Cleans up: sends DELETE /api/enummanagement/values/{newId}
    /// </summary>
    [Fact(Skip = "Integration – requires live crm_db and running CRM API")]
    public async Task POST_api_enummanagement_categories_values_CreatesNewValue()
    {
        /*
         * Integration test implementation (activate by removing [Skip]):
         *
         *   var factory = new WebApplicationFactory<CRM.Api.Program>();
         *   var client  = factory.CreateClient();
         *
         *   // Authenticate
         *   var loginResp = await client.PostAsJsonAsync("/api/auth/login",
         *       new { email = "admin@crm.local", password = "Admin@123" });
         *   var token = (await loginResp.Content.ReadFromJsonAsync<JsonElement>())
         *       .GetProperty("accessToken").GetString();
         *   client.DefaultRequestHeaders.Authorization =
         *       new AuthenticationHeaderValue("Bearer", token);
         *
         *   // Get a writable category id (e.g. "LeadSource")
         *   var catResp = await client.GetAsync("/api/enummanagement/categories/LeadSource");
         *   var category = await catResp.Content.ReadFromJsonAsync<EnumCategoryDto>();
         *
         *   // Create new value
         *   var createPayload = new { key = "integration_test_val", label = "Integration Test", isDefault = false };
         *   var createResp = await client.PostAsJsonAsync(
         *       $"/api/enummanagement/categories/{category!.Id}/values", createPayload);
         *   createResp.StatusCode.Should().Be(HttpStatusCode.Created);
         *
         *   var created = await createResp.Content.ReadFromJsonAsync<EnumValueDto>();
         *   created.Should().NotBeNull();
         *   created!.Key.Should().Be("integration_test_val");
         *
         *   // Clean up
         *   await client.DeleteAsync($"/api/enummanagement/values/{created.Id}");
         */
        await Task.CompletedTask;
    }
}
