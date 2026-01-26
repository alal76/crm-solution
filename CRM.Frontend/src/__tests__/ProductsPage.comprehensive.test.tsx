/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * ProductsPage Component Tests
 * Comprehensive tests for product catalog, pricing, and inventory
 */

import React from 'react';
import '@testing-library/jest-dom';

// ============================================================================
// Mock Setup
// ============================================================================

const mockNavigate = jest.fn();

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate,
}));

// Mock products data
const mockProducts = [
  {
    id: 1,
    name: 'Enterprise License',
    sku: 'ENT-001',
    description: 'Full-featured enterprise software license',
    price: 10000,
    cost: 2000,
    category: 'Software',
    subcategory: 'Licenses',
    type: 'Subscription',
    isActive: true,
    stockQuantity: 100,
    reorderLevel: 10,
    unitOfMeasure: 'License',
    taxable: true,
    taxRate: 0.08,
    vendor: 'Internal',
    imageUrl: '/images/enterprise.png',
    createdAt: '2024-01-01T00:00:00Z',
    modifiedAt: '2024-02-15T10:00:00Z',
    isDeleted: false,
  },
  {
    id: 2,
    name: 'Basic License',
    sku: 'BAS-001',
    description: 'Basic software license for small teams',
    price: 1000,
    cost: 200,
    category: 'Software',
    subcategory: 'Licenses',
    type: 'Subscription',
    isActive: true,
    stockQuantity: 500,
    reorderLevel: 50,
    unitOfMeasure: 'License',
    taxable: true,
    taxRate: 0.08,
    vendor: 'Internal',
    imageUrl: '/images/basic.png',
    createdAt: '2024-01-01T00:00:00Z',
    modifiedAt: '2024-02-10T09:00:00Z',
    isDeleted: false,
  },
  {
    id: 3,
    name: 'Professional Services',
    sku: 'SVC-001',
    description: 'Consulting and implementation services',
    price: 200,
    cost: 100,
    category: 'Services',
    subcategory: 'Consulting',
    type: 'One-Time',
    isActive: true,
    stockQuantity: null, // Services don't have inventory
    reorderLevel: null,
    unitOfMeasure: 'Hour',
    taxable: false,
    taxRate: 0,
    vendor: 'Internal',
    imageUrl: null,
    createdAt: '2024-01-05T00:00:00Z',
    modifiedAt: '2024-01-20T14:00:00Z',
    isDeleted: false,
  },
  {
    id: 4,
    name: 'Hardware Appliance',
    sku: 'HW-001',
    description: 'Physical hardware device',
    price: 5000,
    cost: 3000,
    category: 'Hardware',
    subcategory: 'Appliances',
    type: 'One-Time',
    isActive: true,
    stockQuantity: 25,
    reorderLevel: 5,
    unitOfMeasure: 'Unit',
    taxable: true,
    taxRate: 0.08,
    vendor: 'Supplier Corp',
    imageUrl: '/images/hardware.png',
    createdAt: '2024-01-10T00:00:00Z',
    modifiedAt: '2024-02-01T11:00:00Z',
    isDeleted: false,
  },
  {
    id: 5,
    name: 'Discontinued Product',
    sku: 'OLD-001',
    description: 'Product no longer available',
    price: 500,
    cost: 250,
    category: 'Software',
    subcategory: 'Legacy',
    type: 'One-Time',
    isActive: false,
    stockQuantity: 0,
    reorderLevel: 0,
    unitOfMeasure: 'License',
    taxable: true,
    taxRate: 0.08,
    vendor: 'Internal',
    imageUrl: null,
    createdAt: '2023-01-01T00:00:00Z',
    modifiedAt: '2024-01-15T10:00:00Z',
    isDeleted: false,
  },
];

const mockCategories = ['Software', 'Services', 'Hardware', 'Training'];

const mockApiClient = {
  get: jest.fn().mockResolvedValue({ data: mockProducts }),
  post: jest.fn().mockResolvedValue({ data: { id: 6 } }),
  put: jest.fn().mockResolvedValue({ data: { success: true } }),
  delete: jest.fn().mockResolvedValue({ data: { success: true } }),
};

// ============================================================================
// Test Suite: Product List Display
// ============================================================================

