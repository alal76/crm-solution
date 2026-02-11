import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import apiClient from '../../services/apiClient';
import { ArticleFeedbackWidget } from '../../components/itsm';

interface Article {
  articleId: number;
  number: string;
  title: string;
  articleBody: string;
  authorName: string;
  publishedDate: string;
  viewCount: number;
  helpfulCount: number;
}

export const KnowledgeArticleDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [article, setArticle] = useState<Article | null>(null);
  const [loading, setLoading] = useState(true);
  const [feedbackGiven, setFeedbackGiven] = useState(false);

  useEffect(() => {
    const loadArticle = async () => {
      try {
        const response = await apiClient.get(`/api/knowledge/${id}`);
        setArticle(response.data);
      } catch (error) {
        console.error('Failed to load article', error);
      } finally {
        setLoading(false);
      }
    };

    loadArticle();
  }, [id]);

  const handleFeedback = async (helpful: boolean) => {
    try {
      await apiClient.post(`/api/knowledge/${id}/feedback`, { helpful });
      setFeedbackGiven(true);
    } catch (error) {
      console.error('Failed to submit feedback', error);
    }
  };

  if (loading) return <div className="p-4">Loading...</div>;
  if (!article) return <div className="p-4">Article not found</div>;

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <div className="bg-white rounded-lg shadow-md p-8">
        <h1 className="text-4xl font-bold text-gray-900 mb-2">{article.title}</h1>
        <div className="flex justify-between items-center mb-6 pb-6 border-b">
          <div>
            <p className="text-sm text-gray-600">By {article.authorName}</p>
            <p className="text-sm text-gray-600">{article.number} • Published {new Date(article.publishedDate).toLocaleDateString()}</p>
          </div>
          <div className="text-right">
            <p className="text-sm text-gray-600">{article.viewCount} views</p>
          </div>
        </div>

        <div className="prose max-w-none mb-8">
          <div className="text-gray-900 whitespace-pre-wrap">{article.articleBody}</div>
        </div>

        <div className="bg-gray-50 p-6 rounded-lg">
          <p className="text-sm font-medium text-gray-700 mb-3">Was this article helpful?</p>
          <div className="flex gap-3">
            <button
              disabled={feedbackGiven}
              onClick={() => handleFeedback(true)}
              className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50"
            >
              👍 Yes
            </button>
            <button
              disabled={feedbackGiven}
              onClick={() => handleFeedback(false)}
              className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
            >
              👎 No
            </button>
          </div>
          {feedbackGiven && <p className="text-sm text-gray-600 mt-3">Thank you for your feedback!</p>}
        </div>

        {/* Enhanced Article Feedback Widget */}
        <div className="mt-6">
          <ArticleFeedbackWidget
            articleId={Number(id)}
            showStats
            showRating
            onSubmitFeedback={async (feedback) => {
              await apiClient.post(`/api/knowledge/${id}/feedback`, feedback);
              setFeedbackGiven(true);
            }}
          />
        </div>
      </div>
    </div>
  );
};

export default KnowledgeArticleDetailPage;
