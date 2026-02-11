import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/apiClient';

const KnowledgeArticleEditorPage: React.FC = () => {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    title: '',
    shortDescription: '',
    articleBody: '',
    articleType: 1,
    isInternal: true
  });

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);

    try {
      await apiClient.post('/api/knowledge', formData);
      navigate('/itsm/knowledge');
    } catch (error) {
      console.error('Failed to create article', error);
      setSubmitting(false);
    }
  };

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Knowledge Article Editor</h1>
      <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-md p-6 space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Title</label>
          <input
            type="text"
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Short Description</label>
          <input
            type="text"
            value={formData.shortDescription}
            onChange={(e) => setFormData({ ...formData, shortDescription: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Article Body</label>
          <textarea
            value={formData.articleBody}
            onChange={(e) => setFormData({ ...formData, articleBody: e.target.value })}
            rows={8}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            required
          />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Article Type</label>
            <select
              value={formData.articleType}
              onChange={(e) => setFormData({ ...formData, articleType: Number(e.target.value) })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            >
              <option value={1}>How-To</option>
              <option value={2}>Troubleshooting</option>
              <option value={3}>FAQ</option>
              <option value={4}>Known Error</option>
              <option value={5}>Reference</option>
              <option value={6}>Best Practice</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Visibility</label>
            <select
              value={formData.isInternal ? 'internal' : 'external'}
              onChange={(e) => setFormData({ ...formData, isInternal: e.target.value === 'internal' })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg"
            >
              <option value="internal">Internal</option>
              <option value="external">External</option>
            </select>
          </div>
        </div>
        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => navigate('/itsm/knowledge')}
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

export default KnowledgeArticleEditorPage;
