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
/// Interface for resolving the database context.
/// </summary>
public interface IDbContextResolver
{
    /// <summary>
    /// Resolves the ICrmDbContext (production database)
    /// </summary>
    ICrmDbContext ResolveContext();
}

/// <summary>
/// Resolves the production database context.
/// </summary>
public class DynamicDbContextResolver : IDbContextResolver
{
    private readonly CrmDbContext _productionContext;
    private readonly ILogger<DynamicDbContextResolver> _logger;

    public DynamicDbContextResolver(
        CrmDbContext productionContext,
        ILogger<DynamicDbContextResolver> logger)
    {
        _productionContext = productionContext;
        _logger = logger;
    }

    public ICrmDbContext ResolveContext()
    {
        _logger.LogDebug("ResolveContext called - returning production context");
        return _productionContext;
    }
}
