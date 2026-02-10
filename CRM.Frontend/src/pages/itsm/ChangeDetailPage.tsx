import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import axios from 'axios';
import {
  ApprovalWorkflowPanel,
  RiskAssessmentForm,
  ChangeConflictDetector,
} from '../../components/itsm';
import type { ApprovalStep, ChangeConflict } from '../../components/itsm';

interface ChangeDetail {
  changeId: number;
  number: string;
  shortDescription: string;
  state: number;
  approvalStatus: number;
  plannedStartDate?: string;
  plannedEndDate?: string;
  requestorName?: string;
}

const ChangeDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [change, setChange] = useState<ChangeDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [approvalSteps, setApprovalSteps] = useState<ApprovalStep[]>([]);
  const [conflicts, setConflicts] = useState<ChangeConflict[]>([]);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await axios.get(`/api/changes/${id}`);
        setChange(response.data);
        // Load approval steps and conflicts (best-effort)
        const [approvalResp, conflictResp] = await Promise.allSettled([
          axios.get(`/api/changes/${id}/approvals`),
          axios.get(`/api/changes/${id}/conflicts`),
        ]);
        if (approvalResp.status === 'fulfilled') setApprovalSteps(approvalResp.value.data ?? []);
        if (conflictResp.status === 'fulfilled') setConflicts(conflictResp.value.data ?? []);
      } catch (error) {
        console.error('Failed to load change', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  if (loading) return <div className="p-6">Loading...</div>;
  if (!change) return <div className="p-6">Change not found</div>;

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{change.number}</h1>
          <p className="text-gray-600">{change.shortDescription}</p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => navigate(`/itsm/changes/${change.changeId}/approval`)}
            className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
          >
            Approvals
          </button>
          <button
            onClick={() => navigate(`/itsm/changes/${change.changeId}/edit`)}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Edit
          </button>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow-md p-6 space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <h3 className="text-sm font-semibold text-gray-700">State</h3>
            <p className="text-gray-900">State {change.state}</p>
          </div>
          <div>
            <h3 className="text-sm font-semibold text-gray-700">Approval</h3>
            <p className="text-gray-900">Status {change.approvalStatus}</p>
          </div>
          <div>
            <h3 className="text-sm font-semibold text-gray-700">Requestor</h3>
            <p className="text-gray-900">{change.requestorName || '—'}</p>
          </div>
        </div>
        <div>
          <h3 className="text-sm font-semibold text-gray-700">Planned Window</h3>
          <p className="text-gray-900">
            {change.plannedStartDate ? new Date(change.plannedStartDate).toLocaleString() : '—'}
            {' '}→{' '}
            {change.plannedEndDate ? new Date(change.plannedEndDate).toLocaleString() : '—'}
          </p>
        </div>
      </div>

      {/* Approval Workflow */}
      <div className="mt-6">
        <ApprovalWorkflowPanel
          steps={approvalSteps}
          currentUserId={0}
          title={`Approvals for ${change.number}`}
        />
      </div>

      {/* Risk Assessment */}
      <div className="mt-6">
        <RiskAssessmentForm
          changeRequestId={change.changeId}
        />
      </div>

      {/* Change Conflict Detection */}
      {conflicts.length > 0 && (
        <div className="mt-6">
          <ChangeConflictDetector
            currentChange={{
              id: change.changeId,
              changeNumber: change.number,
              title: change.shortDescription,
              scheduledStart: change.plannedStartDate ?? new Date().toISOString(),
              scheduledEnd: change.plannedEndDate ?? new Date().toISOString(),
              affectedCIs: [],
              assignedTo: change.requestorName ?? '',
            }}
            conflicts={conflicts}
          />
        </div>
      )}
    </div>
  );
};

export default ChangeDetailPage;
