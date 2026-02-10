import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import apiClient from '../../services/api';

interface CatalogRequestDetail {
  requestId: number;
  catalogItemId: number;
  requestedForId: number;
  requestedById: number;
  state: number;
  approvalStatus: number;
  createdAt: string;
}

interface CatalogItemLookup {
  catalogItemId: number;
  name: string;
}

const ServiceCatalogRequestDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [request, setRequest] = useState<CatalogRequestDetail | null>(null);
  const [catalogItems, setCatalogItems] = useState<CatalogItemLookup[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [requestResponse, catalogResponse] = await Promise.all([
          apiClient.get('/api/catalog/requests'),
          apiClient.get('/api/catalog/items')
        ]);
        const items: CatalogRequestDetail[] = requestResponse.data ?? [];
        const found = items.find((item) => item.requestId === Number(id));
        setRequest(found ?? null);
        setCatalogItems(catalogResponse.data ?? []);
      } catch (error) {
        console.error('Failed to load catalog request', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Catalog Request</h1>
      <div className="bg-white rounded-lg shadow-md p-6">
        {loading ? (
          <div>Loading...</div>
        ) : !request ? (
          <div className="text-gray-600">Request not found.</div>
        ) : (
          <div className="space-y-4">
            <p className="text-sm text-gray-600">Request ID: {request.requestId}</p>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <h3 className="text-sm font-semibold text-gray-700">Catalog Item</h3>
                <p className="text-gray-900">
                  {catalogItems.find((item) => item.catalogItemId === request.catalogItemId)?.name ?? `Item ${request.catalogItemId}`}
                </p>
              </div>
              <div>
                <h3 className="text-sm font-semibold text-gray-700">State</h3>
                <p className="text-gray-900">State {request.state}</p>
              </div>
              <div>
                <h3 className="text-sm font-semibold text-gray-700">Approval Status</h3>
                <p className="text-gray-900">Status {request.approvalStatus}</p>
              </div>
              <div>
                <h3 className="text-sm font-semibold text-gray-700">Requested For</h3>
                <p className="text-gray-900">User {request.requestedForId}</p>
              </div>
              <div>
                <h3 className="text-sm font-semibold text-gray-700">Requested By</h3>
                <p className="text-gray-900">User {request.requestedById}</p>
              </div>
              <div>
                <h3 className="text-sm font-semibold text-gray-700">Created At</h3>
                <p className="text-gray-900">{new Date(request.createdAt).toLocaleString()}</p>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ServiceCatalogRequestDetailPage;