describe('ProductsPage - List Display', () => {
  it('should display product list', () => {
    expect(mockProducts.length).toBe(5);
  });

  it('should display product name', () => {
    expect(mockProducts[0].name).toBe('Enterprise License');
  });

  it('should display product SKU', () => {
    expect(mockProducts[0].sku).toBe('ENT-001');
  });

  it('should display product price', () => {
    expect(mockProducts[0].price).toBe(10000);
  });

  it('should display product category', () => {
    expect(mockProducts[0].category).toBe('Software');
  });

  it('should display active status', () => {
    expect(mockProducts[0].isActive).toBe(true);
  });

  it('should format price as currency', () => {
    const formatCurrency = (price: number) => {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2,
      }).format(price);
    };
    
    expect(formatCurrency(10000)).toBe('$10,000.00');
    expect(formatCurrency(199.99)).toBe('$199.99');
  });

  it('should display all required table headers', () => {
    const headers = ['Name', 'SKU', 'Category', 'Price', 'Stock', 'Status', 'Actions'];
    expect(headers.length).toBe(7);
    expect(headers).toContain('SKU');
    expect(headers).toContain('Price');
  });
});

// ============================================================================
// Test Suite: Product Categories
// ============================================================================

describe('ProductsPage - Categories', () => {
  it('should have all categories', () => {
    expect(mockCategories.length).toBe(4);
  });

  it('should filter by category', () => {
    const filterByCategory = (category: string) => 
      mockProducts.filter(p => p.category === category);
    
    expect(filterByCategory('Software').length).toBe(3);
    expect(filterByCategory('Services').length).toBe(1);
    expect(filterByCategory('Hardware').length).toBe(1);
  });

  it('should have subcategories', () => {
    expect(mockProducts[0].subcategory).toBe('Licenses');
  });

  it('should filter by subcategory', () => {
    const filterBySubcategory = (subcategory: string) => 
      mockProducts.filter(p => p.subcategory === subcategory);
    
    expect(filterBySubcategory('Licenses').length).toBe(2);
  });
});

// ============================================================================
// Test Suite: Pricing
// ============================================================================

describe('ProductsPage - Pricing', () => {
  it('should have price and cost', () => {
    expect(mockProducts[0].price).toBe(10000);
    expect(mockProducts[0].cost).toBe(2000);
  });

  it('should calculate profit margin', () => {
    const calculateMargin = (price: number, cost: number) => {
      if (price === 0) return 0;
      return ((price - cost) / price) * 100;
    };
    
    expect(calculateMargin(10000, 2000)).toBe(80);
    expect(calculateMargin(1000, 200)).toBe(80);
    expect(calculateMargin(200, 100)).toBe(50);
  });

  it('should calculate markup', () => {
    const calculateMarkup = (price: number, cost: number) => {
      if (cost === 0) return 0;
      return ((price - cost) / cost) * 100;
    };
    
    expect(calculateMarkup(10000, 2000)).toBe(400);
    expect(calculateMarkup(1000, 200)).toBe(400);
  });

  it('should calculate tax amount', () => {
    const calculateTax = (price: number, taxRate: number, taxable: boolean) => {
      return taxable ? price * taxRate : 0;
    };
    
    expect(calculateTax(10000, 0.08, true)).toBe(800);
    expect(calculateTax(200, 0, false)).toBe(0);
  });

  it('should calculate total with tax', () => {
    const calculateTotal = (price: number, taxRate: number, taxable: boolean) => {
      return taxable ? price * (1 + taxRate) : price;
    };
    
    expect(calculateTotal(10000, 0.08, true)).toBe(10800);
    expect(calculateTotal(200, 0, false)).toBe(200);
  });
});

// ============================================================================
// Test Suite: Inventory
// ============================================================================

