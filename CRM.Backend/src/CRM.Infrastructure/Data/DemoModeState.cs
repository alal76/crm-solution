// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace CRM.Infrastructure.Data;

/// <summary>
/// Singleton state holder for demo mode.
/// Stored at the API layer to avoid circular database dependencies.
/// </summary>
public interface IDemoModeState
{
    /// <summary>
    /// Gets or sets whether demo mode is active
    /// </summary>
    bool IsDemoMode { get; set; }
    
    /// <summary>
    /// Event raised when demo mode changes
    /// </summary>
    event EventHandler<bool>? DemoModeChanged;
}

/// <summary>
/// Thread-safe singleton implementation for demo mode state.
/// Defaults to production mode (demo disabled).
/// </summary>
public class DemoModeState : IDemoModeState
{
    private volatile bool _isDemoMode = false; // Default to production mode
    private readonly object _lock = new();
    
    public event EventHandler<bool>? DemoModeChanged;

    public bool IsDemoMode
    {
        get => _isDemoMode;
        set
        {
            lock (_lock)
            {
                if (_isDemoMode != value)
                {
                    _isDemoMode = value;
                    DemoModeChanged?.Invoke(this, value);
                }
            }
        }
    }
}
