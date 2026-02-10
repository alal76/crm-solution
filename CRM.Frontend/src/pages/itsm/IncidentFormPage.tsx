import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/apiClient';
import { ImpactUrgencyMatrix } from '../../components/itsm';
import type { ImpactLevel, UrgencyLevel } from '../../components/itsm';

export const IncidentFormPage: React.FC = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    shortDescription: '',
    description: '',
    callerId: 0,
    impact: 1,
    urgency: 1,
    categoryId: 0
  });
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await apiClient.post('/incidents', formData);
      navigate('/incidents');
    } catch (error) {
      console.error('Failed to create incident', error);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="p-6 max-w-2xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Create Incident</h1>
      
      <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-md p-6 space-y-6">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Short Description *</label>
          <input
            type="text"
            required
            value={formData.shortDescription}
            onChange={(e) => setFormData({...formData, shortDescription: e.target.value})}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            placeholder="Briefly describe the issue"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
          <textarea
            value={formData.description}
            onChange={(e) => setFormData({...formData, description: e.target.value})}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 h-32"
            placeholder="Detailed description..."
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Impact *</label>
            <select
              required
              value={formData.impact}
              onChange={(e) => setFormData({...formData, impact: parseInt(e.target.value)})}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value={1}>Low</option>
              <option value={2}>Medium</option>
              <option value={3}>High</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Urgency *</label>
            <select
              required
              value={formData.urgency}
              onChange={(e) => setFormData({...formData, urgency: parseInt(e.target.value)})}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value={1}>Low</option>
              <option value={2}>Medium</option>
              <option value={3}>High</option>
            </select>
          </div>
        </div>

        {/* Impact/Urgency Priority Matrix */}
        <div className="mt-2">
          <ImpactUrgencyMatrix
            impact={formData.impact as ImpactLevel}
            urgency={formData.urgency as UrgencyLevel}
            onChange={(impact, urgency) => setFormData({...formData, impact, urgency})}
            showMatrix
          />
        </div>

        <div className="flex gap-4 pt-4">
          <button
            type="submit"
            disabled={submitting}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
          >
            {submitting ? 'Creating...' : 'Create Incident'}
          </button>
          <button
            type="button"
            onClick={() => navigate('/incidents')}
            className="px-6 py-2 bg-gray-300 text-gray-900 rounded-lg hover:bg-gray-400"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
};

export default IncidentFormPage;
