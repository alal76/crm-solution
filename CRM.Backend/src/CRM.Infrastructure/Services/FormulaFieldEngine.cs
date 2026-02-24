// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

#region Interfaces and DTOs

/// <summary>
/// Engine for evaluating formula field expressions.
/// Supports arithmetic, string operations, conditionals, and cross-object references.
/// </summary>
public interface IFormulaFieldEngine
{
    /// <summary>
    /// Evaluates a formula expression with the given field values.
    /// </summary>
    /// <param name="formula">The formula string (e.g., "{Amount} * {Quantity}").</param>
    /// <param name="fieldValues">Dictionary of field name → value.</param>
    /// <returns>Result of the formula evaluation.</returns>
    FormulaResult Evaluate(string formula, Dictionary<string, object?> fieldValues);

    /// <summary>
    /// Evaluates a formula with cross-object field resolution.
    /// </summary>
    /// <param name="formula">The formula string (e.g., "{Account.AnnualRevenue} * 0.1").</param>
    /// <param name="fieldValues">Primary object field values.</param>
    /// <param name="relatedObjects">Related object field values keyed by object name.</param>
    /// <returns>Result of the formula evaluation.</returns>
    FormulaResult EvaluateWithReferences(
        string formula,
        Dictionary<string, object?> fieldValues,
        Dictionary<string, Dictionary<string, object?>> relatedObjects);

    /// <summary>
    /// Validates a formula expression for syntax errors.
    /// </summary>
    /// <param name="formula">The formula string to validate.</param>
    /// <returns>Validation result with any syntax errors found.</returns>
    FormulaValidationResult Validate(string formula);
}

/// <summary>
/// Result of evaluating a formula expression.
/// </summary>
public class FormulaResult
{
    public bool Success { get; set; }
    public object? Value { get; set; }
    public string? StringValue => Value?.ToString();
    public double? NumericValue => Value is double d ? d : (double.TryParse(Value?.ToString(), out var n) ? n : null);
    public string? Error { get; set; }
}

/// <summary>
/// Result of validating a formula expression.
/// </summary>
public class FormulaValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> ReferencedFields { get; set; } = new();
}

#endregion

/// <summary>
/// Evaluates formula expressions like "{Amount} * {Quantity}" or "IF({Status} == 'Closed', {Revenue}, 0)".
/// Also supports cross-object references like "{Account.AnnualRevenue}".
/// </summary>
public partial class FormulaFieldEngine : IFormulaFieldEngine
{
    private readonly ILogger<FormulaFieldEngine> _logger;

    // Regex to match field references like {FieldName} or {Object.FieldName}
    private static readonly Regex FieldReferenceRegex = new(@"\{([A-Za-z_][A-Za-z0-9_.]*)\}", RegexOptions.Compiled);

    // Regex to match IF(...) expressions
    private static readonly Regex IfExpressionRegex = new(
        @"IF\s*\(\s*(.+?)\s*,\s*(.+?)\s*,\s*(.+?)\s*\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Supported functions
    private static readonly HashSet<string> SupportedFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "ABS", "ROUND", "CEIL", "FLOOR", "MIN", "MAX",
        "LEN", "UPPER", "LOWER", "TRIM", "LEFT", "RIGHT",
        "CONCAT", "IF", "ISNULL", "TODAY", "NOW",
        "YEAR", "MONTH", "DAY"
    };

    public FormulaFieldEngine(ILogger<FormulaFieldEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public FormulaResult Evaluate(string formula, Dictionary<string, object?> fieldValues)
    {
        return EvaluateWithReferences(formula, fieldValues, new Dictionary<string, Dictionary<string, object?>>());
    }

    /// <inheritdoc />
    public FormulaResult EvaluateWithReferences(
        string formula,
        Dictionary<string, object?> fieldValues,
        Dictionary<string, Dictionary<string, object?>> relatedObjects)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(formula))
                return new FormulaResult { Success = false, Error = "Formula is empty." };

            // Step 1: Resolve cross-object references (e.g., {Account.AnnualRevenue})
            var resolvedFormula = ResolveFieldReferences(formula, fieldValues, relatedObjects);

