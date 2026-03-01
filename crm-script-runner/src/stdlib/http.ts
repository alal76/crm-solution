/**
 * Tool Bridge HTTP proxy — all outbound HTTP from scripts MUST pass through
 * ctx.tools.call() which enforces policy, audit logging, and rate limiting.
 * Direct fetch/axios usage is blocked by the AST security scanner.
 *
 * This module is intentionally empty. It exists as an explicit reminder
 * that HTTP access from within the sandbox is only permitted via the
 * IToolInvoker bridge, never via direct network calls.
 */

// No exports — direct HTTP access is blocked at compile time by the AST scanner.
// Use ctx.tools.call('http.get', { url, headers }) for outbound requests.
