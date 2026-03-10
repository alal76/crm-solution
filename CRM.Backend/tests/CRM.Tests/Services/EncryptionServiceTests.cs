// CRM Solution — Unit Tests
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for EncryptionService (TCOV-034).</summary>
public class EncryptionServiceTests
{
    private readonly EncryptionService _service;
    private readonly Mock<IDataProtector> _protector;

    public EncryptionServiceTests()
    {
        _protector = new Mock<IDataProtector>();
        _protector.Setup(p => p.Protect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
        _protector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
        _protector.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(_protector.Object);

        // IDataProtector also implements IDataProtectionProvider
        var provider = new Mock<IDataProtectionProvider>();
        provider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(_protector.Object);

        // Use real EphemeralDataProtectionProvider so Protect/Unprotect match
        var realProvider = new EphemeralDataProtectionProvider();
        var logger = new Mock<ILogger<EncryptionService>>().Object;
        _service = new EncryptionService(realProvider, logger);
    }

    // ── IsEncrypted ───────────────────────────────────────────────────────────
    [Fact]
    public void IsEncrypted_ShouldReturnFalse_ForPlaintext()
    {
        _service.IsEncrypted("hello world").Should().BeFalse();
    }

    [Fact]
    public void IsEncrypted_ShouldReturnFalse_ForEmpty()
    {
        _service.IsEncrypted(string.Empty).Should().BeFalse();
    }

    [Fact]
    public void IsEncrypted_ShouldReturnTrue_ForEncryptedValue()
    {
        var encrypted = _service.Encrypt("secret");
        _service.IsEncrypted(encrypted).Should().BeTrue();
    }

    // ── Encrypt ───────────────────────────────────────────────────────────────
    [Fact]
    public void Encrypt_ShouldReturnSameValue_WhenAlreadyEncrypted()
    {
        var encrypted = _service.Encrypt("top-secret");
        var doubleEncrypted = _service.Encrypt(encrypted);
        doubleEncrypted.Should().Be(encrypted);
    }

    [Fact]
    public void Encrypt_ShouldReturnInputUnchanged_WhenEmpty()
    {
        _service.Encrypt(string.Empty).Should().BeEmpty();
    }

    // ── Decrypt ───────────────────────────────────────────────────────────────
    [Fact]
    public void Decrypt_ShouldReturnOriginalValue_AfterEncryptDecryptRoundtrip()
    {
        const string original = "my-api-key-12345";
        var encrypted = _service.Encrypt(original);
        var decrypted = _service.Decrypt(encrypted);
        decrypted.Should().Be(original);
    }

    [Fact]
    public void Decrypt_ShouldReturnInput_WhenNotEncrypted()
    {
        var result = _service.Decrypt("plain-text");
        result.Should().Be("plain-text");
    }
}
