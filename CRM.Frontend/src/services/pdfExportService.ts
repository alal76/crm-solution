/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the Source-Available License (see LICENSE) v3.0
 * 
 * PDF Export Service - Browser-based PDF generation using print functionality
 */

export interface PDFExportOptions {
  title: string;
  subtitle?: string;
  logoUrl?: string;
  headerColor?: string;
  includeDate?: boolean;
  orientation?: 'portrait' | 'landscape';
  pageSize?: 'A4' | 'Letter';
}

export interface TableColumn {
  header: string;
  field: string;
  width?: string;
  align?: 'left' | 'center' | 'right';
  format?: (value: any) => string;
}

export interface PDFSection {
  title?: string;
  content?: string;
  table?: {
    columns: TableColumn[];
    data: Record<string, unknown>[];
  };
  fields?: { label: string; value: string | number | undefined }[];
}

/**
 * Generate CSS styles for print/PDF
 */
const getBaseStyles = (options: PDFExportOptions): string => `
  @page {
    size: ${options.pageSize || 'A4'} ${options.orientation || 'portrait'};
    margin: 20mm;
  }
  
  * {
    box-sizing: border-box;
  }
  
  body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif;
    font-size: 12px;
    line-height: 1.5;
    color: #333;
    margin: 0;
    padding: 20px;
  }
  
  .pdf-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    border-bottom: 3px solid ${options.headerColor || '#1976d2'};
    padding-bottom: 15px;
    margin-bottom: 20px;
  }
  
  .pdf-header-left {
    flex: 1;
  }
  
  .pdf-header-right {
    text-align: right;
  }
  
  .pdf-logo {
    height: 50px;
    margin-bottom: 10px;
  }
  
  .pdf-title {
    font-size: 24px;
    font-weight: bold;
    color: ${options.headerColor || '#1976d2'};
    margin: 0;
  }
  
  .pdf-subtitle {
    font-size: 14px;
    color: #666;
    margin: 5px 0 0 0;
  }
  
  .pdf-date {
    font-size: 11px;
    color: #888;
  }
  
  .pdf-section {
    margin-bottom: 25px;
  }
  
  .pdf-section-title {
    font-size: 16px;
    font-weight: bold;
    color: ${options.headerColor || '#1976d2'};
    border-bottom: 1px solid #ddd;
    padding-bottom: 5px;
    margin-bottom: 15px;
  }
  
  .pdf-field-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
  }
  
  .pdf-field {
    display: flex;
    padding: 5px 0;
    border-bottom: 1px dotted #eee;
  }
  
  .pdf-field-label {
    font-weight: bold;
    color: #555;
    width: 140px;
    flex-shrink: 0;
  }
  
  .pdf-field-value {
    flex: 1;
    color: #333;
  }
  
  .pdf-table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 10px;
  }
  
  .pdf-table th {
    background: ${options.headerColor || '#1976d2'};
    color: white;
    padding: 10px 8px;
    text-align: left;
    font-weight: 600;
    font-size: 11px;
    text-transform: uppercase;
  }
  
  .pdf-table td {
    padding: 8px;
    border-bottom: 1px solid #eee;
  }
  
  .pdf-table tr:nth-child(even) {
    background: #f9f9f9;
  }
  
  .pdf-table tr:hover {
    background: #f0f0f0;
  }
  
  .text-right { text-align: right; }
  .text-center { text-align: center; }
  
  .pdf-footer {
    margin-top: 30px;
    padding-top: 15px;
    border-top: 1px solid #ddd;
    font-size: 10px;
    color: #888;
    text-align: center;
  }
  
  .pdf-content {
    margin: 15px 0;
    line-height: 1.6;
  }
  
  .pdf-status-badge {
    display: inline-block;
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 10px;
    font-weight: bold;
  }
  
  .status-success { background: #e8f5e9; color: #2e7d32; }
  .status-warning { background: #fff3e0; color: #e65100; }
  .status-error { background: #ffebee; color: #c62828; }
  .status-info { background: #e3f2fd; color: #1565c0; }
  
  @media print {
    body {
      padding: 0;
    }
    
    .no-print {
      display: none !important;
    }
    
    .pdf-section {
      page-break-inside: avoid;
    }
  }
`;

/**
 * Generate the PDF header HTML
 */
const generateHeader = (options: PDFExportOptions): string => `
  <div class="pdf-header">
    <div class="pdf-header-left">
      ${options.logoUrl ? `<img src="${options.logoUrl}" alt="Logo" class="pdf-logo" />` : ''}
      <h1 class="pdf-title">${options.title}</h1>
      ${options.subtitle ? `<p class="pdf-subtitle">${options.subtitle}</p>` : ''}
    </div>
    <div class="pdf-header-right">
      ${options.includeDate !== false ? `<p class="pdf-date">Generated: ${new Date().toLocaleString()}</p>` : ''}
    </div>
  </div>
`;

/**
 * Generate a section with fields
 */
const generateFieldsSection = (section: PDFSection): string => {
  if (!section.fields?.length) return '';
  
  const fieldsHtml = section.fields
    .filter(f => f.value !== undefined && f.value !== null && f.value !== '')
    .map(f => `
      <div class="pdf-field">
        <span class="pdf-field-label">${f.label}:</span>
        <span class="pdf-field-value">${f.value}</span>
      </div>
    `).join('');
  
  return `
    <div class="pdf-section">
      ${section.title ? `<div class="pdf-section-title">${section.title}</div>` : ''}
      <div class="pdf-field-grid">${fieldsHtml}</div>
    </div>
  `;
};

