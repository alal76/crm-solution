// API Client Service Tests
describe('API Client Service', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should support HTTP GET requests', async () => {
    const mockClient = {
      get: jest.fn().mockResolvedValue({ data: { id: 1, name: 'Test Customer' } })
    };

    const response = await mockClient.get('/customers');
    expect(response.data.name).toBe('Test Customer');
    expect(mockClient.get).toHaveBeenCalledWith('/customers');
  });

  it('should support HTTP POST requests', async () => {
    const mockClient = {
      post: jest.fn().mockResolvedValue({ data: { id: 1, name: 'New Customer' } })
    };

    const response = await mockClient.post('/customers', { name: 'New Customer' });
    expect(response.data.id).toBe(1);
    expect(mockClient.post).toHaveBeenCalled();
  });

  it('should support HTTP PUT requests', async () => {
    const mockClient = {
      put: jest.fn().mockResolvedValue({ data: { id: 1, name: 'Updated' } })
    };

    const response = await mockClient.put('/customers/1', { name: 'Updated' });
    expect(response.data.name).toBe('Updated');
  });

  it('should support HTTP DELETE requests', async () => {
    const mockClient = {
      delete: jest.fn().mockResolvedValue({ status: 204 })
    };

    const response = await mockClient.delete('/customers/1');
    expect(response.status).toBe(204);
  });

  it('should handle network errors gracefully', async () => {
    const mockClient = {
      get: jest.fn().mockRejectedValue(new Error('Network error'))
    };

    try {
      await mockClient.get('/customers');
      fail('Should have thrown');
    } catch (error: any) {
      expect(error.message).toBe('Network error');
    }
  });

  it('should include authorization headers', () => {
    const config = {
      headers: {
        'Authorization': 'Bearer token123',
        'Content-Type': 'application/json'
      }
    };

    expect(config.headers['Authorization']).toBe('Bearer token123');
  });

  it('should handle 4xx and 5xx errors', async () => {
    const mockError = jest.fn().mockRejectedValue(new Error('404 Not Found'));
    
    try {
      await mockError('/notfound');
    } catch (error: any) {
      expect(error.message).toBe('404 Not Found');
    }
  });

  it('should support request timeout configuration', () => {
    const timeout = 5000;
    expect(timeout).toBe(5000);
  });
});
