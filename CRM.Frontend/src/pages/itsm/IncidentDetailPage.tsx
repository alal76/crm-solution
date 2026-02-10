import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import {
  IncidentTimeline,
  SLACountdownWidget,
  SLABreachAlert,
  RelatedIncidentsWidget,
  ArticleSuggestions,
} from '../../components/itsm';
import type {
  TimelineActivity,
  SLAInstanceData,
  SLABreachInfo,
  RelatedIncident,
} from '../../components/itsm';

interface Incident {
  incidentId: number;
  number: string;
  shortDescription: string;
  description: string;
  state: number;
  priority: number;
  callerName: string;
  assignedToName?: string;
  createdAt: string;
}

export const IncidentDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [incident, setIncident] = useState<Incident | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [timelineActivities, setTimelineActivities] = useState<TimelineActivity[]>([]);
  const [slaInstances, setSlaInstances] = useState<SLAInstanceData[]>([]);
  const [slaBreaches, setSlaBreaches] = useState<SLABreachInfo[]>([]);
  const [relatedIncidents, setRelatedIncidents] = useState<RelatedIncident[]>([]);

  useEffect(() => {
    const loadIncident = async () => {
      try {
        const response = await axios.get(`/api/incidents/${id}`);
        setIncident(response.data);
        // Load supplementary data for ITSM components (best-effort)
        const [timelineResp, slaResp, breachResp, relatedResp] = await Promise.allSettled([
          axios.get(`/api/incidents/${id}/timeline`),
          axios.get(`/api/incidents/${id}/sla`),
          axios.get(`/api/incidents/${id}/sla/breaches`),
          axios.get(`/api/incidents/${id}/related`),
        ]);
        if (timelineResp.status === 'fulfilled') setTimelineActivities(timelineResp.value.data ?? []);
        if (slaResp.status === 'fulfilled') setSlaInstances(slaResp.value.data ?? []);
        if (breachResp.status === 'fulfilled') setSlaBreaches(breachResp.value.data ?? []);
        if (relatedResp.status === 'fulfilled') setRelatedIncidents(relatedResp.value.data ?? []);
      } catch (err) {
        setError('Failed to load incident');
      } finally {
        setLoading(false);
      }
    };

    loadIncident();
  }, [id]);

  if (loading) return <div className="p-4">Loading...</div>;
  if (error) return <div className="p-4 text-red-600">{error}</div>;
  if (!incident) return <div className="p-4">Incident not found</div>;

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex justify-between items-start mb-6">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">{incident.number}</h1>
            <p className="text-gray-600 mt-2">{incident.shortDescription}</p>
          </div>
          <button
            onClick={() => navigate('/incidents')}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Back
          </button>
        </div>

        <div className="grid grid-cols-2 gap-4 mb-6 border-t pt-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Caller</label>
            <p className="text-lg text-gray-900">{incident.callerName}</p>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Priority</label>
            <p className="text-lg text-gray-900">Priority {incident.priority}</p>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">State</label>
            <p className="text-lg text-gray-900">State {incident.state}</p>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Assigned To</label>
            <p className="text-lg text-gray-900">{incident.assignedToName || 'Unassigned'}</p>
          </div>
        </div>

        <div className="border-t pt-4">
          <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
          <p className="text-gray-900 whitespace-pre-wrap">{incident.description}</p>
        </div>

        <div className="mt-6 flex gap-4">
          <button className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700">
            Resolve
          </button>
          <button className="px-4 py-2 bg-orange-600 text-white rounded hover:bg-orange-700">
            Escalate
          </button>
          <button className="px-4 py-2 bg-gray-600 text-white rounded hover:bg-gray-700">
            Close
          </button>
        </div>
      </div>

      {/* SLA Breach Alerts */}
      {slaBreaches.length > 0 && (
        <div className="mt-6">
          {slaBreaches.map((breach) => (
            <div key={breach.id} className="mb-2">
              <SLABreachAlert breach={breach} variant="inline" />
            </div>
          ))}
        </div>
      )}

      {/* SLA Countdown */}
      {slaInstances.length > 0 && (
        <div className="mt-6">
          <SLACountdownWidget slaInstances={slaInstances} showDetails />
        </div>
      )}

      {/* Related Incidents */}
      <div className="mt-6">
        <RelatedIncidentsWidget
          problemId={Number(id)}
          incidents={relatedIncidents}
          readOnly
        />
      </div>

      {/* Knowledge Article Suggestions */}
      <div className="mt-6">
        <ArticleSuggestions
          incidentDescription={incident.shortDescription}
          autoSuggest
        />
      </div>

      {/* Incident Timeline */}
      <div className="mt-6">
        <IncidentTimeline
          activities={timelineActivities}
          showFilters
        />
      </div>
    </div>
  );
};

export default IncidentDetailPage;
