// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for HttpCalloutService (TCOV-033).</summary>
public class HttpCalloutServiceTests
{
    private readonly HttpCalloutService _service;

    public HttpCalloutServiceTests()
    {
        var factory = new Mock<IHttpClientFactory>().Object;
        var logger = new Mock<ILogger<HttpCalloutService>>().Object;
        _service = new HttpCalloutService(factory, logger);
    }

    // ── Validate ─────────────────────────────────────────────────────────────
    [Fact]
    public void Validate_ShouldFail_WhenUrlIsEmpty()
    {
        var config = new HttpCalloutConfig { Url = "", Method = "GET", TimeoutSeconds = 10 };
        var result = _service.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("URL"));
    }

    [Fact]
    public void Validate_ShouldFail_WhenUrlIsNotAbsolute()
    {
        var config = new HttpCalloutConfig { Url = "/relative/path", Method = "GET", TimeoutSeconds = 10 };
        var result = _service.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("absolute"));
    }

    [Fact]
    public void Validate_ShouldFail_WhenMethodIsInvalid()
    {
        var config = new HttpCalloutConfig { Url = "https://example.com", Method = "INVALID", TimeoutSeconds = 10 };
        var result = _service.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Method"));
    }

    [Fact]
    public void Validate_ShouldFail_WhenTimeoutOutOfRange()
    {
        var config = new HttpCalloutConfig { Url = "https://example.com", Method = "GET", TimeoutSeconds = 0 };
        var result = _service.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("TimeoutSeconds"));
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenConfigIsValid()
    {
        var config = new HttpCalloutConfig
        {
            Url = "https://api.example.com/hook",
            Method = "POST",
            TimeoutSeconds = 30,
            RetryCount = 2
        };
        var result = _service.Validate(config);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldFail_WhenRetryCountExceedsMax()
    {
        var config = new HttpCalloutConfig { Url = "https://example.com", Method = "GET", TimeoutSeconds = 10, RetryCount = 10 };
        var result = _service.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RetryCount"));
    }
}
