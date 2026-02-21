// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRM.Api.Authorization;

/// <summary>
/// Custom authorization filter for role-based access control
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly CRM.Core.Entities.UserRole[] _allowedRoles;

    public RequireRoleAttribute(params CRM.Core.Entities.UserRole[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? false)
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        var roleClaim = user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            ?? user.FindFirst(ClaimTypes.Role);

        if (roleClaim != null && Enum.TryParse<CRM.Core.Entities.UserRole>(roleClaim.Value, out var userRole))
        {
            if (_allowedRoles.Contains(userRole))
            {
                return Task.CompletedTask;
            }
        }

        context.Result = new ForbidResult();
        return Task.CompletedTask;
    }
}

public class RequireRoleAttribute<T> : AuthorizeAttribute where T : Enum
{
    public RequireRoleAttribute(params T[] roles)
    {
        Roles = string.Join(",", roles.Select(r => r.ToString()));
    }
}
