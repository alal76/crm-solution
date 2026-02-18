// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Api.Helpers;
using CRM.Core.Entities;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Helpers;

/// <summary>
/// Unit tests for ETagHelper optimistic concurrency control operations.
/// Tests ETag generation, parsing, and conditional request handling.
/// </summary>
public class ETagHelperTests
{
    #region GenerateETag from byte[] Tests

    [Fact]
    public void GenerateETag_WithValidByteArray_ShouldReturnBase64WithQuotes()
    {
        // Arrange
        var rowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

        // Act
        var result = ETagHelper.GenerateETag(rowVersion);

        // Assert
        result.Should().StartWith("\"");
        result.Should().EndWith("\"");
        result.Should().Be($"\"{Convert.ToBase64String(rowVersion)}\"");
    }

    [Fact]
    public void GenerateETag_WithNullByteArray_ShouldReturnZeroETag()
    {
        // Arrange
        byte[]? rowVersion = null;

        // Act
        var result = ETagHelper.GenerateETag(rowVersion);

        // Assert
        result.Should().Be("\"0\"");
    }

    [Fact]
    public void GenerateETag_WithEmptyByteArray_ShouldReturnZeroETag()
    {
        // Arrange
        var rowVersion = Array.Empty<byte>();

        // Act
        var result = ETagHelper.GenerateETag(rowVersion);

        // Assert
        result.Should().Be("\"0\"");
    }

