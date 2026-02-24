// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.RegularExpressions;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Parser for complex condition expressions used in escalation rules.
/// Supports expressions like: "(Priority = High AND Age > 4h) OR Status = Pending"
/// TODO-SD005-012: Complex condition expression parser.
/// </summary>
public interface IConditionExpressionParser
{
    /// <summary>
    /// Parses a condition expression string into an evaluatable expression tree.
    /// </summary>
    /// <param name="expression">The condition expression to parse</param>
    /// <returns>Parsed condition node</returns>
    ConditionNode Parse(string expression);

    /// <summary>
    /// Evaluates a condition expression against provided context values.
    /// </summary>
    /// <param name="expression">The condition expression</param>
    /// <param name="context">Dictionary of field names to their current values</param>
    /// <returns>True if condition is satisfied</returns>
    bool Evaluate(string expression, Dictionary<string, object> context);

    /// <summary>
    /// Validates a condition expression for syntax errors.
    /// </summary>
    /// <param name="expression">The condition expression</param>
    /// <returns>Validation result with any errors</returns>
    ConditionValidationResult Validate(string expression);
}

/// <summary>
/// Represents a node in the condition expression tree.
/// </summary>
public abstract class ConditionNode
{
    public abstract bool Evaluate(Dictionary<string, object> context);
}

/// <summary>
/// Represents a comparison condition (e.g., "Priority = High").
/// </summary>
public class ComparisonNode : ConditionNode
{
    public string FieldName { get; set; } = string.Empty;
    public ComparisonOperator Operator { get; set; }
    public object Value { get; set; } = null!;

    public override bool Evaluate(Dictionary<string, object> context)
    {
        if (!context.TryGetValue(FieldName, out var fieldValue))
            return false;

        return Operator switch
        {
            ComparisonOperator.Equals => AreEqual(fieldValue, Value),
            ComparisonOperator.NotEquals => !AreEqual(fieldValue, Value),
            ComparisonOperator.GreaterThan => Compare(fieldValue, Value) > 0,
            ComparisonOperator.GreaterThanOrEqual => Compare(fieldValue, Value) >= 0,
            ComparisonOperator.LessThan => Compare(fieldValue, Value) < 0,
            ComparisonOperator.LessThanOrEqual => Compare(fieldValue, Value) <= 0,
            ComparisonOperator.Contains => fieldValue?.ToString()?.Contains(Value?.ToString() ?? "") ?? false,
            _ => false
        };
    }

