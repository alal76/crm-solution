// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Lead Service — generated via createCrudService factory.
// All CRUD operations are provided by the factory; see
// src/services/crudServiceFactory.ts for implementation details.
//
// MIGRATION NOTE: This service previously returned the raw Axios response
// object from each method. It now returns unwrapped data directly (matching the
// factory contract). Callers that previously accessed `(await leadService.getAll()).data`
// should now use the result directly, e.g. `(await leadService.getAll()).items`.

import { createCrudService } from './crudServiceFactory';

// ── Entity type ──────────────────────────────────────────────────────────────────

export interface Lead {
  id: number;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  companyName?: string;
  status?: string;
  source?: string;
  score?: number;
  ownerId?: number;
  createdAt?: string;
  updatedAt?: string;
}

// ── Service ────────────────────────────────────────────────────────────────────────

const leadService = createCrudService<Lead>('/leads');

export { leadService };
export default leadService;
