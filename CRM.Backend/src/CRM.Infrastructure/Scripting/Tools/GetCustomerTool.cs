// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CRM.Core.Interfaces;
using CRM.Core.Scripting;

namespace CRM.Infrastructure.Scripting.Tools;

/// <summary>
/// Script tool that retrieves a customer (account) record by ID.
/// Requires the <c>read:customer</c> permission.
/// In production, replace the stub return with a call to <c>IAccountService</c>.
/// </summary>
[ScriptTool("GetCustomer", "Retrieve a customer record by ID", "read:customer")]
public class GetCustomerTool
{
    private readonly ILogger<GetCustomerTool> _logger;
    private readonly IAccountService _accountService;

    /// <summary>Initialises a new <see cref="GetCustomerTool"/>.</summary>
    public GetCustomerTool(ILogger<GetCustomerTool> logger, IAccountService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    /// <summary>Invokes the tool. <paramref name="parameters"/> should contain an <c>Id</c> property.</summary>
    public async Task<object?> InvokeAsync(object parameters, CancellationToken cancellationToken)
    {
        _logger.LogDebug("GetCustomerTool invoked with params {Params}", parameters);

        dynamic p = parameters;
        int id = (int)p.Id;
        var account = await _accountService.GetAccountByIdAsync(id).ConfigureAwait(false);
        return account;
    }
}
