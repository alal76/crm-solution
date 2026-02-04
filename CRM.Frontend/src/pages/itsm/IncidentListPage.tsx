import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

interface Incident {
  incidentId: number;
  number: string;
  shortDescription: string;
  state: number;
  priority: number;
  callerName: string;
  createdAt: string;
}

export const IncidentListPage: React.FC = () => {
  const navigate = useNavigate();
  const [incidents, setIncidents] = useState<Incident[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [pageNumber, setPageNumber] = useState(1);

  useEffect(() => {
    const loadIncidents = async () => {
      setLoading(true);
      try {
        const params = new URLSearchParams({
          searchTerm: searchTerm,
          pageNumber: pageNumber.toString(),
          pageSize: '20'
        });
        const response = await axios.get(`/api/incidents?${params}`);
        setIncidents(response.data.items ?? []);
      } catch (error) {
        console.error('Failed to load incidents', error);
      } finally {
        setLoading(false);
      }
    };

    loadIncidents();
  }, [searchTerm, pageNumber]);

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Incidents</h1>
        <button
          onClick={() => navigate('/incidents/create')}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          + New Incident
        </button>
      </div>

      <div className="mb-6">
        <input
          type="text"
          placeholder="Search incidents..."
          value={searchTerm}
          onChange={(e) => { setSearchTerm(e.target.value); setPageNumber(1); }}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
        />
      </div>

      {loading ? (
        <div>Loading...</div>
      ) : incidents.length === 0 ? (
        <div className="text-gray-600">No incidents found.</div>
      ) : (
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Number</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Description</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Caller</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Priority</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">State</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {incidents.map((incident) => (
                <tr
                  key={incident.incidentId}
                  onClick={() => navigate(`/incidents/${incident.incidentId}`)}
                  className="hover:bg-gray-50 cursor-pointer"
                >
                  <td className="px-6 py-4 text-sm font-medium text-blue-600">{incident.number}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">{incident.shortDescription}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">{incident.callerName}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">P{incident.priority}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">State {incident.state}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default IncidentListPage;
