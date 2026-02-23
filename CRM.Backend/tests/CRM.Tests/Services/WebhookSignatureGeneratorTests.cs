// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for WebhookSignatureGenerator
/// Tests cover HMAC-SHA256 generation, validation, and timestamped signatures
/// </summary>
public class WebhookSignatureGeneratorTests
{
    private readonly WebhookSignatureGenerator _generator = new();

    private const string TestPayload = "{\"event\":\"contact.created\",\"id\":42}";
    private const string TestSecret = "whsec_test_secret_key_2026";

    [Fact]
    public void GenerateSignature_ShouldReturnConsistentHash_ForSameInput()
    {
        var sig1 = _generator.GenerateSignature(TestPayload, TestSecret);
        var sig2 = _generator.GenerateSignature(TestPayload, TestSecret);

        sig1.Should().Be(sig2);
    }

    [Fact]
    public void GenerateSignature_ShouldReturnDifferentHash_ForDifferentSecret()
    {
        var sig1 = _generator.GenerateSignature(TestPayload, TestSecret);
        var sig2 = _generator.GenerateSignature(TestPayload, "different_secret");

        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void GenerateSignature_ShouldReturnDifferentHash_ForDifferentPayload()
    {
        var sig1 = _generator.GenerateSignature(TestPayload, TestSecret);
        var sig2 = _generator.GenerateSignature("{\"event\":\"contact.deleted\"}", TestSecret);

        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void GenerateSignature_ShouldReturnLowercaseHexString()
    {
        var sig = _generator.GenerateSignature(TestPayload, TestSecret);

        sig.Should().MatchRegex("^[0-9a-f]+$");
    }

    [Fact]
    public void GenerateSignature_ShouldReturn64CharHexString()
    {
        // SHA-256 outputs 32 bytes = 64 hex chars
        var sig = _generator.GenerateSignature(TestPayload, TestSecret);

        sig.Should().HaveLength(64);
    }

    [Fact]
    public void ValidateSignature_ShouldReturnTrue_WhenSignatureMatches()
    {
        var sig = _generator.GenerateSignature(TestPayload, TestSecret);

        var result = _generator.ValidateSignature(TestPayload, TestSecret, sig);

        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateSignature_ShouldReturnFalse_WhenSignatureMismatch()
    {
        var result = _generator.ValidateSignature(TestPayload, TestSecret, "0000000000000000000000000000000000000000000000000000000000000000");

        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateSignature_ShouldReturnFalse_WhenPayloadTampered()
    {
        var sig = _generator.GenerateSignature(TestPayload, TestSecret);

        var result = _generator.ValidateSignature("{\"event\":\"tampered\"}", TestSecret, sig);

        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateTimestampedSignature_ShouldIncludeTimestamp()
    {
        long timestamp = 1740000000;
        var sigWithTs = _generator.GenerateTimestampedSignature(TestPayload, TestSecret, timestamp);
        var sigWithoutTs = _generator.GenerateSignature(TestPayload, TestSecret);

        sigWithTs.Should().NotBe(sigWithoutTs);
    }

    [Fact]
    public void GenerateTimestampedSignature_ShouldBeConsistent_ForSameTimestamp()
    {
        long timestamp = 1740000000;
        var sig1 = _generator.GenerateTimestampedSignature(TestPayload, TestSecret, timestamp);
        var sig2 = _generator.GenerateTimestampedSignature(TestPayload, TestSecret, timestamp);

        sig1.Should().Be(sig2);
    }

    [Fact]
    public void GenerateTimestampedSignature_ShouldDiffer_ForDifferentTimestamps()
    {
        var sig1 = _generator.GenerateTimestampedSignature(TestPayload, TestSecret, 1740000000);
        var sig2 = _generator.GenerateTimestampedSignature(TestPayload, TestSecret, 1740000001);

        sig1.Should().NotBe(sig2);
    }
}
