// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TODO-SD005-003: Unit tests verifying the IEscalationRuleService renaming and interface hierarchy.

using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Verifies that TODO-SD005-003 (rename IEscalationRuleAdminService → IEscalationRuleService)
/// is correctly applied: interface hierarchy, concrete implementations, and method count.
/// </summary>
public class EscalationRuleInterfaceRenameTests
{
    [Fact]
    public void EscalationRuleAdminService_ImplementsIEscalationRuleService()
    {
        // The concrete admin service must implement the new canonical interface
        typeof(EscalationRuleAdminService)
            .IsAssignableTo(typeof(IEscalationRuleService))
            .Should().BeTrue("EscalationRuleAdminService must implement IEscalationRuleService after rename");
    }

#pragma warning disable CS0618 // intentionally testing obsolete type
    [Fact]
    public void IEscalationRuleAdminService_ExtendsIEscalationRuleService()
    {
        // The deprecated adaptor interface must still extend the new one for backward compat
        typeof(IEscalationRuleAdminService)
            .IsAssignableTo(typeof(IEscalationRuleService))
            .Should().BeTrue("IEscalationRuleAdminService must extend IEscalationRuleService for backward compatibility");
    }
#pragma warning restore CS0618

    [Fact]
    public void EscalationRuleService_ImplementsIEscalationRulePolicyService()
    {
        // The SLA-focused runtime service must implement the renamed policy interface
        typeof(EscalationRuleService)
            .IsAssignableTo(typeof(IEscalationRulePolicyService))
            .Should().BeTrue("EscalationRuleService must implement IEscalationRulePolicyService");
    }

    [Fact]
    public void IEscalationRuleService_HasSevenMethods()
    {
        // Verify the canonical admin CRUD interface has exactly 7 methods
        var methods = typeof(IEscalationRuleService).GetMethods();
        const string reason = "IEscalationRuleService should declare CreateAsync, UpdateAsync, GetByIdAsync, GetAllAsync, DeleteAsync, TestRuleAsync, GetApplicableRulesAsync";
        methods.Length.Should().Be(7, reason);
    }

    [Fact]
    public void EscalationRuleAdminService_DoesNotImplementIEscalationRulePolicyService()
    {
        // The admin service handles CRUD — it must NOT be assignable to the SLA-enforcement policy interface
        typeof(EscalationRuleAdminService)
            .IsAssignableTo(typeof(IEscalationRulePolicyService))
            .Should().BeFalse("Admin CRUD service should not implement the SLA policy interface");
    }
}