    private static bool AreEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.ToString()?.Equals(b.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private static int Compare(object? a, object? b)
    {
        if (a == null || b == null) return 0;

        // Try numeric comparison
        if (double.TryParse(a.ToString(), out var numA) && double.TryParse(b.ToString(), out var numB))
            return numA.CompareTo(numB);

        // String comparison
        return string.Compare(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Represents a logical AND/OR combination of conditions.
/// </summary>
public class LogicalNode : ConditionNode
{
    public LogicalOperator Operator { get; set; }
    public ConditionNode Left { get; set; } = null!;
    public ConditionNode Right { get; set; } = null!;

    public override bool Evaluate(Dictionary<string, object> context)
    {
        return Operator switch
        {
            LogicalOperator.And => Left.Evaluate(context) && Right.Evaluate(context),
            LogicalOperator.Or => Left.Evaluate(context) || Right.Evaluate(context),
            _ => false
        };
    }
}

/// <summary>
/// Represents a NOT condition.
/// </summary>
public class NotNode : ConditionNode
{
    public ConditionNode Inner { get; set; } = null!;

    public override bool Evaluate(Dictionary<string, object> context)
    {
        return !Inner.Evaluate(context);
    }
}

public enum ComparisonOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains
}

public enum LogicalOperator
{
    And,
    Or
}

/// <summary>
/// Result of condition expression validation.
/// </summary>
public class ConditionValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Implementation of condition expression parser.
/// </summary>
public class ConditionExpressionParser : IConditionExpressionParser
{
    // Token patterns
    private static readonly Regex TokenPattern = new(
        @"(\(|\)|\bAND\b|\bOR\b|\bNOT\b|[A-Za-z_][A-Za-z0-9_]*|[=!<>]=?|'[^']*'|""[^""]*""|\d+[hmdw]?|\S+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Time unit patterns (e.g., "4h" = 4 hours, "30m" = 30 minutes)
    private static readonly Regex TimeUnitPattern = new(@"^(\d+)([hmdw])$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public ConditionNode Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression cannot be empty", nameof(expression));

        var tokens = Tokenize(expression);
        var index = 0;
        return ParseExpression(tokens, ref index);
    }

    /// <inheritdoc />
    public bool Evaluate(string expression, Dictionary<string, object> context)
    {
        var node = Parse(expression);
        return node.Evaluate(context);
    }

    /// <inheritdoc />
    public ConditionValidationResult Validate(string expression)
    {
        var result = new ConditionValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(expression))
        {
            result.IsValid = false;
            result.Errors.Add("Expression cannot be empty");
            return result;
        }

        try
        {
            Parse(expression);
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Parse error: {ex.Message}");
        }

        // Check for balanced parentheses
        var parenCount = 0;
        foreach (var c in expression)
        {
            if (c == '(') parenCount++;
            if (c == ')') parenCount--;
            if (parenCount < 0)
            {
                result.IsValid = false;
                result.Errors.Add("Unbalanced parentheses: unexpected ')'");
                break;
            }
        }
        if (parenCount > 0)
        {
            result.IsValid = false;
            result.Errors.Add($"Unbalanced parentheses: missing {parenCount} closing ')'");
        }

        return result;
    }

    private List<string> Tokenize(string expression)
    {
        var matches = TokenPattern.Matches(expression);
        return matches.Select(m => m.Value).ToList();
    }

    private ConditionNode ParseExpression(List<string> tokens, ref int index)
    {
        var left = ParseTerm(tokens, ref index);

        while (index < tokens.Count)
        {
            var token = tokens[index].ToUpperInvariant();
            
            if (token == "OR")
            {
                index++;
                var right = ParseTerm(tokens, ref index);
                left = new LogicalNode
                {
                    Operator = LogicalOperator.Or,
                    Left = left,
                    Right = right
                };
            }
            else if (token == "AND")
            {
                index++;
                var right = ParseTerm(tokens, ref index);
                left = new LogicalNode
                {
                    Operator = LogicalOperator.And,
                    Left = left,
                    Right = right
                };
            }
            else
            {
                break;
            }
        }

        return left;
    }

    private ConditionNode ParseTerm(List<string> tokens, ref int index)
    {
        if (index >= tokens.Count)
            throw new ArgumentException("Unexpected end of expression");

        var token = tokens[index];

        // Handle NOT
        if (token.Equals("NOT", StringComparison.OrdinalIgnoreCase))
        {
            index++;
            return new NotNode { Inner = ParseTerm(tokens, ref index) };
        }

        // Handle parentheses
        if (token == "(")
        {
            index++;
            var inner = ParseExpression(tokens, ref index);
            
            if (index >= tokens.Count || tokens[index] != ")")
                throw new ArgumentException("Missing closing parenthesis");
            
            index++;
            return inner;
        }

        // Parse comparison
        return ParseComparison(tokens, ref index);
    }

    private ComparisonNode ParseComparison(List<string> tokens, ref int index)
    {
        if (index >= tokens.Count)
            throw new ArgumentException("Expected field name");

        var fieldName = tokens[index];
        index++;

        if (index >= tokens.Count)
            throw new ArgumentException($"Expected operator after '{fieldName}'");

        var opToken = tokens[index];
        index++;

        var op = ParseOperator(opToken);

        if (index >= tokens.Count)
            throw new ArgumentException($"Expected value after operator");

        var valueToken = tokens[index];
        index++;

        // Handle quoted strings
        if ((valueToken.StartsWith("'") && valueToken.EndsWith("'")) ||
            (valueToken.StartsWith("\"") && valueToken.EndsWith("\"")))
        {
            valueToken = valueToken.Substring(1, valueToken.Length - 2);
        }

        // Parse time units (e.g., "4h" to minutes)
        var value = ParseValue(valueToken);

        return new ComparisonNode
        {
            FieldName = fieldName,
            Operator = op,
            Value = value
        };
    }

    private static ComparisonOperator ParseOperator(string token)
    {
        return token switch
        {
            "=" => ComparisonOperator.Equals,
            "==" => ComparisonOperator.Equals,
            "!=" => ComparisonOperator.NotEquals,
            "<>" => ComparisonOperator.NotEquals,
            ">" => ComparisonOperator.GreaterThan,
            ">=" => ComparisonOperator.GreaterThanOrEqual,
            "<" => ComparisonOperator.LessThan,
            "<=" => ComparisonOperator.LessThanOrEqual,
            _ when token.Equals("CONTAINS", StringComparison.OrdinalIgnoreCase) => ComparisonOperator.Contains,
            _ => throw new ArgumentException($"Unknown operator: {token}")
        };
    }

    private static object ParseValue(string token)
    {
        // Try to parse as time unit (e.g., "4h", "30m")
        var timeMatch = TimeUnitPattern.Match(token);
        if (timeMatch.Success)
        {
            var amount = int.Parse(timeMatch.Groups[1].Value);
            var unit = timeMatch.Groups[2].Value.ToLowerInvariant();
            
            // Convert to minutes for consistent comparison
            return unit switch
            {
                "m" => amount,           // minutes
                "h" => amount * 60,      // hours to minutes
                "d" => amount * 60 * 24, // days to minutes
                "w" => amount * 60 * 24 * 7, // weeks to minutes
                _ => amount
            };
        }

        // Try to parse as number
        if (double.TryParse(token, out var number))
            return number;

        // Return as string
        return token;
    }
}
