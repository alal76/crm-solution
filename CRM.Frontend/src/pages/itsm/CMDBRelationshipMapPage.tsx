import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import apiClient from '../../services/api';

interface ConfigurationItem {
  ciId: number;
  ciName: string;
  ciNumber: string;
  ciType: number;
  ciSubtype?: string;
  operationalStatus: number;
  ownerName?: string;
}

const CMDBRelationshipMapPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [ci, setCi] = useState<ConfigurationItem | null>(null);
  const [related, setRelated] = useState<ConfigurationItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [ciResponse, relatedResponse] = await Promise.all([
          apiClient.get(`/api/cmdb/${id}`),
          apiClient.get(`/api/cmdb/${id}/related`),
        ]);
        setCi(ciResponse.data ?? null);
        setRelated(relatedResponse.data ?? []);
      } catch (error) {
        console.error('Failed to load relationship map', error);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      load();
    } else {
      setLoading(false);
    }
  }, [id]);

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">CI Relationship Map</h1>
      <div className="bg-white rounded-lg shadow-md p-6">
        {loading ? (
          <div>Loading...</div>
        ) : !ci ? (
          <div className="text-gray-600">CI not found.</div>
        ) : (
          <>
            <div className="mb-4">
              <p className="text-sm text-gray-600">{ci.ciNumber}</p>
              <p className="text-lg font-semibold text-gray-900">{ci.ciName}</p>
              <p className="text-sm text-gray-600">
                Type {ci.ciType}{ci.ciSubtype ? ` • ${ci.ciSubtype}` : ''}
              </p>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="border border-gray-200 rounded p-4">
                <h3 className="text-sm font-semibold text-gray-700 mb-3">Related configuration items</h3>
                {related.length === 0 ? (
                  <div className="text-sm text-gray-600">No related items found.</div>
                ) : (
                  <ul className="space-y-3">
                    {related.map((item) => (
                      <li key={item.ciId} className="border border-gray-100 rounded p-3">
                        <p className="text-sm text-gray-600">{item.ciNumber}</p>
                        <p className="text-sm font-semibold text-gray-900">{item.ciName}</p>
                        <p className="text-xs text-gray-500">Type {item.ciType}</p>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
              <div className="border border-dashed border-gray-200 rounded p-4 text-gray-500 flex items-center justify-center">
                Visualization placeholder
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default CMDBRelationshipMapPage;
