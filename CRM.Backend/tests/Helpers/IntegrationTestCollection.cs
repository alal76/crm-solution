// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Xunit;

namespace CRM.Tests.Helpers;

/// <summary>
/// Shared test collection for all integration controller tests.
/// All tests in this collection share a single <see cref="ApiTestFactory"/>
/// instance and run sequentially to prevent resource contention from
/// concurrent WebApplicationFactory instances.
/// </summary>
[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<ApiTestFactory>
{
}
