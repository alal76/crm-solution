/**
 * Address Service
 * Provides API calls for address management
 */
import apiClient from './apiClient';
import { Address, CreateAddressDto, UpdateAddressDto } from '../types/address.types';

class AddressService {
  /**
   * Get all addresses for an account
   */
  async getAccountAddresses(accountId: number): Promise<Address[]> {
    try {
      const response = await apiClient.get<Address[]>(`/api/accounts/${accountId}/addresses`);
      return response.data;
    } catch (error: any) {
      console.error(`Error fetching addresses for account ${accountId}:`, error);
      throw error;
    }
  }

  /**
   * Get a specific address by ID
   */
  async getAddressById(accountId: number, addressId: number): Promise<Address> {
    try {
      const response = await apiClient.get<Address>(
        `/api/accounts/${accountId}/addresses/${addressId}`
      );
      return response.data;
    } catch (error: any) {
      console.error(`Error fetching address ${addressId}:`, error);
      throw error;
    }
  }

  /**
   * Create a new address for an account
   */
  async createAddress(accountId: number, address: CreateAddressDto): Promise<Address> {
    try {
      const response = await apiClient.post<Address>(
        `/api/accounts/${accountId}/addresses`,
        address
      );
      return response.data;
    } catch (error: any) {
      console.error('Error creating address:', error);
      throw error;
    }
  }

  /**
   * Update an existing address
   */
  async updateAddress(
    accountId: number,
    addressId: number,
    address: UpdateAddressDto
  ): Promise<Address> {
    try {
      const response = await apiClient.put<Address>(
        `/api/accounts/${accountId}/addresses/${addressId}`,
        address
      );
      return response.data;
    } catch (error: any) {
      console.error(`Error updating address ${addressId}:`, error);
      throw error;
    }
  }

  /**
   * Delete an address (soft delete)
   */
  async deleteAddress(accountId: number, addressId: number): Promise<void> {
    try {
      await apiClient.delete(`/api/accounts/${accountId}/addresses/${addressId}`);
    } catch (error: any) {
      console.error(`Error deleting address ${addressId}:`, error);
      throw error;
    }
  }

  /**
   * Set an address as primary billing
   */
  async setPrimaryBillingAddress(accountId: number, addressId: number): Promise<Address> {
    try {
      const response = await apiClient.patch<Address>(
        `/api/accounts/${accountId}/addresses/${addressId}/set-primary-billing`
      );
      return response.data;
    } catch (error: any) {
      console.error(`Error setting primary billing address:`, error);
      throw error;
    }
  }

  /**
   * Set an address as primary shipping
   */
  async setPrimaryShippingAddress(accountId: number, addressId: number): Promise<Address> {
    try {
      const response = await apiClient.patch<Address>(
        `/api/accounts/${accountId}/addresses/${addressId}/set-primary-shipping`
      );
      return response.data;
    } catch (error: any) {
      console.error(`Error setting primary shipping address:`, error);
      throw error;
    }
  }
}

export default new AddressService();
