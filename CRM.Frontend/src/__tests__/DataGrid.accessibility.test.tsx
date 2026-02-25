/**
 * DataGrid Accessibility Tests — task TODO-UX-01/02/03
 * Tests ARIA attributes, keyboard navigation and screen-reader announcements
 * against WCAG 2.1 AA requirements.
 */
import React from 'react';
import { render, screen, fireEvent, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { DataGrid, DataGridColumn } from '../components/common/DataGrid';

// ─── Test data ────────────────────────────────────────────────────────────────

interface TestRow {
  id: number;
  name: string;
  email: string;
  status: string;
}

const columns: DataGridColumn<TestRow>[] = [
  { field: 'name', headerName: 'Name', sortable: true },
  { field: 'email', headerName: 'Email', sortable: true },
  { field: 'status', headerName: 'Status', sortable: false },
];

const rows: TestRow[] = [
  { id: 1, name: 'Alice Smith', email: 'alice@example.com', status: 'Active' },
  { id: 2, name: 'Bob Jones', email: 'bob@example.com', status: 'Inactive' },
  { id: 3, name: 'Carol White', email: 'carol@example.com', status: 'Active' },
];

// ─── Wrappers ─────────────────────────────────────────────────────────────────

const renderGrid = (props: Partial<React.ComponentProps<typeof DataGrid<TestRow>>> = {}) =>
  render(
    <DataGrid<TestRow>
      rows={rows}
      columns={columns}
      ariaLabel="Test data grid"
      totalCount={rows.length}
      {...props}
    />
  );

// ─── Tests ───────────────────────────────────────────────────────────────────

describe('DataGrid Accessibility (WCAG 2.1 AA)', () => {
  // ── ARIA attributes ────────────────────────────────────────────────────────

  it('renders a <table> with aria-label', () => {
    renderGrid();
    const table = screen.getByRole('table', { name: /test data grid/i });
    expect(table).toBeInTheDocument();
  });

  it('applies aria-rowcount matching totalCount prop', () => {
    renderGrid({ totalCount: 42 });
    const table = screen.getByRole('table');
    expect(table).toHaveAttribute('aria-rowcount', '42');
  });

  it('applies aria-colcount matching number of visible columns', () => {
    renderGrid();
    const table = screen.getByRole('table');
    // 3 columns → aria-colcount="3"
    expect(table).toHaveAttribute('aria-colcount', '3');
  });

  it('applies aria-colcount = 4 when selectable (selection column added)', () => {
    renderGrid({ selectable: true });
    const table = screen.getByRole('table');
    expect(table).toHaveAttribute('aria-colcount', '4');
  });

  it('sets aria-sort="ascending" on sorted column header', () => {
    renderGrid({ sortField: 'name', sortDirection: 'asc', onSortChange: jest.fn() });
    const nameHeader = screen.getByRole('columnheader', { name: /sort by name/i });
    expect(nameHeader).toHaveAttribute('aria-sort', 'ascending');
  });

  it('sets aria-sort="descending" on sorted column header', () => {
    renderGrid({ sortField: 'email', sortDirection: 'desc', onSortChange: jest.fn() });
    const emailHeader = screen.getByRole('columnheader', { name: /sort by email/i });
    expect(emailHeader).toHaveAttribute('aria-sort', 'descending');
  });

  it('does not set aria-sort on unsortable column header', () => {
    renderGrid({ onSortChange: jest.fn() });
    // "Status" column has sortable: false
    const statusHeaders = screen.getAllByRole('columnheader');
    const statusHeader = statusHeaders.find((h) => h.textContent === 'Status');
    expect(statusHeader).not.toHaveAttribute('aria-sort');
  });

  it('renders aria-selected on selected rows', () => {
    renderGrid({ selectable: true, selectedIds: [1], onSelectionChange: jest.fn() });
    const rows = screen.getAllByRole('row');
    // rows[0] = header row, rows[1] = first data row (id=1)
    expect(rows[1]).toHaveAttribute('aria-selected', 'true');
    expect(rows[2]).not.toHaveAttribute('aria-selected');
  });

  // ── Loading indicator accessibility ───────────────────────────────────────

  it('has a role="status" aria-live="polite" region for loading announcements', () => {
    renderGrid({ loading: false });
    const liveRegion = screen.getByRole('status');
    expect(liveRegion).toHaveAttribute('aria-live', 'polite');
    expect(liveRegion).toHaveAttribute('aria-atomic', 'true');
  });

  it('announces "Loading data" text when loading=true', () => {
    renderGrid({ loading: true });
    const liveRegion = screen.getByRole('status');
    expect(liveRegion.textContent).toContain('Loading data');
  });

  it('announces row count when data is loaded', () => {
    renderGrid({ loading: false });
    const liveRegion = screen.getByRole('status');
    expect(liveRegion.textContent).toContain('3 rows');
  });

  // ── Empty state ────────────────────────────────────────────────────────────

  it('announces custom empty message', () => {
    renderGrid({ rows: [], emptyMessage: 'Nothing to show' });
    const liveRegion = screen.getByRole('status');
    expect(liveRegion.textContent).toContain('Nothing to show');
  });

  // ── Column headers with sort buttons ──────────────────────────────────────

  it('column sort buttons have descriptive aria-label', () => {
    renderGrid({ onSortChange: jest.fn() });
    expect(screen.getByLabelText(/sort by name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/sort by email/i)).toBeInTheDocument();
  });

  // ── Select all checkbox ────────────────────────────────────────────────────

  it('select-all checkbox has aria-label', () => {
    renderGrid({ selectable: true, onSelectionChange: jest.fn() });
    expect(screen.getByLabelText(/select all rows/i)).toBeInTheDocument();
  });

  it('individual row checkboxes have descriptive aria-label', () => {
    renderGrid({ selectable: true, onSelectionChange: jest.fn() });
    expect(screen.getByLabelText(/select row 1/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/select row 2/i)).toBeInTheDocument();
  });

  // ── Table pagination accessibility ────────────────────────────────────────

  it('pagination component has aria-label', () => {
    renderGrid({ onPageChange: jest.fn(), onPageSizeChange: jest.fn() });
    // MUI TablePagination applies aria-label to its nav
    const pagination = document.querySelector('[aria-label="Table pagination"]');
    expect(pagination).toBeInTheDocument();
  });
});
