import React from 'react';
import { useNavigate } from 'react-router-dom';

const ITSMOverviewPage: React.FC = () => {
  const navigate = useNavigate();

  const cards = [
    { title: 'Incidents', description: 'Track and resolve incidents', action: () => navigate('/itsm/incidents') },
    { title: 'Problems', description: 'Root cause analysis and known errors', action: () => navigate('/itsm/problems') },
    { title: 'Changes', description: 'Plan, approve, and schedule changes', action: () => navigate('/itsm/changes') },
    { title: 'CMDB', description: 'Configuration items and relationships', action: () => navigate('/itsm/cmdb') },
    { title: 'Knowledge Base', description: 'Search and manage articles', action: () => navigate('/itsm/knowledge') },
    { title: 'Service Catalog', description: 'Request and fulfill services', action: () => navigate('/itsm/catalog') }
  ];

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">ITSM Overview</h1>
        <button
          onClick={() => navigate('/itsm/metrics')}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          View Metrics
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
        {cards.map((card) => (
          <button
            key={card.title}
            onClick={card.action}
            className="text-left bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow border border-gray-100"
          >
            <h2 className="text-xl font-semibold text-gray-900 mb-2">{card.title}</h2>
            <p className="text-sm text-gray-600">{card.description}</p>
          </button>
        ))}
      </div>

      <div className="mt-8 grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-2">SLA Health</h3>
          <p className="text-sm text-gray-600">Track response and resolution compliance.</p>
          <button
            onClick={() => navigate('/itsm/sla')}
            className="mt-4 text-sm text-blue-600 hover:text-blue-700"
          >
            Open SLA Dashboard →
          </button>
        </div>
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Change Calendar</h3>
          <p className="text-sm text-gray-600">Review upcoming changes and blackout windows.</p>
          <button
            onClick={() => navigate('/itsm/changes/calendar')}
            className="mt-4 text-sm text-blue-600 hover:text-blue-700"
          >
            Open Calendar →
          </button>
        </div>
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Knowledge Authoring</h3>
          <p className="text-sm text-gray-600">Create and curate knowledge articles.</p>
          <button
            onClick={() => navigate('/itsm/knowledge/editor')}
            className="mt-4 text-sm text-blue-600 hover:text-blue-700"
          >
            Open Editor →
          </button>
        </div>
      </div>
    </div>
  );
};

export default ITSMOverviewPage;
