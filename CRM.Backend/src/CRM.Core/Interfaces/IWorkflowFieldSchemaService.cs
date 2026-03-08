// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.Workflow;

namespace CRM.Core.Interfaces;

/// <summary>
/// PRA-004: Abstracts the entity field and related-entity schema definitions used in
/// workflow condition/action configuration, removing the 12+ hardcoded schemas from
/// WorkflowController and making them externally configurable.
/// </summary>
public interface IWorkflowFieldSchemaService
{
    /// <summary>
    /// Gets all entity field schemas keyed by entity type name (e.g. "Lead", "Account").
    /// </summary>
    Dictionary<string, List<EntityFieldConfig>> GetAllFieldSchemas();

    /// <summary>
    /// Gets the field schema list for a specific entity type.
    /// Returns null if the entity type is not registered.
    /// </summary>
    List<EntityFieldConfig>? GetFieldSchemas(string entityType);

    /// <summary>
    /// Gets all related-entity schemas keyed by entity type name.
    /// </summary>
    Dictionary<string, List<RelatedEntityConfig>> GetAllRelatedEntitySchemas();
}
