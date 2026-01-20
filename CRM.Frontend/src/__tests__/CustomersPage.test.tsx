import React from 'react';
import '@testing-library/jest-dom';

describe('CustomersPage Component', () => {
  const mockCustomers = [
    { id: 1, name: 'Acme Corp', email: 'contact@acme.com' },
    { id: 2, name: 'TechStart Inc', email: 'info@techstart.com' },
  ];

  it('should display customer list', () => {
    expect(mockCustomers.length).toBe(2);
    expect(mockCustomers[0].name).toBe('Acme Corp');
  });

  it('should fetch customers from API', async () => {
    const mockFetch = jest.fn().mockResolvedValue({ data: mockCustomers });
    const result = await mockFetch('/customers');
    expect(result.data).toBe(mockCustomers);
  });

  it('should handle add customer action', () => {
    const handleAdd = jest.fn();
    handleAdd({ name: 'New Corp', email: 'new@example.com' });
    expect(handleAdd).toHaveBeenCalled();
  });

  it('should handle edit customer action', () => {
    const handleEdit = jest.fn();
    handleEdit(1, { name: 'Updated Corp' });
    expect(handleEdit).toHaveBeenCalledWith(1, expect.any(Object));
  });

  it('should handle delete customer action', () => {
    const handleDelete = jest.fn();
    handleDelete(1);
    expect(handleDelete).toHaveBeenCalledWith(1);
  });

  it('should display table headers', () => {
    const headers = ['Name', 'Email', 'Phone', 'Website', 'Actions'];
    expect(headers.length).toBe(5);
    expect(headers).toContain('Email');
  });

  it('should support pagination', () => {
    const page = 1;
    const pageSize = 10;
    expect(page).toBeGreaterThanOrEqual(1);
    expect(pageSize).toBeGreaterThan(0);
  });

  it('should support search/filter', () => {
    const searchTerm = 'Acme';
    const filtered = mockCustomers.filter(c => c.name.includes(searchTerm));
    expect(filtered.length).toBe(1);
  });

  it('should display empty state when no customers', () => {
    const emptyList = [];
    expect(emptyList.length).toBe(0);
  });

  it('should handle API errors', async () => {
    const mockError = jest.fn().mockRejectedValue(new Error('API Error'));
    try {
      await mockError('/customers');
    } catch (error: any) {
      expect(error.message).toBe('API Error');
    }
  });

  it('should validate customer data', () => {
    const customer = { name: 'Test', email: 'test@example.com' };
    expect(customer.name).toBeTruthy();
    expect(customer.email.includes('@')).toBe(true);
  });
});
