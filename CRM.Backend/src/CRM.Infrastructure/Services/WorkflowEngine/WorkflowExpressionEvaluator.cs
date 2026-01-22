using System.Text.Json;
using System.Text.RegularExpressions;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine;

/// <summary>
/// Evaluates expressions and replaces variables in workflow configurations
/// Supports simple expression syntax with variable substitution
/// </summary>
public class WorkflowExpressionEvaluator : IWorkflowExpressionEvaluator
{
    private readonly ILogger<WorkflowExpressionEvaluator> _logger;
    
    // Pattern for variable substitution: {{variableName}} or {{variableName.property}}
    private static readonly Regex VariablePattern = new(@"\{\{([a-zA-Z_][a-zA-Z0-9_\.]*)\}\}", 
        RegexOptions.Compiled);

    public WorkflowExpressionEvaluator(ILogger<WorkflowExpressionEvaluator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Evaluates a condition expression against workflow variables
    /// </summary>
    public Task<bool> EvaluateConditionAsync(
        string condition,
        Dictionary<string, object?> variables,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return Task.FromResult(true);
        }

        try
        {
            // Replace variables first
            var processedCondition = ReplaceVariablesSync(condition, variables);
            
            // Parse and evaluate the condition
            var result = EvaluateConditionExpression(processedCondition, variables);
            
            _logger.LogDebug("Condition '{Condition}' evaluated to {Result}", condition, result);
            
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error evaluating condition: {Condition}", condition);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Evaluates an expression and returns its value
    /// </summary>
    public Task<object?> EvaluateExpressionAsync(
        string expression,
        Dictionary<string, object?> variables,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return Task.FromResult<object?>(null);
        }

        try
        {
            // Simple expressions: direct variable reference
            if (expression.StartsWith("{{") && expression.EndsWith("}}"))
            {
                var varName = expression[2..^2].Trim();
                var value = GetVariableValue(varName, variables);
                return Task.FromResult(value);
            }

            // Arithmetic expressions
            if (IsArithmeticExpression(expression))
            {
                var result = EvaluateArithmetic(expression, variables);
                return Task.FromResult<object?>(result);
            }

            // String with variable substitution
            var processed = ReplaceVariablesSync(expression, variables);
            return Task.FromResult<object?>(processed);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error evaluating expression: {Expression}", expression);
            return Task.FromResult<object?>(null);
        }
    }

    /// <summary>
    /// Replaces {{variable}} placeholders with actual values
    /// </summary>
    public Task<string> ReplaceVariablesAsync(
        string template,
        Dictionary<string, object?> variables,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return Task.FromResult(template ?? string.Empty);
        }

        var result = ReplaceVariablesSync(template, variables);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Validates an expression syntax
    /// </summary>
    public Task<WorkflowValidationResultDto> ValidateExpressionAsync(
        string expression,
        CancellationToken cancellationToken = default)
    {
        var result = new WorkflowValidationResultDto { IsValid = true };

        if (string.IsNullOrWhiteSpace(expression))
        {
            return Task.FromResult(result);
        }

        try
        {
            // Check for balanced braces
            var braceCount = 0;
            foreach (var c in expression)
            {
                if (c == '{') braceCount++;
                else if (c == '}') braceCount--;
                
                if (braceCount < 0)
                {
                    result.IsValid = false;
                    result.Errors.Add("Unbalanced braces in expression");
                    return Task.FromResult(result);
                }
            }

            if (braceCount != 0)
            {
                result.IsValid = false;
                result.Errors.Add("Unbalanced braces in expression");
                return Task.FromResult(result);
            }

            // Check for valid variable references
            var matches = VariablePattern.Matches(expression);
            foreach (Match match in matches)
            {
                var varName = match.Groups[1].Value;
                if (string.IsNullOrEmpty(varName))
                {
                    result.IsValid = false;
                    result.Errors.Add("Empty variable reference found");
                }
            }

            // Try to parse as condition
            if (expression.Contains("==") || expression.Contains("!=") || 
                expression.Contains(">") || expression.Contains("<") ||
                expression.Contains(" and ") || expression.Contains(" or "))
            {
                // Basic syntax check passed
                result.Suggestions.Add("Expression appears to be a condition");
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Expression validation error: {ex.Message}");
        }

        return Task.FromResult(result);
    }

    private string ReplaceVariablesSync(string template, Dictionary<string, object?> variables)
    {
        return VariablePattern.Replace(template, match =>
        {
            var varPath = match.Groups[1].Value;
            var value = GetVariableValue(varPath, variables);
            return FormatValue(value);
        });
    }

    private object? GetVariableValue(string path, Dictionary<string, object?> variables)
    {
        var parts = path.Split('.');
        object? current = null;

        // Get root variable
        var rootName = parts[0];
        if (!variables.TryGetValue(rootName, out current))
        {
            // Try case-insensitive
            var key = variables.Keys.FirstOrDefault(k => 
                k.Equals(rootName, StringComparison.OrdinalIgnoreCase));
            if (key != null)
            {
                current = variables[key];
            }
        }

        if (current == null || parts.Length == 1)
        {
            return current;
        }

        // Navigate nested properties
        for (int i = 1; i < parts.Length; i++)
        {
            if (current == null) return null;

            var propertyName = parts[i];

            // Handle JsonElement
            if (current is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Object)
                {
                    if (jsonElement.TryGetProperty(propertyName, out var prop))
                    {
                        current = GetJsonElementValue(prop);
                    }
                    else
                    {
                        // Try case-insensitive
                        var found = false;
                        foreach (var p in jsonElement.EnumerateObject())
                        {
                            if (p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                            {
                                current = GetJsonElementValue(p.Value);
                                found = true;
                                break;
                            }
                        }
                        if (!found) return null;
                    }
                }
                else if (jsonElement.ValueKind == JsonValueKind.Array && int.TryParse(propertyName, out var index))
                {
                    if (index >= 0 && index < jsonElement.GetArrayLength())
                    {
                        current = GetJsonElementValue(jsonElement[index]);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            // Handle Dictionary
            else if (current is IDictionary<string, object?> dict)
            {
                if (dict.TryGetValue(propertyName, out var val))
                {
                    current = val;
                }
                else
                {
                    var key = dict.Keys.FirstOrDefault(k => 
                        k.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                    current = key != null ? dict[key] : null;
                }
            }
            // Handle object properties via reflection
            else
            {
                var type = current.GetType();
                var prop = type.GetProperty(propertyName, 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.IgnoreCase);
                
                current = prop?.GetValue(current);
            }
        }

        return current;
    }

    private object? GetJsonElementValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element
        };
    }

    private string FormatValue(object? value)
    {
        if (value == null) return string.Empty;
        
        return value switch
        {
            DateTime dt => dt.ToString("O"),
            DateTimeOffset dto => dto.ToString("O"),
            bool b => b.ToString().ToLower(),
            JsonElement je => je.GetRawText(),
            _ => value.ToString() ?? string.Empty
        };
    }

    private bool EvaluateConditionExpression(string condition, Dictionary<string, object?> variables)
    {
        condition = condition.Trim();

        // Boolean literals
        if (condition.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
        if (condition.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;

        // Handle AND/OR operators
        if (ContainsLogicalOperator(condition, out var op, out var left, out var right))
        {
            var leftResult = EvaluateConditionExpression(left, variables);
            var rightResult = EvaluateConditionExpression(right, variables);

            return op.ToLower() switch
            {
                "and" or "&&" => leftResult && rightResult,
                "or" or "||" => leftResult || rightResult,
                _ => false
            };
        }

        // Handle NOT operator
        if (condition.StartsWith("!", StringComparison.OrdinalIgnoreCase) ||
            condition.StartsWith("not ", StringComparison.OrdinalIgnoreCase))
        {
            var inner = condition.StartsWith("!") 
                ? condition[1..].Trim() 
                : condition[4..].Trim();
            return !EvaluateConditionExpression(inner, variables);
        }

        // Handle comparison operators
        foreach (var compOp in new[] { "==", "!=", ">=", "<=", ">", "<", " eq ", " ne ", " gt ", " lt ", " gte ", " lte " })
        {
            var index = condition.IndexOf(compOp, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                var leftPart = condition[..index].Trim();
                var rightPart = condition[(index + compOp.Length)..].Trim();

                var leftValue = ResolveValue(leftPart, variables);
                var rightValue = ResolveValue(rightPart, variables);

                return CompareValues(leftValue, rightValue, compOp.Trim().ToLower());
            }
        }

        // Handle contains/startsWith/endsWith
        if (condition.Contains(".contains(", StringComparison.OrdinalIgnoreCase))
        {
            return EvaluateStringMethod(condition, "contains", variables);
        }
        if (condition.Contains(".startsWith(", StringComparison.OrdinalIgnoreCase))
        {
            return EvaluateStringMethod(condition, "startswith", variables);
        }
        if (condition.Contains(".endsWith(", StringComparison.OrdinalIgnoreCase))
        {
            return EvaluateStringMethod(condition, "endswith", variables);
        }

        // Check if it's a variable that evaluates to boolean
        var varValue = ResolveValue(condition, variables);
        return IsTruthy(varValue);
    }

    private bool ContainsLogicalOperator(string condition, out string op, out string left, out string right)
    {
        op = left = right = string.Empty;

        // Find AND/OR not inside parentheses
        var depth = 0;
        var words = new[] { " and ", " or ", "&&", "||" };

        foreach (var word in words)
        {
            var searchStart = 0;
            while (true)
            {
                var idx = condition.IndexOf(word, searchStart, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) break;

                // Count parentheses before this position
                depth = 0;
                for (int i = 0; i < idx; i++)
                {
                    if (condition[i] == '(') depth++;
                    else if (condition[i] == ')') depth--;
                }

                if (depth == 0)
                {
                    op = word.Trim();
                    left = condition[..idx].Trim();
                    right = condition[(idx + word.Length)..].Trim();
                    return true;
                }

                searchStart = idx + 1;
            }
        }

        return false;
    }

    private object? ResolveValue(string expr, Dictionary<string, object?> variables)
    {
        expr = expr.Trim();

        // String literal
        if ((expr.StartsWith('"') && expr.EndsWith('"')) ||
            (expr.StartsWith('\'') && expr.EndsWith('\'')))
        {
            return expr[1..^1];
        }

        // Number literal
        if (double.TryParse(expr, out var num))
        {
            return num;
        }

        // Boolean literal
        if (expr.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
        if (expr.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
        if (expr.Equals("null", StringComparison.OrdinalIgnoreCase)) return null;

        // Variable reference (with or without braces)
        if (expr.StartsWith("{{") && expr.EndsWith("}}"))
        {
            expr = expr[2..^2];
        }

        return GetVariableValue(expr, variables);
    }

    private bool CompareValues(object? left, object? right, string op)
    {
        // Null comparisons
        if (left == null && right == null) return op is "==" or "eq";
        if (left == null || right == null) return op is "!=" or "ne";

        // Try numeric comparison
        if (TryGetNumber(left, out var leftNum) && TryGetNumber(right, out var rightNum))
        {
            return op switch
            {
                "==" or "eq" => Math.Abs(leftNum - rightNum) < 0.0001,
                "!=" or "ne" => Math.Abs(leftNum - rightNum) >= 0.0001,
                ">" or "gt" => leftNum > rightNum,
                "<" or "lt" => leftNum < rightNum,
                ">=" or "gte" => leftNum >= rightNum,
                "<=" or "lte" => leftNum <= rightNum,
                _ => false
            };
        }

        // String comparison
        var leftStr = left.ToString() ?? "";
        var rightStr = right.ToString() ?? "";

        return op switch
        {
            "==" or "eq" => leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase),
            "!=" or "ne" => !leftStr.Equals(rightStr, StringComparison.OrdinalIgnoreCase),
            ">" or "gt" => string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) > 0,
            "<" or "lt" => string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) < 0,
            ">=" or "gte" => string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) >= 0,
            "<=" or "lte" => string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase) <= 0,
            _ => false
        };
    }

    private bool TryGetNumber(object? value, out double result)
    {
        result = 0;
        if (value == null) return false;

        return value switch
        {
            int i => (result = i) == i,
            long l => (result = l) == l,
            float f => (result = f) == f,
            double d => (result = d) == d,
            decimal m => (result = (double)m) == (double)m,
            string s => double.TryParse(s, out result),
            _ => false
        };
    }

    private bool EvaluateStringMethod(string condition, string method, Dictionary<string, object?> variables)
    {
        try
        {
            var methodPattern = $@"(.+)\.{method}\s*\((.+)\)";
            var match = Regex.Match(condition, methodPattern, RegexOptions.IgnoreCase);
            
            if (!match.Success) return false;

            var targetExpr = match.Groups[1].Value.Trim();
            var argExpr = match.Groups[2].Value.Trim();

            var targetValue = ResolveValue(targetExpr, variables)?.ToString() ?? "";
            var argValue = ResolveValue(argExpr, variables)?.ToString() ?? "";

            return method.ToLower() switch
            {
                "contains" => targetValue.Contains(argValue, StringComparison.OrdinalIgnoreCase),
                "startswith" => targetValue.StartsWith(argValue, StringComparison.OrdinalIgnoreCase),
                "endswith" => targetValue.EndsWith(argValue, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    private bool IsTruthy(object? value)
    {
        if (value == null) return false;

        return value switch
        {
            bool b => b,
            int i => i != 0,
            long l => l != 0,
            double d => d != 0,
            string s => !string.IsNullOrEmpty(s) && !s.Equals("false", StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }

    private bool IsArithmeticExpression(string expression)
    {
        return Regex.IsMatch(expression, @"[\+\-\*\/\%]");
    }

    private double EvaluateArithmetic(string expression, Dictionary<string, object?> variables)
    {
        // Replace variables with values
        var processed = VariablePattern.Replace(expression, match =>
        {
            var varPath = match.Groups[1].Value;
            var value = GetVariableValue(varPath, variables);
            if (TryGetNumber(value, out var num))
            {
                return num.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            return "0";
        });

        // Simple arithmetic evaluation (basic implementation)
        // For production, consider using a proper expression evaluator like NCalc
        return EvaluateSimpleArithmetic(processed);
    }

    private double EvaluateSimpleArithmetic(string expression)
    {
        // Very basic implementation - handles +, -, *, /
        // In production, use DataTable.Compute or NCalc library
        try
        {
            var dataTable = new System.Data.DataTable();
            var result = dataTable.Compute(expression, null);
            return Convert.ToDouble(result);
        }
        catch
        {
            return 0;
        }
    }
}
