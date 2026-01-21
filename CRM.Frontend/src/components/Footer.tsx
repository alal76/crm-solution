import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';
import { Settings as SettingsIcon } from '@mui/icons-material';
import { debugLog, debugError } from '../utils/debug';
import { getHealthCheckUrl, getServicePorts } from '../config/ports';
import '../styles/Footer.css';

interface HealthStatus {
  status: 'up' | 'down';
  timestamp?: string;
}

function Footer() {
  const [apiStatus, setApiStatus] = useState<HealthStatus>({ status: 'down' });
  const [dbStatus, setDbStatus] = useState<HealthStatus>({ status: 'down' });
  const [ports, setPorts] = useState(getServicePorts());
  const [buildInfo] = useState({
    version: process.env.REACT_APP_VERSION || '1.3.0',
    buildDate: process.env.REACT_APP_BUILD_DATE || new Date().toISOString().split('T')[0],
  });

  useEffect(() => {
    // Cache the port configuration
    setPorts(getServicePorts());
  }, []);

  useEffect(() => {
    const checkHealth = async () => {
      try {
        const healthUrl = getHealthCheckUrl();
        
        debugLog('Health check URL:', { healthUrl, ports });
        
        const response = await axios.get(healthUrl, {
          timeout: 5000,
        });
        
        debugLog('Health check success:', response.data);
        
        if (response.status === 200) {
          setApiStatus({ status: 'up', timestamp: response.data.timestamp });
          // If API is up, database is also up (API checks DB health)
          setDbStatus({ status: 'up' });
        }
      } catch (error: any) {
        debugError('Health check failed:', {
          message: error.message,
          status: error.response?.status,
        });
        setApiStatus({ status: 'down' });
        setDbStatus({ status: 'down' });
      }
    };

    // Check health immediately and then every 30 seconds
    checkHealth();
    const interval = setInterval(checkHealth, 30000);

    return () => clearInterval(interval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
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
