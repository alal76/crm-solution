// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Data;

/// <summary>
/// Interface for resolving the appropriate database context based on system settings.
/// This enables dynamic switching between production and demo databases.
/// </summary>
public interface IDbContextResolver
{
    /// <summary>
    /// Resolves the appropriate ICrmDbContext based on the UseDemoDatabase setting
    /// </summary>
    ICrmDbContext ResolveContext();
    
    /// <summary>
    /// Gets whether demo mode is currently active
    /// </summary>
    bool IsDemoModeActive();
    
    /// <summary>
    /// Clears the cached demo mode state to force a refresh
    /// </summary>
    void ClearCache();
}

/// <summary>
/// Resolves the appropriate database context (production or demo) based on the IDemoModeState singleton.
/// The demo mode state is stored at the API layer to avoid circular database dependencies.
/// </summary>
public class DynamicDbContextResolver : IDbContextResolver
{
    private readonly CrmDbContext _productionContext;
    private readonly IDemoDbContextFactory _demoDbContextFactory;
    private readonly IDemoModeState _demoModeState;
    private readonly ILogger<DynamicDbContextResolver> _logger;
    
    // Store the demo context for the lifetime of this resolver (scoped)
    private CrmDbContext? _demoContext;

    public DynamicDbContextResolver(
        CrmDbContext productionContext,
        IDemoDbContextFactory demoDbContextFactory,
        IDemoModeState demoModeState,
        ILogger<DynamicDbContextResolver> logger)
    {
        _productionContext = productionContext;
        _demoDbContextFactory = demoDbContextFactory;
        _demoModeState = demoModeState;
        _logger = logger;
    }

    public bool IsDemoModeActive()
    {
        var isDemoMode = _demoModeState.IsDemoMode;
        _logger.LogDebug("Demo mode check: IsDemoMode = {IsDemoMode}", isDemoMode);
        return isDemoMode;
    }

    public ICrmDbContext ResolveContext()
    {
        var isDemoMode = IsDemoModeActive();
        _logger.LogInformation("ResolveContext called - IsDemoMode: {IsDemoMode}", isDemoMode);
        
        if (isDemoMode)
        {
            try
            {
                // Create or reuse demo context for this scoped lifetime
                if (_demoContext == null)
                {
                    _demoContext = _demoDbContextFactory.CreateDemoContext();
                    _logger.LogInformation("Created new DEMO database context");
                }
                _logger.LogInformation("Returning DEMO database context");
                return _demoContext;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create demo context, falling back to production");
                return _productionContext;
            }
        }
        
        _logger.LogInformation("Returning PRODUCTION database context");
        return _productionContext;
    }

    public void ClearCache()
    {
        // No longer using cache - demo mode state is managed by IDemoModeState singleton
        _logger.LogDebug("ClearCache called - no-op as demo mode is managed by IDemoModeState singleton");
    }
}
