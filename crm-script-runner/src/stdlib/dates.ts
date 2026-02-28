export function formatDate(date: Date | string, format = 'ISO'): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  if (format === 'ISO') return d.toISOString();
  return d.toLocaleDateString();
}

export function parseDate(str: string): Date {
  return new Date(str);
}

export function addDays(date: Date, days: number): Date {
  const d = new Date(date);
  d.setDate(d.getDate() + days);
  return d;
}
