import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

interface Problem {
  problemId: number;
  number: string;
  shortDescription: string;
  state: number;
  priority: number;
  createdAt: string;
}

const ProblemListPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<Problem[]>([]);
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
        const response = await axios.get(`/api/problems?${params}`);
        setItems(response.data.items ?? response.data ?? []);
      } catch (error) {
        console.error('Failed to load problems', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [searchTerm]);

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Problems</h1>
        <button
          onClick={() => navigate('/itsm/problems/create')}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          + New Problem
        </button>
      </div>

      <div className="mb-6">
        <input
          type="text"
          placeholder="Search problems..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
        />
      </div>

      {loading ? (
        <div>Loading...</div>
      ) : items.length === 0 ? (
        <div className="text-gray-600">No problems found.</div>
      ) : (
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Number</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Description</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">Priority</th>
                <th className="px-6 py-3 text-left text-sm font-medium text-gray-900">State</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {items.map((problem) => (
                <tr
                  key={problem.problemId}
                  onClick={() => navigate(`/itsm/problems/${problem.problemId}`)}
                  className="hover:bg-gray-50 cursor-pointer"
                >
                  <td className="px-6 py-4 text-sm font-medium text-blue-600">{problem.number}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">{problem.shortDescription}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">P{problem.priority}</td>
                  <td className="px-6 py-4 text-sm text-gray-900">State {problem.state}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default ProblemListPage;
