import ivm from 'isolated-vm';
import { compileTypeScript } from './compiler';
import type { ScriptExecutionRequest, ScriptExecutionResponse } from './types/context';

/**
 * SARCH-032: Execute TypeScript scripts in an isolated V8 Isolate using isolated-vm.
 * The sandbox has no access to Node.js globals, file system, or network.
 * Input/output are passed via ExternalCopy to prevent prototype pollution.
 */
export async function executeScript<TIn, TOut>(
  req: ScriptExecutionRequest<TIn>
): Promise<ScriptExecutionResponse<TOut>> {
  const start = Date.now();
  const traceId = Math.random().toString(36).slice(2, 18);

  // Compile TypeScript to JS
  const compiled = await compileTypeScript(req.source);
  if (!compiled.success) {
    return { success: false, error: compiled.errors?.join('; '), traceId, durationMs: Date.now() - start };
  }

  const memoryMb = req.memoryLimitMb ?? 64;
  const timeoutMs = req.timeoutMs ?? 30_000;

  let isolate: ivm.Isolate | null = null;
  try {
    // Create isolated V8 isolate with memory limit
    isolate = new ivm.Isolate({ memoryLimit: memoryMb });
    const context = await isolate.createContext();
    const jail = context.global;

    // Inject minimal context into sandbox — no Node.js globals exposed
    await jail.set('__input__', new ivm.ExternalCopy(req.input).copyInto());
    await jail.set('__config__', new ivm.ExternalCopy(req.config ?? {}).copyInto());
    await jail.set('console', new ivm.Reference({
      log: new ivm.Reference((...args: unknown[]) => process.stdout.write(JSON.stringify(args) + '\n')),
    }));

    const wrappedCode = `
      (async function(ctx) {
        ${compiled.code}
        if (typeof run === 'function') return await run(ctx);
        throw new Error('Script must export a run(ctx) function');
      })({ input: __input__, config: __config__, tools: {}, logger: console, metrics: {}, secrets: {}, state: {}, env: {} })
    `;

    const resultRef = await context.eval(wrappedCode, { timeout: timeoutMs, promise: true });
    const output = resultRef as TOut;

    const memStat = isolate.getHeapStatisticsSync();

    return {
      success: true,
      output,
      traceId,
      durationMs: Date.now() - start,
      memoryPeakBytes: memStat.total_heap_size,
    };
  } catch (err) {
    return {
      success: false,
      error: (err as Error).message,
      traceId,
      durationMs: Date.now() - start,
    };
  } finally {
    isolate?.dispose();
  }
}
