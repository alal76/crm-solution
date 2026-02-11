import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/apiClient';

interface ConfigurationItem {
  ciId: number;
  ciName: string;
  ciNumber: string;
  ciType: number;
  operationalStatus: number;
}

const CMDBListPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<ConfigurationItem[]>([]);
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
        const response = await apiClient.get(`/api/cmdb?${params}`);
        setItems(response.data ?? []);
      } catch (error) {
        console.error('Failed to load configuration items', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [searchTerm]);

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">CMDB</h1>
        <button
          onClick={() => navigate('/itsm/cmdb/create')}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          + New CI
        </button>
      </div>

      <div className="mb-6">
        <input
          type="text"
          placeholder="Search configuration items..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg"
        />
      </div>

      {loading ? (
        <div>Loading...</div>
      ) : items.length === 0 ? (
        <div className="text-gray-600">No configuration items found.</div>
      ) : (
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Name</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Number</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Type</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {items.map((ci) => (
                <tr
                  key={ci.ciId}
                  onClick={() => navigate(`/itsm/cmdb/${ci.ciId}`)}
                  className="hover:bg-gray-50 cursor-pointer"
                >
                  <td className="px-6 py-4 text-sm font-medium text-blue-600">{ci.ciName}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">{ci.ciNumber}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">Type {ci.ciType}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">Status {ci.operationalStatus}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default CMDBListPage;
