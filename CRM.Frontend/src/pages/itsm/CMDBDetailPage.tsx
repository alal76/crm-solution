import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import axios from 'axios';

interface ConfigurationItemDetail {
  ciId: number;
  ciName: string;
  ciNumber: string;
  ciType: number;
  ciSubtype?: string;
  operationalStatus: number;
  description?: string;
}

const CMDBDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [ci, setCi] = useState<ConfigurationItemDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await axios.get(`/api/cmdb/${id}`);
        setCi(response.data);
      } catch (error) {
        console.error('Failed to load CI', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  if (loading) return <div className="p-6">Loading...</div>;
  if (!ci) return <div className="p-6">Configuration item not found</div>;

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{ci.ciName}</h1>
          <p className="text-gray-600">{ci.ciNumber}</p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => navigate(`/itsm/cmdb/${ci.ciId}/relationships`)}
            className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
          >
            Relationships
          </button>
          <button
            onClick={() => navigate(`/itsm/cmdb/${ci.ciId}/impact`)}
            className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
          >
            Impact Analysis
          </button>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow-md p-6 space-y-4">
        <div>
          <h2 className="text-sm font-semibold text-gray-700">Description</h2>
          <p className="text-gray-900 whitespace-pre-wrap">{ci.description || '—'}</p>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <h3 className="text-sm font-semibold text-gray-700">Type</h3>
            <p className="text-gray-900">Type {ci.ciType}</p>
          </div>
          <div>
            <h3 className="text-sm font-semibold text-gray-700">Subtype</h3>
            <p className="text-gray-900">{ci.ciSubtype || '—'}</p>
          </div>
          <div>
            <h3 className="text-sm font-semibold text-gray-700">Status</h3>
            <p className="text-gray-900">Status {ci.operationalStatus}</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CMDBDetailPage;
