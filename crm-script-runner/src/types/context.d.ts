/**
 * TypeScript type definitions for IScriptContext<TInput>.
 * These are generated to match the C# IScriptContext<TInput> contracts.
 * @engine/contracts
 */

export interface IScriptContext<TInput = unknown> {
  input: TInput;
  env: ExecutionEnvironment;
  tools: IToolInvoker;
  config: Record<string, unknown>;
  secrets: ISecretAccessor;
  state: IStateAccessor;
  metrics: IMetricsRecorder;
  logger: IScriptLogger;
}

export interface ExecutionEnvironment {
  tenantId: string;
  correlationId: string;
  callerId: string;
  workflowInstanceId?: string;
  agentId?: string;
}

export interface IToolInvoker {
  call<T = unknown>(toolName: string, parameters: unknown): Promise<ToolResult<T>>;
}

export interface ToolResult<T = unknown> {
  success: boolean;
  value?: T;
  error?: string;
  durationMs: number;
}

export interface IStateAccessor {
  get<T = unknown>(key: string): Promise<T | null>;
  set<T = unknown>(key: string, value: T): Promise<void>;
  delete(key: string): Promise<void>;
}

export interface ISecretAccessor {
  get(secretName: string): Promise<string | null>;
}

export interface IMetricsRecorder {
  increment(metric: string, value?: number): void;
  recordValue(metric: string, value: number): void;
}

export interface IScriptLogger {
  debug(message: string, ...args: unknown[]): void;
  info(message: string, ...args: unknown[]): void;
  warn(message: string, ...args: unknown[]): void;
  error(message: string, ...args: unknown[]): void;
}

export interface ScriptExecutionRequest<TInput = unknown> {
  scriptId: string;
  source: string;
  input: TInput;
  config?: Record<string, unknown>;
  timeoutMs?: number;
  memoryLimitMb?: number;
}

export interface ScriptExecutionResponse<TOutput = unknown> {
  success: boolean;
  output?: TOutput;
  error?: string;
  traceId: string;
  durationMs: number;
  memoryPeakBytes?: number;
}
