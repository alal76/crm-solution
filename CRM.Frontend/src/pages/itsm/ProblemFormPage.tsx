import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/apiClient';

const ProblemFormPage: React.FC = () => {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    shortDescription: '',
    description: '',
    priority: 2
  });

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);

    try {
      await apiClient.post('/problems', formData);
      navigate('/itsm/problems');
    } catch (error) {
      console.error('Failed to create problem', error);
      setSubmitting(false);
    }
  };

  return (
    <div className="p-6 max-w-3xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Create Problem</h1>
      <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-md p-6 space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Short Description</label>
          <input
            type="text"
            value={formData.shortDescription}
            onChange={(e) => setFormData({ ...formData, shortDescription: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
          <textarea
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            rows={4}
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Priority</label>
          <select
            value={formData.priority}
            onChange={(e) => setFormData({ ...formData, priority: Number(e.target.value) })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
          >
            <option value={1}>P1 - Critical</option>
            <option value={2}>P2 - High</option>
            <option value={3}>P3 - Medium</option>
            <option value={4}>P4 - Low</option>
          </select>
        </div>
        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => navigate('/itsm/problems')}
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

export default ProblemFormPage;
