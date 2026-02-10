import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/apiClient';

const ChangeFormPage: React.FC = () => {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    shortDescription: '',
    description: '',
    type: 2,
    risk: 2,
    impact: 2,
    plannedStartDate: '',
    plannedEndDate: '',
    implementationPlan: '',
    backoutPlan: ''
  });

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);

    try {
      await apiClient.post('/changes', formData);
      navigate('/itsm/changes');
    } catch (error) {
      console.error('Failed to create change', error);
      setSubmitting(false);
    }
  };

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Create Change</h1>
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
            rows={4}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
          />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
            <select
              value={formData.type}
              onChange={(e) => setFormData({ ...formData, type: Number(e.target.value) })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            >
              <option value={1}>Standard</option>
              <option value={2}>Normal</option>
              <option value={3}>Emergency</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Risk</label>
            <select
              value={formData.risk}
              onChange={(e) => setFormData({ ...formData, risk: Number(e.target.value) })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            >
              <option value={1}>High</option>
              <option value={2}>Medium</option>
              <option value={3}>Low</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Impact</label>
            <select
              value={formData.impact}
              onChange={(e) => setFormData({ ...formData, impact: Number(e.target.value) })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            >
              <option value={1}>High</option>
              <option value={2}>Medium</option>
              <option value={3}>Low</option>
            </select>
          </div>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Planned Start</label>
            <input
              type="datetime-local"
              value={formData.plannedStartDate}
              onChange={(e) => setFormData({ ...formData, plannedStartDate: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Planned End</label>
            <input
              type="datetime-local"
              value={formData.plannedEndDate}
              onChange={(e) => setFormData({ ...formData, plannedEndDate: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Implementation Plan</label>
            <textarea
              value={formData.implementationPlan}
              onChange={(e) => setFormData({ ...formData, implementationPlan: e.target.value })}
              rows={3}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Backout Plan</label>
            <textarea
              value={formData.backoutPlan}
              onChange={(e) => setFormData({ ...formData, backoutPlan: e.target.value })}
              rows={3}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>
        </div>
        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => navigate('/itsm/changes')}
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

export default ChangeFormPage;
