// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.Workflow;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Services;

/// <summary>
/// PRA-004: Singleton service that provides entity field and related-entity schema
/// definitions for workflow configuration.  Delegates to <see cref="WorkflowFieldSchemas"/>
/// which holds the canonical schema dictionary, keeping the schemas in one place and
/// eliminating the hardcoded inline definitions that were previously in WorkflowController.
/// </summary>
public class WorkflowFieldSchemaService : IWorkflowFieldSchemaService
{
    /// <inheritdoc />
    public Dictionary<string, List<EntityFieldConfig>> GetAllFieldSchemas() =>
        WorkflowFieldSchemas.EntityFields;

    /// <inheritdoc />
    public List<EntityFieldConfig>? GetFieldSchemas(string entityType) =>
        WorkflowFieldSchemas.EntityFields.TryGetValue(entityType, out var fields) ? fields : null;

    /// <inheritdoc />
    public Dictionary<string, List<RelatedEntityConfig>> GetAllRelatedEntitySchemas() =>
        WorkflowFieldSchemas.RelatedEntities;
}
