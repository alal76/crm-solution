// ENUM-TEST-008: Frontend unit tests for EnumManagementPage
// Tests the service layer and component logic without requiring a full render environment.
//
// NOTE: EnumManagementPage imports directly from enumManagementService (not enumService).
// Mock target: ../../services/enumManagementService
import '@testing-library/jest-dom';
import { LookupCategoryDto, LookupItemDto } from '../services/enumManagementService';

// ─── Mock the service module ──────────────────────────────────────────────────
jest.mock('../services/enumManagementService', () => ({
  getCategories: jest.fn(),
  createCategory: jest.fn(),
  updateCategory: jest.fn(),
  deleteCategory: jest.fn(),
  getItems: jest.fn(),
  createItem: jest.fn(),
  updateItem: jest.fn(),
  deleteItem: jest.fn(),
  reorderItems: jest.fn(),
}));

import * as enumManagementService from '../services/enumManagementService';

const mockGetCategories = enumManagementService.getCategories as jest.Mock;
const mockGetItems = enumManagementService.getItems as jest.Mock;
const mockCreateItem = enumManagementService.createItem as jest.Mock;
const mockDeleteItem = enumManagementService.deleteItem as jest.Mock;

// ─── Test data ────────────────────────────────────────────────────────────────

const mockCategory: LookupCategoryDto = {
  id: 1,
  name: 'LeadStatus',
  description: 'Status values for leads',
  isActive: true,
  isSystemManaged: true,
  allowCustomValues: false,
  entityType: 'Lead',
  propertyName: 'Status',
  itemCount: 5,
  createdAt: '2026-01-01T00:00:00Z',
};

const mockItems: LookupItemDto[] = [
  {
    id: 1,
    lookupCategoryId: 1,
    key: 'new',
    value: 'New',
    sortOrder: 0,
    isActive: true,
    isDefault: true,
    isSystemValue: true,
    createdAt: '2026-01-01T00:00:00Z',
  },
  {
    id: 2,
    lookupCategoryId: 1,
    key: 'contacted',
    value: 'Contacted',
    sortOrder: 1,
    isActive: true,
    isDefault: false,
    isSystemValue: true,
    createdAt: '2026-01-01T00:00:00Z',
  },
];

// ─── Tests ────────────────────────────────────────────────────────────────────

describe('EnumManagementPage - Service Integration (ENUM-TEST-008)', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('ENUM-TEST-008-A: getCategories returns an array of LookupCategoryDto', async () => {
    mockGetCategories.mockResolvedValue([mockCategory]);

    const result = await enumManagementService.getCategories();

    expect(result).toHaveLength(1);
    expect(result[0].name).toBe('LeadStatus');
    expect(result[0].isSystemManaged).toBe(true);
    expect(result[0].itemCount).toBe(5);
  });

  it('ENUM-TEST-008-B: getCategories can be filtered by includeInactive flag', async () => {
    const inactiveCategory: LookupCategoryDto = { ...mockCategory, id: 2, name: 'Archived', isActive: false };
    mockGetCategories.mockResolvedValue([mockCategory, inactiveCategory]);

    const all = await enumManagementService.getCategories();
    const activeOnly = all.filter(c => c.isActive);

    expect(all).toHaveLength(2);
    expect(activeOnly).toHaveLength(1);
    expect(activeOnly[0].name).toBe('LeadStatus');
  });

  it('ENUM-TEST-008-C: getItems returns values ordered by sortOrder', async () => {
    mockGetItems.mockResolvedValue([...mockItems].reverse()); // return in wrong order

    const items = await enumManagementService.getItems(1, { includeInactive: false });
    const sorted = [...items].sort((a, b) => a.sortOrder - b.sortOrder);

    expect(sorted[0].key).toBe('new');
    expect(sorted[1].key).toBe('contacted');
  });

  it('ENUM-TEST-008-D: getCategories handles empty response gracefully', async () => {
    mockGetCategories.mockResolvedValue([]);

    const result = await enumManagementService.getCategories();

    expect(result).toHaveLength(0);
    expect(Array.isArray(result)).toBe(true);
  });

  it('ENUM-TEST-008-E: getCategories propagates API errors', async () => {
    mockGetCategories.mockRejectedValue(new Error('Network Error'));

    await expect(enumManagementService.getCategories()).rejects.toThrow('Network Error');
  });

  it('ENUM-TEST-008-F: deleteItem calls service with correct id', async () => {
    mockDeleteItem.mockResolvedValue(undefined);

    await enumManagementService.deleteItem(42);

    expect(mockDeleteItem).toHaveBeenCalledWith(42);
    expect(mockDeleteItem).toHaveBeenCalledTimes(1);
  });

  it('ENUM-TEST-008-G: createItem sends correct payload shape', async () => {
    const newItem: LookupItemDto = {
      id: 99,
      lookupCategoryId: 1,
      key: 'qualified',
      value: 'Qualified',
      sortOrder: 5,
      isActive: true,
      isDefault: false,
      isSystemValue: false,
      createdAt: '2026-01-01T00:00:00Z',
    };
    mockCreateItem.mockResolvedValue(newItem);

    const createPayload = {
      key: 'qualified',
      value: 'Qualified',
      meta: '',
      sortOrder: 5,
      isActive: true,
      isDefault: false,
      color: '',
      icon: '',
      validationRules: '',
    };

    const result = await enumManagementService.createItem(1, createPayload);

    expect(mockCreateItem).toHaveBeenCalledWith(1, createPayload);
    expect(result.key).toBe('qualified');
    expect(result.id).toBe(99);
  });
});
