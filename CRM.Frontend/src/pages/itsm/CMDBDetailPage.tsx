import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import apiClient from '../../services/api';
import { RelationshipDiagram, ServiceMap } from '../../components/itsm';
import type { ConfigurationItemNode, ServiceNode } from '../../components/itsm';

interface ConfigurationItemDetail {
  ciId: number;
  ciName: string;
  ciNumber: string;
  ciType: number;
  ciSubtype?: string;
  operationalStatus: number;
  description?: string;
}

const CMDBDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [ci, setCi] = useState<ConfigurationItemDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [relatedCIs, setRelatedCIs] = useState<ConfigurationItemNode[]>([]);
  const [ciRelationships, setCiRelationships] = useState<any[]>([]);
  const [serviceNodes, setServiceNodes] = useState<ServiceNode[]>([]);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get(`/api/cmdb/${id}`);
        setCi(response.data);
        // Load relationships and service map (best-effort)
        const [relResp, svcResp] = await Promise.allSettled([
          apiClient.get(`/api/cmdb/${id}/relationships`),
          apiClient.get(`/api/cmdb/${id}/services`),
        ]);
        if (relResp.status === 'fulfilled') {
          setRelatedCIs(relResp.value.data?.relatedCIs ?? []);
          setCiRelationships(relResp.value.data?.relationships ?? []);
        }
        if (svcResp.status === 'fulfilled') setServiceNodes(svcResp.value.data ?? []);
      } catch (error) {
        console.error('Failed to load CI', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  if (loading) return <div className="p-6">Loading...</div>;
  if (!ci) return <div className="p-6">Configuration item not found</div>;

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{ci.ciName}</h1>
          <p className="text-gray-600">{ci.ciNumber}</p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => navigate(`/itsm/cmdb/${ci.ciId}/relationships`)}
            className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
          >
            Relationships
          </button>
          <button
            onClick={() => navigate(`/itsm/cmdb/${ci.ciId}/impact`)}
            className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
          >
            Impact Analysis
          </button>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow-md p-6 space-y-4">
        <div>
          <h2 className="text-sm font-semibold text-gray-700">Description</h2>
          <p className="text-gray-900 whitespace-pre-wrap">{ci.description || '—'}</p>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <h3 className="text-sm font-semibold text-gray-700">Type</h3>
            <p className="text-gray-900">Type {ci.ciType}</p>
          </div>
          <div>
            <h3 className="text-sm font-semibold text-gray-700">Subtype</h3>
            <p className="text-gray-900">{ci.ciSubtype || '—'}</p>
          </div>
          <div>
            <h3 className="text-sm font-semibold text-gray-700">Status</h3>
            <p className="text-gray-900">Status {ci.operationalStatus}</p>
          </div>
        </div>
      </div>

      {/* CI Relationship Diagram */}
      <div className="mt-6">
        <RelationshipDiagram
          centerCI={{
            id: ci.ciId,
            name: ci.ciName,
            ciNumber: ci.ciNumber,
            ciType: 'server',
            status: 'operational',
            criticality: 'medium',
          }}
          relatedCIs={relatedCIs}
          relationships={ciRelationships}
        />
      </div>

      {/* Service Map */}
      {serviceNodes.length > 0 && (
        <div className="mt-6">
          <ServiceMap
            services={serviceNodes}
            selectedServiceId={String(ci.ciId)}
            showLegend
          />
        </div>
      )}
    </div>
  );
};

export default CMDBDetailPage;
