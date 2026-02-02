/**
 * HTML sanitization utilities using DOMPurify.
 * Use these utilities whenever rendering user-provided or dynamic HTML content.
 */

import DOMPurify from 'dompurify';

// Configure DOMPurify defaults
const defaultConfig: DOMPurify.Config = {
  ALLOWED_TAGS: [
    'a', 'b', 'i', 'u', 'em', 'strong', 'p', 'br', 'hr',
    'ul', 'ol', 'li', 'span', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
    'table', 'thead', 'tbody', 'tr', 'th', 'td',
    'blockquote', 'code', 'pre', 'img', 'figure', 'figcaption',
  ],
  ALLOWED_ATTR: [
    'href', 'src', 'alt', 'title', 'class', 'id', 'style',
    'target', 'rel', 'width', 'height', 'colspan', 'rowspan',
  ],
  ALLOW_DATA_ATTR: false,
  ADD_ATTR: ['target'], // Allow target attribute for links
  ADD_TAGS: [], // No additional tags
  FORBID_TAGS: ['script', 'style', 'iframe', 'object', 'embed', 'form', 'input'],
  FORBID_ATTR: ['onerror', 'onload', 'onclick', 'onmouseover', 'onfocus', 'onblur'],
};

// Strict config for minimal HTML (just text formatting)
const strictConfig: DOMPurify.Config = {
  ALLOWED_TAGS: ['b', 'i', 'u', 'em', 'strong', 'br', 'p', 'span'],
  ALLOWED_ATTR: ['class'],
  ALLOW_DATA_ATTR: false,
};

// Config for rich text editors
const richTextConfig: DOMPurify.Config = {
  ...defaultConfig,
  ALLOWED_TAGS: [
    ...(defaultConfig.ALLOWED_TAGS || []),
    'sub', 'sup', 'mark', 'del', 'ins', 'abbr', 'cite',
  ],
};

/**
 * Sanitize HTML content using default configuration
 * @param html - The HTML string to sanitize
 * @returns Sanitized HTML string safe for rendering
 */
export function sanitizeHtml(html: string): string {
  return DOMPurify.sanitize(html, defaultConfig);
}

/**
 * Sanitize HTML with strict configuration (minimal tags)
 * Use for user comments, notes, etc.
 * @param html - The HTML string to sanitize
 * @returns Sanitized HTML string with only basic formatting
 */
export function sanitizeHtmlStrict(html: string): string {
  return DOMPurify.sanitize(html, strictConfig);
}

/**
 * Sanitize HTML for rich text editor content
 * Allows more tags for formatted content
 * @param html - The HTML string to sanitize
 * @returns Sanitized HTML string for rich content
 */
export function sanitizeRichText(html: string): string {
  return DOMPurify.sanitize(html, richTextConfig);
}

/**
 * Strip all HTML tags, returning only text content
 * @param html - The HTML string to strip
 * @returns Plain text without any HTML
 */
export function stripHtml(html: string): string {
  return DOMPurify.sanitize(html, { ALLOWED_TAGS: [], KEEP_CONTENT: true });
}

/**
 * Sanitize a URL to prevent javascript: and data: URLs
 * @param url - The URL to sanitize
 * @returns Safe URL or empty string if malicious
 */
export function sanitizeUrl(url: string): string {
  const trimmed = url.trim().toLowerCase();
  
  // Block dangerous protocols
  if (
    trimmed.startsWith('javascript:') ||
    trimmed.startsWith('data:') ||
    trimmed.startsWith('vbscript:')
  ) {
    return '';
  }
  
  // Allow http, https, mailto, tel, and relative URLs
  if (
    trimmed.startsWith('http://') ||
    trimmed.startsWith('https://') ||
    trimmed.startsWith('mailto:') ||
    trimmed.startsWith('tel:') ||
    trimmed.startsWith('/') ||
    trimmed.startsWith('#') ||
    !trimmed.includes(':')
  ) {
    return url;
  }
  
  return '';
}

/**
 * Escape HTML entities for safe display as text
 * Use when you want to show HTML as text, not render it
 * @param text - The text to escape
 * @returns HTML-escaped text
 */
export function escapeHtml(text: string): string {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

/**
 * Create a safe HTML props object for dangerouslySetInnerHTML
 * @param html - The HTML to sanitize and wrap
 * @returns Object suitable for dangerouslySetInnerHTML
 */
export function createSafeHtml(html: string): { __html: string } {
  return { __html: sanitizeHtml(html) };
}

/**
 * Create a safe HTML props object with strict sanitization
 * @param html - The HTML to sanitize and wrap
 * @returns Object suitable for dangerouslySetInnerHTML
 */
export function createSafeHtmlStrict(html: string): { __html: string } {
  return { __html: sanitizeHtmlStrict(html) };
}

export default {
  sanitizeHtml,
  sanitizeHtmlStrict,
  sanitizeRichText,
  stripHtml,
  sanitizeUrl,
  escapeHtml,
  createSafeHtml,
  createSafeHtmlStrict,
};
