import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';
import { Settings as SettingsIcon } from '@mui/icons-material';
import { debugLog, debugError } from '../utils/debug';
import '../styles/Footer.css';

interface HealthStatus {
  status: 'up' | 'down';
  timestamp?: string;
}

function Footer() {
  const [apiStatus, setApiStatus] = useState<HealthStatus>({ status: 'down' });
  const [dbStatus, setDbStatus] = useState<HealthStatus>({ status: 'down' });
  const [buildInfo] = useState({
    version: process.env.REACT_APP_VERSION || '1.1.0',
    buildDate: process.env.REACT_APP_BUILD_DATE || new Date().toISOString().split('T')[0],
  });

  useEffect(() => {
    const checkHealth = async () => {
      try {
        // Use axios directly to avoid baseURL manipulation and auth headers
        // This creates a simple request to the health endpoint
        const apiUrl = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';
        const healthUrl = apiUrl.replace('/api', '') + '/health'; // Remove /api and add /health
        
        console.log('[Health Check] URL:', healthUrl);
        debugLog('Health check URL:', healthUrl);
        
        const response = await axios.get(healthUrl, {
          timeout: 5000,
        });
        
        console.log('[Health Check] Success:', response.status, response.data);
        debugLog('Health check success:', response.data);
        
        if (response.status === 200) {
          setApiStatus({ status: 'up', timestamp: response.data.timestamp });
          // If API is up, database is also up (API checks DB health)
          setDbStatus({ status: 'up' });
        }
      } catch (error: any) {
        console.error('[Health Check] Failed:', error.message, error.response?.status);
        debugError('Health check failed:', error.message);
        setApiStatus({ status: 'down' });
        setDbStatus({ status: 'down' });
      }
    };

    // Check health immediately and then every 30 seconds
    checkHealth();
    const interval = setInterval(checkHealth, 30000);

    return () => clearInterval(interval);
  }, []);

  const getStatusClass = (status: 'up' | 'down') => {
    return status === 'up' ? 'status-up' : 'status-down';
  };

  const getStatusText = (status: 'up' | 'down') => {
    return status === 'up' ? '● Online' : '● Offline';
  };

  return (
    <footer className="footer">
      <div className="footer-content">
        <div className="footer-section">
          <div className="build-info">
            <span className="build-label">Version:</span>
            <span className="build-value">{buildInfo.version}</span>
            <span className="build-label">Build Date:</span>
            <span className="build-value">{buildInfo.buildDate}</span>
          </div>
        </div>

        <div className="footer-section">
          <div className="status-info">
            <div className={`status-item ${getStatusClass(apiStatus.status)}`}>
              <span className="status-label">API:</span>
              <span className="status-value">{getStatusText(apiStatus.status)}</span>
            </div>
            <div className={`status-item ${getStatusClass(dbStatus.status)}`}>
              <span className="status-label">Database:</span>
              <span className="status-value">{getStatusText(dbStatus.status)}</span>
            </div>
          </div>
        </div>

        <div className="footer-section">
          <Link to="/settings" className="settings-link" title="Go to Settings">
            <SettingsIcon sx={{ fontSize: '1.2rem', mr: 0.5 }} />
            Settings
          </Link>
        </div>

        <div className="footer-section">
          <div className="copyright">
            <span>&copy; 2026 CRM Solution. All rights reserved.</span>
          </div>
        </div>
      </div>
    </footer>
  );
}

export default Footer;
