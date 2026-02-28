import { createHash } from 'crypto';

export function hashSha256(input: string): string {
  return createHash('sha256').update(input, 'utf-8').digest('hex');
}
