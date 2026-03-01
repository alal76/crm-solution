import { describe, it, expect } from 'vitest';
import { compileTypeScript } from '../src/compiler';

/**
 * SARCH-037: Vitest test harness for the TypeScript AST security scanner
 * and SWC compilation pipeline.
 */
describe('TypeScript Compiler / AST Security Scanner', () => {
  it('should compile valid TypeScript', async () => {
    const result = await compileTypeScript('const x: number = 42;');
    expect(result.success).toBe(true);
    expect(result.code).toBeDefined();
  });

  it('should block eval() usage', async () => {
    const result = await compileTypeScript('eval("danger")');
    expect(result.success).toBe(false);
    expect(result.errors?.some(e => e.includes('eval'))).toBe(true);
  });

  it('should block new Function()', async () => {
    const result = await compileTypeScript('new Function("return this")');
    expect(result.success).toBe(false);
  });

  it('should block dynamic import()', async () => {
    const result = await compileTypeScript('import("os")');
    expect(result.success).toBe(false);
  });

  it('should block globalThis access', async () => {
    const result = await compileTypeScript('const g = globalThis;');
    expect(result.success).toBe(false);
  });

  it('should block process.env access', async () => {
    const result = await compileTypeScript('const key = process.env.SECRET;');
    expect(result.success).toBe(false);
  });

  it('should block require() calls', async () => {
    const result = await compileTypeScript('const fs = require("fs");');
    expect(result.success).toBe(false);
  });

  it('should block __dirname access', async () => {
    const result = await compileTypeScript('console.log(__dirname);');
    expect(result.success).toBe(false);
  });

  it('should block __filename access', async () => {
    const result = await compileTypeScript('console.log(__filename);');
    expect(result.success).toBe(false);
  });

  it('should allow safe TypeScript code', async () => {
    const source = `
      interface MyInput { name: string; }
      async function run(ctx: any): Promise<string> {
        return \`Hello \${ctx.input.name}\`;
      }
    `;
    const result = await compileTypeScript(source);
    expect(result.success).toBe(true);
  });

  it('should return source map for compiled output', async () => {
    const result = await compileTypeScript('const x: number = 42;');
    expect(result.success).toBe(true);
    expect(result.sourceMap).toBeDefined();
  });

  it('should return error for invalid TypeScript syntax', async () => {
    const result = await compileTypeScript('const x: = 42;');
    expect(result.success).toBe(false);
    expect(result.errors).toBeDefined();
  });
});