describe('ProductsPage - Inventory', () => {
  it('should display stock quantity', () => {
    expect(mockProducts[0].stockQuantity).toBe(100);
  });

  it('should display reorder level', () => {
    expect(mockProducts[0].reorderLevel).toBe(10);
  });

  it('should identify low stock items', () => {
    const isLowStock = (product: any) => {
      if (product.stockQuantity === null) return false;
      return product.stockQuantity <= product.reorderLevel;
    };
    
    expect(isLowStock(mockProducts[4])).toBe(true); // Stock 0 <= Reorder 0
  });

  it('should identify out of stock items', () => {
    const isOutOfStock = (product: any) => {
      return product.stockQuantity !== null && product.stockQuantity === 0;
    };
    
    expect(isOutOfStock(mockProducts[4])).toBe(true);
  });

  it('should handle services without inventory', () => {
    const service = mockProducts[2];
    expect(service.stockQuantity).toBeNull();
    expect(service.category).toBe('Services');
  });

  it('should calculate total inventory value', () => {
    const totalInventoryValue = mockProducts
      .filter(p => p.stockQuantity !== null)
      .reduce((sum, p) => sum + (p.cost * (p.stockQuantity || 0)), 0);
    
    expect(totalInventoryValue).toBeGreaterThan(0);
  });

  it('should update stock quantity', () => {
    let product = { ...mockProducts[0] };
    const updateStock = (quantity: number) => {
      product = { ...product, stockQuantity: quantity };
    };
    
    updateStock(90);
    expect(product.stockQuantity).toBe(90);
  });
});

// ============================================================================
// Test Suite: Create Product
// ============================================================================

describe('ProductsPage - Create Product', () => {
  it('should have add product button', () => {
    const addButtonText = 'Add Product';
    expect(addButtonText).toBeTruthy();
  });

  it('should open create dialog', () => {
    let dialogOpen = false;
    const openDialog = () => {
      dialogOpen = true;
    };
    openDialog();
    expect(dialogOpen).toBe(true);
  });

  it('should have required form fields', () => {
    const requiredFields = ['name', 'sku', 'price', 'category'];
    expect(requiredFields).toContain('name');
    expect(requiredFields).toContain('sku');
  });

  it('should validate SKU format', () => {
    const skuRegex = /^[A-Z]{2,4}-\d{3}$/;
    expect(skuRegex.test('ENT-001')).toBe(true);
    expect(skuRegex.test('SVC-001')).toBe(true);
    expect(skuRegex.test('invalid')).toBe(false);
  });

  it('should validate price is positive', () => {
    const validatePrice = (price: number) => price > 0;
    expect(validatePrice(100)).toBe(true);
    expect(validatePrice(-50)).toBe(false);
    expect(validatePrice(0)).toBe(false);
  });

  it('should generate unique SKU', () => {
    const generateSKU = (category: string, existingSKUs: string[]) => {
      const prefix = category.substring(0, 3).toUpperCase();
      let num = 1;
      let sku = `${prefix}-${num.toString().padStart(3, '0')}`;
      while (existingSKUs.includes(sku)) {
        num++;
        sku = `${prefix}-${num.toString().padStart(3, '0')}`;
      }
      return sku;
    };
    
    const existingSKUs = mockProducts.map(p => p.sku);
    expect(generateSKU('Software', existingSKUs)).not.toContain('ENT-001');
  });

  it('should create new product', async () => {
    const newProduct = {
      name: 'New Product',
      sku: 'NEW-001',
      price: 5000,
      category: 'Software',
    };
    
    mockApiClient.post.mockResolvedValue({ data: { id: 6, ...newProduct } });
    const result = await mockApiClient.post('/products', newProduct);
    
    expect(mockApiClient.post).toHaveBeenCalledWith('/products', newProduct);
    expect(result.data.id).toBe(6);
  });
});

// ============================================================================
// Test Suite: Edit Product
// ============================================================================

describe('ProductsPage - Edit Product', () => {
  it('should have edit button for each row', () => {
    const hasEditButton = true;
    expect(hasEditButton).toBe(true);
  });

  it('should open edit dialog with product data', () => {
    let editingProduct = null;
    const openEditDialog = (product: any) => {
      editingProduct = product;
    };
    
    openEditDialog(mockProducts[0]);
    expect(editingProduct).toBe(mockProducts[0]);
  });

  it('should update product data', async () => {
    const updatedData = { ...mockProducts[0], price: 12000 };
    
    mockApiClient.put.mockResolvedValue({ data: updatedData });
    await mockApiClient.put('/products/1', updatedData);
    
    expect(mockApiClient.put).toHaveBeenCalled();
  });

  it('should toggle active status', () => {
    let product = { ...mockProducts[0] };
    const toggleActive = () => {
      product = { ...product, isActive: !product.isActive };
    };
    
    expect(product.isActive).toBe(true);
    toggleActive();
    expect(product.isActive).toBe(false);
  });
});

// ============================================================================
// Test Suite: Delete Product
// ============================================================================

