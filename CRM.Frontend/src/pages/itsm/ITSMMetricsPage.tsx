import React, { useEffect, useMemo, useState } from 'react';
import axios from 'axios';

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

interface IncidentSummary {
  incidentId: number;
  number: string;
  shortDescription: string;
  state: number;
  priority: number;
}

interface ProblemSummary {
  problemId: number;
  number: string;
  shortDescription: string;
  state: number;
}

interface ChangeSummary {
  changeId: number;
  number: string;
  shortDescription: string;
  state: number;
  approvalStatus: number;
}

interface KnowledgeSummary {
  articleId: number;
  title: string;
  viewCount: number;
  helpfulCount: number;
}

interface SLAInstance {
  slaInstanceId: number;
  targetId: number;
  targetType: number;
  responseBreached: boolean;
  resolutionBreached: boolean;
}

const ITSMMetricsPage: React.FC = () => {
  const [incidents, setIncidents] = useState<PagedResult<IncidentSummary> | null>(null);
  const [problems, setProblems] = useState<PagedResult<ProblemSummary> | null>(null);
  const [changes, setChanges] = useState<PagedResult<ChangeSummary> | null>(null);
  const [knowledge, setKnowledge] = useState<KnowledgeSummary[]>([]);
  const [breachedSlas, setBreachedSlas] = useState<SLAInstance[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const [incidentResponse, problemResponse, changeResponse, knowledgeResponse, slaResponse] = await Promise.all([
          axios.get<PagedResult<IncidentSummary>>('/api/incidents?pageNumber=1&pageSize=5'),
          axios.get<PagedResult<ProblemSummary>>('/api/problems?pageNumber=1&pageSize=5'),
          axios.get<PagedResult<ChangeSummary>>('/api/changes?pageNumber=1&pageSize=5'),
          axios.get<KnowledgeSummary[]>('/api/knowledge/search?searchTerm='),
          axios.get<SLAInstance[]>('/api/sla/breached'),
        ]);

        setIncidents(incidentResponse.data);
        setProblems(problemResponse.data);
        setChanges(changeResponse.data);
        setKnowledge(knowledgeResponse.data ?? []);
        setBreachedSlas(slaResponse.data ?? []);
      } catch (loadError) {
        console.error('Failed to load metrics', loadError);
        setError('Unable to load ITSM metrics.');
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  const knowledgeSummary = useMemo(() => {
    if (!knowledge.length) return { totalViews: 0, helpful: 0, top: [] as KnowledgeSummary[] };
    const totalViews = knowledge.reduce((sum, item) => sum + (item.viewCount ?? 0), 0);
    const helpful = knowledge.reduce((sum, item) => sum + (item.helpfulCount ?? 0), 0);
    const top = [...knowledge].sort((a, b) => (b.viewCount ?? 0) - (a.viewCount ?? 0)).slice(0, 5);
    return { totalViews, helpful, top };
  }, [knowledge]);

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">ITSM Metrics</h1>
      {loading ? (
        <div>Loading...</div>
      ) : (
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Incident Trends</h2>
          <p className="text-sm text-gray-600 mb-4">Total incidents: {incidents?.totalCount ?? 0}</p>
          <ul className="space-y-2">
            {(incidents?.items ?? []).map((item) => (
              <li key={item.incidentId} className="text-sm text-gray-700">
                {item.number} • {item.shortDescription}
              </li>
            ))}
          </ul>
        </div>
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">SLA Compliance</h2>
          <p className="text-sm text-gray-600 mb-2">Breached SLAs: {breachedSlas.length}</p>
          <p className="text-sm text-gray-600">Active incidents: {incidents?.totalCount ?? 0}</p>
        </div>
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Change Success Rate</h2>
          <p className="text-sm text-gray-600 mb-4">Total changes: {changes?.totalCount ?? 0}</p>
          <ul className="space-y-2">
            {(changes?.items ?? []).map((item) => (
              <li key={item.changeId} className="text-sm text-gray-700">
                {item.number} • {item.shortDescription}
              </li>
            ))}
          </ul>
        </div>
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Knowledge Engagement</h2>
          <p className="text-sm text-gray-600 mb-2">Total views: {knowledgeSummary.totalViews}</p>
          <p className="text-sm text-gray-600 mb-4">Helpful votes: {knowledgeSummary.helpful}</p>
          <ul className="space-y-2">
            {knowledgeSummary.top.map((item) => (
              <li key={item.articleId} className="text-sm text-gray-700">
                {item.title} • {item.viewCount} views
              </li>
            ))}
          </ul>
        </div>
      </div>
      )}
      {error && <div className="text-sm text-red-600 mt-4">{error}</div>}
    </div>
  );
};

export default ITSMMetricsPage;