/**
 * Generate a section with a table
 */
const generateTableSection = (section: PDFSection): string => {
  if (!section.table?.columns?.length || !section.table?.data?.length) return '';
  
  const headerCells = section.table.columns.map(col => 
    `<th style="${col.width ? `width: ${col.width}` : ''}" class="${col.align ? `text-${col.align}` : ''}">${col.header}</th>`
  ).join('');
  
  const dataRows = section.table.data.map(row => {
    const cells = section.table!.columns.map(col => {
      const value = row[col.field];
      const displayValue = col.format ? col.format(value) : (value ?? '-');
      return `<td class="${col.align ? `text-${col.align}` : ''}">${displayValue}</td>`;
    }).join('');
    return `<tr>${cells}</tr>`;
  }).join('');
  
  return `
    <div class="pdf-section">
      ${section.title ? `<div class="pdf-section-title">${section.title}</div>` : ''}
      <table class="pdf-table">
        <thead><tr>${headerCells}</tr></thead>
        <tbody>${dataRows}</tbody>
      </table>
    </div>
  `;
};

/**
 * Generate a content section
 */
const generateContentSection = (section: PDFSection): string => {
  if (!section.content) return '';
  
  return `
    <div class="pdf-section">
      ${section.title ? `<div class="pdf-section-title">${section.title}</div>` : ''}
      <div class="pdf-content">${section.content}</div>
    </div>
  `;
};

/**
 * Generate the complete PDF document and open in a new window for printing
 */
export const generatePDF = (
  options: PDFExportOptions,
  sections: PDFSection[]
): Window | null => {
  const printWindow = window.open('', '_blank');
  if (!printWindow) {
    console.error('Failed to open print window. Check popup blocker settings.');
    return null;
  }
  
  const sectionsHtml = sections.map(section => {
    if (section.table) return generateTableSection(section);
    if (section.fields) return generateFieldsSection(section);
    if (section.content) return generateContentSection(section);
    return '';
  }).join('');
  
  const html = `
    <!DOCTYPE html>
    <html>
    <head>
      <meta charset="UTF-8">
      <title>${options.title}</title>
      <style>${getBaseStyles(options)}</style>
    </head>
    <body>
      ${generateHeader(options)}
      ${sectionsHtml}
      <div class="pdf-footer">
        CRM Solution &copy; ${new Date().getFullYear()} - Confidential
      </div>
      <div class="no-print" style="margin-top: 20px; text-align: center;">
        <button onclick="window.print()" style="padding: 10px 30px; font-size: 14px; cursor: pointer; background: ${options.headerColor || '#1976d2'}; color: white; border: none; border-radius: 4px;">
          Print / Save as PDF
        </button>
        <button onclick="window.close()" style="padding: 10px 30px; font-size: 14px; cursor: pointer; margin-left: 10px; background: #666; color: white; border: none; border-radius: 4px;">
          Close
        </button>
      </div>
    </body>
    </html>
  `;
  
  printWindow.document.write(html);
  printWindow.document.close();
  
  return printWindow;
};

/**
 * Quick export for a single entity (e.g., quote, contract, etc.)
 */
export const exportEntityToPDF = (
  entityType: string,
  entityData: Record<string, unknown>,
  fieldMappings: { label: string; field: string; format?: (val: any) => string }[],
  options?: Partial<PDFExportOptions>
): Window | null => {
  const fields = fieldMappings.map(m => ({
    label: m.label,
    value: m.format ? m.format(entityData[m.field]) : String(entityData[m.field] ?? ''),
  }));
  
  return generatePDF(
    {
      title: `${entityType} Details`,
      subtitle: String(entityData['name'] || entityData['title'] || `#${entityData['id']}`),
      includeDate: true,
      ...options,
    },
    [{ title: 'Details', fields }]
  );
};

/**
 * Quick export for a list/table
 */
export const exportTableToPDF = (
  title: string,
  columns: TableColumn[],
  data: Record<string, unknown>[],  
  options?: Partial<PDFExportOptions>
): Window | null => {
  return generatePDF(
    {
      title,
      includeDate: true,
      orientation: columns.length > 5 ? 'landscape' : 'portrait',
      ...options,
    },
    [{ table: { columns, data } }]
  );
};

// ==================== Format Helpers ====================

export const formatCurrency = (value: number | undefined): string => {
  if (value === undefined || value === null) return '-';
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
};

export const formatDate = (dateString: string | undefined): string => {
  if (!dateString) return '-';
  return new Date(dateString).toLocaleDateString();
};

export const formatDateTime = (dateString: string | undefined): string => {
  if (!dateString) return '-';
  return new Date(dateString).toLocaleString();
};

export const formatPercent = (value: number | undefined): string => {
  if (value === undefined || value === null) return '-';
  return `${value.toFixed(1)}%`;
};

export const formatNumber = (value: number | undefined): string => {
  if (value === undefined || value === null) return '-';
  return new Intl.NumberFormat('en-US').format(value);
};

export const formatBoolean = (value: boolean | undefined): string => {
  if (value === undefined || value === null) return '-';
  return value ? 'Yes' : 'No';
};

export default {
  generatePDF,
  exportEntityToPDF,
  exportTableToPDF,
  formatCurrency,
  formatDate,
  formatDateTime,
  formatPercent,
  formatNumber,
  formatBoolean,
};
