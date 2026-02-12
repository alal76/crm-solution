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

using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Attributes;

namespace CRM.Infrastructure.AI.SK.Filters;

/// <summary>
/// Semantic Kernel function invocation filter that intercepts functions marked with
/// <see cref="RequiresApprovalAttribute"/> and creates approval requests in the database
/// for human review before allowing execution to proceed.
/// </summary>
/// <remarks>
/// When a function requires approval:
/// <list type="number">
///   <item>Execution is paused</item>
///   <item>An <see cref="AgentApprovalRequest"/> record is created</item>
///   <item>The result is set to "APPROVAL_REQUIRED:{id}" so the caller can poll for approval</item>
///   <item>A separate approval workflow resumes execution once approved</item>
/// </list>
/// </remarks>
public class HumanApprovalFilter : IFunctionInvocationFilter
{
    #region Fields

    private readonly ICrmDbContext _context;
    private readonly ILogger<HumanApprovalFilter> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanApprovalFilter"/> class.
    /// </summary>
    /// <param name="context">Database context for persisting approval requests.</param>
    /// <param name="logger">Logger instance.</param>
    public HumanApprovalFilter(ICrmDbContext context, ILogger<HumanApprovalFilter> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IFunctionInvocationFilter Implementation

    /// <inheritdoc />
    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        // Check if the underlying method has [RequiresApproval]
        var approvalAttr = GetApprovalAttribute(context.Function);

        if (approvalAttr != null)
        {
            _logger.LogInformation(
                "Function {Plugin}.{Function} requires {Tier} approval — pausing execution",
                context.Function.PluginName,
                context.Function.Name,
                approvalAttr.Tier);

            // Serialize function arguments for the approval record
            var serializedParams = SerializeArguments(context.Arguments);

            // Create approval request in database
            var approvalRequest = new AgentApprovalRequest
            {
                ActionDescription = !string.IsNullOrEmpty(approvalAttr.Description)
                    ? approvalAttr.Description
                    : $"{context.Function.PluginName}.{context.Function.Name}",
                PluginName = context.Function.PluginName ?? "Unknown",
                FunctionName = context.Function.Name,
                Parameters = serializedParams,
                Status = ApprovalStatus.Pending,
                ApprovalTier = approvalAttr.Tier,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(approvalAttr.TimeoutMinutes > 0 ? approvalAttr.TimeoutMinutes : 60)
            };

            _context.Set<AgentApprovalRequest>().Add(approvalRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Approval request {ApprovalId} created for {Plugin}.{Function}",
                approvalRequest.Id,
                context.Function.PluginName,
                context.Function.Name);

            // Set result to indicate approval is required — caller must poll/wait
            context.Result = new FunctionResult(
                context.Function,
                $"APPROVAL_REQUIRED:{approvalRequest.Id}");

            return;
        }

        // No approval attribute — proceed with normal execution
        await next(context);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Attempts to find a <see cref="RequiresApprovalAttribute"/> on the SK function's underlying method.
    /// </summary>
    private static RequiresApprovalAttribute? GetApprovalAttribute(KernelFunction function)
    {
        // SK functions resolved from plugins expose metadata we can inspect
        try
        {
            var methodInfo = function.GetType()
                .GetProperty("Method", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(function) as MethodInfo;

            return methodInfo?.GetCustomAttribute<RequiresApprovalAttribute>();
        }
        catch
        {
            // If reflection fails, assume no approval needed
            return null;
        }
    }

    /// <summary>
    /// Serializes function arguments to a JSON string for audit storage.
    /// </summary>
    private static string SerializeArguments(KernelArguments? arguments)
    {
        if (arguments == null || arguments.Count == 0)
        {
            return "{}";
        }

        try
        {
            var dict = arguments.ToDictionary(a => a.Key, a => a.Value?.ToString());
            return JsonSerializer.Serialize(dict, _jsonOptions);
        }
        catch
        {
            return "{}";
        }
    }

    #endregion
}
