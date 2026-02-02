/**
 * Centralized storage service that provides type-safe, encrypted access to browser storage.
 * Abstracts localStorage/sessionStorage for easier testing and security.
 */

import logger from './logger';

type StorageType = 'local' | 'session';

interface StorageOptions {
  encrypt?: boolean;
  expiry?: number; // Time in milliseconds
  storage?: StorageType;
}

interface StoredItem<T> {
  value: T;
  expiry?: number;
  encrypted?: boolean;
}

class StorageService {
  private readonly encoder = new TextEncoder();
  private readonly decoder = new TextDecoder();

  /**
   * Set a value in storage
   */
  set<T>(key: string, value: T, options: StorageOptions = {}): boolean {
    const { encrypt = false, expiry, storage = 'local' } = options;

    try {
      const item: StoredItem<T> = {
        value,
        expiry: expiry ? Date.now() + expiry : undefined,
        encrypted: encrypt,
      };

      let stringValue = JSON.stringify(item);
      
      if (encrypt) {
        stringValue = this.encrypt(stringValue);
      }

      this.getStorage(storage).setItem(this.prefixKey(key), stringValue);
      return true;
    } catch (error) {
      if (error instanceof Error && error.name === 'QuotaExceededError') {
        logger.warn('Storage quota exceeded, attempting cleanup', { key });
        this.cleanup(storage);
        // Retry once after cleanup
        try {
          const item: StoredItem<T> = { value, expiry: expiry ? Date.now() + expiry : undefined };
          this.getStorage(storage).setItem(this.prefixKey(key), JSON.stringify(item));
          return true;
        } catch {
          logger.error('Storage quota exceeded even after cleanup', error);
          return false;
        }
      }
      logger.error('Storage set error', error);
      return false;
    }
  }

  /**
   * Get a value from storage
   */
  get<T>(key: string, options: StorageOptions = {}): T | null {
    const { storage = 'local' } = options;

    try {
      let stringValue = this.getStorage(storage).getItem(this.prefixKey(key));
      
      if (!stringValue) {
        return null;
      }

      // Try to detect if it's encrypted (base64 encoded)
      if (this.isEncrypted(stringValue)) {
        stringValue = this.decrypt(stringValue);
      }

      const item: StoredItem<T> = JSON.parse(stringValue);

      // Check expiry
      if (item.expiry && Date.now() > item.expiry) {
        this.remove(key, options);
        return null;
      }

      return item.value;
    } catch (error) {
      logger.error('Storage get error', error);
      return null;
    }
  }

  /**
   * Remove a value from storage
   */
  remove(key: string, options: StorageOptions = {}): void {
    const { storage = 'local' } = options;
    this.getStorage(storage).removeItem(this.prefixKey(key));
  }

  /**
   * Clear all app-related storage
   */
  clear(storage: StorageType = 'local'): void {
    const s = this.getStorage(storage);
    const keysToRemove: string[] = [];
    
    for (let i = 0; i < s.length; i++) {
      const key = s.key(i);
      if (key?.startsWith('crm_')) {
        keysToRemove.push(key);
      }
    }
    
    keysToRemove.forEach(key => s.removeItem(key));
  }

  /**
   * Check if a key exists in storage
   */
  has(key: string, options: StorageOptions = {}): boolean {
    return this.get(key, options) !== null;
  }

  /**
   * Get all keys in storage
   */
  keys(storage: StorageType = 'local'): string[] {
    const s = this.getStorage(storage);
    const keys: string[] = [];
    
    for (let i = 0; i < s.length; i++) {
      const key = s.key(i);
      if (key?.startsWith('crm_')) {
        keys.push(key.replace('crm_', ''));
      }
    }
    
    return keys;
  }

  // Convenience methods for common use cases

  /**
   * Get/set authentication token (encrypted)
   */
  getToken(): string | null {
    return this.get<string>('auth_token', { encrypt: true });
  }

  setToken(token: string): boolean {
    return this.set('auth_token', token, { encrypt: true });
  }

  removeToken(): void {
    this.remove('auth_token');
  }

  /**
   * Get/set user preferences
   */
  getPreference<T>(key: string, defaultValue: T): T {
    return this.get<T>(`pref_${key}`) ?? defaultValue;
  }

  setPreference<T>(key: string, value: T): boolean {
    return this.set(`pref_${key}`, value);
  }

  /**
   * Get/set session data (uses sessionStorage)
   */
  getSessionData<T>(key: string): T | null {
    return this.get<T>(key, { storage: 'session' });
  }

  setSessionData<T>(key: string, value: T): boolean {
    return this.set(key, value, { storage: 'session' });
  }

  // Private helper methods

  private getStorage(type: StorageType): Storage {
    return type === 'session' ? sessionStorage : localStorage;
  }

  private prefixKey(key: string): string {
    return `crm_${key}`;
  }

  private encrypt(value: string): string {
    // Simple base64 encoding - in production, use a proper encryption library
    // For sensitive data, consider using Web Crypto API
    try {
      return btoa(unescape(encodeURIComponent(value)));
    } catch {
      return value;
    }
  }

  private decrypt(value: string): string {
    try {
      return decodeURIComponent(escape(atob(value)));
    } catch {
      return value;
    }
  }

  private isEncrypted(value: string): boolean {
    // Check if it looks like base64 and doesn't start with '{'
    return !value.startsWith('{') && /^[A-Za-z0-9+/=]+$/.test(value);
  }

  private cleanup(storage: StorageType): void {
    const s = this.getStorage(storage);
    const keysToRemove: string[] = [];
    
    // Find expired items
    for (let i = 0; i < s.length; i++) {
      const key = s.key(i);
      if (key?.startsWith('crm_')) {
        try {
          const value = s.getItem(key);
          if (value) {
            const item = JSON.parse(value) as StoredItem<unknown>;
            if (item.expiry && Date.now() > item.expiry) {
              keysToRemove.push(key);
            }
          }
        } catch {
          // If we can't parse it, it might be old format - mark for removal
          keysToRemove.push(key);
        }
      }
    }
    
    keysToRemove.forEach(key => s.removeItem(key));
    logger.debug(`Storage cleanup: removed ${keysToRemove.length} expired items`);
  }
}

export const storage = new StorageService();
export default storage;
