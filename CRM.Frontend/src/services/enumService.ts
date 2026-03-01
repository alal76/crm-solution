/**
 * ENUM-FE-002 + ENUM-FE-018: enumService.ts
 * Provides a typed API for managing configurable enum categories and values.
 * Wraps the existing enumManagementService endpoints and maps to the canonical
 * EnumCategory / EnumValue types from src/types/enums.ts.
 * Also triggers cache invalidation (ENUM-FE-018) after mutations.
 */
import {
  EnumCategory,
  EnumValue,
  CreateEnumValueDto,
  UpdateEnumValueDto,
  EnumValidationResult,
} from '../types/enums';
import {
  LookupCategoryDto,
  LookupItemDto,
  getCategories as svcGetCategories,
  getItems as svcGetItems,
  createItem as svcCreateItem,
  updateItem as svcUpdateItem,
  deleteItem as svcDeleteItem,
  reorderItems as svcReorderItems,
} from './enumManagementService';

// ─── Mappers ─────────────────────────────────────────────────────────────────

function mapCategory(dto: LookupCategoryDto): EnumCategory {
  return {
    id: dto.id,
    name: dto.name,
    displayName: dto.name,
    description: dto.description ?? undefined,
    entityType: dto.entityType ?? undefined,
    propertyName: dto.propertyName ?? undefined,
    isSystemManaged: dto.isSystemManaged,
    allowCustomValues: dto.allowCustomValues,
    valueCount: dto.itemCount,
  };
}

function mapValue(item: LookupItemDto, categoryId: number): EnumValue {
  return {
    id: item.id,
    categoryId,
    key: item.key,
    label: item.value,
    description: item.meta ?? undefined,
    sortOrder: item.sortOrder,
    isActive: item.isActive,
    isDefault: item.isDefault,
    isSystemValue: item.isSystemValue,
    color: item.color ?? undefined,
    icon: item.icon ?? undefined,
  };
}

// ─── Cache invalidation helper (lazy import to avoid circular deps) ───────────

let _cacheInvalidate: ((name?: string) => void) | null = null;
export function registerCacheInvalidator(fn: (name?: string) => void): void {
  _cacheInvalidate = fn;
}
function invalidate(categoryName?: string): void {
  _cacheInvalidate?.(categoryName);
}

// ─── Service ─────────────────────────────────────────────────────────────────

const enumService = {
  /** List all enum categories. */
  async getCategories(): Promise<EnumCategory[]> {
    const data = await svcGetCategories();
    return data.map(mapCategory);
  },

  /** Get a single category by name (searches the full list). */
  async getCategoryByName(name: string): Promise<EnumCategory> {
    const all = await enumService.getCategories();
    const found = all.find(c => c.name.toLowerCase() === name.toLowerCase());
    if (!found) throw new Error(`Enum category '${name}' not found`);
    return found;
  },

  /** Get all active values for a category by its name. */
  async getValuesByCategoryName(name: string): Promise<EnumValue[]> {
    const cat = await enumService.getCategoryByName(name);
    return enumService.getValuesByCategoryId(cat.id);
  },

  /** Get all active values for a category by its numeric ID. */
  async getValuesByCategoryId(categoryId: number): Promise<EnumValue[]> {
    const data = await svcGetItems(categoryId, { includeInactive: false });
    return data.map(item => mapValue(item, categoryId));
  },

  /** Create a new value in the given category. */
  async createValue(categoryId: number, dto: CreateEnumValueDto): Promise<EnumValue> {
    const created = await svcCreateItem(categoryId, {
      key: dto.key,
      value: dto.label,
      meta: dto.description ?? '',
      sortOrder: 0,
      isActive: true,
      isDefault: dto.isDefault ?? false,
      color: dto.color ?? '',
      icon: dto.icon ?? '',
      validationRules: dto.metadata ?? '',
    });
    invalidate();
    return mapValue(created, categoryId);
  },

  /** Update an existing value. */
  async updateValue(valueId: number, dto: UpdateEnumValueDto): Promise<EnumValue> {
    const updated = await svcUpdateItem(valueId, {
      key: dto.key ?? '',
      value: dto.label,
      meta: dto.description ?? '',
      sortOrder: dto.sortOrder,
      isActive: dto.isActive,
      isDefault: dto.isDefault,
      color: dto.color ?? '',
      icon: dto.icon ?? '',
      validationRules: dto.metadata ?? '',
    });
    invalidate();
    return mapValue(updated, updated.lookupCategoryId);
  },

  /** Delete a value by ID. */
  async deleteValue(valueId: number): Promise<void> {
    await svcDeleteItem(valueId);
    invalidate();
  },

  /** Reorder values within a category. */
  async reorderValues(categoryId: number, orderedIds: number[]): Promise<void> {
    await svcReorderItems(categoryId, orderedIds);
    invalidate();
  },

  /** Validate that a value string is acceptable for a given category. */
  async validate(categoryName: string, value: string): Promise<EnumValidationResult> {
    try {
      const cat = await enumService.getCategoryByName(categoryName);
      const values = await enumService.getValuesByCategoryId(cat.id);
      const exists = values.some(v => v.key === value && v.isActive);
      return {
        isValid: exists,
        errorMessage: exists ? undefined : `'${value}' is not a valid option for ${categoryName}`,
      };
    } catch {
      return { isValid: true, warningMessage: 'Could not validate enum value against server' };
    }
  },
};

export default enumService;