            // Step 2: Process IF(...) expressions
            resolvedFormula = ProcessIfExpressions(resolvedFormula);

            // Step 3: Process built-in functions
            resolvedFormula = ProcessFunctions(resolvedFormula);

            // Step 4: Evaluate the resulting arithmetic/string expression
            var result = EvaluateExpression(resolvedFormula);

            return new FormulaResult { Success = true, Value = result };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Formula evaluation failed: {Formula}", formula);
            return new FormulaResult { Success = false, Error = ex.Message };
        }
    }

    /// <inheritdoc />
    public FormulaValidationResult Validate(string formula)
    {
        var result = new FormulaValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(formula))
        {
            result.IsValid = false;
            result.Errors.Add("Formula is empty.");
            return result;
        }

        // Extract all field references
        var matches = FieldReferenceRegex.Matches(formula);
        foreach (Match match in matches)
        {
            result.ReferencedFields.Add(match.Groups[1].Value);
        }

        // Check for balanced braces
        int braceCount = 0;
        foreach (char c in formula)
        {
            if (c == '{') braceCount++;
            if (c == '}') braceCount--;
            if (braceCount < 0)
            {
                result.IsValid = false;
                result.Errors.Add("Unbalanced braces in formula.");
                break;
            }
        }
        if (braceCount != 0 && result.IsValid)
        {
            result.IsValid = false;
            result.Errors.Add("Unbalanced braces in formula.");
        }

        // Check for balanced parentheses
        int parenCount = 0;
        foreach (char c in formula)
        {
            if (c == '(') parenCount++;
            if (c == ')') parenCount--;
            if (parenCount < 0)
            {
                result.IsValid = false;
                result.Errors.Add("Unbalanced parentheses in formula.");
                break;
            }
        }
        if (parenCount != 0 && result.IsValid)
        {
            result.IsValid = false;
            result.Errors.Add("Unbalanced parentheses in formula.");
        }

        return result;
    }

    /// <summary>
    /// Resolves field references like {FieldName} and {Object.FieldName} to their values.
    /// </summary>
    private string ResolveFieldReferences(
        string formula,
        Dictionary<string, object?> fieldValues,
        Dictionary<string, Dictionary<string, object?>> relatedObjects)
    {
        return FieldReferenceRegex.Replace(formula, match =>
        {
            var reference = match.Groups[1].Value;

            // Check for cross-object reference (e.g., Account.AnnualRevenue)
            if (reference.Contains('.'))
            {
                var parts = reference.Split('.', 2);
                var objectName = parts[0];
                var fieldName = parts[1];

                if (relatedObjects.TryGetValue(objectName, out var relatedFields) &&
                    relatedFields.TryGetValue(fieldName, out var relatedValue))
                {
                    return FormatValue(relatedValue);
                }

                // Also check the primary field values with dotted name
                if (fieldValues.TryGetValue(reference, out var dottedValue))
                {
                    return FormatValue(dottedValue);
                }

                return "0"; // Default for unresolved cross-object references
            }

            // Simple field reference
            if (fieldValues.TryGetValue(reference, out var value))
            {
                return FormatValue(value);
            }

            return "0"; // Default for unresolved fields
        });
    }

    /// <summary>
    /// Processes IF(condition, trueValue, falseValue) expressions.
    /// </summary>
    private string ProcessIfExpressions(string formula)
    {
        // Process nested IFs from inside out
        int maxIterations = 10;
        while (maxIterations-- > 0 && IfExpressionRegex.IsMatch(formula))
        {
            formula = IfExpressionRegex.Replace(formula, match =>
            {
                var condition = match.Groups[1].Value.Trim();
                var trueValue = match.Groups[2].Value.Trim();
                var falseValue = match.Groups[3].Value.Trim();

                bool conditionResult = EvaluateCondition(condition);
                return conditionResult ? trueValue : falseValue;
            });
        }

        return formula;
    }

    /// <summary>
    /// Evaluates a condition expression like "value == 'Closed'" or "amount > 1000".
    /// </summary>
    private static bool EvaluateCondition(string condition)
    {
        // Try == comparison
        if (condition.Contains("=="))
        {
            var parts = condition.Split("==", 2);
            return NormalizeForComparison(parts[0]) == NormalizeForComparison(parts[1]);
        }

        // Try != comparison
        if (condition.Contains("!="))
        {
            var parts = condition.Split("!=", 2);
            return NormalizeForComparison(parts[0]) != NormalizeForComparison(parts[1]);
        }

        // Try >= comparison
        if (condition.Contains(">="))
        {
            var parts = condition.Split(">=", 2);
            if (double.TryParse(parts[0].Trim(), out var left) && double.TryParse(parts[1].Trim(), out var right))
                return left >= right;
        }

        // Try <= comparison
        if (condition.Contains("<="))
        {
            var parts = condition.Split("<=", 2);
            if (double.TryParse(parts[0].Trim(), out var left) && double.TryParse(parts[1].Trim(), out var right))
                return left <= right;
        }

        // Try > comparison
        if (condition.Contains('>'))
        {
            var parts = condition.Split('>', 2);
            if (double.TryParse(parts[0].Trim(), out var left) && double.TryParse(parts[1].Trim(), out var right))
                return left > right;
        }

        // Try < comparison
        if (condition.Contains('<'))
        {
            var parts = condition.Split('<', 2);
            if (double.TryParse(parts[0].Trim(), out var left) && double.TryParse(parts[1].Trim(), out var right))
                return left < right;
        }

        // Boolean: "true", "1", non-zero number
        if (bool.TryParse(condition.Trim(), out var boolResult))
            return boolResult;

        if (double.TryParse(condition.Trim(), out var numResult))
            return numResult != 0;

        return false;
    }

    /// <summary>
    /// Processes built-in functions in the formula.
    /// </summary>
    private static string ProcessFunctions(string formula)
    {
        // ISNULL(value, default)
        formula = Regex.Replace(formula, @"ISNULL\s*\(\s*(.+?)\s*,\s*(.+?)\s*\)", match =>
        {
            var value = match.Groups[1].Value.Trim();
            var defaultValue = match.Groups[2].Value.Trim();
            return (value == "0" || string.IsNullOrEmpty(value) || value == "null") ? defaultValue : value;
        }, RegexOptions.IgnoreCase);

        // ABS(value)
        formula = Regex.Replace(formula, @"ABS\s*\(\s*(.+?)\s*\)", match =>
        {
            if (double.TryParse(match.Groups[1].Value.Trim(), out var val))
                return Math.Abs(val).ToString(CultureInfo.InvariantCulture);
            return match.Value;
        }, RegexOptions.IgnoreCase);

        // ROUND(value, decimals)
        formula = Regex.Replace(formula, @"ROUND\s*\(\s*(.+?)\s*,\s*(\d+)\s*\)", match =>
        {
            if (double.TryParse(match.Groups[1].Value.Trim(), out var val) &&
                int.TryParse(match.Groups[2].Value.Trim(), out var dec))
                return Math.Round(val, dec).ToString(CultureInfo.InvariantCulture);
            return match.Value;
        }, RegexOptions.IgnoreCase);

        // UPPER(value)
        formula = Regex.Replace(formula, @"UPPER\s*\(\s*'(.+?)'\s*\)", match =>
            $"'{match.Groups[1].Value.ToUpperInvariant()}'", RegexOptions.IgnoreCase);

        // LOWER(value)
        formula = Regex.Replace(formula, @"LOWER\s*\(\s*'(.+?)'\s*\)", match =>
            $"'{match.Groups[1].Value.ToLowerInvariant()}'", RegexOptions.IgnoreCase);

        // TRIM(value)
        formula = Regex.Replace(formula, @"TRIM\s*\(\s*'(.+?)'\s*\)", match =>
            $"'{match.Groups[1].Value.Trim()}'", RegexOptions.IgnoreCase);

        // LEN(value)
        formula = Regex.Replace(formula, @"LEN\s*\(\s*'(.+?)'\s*\)", match =>
            match.Groups[1].Value.Length.ToString(), RegexOptions.IgnoreCase);

        // TODAY()
        formula = Regex.Replace(formula, @"TODAY\s*\(\s*\)", _ =>
            $"'{DateTime.UtcNow:yyyy-MM-dd}'", RegexOptions.IgnoreCase);

        // NOW()
        formula = Regex.Replace(formula, @"NOW\s*\(\s*\)", _ =>
            $"'{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}'", RegexOptions.IgnoreCase);

        // CONCAT('a', 'b', ...)
        formula = Regex.Replace(formula, @"CONCAT\s*\((.+?)\)", match =>
        {
            var args = match.Groups[1].Value.Split(',')
                .Select(a => a.Trim().Trim('\'', '"'))
                .ToArray();
            return $"'{string.Join("", args)}'";
        }, RegexOptions.IgnoreCase);

        return formula;
    }

    /// <summary>
    /// Evaluates result expression (simple arithmetic: +, -, *, /, or returns a string).
    /// </summary>
    private static object EvaluateExpression(string expression)
    {
        expression = expression.Trim();

        // If it's a quoted string, return string value
        if (expression.StartsWith('\'') && expression.EndsWith('\''))
            return expression.Trim('\'');
        if (expression.StartsWith('"') && expression.EndsWith('"'))
            return expression.Trim('"');

        // Try to evaluate as arithmetic
        try
        {
            return EvaluateArithmetic(expression);
        }
        catch
        {
            // Return as string if arithmetic fails
            return expression;
        }
    }

    /// <summary>
    /// Simple recursive descent arithmetic evaluator supporting +, -, *, /, (, ).
    /// </summary>
    private static double EvaluateArithmetic(string expression)
    {
        expression = expression.Trim();

        // Remove outer parentheses
        if (expression.StartsWith('(') && expression.EndsWith(')'))
        {
            int depth = 0;
            bool allWrapped = true;
            for (int i = 0; i < expression.Length - 1; i++)
            {
                if (expression[i] == '(') depth++;
                if (expression[i] == ')') depth--;
                if (depth == 0 && i > 0) { allWrapped = false; break; }
            }
            if (allWrapped)
                expression = expression[1..^1].Trim();
        }

        // Find the lowest precedence operator outside parentheses
        int parenDepth = 0;
        int lastAdd = -1, lastMul = -1;

        for (int i = expression.Length - 1; i >= 0; i--)
        {
            char c = expression[i];
            if (c == ')') parenDepth++;
            if (c == '(') parenDepth--;

            if (parenDepth == 0)
            {
                if ((c == '+' || c == '-') && i > 0 && lastAdd < 0)
                    lastAdd = i;
                if ((c == '*' || c == '/') && lastMul < 0)
                    lastMul = i;
            }
        }

        // Split on lowest precedence operator
        if (lastAdd > 0)
        {
            var left = EvaluateArithmetic(expression[..lastAdd]);
            var right = EvaluateArithmetic(expression[(lastAdd + 1)..]);
            return expression[lastAdd] == '+' ? left + right : left - right;
        }

        if (lastMul > 0)
        {
            var left = EvaluateArithmetic(expression[..lastMul]);
            var right = EvaluateArithmetic(expression[(lastMul + 1)..]);
            return expression[lastMul] == '*' ? left * right : (right != 0 ? left / right : 0);
        }

        // Try parse as number
        if (double.TryParse(expression, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
            return number;

        throw new FormatException($"Cannot evaluate expression: {expression}");
    }

    private static string FormatValue(object? value)
    {
        if (value == null) return "0";
        if (value is string s) return $"'{s}'";
        if (value is DateTime dt) return $"'{dt:yyyy-MM-dd}'";
        if (value is bool b) return b ? "1" : "0";
        if (value is decimal dec) return dec.ToString(CultureInfo.InvariantCulture);
        if (value is double d) return d.ToString(CultureInfo.InvariantCulture);
        if (value is float f) return f.ToString(CultureInfo.InvariantCulture);
        if (value is int i) return i.ToString();
        if (value is long l) return l.ToString();
        return value.ToString() ?? "0";
    }

    private static string NormalizeForComparison(string value)
    {
        return value.Trim().Trim('\'', '"').ToLowerInvariant();
    }
}