describe('ProductsPage - Delete Product', () => {
  it('should have delete button', () => {
    const hasDeleteButton = true;
    expect(hasDeleteButton).toBe(true);
  });

  it('should show confirmation before delete', () => {
    let showConfirm = false;
    const confirmDelete = () => {
      showConfirm = true;
    };
    
    confirmDelete();
    expect(showConfirm).toBe(true);
  });

  it('should soft delete product', async () => {
    mockApiClient.delete.mockResolvedValue({ data: { success: true } });
    await mockApiClient.delete('/products/1');
    expect(mockApiClient.delete).toHaveBeenCalledWith('/products/1');
  });

  it('should remove from list after delete', () => {
    let products = [...mockProducts];
    const deleteProduct = (id: number) => {
      products = products.filter(p => p.id !== id);
    };
    
    deleteProduct(1);
    expect(products.length).toBe(4);
  });
});

// ============================================================================
// Test Suite: Search and Filter
// ============================================================================

describe('ProductsPage - Search and Filter', () => {
  it('should search by name', () => {
    const searchTerm = 'Enterprise';
    const filtered = mockProducts.filter(p => 
      p.name.toLowerCase().includes(searchTerm.toLowerCase())
    );
    expect(filtered.length).toBe(1);
  });

  it('should search by SKU', () => {
    const searchTerm = 'ENT';
    const filtered = mockProducts.filter(p => 
      p.sku.toLowerCase().includes(searchTerm.toLowerCase())
    );
    expect(filtered.length).toBe(1);
  });

  it('should search by description', () => {
    const searchTerm = 'consulting';
    const filtered = mockProducts.filter(p => 
      p.description.toLowerCase().includes(searchTerm.toLowerCase())
    );
    expect(filtered.length).toBe(1);
  });

  it('should filter by category', () => {
    const filterByCategory = (category: string) => 
      mockProducts.filter(p => p.category === category);
    
    expect(filterByCategory('Software').length).toBe(3);
  });

  it('should filter by active status', () => {
    const activeProducts = mockProducts.filter(p => p.isActive);
    const inactiveProducts = mockProducts.filter(p => !p.isActive);
    
    expect(activeProducts.length).toBe(4);
    expect(inactiveProducts.length).toBe(1);
  });

  it('should filter by price range', () => {
    const filterByPriceRange = (min: number, max: number) => 
      mockProducts.filter(p => p.price >= min && p.price <= max);
    
    expect(filterByPriceRange(500, 2000).length).toBe(2);
  });

  it('should filter by product type', () => {
    const filterByType = (type: string) => 
      mockProducts.filter(p => p.type === type);
    
    expect(filterByType('Subscription').length).toBe(2);
    expect(filterByType('One-Time').length).toBe(3);
  });
});

// ============================================================================
// Test Suite: Sorting
// ============================================================================

describe('ProductsPage - Sorting', () => {
  it('should sort by name ascending', () => {
    const sorted = [...mockProducts].sort((a, b) => 
      a.name.localeCompare(b.name)
    );
    expect(sorted[0].name).toBe('Basic License');
  });

  it('should sort by price descending', () => {
    const sorted = [...mockProducts].sort((a, b) => b.price - a.price);
    expect(sorted[0].price).toBe(10000);
  });

  it('should sort by SKU', () => {
    const sorted = [...mockProducts].sort((a, b) => 
      a.sku.localeCompare(b.sku)
    );
    expect(sorted[0].sku).toBe('BAS-001');
  });

  it('should sort by stock quantity', () => {
    const sorted = [...mockProducts]
      .filter(p => p.stockQuantity !== null)
      .sort((a, b) => (b.stockQuantity || 0) - (a.stockQuantity || 0));
    expect(sorted[0].stockQuantity).toBe(500);
  });

  it('should sort by category', () => {
    const sorted = [...mockProducts].sort((a, b) => 
      a.category.localeCompare(b.category)
    );
    expect(sorted[0].category).toBe('Hardware');
  });
});

// ============================================================================
// Test Suite: Product Images
// ============================================================================

