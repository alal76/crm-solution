import React, { useEffect, useState } from 'react';
import apiClient from '../../services/apiClient';

interface SLAInstance {
  slaInstanceId: number;
  targetId: number;
  targetType: number;
  responseDueAt?: string;
  resolutionDueAt?: string;
  responseBreached: boolean;
  resolutionBreached: boolean;
  state: number;
}

const SLAInstanceListPage: React.FC = () => {
  const [items, setItems] = useState<SLAInstance[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get('/api/sla/breached');
        setItems(response.data ?? []);
      } catch (error) {
        console.error('Failed to load SLA instances', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">SLA Instances</h1>
      {loading ? (
        <div>Loading...</div>
      ) : (
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Target</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Response Due</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Resolution Due</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">State</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {items.map((sla) => (
                <tr key={sla.slaInstanceId} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm text-gray-900">
                    {sla.targetType} / {sla.targetId}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-900">
                    {sla.responseDueAt ? new Date(sla.responseDueAt).toLocaleString() : '—'}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-900">
                    {sla.resolutionDueAt ? new Date(sla.resolutionDueAt).toLocaleString() : '—'}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-900">State {sla.state}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default SLAInstanceListPage;
