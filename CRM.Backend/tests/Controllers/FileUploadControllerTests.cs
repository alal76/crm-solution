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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for FileUploadController
/// Covers: File uploads, downloads, validation, storage
/// </summary>
public class FileUploadControllerTests
{
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<FileUploadController>> _mockLogger;
    private readonly FileUploadController _controller;

    public FileUploadControllerTests()
    {
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<FileUploadController>>();

        _controller = new FileUploadController(
            _mockFileStorageService.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);

        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    private Mock<IFormFile> CreateMockFile(string fileName, string contentType, int size = 1024)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(size);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[size]));
        return fileMock;
    }

    #region Upload Tests

    [Fact]
    public async Task Upload_ValidFile_ReturnsOkWithFileInfo()
    {
        // Arrange
        var fileMock = CreateMockFile("document.pdf", "application/pdf");

        var uploadResult = new FileUploadResultDto
        {
            FileId = "file-123",
            FileName = "document.pdf",
            FileSize = 1024,
            ContentType = "application/pdf",
            Url = "https://storage.example.com/files/file-123"
        };

        _mockFileStorageService.Setup(s => s.UploadAsync(fileMock.Object, null))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _controller.Upload(fileMock.Object);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFile = okResult.Value.Should().BeOfType<FileUploadResultDto>().Subject;
        returnedFile.FileId.Should().Be("file-123");
    }

    [Fact]
    public async Task Upload_NoFile_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Upload(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Upload_EmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = CreateMockFile("empty.pdf", "application/pdf", 0);

        // Act
        var result = await _controller.Upload(fileMock.Object);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Upload_FileTooLarge_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = CreateMockFile("large.pdf", "application/pdf", 100 * 1024 * 1024);

        _mockFileStorageService.Setup(s => s.UploadAsync(fileMock.Object, null))
            .ThrowsAsync(new ArgumentException("File size exceeds maximum allowed"));

        // Act
        var result = await _controller.Upload(fileMock.Object);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Upload_InvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = CreateMockFile("virus.exe", "application/x-executable");

        _mockFileStorageService.Setup(s => s.UploadAsync(fileMock.Object, null))
            .ThrowsAsync(new ArgumentException("File type not allowed"));

        // Act
        var result = await _controller.Upload(fileMock.Object);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Upload_WithEntityAssociation_ReturnsOkWithFileInfo()
    {
        // Arrange
        var fileMock = CreateMockFile("contract.pdf", "application/pdf");

        var uploadResult = new FileUploadResultDto
        {
            FileId = "file-456",
            FileName = "contract.pdf",
            EntityType = "Account",
            EntityId = 1
        };

        _mockFileStorageService.Setup(s => s.UploadAsync(fileMock.Object, It.IsAny<FileAssociationDto>()))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _controller.Upload(fileMock.Object, "Account", 1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFile = okResult.Value.Should().BeOfType<FileUploadResultDto>().Subject;
        returnedFile.EntityType.Should().Be("Account");
    }

    #endregion

    #region Multiple Upload Tests

    [Fact]
    public async Task UploadMultiple_ValidFiles_ReturnsOkWithResults()
    {
        // Arrange
        var files = new List<IFormFile>
        {
            CreateMockFile("file1.pdf", "application/pdf").Object,
            CreateMockFile("file2.pdf", "application/pdf").Object
        };

        var results = new List<FileUploadResultDto>
        {
            new FileUploadResultDto { FileId = "file-1", FileName = "file1.pdf" },
            new FileUploadResultDto { FileId = "file-2", FileName = "file2.pdf" }
        };

        _mockFileStorageService.Setup(s => s.UploadMultipleAsync(files, null))
            .ReturnsAsync(results);

        // Act
        var result = await _controller.UploadMultiple(files);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResults = okResult.Value.Should().BeAssignableTo<IEnumerable<FileUploadResultDto>>().Subject;
        returnedResults.Should().HaveCount(2);
    }

    [Fact]
    public async Task UploadMultiple_EmptyList_ReturnsBadRequest()
    {
        // Arrange
        var files = new List<IFormFile>();

        // Act
        var result = await _controller.UploadMultiple(files);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadMultiple_TooManyFiles_ReturnsBadRequest()
    {
        // Arrange
        var files = new List<IFormFile>();
        for (int i = 0; i < 50; i++)
        {
            files.Add(CreateMockFile($"file{i}.pdf", "application/pdf").Object);
        }

        _mockFileStorageService.Setup(s => s.UploadMultipleAsync(files, null))
            .ThrowsAsync(new ArgumentException("Too many files in single upload"));

        // Act
        var result = await _controller.UploadMultiple(files);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Download Tests

    [Fact]
    public async Task Download_ExistingFile_ReturnsFile()
    {
        // Arrange
        var fileContent = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
        var fileInfo = new FileDownloadDto
        {
            Content = fileContent,
            ContentType = "application/pdf",
            FileName = "document.pdf"
        };

        _mockFileStorageService.Setup(s => s.DownloadAsync("file-123"))
            .ReturnsAsync(fileInfo);

        // Act
        var result = await _controller.Download("file-123");

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Be("document.pdf");
    }

    [Fact]
    public async Task Download_NonExistingFile_ReturnsNotFound()
    {
        // Arrange
        _mockFileStorageService.Setup(s => s.DownloadAsync("invalid-id"))
            .ReturnsAsync((FileDownloadDto?)null);

        // Act
        var result = await _controller.Download("invalid-id");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Get File Info Tests

    [Fact]
    public async Task GetFileInfo_ExistingFile_ReturnsOk()
    {
        // Arrange
        var fileInfo = new FileInfoDto
        {
            FileId = "file-123",
            FileName = "document.pdf",
            FileSize = 1024,
            ContentType = "application/pdf",
            UploadedAt = DateTime.UtcNow,
            UploadedBy = "user@example.com"
        };

        _mockFileStorageService.Setup(s => s.GetFileInfoAsync("file-123"))
            .ReturnsAsync(fileInfo);

        // Act
        var result = await _controller.GetFileInfo("file-123");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedInfo = okResult.Value.Should().BeOfType<FileInfoDto>().Subject;
        returnedInfo.FileId.Should().Be("file-123");
    }

    [Fact]
    public async Task GetFileInfo_NonExistingFile_ReturnsNotFound()
    {
        // Arrange
        _mockFileStorageService.Setup(s => s.GetFileInfoAsync("invalid-id"))
            .ReturnsAsync((FileInfoDto?)null);

        // Act
        var result = await _controller.GetFileInfo("invalid-id");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Get Files By Entity Tests

    [Fact]
    public async Task GetFilesByEntity_ReturnsFiles()
    {
        // Arrange
        var files = new List<FileInfoDto>
        {
            new FileInfoDto { FileId = "file-1", FileName = "contract.pdf" },
            new FileInfoDto { FileId = "file-2", FileName = "proposal.docx" }
        };

        _mockFileStorageService.Setup(s => s.GetFilesByEntityAsync("Account", 1))
            .ReturnsAsync(files);

        // Act
        var result = await _controller.GetFilesByEntity("Account", 1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFiles = okResult.Value.Should().BeAssignableTo<IEnumerable<FileInfoDto>>().Subject;
        returnedFiles.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFilesByEntity_NoFiles_ReturnsEmptyList()
    {
        // Arrange
        _mockFileStorageService.Setup(s => s.GetFilesByEntityAsync("Account", 999))
            .ReturnsAsync(new List<FileInfoDto>());

        // Act
        var result = await _controller.GetFilesByEntity("Account", 999);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedFiles = okResult.Value.Should().BeAssignableTo<IEnumerable<FileInfoDto>>().Subject;
        returnedFiles.Should().BeEmpty();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingFile_ReturnsNoContent()
    {
        // Arrange
        _mockFileStorageService.Setup(s => s.DeleteAsync("file-123"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete("file-123");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingFile_ReturnsNotFound()
    {
        // Arrange
        _mockFileStorageService.Setup(s => s.DeleteAsync("invalid-id"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete("invalid-id");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task BulkDelete_ValidIds_ReturnsOkWithCount()
    {
        // Arrange
        var fileIds = new List<string> { "file-1", "file-2", "file-3" };

        _mockFileStorageService.Setup(s => s.BulkDeleteAsync(fileIds))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDelete(fileIds);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { DeletedCount = 3 });
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateFile_ValidFile_ReturnsOk()
    {
        // Arrange
        var fileMock = CreateMockFile("valid.pdf", "application/pdf");

        _mockFileStorageService.Setup(s => s.ValidateFileAsync(fileMock.Object))
            .ReturnsAsync(new FileValidationResultDto { IsValid = true });

        // Act
        var result = await _controller.ValidateFile(fileMock.Object);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var validationResult = okResult.Value.Should().BeOfType<FileValidationResultDto>().Subject;
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateFile_InvalidFile_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = CreateMockFile("infected.exe", "application/x-executable");

        _mockFileStorageService.Setup(s => s.ValidateFileAsync(fileMock.Object))
            .ReturnsAsync(new FileValidationResultDto
            {
                IsValid = false,
                Errors = new List<string> { "File type not allowed", "Potential security risk" }
            });

        // Act
        var result = await _controller.ValidateFile(fileMock.Object);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeOfType<FileValidationResultDto>();
    }

    #endregion

    #region Get Allowed Types Tests

    [Fact]
    public async Task GetAllowedTypes_ReturnsAllowedTypes()
    {
        // Arrange
        var allowedTypes = new AllowedFileTypesDto
        {
            Extensions = new List<string> { ".pdf", ".doc", ".docx", ".jpg", ".png" },
            MimeTypes = new List<string> { "application/pdf", "image/jpeg", "image/png" },
            MaxFileSize = 10 * 1024 * 1024
        };

        _mockFileStorageService.Setup(s => s.GetAllowedTypesAsync())
            .ReturnsAsync(allowedTypes);

        // Act
        var result = await _controller.GetAllowedTypes();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var types = okResult.Value.Should().BeOfType<AllowedFileTypesDto>().Subject;
        types.Extensions.Should().Contain(".pdf");
    }

    #endregion

    #region Image Operations Tests

    [Fact]
    public async Task UploadImage_ValidImage_ReturnsOkWithUrls()
    {
        // Arrange
        var fileMock = CreateMockFile("photo.jpg", "image/jpeg");

        var uploadResult = new ImageUploadResultDto
        {
            FileId = "img-123",
            OriginalUrl = "https://storage.example.com/images/img-123.jpg",
            ThumbnailUrl = "https://storage.example.com/images/img-123-thumb.jpg"
        };

        _mockFileStorageService.Setup(s => s.UploadImageAsync(fileMock.Object, It.IsAny<ImageUploadOptionsDto>()))
            .ReturnsAsync(uploadResult);

        // Act
        var result = await _controller.UploadImage(fileMock.Object);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value.Should().BeOfType<ImageUploadResultDto>().Subject;
        returnedResult.ThumbnailUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UploadImage_NonImageFile_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = CreateMockFile("document.pdf", "application/pdf");

        _mockFileStorageService.Setup(s => s.UploadImageAsync(fileMock.Object, It.IsAny<ImageUploadOptionsDto>()))
            .ThrowsAsync(new ArgumentException("File is not a valid image"));

        // Act
        var result = await _controller.UploadImage(fileMock.Object);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetThumbnail_ExistingImage_ReturnsImage()
    {
        // Arrange
        var thumbnailData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header

        _mockFileStorageService.Setup(s => s.GetThumbnailAsync("img-123"))
            .ReturnsAsync(new FileDownloadDto
            {
                Content = thumbnailData,
                ContentType = "image/jpeg",
                FileName = "thumbnail.jpg"
            });

        // Act
        var result = await _controller.GetThumbnail("img-123");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    #endregion

    #region Storage Statistics Tests

    [Fact]
    public async Task GetStorageStatistics_ReturnsStats()
    {
        // Arrange
        var stats = new StorageStatisticsDto
        {
            TotalFiles = 500,
            TotalSize = 1024 * 1024 * 1024, // 1 GB
            UsedQuota = 50,
            FilesByType = new Dictionary<string, int>
            {
                { "pdf", 200 },
                { "docx", 150 },
                { "jpg", 100 },
                { "other", 50 }
            }
        };

        _mockFileStorageService.Setup(s => s.GetStorageStatisticsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStorageStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<StorageStatisticsDto>().Subject;
        returnedStats.TotalFiles.Should().Be(500);
    }

    [Fact]
    public async Task GetUserStorageUsage_ReturnsUserUsage()
    {
        // Arrange
        var usage = new UserStorageUsageDto
        {
            UserId = 1,
            TotalFiles = 50,
            TotalSize = 100 * 1024 * 1024,
            QuotaRemaining = 900 * 1024 * 1024
        };

        _mockFileStorageService.Setup(s => s.GetUserStorageUsageAsync(1))
            .ReturnsAsync(usage);

        // Act
        var result = await _controller.GetUserStorageUsage();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<UserStorageUsageDto>();
    }

    #endregion

    #region Cleanup Tests

    [Fact]
    public async Task CleanupOrphanedFiles_ReturnsDeletedCount()
    {
        // Arrange
        _mockFileStorageService.Setup(s => s.CleanupOrphanedFilesAsync())
            .ReturnsAsync(10);

        // Act
        var result = await _controller.CleanupOrphanedFiles();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { DeletedCount = 10 });
    }

    #endregion
}
