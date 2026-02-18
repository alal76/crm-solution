// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace CRM.Tests.Services;

public class BrandingConfigServiceTests
{
    private static CrmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var configuration = new ConfigurationBuilder().Build();
        return new CrmDbContext(options, configuration);
    }

    private static byte[] CreatePngBytes(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }

    private static BrandingConfigService CreateService(CrmDbContext context, Mock<IFileStorageService> fileStorage)
    {
        var logger = new Mock<ILogger<BrandingConfigService>>();
        return new BrandingConfigService(context, fileStorage.Object, logger.Object);
    }

    [Fact]
    public async Task GetCurrentBrandingAsync_ShouldCreateDefaultConfig()
    {
        var context = CreateContext();
        var fileStorage = new Mock<IFileStorageService>();
        var service = CreateService(context, fileStorage);

        var result = await service.GetCurrentBrandingAsync();

        result.SolutionName.Should().Be("CRM Solution");
        result.IsCustomBrandingEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateSolutionNameAsync_ShouldRejectInvalidCharacters()
    {
        var context = CreateContext();
        var fileStorage = new Mock<IFileStorageService>();
        var service = CreateService(context, fileStorage);

        Func<Task> act = async () => await service.UpdateSolutionNameAsync("CRM@2026", 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadCustomLogoAsync_ShouldReturnValidationError_ForInvalidMimeType()
    {
        var context = CreateContext();
        var fileStorage = new Mock<IFileStorageService>();
        var service = CreateService(context, fileStorage);

        var request = new UploadLogoRequest
        {
            FileContent = Convert.ToBase64String(CreatePngBytes(300, 300)),
            FileName = "logo.gif",
            MimeType = "image/gif",
            FileSizeBytes = 1024
        };

        var response = await service.UploadCustomLogoAsync(request, 1);

        response.Success.Should().BeFalse();
        response.ValidationErrors.Should().ContainKey("logo");
    }

    [Fact]
    public async Task UploadCustomLogoAsync_ShouldRejectInvalidDimensions()
    {
        var context = CreateContext();
        var fileStorage = new Mock<IFileStorageService>();
        var service = CreateService(context, fileStorage);

        var request = new UploadLogoRequest
        {
            FileContent = Convert.ToBase64String(CreatePngBytes(100, 100)),
            FileName = "logo.png",
            MimeType = "image/png",
            FileSizeBytes = 1024
        };

        var response = await service.UploadCustomLogoAsync(request, 1);

        response.Success.Should().BeFalse();
        response.ValidationErrors.Should().ContainKey("logo");
    }

    [Fact]
    public async Task UploadFaviconAsync_ShouldRejectInvalidDimensions()
    {
        var context = CreateContext();
        var fileStorage = new Mock<IFileStorageService>();
        var service = CreateService(context, fileStorage);

        var request = new UploadFaviconRequest
        {
            FileContent = Convert.ToBase64String(CreatePngBytes(48, 48)),
            FileName = "favicon.png",
            MimeType = "image/png",
            FileSizeBytes = 512
        };

        var response = await service.UploadFaviconAsync(request, 1);

        response.Success.Should().BeFalse();
        response.ValidationErrors.Should().ContainKey("favicon");
    }

    [Fact]
    public async Task SetCustomBrandingEnabledAsync_ShouldUpdateFlag()
    {
        var context = CreateContext();
        var fileStorage = new Mock<IFileStorageService>();
        var service = CreateService(context, fileStorage);

        var result = await service.SetCustomBrandingEnabledAsync(false, 1);

        result.IsCustomBrandingEnabled.Should().BeFalse();
    }
}
