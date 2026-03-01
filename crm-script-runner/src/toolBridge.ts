/**
 * Tool Bridge client for TypeScript scripts.
 *
 * Scripts MUST NOT call platform APIs directly — all external calls must
 * go through this bridge. In the isolated-vm execution context the bridge
 * is injected by the crm-script-runner host via `ctx.tools`; this module
 * provides the TypeScript interface definition and a standalone stub that
 * throws when invoked outside the runner.
 */

/** Options used to construct the bridge client (provided by the host). */
export interface ToolBridgeOptions {
  /** C# ToolBridgeInvoker callback URL (used when the bridge is HTTP-backed). */
  hostUrl: string;
  /** Unique identifier for the current script execution. */
  executionId: string;
  /** Permission names granted to this script execution. */
  permissions: string[];
}

/**
 * Client-side representation of a tool invocation result.
 * Mirrors `ToolResult<TResult>` from `CRM.Core.Scripting`.
 */
export interface ToolResult<T> {
  success: boolean;
  value?: T;
  error?: string;
  /** Wall-clock duration in milliseconds. */
  durationMs: number;
}

/**
 * Tool Bridge client for TypeScript scripts.
 *
 * In the isolated-vm context, `ctx.tools` is pre-wired by the host;
 * scripts call `await ctx.tools.call<MyResult>("ToolName", { ...params })`.
 *
 * **This class is never instantiated inside a user script directly.**
 * It is exported for use in the `@engine/testing` harness and for
 * IDE type-checking inside the runner package.
 */
export class ToolBridgeClient {
  constructor(private readonly opts: ToolBridgeOptions) {}

  /**
   * Calls a named platform tool registered in the CRM ToolRegistry.
   *
   * @param toolName - Name of the tool (e.g., `"GetCustomer"`).
   * @param parameters - Input parameters for the tool (must be JSON-serialisable).
   * @returns The tool's result, including success flag and value or error message.
   * @throws {Error} When called outside the crm-script-runner host context.
   */
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  async call<T>(toolName: string, parameters: unknown): Promise<ToolResult<T>> {
    // In the runner context this method is replaced by the host via
    // isolated-vm Reference injection (applySync / applySyncPromise).
    throw new Error(
      `Tool Bridge not available in standalone mode. ` +
        `Tool '${toolName}' must be called via ctx.tools inside the crm-script-runner.`,
    );
  }
}

/** Default export for convenience in script entry points. */
export default ToolBridgeClient;
