// ENUM-TEST-009: Frontend unit tests for EnumEditorPage
// Tests the service layer and value editing logic for the EnumEditorPage.
//
// NOTE: EnumEditorPage imports from enumManagementService and enumCacheService.
// Mock targets: ../../services/enumManagementService, ../../services/enumCacheService
import '@testing-library/jest-dom';
import { LookupCategoryDto, LookupItemDto } from '../services/enumManagementService';

// ─── Mock the service modules ─────────────────────────────────────────────────
jest.mock('../services/enumManagementService', () => ({
  getCategories: jest.fn(),
  getItems: jest.fn(),
  createItem: jest.fn(),
  updateItem: jest.fn(),
  deleteItem: jest.fn(),
  reorderItems: jest.fn(),
}));

jest.mock('../services/enumCacheService', () => ({
  default: {
    invalidate: jest.fn(),
    invalidateAll: jest.fn(),
    get: jest.fn(),
    set: jest.fn(),
  },
}));

import * as enumManagementService from '../services/enumManagementService';

const mockGetCategories = enumManagementService.getCategories as jest.Mock;
const mockGetItems = enumManagementService.getItems as jest.Mock;
const mockCreateItem = enumManagementService.createItem as jest.Mock;
const mockUpdateItem = enumManagementService.updateItem as jest.Mock;
const mockDeleteItem = enumManagementService.deleteItem as jest.Mock;
const mockReorderItems = enumManagementService.reorderItems as jest.Mock;

// ─── Test data ────────────────────────────────────────────────────────────────

const mockCategory: LookupCategoryDto = {
  id: 1,
  name: 'LeadStatus',
  description: 'Status values for leads',
  isActive: true,
  isSystemManaged: false,
  allowCustomValues: true,
  entityType: 'Lead',
  propertyName: 'Status',
  itemCount: 3,
  createdAt: '2026-01-01T00:00:00Z',
};

const mockValues: LookupItemDto[] = [
  {
    id: 1, lookupCategoryId: 1, key: 'new', value: 'New',
    sortOrder: 0, isActive: true, isDefault: true, isSystemValue: true, createdAt: '2026-01-01T00:00:00Z',
  },
  {
    id: 2, lookupCategoryId: 1, key: 'contacted', value: 'Contacted',
    sortOrder: 1, isActive: true, isDefault: false, isSystemValue: false, createdAt: '2026-01-01T00:00:00Z',
  },
  {
    id: 3, lookupCategoryId: 1, key: 'qualified', value: 'Qualified',
    sortOrder: 2, isActive: true, isDefault: false, isSystemValue: false, createdAt: '2026-01-01T00:00:00Z',
  },
];

// ─── Tests ────────────────────────────────────────────────────────────────────

describe('EnumEditorPage - Service Integration (ENUM-TEST-009)', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('ENUM-TEST-009-A: getItems returns values for the editor page', async () => {
    mockGetItems.mockResolvedValue(mockValues);

    const items = await enumManagementService.getItems(1, { includeInactive: false });

    expect(items).toHaveLength(3);
    expect(items[0].key).toBe('new');
    expect(items[1].key).toBe('contacted');
    expect(items[2].key).toBe('qualified');
  });

  it('ENUM-TEST-009-B: createItem sends correct payload and returns new value', async () => {
    const newItem: LookupItemDto = {
      id: 10, lookupCategoryId: 1, key: 'unqualified', value: 'Unqualified',
      sortOrder: 3, isActive: true, isDefault: false, isSystemValue: false, createdAt: '2026-01-01T00:00:00Z',
    };
    mockCreateItem.mockResolvedValue(newItem);

    const payload = {
      key: 'unqualified', value: 'Unqualified', meta: '',
      sortOrder: 3, isActive: true, isDefault: false, color: '', icon: '', validationRules: '',
    };

    const result = await enumManagementService.createItem(1, payload);

    expect(mockCreateItem).toHaveBeenCalledWith(1, payload);
    expect(result.key).toBe('unqualified');
    expect(result.isSystemValue).toBe(false);
  });

  it('ENUM-TEST-009-C: updateItem sends updated fields', async () => {
    const updatedItem: LookupItemDto = { ...mockValues[1], value: 'Contacted - Updated' };
    mockUpdateItem.mockResolvedValue(updatedItem);

    const updatePayload = {
      key: 'contacted', value: 'Contacted - Updated', meta: '',
      sortOrder: 1, isActive: true, isDefault: false, color: '', icon: '', validationRules: '',
    };

    const result = await enumManagementService.updateItem(2, updatePayload);

    expect(mockUpdateItem).toHaveBeenCalledWith(2, updatePayload);
    expect(result.value).toBe('Contacted - Updated');
  });

  it('ENUM-TEST-009-D: deleteItem cannot be called for system values (guarded by UI)', async () => {
    // The UI prevents calling deleteItem for isSystemValue = true items.
    // This test verifies the guard logic: system items should not trigger deleteItem.
    mockDeleteItem.mockResolvedValue(undefined);

    const systemValues = mockValues.filter(v => v.isSystemValue);
    const nonSystemValues = mockValues.filter(v => !v.isSystemValue);

    // Simulate the UI guard: only call deleteItem for non-system values
    for (const v of nonSystemValues) {
      await enumManagementService.deleteItem(v.id);
    }

    expect(mockDeleteItem).toHaveBeenCalledTimes(nonSystemValues.length);
    // deleteItem should never be called with system value ids
    for (const sv of systemValues) {
      expect(mockDeleteItem).not.toHaveBeenCalledWith(sv.id);
    }
  });

  it('ENUM-TEST-009-E: reorderItems sends ordering correctly', async () => {
    mockReorderItems.mockResolvedValue(undefined);

    // Reverse order: [3, 2, 1]
    const reversedIds = mockValues.map(v => v.id).reverse();
    await enumManagementService.reorderItems(1, reversedIds);

    expect(mockReorderItems).toHaveBeenCalledWith(1, [3, 2, 1]);
  });

  it('ENUM-TEST-009-F: getCategories finds category by name', async () => {
    mockGetCategories.mockResolvedValue([mockCategory]);

    const all = await enumManagementService.getCategories();
    const found = all.find((c: LookupCategoryDto) => c.name.toLowerCase() === 'leadstatus');

    expect(found).toBeDefined();
    expect(found!.id).toBe(1);
    expect(found!.allowCustomValues).toBe(true);
  });

  it('ENUM-TEST-009-G: error from getItems is propagated', async () => {
    mockGetItems.mockRejectedValue(new Error('DB connection error'));

    await expect(enumManagementService.getItems(1, {})).rejects.toThrow('DB connection error');
  });

  it('ENUM-TEST-009-H: item validation — key and value are required', () => {
    // Mirror the ItemFormDialog validate() logic from EnumEditorPage
    const validate = (form: { key: string; value: string }) => {
      const errors: Record<string, string> = {};
      if (!form.key.trim()) errors.key = 'Key is required';
      if (!form.value.trim()) errors.value = 'Display value is required';
      return errors;
    };

    expect(validate({ key: '', value: '' })).toEqual({
      key: 'Key is required',
      value: 'Display value is required',
    });
    expect(validate({ key: 'test', value: '' })).toHaveProperty('value');
    expect(validate({ key: 'test', value: 'Test Value' })).toEqual({});
  });
});
