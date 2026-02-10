import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/apiClient';

interface ChangeItem {
  changeId: number;
  number: string;
  shortDescription: string;
  state: number;
  approvalStatus: number;
  plannedStartDate?: string;
}

const ChangeListPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<ChangeItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const params = new URLSearchParams({
          searchTerm,
          pageNumber: '1',
          pageSize: '20'
        });
        const response = await apiClient.get(`/changes?${params}`);
        setItems(response.data.items ?? response.data ?? []);
      } catch (error) {
        console.error('Failed to load changes', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [searchTerm]);

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Changes</h1>
        <div className="flex gap-2">
          <button
            onClick={() => navigate('/itsm/changes/calendar')}
            className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
          >
            Calendar
          </button>
          <button
            onClick={() => navigate('/itsm/changes/create')}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            + New Change
          </button>
        </div>
      </div>

      <div className="mb-6">
        <input
          type="text"
          placeholder="Search changes..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg"
        />
      </div>

      {loading ? (
        <div>Loading...</div>
      ) : items.length === 0 ? (
        <div className="text-gray-600">No changes found.</div>
      ) : (
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Number</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Description</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">State</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Approval</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {items.map((change) => (
                <tr
                  key={change.changeId}
                  onClick={() => navigate(`/itsm/changes/${change.changeId}`)}
                  className="hover:bg-gray-50 cursor-pointer"
                >
                  <td className="px-6 py-4 text-sm font-medium text-blue-600">{change.number}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">{change.shortDescription}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">State {change.state}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">Status {change.approvalStatus}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default ChangeListPage;
