// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Xunit;

// Register custom xUnit framework for this assembly
// This enables automatic try-catch logging of all test results (pass/fail/skip)
[assembly: TestFramework("CRM.Tests.Infrastructure.TestLogging.LoggingTestFramework", "CRM.Tests.Services")]
