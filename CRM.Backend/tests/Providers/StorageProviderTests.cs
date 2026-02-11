// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Threading;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for Storage Provider
/// Covers: File upload, download, delete, metadata
/// </summary>
public class StorageProviderTests
{
    private readonly Mock<IOptions<StorageSettings>> _mockStorageSettings;
    private readonly Mock<ILogger<StorageService>> _mockLogger;
    private readonly StorageSettings _settings;

    public StorageProviderTests()
    {
        _settings = new StorageSettings
        {
            BasePath = "/tmp/crm-storage",
            MaxFileSizeBytes = 10 * 1024 * 1024, // 10MB
            AllowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".png" },
            UseSubdirectories = true
        };

        _mockStorageSettings = new Mock<IOptions<StorageSettings>>();
        _mockStorageSettings.Setup(x => x.Value).Returns(_settings);
        _mockLogger = new Mock<ILogger<StorageService>>();
    }

    #region Upload Tests

    [Fact]
    public async Task UploadAsync_ValidFile_ReturnsFileId()
    {
        // Arrange
        var service = CreateMockStorageService();
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "document.pdf";

        // Act
        var result = await service.UploadAsync(content, fileName);

        // Assert
        result.Success.Should().BeTrue();
        result.FileId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UploadAsync_EmptyContent_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();
        var content = Array.Empty<byte>();
        var fileName = "empty.pdf";

        // Act
        var result = await service.UploadAsync(content, fileName);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UploadAsync_NullContent_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();

        // Act
        var result = await service.UploadAsync(null!, "test.pdf");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UploadAsync_ExceedsMaxSize_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();
        var content = new byte[_settings.MaxFileSizeBytes + 1];
        var fileName = "large.pdf";

        // Act
        var result = await service.UploadAsync(content, fileName);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("size");
    }

    [Fact]
    public async Task UploadAsync_DisallowedExtension_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();
        var content = new byte[] { 1, 2, 3 };
        var fileName = "script.exe";

        // Act
        var result = await service.UploadAsync(content, fileName);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("extension");
    }

    [Fact]
    public async Task UploadAsync_NoExtension_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();
        var content = new byte[] { 1, 2, 3 };
        var fileName = "noextension";

        // Act
        var result = await service.UploadAsync(content, fileName);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UploadAsync_WithMetadata_StoresMetadata()
    {
        // Arrange
        var service = CreateMockStorageService();
        var content = new byte[] { 1, 2, 3 };
        var fileName = "document.pdf";
        var metadata = new Dictionary<string, string>
        {
            { "Author", "John Doe" },
            { "Category", "Contracts" }
        };

        // Act
        var result = await service.UploadAsync(content, fileName, metadata);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UploadAsync_WithEntityLink_LinksToEntity()
    {
        // Arrange
        var service = CreateMockStorageService();
        var content = new byte[] { 1, 2, 3 };
        var fileName = "document.pdf";

        // Act
        var result = await service.UploadAsync(content, fileName, entityType: "Account", entityId: 1);

        // Assert
        result.Success.Should().BeTrue();
        result.EntityType.Should().Be("Account");
        result.EntityId.Should().Be(1);
    }

    #endregion

    #region Download Tests

    [Fact]
    public async Task DownloadAsync_ExistingFile_ReturnsContent()
    {
        // Arrange
        var service = CreateMockStorageService();
        var uploadResult = await service.UploadAsync(new byte[] { 1, 2, 3 }, "test.pdf");

        // Act
        var result = await service.DownloadAsync(uploadResult.FileId!);

        // Assert
        result.Success.Should().BeTrue();
        result.Content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DownloadAsync_NonExistingFile_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();
        var fileId = "non-existing-id";

        // Act
        var result = await service.DownloadAsync(fileId);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DownloadAsync_EmptyFileId_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();

        // Act
        var result = await service.DownloadAsync("");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DownloadAsync_NullFileId_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();

        // Act
        var result = await service.DownloadAsync(null!);

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_ExistingFile_ReturnsTrue()
    {
        // Arrange
        var service = CreateMockStorageService();
        var uploadResult = await service.UploadAsync(new byte[] { 1, 2, 3 }, "test.pdf");

        // Act
        var result = await service.DeleteAsync(uploadResult.FileId!);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingFile_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();
        var fileId = "non-existing-id";

        // Act
        var result = await service.DeleteAsync(fileId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_EmptyFileId_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();

        // Act
        var result = await service.DeleteAsync("");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public async Task GetMetadataAsync_ExistingFile_ReturnsMetadata()
    {
        // Arrange
        var service = CreateMockStorageService();
        var metadata = new Dictionary<string, string> { { "Key1", "Value1" } };
        var uploadResult = await service.UploadAsync(new byte[] { 1, 2, 3 }, "test.pdf", metadata);

        // Act
        var result = await service.GetMetadataAsync(uploadResult.FileId!);

        // Assert
        result.Should().NotBeNull();
        result!.FileName.Should().Be("test.pdf");
    }

    [Fact]
    public async Task GetMetadataAsync_NonExistingFile_ReturnsNull()
    {
        // Arrange
        var service = CreateMockStorageService();

        // Act
        var result = await service.GetMetadataAsync("non-existing");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateMetadataAsync_ExistingFile_UpdatesMetadata()
    {
        // Arrange
        var service = CreateMockStorageService();
        var uploadResult = await service.UploadAsync(new byte[] { 1, 2, 3 }, "test.pdf");
        var newMetadata = new Dictionary<string, string> { { "NewKey", "NewValue" } };

        // Act
        var result = await service.UpdateMetadataAsync(uploadResult.FileId!, newMetadata);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region List Tests

    [Fact]
    public async Task ListByEntityAsync_HasFiles_ReturnsFiles()
    {
        // Arrange
        var service = CreateMockStorageService();
        await service.UploadAsync(new byte[] { 1 }, "file1.pdf", entityType: "Account", entityId: 1);
        await service.UploadAsync(new byte[] { 2 }, "file2.pdf", entityType: "Account", entityId: 1);

        // Act
        var result = await service.ListByEntityAsync("Account", 1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListByEntityAsync_NoFiles_ReturnsEmpty()
    {
        // Arrange
        var service = CreateMockStorageService();

        // Act
        var result = await service.ListByEntityAsync("Account", 999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTotalStorageUsedAsync_ReturnsSize()
    {
        // Arrange
        var service = CreateMockStorageService();
        await service.UploadAsync(new byte[1000], "file1.pdf");
        await service.UploadAsync(new byte[2000], "file2.pdf");

        // Act
        var result = await service.GetTotalStorageUsedAsync();

        // Assert
        result.Should().Be(3000);
    }

    #endregion

    #region Copy/Move Tests

    [Fact]
    public async Task CopyAsync_ExistingFile_CreatesACopy()
    {
        // Arrange
        var service = CreateMockStorageService();
        var uploadResult = await service.UploadAsync(new byte[] { 1, 2, 3 }, "original.pdf");

        // Act
        var result = await service.CopyAsync(uploadResult.FileId!, "copy.pdf");

        // Assert
        result.Success.Should().BeTrue();
        result.FileId.Should().NotBe(uploadResult.FileId);
    }

    [Fact]
    public async Task MoveAsync_ExistingFile_MovesFile()
    {
        // Arrange
        var service = CreateMockStorageService();
        var uploadResult = await service.UploadAsync(new byte[] { 1, 2, 3 }, "original.pdf");

        // Act
        var result = await service.MoveAsync(uploadResult.FileId!, "moved.pdf");

        // Assert
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Existence Tests

    [Fact]
    public async Task ExistsAsync_ExistingFile_ReturnsTrue()
    {
        // Arrange
        var service = CreateMockStorageService();
        var uploadResult = await service.UploadAsync(new byte[] { 1, 2, 3 }, "test.pdf");

        // Act
        var result = await service.ExistsAsync(uploadResult.FileId!);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingFile_ReturnsFalse()
    {
        // Arrange
        var service = CreateMockStorageService();

        // Act
        var result = await service.ExistsAsync("non-existing");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region URL Generation Tests

    [Fact]
    public async Task GetDownloadUrlAsync_ExistingFile_ReturnsUrl()
    {
        // Arrange
        var service = CreateMockStorageService();
        var uploadResult = await service.UploadAsync(new byte[] { 1, 2, 3 }, "test.pdf");

        // Act
        var result = await service.GetDownloadUrlAsync(uploadResult.FileId!);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetDownloadUrlAsync_WithExpiry_ReturnsTemporaryUrl()
    {
        // Arrange
        var service = CreateMockStorageService();
        var uploadResult = await service.UploadAsync(new byte[] { 1, 2, 3 }, "test.pdf");

        // Act
        var result = await service.GetDownloadUrlAsync(uploadResult.FileId!, TimeSpan.FromHours(1));

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("expires");
    }

    #endregion

    #region Helper Methods

    private MockStorageService CreateMockStorageService()
    {
        return new MockStorageService(_mockStorageSettings.Object, _mockLogger.Object);
    }

    #endregion
}

// Mock implementation for testing
public class MockStorageService : StorageService
{
    private readonly Dictionary<string, StoredFile> _files = new();
    private readonly StorageSettings _settings;

    public MockStorageService(IOptions<StorageSettings> settings, ILogger<StorageService> logger)
        : base(settings, logger)
    {
        _settings = settings.Value;
    }

    public override async Task<UploadResult> UploadAsync(byte[] content, string fileName, Dictionary<string, string>? metadata = null, string? entityType = null, int? entityId = null)
    {
        await Task.Delay(1);

        if (content == null || content.Length == 0)
            return new UploadResult { Success = false, ErrorMessage = "Empty content" };

        if (content.Length > _settings.MaxFileSizeBytes)
            return new UploadResult { Success = false, ErrorMessage = "File exceeds max size" };

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
            return new UploadResult { Success = false, ErrorMessage = "Missing extension" };

        if (!_settings.AllowedExtensions.Contains(extension))
            return new UploadResult { Success = false, ErrorMessage = "Invalid extension" };

        var fileId = Guid.NewGuid().ToString();
        _files[fileId] = new StoredFile
        {
            Content = content,
            FileName = fileName,
            Metadata = metadata ?? new Dictionary<string, string>(),
            EntityType = entityType,
            EntityId = entityId,
            CreatedAt = DateTime.UtcNow
        };

        return new UploadResult { Success = true, FileId = fileId, EntityType = entityType, EntityId = entityId };
    }

    public override async Task<DownloadResult> DownloadAsync(string fileId)
    {
        await Task.Delay(1);

        if (string.IsNullOrEmpty(fileId))
            return new DownloadResult { Success = false };

        if (!_files.TryGetValue(fileId, out var file))
            return new DownloadResult { Success = false };

        return new DownloadResult { Success = true, Content = file.Content, FileName = file.FileName };
    }

    public override async Task<bool> DeleteAsync(string fileId)
    {
        await Task.Delay(1);
        if (string.IsNullOrEmpty(fileId)) return false;
        return _files.Remove(fileId);
    }

    public override async Task<FileMetadata?> GetMetadataAsync(string fileId)
    {
        await Task.Delay(1);
        if (!_files.TryGetValue(fileId, out var file))
            return null;
        return new FileMetadata { FileName = file.FileName, Size = file.Content.Length, CustomMetadata = file.Metadata };
    }

    public override async Task<bool> UpdateMetadataAsync(string fileId, Dictionary<string, string> metadata)
    {
        await Task.Delay(1);
        if (!_files.TryGetValue(fileId, out var file))
            return false;
        file.Metadata = metadata;
        return true;
    }

    public override async Task<IEnumerable<FileMetadata>> ListByEntityAsync(string entityType, int entityId)
    {
        await Task.Delay(1);
        return _files.Values
            .Where(f => f.EntityType == entityType && f.EntityId == entityId)
            .Select(f => new FileMetadata { FileName = f.FileName, Size = f.Content.Length });
    }

    public override async Task<long> GetTotalStorageUsedAsync()
    {
        await Task.Delay(1);
        return _files.Values.Sum(f => f.Content.Length);
    }

    public override async Task<UploadResult> CopyAsync(string fileId, string newFileName)
    {
        await Task.Delay(1);
        if (!_files.TryGetValue(fileId, out var file))
            return new UploadResult { Success = false };
        return await UploadAsync(file.Content, newFileName);
    }

    public override async Task<UploadResult> MoveAsync(string fileId, string newFileName)
    {
        var copyResult = await CopyAsync(fileId, newFileName);
        if (copyResult.Success)
            await DeleteAsync(fileId);
        return copyResult;
    }

    public override async Task<bool> ExistsAsync(string fileId)
    {
        await Task.Delay(1);
        return _files.ContainsKey(fileId);
    }

    public override async Task<string?> GetDownloadUrlAsync(string fileId, TimeSpan? expiry = null)
    {
        await Task.Delay(1);
        if (!_files.ContainsKey(fileId))
            return null;
        var url = $"/api/files/{fileId}/download";
        if (expiry.HasValue)
            url += $"?expires={DateTime.UtcNow.Add(expiry.Value).Ticks}";
        return url;
    }

    private class StoredFile
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

// Supporting classes
public class StorageSettings
{
    public string BasePath { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; }
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    public bool UseSubdirectories { get; set; }
}

public class UploadResult
{
    public bool Success { get; set; }
    public string? FileId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
}

public class DownloadResult
{
    public bool Success { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
}

public class FileMetadata
{
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public Dictionary<string, string> CustomMetadata { get; set; } = new();
}

public abstract class StorageService
{
    protected readonly IOptions<StorageSettings> _settings;
    protected readonly ILogger<StorageService> _logger;

    protected StorageService(IOptions<StorageSettings> settings, ILogger<StorageService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public abstract Task<UploadResult> UploadAsync(byte[] content, string fileName, Dictionary<string, string>? metadata = null, string? entityType = null, int? entityId = null);
    public abstract Task<DownloadResult> DownloadAsync(string fileId);
    public abstract Task<bool> DeleteAsync(string fileId);
    public abstract Task<FileMetadata?> GetMetadataAsync(string fileId);
    public abstract Task<bool> UpdateMetadataAsync(string fileId, Dictionary<string, string> metadata);
    public abstract Task<IEnumerable<FileMetadata>> ListByEntityAsync(string entityType, int entityId);
    public abstract Task<long> GetTotalStorageUsedAsync();
    public abstract Task<UploadResult> CopyAsync(string fileId, string newFileName);
    public abstract Task<UploadResult> MoveAsync(string fileId, string newFileName);
    public abstract Task<bool> ExistsAsync(string fileId);
    public abstract Task<string?> GetDownloadUrlAsync(string fileId, TimeSpan? expiry = null);
}