    [Fact]
    public void GenerateETag_WithSameRowVersion_ShouldReturnSameETag()
    {
        // Arrange
        var rowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        var result1 = ETagHelper.GenerateETag(rowVersion);
        var result2 = ETagHelper.GenerateETag(rowVersion);

        // Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public void GenerateETag_WithDifferentRowVersions_ShouldReturnDifferentETags()
    {
        // Arrange
        var rowVersion1 = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var rowVersion2 = new byte[] { 0x05, 0x06, 0x07, 0x08 };

        // Act
        var result1 = ETagHelper.GenerateETag(rowVersion1);
        var result2 = ETagHelper.GenerateETag(rowVersion2);

        // Assert
        result1.Should().NotBe(result2);
    }

    #endregion

    #region GenerateETag from BaseEntity Tests

    [Fact]
    public void GenerateETag_WithEntityHavingRowVersion_ShouldReturnValidETag()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = 1,
            RowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 }
        };

        // Act
        var result = ETagHelper.GenerateETag(entity);

        // Assert
        result.Should().StartWith("\"");
        result.Should().EndWith("\"");
        result.Should().Be($"\"{Convert.ToBase64String(entity.RowVersion)}\"");
    }

    [Fact]
    public void GenerateETag_WithEntityHavingNullRowVersion_ShouldReturnZeroETag()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = 1,
            RowVersion = null
        };

        // Act
        var result = ETagHelper.GenerateETag(entity);

        // Assert
        result.Should().Be("\"0\"");
    }

    #endregion

    #region ParseETag Tests

    [Fact]
    public void ParseETag_WithValidETag_ShouldReturnByteArray()
    {
        // Arrange
        var originalBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var etag = $"\"{Convert.ToBase64String(originalBytes)}\"";

        // Act
        var result = ETagHelper.ParseETag(etag);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(originalBytes);
    }

    [Fact]
    public void ParseETag_WithValidETagWithoutQuotes_ShouldReturnByteArray()
    {
        // Arrange
        var originalBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var etag = Convert.ToBase64String(originalBytes);

        // Act
        var result = ETagHelper.ParseETag(etag);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(originalBytes);
    }

    [Fact]
    public void ParseETag_WithNullETag_ShouldReturnNull()
    {
        // Arrange
        string? etag = null;

        // Act
        var result = ETagHelper.ParseETag(etag);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseETag_WithEmptyETag_ShouldReturnNull()
    {
        // Arrange
        var etag = "";

        // Act
        var result = ETagHelper.ParseETag(etag);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseETag_WithWhitespaceETag_ShouldReturnNull()
    {
        // Arrange
        var etag = "   ";

        // Act
        var result = ETagHelper.ParseETag(etag);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseETag_WithZeroETag_ShouldReturnNull()
    {
        // Arrange
        var etag = "\"0\"";

        // Act
        var result = ETagHelper.ParseETag(etag);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseETag_WithInvalidBase64_ShouldReturnNull()
    {
        // Arrange
        var etag = "\"not-valid-base64!@#$%\"";

        // Act
        var result = ETagHelper.ParseETag(etag);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseETag_RoundTrip_ShouldReturnOriginalValue()
    {
        // Arrange
        var originalBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

        // Act
        var etag = ETagHelper.GenerateETag(originalBytes);
        var result = ETagHelper.ParseETag(etag);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(originalBytes);
    }

    #endregion

    #region IsMatch Tests

    [Fact]
    public void IsMatch_WithMatchingETag_ShouldReturnTrue()
    {
        // Arrange
        var rowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var ifMatch = ETagHelper.GenerateETag(rowVersion);

        // Act
        var result = ETagHelper.IsMatch(ifMatch, rowVersion);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMatch_WithNonMatchingETag_ShouldReturnFalse()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var differentRowVersion = new byte[] { 0x05, 0x06, 0x07, 0x08 };
        var ifMatch = ETagHelper.GenerateETag(differentRowVersion);

        // Act
        var result = ETagHelper.IsMatch(ifMatch, currentRowVersion);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMatch_WithNullIfMatch_ShouldReturnTrue()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        var result = ETagHelper.IsMatch(null, currentRowVersion);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMatch_WithEmptyIfMatch_ShouldReturnTrue()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        var result = ETagHelper.IsMatch("", currentRowVersion);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMatch_WithWildcard_ShouldReturnTrue()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        var result = ETagHelper.IsMatch("*", currentRowVersion);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMatch_WithNullCurrentRowVersion_ShouldReturnFalse()
    {
        // Arrange
        var ifMatch = "\"AQIDBAU=\"";

        // Act
        var result = ETagHelper.IsMatch(ifMatch, null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMatch_WithInvalidETagFormat_ShouldReturnFalse()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        var result = ETagHelper.IsMatch("\"invalid!@#\"", currentRowVersion);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsNoneMatch Tests

    [Fact]
    public void IsNoneMatch_WithNullIfNoneMatch_ShouldReturnTrue()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        var result = ETagHelper.IsNoneMatch(null, currentRowVersion);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNoneMatch_WithEmptyIfNoneMatch_ShouldReturnTrue()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        var result = ETagHelper.IsNoneMatch("", currentRowVersion);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNoneMatch_WithMatchingETag_ShouldReturnFalse()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var ifNoneMatch = ETagHelper.GenerateETag(currentRowVersion);

        // Act
        var result = ETagHelper.IsNoneMatch(ifNoneMatch, currentRowVersion);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNoneMatch_WithNonMatchingETag_ShouldReturnTrue()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var differentRowVersion = new byte[] { 0x05, 0x06, 0x07, 0x08 };
        var ifNoneMatch = ETagHelper.GenerateETag(differentRowVersion);

        // Act
        var result = ETagHelper.IsNoneMatch(ifNoneMatch, currentRowVersion);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNoneMatch_WithWildcard_ShouldReturnFalse()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        var result = ETagHelper.IsNoneMatch("*", currentRowVersion);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNoneMatch_WithMultipleETags_OneMatching_ShouldReturnFalse()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var differentRowVersion = new byte[] { 0x05, 0x06, 0x07, 0x08 };
        var currentEtag = ETagHelper.GenerateETag(currentRowVersion);
        var differentEtag = ETagHelper.GenerateETag(differentRowVersion);
        var ifNoneMatch = $"{differentEtag}, {currentEtag}";

        // Act
        var result = ETagHelper.IsNoneMatch(ifNoneMatch, currentRowVersion);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNoneMatch_WithMultipleETags_NoneMatching_ShouldReturnTrue()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var etag1 = ETagHelper.GenerateETag(new byte[] { 0x11, 0x12, 0x13, 0x14 });
        var etag2 = ETagHelper.GenerateETag(new byte[] { 0x21, 0x22, 0x23, 0x24 });
        var ifNoneMatch = $"{etag1}, {etag2}";

        // Act
        var result = ETagHelper.IsNoneMatch(ifNoneMatch, currentRowVersion);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNoneMatch_WithMultipleETags_IncludingWildcard_ShouldReturnFalse()
    {
        // Arrange
        var currentRowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var differentEtag = ETagHelper.GenerateETag(new byte[] { 0x11, 0x12, 0x13, 0x14 });
        var ifNoneMatch = $"{differentEtag}, *";

        // Act
        var result = ETagHelper.IsNoneMatch(ifNoneMatch, currentRowVersion);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Test Entity

    private class TestEntity : BaseEntity
    {
    }

    #endregion
}
