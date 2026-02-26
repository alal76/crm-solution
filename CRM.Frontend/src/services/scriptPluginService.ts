/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Script Plugin Service — CRUD and ad-hoc execution for the scripting engine.
 * Backend: /api/scripting/* (ScriptingController)
 */

import apiClient from './apiClient';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

/** ScriptLanguage enum: 0 = JavaScript, 1 = Python, 2 = CSharp */
export type ScriptLanguage = 0 | 1 | 2;

export interface ScriptPluginDto {
  id: number;
  name: string;
  description: string | null;
  language: ScriptLanguage;
  code: string;
  parameterSchema: string | null;
  returnValueDescription: string | null;
  isActive: boolean;
  version: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateScriptPluginDto {
  name: string;
  description?: string;
  language: ScriptLanguage;
  code: string;
  parameterSchema?: string;
  returnValueDescription?: string;
}

export interface UpdateScriptPluginDto {
  name: string;
  description?: string;
  code: string;
  parameterSchema?: string;
  returnValueDescription?: string;
  isActive: boolean;
}

export interface ScriptPluginTestRequest {
  variables: Record<string, unknown>;
  context: Record<string, unknown>;
  timeout?: number;
}

export interface ScriptPluginTestResult {
  success: boolean;
  returnValue: unknown;
  logs: string[];
  errorMessage: string | null;
  executionTime: string;
}

export interface ScriptValidateRequest {
  language: ScriptLanguage;
  code: string;
}

export interface ScriptDiagnostic {
  line: number;
  column: number;
  message: string;
  severity: string;
}

export interface ScriptValidateResult {
  isValid: boolean;
  diagnostics: ScriptDiagnostic[];
}

export interface ScriptExecuteRequest {
  language: ScriptLanguage;
  code: string;
  variables?: Record<string, unknown>;
  timeout?: number;
}

export interface ScriptExecuteResult {
  success: boolean;
  returnValue: unknown;
  logs: string[];
  errorMessage: string | null;
  executionTimeMs: number;
}

export interface ScriptEngineInfo {
  name: string;
  language: ScriptLanguage;
  isAvailable: boolean;
}

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

const BASE = '/scripting';

// ---------------------------------------------------------------------------
// Plugin CRUD
// ---------------------------------------------------------------------------

export const getScriptPlugins = (includeInactive = false): Promise<ScriptPluginDto[]> =>
  apiClient
    .get<ScriptPluginDto[]>(`${BASE}/plugins`, { params: { includeInactive } })
    .then((res) => res.data);

export const getScriptPlugin = (id: number): Promise<ScriptPluginDto> =>
  apiClient.get<ScriptPluginDto>(`${BASE}/plugins/${id}`).then((res) => res.data);

export const createScriptPlugin = (dto: CreateScriptPluginDto): Promise<ScriptPluginDto> =>
  apiClient.post<ScriptPluginDto>(`${BASE}/plugins`, dto).then((res) => res.data);

export const updateScriptPlugin = (
  id: number,
  dto: UpdateScriptPluginDto,
): Promise<ScriptPluginDto> =>
  apiClient.put<ScriptPluginDto>(`${BASE}/plugins/${id}`, dto).then((res) => res.data);

export const deleteScriptPlugin = (id: number): Promise<void> =>
  apiClient.delete(`${BASE}/plugins/${id}`).then(() => undefined);

export const testScriptPlugin = (
  id: number,
  req: ScriptPluginTestRequest,
): Promise<ScriptPluginTestResult> =>
  apiClient
    .post<ScriptPluginTestResult>(`${BASE}/plugins/${id}/test`, req)
    .then((res) => res.data);

// ---------------------------------------------------------------------------
// Ad-hoc scripting
// ---------------------------------------------------------------------------

export const validateScript = (req: ScriptValidateRequest): Promise<ScriptValidateResult> =>
  apiClient.post<ScriptValidateResult>(`${BASE}/validate`, req).then((res) => res.data);

export const executeScript = (req: ScriptExecuteRequest): Promise<ScriptExecuteResult> =>
  apiClient.post<ScriptExecuteResult>(`${BASE}/execute`, req).then((res) => res.data);

export const getScriptEngines = (): Promise<ScriptEngineInfo[]> =>
  apiClient.get<ScriptEngineInfo[]>(`${BASE}/engines`).then((res) => res.data);

// ---------------------------------------------------------------------------
// Default export (object style for consumers that prefer it)
// ---------------------------------------------------------------------------

const scriptPluginService = {
  getScriptPlugins,
  getScriptPlugin,
  createScriptPlugin,
  updateScriptPlugin,
  deleteScriptPlugin,
  testScriptPlugin,
  validateScript,
  executeScript,
  getScriptEngines,
};

export default scriptPluginService;
