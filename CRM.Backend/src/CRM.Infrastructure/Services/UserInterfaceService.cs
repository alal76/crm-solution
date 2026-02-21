// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of user interface management service
/// Manages UI preferences, customizations, and dashboard layouts
/// </summary>
public class UserInterfaceService : IUserInterfaceService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<UserInterfaceService> _logger;

    public UserInterfaceService(ICrmDbContext dbContext, ILogger<UserInterfaceService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UIPreferenceDto?> GetUserUIPreferencesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var preference = await _dbContext.UIPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, cancellationToken);

        if (preference == null)
            return null;

        return MapToDto(preference);
    }

    public async Task<UIPreferenceDto> SaveUIPreferencesAsync(int userId, CreateUpdateUIPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        var preference = await _dbContext.UIPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, cancellationToken);

        if (preference == null)
        {
            preference = new UIPreference { UserId = userId, CreatedAt = DateTime.UtcNow };
            _dbContext.UIPreferences.Add(preference);
        }

        // Update only provided properties
        if (dto.Theme != null)
            preference.Theme = dto.Theme;
        if (dto.SidebarPosition != null)
            preference.SidebarPosition = dto.SidebarPosition;
        if (dto.SidebarWidth.HasValue)
            preference.SidebarWidth = dto.SidebarWidth.Value;
        if (dto.FontSize != null)
            preference.FontSize = dto.FontSize;
        if (dto.ShowBreadcrumbs.HasValue)
            preference.ShowBreadcrumbs = dto.ShowBreadcrumbs.Value;
        if (dto.ShowStatusBar.HasValue)
            preference.ShowStatusBar = dto.ShowStatusBar.Value;
        if (dto.ShowTopNavigation.HasValue)
            preference.ShowTopNavigation = dto.ShowTopNavigation.Value;
        if (dto.DefaultPageSize.HasValue)
            preference.DefaultPageSize = dto.DefaultPageSize.Value;
        if (dto.DateFormat != null)
            preference.DateFormat = dto.DateFormat;
        if (dto.TimeFormat != null)
            preference.TimeFormat = dto.TimeFormat;
        if (dto.CustomColorScheme != null)
            preference.CustomColorScheme = dto.CustomColorScheme;

        preference.UpdatedAt = DateTime.UtcNow;
        preference.LastPreferenceUpdate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("UI preferences saved for user {UserId}", userId);

        return MapToDto(preference);
    }

    public async Task<bool> ResetUIPreferencesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var preference = await _dbContext.UIPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, cancellationToken);

        if (preference == null)
            return true;

        preference.Theme = "auto";
        preference.SidebarPosition = "left";
        preference.SidebarWidth = 250;
        preference.FontSize = "normal";
        preference.ShowBreadcrumbs = true;
        preference.ShowStatusBar = true;
        preference.ShowTopNavigation = true;
        preference.DefaultPageSize = 20;
        preference.DateFormat = "MM/dd/yyyy";
        preference.TimeFormat = "hh:mm a";
        preference.CustomColorScheme = null;
        preference.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("UI preferences reset for user {UserId}", userId);

        return true;
    }

    public async Task<UICustomizationDto?> GetUICustomizationAsync(int userId, string moduleName, string pageName, CancellationToken cancellationToken = default)
    {
        var customization = await _dbContext.UICustomizations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ModuleName == moduleName && c.PageName == pageName && !c.IsDeleted, cancellationToken);

        return customization == null ? null : MapToDto(customization);
    }

    public async Task<IEnumerable<UICustomizationDto>> GetAllUICustomizationsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var customizations = await _dbContext.UICustomizations
            .AsNoTracking()
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        return customizations.Select(MapToDto).ToList();
    }

    public async Task<UICustomizationDto> SaveUICustomizationAsync(int userId, CreateUpdateUICustomizationDto dto, CancellationToken cancellationToken = default)
    {
        var customization = await _dbContext.UICustomizations
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ModuleName == dto.ModuleName && c.PageName == dto.PageName && !c.IsDeleted, cancellationToken);

        if (customization == null)
        {
            customization = new UICustomization
            {
                UserId = userId,
                ModuleName = dto.ModuleName,
                PageName = dto.PageName,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.UICustomizations.Add(customization);
        }

        if (dto.VisibleColumns != null)
            customization.VisibleColumns = string.Join(",", dto.VisibleColumns);
        if (dto.DefaultSortColumn != null)
            customization.DefaultSortColumn = dto.DefaultSortColumn;
        if (dto.DefaultSortOrder != null)
            customization.DefaultSortOrder = dto.DefaultSortOrder;
        if (dto.StoredFilters != null)
            customization.StoredFilters = JsonConvert.SerializeObject(dto.StoredFilters);
        if (dto.SavedSearches != null)
            customization.SavedSearches = JsonConvert.SerializeObject(dto.SavedSearches);
        if (dto.RowHeight != null)
            customization.RowHeight = dto.RowHeight;
        if (dto.ShowRowNumbers.HasValue)
            customization.ShowRowNumbers = dto.ShowRowNumbers.Value;
        if (dto.ShowFilters.HasValue)
            customization.ShowFilters = dto.ShowFilters.Value;
        if (dto.ColumnWidths != null)
            customization.ColumnWidths = JsonConvert.SerializeObject(dto.ColumnWidths);
        if (dto.RowsPerPage.HasValue)
            customization.RowsPerPage = dto.RowsPerPage.Value;

        customization.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("UI customization saved for user {UserId}, module {Module}", userId, dto.ModuleName);

        return MapToDto(customization);
    }

    public async Task<bool> DeleteUICustomizationAsync(int userId, string moduleName, string pageName, CancellationToken cancellationToken = default)
    {
        var customization = await _dbContext.UICustomizations
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ModuleName == moduleName && c.PageName == pageName && !c.IsDeleted, cancellationToken);

        if (customization == null)
            return true;

        customization.IsDeleted = true;
        customization.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("UI customization deleted for user {UserId}, module {Module}", userId, moduleName);

        return true;
    }

    public async Task<DashboardCustomizationDto?> GetDashboardCustomizationAsync(int userId, string dashboardName, CancellationToken cancellationToken = default)
    {
        var dashboard = await _dbContext.DashboardCustomizations
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DashboardName == dashboardName && !d.IsDeleted, cancellationToken);

        return dashboard == null ? null : MapToDto(dashboard);
    }

    public async Task<IEnumerable<DashboardCustomizationDto>> GetAllDashboardCustomizationsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var dashboards = await _dbContext.DashboardCustomizations
            .AsNoTracking()
            .Where(d => d.UserId == userId && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        return dashboards.Select(MapToDto).ToList();
    }

    public async Task<DashboardCustomizationDto> SaveDashboardCustomizationAsync(int userId, CreateUpdateDashboardCustomizationDto dto, CancellationToken cancellationToken = default)
    {
        var dashboard = await _dbContext.DashboardCustomizations
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DashboardName == dto.DashboardName && !d.IsDeleted, cancellationToken);

        if (dashboard == null)
        {
            dashboard = new DashboardCustomization
            {
                UserId = userId,
                DashboardName = dto.DashboardName,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.DashboardCustomizations.Add(dashboard);
        }

        if (dto.LayoutConfig != null)
            dashboard.LayoutConfig = JsonConvert.SerializeObject(dto.LayoutConfig);
        if (dto.Widgets != null)
            dashboard.Widgets = JsonConvert.SerializeObject(dto.Widgets);
        if (dto.IsDefault.HasValue)
        {
            if (dto.IsDefault.Value)
            {
                // Clear other defaults for this user
                var others = await _dbContext.DashboardCustomizations
                    .Where(d => d.UserId == userId && d.IsDefault && !d.IsDeleted)
                    .ToListAsync(cancellationToken);
                foreach (var other in others)
                    other.IsDefault = false;
            }
            dashboard.IsDefault = dto.IsDefault.Value;
        }
        if (dto.GridColumns.HasValue)
            dashboard.GridColumns = dto.GridColumns.Value;
        if (dto.AutoRefresh.HasValue)
            dashboard.AutoRefresh = dto.AutoRefresh.Value;
        if (dto.RefreshIntervalSeconds.HasValue)
            dashboard.RefreshIntervalSeconds = dto.RefreshIntervalSeconds.Value;

        dashboard.LastModified = DateTime.UtcNow;
        dashboard.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Dashboard customization saved for user {UserId}: {DashboardName}", userId, dto.DashboardName);

        return MapToDto(dashboard);
    }

    public async Task<bool> DeleteDashboardCustomizationAsync(int userId, string dashboardName, CancellationToken cancellationToken = default)
    {
        var dashboard = await _dbContext.DashboardCustomizations
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DashboardName == dashboardName && !d.IsDeleted, cancellationToken);

        if (dashboard == null)
            return true;

        dashboard.IsDeleted = true;
        dashboard.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Dashboard deleted for user {UserId}: {DashboardName}", userId, dashboardName);

        return true;
    }

    public async Task<bool> SetDefaultDashboardAsync(int userId, string dashboardName, CancellationToken cancellationToken = default)
    {
        var dashboard = await _dbContext.DashboardCustomizations
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DashboardName == dashboardName && !d.IsDeleted, cancellationToken);

        if (dashboard == null)
            return false;

        var others = await _dbContext.DashboardCustomizations
            .Where(d => d.UserId == userId && d.IsDefault && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var other in others)
            other.IsDefault = false;

        dashboard.IsDefault = true;
        dashboard.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<string[]> GetSavedViewsAsync(int userId, string moduleName, CancellationToken cancellationToken = default)
    {
        var customization = await _dbContext.UICustomizations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ModuleName == moduleName && !c.IsDeleted, cancellationToken);

        if (customization?.SavedSearches == null)
            return Array.Empty<string>();

        try
        {
            return JsonConvert.DeserializeObject<string[]>(customization.SavedSearches) ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public async Task<bool> SaveViewAsync(int userId, string moduleName, string viewName, object filterCriteria, CancellationToken cancellationToken = default)
    {
        var customization = await _dbContext.UICustomizations
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ModuleName == moduleName && !c.IsDeleted, cancellationToken);

        if (customization == null)
        {
            customization = new UICustomization
            {
                UserId = userId,
                ModuleName = moduleName,
                PageName = "ListView",
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.UICustomizations.Add(customization);
        }

        var views = new List<string>();
        if (!string.IsNullOrEmpty(customization.SavedSearches))
        {
            try
            {
                views = JsonConvert.DeserializeObject<List<string>>(customization.SavedSearches) ?? new List<string>();
            }
            catch { }
        }

        if (!views.Contains(viewName))
            views.Add(viewName);

        customization.SavedSearches = JsonConvert.SerializeObject(views);
        customization.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteViewAsync(int userId, string moduleName, string viewName, CancellationToken cancellationToken = default)
    {
        var customization = await _dbContext.UICustomizations
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ModuleName == moduleName && !c.IsDeleted, cancellationToken);

        if (customization?.SavedSearches == null)
            return true;

        try
        {
            var views = JsonConvert.DeserializeObject<List<string>>(customization.SavedSearches) ?? new List<string>();
            views.Remove(viewName);
            customization.SavedSearches = JsonConvert.SerializeObject(views);
            customization.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch { }

        return true;
    }

    private static UIPreferenceDto MapToDto(UIPreference preference) => new()
    {
        Id = preference.Id,
        UserId = preference.UserId,
        Theme = preference.Theme,
        SidebarPosition = preference.SidebarPosition,
        SidebarWidth = preference.SidebarWidth,
        FontSize = preference.FontSize,
        ShowBreadcrumbs = preference.ShowBreadcrumbs,
        ShowStatusBar = preference.ShowStatusBar,
        ShowTopNavigation = preference.ShowTopNavigation,
        DefaultPageSize = preference.DefaultPageSize,
        DateFormat = preference.DateFormat,
        TimeFormat = preference.TimeFormat,
        CustomColorScheme = preference.CustomColorScheme,
        LastPreferenceUpdate = preference.LastPreferenceUpdate
    };

    private static UICustomizationDto MapToDto(UICustomization customization) => new()
    {
        Id = customization.Id,
        UserId = customization.UserId,
        ModuleName = customization.ModuleName,
        PageName = customization.PageName,
        VisibleColumns = customization.VisibleColumns,
        DefaultSortColumn = customization.DefaultSortColumn,
        DefaultSortOrder = customization.DefaultSortOrder,
        StoredFilters = customization.StoredFilters,
        SavedSearches = customization.SavedSearches,
        RowHeight = customization.RowHeight,
        ShowRowNumbers = customization.ShowRowNumbers,
        ShowFilters = customization.ShowFilters,
        ColumnWidths = customization.ColumnWidths,
        RowsPerPage = customization.RowsPerPage
    };

    private static DashboardCustomizationDto MapToDto(DashboardCustomization dashboard) => new()
    {
        Id = dashboard.Id,
        UserId = dashboard.UserId,
        DashboardName = dashboard.DashboardName,
        LayoutConfig = dashboard.LayoutConfig,
        Widgets = dashboard.Widgets,
        IsDefault = dashboard.IsDefault,
        GridColumns = dashboard.GridColumns,
        AutoRefresh = dashboard.AutoRefresh,
        RefreshIntervalSeconds = dashboard.RefreshIntervalSeconds,
        LastModified = dashboard.LastModified
    };
}
