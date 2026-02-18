/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the Source-Available License (see LICENSE) as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * REFACTORED: This file now re-exports from the modular workflow folder.
 * For new code, prefer importing directly from './workflow':
 * 
 *   import { workflowService, WorkflowDefinition } from './workflow';
 * 
 * This module has been split into:
 * - workflow/enums.ts - Workflow status, node type, and task enums
 * - workflow/types.ts - Core workflow interfaces and DTOs
 * - workflow/aiTypes.ts - AI-enhanced workflow node configuration types
 * - workflow/workflowDefinitionApi.ts - API for workflow definitions
 * - workflow/workflowInstanceApi.ts - API for workflow instances
 * - workflow/index.ts - Barrel export
 */

// Re-export everything from the new module structure for backward compatibility
export * from './workflow';

// Re-export default for backward compatibility
export { default } from './workflow';
