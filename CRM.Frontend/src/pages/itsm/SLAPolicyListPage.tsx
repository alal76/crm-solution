import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/api';

interface SLAPolicy {
  slaPolicyId: number;
  name: string;
  targetType: number;
  p1ResponseMinutes?: number;
  p1ResolutionMinutes?: number;
  isActive: boolean;
}

const SLAPolicyListPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<SLAPolicy[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get('/api/sla/policies');
        setItems(response.data ?? []);
      } catch (error) {
        console.error('Failed to load SLA policies', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">SLA Policies</h1>
        <button
          onClick={() => navigate('/itsm/sla/policies/create')}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          + New Policy
        </button>
      </div>

      {loading ? (
        <div>Loading...</div>
      ) : (
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Name</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Target</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Response (P1)</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Resolution (P1)</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Active</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {items.map((policy) => (
                <tr
                  key={policy.slaPolicyId}
                  className="hover:bg-gray-50"
                >
                  <td className="px-6 py-4 text-sm font-medium text-blue-600">{policy.name}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">Type {policy.targetType}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">{policy.p1ResponseMinutes ?? '—'} min</td>
                  <td className="px-6 py-4 text-sm text-gray-900">{policy.p1ResolutionMinutes ?? '—'} min</td>
                  <td className="px-6 py-4 text-sm text-gray-900">{policy.isActive ? 'Yes' : 'No'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default SLAPolicyListPage;
