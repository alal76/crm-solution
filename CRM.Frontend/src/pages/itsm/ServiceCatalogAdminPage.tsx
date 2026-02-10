import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/api';

interface CatalogAdminItem {
  catalogItemId: number;
  name: string;
  shortDescription?: string;
  categoryName?: string;
  isFeatured: boolean;
  isActive: boolean;
  price?: number;
  requestCount: number;
}

const ServiceCatalogAdminPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<CatalogAdminItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get('/api/catalog/items');
        setItems(response.data ?? []);
      } catch (loadError) {
        console.error('Failed to load catalog items', loadError);
        setError('Unable to load catalog items.');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  const summary = useMemo(() => {
    const total = items.length;
    const featured = items.filter((item) => item.isFeatured).length;
    const active = items.filter((item) => item.isActive).length;
    const totalRequests = items.reduce((sum, item) => sum + (item.requestCount ?? 0), 0);
    return { total, featured, active, totalRequests };
  }, [items]);

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Catalog Administration</h1>
        <button
          onClick={() => navigate('/itsm/catalog')}
          className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
        >
          Back to Catalog
        </button>
      </div>
      <div className="bg-white rounded-lg shadow-md p-6 space-y-4">
        {loading ? (
          <div>Loading...</div>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div className="border border-gray-200 rounded p-4">
                <p className="text-sm text-gray-600">Total items</p>
                <p className="text-2xl font-semibold text-gray-900">{summary.total}</p>
              </div>
              <div className="border border-gray-200 rounded p-4">
                <p className="text-sm text-gray-600">Active items</p>
                <p className="text-2xl font-semibold text-gray-900">{summary.active}</p>
              </div>
              <div className="border border-gray-200 rounded p-4">
                <p className="text-sm text-gray-600">Featured items</p>
                <p className="text-2xl font-semibold text-gray-900">{summary.featured}</p>
              </div>
              <div className="border border-gray-200 rounded p-4">
                <p className="text-sm text-gray-600">Total requests</p>
                <p className="text-2xl font-semibold text-gray-900">{summary.totalRequests}</p>
              </div>
            </div>

            {items.length === 0 ? (
              <div className="text-gray-600">No catalog items available.</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gray-50 border-b">
                    <tr>
                      <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">Name</th>
                      <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">Category</th>
                      <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">Status</th>
                      <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">Requests</th>
                      <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">Price</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y">
                    {items.map((item) => (
                      <tr key={item.catalogItemId} className="hover:bg-gray-50">
                        <td className="px-4 py-3">
                          <p className="text-sm font-medium text-gray-900">{item.name}</p>
                          {item.shortDescription && (
                            <p className="text-xs text-gray-500">{item.shortDescription}</p>
                          )}
                        </td>
                        <td className="px-4 py-3 text-sm text-gray-700">{item.categoryName ?? 'Uncategorized'}</td>
                        <td className="px-4 py-3 text-sm text-gray-700">
                          {item.isActive ? 'Active' : 'Inactive'}
                          {item.isFeatured ? ' • Featured' : ''}
                        </td>
                        <td className="px-4 py-3 text-sm text-gray-700">{item.requestCount}</td>
                        <td className="px-4 py-3 text-sm text-gray-700">
                          {item.price ? `$${item.price}` : '—'}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </>
        )}
        {error && <div className="text-sm text-red-600">{error}</div>}
      </div>
    </div>
  );
};

export default ServiceCatalogAdminPage;
