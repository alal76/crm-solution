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

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for UserProfilesController
/// Covers: Profile CRUD, preferences, settings, avatar
/// </summary>
public class UserProfilesControllerTests
{
    private readonly Mock<IUserProfileService> _mockProfileService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<UserProfilesController>> _mockLogger;
    private readonly UserProfilesController _controller;

    public UserProfilesControllerTests()
    {
        _mockProfileService = new Mock<IUserProfileService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<UserProfilesController>>();

        _controller = new UserProfilesController(
            _mockProfileService.Object,
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

    #region GetCurrentProfile Tests

    [Fact]
    public async Task GetCurrentProfile_ReturnsOkWithProfile()
    {
        // Arrange
        var profile = new UserProfileDto
        {
            Id = 1,
            UserId = 1,
            Theme = "light",
            Language = "en-US",
            TimeZone = "America/New_York"
        };

        _mockProfileService.Setup(s => s.GetByUserIdAsync(1))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetCurrentProfile();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProfile = okResult.Value.Should().BeOfType<UserProfileDto>().Subject;
        returnedProfile.UserId.Should().Be(1);
    }

    [Fact]
    public async Task GetCurrentProfile_NoProfile_ReturnsNotFound()
    {
        // Arrange
        _mockProfileService.Setup(s => s.GetByUserIdAsync(1))
            .ReturnsAsync((UserProfileDto?)null);

        // Act
        var result = await _controller.GetCurrentProfile();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingProfile_ReturnsOk()
    {
        // Arrange
        var profile = new UserProfileDto
        {
            Id = 1,
            UserId = 5,
            Theme = "dark",
            NotificationsEnabled = true
        };

        _mockProfileService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProfile = okResult.Value.Should().BeOfType<UserProfileDto>().Subject;
        returnedProfile.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingProfile_ReturnsNotFound()
    {
        // Arrange
        _mockProfileService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((UserProfileDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Update Profile Tests

    [Fact]
    public async Task UpdateProfile_ValidProfile_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateUserProfileDto
        {
            Theme = "dark",
            Language = "es-ES",
            TimeZone = "Europe/Madrid"
        };

        var updatedProfile = new UserProfileDto
        {
            Id = 1,
            Theme = "dark",
            Language = "es-ES",
            TimeZone = "Europe/Madrid"
        };

        _mockProfileService.Setup(s => s.UpdateAsync(1, updateDto))
            .ReturnsAsync(updatedProfile);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateProfile(updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProfile = okResult.Value.Should().BeOfType<UserProfileDto>().Subject;
        returnedProfile.Theme.Should().Be("dark");
    }

    [Fact]
    public async Task UpdateProfile_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UpdateProfile(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Theme Preferences Tests

    [Fact]
    public async Task UpdateTheme_ValidTheme_ReturnsOk()
    {
        // Arrange
        var themeRequest = new UpdateThemeDto { Theme = "dark" };

        _mockProfileService.Setup(s => s.UpdateThemeAsync(1, "dark"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateTheme(themeRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task UpdateTheme_InvalidTheme_ReturnsBadRequest()
    {
        // Arrange
        var themeRequest = new UpdateThemeDto { Theme = "invalid" };

        _mockProfileService.Setup(s => s.UpdateThemeAsync(1, "invalid"))
            .ThrowsAsync(new ArgumentException("Invalid theme"));

        // Act
        var result = await _controller.UpdateTheme(themeRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAvailableThemes_ReturnsThemes()
    {
        // Arrange
        var themes = new List<ThemeDto>
        {
            new ThemeDto { Id = "light", Name = "Light Theme" },
            new ThemeDto { Id = "dark", Name = "Dark Theme" },
            new ThemeDto { Id = "system", Name = "System Default" }
        };

        _mockProfileService.Setup(s => s.GetAvailableThemesAsync())
            .ReturnsAsync(themes);

        // Act
        var result = await _controller.GetAvailableThemes();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedThemes = okResult.Value.Should().BeAssignableTo<IEnumerable<ThemeDto>>().Subject;
        returnedThemes.Should().HaveCount(3);
    }

    #endregion

    #region Language Preferences Tests

    [Fact]
    public async Task UpdateLanguage_ValidLanguage_ReturnsOk()
    {
        // Arrange
        var languageRequest = new UpdateLanguageDto { Language = "es-ES" };

        _mockProfileService.Setup(s => s.UpdateLanguageAsync(1, "es-ES"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateLanguage(languageRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetAvailableLanguages_ReturnsLanguages()
    {
        // Arrange
        var languages = new List<LanguageDto>
        {
            new LanguageDto { Code = "en-US", Name = "English (US)" },
            new LanguageDto { Code = "es-ES", Name = "Spanish (Spain)" },
            new LanguageDto { Code = "fr-FR", Name = "French (France)" }
        };

        _mockProfileService.Setup(s => s.GetAvailableLanguagesAsync())
            .ReturnsAsync(languages);

        // Act
        var result = await _controller.GetAvailableLanguages();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedLanguages = okResult.Value.Should().BeAssignableTo<IEnumerable<LanguageDto>>().Subject;
        returnedLanguages.Should().HaveCount(3);
    }

    #endregion

    #region Timezone Preferences Tests

    [Fact]
    public async Task UpdateTimeZone_ValidTimeZone_ReturnsOk()
    {
        // Arrange
        var timeZoneRequest = new UpdateTimeZoneDto { TimeZone = "America/Los_Angeles" };

        _mockProfileService.Setup(s => s.UpdateTimeZoneAsync(1, "America/Los_Angeles"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateTimeZone(timeZoneRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetAvailableTimeZones_ReturnsTimeZones()
    {
        // Arrange
        var timeZones = new List<TimeZoneDto>
        {
            new TimeZoneDto { Id = "America/New_York", DisplayName = "Eastern Time (US & Canada)" },
            new TimeZoneDto { Id = "America/Los_Angeles", DisplayName = "Pacific Time (US & Canada)" }
        };

        _mockProfileService.Setup(s => s.GetAvailableTimeZonesAsync())
            .ReturnsAsync(timeZones);

        // Act
        var result = await _controller.GetAvailableTimeZones();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<TimeZoneDto>>();
    }

    #endregion

    #region Notification Preferences Tests

    [Fact]
    public async Task GetNotificationPreferences_ReturnsPreferences()
    {
        // Arrange
        var preferences = new NotificationPreferencesDto
        {
            EmailEnabled = true,
            PushEnabled = true,
            InAppEnabled = true,
            DailyDigest = false
        };

        _mockProfileService.Setup(s => s.GetNotificationPreferencesAsync(1))
            .ReturnsAsync(preferences);

        // Act
        var result = await _controller.GetNotificationPreferences();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPrefs = okResult.Value.Should().BeOfType<NotificationPreferencesDto>().Subject;
        returnedPrefs.EmailEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateNotificationPreferences_ValidPreferences_ReturnsOk()
    {
        // Arrange
        var preferences = new UpdateNotificationPreferencesDto
        {
            EmailEnabled = true,
            PushEnabled = false,
            InAppEnabled = true
        };

        _mockProfileService.Setup(s => s.UpdateNotificationPreferencesAsync(1, preferences))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateNotificationPreferences(preferences);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ToggleEmailNotifications_ReturnsOk()
    {
        // Arrange
        _mockProfileService.Setup(s => s.ToggleEmailNotificationsAsync(1, false))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ToggleEmailNotifications(false);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task TogglePushNotifications_ReturnsOk()
    {
        // Arrange
        _mockProfileService.Setup(s => s.TogglePushNotificationsAsync(1, true))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TogglePushNotifications(true);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Avatar Tests

    [Fact]
    public async Task UploadAvatar_ValidImage_ReturnsOk()
    {
        // Arrange
        var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(imageData.Length);
        fileMock.Setup(f => f.ContentType).Returns("image/png");

        _mockProfileService.Setup(s => s.UploadAvatarAsync(1, fileMock.Object))
            .ReturnsAsync("https://storage.example.com/avatars/1.png");

        // Act
        var result = await _controller.UploadAvatar(fileMock.Object);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { AvatarUrl = "https://storage.example.com/avatars/1.png" });
    }

    [Fact]
    public async Task UploadAvatar_InvalidFormat_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        _mockProfileService.Setup(s => s.UploadAvatarAsync(1, fileMock.Object))
            .ThrowsAsync(new ArgumentException("Invalid file format"));

        // Act
        var result = await _controller.UploadAvatar(fileMock.Object);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadAvatar_FileTooLarge_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(10 * 1024 * 1024); // 10MB

        _mockProfileService.Setup(s => s.UploadAvatarAsync(1, fileMock.Object))
            .ThrowsAsync(new ArgumentException("File size exceeds limit"));

        // Act
        var result = await _controller.UploadAvatar(fileMock.Object);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteAvatar_ReturnsOk()
    {
        // Arrange
        _mockProfileService.Setup(s => s.DeleteAvatarAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAvatar();

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Dashboard Preferences Tests

    [Fact]
    public async Task GetDashboardPreferences_ReturnsPreferences()
    {
        // Arrange
        var preferences = new DashboardPreferencesDto
        {
            DefaultDashboardId = 1,
            WidgetLayout = "grid",
            RefreshInterval = 30
        };

        _mockProfileService.Setup(s => s.GetDashboardPreferencesAsync(1))
            .ReturnsAsync(preferences);

        // Act
        var result = await _controller.GetDashboardPreferences();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<DashboardPreferencesDto>();
    }

    [Fact]
    public async Task UpdateDashboardPreferences_ValidPreferences_ReturnsOk()
    {
        // Arrange
        var preferences = new UpdateDashboardPreferencesDto
        {
            DefaultDashboardId = 2,
            WidgetLayout = "list",
            RefreshInterval = 60
        };

        _mockProfileService.Setup(s => s.UpdateDashboardPreferencesAsync(1, preferences))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateDashboardPreferences(preferences);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Calendar Preferences Tests

    [Fact]
    public async Task GetCalendarPreferences_ReturnsPreferences()
    {
        // Arrange
        var preferences = new CalendarPreferencesDto
        {
            DefaultView = "week",
            StartOfWeek = DayOfWeek.Monday,
            WorkingHoursStart = "09:00",
            WorkingHoursEnd = "17:00"
        };

        _mockProfileService.Setup(s => s.GetCalendarPreferencesAsync(1))
            .ReturnsAsync(preferences);

        // Act
        var result = await _controller.GetCalendarPreferences();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<CalendarPreferencesDto>();
    }

    [Fact]
    public async Task UpdateCalendarPreferences_ValidPreferences_ReturnsOk()
    {
        // Arrange
        var preferences = new UpdateCalendarPreferencesDto
        {
            DefaultView = "month",
            StartOfWeek = DayOfWeek.Sunday
        };

        _mockProfileService.Setup(s => s.UpdateCalendarPreferencesAsync(1, preferences))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateCalendarPreferences(preferences);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Reset Preferences Tests

    [Fact]
    public async Task ResetToDefaults_ReturnsOk()
    {
        // Arrange
        _mockProfileService.Setup(s => s.ResetToDefaultsAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetToDefaults();

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ResetNotificationPreferences_ReturnsOk()
    {
        // Arrange
        _mockProfileService.Setup(s => s.ResetNotificationPreferencesAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetNotificationPreferences();

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Recent Items Tests

    [Fact]
    public async Task GetRecentItems_ReturnsRecentItems()
    {
        // Arrange
        var recentItems = new List<RecentItemDto>
        {
            new RecentItemDto { EntityType = "Account", EntityId = 1, Name = "Acme Corp" },
            new RecentItemDto { EntityType = "Contact", EntityId = 5, Name = "John Doe" }
        };

        _mockProfileService.Setup(s => s.GetRecentItemsAsync(1, 10))
            .ReturnsAsync(recentItems);

        // Act
        var result = await _controller.GetRecentItems(10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value.Should().BeAssignableTo<IEnumerable<RecentItemDto>>().Subject;
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddRecentItem_ValidItem_ReturnsOk()
    {
        // Arrange
        var item = new AddRecentItemDto
        {
            EntityType = "Account",
            EntityId = 1,
            Name = "Acme Corp"
        };

        _mockProfileService.Setup(s => s.AddRecentItemAsync(1, item))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AddRecentItem(item);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ClearRecentItems_ReturnsOk()
    {
        // Arrange
        _mockProfileService.Setup(s => s.ClearRecentItemsAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ClearRecentItems();

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Favorites Tests

    [Fact]
    public async Task GetFavorites_ReturnsFavorites()
    {
        // Arrange
        var favorites = new List<FavoriteItemDto>
        {
            new FavoriteItemDto { EntityType = "Account", EntityId = 1, Name = "Acme Corp" },
            new FavoriteItemDto { EntityType = "Report", EntityId = 5, Name = "Sales Report" }
        };

        _mockProfileService.Setup(s => s.GetFavoritesAsync(1))
            .ReturnsAsync(favorites);

        // Act
        var result = await _controller.GetFavorites();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value.Should().BeAssignableTo<IEnumerable<FavoriteItemDto>>().Subject;
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddFavorite_ValidItem_ReturnsOk()
    {
        // Arrange
        var item = new AddFavoriteDto
        {
            EntityType = "Account",
            EntityId = 1
        };

        _mockProfileService.Setup(s => s.AddFavoriteAsync(1, item))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AddFavorite(item);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RemoveFavorite_ValidItem_ReturnsOk()
    {
        // Arrange
        _mockProfileService.Setup(s => s.RemoveFavoriteAsync(1, "Account", 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveFavorite("Account", 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion
}
