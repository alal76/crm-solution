import React, { useEffect, useState } from 'react';
import apiClient from '../../services/api';

interface KnowledgeApprovalItem {
  articleId: number;
  number: string;
  title: string;
  shortDescription?: string;
  publishingState: number;
  authorName?: string;
  publishedDate?: string;
}

const KnowledgeArticleApprovalPage: React.FC = () => {
  const [items, setItems] = useState<KnowledgeApprovalItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [publishingId, setPublishingId] = useState<number | null>(null);
  const [rejectingId, setRejectingId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get('/api/knowledge/pending');
        setItems(response.data ?? []);
      } catch (loadError) {
        console.error('Failed to load pending articles', loadError);
        setError('Unable to load approval queue.');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  const handlePublish = async (articleId: number) => {
    setPublishingId(articleId);
    setError(null);

    try {
      await apiClient.patch(`/api/knowledge/${articleId}/publish`);
      setItems((prev) => prev.filter((item) => item.articleId !== articleId));
    } catch (publishError) {
      console.error('Failed to publish article', publishError);
      setError('Unable to publish article.');
    } finally {
      setPublishingId(null);
    }
  };

  const handleReject = async (articleId: number) => {
    setRejectingId(articleId);
    setError(null);

    try {
      await apiClient.patch(`/api/knowledge/${articleId}/retire`);
      setItems((prev) => prev.filter((item) => item.articleId !== articleId));
    } catch (rejectError) {
      console.error('Failed to reject article', rejectError);
      setError('Unable to reject article.');
    } finally {
      setRejectingId(null);
    }
  };

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Knowledge Article Approvals</h1>
      <div className="bg-white rounded-lg shadow-md p-6">
        {loading ? (
          <div>Loading...</div>
        ) : items.length === 0 ? (
          <div className="text-gray-600">No articles awaiting approval.</div>
        ) : (
          <div className="space-y-4">
            {items.map((item) => (
              <div key={item.articleId} className="border border-gray-200 rounded-lg p-4">
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <p className="text-sm text-gray-600">{item.number}</p>
                    <p className="text-lg font-semibold text-gray-900">{item.title}</p>
                    {item.shortDescription && (
                      <p className="text-sm text-gray-600 mt-1">{item.shortDescription}</p>
                    )}
                    <p className="text-xs text-gray-500 mt-2">Draft state {item.publishingState}</p>
                  </div>
                  <div className="flex gap-2">
                    <button
                      type="button"
                      onClick={() => handleReject(item.articleId)}
                      disabled={rejectingId === item.articleId || publishingId === item.articleId}
                      className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
                    >
                      {rejectingId === item.articleId ? 'Rejecting...' : 'Reject'}
                    </button>
                    <button
                      type="button"
                      onClick={() => handlePublish(item.articleId)}
                      disabled={publishingId === item.articleId || rejectingId === item.articleId}
                      className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
                    >
                      {publishingId === item.articleId ? 'Publishing...' : 'Publish'}
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
        {error && <div className="text-sm text-red-600 mt-4">{error}</div>}
      </div>
    </div>
  );
};

export default KnowledgeArticleApprovalPage;
