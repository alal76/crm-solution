import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

interface CatalogRequestItem {
  requestId: number;
  catalogItemId: number;
  state: number;
  createdAt: string;
}

interface CatalogItemLookup {
  catalogItemId: number;
  name: string;
}

const ServiceCatalogRequestListPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<CatalogRequestItem[]>([]);
  const [catalogItems, setCatalogItems] = useState<CatalogItemLookup[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [requestResponse, catalogResponse] = await Promise.all([
          axios.get('/api/catalog/requests'),
          axios.get('/api/catalog/items')
        ]);
        setItems(requestResponse.data ?? []);
        setCatalogItems(catalogResponse.data ?? []);
      } catch (error) {
        console.error('Failed to load catalog requests', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Catalog Requests</h1>
      {loading ? (
        <div>Loading...</div>
      ) : (
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Request</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Catalog Item</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">State</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {items.map((request) => (
                <tr
                  key={request.requestId}
                  onClick={() => navigate(`/itsm/catalog/requests/${request.requestId}`)}
                  className="hover:bg-gray-50 cursor-pointer"
                >
                  <td className="px-6 py-4 text-sm font-medium text-blue-600">REQ-{request.requestId}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">
                    {catalogItems.find((item) => item.catalogItemId === request.catalogItemId)?.name ?? `Item ${request.catalogItemId}`}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-900">State {request.state}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default ServiceCatalogRequestListPage;
