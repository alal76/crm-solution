import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import axios from 'axios';

interface ProblemDetail {
  problemId: number;
  number: string;
  shortDescription: string;
  description?: string;
  state: number;
  priority: number;
  rootCause?: string;
  workaround?: string;
  knownError?: boolean;
}

const ProblemDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [problem, setProblem] = useState<ProblemDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await axios.get(`/api/problems/${id}`);
        setProblem(response.data);
      } catch (error) {
        console.error('Failed to load problem', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  if (loading) return <div className="p-6">Loading...</div>;
  if (!problem) return <div className="p-6">Problem not found</div>;

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{problem.number}</h1>
          <p className="text-gray-600">{problem.shortDescription}</p>
        </div>
        <button
          onClick={() => navigate(`/itsm/problems/${problem.problemId}/edit`)}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          Edit
        </button>
      </div>

      <div className="bg-white rounded-lg shadow-md p-6 space-y-4">
        <div>
          <h2 className="text-sm font-semibold text-gray-700">Description</h2>
          <p className="text-gray-900 whitespace-pre-wrap">{problem.description || '—'}</p>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <h3 className="text-sm font-semibold text-gray-700">Priority</h3>
            <p className="text-gray-900">P{problem.priority}</p>
          </div>
          <div>
            <h3 className="text-sm font-semibold text-gray-700">State</h3>
            <p className="text-gray-900">State {problem.state}</p>
          </div>
        </div>
        <div>
          <h3 className="text-sm font-semibold text-gray-700">Root Cause</h3>
          <p className="text-gray-900 whitespace-pre-wrap">{problem.rootCause || '—'}</p>
        </div>
        <div>
          <h3 className="text-sm font-semibold text-gray-700">Workaround</h3>
          <p className="text-gray-900 whitespace-pre-wrap">{problem.workaround || '—'}</p>
        </div>
        <div>
          <h3 className="text-sm font-semibold text-gray-700">Known Error</h3>
          <p className="text-gray-900">{problem.knownError ? 'Yes' : 'No'}</p>
        </div>
      </div>
    </div>
  );
};

export default ProblemDetailPage;
