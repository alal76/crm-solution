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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Diagnostics endpoint for auth pipeline isolation.
/// </summary>
[ApiController]
[Route("api/auth-diagnostics")]
public class AuthDiagnosticsController : ControllerBase
{
    private readonly ILogger<AuthDiagnosticsController> _logger;

    public AuthDiagnosticsController(ILogger<AuthDiagnosticsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simple POST endpoint to validate POST handling without auth service DI.
    /// </summary>
    [HttpPost("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        _logger.LogWarning("AuthDiagnosticsController.Ping reached");
        return Ok(new { ok = true });
    }
}
