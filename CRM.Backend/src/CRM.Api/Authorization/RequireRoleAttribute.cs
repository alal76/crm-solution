using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

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
