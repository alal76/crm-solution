/**
 * BulkExportButton.tsx
 * CSV export button for downloading all accounts data
 */
import React, { useState } from 'react';
import { Button, CircularProgress, Tooltip, SxProps, Theme } from '@mui/material';
import { Download as DownloadIcon } from '@mui/icons-material';
import apiClient from '../../services/apiClient';
import logger from '../../services/logger';

interface BulkExportButtonProps {
  variant?: 'text' | 'outlined' | 'contained';
  size?: 'small' | 'medium' | 'large';
  color?: 'inherit' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';
  disabled?: boolean;
  sx?: SxProps<Theme>;
}

/**
 * Convert data to CSV format
 */
const convertToCSV = (data: any[]): string => {
  if (data.length === 0) return '';

  // Define CSV headers
  const headers = [
    'First Name',
    'Last Name',
    'Email',
    'Phone',
    'Company',
    'Category',
    'Industry',
    'Account Type',
    'Lifecycle Stage',
    'Priority',
    'Owner',
    'Health Score',
  ];

  // Helper to escape CSV values
  const escapeCSV = (value: any): string => {
    if (value === null || value === undefined) return '';
    const stringValue = String(value);
    if (stringValue.includes(',') || stringValue.includes('"') || stringValue.includes('\n')) {
      return `"${stringValue.replace(/"/g, '""')}"`;
    }
    return stringValue;
  };

  // Map account data to CSV row
  const rows = data.map(account => [
    escapeCSV(account.firstName),
    escapeCSV(account.lastName),
    escapeCSV(account.email),
    escapeCSV(account.phone),
    escapeCSV(account.company),
    escapeCSV(account.category),
    escapeCSV(account.industry),
    escapeCSV(account.accountType),
    escapeCSV(account.lifecycleStage),
    escapeCSV(account.priority),
    escapeCSV(account.ownerName),
    escapeCSV(account.healthScore),
  ]);

  // Combine headers and rows
  return [headers.join(','), ...rows.map(row => row.join(','))].join('\n');
};

/**
 * Trigger CSV download
 */
const downloadCSV = (csvContent: string, filename: string) => {
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
  const link = document.createElement('a');
  const url = URL.createObjectURL(blob);

  link.setAttribute('href', url);
  link.setAttribute('download', filename);
  link.style.visibility = 'hidden';

  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);

  URL.revokeObjectURL(url);
};

const BulkExportButton: React.FC<BulkExportButtonProps> = ({
  variant = 'outlined',
  size = 'medium',
  color = 'primary',
  disabled = false,
  sx,
}) => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleExport = async () => {
    setIsLoading(true);
    setError(null);

    try {
      // Fetch all accounts
      const response = await apiClient.get('/accounts', {
        params: {
          pageSize: 999999, // Fetch all
          sortBy: 'firstName',
          sortOrder: 'asc',
        },
      });

      const accounts = response.data.items || [];

      if (accounts.length === 0) {
        setError('No accounts to export');
        setIsLoading(false);
        return;
      }

      // Convert to CSV
      const csvContent = convertToCSV(accounts);

      // Generate filename with current date
      const now = new Date();
      const dateString = now.toISOString().split('T')[0]; // YYYY-MM-DD
      const filename = `accounts_export_${dateString}.csv`;

      // Trigger download
      downloadCSV(csvContent, filename);

      logger.info(`Successfully exported ${accounts.length} accounts to ${filename}`);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Failed to export accounts';
      setError(errorMessage);
      logger.error('Account export failed', errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  const tooltip = error
    ? error
    : 'Download all accounts as CSV file';

  return (
    <Tooltip title={tooltip}>
      <span>
        <Button
          variant={variant}
          size={size}
          color={color}
          disabled={disabled || isLoading}
          onClick={handleExport}
          startIcon={isLoading ? <CircularProgress size={20} /> : <DownloadIcon />}
          sx={sx}
        >
          {isLoading ? 'Exporting...' : 'Export CSV'}
        </Button>
      </span>
    </Tooltip>
  );
};

export default BulkExportButton;
