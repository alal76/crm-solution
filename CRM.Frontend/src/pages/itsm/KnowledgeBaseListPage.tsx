import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../services/apiClient';

interface Article {
  articleId: number;
  number: string;
  title: string;
  shortDescription: string;
  viewCount: number;
  helpfulCount: number;
  publishedDate: string;
}

export const KnowledgeBaseListPage: React.FC = () => {
  const navigate = useNavigate();
  const [articles, setArticles] = useState<Article[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    const loadArticles = async () => {
      setLoading(true);
      try {
        const params = new URLSearchParams({
          searchTerm: searchTerm,
          pageNumber: '1',
          pageSize: '20'
        });
        const response = await apiClient.get(`/api/knowledge/search?${params}`);
        setArticles(response.data ?? []);
      } catch (error) {
        console.error('Failed to load articles', error);
      } finally {
        setLoading(false);
      }
    };

    loadArticles();
  }, [searchTerm]);

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Knowledge Base</h1>

      <div className="mb-8">
        <input
          type="text"
          placeholder="Search knowledge articles..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full max-w-2xl px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
        />
      </div>

      {loading ? (
        <div>Loading...</div>
      ) : articles.length === 0 ? (
        <div className="text-gray-600">No articles found.</div>
      ) : (
        <div className="space-y-4">
          {articles.map((article) => (
            <div
              key={article.articleId}
              onClick={() => navigate(`/knowledge/${article.articleId}`)}
              className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg cursor-pointer transition-shadow border-l-4 border-blue-500"
            >
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <h3 className="text-lg font-bold text-gray-900 mb-2">{article.title}</h3>
                  <p className="text-gray-600 mb-3">{article.shortDescription}</p>
                  <p className="text-sm text-gray-500">{article.number}</p>
                </div>
                <div className="ml-4 text-right">
                  <p className="text-2xl font-bold text-green-600">{article.helpfulCount}</p>
                  <p className="text-xs text-gray-500">helpful</p>
                  <p className="text-sm text-gray-500 mt-2">{article.viewCount} views</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default KnowledgeBaseListPage;