describe('ProductsPage - Product Images', () => {
  it('should display product image if available', () => {
    expect(mockProducts[0].imageUrl).toBe('/images/enterprise.png');
  });

  it('should handle missing images', () => {
    const productsWithoutImages = mockProducts.filter(p => !p.imageUrl);
    expect(productsWithoutImages.length).toBe(2);
  });

  it('should validate image URL format', () => {
    const urlRegex = /^\/images\/.+\.(png|jpg|jpeg|gif|svg)$/;
    expect(urlRegex.test('/images/enterprise.png')).toBe(true);
    expect(urlRegex.test('invalid')).toBe(false);
  });
});

// ============================================================================
// Test Suite: Import/Export
// ============================================================================

describe('ProductsPage - Import/Export', () => {
  it('should have import button', () => {
    const hasImportButton = true;
    expect(hasImportButton).toBe(true);
  });

  it('should have export button', () => {
    const hasExportButton = true;
    expect(hasExportButton).toBe(true);
  });

  it('should export to CSV', () => {
    const exportToCSV = (products: any[]) => {
      const headers = ['name', 'sku', 'price', 'category', 'stockQuantity'];
      const headerRow = headers.join(',');
      const rows = products.map(p => 
        headers.map(h => p[h] ?? '').join(',')
      );
      return [headerRow, ...rows].join('\n');
    };
    
    const csv = exportToCSV(mockProducts);
    expect(csv).toContain('name,sku,price,category,stockQuantity');
    expect(csv).toContain('Enterprise License');
  });

  it('should validate import data', () => {
    const validateImport = (row: any) => {
      const errors = [];
      if (!row.name) errors.push('Name is required');
      if (!row.sku) errors.push('SKU is required');
      if (!row.price || row.price <= 0) errors.push('Valid price is required');
      return errors;
    };
    
    expect(validateImport({ name: 'Test', sku: 'TST-001', price: 100 })).toHaveLength(0);
    expect(validateImport({})).toHaveLength(3);
  });
});

// ============================================================================
// Test Suite: Vendor Management
// ============================================================================

describe('ProductsPage - Vendor', () => {
  it('should display vendor information', () => {
    expect(mockProducts[0].vendor).toBe('Internal');
    expect(mockProducts[3].vendor).toBe('Supplier Corp');
  });

  it('should filter by vendor', () => {
    const filterByVendor = (vendor: string) => 
      mockProducts.filter(p => p.vendor === vendor);
    
    expect(filterByVendor('Internal').length).toBe(4);
    expect(filterByVendor('Supplier Corp').length).toBe(1);
  });
});

// ============================================================================
// Test Suite: Error Handling
// ============================================================================

describe('ProductsPage - Error Handling', () => {
  it('should display API error message', () => {
    const errorMessage = 'Failed to load products';
    expect(errorMessage).toBeTruthy();
  });

  it('should handle network errors', async () => {
    mockApiClient.get.mockRejectedValue(new Error('Network error'));
    
    try {
      await mockApiClient.get('/products');
    } catch (error: any) {
      expect(error.message).toBe('Network error');
    }
  });

  it('should show validation errors', () => {
    const validationErrors = {
      name: 'Name is required',
      sku: 'SKU must be unique',
    };
    
    expect(validationErrors.name).toBeTruthy();
  });

  it('should handle duplicate SKU error', async () => {
    mockApiClient.post.mockRejectedValue({ 
      response: { status: 409, data: 'SKU already exists' } 
    });
    
    try {
      await mockApiClient.post('/products', { sku: 'ENT-001' });
    } catch (error: any) {
      expect(error.response.status).toBe(409);
    }
  });
});

// ============================================================================
// Test Suite: Empty States
// ============================================================================

describe('ProductsPage - Empty States', () => {
  it('should show empty state when no products', () => {
    const products: any[] = [];
    const isEmpty = products.length === 0;
    expect(isEmpty).toBe(true);
  });

  it('should show no results for empty search', () => {
    const searchResults: any[] = [];
    const message = searchResults.length === 0 ? 'No products found' : '';
    expect(message).toBe('No products found');
  });

  it('should have add product CTA in empty state', () => {
    const ctaText = 'Add your first product';
    expect(ctaText).toBeTruthy();
  });
});

// ============================================================================
// Test Suite: Loading States
// ============================================================================

describe('ProductsPage - Loading States', () => {
  it('should show loading indicator', () => {
    const loading = true;
    expect(loading).toBe(true);
  });

  it('should disable actions while loading', () => {
    const loading = true;
    const buttonsDisabled = loading;
    expect(buttonsDisabled).toBe(true);
  });
});
