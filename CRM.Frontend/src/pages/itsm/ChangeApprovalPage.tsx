import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import apiClient from '../../services/apiClient';

interface ChangeApprovalDetail {
  changeId: number;
  number: string;
  shortDescription: string;
  state: number;
  approvalStatus: number;
}

const ChangeApprovalPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [change, setChange] = useState<ChangeApprovalDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [comments, setComments] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [rejecting, setRejecting] = useState(false);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get(`/changes/${id}`);
        setChange(response.data);
      } catch (error) {
        console.error('Failed to load change', error);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      load();
    } else {
      setLoading(false);
    }
  }, [id]);

  const handleApprove = async () => {
    if (!id) return;
    setSubmitting(true);
    setSubmitError(null);

    try {
      await apiClient.post(`/changes/${id}/approvals`, { comments });
      setSubmitted(true);
    } catch (error) {
      console.error('Failed to approve change', error);
      setSubmitError('Unable to submit approval. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleReject = async () => {
    if (!id) return;
    setRejecting(true);
    setSubmitError(null);

    try {
      await apiClient.post(`/changes/${id}/rejections`, { comments });
      setSubmitted(true);
    } catch (error) {
      console.error('Failed to reject change', error);
      setSubmitError('Unable to submit rejection. Please try again.');
    } finally {
      setRejecting(false);
    }
  };

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-gray-900 mb-6">Change Approvals</h1>
      <div className="bg-white rounded-lg shadow-md p-6 space-y-4">
        {loading ? (
          <div>Loading...</div>
        ) : !change ? (
          <div className="text-gray-600">Change not found.</div>
        ) : (
          <>
            <div>
              <p className="text-sm text-gray-600">{change.number}</p>
              <p className="text-lg font-semibold text-gray-900">{change.shortDescription}</p>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <h3 className="text-sm font-semibold text-gray-700">State</h3>
                <p className="text-gray-900">State {change.state}</p>
              </div>
              <div>
                <h3 className="text-sm font-semibold text-gray-700">Approval</h3>
                <p className="text-gray-900">Status {change.approvalStatus}</p>
              </div>
            </div>
            <div className="border border-gray-200 rounded p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2" htmlFor="approval-comments">
                  Approval comments
                </label>
                <textarea
                  id="approval-comments"
                  value={comments}
                  onChange={(event) => setComments(event.target.value)}
                  className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  rows={4}
                  placeholder="Add comments for the change owner"
                  disabled={submitted || submitting}
                />
              </div>
              {submitError && <div className="text-sm text-red-600">{submitError}</div>}
              {submitted && <div className="text-sm text-green-600">Approval submitted.</div>}
              <div className="flex justify-end gap-3">
                <button
                  type="button"
                  onClick={handleReject}
                  disabled={rejecting || submitted || submitting}
                  className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
                >
                  {rejecting ? 'Rejecting...' : 'Reject Change'}
                </button>
                <button
                  type="button"
                  onClick={handleApprove}
                  disabled={submitting || submitted || rejecting}
                  className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
                >
                  {submitting ? 'Submitting...' : 'Approve Change'}
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default ChangeApprovalPage;
