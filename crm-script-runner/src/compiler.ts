import * as swc from '@swc/core';

const FORBIDDEN_PATTERNS = [
  /\beval\s*\(/,
  /\bnew\s+Function\s*\(/,
  /\bimport\s*\(/,
  /\bglobalThis\b/,
  /\bprocess\.env\b/,
  /\brequire\s*\(/,
  /\b__dirname\b/,
  /\b__filename\b/,
];

export interface CompileResult {
  success: boolean;
  code?: string;
  sourceMap?: string;
  errors?: string[];
}

/**
 * SARCH-031: Compile TypeScript source using SWC with an AST security scanner.
 * Step 1: Regex-based AST scan for forbidden patterns.
 * Step 2: SWC transpile TypeScript → JavaScript (CommonJS).
 */
export async function compileTypeScript(source: string): Promise<CompileResult> {
  // Step 1: AST scan for forbidden patterns
  const errors: string[] = [];
  for (const pattern of FORBIDDEN_PATTERNS) {
    if (pattern.test(source)) {
      errors.push(`Forbidden pattern detected: ${pattern.source}`);
    }
  }
  if (errors.length > 0) {
    return { success: false, errors };
  }

  // Step 2: SWC transpile TypeScript → JavaScript
  try {
    const result = await swc.transform(source, {
      filename: 'script.ts',
      jsc: {
        parser: { syntax: 'typescript', tsx: false },
        target: 'es2022',
        loose: false,
      },
      module: { type: 'commonjs' },
      sourceMaps: true,
    });
    return { success: true, code: result.code, sourceMap: result.map };
  } catch (err) {
    return { success: false, errors: [(err as Error).message] };
  }
}
