// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CRM.Infrastructure.Scripting.Security;

/// <summary>
/// Roslyn diagnostic analyzer that blocks forbidden APIs in user scripts.
/// Runs at compile time — compilation fails if any violation is found.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SecureScriptAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor ForbiddenApiRule = new(
        id: "SCRPT001",
        title: "Forbidden API usage detected",
        messageFormat: "'{0}' is not permitted in CRM scripts. Use the Tool Bridge instead.",
        category: "Security",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ForbiddenNamespaceRule = new(
        id: "SCRPT002",
        title: "Forbidden namespace access detected",
        messageFormat: "Namespace '{0}' is not permitted in CRM scripts.",
        category: "Security",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableHashSet<string> ForbiddenTypes = ImmutableHashSet.Create(
        "System.IO.File",
        "System.IO.Directory",
        "System.IO.Stream",
        "System.Net.Http.HttpClient",
        "System.Net.WebClient",
        "System.Reflection.Assembly",
        "System.Runtime.InteropServices.Marshal",
        "System.Diagnostics.Process",
        "System.Threading.Thread",
        "System.Environment",
        "System.AppDomain");

    private static readonly ImmutableHashSet<string> ForbiddenNamespaces = ImmutableHashSet.Create(
        "System.IO",
        "System.Net",
        "System.Reflection",
        "System.Runtime.InteropServices",
        "System.Diagnostics",
        "Microsoft.Win32");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(ForbiddenApiRule, ForbiddenNamespaceRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is MemberAccessExpressionSyntax memberAccess)
        {
            var symbol = ctx.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
            if (symbol?.ContainingType != null)
            {
                var fullName = symbol.ContainingType.ToDisplayString();
                if (ForbiddenTypes.Contains(fullName))
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(ForbiddenApiRule, memberAccess.GetLocation(), fullName));
                }
            }
        }
    }

    private static void AnalyzeUsing(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is UsingDirectiveSyntax usingDirective)
        {
            var ns = usingDirective.Name?.ToString() ?? string.Empty;
            foreach (var forbidden in ForbiddenNamespaces)
            {
                if (ns.StartsWith(forbidden, System.StringComparison.Ordinal))
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(ForbiddenNamespaceRule, usingDirective.GetLocation(), ns));
                    break;
                }
            }
        }
    }
}
