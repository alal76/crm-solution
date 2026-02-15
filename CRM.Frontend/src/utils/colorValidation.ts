export const HEX_COLOR_REGEX = /^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$/;

export const isValidHexColor = (value?: string | null): boolean => {
  if (!value) return false;
  return HEX_COLOR_REGEX.test(value.trim());
};

export const normalizeHexColor = (value: string): string => {
  const trimmed = value.trim();
  if (!trimmed) return trimmed;
  if (trimmed.startsWith('#')) return trimmed;
  return `#${trimmed}`;
};
