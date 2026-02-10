import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { CatalogCategoryBrowser } from '../../components/itsm';
import type { CatalogCategory } from '../../components/itsm';

interface CatalogItem {
  catalogItemId: number;
  name: string;
  shortDescription: string;
  categoryName: string;
  price?: number;
  isFeatured: boolean;
}

export const ServiceCatalogPage: React.FC = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<CatalogItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [categories, setCategories] = useState<CatalogCategory[]>([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);

  useEffect(() => {
    const loadItems = async () => {
      setLoading(true);
      try {
        const params = searchTerm ? `?searchTerm=${searchTerm}` : '';
        const response = await axios.get(`/api/catalog/search${params}`);
        setItems(response.data ?? []);
      } catch (error) {
        console.error('Failed to load catalog', error);
      } finally {
        setLoading(false);
      }
    };

    loadItems();
  }, [searchTerm]);

  useEffect(() => {
    const loadCategories = async () => {
      try {
        const response = await axios.get('/api/catalog/categories');
        setCategories(response.data ?? []);
      } catch (error) {
        console.error('Failed to load categories', error);
      }
    };
    loadCategories();
  }, []);

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Service Catalog</h1>

      {/* Category Browser */}
      {categories.length > 0 && (
        <div className="mb-6">
          <CatalogCategoryBrowser
            categories={categories}
            selectedCategoryId={selectedCategoryId ?? undefined}
            onCategorySelect={(catId) => setSelectedCategoryId(catId)}
            onItemSelect={(itemId) => navigate(`/catalog/${itemId}`)}
            variant="grid"
            showSearch
          />
        </div>
      )}

      <div className="mb-8">
        <input
          type="text"
          placeholder="Search services..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full max-w-md px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
        />
      </div>

      {loading ? (
        <div>Loading...</div>
      ) : items.length === 0 ? (
        <div className="text-gray-600">No catalog items found.</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {items.map((item) => (
            <div
              key={item.catalogItemId}
              onClick={() => navigate(`/catalog/${item.catalogItemId}`)}
              className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg cursor-pointer transition-shadow"
            >
              <div className="flex items-start justify-between mb-3">
                <h3 className="text-lg font-bold text-gray-900">{item.name}</h3>
                {item.isFeatured && <span className="text-xs bg-yellow-100 text-yellow-800 px-2 py-1 rounded">Featured</span>}
              </div>
              <p className="text-sm text-gray-600 mb-3">{item.shortDescription}</p>
              <p className="text-xs text-gray-500 mb-4">{item.categoryName}</p>
              {item.price && <p className="text-lg font-bold text-green-600">${item.price}</p>}
              <button
                className="w-full mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/catalog/${item.catalogItemId}/request`);
                }}
              >
                Request Service
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ServiceCatalogPage;
