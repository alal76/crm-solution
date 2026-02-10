import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/api';

const SLAPolicyFormPage: React.FC = () => {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    targetType: 1,
    p1ResponseMinutes: 15,
    p1ResolutionMinutes: 240,
    useBusinessHours: true,
    isActive: true
  });

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);

    try {
      await apiClient.post('/api/sla/policies', formData);
      navigate('/itsm/sla/policies');
    } catch (error) {
      console.error('Failed to create SLA policy', error);
      setSubmitting(false);
    }
  };

  return (
    <div className="p-6 max-w-3xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Create SLA Policy</h1>
      <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-md p-6 space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
          <input
            type="text"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Target Type</label>
          <select
            value={formData.targetType}
            onChange={(e) => setFormData({ ...formData, targetType: Number(e.target.value) })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
          >
            <option value={1}>Incident</option>
            <option value={2}>Service Request</option>
            <option value={3}>Change</option>
          </select>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">P1 Response (minutes)</label>
            <input
              type="number"
              value={formData.p1ResponseMinutes}
              onChange={(e) => setFormData({ ...formData, p1ResponseMinutes: Number(e.target.value) })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
              min={1}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">P1 Resolution (minutes)</label>
            <input
              type="number"
              value={formData.p1ResolutionMinutes}
              onChange={(e) => setFormData({ ...formData, p1ResolutionMinutes: Number(e.target.value) })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
              min={1}
            />
          </div>
        </div>
        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={formData.useBusinessHours}
            onChange={(e) => setFormData({ ...formData, useBusinessHours: e.target.checked })}
          />
          <label className="text-sm text-gray-700">Use business hours</label>
        </div>
        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={formData.isActive}
            onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
          />
          <label className="text-sm text-gray-700">Active</label>
        </div>
        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => navigate('/itsm/sla/policies')}
            className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={submitting}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
          >
            {submitting ? 'Saving...' : 'Create'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default SLAPolicyFormPage;
