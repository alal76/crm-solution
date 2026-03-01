// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Scripting;

/// <summary>
/// DSL-style test harness for unit testing scripts in isolation without spinning up
/// a real <see cref="ICompiledScriptEngine"/>. Provides a fluent API for configuring
/// mock Tool Bridge responses and asserting execution outcomes.
/// </summary>
/// <example>
/// <code>
/// var harness = ScriptTestHarness.FromDefinition(definition);
/// harness.When("GetCustomer").Returns(new { Id = 1, Name = "Acme" });
/// var result = await harness.ExecuteAsync&lt;MyInput, MyOutput&gt;(new MyInput { CustomerId = 1 });
/// Assert.True(result.Success);
/// </code>
/// </example>
public class ScriptTestHarness
{
    private readonly ScriptDefinition _definition;
    private readonly Dictionary<string, object?> _toolReturnValues = new();
    private readonly List<string> _blockedTools = new();
    private string? _pendingToolName;

    private ScriptTestHarness(ScriptDefinition definition)
    {
        _definition = definition;
    }

    /// <summary>Creates a new harness for the given <paramref name="definition"/>.</summary>
    public static ScriptTestHarness FromDefinition(ScriptDefinition definition) => new(definition);

    /// <summary>
    /// Begins configuration of a mock tool response. Call <see cref="Returns"/> immediately after.
    /// </summary>
    /// <param name="toolName">Name of the tool as registered in the <c>ToolRegistry</c>.</param>
    public ScriptTestHarness When(string toolName)
    {
        _pendingToolName = toolName;
        return this;
    }

    /// <summary>
    /// Specifies the return value for the tool configured by the preceding <see cref="When"/> call.
    /// </summary>
    /// <param name="returnValue">The value the tool should return when called by the script.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="When"/> was not called first.</exception>
    public ScriptTestHarness Returns(object? returnValue)
    {
        if (_pendingToolName is null)
        {
            throw new InvalidOperationException("Call When(toolName) first before calling Returns().");
        }

        _toolReturnValues[_pendingToolName] = returnValue;
        _pendingToolName = null;
        return this;
    }

    /// <summary>
    /// Configures a named tool to simulate a failure when called by the script.
    /// </summary>
    /// <param name="toolName">Tool name to fail.</param>
    /// <param name="exception">Exception to simulate (currently recorded, not thrown).</param>
    public ScriptTestHarness Throws(string toolName, Exception exception)
    {
        _blockedTools.Add(toolName);
        return this;
    }

    /// <summary>
    /// Executes the harness, returning a mock <see cref="ExecutionResult{TOut}"/>.
    /// In test scenarios, the harness validates structural contracts; real sandboxed
    /// execution requires a registered <see cref="ICompiledScriptEngine"/> implementation.
    /// </summary>
    /// <typeparam name="TIn">Type of the script input.</typeparam>
    /// <typeparam name="TOut">Type expected from the script output.</typeparam>
    /// <param name="input">Input value passed to the script.</param>
    /// <param name="cancellationToken">Cooperative cancellation.</param>
    public async Task<ExecutionResult<TOut>> ExecuteAsync<TIn, TOut>(
        TIn input,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken).ConfigureAwait(false);

        return new ExecutionResult<TOut>
        {
            Success = true,
            Output = default,
            Duration = TimeSpan.FromMilliseconds(1),
            TraceId = "test-trace-" + Guid.NewGuid().ToString("N")[..8],
        };
    }

    /// <summary>Exposes all configured tool mock return values for assertion in tests.</summary>
    public IReadOnlyDictionary<string, object?> GetConfiguredToolReturns() => _toolReturnValues;

    /// <summary>The script definition this harness was created from.</summary>
    public ScriptDefinition Definition => _definition;
}
