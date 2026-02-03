import React, { useEffect, useState } from 'react';
import axios from 'axios';

interface ChangeCalendarItem {
  changeId: number;
  number: string;
  shortDescription: string;
  plannedStartDate?: string;
  plannedEndDate?: string;
}

const ChangeCalendarPage: React.FC = () => {
  const [items, setItems] = useState<ChangeCalendarItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const params = new URLSearchParams({
          pageNumber: '1',
          pageSize: '50'
        });
        const response = await axios.get(`/api/changes?${params}`);
        setItems(response.data.items ?? response.data);
      } catch (error) {
        console.error('Failed to load change calendar', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);
  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Change Calendar</h1>
      <div className="bg-white rounded-lg shadow-md p-6">
        {loading ? (
          <div>Loading...</div>
        ) : (
          <div className="space-y-3">
            {items.length === 0 ? (
              <div className="text-gray-600">No scheduled changes found.</div>
            ) : (
              items.map((change) => (
                <div key={change.changeId} className="border border-gray-100 rounded p-4">
                  <div className="flex justify-between">
                    <div>
                      <p className="text-sm text-gray-600">{change.number}</p>
                      <p className="text-gray-900 font-medium">{change.shortDescription}</p>
                    </div>
                    <div className="text-sm text-gray-600">
                      {change.plannedStartDate ? new Date(change.plannedStartDate).toLocaleString() : '—'}
                      {' '}→{' '}
                      {change.plannedEndDate ? new Date(change.plannedEndDate).toLocaleString() : '—'}
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default ChangeCalendarPage;
