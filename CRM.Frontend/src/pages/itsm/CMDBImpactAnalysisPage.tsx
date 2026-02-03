import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';

const CMDBImpactAnalysisPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [impacts, setImpacts] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await axios.get(`/api/cmdb/${id}/impact-analysis`);
        setImpacts(response.data ?? []);
      } catch (error) {
        console.error('Failed to load impact analysis', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Impact Analysis</h1>
      <div className="bg-white rounded-lg shadow-md p-6">
        {loading ? (
          <div>Loading...</div>
        ) : impacts.length === 0 ? (
          <div className="text-gray-600">No impacts found.</div>
        ) : (
          <ul className="list-disc pl-6 space-y-2">
            {impacts.map((impact, index) => (
              <li key={index} className="text-gray-700">{impact}</li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
};

export default CMDBImpactAnalysisPage;
