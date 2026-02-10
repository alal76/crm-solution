import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { SLACountdownWidget, SLABreachBanner } from '../../components/itsm';
import type { SLAInstanceData, SLABreachInfo } from '../../components/itsm';

interface SLAPolicySummary {
  slaPolicyId: number;
}

interface SLAInstanceSummary {
  slaInstanceId: number;
  targetId?: number;
  targetType?: number;
  responseBreached: boolean;
  resolutionBreached: boolean;
  responseDueAt?: string;
  resolutionDueAt?: string;
}

const SLADashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const [policyCount, setPolicyCount] = useState(0);
  const [breachedCount, setBreachedCount] = useState(0);
  const [breaches, setBreaches] = useState<SLAInstanceSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [slaInstances, setSlaInstances] = useState<SLAInstanceData[]>([]);
  const [slaBreachInfos, setSlaBreachInfos] = useState<SLABreachInfo[]>([]);

  useEffect(() => {
    const load = async () => {
      try {
        const [policiesResp, breachedResp, instancesResp, breachInfoResp] = await Promise.allSettled([
          axios.get('/api/sla/policies'),
          axios.get('/api/sla/breached'),
          axios.get('/api/sla/instances/active'),
          axios.get('/api/sla/breach-alerts'),
        ]);
        if (policiesResp.status === 'fulfilled') {
          const policies: SLAPolicySummary[] = policiesResp.value.data ?? [];
          setPolicyCount(policies.length);
        }
        if (breachedResp.status === 'fulfilled') {
          const breachData: SLAInstanceSummary[] = breachedResp.value.data ?? [];
          setBreachedCount(breachData.length);
          setBreaches(breachData);
        }
        if (instancesResp.status === 'fulfilled') setSlaInstances(instancesResp.value.data ?? []);
        if (breachInfoResp.status === 'fulfilled') setSlaBreachInfos(breachInfoResp.value.data ?? []);
      } catch (error) {
        console.error('Failed to load SLA dashboard', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, []);

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">SLA Dashboard</h1>
        <div className="flex gap-2">
          <button
            onClick={() => navigate('/itsm/sla/policies')}
            className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
          >
            Policies
          </button>
          <button
            onClick={() => navigate('/itsm/sla/instances')}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Instances
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Active SLAs</h2>
          <div className="text-3xl font-bold text-blue-600">{loading ? '—' : policyCount}</div>
        </div>
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">Breached</h2>
          <div className="text-3xl font-bold text-red-600">{loading ? '—' : breachedCount}</div>
        </div>
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-2">On Track</h2>
          <div className="text-3xl font-bold text-green-600">{loading ? '—' : Math.max(policyCount - breachedCount, 0)}</div>
        </div>
      </div>

      {/* SLA Breach Banner */}
      {slaBreachInfos.length > 0 && (
        <div className="mt-6">
          <SLABreachBanner breaches={slaBreachInfos} maxDisplay={5} />
        </div>
      )}

      {/* SLA Countdown Timers */}
      {slaInstances.length > 0 && (
        <div className="mt-6">
          <SLACountdownWidget slaInstances={slaInstances} showDetails />
        </div>
      )}

      <div className="mt-6 bg-white rounded-lg shadow-md p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Breached SLA Instances</h2>
        {loading ? (
          <div>Loading...</div>
        ) : breaches.length === 0 ? (
          <div className="text-gray-600">No breached SLA instances.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50 border-b">
                <tr>
                  <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">Target</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">Response Due</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">Resolution Due</th>
                  <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">Breaches</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {breaches.map((item) => (
                  <tr key={item.slaInstanceId} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {item.targetType ?? '—'} / {item.targetId ?? '—'}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-700">
                      {item.responseDueAt ? new Date(item.responseDueAt).toLocaleString() : '—'}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-700">
                      {item.resolutionDueAt ? new Date(item.resolutionDueAt).toLocaleString() : '—'}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-700">
                      {item.responseBreached ? 'Response' : ''}
                      {item.responseBreached && item.resolutionBreached ? ' & ' : ''}
                      {item.resolutionBreached ? 'Resolution' : ''}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default SLADashboardPage;
